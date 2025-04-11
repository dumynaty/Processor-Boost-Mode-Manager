using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;

namespace ProcessorBoostModeManager.Common.MenuItemsServices
{
    public class BoostModeService
    {
        public ObservableCollection<BoostModeModel> BoostModeMenuItems { get; set; } = new ObservableCollection<BoostModeModel>();

        public BoostModeService()
        {
            InitializeMenuItemBoostModes();
        }

        private void InitializeMenuItemBoostModes()
        {
            foreach (var boostMode in Enum.GetValues(typeof(CPUBoostMode)))
            {
                BoostModeModel model = new BoostModeModel();
                model.Name = boostMode.ToString() ?? "Unknown";
                model.IsChecked = false;
                BoostModeMenuItems.Add(model);
            }
        }

        public void SetMenuItemsSavedState(string BoostModes)
        {
            string[] BoostModesArray = BoostModes.Split(',');
            foreach (var menuItem in BoostModeMenuItems)
            {
                menuItem.IsChecked = BoostModesArray.Contains(menuItem.Name);
            }
        }

        public void ToggleMenuItemState(string selectedBoostMode, bool IsChecked)
        {
            foreach (var menuItem in BoostModeMenuItems)
            {
                if (menuItem.Name == selectedBoostMode)
                {
                    menuItem.IsChecked = IsChecked;
                    break;
                }
            }
        }
    }
}
