using Boutique.Utilities;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Serilog;

namespace Boutique.Services.GameData;

/// <summary>
///   A leveled actor list used as an NPC template. The game rolls one member per actor when the
///   actor is first created, so a patch on a single member only reaches the share of actors that
///   roll it.
/// </summary>
public sealed record NpcTemplatePool(FormKey PoolFormKey, string? PoolEditorId, IReadOnlyList<FormKey> Members);

/// <summary>
///   Maps NPCs to the leveled actor template pools that decide their appearance. Both the pools an
///   NPC is a member of and the pools it reaches through its template chain are collected: an NPC
///   sits in several pools at once in vanilla data, so picking one of them would expand to a
///   partial set.
/// </summary>
public static class NpcTemplatePoolIndex
{
  private const int MaxTemplateChainDepth = 8;

  public static IReadOnlyDictionary<FormKey, IReadOnlyList<NpcTemplatePool>> Build(
    ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache,
    ILogger logger)
  {
    var templateTargets = new Dictionary<FormKey, FormKey?>();
    foreach (var npc in linkCache.WinningOverrides<INpcGetter>())
    {
      RecordProcessingHelper.TryProcessRecord(
        logger,
        npc,
        () => templateTargets[npc.FormKey] = npc.Template.IsNull ? null : npc.Template.FormKey,
        "NPC template");
    }

    var lists = new Dictionary<FormKey, NpcTemplatePool>();
    foreach (var list in linkCache.WinningOverrides<ILeveledNpcGetter>())
    {
      RecordProcessingHelper.TryProcessRecord(
        logger,
        list,
        () => lists[list.FormKey] = new NpcTemplatePool(list.FormKey, list.EditorID, ResolveMembers(list, linkCache)),
        "leveled NPC");
    }

    return Build(templateTargets, lists);
  }

  /// <summary>
  ///   Maps every NPC whose appearance is decided by a leveled actor template pool to those pools.
  /// </summary>
  /// <param name="templateTargets">Every NPC and the record its Template field points at, if any.</param>
  /// <param name="lists">Leveled actor lists by form key, with whatever members they resolve to.</param>
  public static IReadOnlyDictionary<FormKey, IReadOnlyList<NpcTemplatePool>> Build(
    IReadOnlyDictionary<FormKey, FormKey?> templateTargets,
    IReadOnlyDictionary<FormKey, NpcTemplatePool> lists)
  {
    var pools = new Dictionary<FormKey, NpcTemplatePool>();
    foreach (var target in templateTargets.Values.OfType<FormKey>())
    {
      // Only lists an NPC actually templates off roll appearances; the rest are spawns or loot.
      if (lists.TryGetValue(target, out var list) && list.Members.Count > 0 && !pools.ContainsKey(target))
      {
        pools[target] = list;
      }
    }

    var index = new Dictionary<FormKey, List<NpcTemplatePool>>();

    foreach (var pool in pools.Values)
    {
      foreach (var member in pool.Members)
      {
        Map(index, member, pool);
      }
    }

    foreach (var (npcFormKey, firstTemplate) in templateTargets)
    {
      foreach (var template in ChainOfTemplates(npcFormKey, firstTemplate, templateTargets))
      {
        if (pools.TryGetValue(template, out var pool))
        {
          Map(index, npcFormKey, pool);
        }
      }
    }

    return index.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<NpcTemplatePool>)pair.Value);
  }

  private static void Map(
    Dictionary<FormKey, List<NpcTemplatePool>> index,
    FormKey npcFormKey,
    NpcTemplatePool pool)
  {
    if (!index.TryGetValue(npcFormKey, out var mapped))
    {
      index[npcFormKey] = mapped = [];
    }

    if (!mapped.Any(existing => existing.PoolFormKey == pool.PoolFormKey))
    {
      mapped.Add(pool);
    }
  }

  private static IEnumerable<FormKey> ChainOfTemplates(
    FormKey start,
    FormKey? firstTemplate,
    IReadOnlyDictionary<FormKey, FormKey?> templateTargets)
  {
    var visited = new HashSet<FormKey> { start };
    var current = firstTemplate;

    for (var depth = 0;
         current is { } template && depth < MaxTemplateChainDepth && visited.Add(template);
         depth++)
    {
      yield return template;
      current = templateTargets.TryGetValue(template, out var next) ? next : null;
    }
  }

  private static List<FormKey> ResolveMembers(
    ILeveledNpcGetter list,
    ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
  {
    if (list.Entries is not { } entries)
    {
      return [];
    }

    var members = new List<FormKey>();
    foreach (var entry in entries)
    {
      if (entry.Data?.Reference.FormKeyNullable is not { } formKey || formKey == FormKey.Null)
      {
        continue;
      }

      // Nested lists and forms dropped by an override have no appearance to inherit.
      if (!linkCache.TryResolve<INpcGetter>(formKey, out _))
      {
        continue;
      }

      if (!members.Contains(formKey))
      {
        members.Add(formKey);
      }
    }

    return members;
  }
}
