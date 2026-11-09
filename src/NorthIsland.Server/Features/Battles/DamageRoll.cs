using NorthIsland.Protocol.Messages.Battle;

namespace NorthIsland.Server.Features.Battles;

/// <summary>The result of rolling one attack's damage.</summary>
/// <param name="Damage">The damage dealt.</param>
/// <param name="Critical">True when the hit was a critical hit.</param>
public readonly record struct DamageRoll(
    int Damage,
    bool Critical);
