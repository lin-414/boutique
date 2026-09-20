using Boutique.Services.GameData;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Serilog;
using Xunit;

namespace Boutique.Tests.Integration;

/// <summary>
///   Imperial city guards are the case that motivated the index: a placed guard templates off
///   LvlGuardImperial, which templates off the leveled actor list LCharGuardImperial, and the outfit
///   lives on whichever of the nine members the game rolled for that actor.
/// </summary>
public class NpcTemplatePoolSkyrimEsmTests
{
  private static readonly ModKey Skyrim = ModKey.FromNameAndExtension("Skyrim.esm");

  private static FormKey Key(uint id) => new(Skyrim, id);

  private static readonly FormKey GuardVariant        = Key(0xAA8D5);
  private static readonly FormKey AnotherGuardVariant = Key(0xAA913);
  private static readonly FormKey PlacedSolitudeGuard = Key(0x10C068);
  private static readonly FormKey ImperialGuardList   = Key(0xE7B2C);

  [SkippableFact]
  public void ImperialGuardVariants_ShareOneLeveledActorTemplatePool()
  {
    var path = SkyrimTestData.ResolveEsmPath();
    Skip.If(path is null, SkyrimTestData.MissingMessage);

    using var mod   = SkyrimMod.CreateFromBinaryOverlay(path!, SkyrimRelease.SkyrimSE);
    var       index = NpcTemplatePoolIndex.Build(mod.ToImmutableLinkCache(), Log.Logger);

    var rolledBy = index[GuardVariant].First(pool => pool.PoolFormKey == ImperialGuardList);
    rolledBy.PoolEditorId.Should().Be("LCharGuardImperial");
    rolledBy.Members.Should().HaveCount(9);
    rolledBy.Members.Should().Contain(AnotherGuardVariant);

    index[PlacedSolitudeGuard].Should().Contain(pool => pool.PoolFormKey == ImperialGuardList);
  }
}
