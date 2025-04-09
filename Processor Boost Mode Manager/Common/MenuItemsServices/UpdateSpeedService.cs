using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;

namespace ProcessorBoostModeManager.Common.MenuItemsServices
{
    public class UpdateSpeedService
    {
        public ObservableCollection<UpdateSpeedModel> UpdateSpeedMenuItems { get; set; } = new ObservableCollection<UpdateSpeedModel>();

        public UpdateSpeedService()
        {
            InitializeMenuItemUpdateSpeed();
        }

        private void InitializeMenuItemUpdateSpeed()
        {
            Dictionary<string, int> AvailableUpdateSpeeds = new Dictionary<string, int>()
            {
                { "High (1 second)", 1 },
                { "Medium (5 seconds)", 5 },
                { "Low (60 seconds)", 60 }
            };

            foreach (var speedMode in AvailableUpdateSpeeds)
            {
                UpdateSpeedModel model = new UpdateSpeedModel();
                model.Name = speedMode.Key;
                model.IsChecked = false;
                model.Speed = speedMode.Value;
                UpdateSpeedMenuItems.Add(model);
            }
        }

        public void SetMenuItemsUpdateSpeed(int newSpeedMode)
        {
            foreach (var menuItem in UpdateSpeedMenuItems)
            {
                if (menuItem.IsChecked != (menuItem.Speed == newSpeedMode))
                    menuItem.IsChecked = (menuItem.Speed == newSpeedMode);
            }
        }
    }
}
