using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProcessorBoostModeManager.Models
{
    public class UpdateSpeedModel : INotifyPropertyChanged
    {
        private string name = string.Empty;
        private bool isChecked;
        private int speed;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }
        public int Speed
        {
            get => speed;
            set
            {
                speed = value;
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
