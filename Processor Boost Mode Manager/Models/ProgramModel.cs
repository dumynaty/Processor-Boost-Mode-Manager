using ProcessorBoostModeManager.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ProcessorBoostModeManager.Models
{
    public class ProgramModel : INotifyPropertyChanged
    {
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required BoostMode BoostMode { get; set; } = BoostMode.Disabled;

        [JsonIgnore] // This will prevent the JSON from serializing IsRunning - - No need to write this to file
        private bool _isRunning;
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
        private bool _highestValue;
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
