using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcessorBoostModeManager.ViewModels
{
    public class ComboBoxSelectionViewModel
    {
        public ObservableCollection<ComboBoxModel> ComboBoxItems { get; set; } = new ObservableCollection<ComboBoxModel>();

        public ComboBoxSelectionViewModel()
        {
            InitializeSelections();
        }

        private void InitializeSelections()
        {
            string[] Selections = { 
                "Disabled",
                "Enabled",
                "Aggressive",
                "EfficientEnabled",
                "EfficientAggressive",
                "AggressiveOnGuaranteed",
                "EfficientAggressiveOnGuaranteed"};

            foreach (var selection in Selections)
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
