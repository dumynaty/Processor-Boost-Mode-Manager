using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.Models.Poco;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ProcessorBoostModeManager.ViewModels
{
    public class ProgramViewModel : INotifyPropertyChanged
    {
        private readonly ProgramModel _model;
        public ProgramModel Model => _model;

        public ProgramViewModel(ProgramModel model)
        {
            _model = model;
        }

        public string Name => _model.Name;
        public string Location => _model.Location;
        public CPUBoostMode BoostMode
        {
            get => _model.BoostMode;
            set { _model.BoostMode = value; OnPropertyChanged(); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }
        
        private bool _highestValue;
        public bool HighestValue
        {
            get => _highestValue;
            set { _highestValue = value; OnPropertyChanged(); }
        }

        public ComboBoxSelectionViewModel ComboBoxSelection { get; set; } = new();
        public BitmapSource? Icon { get; set; } = null;


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
