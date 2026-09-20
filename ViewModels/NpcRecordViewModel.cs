using Boutique.Models;
using ReactiveUI.SourceGenerators;

namespace Boutique.ViewModels;

public partial class NpcRecordViewModel(NpcRecord npcRecord) : SelectableRecordViewModel<NpcRecord>(npcRecord)
{
  [Reactive] private string? _conflictingFileName;

  [Reactive] private bool _hasConflict;

  [Reactive] private string? _overlappingFileName;

  [Reactive] private bool _hasOverlap;

  /// <summary>
  ///   Set when the NPC was pulled in because it shares a leveled actor template pool with an NPC
  ///   the user picked, not because the user picked it.
  /// </summary>
  [Reactive] private bool _isFromTemplatePool;

  [Reactive] private string? _templatePoolEditorId;

  public NpcRecord NpcRecord => Record;
}
