using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Formulas;

namespace NorthIsland.Server.Data;

/// <summary>The formula table loaded from Formula.dat, and the rules that apply it to a character.</summary>
public sealed class FormulaData
{
    /// <summary>The table, read from the file on first use; null when the file is missing.</summary>
    private readonly Lazy<FormulaTable?> _table;

    /// <summary>The logger.</summary>
    private readonly ILogger<FormulaData> _logger;

    /// <summary>Locates Formula.dat in the game data directory; the file is read on first use.</summary>
    /// <param name="options">The server options, which name the game data directory.</param>
    /// <param name="logger">The logger.</param>
    public FormulaData(
        IOptions<ServerOptions> options,
        ILogger<FormulaData> logger)
    {
        _logger = logger;
        var path = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "Formula.dat");
        _table = new Lazy<FormulaTable?>(() =>
        {
            if (!File.Exists(path))
            {
                logger.LogWarning("Formula data not found at {Path}; HP / SP / exp keep their stored values", path);
                return null;
            }
            var table = FormulaTable.Open(path);
            logger.LogInformation("Loaded formulas from {Path}", path);
            return table;
        });
    }

    /// <summary>The formula table, or null when Formula.dat was not found.</summary>
    public FormulaTable? Table => _table.Value;

    /// <summary>Sets a new character to level 1 with the experience of that level and full HP and SP; does nothing without the table.</summary>
    /// <param name="character">The new character.</param>
    public void InitialiseNew(Character character)
    {
        if (Table is not { } f)
            return;
        character.Level = 1;
        character.Exp = f.TotalLevelExp(1, character.RebornCount);
        Recalculate(character);
        character.Hp = character.MaxHp;
        character.Sp = character.MaxSp;
    }

    /// <summary>
    /// Brings level, max HP and max SP of a character in line with its experience and attributes; the HP and SP keep their full or partial state.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <param name="hpBonus">Extra max HP to add.</param>
    /// <param name="spBonus">Extra max SP to add.</param>
    public void Recalculate(Character character, int hpBonus = 0, int spBonus = 0)
    {
        if (Table is not { } f)
            return;
        var reborn = character.RebornCount;
        var minExp = f.TotalLevelExp(Math.Max((int)character.Level, 1), reborn);
        if (character.Exp < minExp)
            character.Exp = minExp;
        character.Level = (byte)f.LevelFromExp(character.Exp, reborn);

        var totalLevel = FormulaTable.TotalLevel(character.Level, reborn);
        var maxHp = f.MaxHp(totalLevel, character.Constitution, hpBonus, 0);
        var maxSp = f.MaxSp(totalLevel, character.Wisdom, spBonus, 0);
        var wasFullHp = character.Hp >= character.MaxHp;
        var wasFullSp = character.Sp >= character.MaxSp;
        if (maxHp != character.MaxHp || maxSp != character.MaxSp)
            _logger.LogInformation("{Name}: max HP {OldHp} -> {Hp}, max SP {OldSp} -> {Sp}", character.Name, character.MaxHp, maxHp, character.MaxSp, maxSp);
        character.MaxHp = maxHp;
        character.MaxSp = maxSp;
        character.Hp = wasFullHp ? maxHp : Math.Min(character.Hp, maxHp);
        character.Sp = wasFullSp ? maxSp : Math.Min(character.Sp, maxSp);
    }
}
