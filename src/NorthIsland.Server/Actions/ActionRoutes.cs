using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NorthIsland.Server.Actions;

/// <summary>
/// The routes of one action: registers handlers on the packet router under that action's number and checks that the sub codes named belong to it.
/// </summary>
public sealed partial class ActionRoutes
{
    /// <summary>The router the routes are added to.</summary>
    private readonly PacketRouter _router;

    /// <summary>Creates the routes of one action.</summary>
    /// <param name="router">The router the routes are added to.</param>
    /// <param name="code">The number of the action the routes belong to.</param>
    public ActionRoutes(
        PacketRouter router,
        byte code)
    {
        _router = router;
        Code = code;
    }

    /// <summary>The number of the action the routes belong to.</summary>
    public byte Code { get; }

    /// <summary>Routes one sub code of the action to a handler.</summary>
    /// <param name="sub">The sub code.</param>
    /// <param name="name">The route's name, used in the log.</param>
    /// <param name="handler">The handler that receives the packet body.</param>
    /// <param name="subExpression">
    /// The source text of <paramref name="sub"/>, filled in by the compiler to check that it names a sub code of this action.
    /// </param>
    /// <returns>These routes, so calls can be chained.</returns>
    public ActionRoutes On(byte sub, string name, PacketHandler handler, [CallerArgumentExpression(nameof(sub))] string subExpression = "")
    {
        CheckSub(subExpression);
        _router.On(Code, sub, name, handler);
        return this;
    }

    /// <summary>Routes every packet of the action, whatever its sub code, to a handler.</summary>
    /// <param name="name">The route's name, used in the log.</param>
    /// <param name="handler">The handler that receives the packet.</param>
    /// <returns>These routes, so calls can be chained.</returns>
    public ActionRoutes On(string name, PacketHandler handler)
    {
        _router.On(Code, name, handler);
        return this;
    }

    /// <summary>
    /// Registers the packets that free the client when the handler of a sub code fails, gets a malformed packet or, if requested, sends no reply.
    /// </summary>
    /// <param name="sub">The sub code.</param>
    /// <param name="clear">Builds the packets to send.</param>
    /// <param name="whenSilent">True to send them also when the handler sent no reply.</param>
    /// <param name="subExpression">
    /// The source text of <paramref name="sub"/>, filled in by the compiler to check that it names a sub code of this action.
    /// </param>
    /// <returns>These routes, so calls can be chained.</returns>
    public ActionRoutes Clears(byte sub, Func<IEnumerable<byte[]>> clear, bool whenSilent, [CallerArgumentExpression(nameof(sub))] string subExpression = "")
    {
        CheckSub(subExpression);
        _router.Clears(Code, sub, clear, whenSilent);
        return this;
    }

    /// <summary>Registers the same freeing packets for several sub codes.</summary>
    /// <param name="subs">The sub codes.</param>
    /// <param name="clear">Builds the packets to send.</param>
    /// <param name="whenSilent">True to send them also when the handler sent no reply.</param>
    /// <param name="subsExpression">
    /// The source text of <paramref name="subs"/>, filled in by the compiler to check that it names sub codes of this action.
    /// </param>
    /// <returns>These routes, so calls can be chained.</returns>
    public ActionRoutes Clears(
        byte[] subs,
        Func<IEnumerable<byte[]>> clear,
        bool whenSilent,
        [CallerArgumentExpression(nameof(subs))] string subsExpression = "")
    {
        ArgumentNullException.ThrowIfNull(subs);
        CheckSub(subsExpression);
        foreach (var sub in subs)
            _router.Clears(Code, sub, clear, whenSilent);
        return this;
    }

    /// <summary>Registers freeing packets for every sub code of the action that has no clear entry of its own.</summary>
    /// <param name="clear">Builds the packets to send.</param>
    /// <param name="whenSilent">True to send them also when the handler sent no reply.</param>
    /// <returns>These routes, so calls can be chained.</returns>
    public ActionRoutes Clears(Func<IEnumerable<byte[]>> clear, bool whenSilent)
    {
        _router.Clears(Code, null, clear, whenSilent);
        return this;
    }

    /// <summary>Throws when the source text of a sub code argument names an opcode constant of another action.</summary>
    /// <param name="expression">The source text of the argument.</param>
    private void CheckSub(string expression)
    {
        foreach (Match m in OpcodeConstant().Matches(expression))
            if (int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) != Code)
                throw new InvalidOperationException($"The routes of action {Code} name a sub of another action: {expression}");
    }

    /// <summary>Matches an opcode constant such as <c>Action23.Items</c> and captures its action number.</summary>
    /// <returns>The compiled regular expression.</returns>
    [GeneratedRegex(@"\bAction(\d+)\.\w+")]
    private static partial Regex OpcodeConstant();
}
