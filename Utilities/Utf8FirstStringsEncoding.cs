using System.Text;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Strings.DI;

namespace Boutique.Utilities;

/// <summary>
/// Reads string bytes as UTF-8 when they are valid UTF-8, falling back to CP1252 otherwise.
/// The game itself reads 8-bit .strings files as UTF-8, so localisation packs ship UTF-8 text
/// under English file names; Mutagen's default English encoding is plain CP1252, which garbles
/// that text.
/// </summary>
public sealed class Utf8FirstStringsEncoding : IMutagenEncoding
{
  private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

  public static readonly Utf8FirstStringsEncoding Instance = new();

  public string GetString(ReadOnlySpan<byte> bytes)
  {
    try
    {
      return StrictUtf8.GetString(bytes);
    }
    catch (DecoderFallbackException)
    {
      return MutagenEncoding._1252.GetString(bytes);
    }
  }

  public int GetByteCount(ReadOnlySpan<char> str) => Encoding.UTF8.GetByteCount(str);

  public int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes) => Encoding.UTF8.GetBytes(chars, bytes);
}
