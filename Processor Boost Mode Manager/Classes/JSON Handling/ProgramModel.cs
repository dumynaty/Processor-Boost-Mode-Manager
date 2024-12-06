using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace ProcessBoostModeManager
{
    public class ProgramModel : INotifyPropertyChanged
    {
        private bool _isRunning;
        private bool _highestValue;

        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string BoostMode { get; set; } = "Disabled";

        [JsonIgnore] // This will prevent the JSON from serializing IsRunning - - No need to write this to file
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore] // Ignore HighestValue - No need to write this to file
        public bool HighestValue
        {
            get => _highestValue;
            set
            {
                if (_highestValue != value)
                {
                    _highestValue = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore] // Ignore Icon - Mandatory, cannot store icons(.ico .png) in JSON file
        public BitmapSource? Icon { get; set; } = null;


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}