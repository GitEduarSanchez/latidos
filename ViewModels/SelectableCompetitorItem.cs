using Latidos.Models;

namespace Latidos.ViewModels;

public class SelectableCompetitorItem : BindableObject
{
    private readonly Action? _onSelectionChanged;

    public SelectableCompetitorItem(CompetitorProfile competitor, Action? onSelectionChanged = null)
    {
        Competitor = competitor;
        _onSelectionChanged = onSelectionChanged;
    }

    public CompetitorProfile Competitor { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
            _onSelectionChanged?.Invoke();
        }
    }
}
