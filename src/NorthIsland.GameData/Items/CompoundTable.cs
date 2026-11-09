using System.Buffers.Binary;

namespace NorthIsland.GameData.Items;

/// <summary>The manufacture recipes of Compound2.dat.</summary>
public sealed class CompoundTable
{
    /// <summary>Size of one Compound2.dat record in bytes.</summary>
    public const int RecordSize = 65;

    /// <summary>Value of the first word of Compound2.dat that this reader accepts.</summary>
    public const ushort Version = 6;

    /// <summary>The recipes in file order, records without a product left out.</summary>
    public IReadOnlyList<CompoundRecipe> Recipes { get; }

    /// <summary>Creates the table.</summary>
    /// <param name="recipes">The recipes.</param>
    private CompoundTable(
        List<CompoundRecipe> recipes) => Recipes = recipes;

    /// <summary>Reads and parses a Compound2.dat file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static CompoundTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>
    /// Parses the contents of a Compound2.dat file; throws <see cref="InvalidDataException"/> when it is too short or has the
    /// wrong version.
    /// </summary>
    /// <param name="data">The raw file contents.</param>
    /// <returns>The parsed table.</returns>
    public static CompoundTable Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < RecordSize || BinaryPrimitives.ReadUInt16LittleEndian(data) != Version)
            throw new InvalidDataException("Compound2.dat: version inconsistent");
        List<CompoundRecipe> recipes = [];
        for (var i = 1; (i + 1) * RecordSize <= data.Length; i++)
        {
            var r = data.Slice(i * RecordSize, RecordSize);
            List<(ushort, byte)> materials = [];
            for (var k = 0; k < 5; k++)
            {
                var item = W(r, 11 + 3 * k);
                if (item != 0)
                    materials.Add((item, B(r, 13 + 3 * k))); // Count
            }
            var product = W(r, 0);
            if (product == 0)
                continue;
            recipes.Add(new CompoundRecipe(i, product,
                W(r, 2), // 2 Blueprint
                B(r, 4), // 4 Category
                product == 39123 ? (ushort)0 : W(r, 5), // 39123 Gas Storage, 5 Facility
                B(r, 7), // 7 PerBatch
                materials,
                W(r, 27))); // 27 Minutes
        }
        return new CompoundTable(recipes);
    }

    /// <summary>Decodes an obfuscated byte of a record.</summary>
    /// <param name="r">The record.</param>
    /// <param name="o">Byte offset in the record.</param>
    /// <returns>The decoded byte.</returns>
    private static byte B(ReadOnlySpan<byte> r, int o) => unchecked((byte)((r[o] ^ 211) - 3));

    /// <summary>Decodes an obfuscated little-endian word of a record.</summary>
    /// <param name="r">The record.</param>
    /// <param name="o">Byte offset in the record.</param>
    /// <returns>The decoded word.</returns>
    private static ushort W(ReadOnlySpan<byte> r, int o) =>
        unchecked((ushort)((BinaryPrimitives.ReadUInt16LittleEndian(r.Slice(o, 2)) ^ 64444) - 3));
}
