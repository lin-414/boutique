using Boutique.Utilities;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Strings.DI;
using Xunit;

namespace Boutique.Tests;

public class Utf8FirstStringsEncodingTests
{
  [Fact]
  public void GetString_ValidUtf8Chinese_DecodesCorrectly()
  {
    var bytes = "帝国士兵"u8;

    Utf8FirstStringsEncoding.Instance.GetString(bytes).Should().Be("帝国士兵");
  }

  [Fact]
  public void GetString_AsciiContent_DecodesIdentically()
  {
    var bytes = "Imperial Soldier"u8;

    Utf8FirstStringsEncoding.Instance.GetString(bytes).Should().Be("Imperial Soldier");
  }

  [Fact]
  public void GetString_InvalidUtf8Content_FallsBackToCp1252()
  {
    // "Café" encoded in CP1252 ends with 0xE9, which cannot be decoded as UTF-8.
    var cp1252Bytes = new byte[] { 0x43, 0x61, 0x66, 0xE9 };

    Utf8FirstStringsEncoding.Instance.GetString(cp1252Bytes).Should().Be("Café");
  }

  [Fact]
  public void GetByteCount_MatchesUtf8Semantics()
  {
    Utf8FirstStringsEncoding.Instance.GetByteCount("帝国士兵").Should().Be(12);
  }

  [Fact]
  public void Provider_English_ReturnsUtf8FirstEncoding()
  {
    Utf8FirstStringsEncodingProvider.Instance
      .GetEncoding(GameRelease.SkyrimSE, Language.English)
      .Should().BeSameAs(Utf8FirstStringsEncoding.Instance);
  }

  [Fact]
  public void Provider_OtherLanguages_KeepMutagenDefaults()
  {
    Utf8FirstStringsEncodingProvider.Instance
      .GetEncoding(GameRelease.SkyrimSE, Language.Chinese)
      .Should().BeSameAs(MutagenEncoding.GetEncoding(GameRelease.SkyrimSE, Language.Chinese));

    Utf8FirstStringsEncodingProvider.Instance
      .GetEncoding(GameRelease.SkyrimSE, Language.German)
      .Should().BeSameAs(MutagenEncoding.GetEncoding(GameRelease.SkyrimSE, Language.German));
  }
}
