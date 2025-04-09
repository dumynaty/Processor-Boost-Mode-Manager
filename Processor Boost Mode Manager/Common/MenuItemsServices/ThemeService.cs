using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;

namespace ProcessorBoostModeManager.Common.MenuItemsServices
{
    public class ThemeService
    {
        public ObservableCollection<ThemeModel> ThemeMenuItems { get; set; } = new ObservableCollection<ThemeModel>();

        public ThemeService()
        {
            InitializeThemes();
        }

        private void InitializeThemes()
        {
            string[] AvailableThemes = new string[]
            {
                "Classic", "Light", "Dark"
            };

            foreach (var themeName in AvailableThemes)
            {
                ThemeModel model = new ThemeModel();
                model.Name = themeName;
                model.IsChecked = false;
                ThemeMenuItems.Add(model);
            }
        }

        public void SetMenuItemsTheme(string selectedTheme)
        {
            foreach (var menuItem in ThemeMenuItems)
            {
                if (menuItem.IsChecked != (menuItem.Name == selectedTheme))
                    menuItem.IsChecked = menuItem.Name == selectedTheme;
            }
        }
    }
}
