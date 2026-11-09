namespace NorthIsland.Server.Features.Instances;

/// <summary>The state of an instance.</summary>
public enum InstanceState : byte
{
    /// <summary>The instance is gathering members and shows in the list.</summary>
    Waiting = 1,

    /// <summary>The instance has started; nobody can join any more.</summary>
    Started = 2,
}
