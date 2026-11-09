using System.Text;

namespace NorthIsland.GameData.Events;

/// <summary>The text encoding of the event file.</summary>
public static class EveText
{
    /// <summary>Registers the code page encodings and selects code page 950.</summary>
    static EveText()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Current = Encoding.GetEncoding(950);
    }

    /// <summary>The code page 950 encoding used for names in the event file.</summary>
    public static Encoding Current { get; }
}
