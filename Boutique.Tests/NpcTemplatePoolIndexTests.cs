using Boutique.Services.GameData;
using FluentAssertions;
using Mutagen.Bethesda.Plugins;
using Xunit;

namespace Boutique.Tests;

/// <summary>
///   The game rolls one member of a leveled actor list as an NPC's template, so a distribution that
///   only covers the member the user picked reaches a fraction of the actors. The index is what the
///   editor expands a selection against.
/// </summary>
public class NpcTemplatePoolIndexTests
{
  private static readonly ModKey Master = ModKey.FromNameAndExtension("Skyrim.esm");

  private static FormKey Key(uint id) => new(Master, id);

  private static NpcTemplatePool List(uint id, params uint[] members) =>
    new(Key(id), $"Pool{id:X}", members.Select(Key).ToList());

  private static IReadOnlyDictionary<FormKey, FormKey?> Templates(params (uint Npc, uint? Template)[] pairs) =>
    pairs.ToDictionary(
      p => Key(p.Npc),
      p => p.Template is { } template ? (FormKey?)Key(template) : null);

  private static IReadOnlyDictionary<FormKey, NpcTemplatePool> Lists(params NpcTemplatePool[] lists) =>
    lists.ToDictionary(l => l.PoolFormKey, l => l);

  [Fact]
  public void PoolMembers_AreMappedToTheListThatHoldsThem()
  {
    var pool  = List(0x50, 0x01, 0x02);
    var index = NpcTemplatePoolIndex.Build(Templates((0x20, 0x50)), Lists(pool));

    index[Key(0x01)].Should().ContainSingle().Which.Should().BeSameAs(pool);
    index[Key(0x02)].Should().ContainSingle().Which.Should().BeSameAs(pool);
  }

  [Fact]
  public void NpcReachingTheListThroughItsTemplateChain_IsMappedToThatList()
  {
    var pool  = List(0x50, 0x30);
    var index = NpcTemplatePoolIndex.Build(
      Templates((0x10, 0x20), (0x20, 0x50)),
      Lists(pool));

    index[Key(0x10)].Should().ContainSingle().Which
      .Should().BeSameAs(pool, "the placed guard has no outfit of its own");
    index[Key(0x30)].Should().ContainSingle().Which
      .Should().BeSameAs(pool, "the rolled member is where the outfit actually lives");
  }

  [Fact]
  public void MemberOfSeveralPools_IsMappedToAllOfThem()
  {
    var first  = List(0x50, 0x01);
    var second = List(0x51, 0x01);
    var index  = NpcTemplatePoolIndex.Build(
      Templates((0x20, 0x50), (0x21, 0x51)),
      Lists(first, second));

    index[Key(0x01)].Should().BeEquivalentTo(new[] { first, second });
  }

  [Fact]
  public void ListNobodyTemplatesOff_IsNotATemplatePool()
  {
    var index = NpcTemplatePoolIndex.Build(
      Templates((0x01, null)),
      Lists(List(0x50, 0x01)));

    index.Should().BeEmpty("a leveled actor list used for spawns does not decide any appearance");
  }

  [Fact]
  public void PlainNpc_IsNotMapped()
  {
    var index = NpcTemplatePoolIndex.Build(Templates((0x01, null)), Lists());

    index.Should().NotContainKey(Key(0x01));
  }

  [Fact]
  public void TemplateCycle_TerminatesWithoutMapping()
  {
    var index = NpcTemplatePoolIndex.Build(Templates((0x01, 0x02), (0x02, 0x01)), Lists());

    index.Should().BeEmpty();
  }

  [Fact]
  public void ChainLongerThanTheWalkLimit_IsNotMapped()
  {
    var templates = new List<(uint, uint?)> { (0x100, 0x101) };
    for (uint id = 0x101; id < 0x10D; id++)
    {
      templates.Add((id, id + 1));
    }

    templates.Add((0x10D, 0x50));
    var index = NpcTemplatePoolIndex.Build(Templates(templates.ToArray()), Lists(List(0x50, 0x999)));

    index.Should().NotContainKey(Key(0x100));
  }
}
