using Mutagen.Bethesda;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Strings.DI;

namespace Boutique.Utilities;

/// <summary>
/// Encoding provider for .strings files that reads English-language files as UTF-8 with a CP1252
/// fallback instead of Mutagen's plain CP1252, fixing garbled names from Chinese localisation
/// packs. Every other language keeps Mutagen's default encoding table.
/// </summary>
public sealed class Utf8FirstStringsEncodingProvider : IMutagenEncodingProvider
{
  public static readonly Utf8FirstStringsEncodingProvider Instance = new();

  public IMutagenEncoding GetEncoding(GameRelease release, Language language) =>
    language == Language.English
      ? Utf8FirstStringsEncoding.Instance
      : MutagenEncoding.GetEncoding(release, language);
}
