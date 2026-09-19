using Boutique.Models;
using Boutique.Utilities;
using Mutagen.Bethesda.Plugins;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Boutique.ViewModels;

public partial class SelectableRecordViewModel<TRecord>(TRecord record) : ReactiveObject, ISelectableRecordViewModel
  where TRecord : IGameRecord
{
  [Reactive] private bool _isExcluded;

  [Reactive] private bool    _isSelected;
  private            string? _searchCache;

  protected TRecord Record { get; } = record;

  public string EditorID => Record.EditorID ?? "(No EditorID)";
  public string DisplayName => Record.DisplayName;
  public string ModDisplayName => Record.ModDisplayName;
  public string FormKeyString => Record.FormKeyString;
  public FormKey FormKey => Record.FormKey;

  public bool MatchesSearch(string searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      return true;
    }

    _searchCache ??= $"{DisplayName} {EditorID} {ModDisplayName} {FormKeyString}".ToLowerInvariant();
    if (_searchCache.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    // Accept form ids in any common format ("0x00020546", "00020546", "20546") that the
    // Mutagen display string ("020546:Skyrim.esm") cannot match as a plain substring.
    return FormKeyHelper.TryParseSearchFormId(searchTerm, out var formId) && FormKey.ID == formId;
  }
}
