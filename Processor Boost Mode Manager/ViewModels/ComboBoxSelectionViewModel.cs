using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcessorBoostModeManager.ViewModels
{
    public class ComboBoxSelectionViewModel
    {
        private readonly string[] BoostModeValues = Enum.GetNames(typeof(CPUBoostMode));
        public ObservableCollection<ComboBoxModel> ComboBoxItems { get; set; } = new ObservableCollection<ComboBoxModel>();
        public Visibility ComboBoxVisibility { get; set; } = Visibility.Visible;
        public ComboBoxSelectionViewModel()
        {
            InitializeSelections();
        }

        private void InitializeSelections()
        {
            foreach (var selection in BoostModeValues)
            {
                ComboBoxModel model = new ComboBoxModel();
                model.Name = selection;
                model.IsVisible = Visibility.Visible;
                ComboBoxItems.Add(model);
            }
        }

        public void SetSavedComboBoxItems(string BoostModes)
        {
            string[] savedBoostModes = BoostModes.Split(',');
            foreach (var item in ComboBoxItems)
            {
                if (savedBoostModes.Contains(item.Name))
                {
                    item.IsVisible = Visibility.Visible;
                }
                else
                {
                    item.IsVisible = Visibility.Collapsed;
                }
            }
        }
    }
}
