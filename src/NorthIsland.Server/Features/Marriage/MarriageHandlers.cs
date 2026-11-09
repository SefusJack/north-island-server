using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Marriage;

/// <summary>Handles marriage applications, the wedding ceremony, divorce and the spouse card.</summary>
/// <param name="marriages">The marriage records.</param>
/// <param name="world">The world state: sessions, teams and map occupants.</param>
/// <param name="guilds">The guild registry, for the spouse's guild name on the card.</param>
/// <param name="marks">Sets and clears the marriage marks.</param>
/// <param name="friends">Delivers the spouse mail.</param>
/// <param name="maps">Map data, for the scene name in the wedding announcement.</param>
/// <param name="options">Marriage options.</param>
/// <param name="logger">Logger.</param>
public sealed class MarriageHandlers(
    MarriageRegistry marriages,
    WorldState world,
    GuildRegistry guilds,
    MarkData marks,
    FriendHandlers friends,
    MapData maps,
    IOptions<MarriageOptions> options,
    ILogger<MarriageHandlers> logger)
{
    /// <summary>A marriage application or divorce that waits for the answers of both players.</summary>
    /// <param name="applicantId">Login id of the player who started it.</param>
    /// <param name="groomId">Login id of the groom; for a divorce, the applicant.</param>
    /// <param name="brideId">Login id of the bride; for a divorce, the spouse.</param>
    private sealed class Pending(
        int applicantId,
        int groomId,
        int brideId)
    {
        /// <summary>Login id of the player who started the request.</summary>
        public int ApplicantId { get; } = applicantId;

        /// <summary>Login id of the groom; for a divorce, the applicant.</summary>
        public int GroomId { get; } = groomId;

        /// <summary>Login id of the bride; for a divorce, the spouse.</summary>
        public int BrideId { get; } = brideId;

        /// <summary>Login ids of the players who have said yes so far.</summary>
        public HashSet<int> Accepted { get; } = [];
    }

    /// <summary>Pending marriage applications by player id; both players point to the same entry.</summary>
    private readonly ConcurrentDictionary<int, Pending> _applications = new();

    /// <summary>Pending divorces by player id; both spouses point to the same entry.</summary>
    private readonly ConcurrentDictionary<int, Pending> _divorces = new();

    /// <summary>The maps on which a wedding ceremony is running.</summary>
    private readonly ConcurrentDictionary<ushort, bool> _ceremonies = new();

    /// <summary>Subscribes to the world's leaving event so that a leaving player's pending requests are cancelled.</summary>
    internal void Start()
    {
        world.Leaving += OnLeaving;
    }

    /// <summary>Runs a marriage step for a player.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="step">Whether to apply, hold the wedding or divorce.</param>
    public void Run(GameSession session, MarriageStep step)
    {
        switch (step)
        {
            case MarriageStep.Apply: // apply for marriage
                Apply(session);
                break;
            case MarriageStep.Wedding: // hold the wedding
                Wedding(session);
                break;
            case MarriageStep.Divorce: // apply for a divorce
                DivorceApply(session);
                break;
        }
    }

    /// <summary>
    /// Tells a player who was divorced while offline, and sends the spouse card and online state of a married player; the spouse is told they are
    /// online.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character that entered the world.</param>
    public void SendOnLogin(GameSession session, Character character)
    {
        var id = session.Account!.LoginId;
        if (marriages.TakeDivorcedWhileAway(id))
        {
            session.Send(MarriageMessages.DivorceResult(MarriageMessages.SpouseDivorced));
            marks.TrySetFlag(session, character, MarriageRegistry.MarriedFlag, on: false);
        }
        if (marriages.CoupleOf(id) is not { } couple)
            return;
        marriages.Refresh(id, character);
        SendCard(session, couple);
        if (world.SessionOf(couple.SpouseOf(id).Id) is { } spouse)
        {
            spouse.Send(MarriageMessages.SpouseOnline(true));
            SendCard(spouse, couple);
        }
    }

    /// <summary>Cancels the leaving player's pending application or divorce, saves their card and tells their spouse they went offline.</summary>
    /// <param name="session">The session that is leaving the world.</param>
    private void OnLeaving(GameSession session)
    {
        if (session.Account is not { } account || session.ActiveCharacter is not { } character)
            return;
        var id = account.LoginId;
        if (_applications.TryRemove(id, out var application))
            Cancel(application, id, divorce: false);
        if (_divorces.TryRemove(id, out var divorce))
            Cancel(divorce, id, divorce: true);
        if (marriages.CoupleOf(id) is not { } couple)
            return;
        marriages.Refresh(id, character);
        if (world.SessionOf(couple.SpouseOf(id).Id) is { } spouse)
            spouse.Send(MarriageMessages.SpouseOnline(false));
    }

    /// <summary>Delivers a letter from the player to their spouse as mail; it is dropped when the addressed name is not the spouse's.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnSpouseMail(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || body.Length < 5)
            return;
        var nameLength = body.Span[4];
        if (body.Length < 5 + nameLength)
            return;
        var name = NorthIsland.Protocol.Text.GameEncoding.GetString(body.Span.Slice(5, nameLength)).Trim();
        var text = NorthIsland.Protocol.Text.GameEncoding.GetString(body.Span[(5 + nameLength)..]);
        var id = session.Account!.LoginId;
        if (text.Length == 0 || marriages.CoupleOf(id) is not { } couple)
            return;
        var spouse = couple.SpouseOf(id);
        if (!string.Equals(spouse.Name, GmTag.Strip(name), StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("[{Session}] {Name}: spouse mail to {Target}, who is not the spouse", session.Id, character.Name, name);
            return;
        }
        await friends.DeliverAsync(id, spouse.Id, text, session).ConfigureAwait(false);
        logger.LogInformation("[{Session}] {Name} mails the spouse {Spouse}: {Text}", session.Id, character.Name, spouse.Name, text);
    }

    /// <summary>Resends the spouse card to a married player.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnRefresh(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not null && marriages.CoupleOf(session.Account!.LoginId) is { } couple)
            SendCard(session, couple);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Sends a player the spouse's id, card and online state; the card is refreshed from the spouse's character first when the spouse is online.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="couple">The player's couple.</param>
    private void SendCard(GameSession session, Couple couple)
    {
        var id = session.Account!.LoginId;
        var card = couple.SpouseOf(id);
        var spouse = world.SessionOf(card.Id);
        var online = spouse is not null && spouse.ActiveCharacter is not null;
        if (online)
        {
            lock (marriages.Lock)
                card.Refresh(spouse!.ActiveCharacter!);
        }
        session.Send(MarriageMessages.SpouseId(card.Id));
        session.Send(MarriageMessages.SpouseInfo(card.Info(guilds.Of(card.Id)?.Name ?? ""), session.Client));
        session.Send(MarriageMessages.SpouseOnline(online));
    }

    /// <summary>Finds the other member of the player's two-player team when they are on the same map.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The partner's session, or null when there is none.</returns>
    private GameSession? Partner(GameSession session)
    {
        var id = session.Account!.LoginId;
        var team = world.Teams.TeamOf(id);
        if (team.Count != 2)
            return null;
        var otherId = team[0] == id ? team[1] : team[0];
        return world.SessionOf(otherId) is { } other && other.ActiveCharacter?.MapId == session.ActiveCharacter!.MapId ? other : null;
    }

    /// <summary>
    /// Starts a marriage application for the player and their team partner: checks both and asks each to confirm. The partner with a male body type
    /// is the groom.
    /// </summary>
    /// <param name="session">The session of the applying player.</param>
    private void Apply(GameSession session)
    {
        if (InWorld(session) is not { } character)
            return;
        if (Partner(session) is not { } partner)
        {
            session.Send(MarriageMessages.Result(MarriageMessages.NotTwo));
            return;
        }
        var (a, b) = (session, partner);
        var (groom, bride) = MarriageRegistry.IsMale(character.BodyType) ? (a, b) : (b, a);
        var refusal = ApplicationRefusal(a) ?? ApplicationRefusal(b)
            ?? (MarriageRegistry.IsMale(groom.ActiveCharacter!.BodyType) == MarriageRegistry.IsMale(bride.ActiveCharacter!.BodyType)
                ? MarriageMessages.Result(MarriageMessages.SameGender) : null)
                ?? RingRefusal(groom, MarriageRegistry.GroomRing) ?? RingRefusal(bride, MarriageRegistry.BrideRing);
        if (refusal is not null)
        {
            a.Send(refusal);
            b.Send(refusal);
            logger.LogInformation("[{Session}] {A} and {B}: marriage application refused (45/20 {Code})", session.Id, character.Name,
                partner.ActiveCharacter!.Name, refusal[2]);
            return;
        }
        var pending = new Pending(Id(session), Id(groom), Id(bride));
        _applications[Id(a)] = pending;
        _applications[Id(b)] = pending;
        a.Send(MarriageMessages.Application(Id(b)));
        b.Send(MarriageMessages.Application(Id(a)));
        logger.LogInformation("[{Session}] {Groom} and {Bride} apply to marry", session.Id, groom.ActiveCharacter!.Name, bride.ActiveCharacter!.Name);
    }

    /// <summary>
    /// Checks whether a player may apply: body shape, level, not married, no other application, no wait after a divorce, and the fee.
    /// </summary>
    /// <param name="s">The player's session.</param>
    /// <returns>The refusal packet, or null when the player may apply.</returns>
    private byte[]? ApplicationRefusal(GameSession s)
    {
        var c = s.ActiveCharacter!;
        var id = Id(s);
        var now = GameClock.UtcNow;
        return c.BodyShiftOn ? MarriageMessages.Result(MarriageMessages.BodyShifted, id)
            : c.Level < MarriageRegistry.MinLevel && c.RebornCount == 0 ? MarriageMessages.Result(MarriageMessages.LevelTooLow, id)
                : marriages.CoupleOf(id) is not null ? MarriageMessages.Result(MarriageMessages.AlreadyMarried)
                    : marriages.ApplicationOf(id, now) is not null ? MarriageMessages.Result(MarriageMessages.AppliedBefore)
                        : marriages.Cooling(id, now) ? MarriageMessages.Result(MarriageMessages.DivorceCooldown, id)
                            : c.Gold < MarriageRegistry.ApplicationFee ? MarriageMessages.Result(MarriageMessages.NoMoney, id)
                                : null;
    }

    /// <summary>Checks that a player carries or wears the ring.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="ring">Item id of the ring.</param>
    /// <returns>The refusal packet, or null when the ring is there.</returns>
    private static byte[]? RingRefusal(GameSession s, ushort ring) =>
        Bag.Count(s.ActiveCharacter!, ring) > 0 || s.ActiveCharacter!.Equipment.Exists(e => e.ItemId == ring)
            ? null
            : MarriageMessages.Result(MarriageMessages.NoRing, Id(s));

    /// <summary>
    /// Handles a yes or no to a marriage application; once both have said yes, charges the fee and records the application with its deadline.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnApplicationAnswer(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || body.Length < 9 || !_applications.TryGetValue(Id(session), out var pending))
            return ValueTask.CompletedTask;
        var id = Id(session);
        if (body.Span[8] != 1)
        {
            _applications.TryRemove(pending.GroomId, out _);
            _applications.TryRemove(pending.BrideId, out _);
            Cancel(pending, id, divorce: false);
            return ValueTask.CompletedTask;
        }
        lock (pending)
        {
            pending.Accepted.Add(id);
            if (pending.Accepted.Count < 2)
                return ValueTask.CompletedTask;
        }
        _applications.TryRemove(pending.GroomId, out _);
        _applications.TryRemove(pending.BrideId, out _);
        if (world.SessionOf(pending.GroomId) is not { } groom || world.SessionOf(pending.BrideId) is not { } bride
            || groom.ActiveCharacter is not { } g || bride.ActiveCharacter is not { } b)
            return ValueTask.CompletedTask;
        if (g.Gold < MarriageRegistry.ApplicationFee || b.Gold < MarriageRegistry.ApplicationFee)
        {
            var poor = g.Gold < MarriageRegistry.ApplicationFee ? pending.GroomId : pending.BrideId;
            groom.Send(MarriageMessages.Result(MarriageMessages.NoMoney, poor));
            bride.Send(MarriageMessages.Result(MarriageMessages.NoMoney, poor));
            return ValueTask.CompletedTask;
        }
        var application = marriages.Apply(pending.GroomId, pending.BrideId, GameClock.UtcNow);
        var deadline = application.UntilUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        foreach (var (s, c) in new[] { (groom, g), (bride, b) })
        {
            c.Gold -= MarriageRegistry.ApplicationFee;
            s.Send(MarriageMessages.FeePaidResult(MarriageMessages.FeePaid, c.Gold));
            s.Send(MarriageMessages.AppliedResult(deadline));
            s.Send(MarriageMessages.Result(MarriageMessages.Deadline));
            marks.TrySetFlag(s, c, MarriageRegistry.AppliedFlag, on: true);
        }
        logger.LogInformation("[{Session}] {Groom} and {Bride} applied to marry (until {Until})", session.Id, g.Name, b.Name, deadline);
        return ValueTask.CompletedTask;
    }

    /// <summary>Cancels a pending application or divorce and tells both players.</summary>
    /// <param name="pending">The request to cancel.</param>
    /// <param name="byId">Login id of the player who cancelled or left.</param>
    /// <param name="divorce">True for a divorce, false for a marriage application.</param>
    private void Cancel(Pending pending, int byId, bool divorce)
    {
        var packet = divorce
            ? MarriageMessages.DivorceResult(MarriageMessages.DivorceCancelled)
            : MarriageMessages.Result(MarriageMessages.Cancelled, byId);
        foreach (var pid in new[] { pending.GroomId, pending.BrideId })
        {
            (divorce ? _divorces : _applications).TryRemove(pid, out _);
            if (world.SessionOf(pid) is { } s)
                s.Send(packet);
        }
        logger.LogInformation("{What} of {A} and {B} cancelled by {By}", divorce ? "Divorce" : "Marriage application", pending.GroomId, pending.BrideId, byId);
    }

    /// <summary>
    /// Starts the wedding for a team that holds an application: checks rings, clothes and that no ceremony runs on the map, then runs the ceremony.
    /// </summary>
    /// <param name="session">The player's session.</param>
    private void Wedding(GameSession session)
    {
        if (InWorld(session) is not { } character)
            return;
        var now = GameClock.UtcNow;
        var application = marriages.ApplicationOf(Id(session), now);
        var partner = Partner(session);
        if (application is null || partner is null || !application.Has(Id(partner)))
        {
            var someoneApplied = application is not null || (partner is not null && marriages.ApplicationOf(Id(partner), now) is not null);
            session.Send(MarriageMessages.Result(someoneApplied ? MarriageMessages.MembersNotApplied : MarriageMessages.NoApplication));
            return;
        }
        var groom = world.SessionOf(application.GroomId)!;
        var bride = world.SessionOf(application.BrideId)!;
        var refusal = RingRefusal(groom, MarriageRegistry.GroomRing) ?? RingRefusal(bride, MarriageRegistry.BrideRing)
            ?? (options.Value.WeddingClothesCheck
                ? ClothesRefusal(groom, MarriageRegistry.GroomRing, MarriageRegistry.GroomClothes, MarriageMessages.GroomClothes)
                    ?? ClothesRefusal(bride, MarriageRegistry.BrideRing, MarriageRegistry.BrideClothes, MarriageMessages.BrideClothes)
                : null);
        if (refusal is null && !_ceremonies.TryAdd(character.MapId, true))
            refusal = MarriageMessages.Result(MarriageMessages.WeddingRunning);
        if (refusal is not null)
        {
            groom.Send(refusal);
            bride.Send(refusal);
            logger.LogInformation("[{Session}] wedding of {Groom} and {Bride} refused (45/20 {Code})", session.Id, groom.ActiveCharacter!.Name,
                bride.ActiveCharacter!.Name, refusal[2]);
            return;
        }
        _ = CeremonyAsync(character.MapId, application, groom, bride);
    }

    /// <summary>Checks that a player wears the ring and all the clothes.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="ring">Item id of the ring.</param>
    /// <param name="clothes">Item ids that must all be worn.</param>
    /// <param name="code">The refusal code sent when something is missing.</param>
    /// <returns>The refusal packet, or null when everything is worn.</returns>
    private static byte[]? ClothesRefusal(GameSession s, ushort ring, ImmutableArray<ushort> clothes, byte code) =>
        clothes.Append(ring).All(item => s.ActiveCharacter!.Equipment.Exists(e => e.ItemId == item)) ? null : MarriageMessages.Result(code, Id(s));

    /// <summary>
    /// Runs the ceremony: locks the couple, announces it, plays the steps for everyone on the map, and records the marriage if both are still there
    /// at the end.
    /// </summary>
    /// <param name="mapId">The map of the church.</param>
    /// <param name="application">The couple's application.</param>
    /// <param name="groom">The groom's session.</param>
    /// <param name="bride">The bride's session.</param>
    private async Task CeremonyAsync(ushort mapId, MarriageApplication application, GameSession groom, GameSession bride)
    {
        var (g, b) = (Id(groom), Id(bride));
        void ToChurch(byte[] packet)
        {
            foreach (var s in world.OnMap(mapId))
                s.Send(packet);
        }
        bool Present() => world.SessionOf(g) is { } gs && gs.ActiveCharacter?.MapId == mapId
            && world.SessionOf(b) is { } bs && bs.ActiveCharacter?.MapId == mapId;
        try
        {
            marriages.BeginCeremony(g, b);
            groom.Send(MarriageMessages.InputLock(true));
            bride.Send(MarriageMessages.InputLock(true));
            var place = maps.SceneName(mapId);
            var line = MarriageMessages.CeremonyStarted(g, b, groom.ActiveCharacter!.Name, bride.ActiveCharacter!.Name,
                "holding the wedding at " + (place.Length > 0 ? place : "the church"));
            foreach (var s in world.OnlineSessions())
                s.Send(line);
            foreach (var id in new[] { g, b })
                if (marriages.HidePets(id))
                    ToChurch(MarriageMessages.PetsHidden(id, true));
            ToChurch(MarriageMessages.WeddingMusic(true));
            logger.LogInformation("Wedding of {Groom} and {Bride} on map {Map} begins", groom.ActiveCharacter?.Name, bride.ActiveCharacter?.Name, mapId);
            foreach (byte step in (byte[])[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 16, 17, 18, 19, 21])
            {
                if (!Present())
                {
                    ToChurch(MarriageMessages.WeddingStep(99, g, b));
                    logger.LogInformation("Wedding of {Groom} and {Bride} interrupted", g, b);
                    return;
                }
                ToChurch(MarriageMessages.WeddingStep(step, g, b));
                await Task.Delay(options.Value.WeddingStepMs).ConfigureAwait(false);
            }
            if (!Present() || groom.ActiveCharacter is not { } gc || bride.ActiveCharacter is not { } bc)
                return;
            var couple = marriages.Marry(application, gc, bc);
            foreach (var (s, c) in new[] { (groom, gc), (bride, bc) })
            {
                marks.TrySetFlag(s, c, MarriageRegistry.AppliedFlag, on: false);
                marks.TrySetFlag(s, c, MarriageRegistry.MarriedFlag, on: true);
                SendCard(s, couple);
            }
            ToChurch(MarriageMessages.WeddingStep(23, g, b));
            logger.LogInformation("{Groom} and {Bride} are married", gc.Name, bc.Name);
        }
        finally
        {
            marriages.EndCeremony(g, b);
            foreach (var id in new[] { g, b })
                if (marriages.ShowPets(id))
                    ToChurch(MarriageMessages.PetsHidden(id, false));
            groom.Send(MarriageMessages.InputLock(false));
            bride.Send(MarriageMessages.InputLock(false));
            ToChurch(MarriageMessages.WeddingMusic(false));
            _ceremonies.TryRemove(mapId, out _);
        }
    }

    /// <summary>
    /// Starts a divorce for the player: checks the marriage and the fee; divorces at once when the spouse has been away long enough, otherwise asks
    /// the spouse when they are on the same map.
    /// </summary>
    /// <param name="session">The player's session.</param>
    private void DivorceApply(GameSession session)
    {
        if (InWorld(session) is not { } character)
            return;
        var id = Id(session);
        if (marriages.CoupleOf(id) is not { } couple)
        {
            session.Send(MarriageMessages.DivorceResult(MarriageMessages.NotMarried));
            return;
        }
        if (character.Gold < MarriageRegistry.DivorceFee)
        {
            session.Send(MarriageMessages.Result(MarriageMessages.NoMoney, id));
            return;
        }
        var spouseCard = couple.SpouseOf(id);
        if (world.SessionOf(spouseCard.Id) is not { } spouse)
        {
            if (GameClock.UtcNow - spouseCard.LastSeenUtc < MarriageRegistry.AwayForDivorce)
            {
                session.Send(MarriageMessages.DivorceResult(MarriageMessages.SpouseAway));
                return;
            }
            Divorce(session, character, couple, absentId: spouseCard.Id);
            return;
        }
        if (spouse.ActiveCharacter?.MapId != character.MapId)
        {
            session.Send(MarriageMessages.Result(MarriageMessages.NotSameStream));
            return;
        }
        var pending = new Pending(id, id, spouseCard.Id);
        _divorces[id] = pending;
        _divorces[spouseCard.Id] = pending;
        session.Send(MarriageMessages.DivorceForm(spouseCard.Id, spouseApplied: false));
        spouse.Send(MarriageMessages.DivorceForm(id, spouseApplied: true));
        logger.LogInformation("[{Session}] {Name} asks {Spouse} for a divorce", session.Id, character.Name, spouseCard.Name);
    }

    /// <summary>Handles a yes or no to a divorce; once both have said yes, the applicant divorces if they can still pay.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnDivorceAnswer(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || body.Length < 9 || !_divorces.TryGetValue(Id(session), out var pending))
            return ValueTask.CompletedTask;
        var id = Id(session);
        if (body.Span[8] != 1)
        {
            Cancel(pending, id, divorce: true);
            return ValueTask.CompletedTask;
        }
        lock (pending)
        {
            pending.Accepted.Add(id);
            if (pending.Accepted.Count < 2)
                return ValueTask.CompletedTask;
        }
        _divorces.TryRemove(pending.GroomId, out _);
        _divorces.TryRemove(pending.BrideId, out _);
        if (world.SessionOf(pending.ApplicantId) is { } applicant && applicant.ActiveCharacter is { } c
            && marriages.CoupleOf(pending.ApplicantId) is { } couple && c.Gold >= MarriageRegistry.DivorceFee)
            Divorce(applicant, c, couple, absentId: 0);
        return ValueTask.CompletedTask;
    }

    /// <summary>Charges the fee and divorces: records it, tells both spouses who are online and clears their married mark.</summary>
    /// <param name="applicant">The session of the paying player.</param>
    /// <param name="c">The paying player's character.</param>
    /// <param name="couple">The couple to divorce.</param>
    /// <param name="absentId">Login id of a spouse who is offline and is told at the next login, or 0.</param>
    private void Divorce(GameSession applicant, Character c, Couple couple, int absentId)
    {
        c.Gold -= MarriageRegistry.DivorceFee;
        applicant.Send(MarriageMessages.FeePaidResult(MarriageMessages.DivorceFeePaid, c.Gold));
        marriages.Divorce(couple, GameClock.UtcNow, absentId);
        foreach (var pid in new[] { couple.Husband.Id, couple.Wife.Id })
        {
            if (world.SessionOf(pid) is not { } s || s.ActiveCharacter is not { } sc)
                continue;
            s.Send(MarriageMessages.DivorceResult(MarriageMessages.Divorced));
            s.Send(MarriageMessages.DivorceResult(MarriageMessages.ClearSpouse));
            marks.TrySetFlag(s, sc, MarriageRegistry.MarriedFlag, on: false);
        }
        logger.LogInformation("[{Session}] {Husband} and {Wife} are divorced{Absent}", applicant.Id, couple.Husband.Name, couple.Wife.Name,
            absentId != 0 ? " (the spouse was away 30 days)" : "");
    }

    /// <summary>Shows the player's pets again to everyone on the map when the client reports that they are no longer hidden.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnPetFlagClear(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is { } character && marriages.ShowPets(Id(session)))
            foreach (var s in world.OnMap(character.MapId))
                s.Send(MarriageMessages.PetsHidden(Id(session), false));
        return ValueTask.CompletedTask;
    }

    /// <summary>Gets the login id of the session's account.</summary>
    /// <param name="s">The player's session.</param>
    /// <returns>The login id.</returns>
    private static int Id(GameSession s) => s.Account!.LoginId;

    /// <summary>Gets the character of a session that is in the world.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The character, or null when the session is not in the world.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
