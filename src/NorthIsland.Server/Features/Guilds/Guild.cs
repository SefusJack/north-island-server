namespace NorthIsland.Server.Features.Guilds;

/// <summary>A guild with its lord, vices, members, rules, mailboxes and badge; the guild registry saves it.</summary>
public sealed class Guild
{
    /// <summary>The most members a guild can have.</summary>
    public const int MaxMembers = 100;

    /// <summary>The guild's id, unique and counted up from 1.</summary>
    public uint Id { get; set; }

    /// <summary>The guild's name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Login id of the lord.</summary>
    public int LeaderId { get; set; }

    /// <summary>Login ids of the vice lords.</summary>
    public List<int> Vices { get; set; } = [];

    /// <summary>The members, including the lord and the vices, with the data the guild window shows.</summary>
    public List<GuildMember> Members { get; set; } = [];

    /// <summary>The guild rules text the lord set.</summary>
    public string Rules { get; set; } = "";

    /// <summary>Letters waiting for members who were offline, by recipient login id.</summary>
    public Dictionary<int, List<GuildMail>> Mail { get; set; } = [];

    /// <summary>When the guild was founded, as an OLE automation date.</summary>
    public double Founded { get; set; }

    /// <summary>When the dismissal checks started counting, as an OLE automation date; set at founding.</summary>
    public double RulesSince { get; set; }

    /// <summary>Whether the guild already passed the recruiting check (enough members of the required level).</summary>
    public bool RecruitChecked { get; set; }

    /// <summary>Whether enough members were online together once, which spares the guild from the online check.</summary>
    public bool EnoughOnline { get; set; }

    /// <summary>The badge image bytes, or null when the guild has none.</summary>
    public byte[]? Badge { get; set; }

    /// <summary>Version of the badge, changed with every upload so clients know to fetch it again; 0 while there is no badge.</summary>
    public byte BadgeVersion { get; set; }

    /// <summary>Whether a player may change the badge.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>True for the lord, the vices and members with the first right.</returns>
    public bool CanChangeBadge(int id) => id == LeaderId || Vices.Contains(id) || Member(id)?.Rights.ElementAtOrDefault(0) == 1;

    /// <summary>Finds a member by login id.</summary>
    /// <param name="id">The login id.</param>
    /// <returns>The member, or null when the player is not in the guild.</returns>
    public GuildMember? Member(int id) => Members.Find(m => m.Id == id);

    /// <summary>Whether a player may invite others into the guild.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>True for the lord, the vices and members with the second right.</returns>
    public bool CanInvite(int id) => id == LeaderId || Vices.Contains(id) || Member(id)?.Rights.ElementAtOrDefault(1) == 1;
}
