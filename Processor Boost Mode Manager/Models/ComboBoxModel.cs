using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProcessorBoostModeManager.Models
{
    public class ComboBoxModel : INotifyPropertyChanged
    {
        private string name = string.Empty;
        private Visibility isVisible;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public Visibility IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
