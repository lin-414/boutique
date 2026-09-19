using Boutique.Models;
using Boutique.ViewModels;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace Boutique.Tests;

/// <summary>
///     Verifies record list search matches form ids written in any common format
///     ("0x00020546", "00020546", "20546"), not just the Mutagen display string ("020546:TestMod.esp").
/// </summary>
public class MatchesSearchTests
{
    private static readonly ModKey TestMod = ModKey.FromNameAndExtension("TestMod.esp");

    private static NpcRecordViewModel CreateNpc(uint formId, string editorId = "EncBandit01", string name = "Bandit")
    {
        return new NpcRecordViewModel(new NpcRecord(new FormKey(TestMod, formId), editorId, name, TestMod));
    }

    [Theory]
    [InlineData("0x00020546")]
    [InlineData("00020546")]
    [InlineData("20546")]
    [InlineData("020546")]
    public void MatchesSearch_PaddedOrPrefixedFormId_MatchesByFormKey(string searchTerm)
    {
        var vm = CreateNpc(0x20546);

        vm.MatchesSearch(searchTerm).Should().BeTrue();
    }

    [Fact]
    public void MatchesSearch_NonMatchingFormId_ReturnsFalse()
    {
        var vm = CreateNpc(0x20546);

        vm.MatchesSearch("99999999").Should().BeFalse();
    }

    [Theory]
    [InlineData("bandit")]
    [InlineData("EncBandit01")]
    [InlineData("TestMod.esp")]
    [InlineData("020546:TestMod.esp")]
    public void MatchesSearch_TextTerms_StillMatchDisplayNameEditorIdModAndFormKey(string searchTerm)
    {
        var vm = CreateNpc(0x20546);

        vm.MatchesSearch(searchTerm).Should().BeTrue();
    }

    [Fact]
    public void MatchesSearch_EmptyTerm_MatchesEverything()
    {
        var vm = CreateNpc(0x20546);

        vm.MatchesSearch("").Should().BeTrue();
        vm.MatchesSearch("   ").Should().BeTrue();
    }

    [Fact]
    public void MatchesSearch_OutfitRecord_MatchesPaddedFormId()
    {
        var mod = new SkyrimMod(TestMod, SkyrimRelease.SkyrimSE);
        var outfit = mod.Outfits.AddNew("MyOutfit");

        var vm = new OutfitRecordViewModel(outfit);

        vm.MatchesSearch("00000800").Should().BeTrue();
        vm.MatchesSearch("0x800").Should().BeTrue();
        vm.MatchesSearch("NonexistentEditor").Should().BeFalse();
    }
}
