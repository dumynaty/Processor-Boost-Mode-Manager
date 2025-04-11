using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ProcessorBoostModeManager.Common
{
    public class StatusMessageService : INotifyPropertyChanged
    {
        private string permanentUpperMessage = string.Empty;
        private string permanentLowerMessage = string.Empty;
        private readonly DispatcherTimer timerUpper = new();
        private readonly DispatcherTimer timerLower = new();

        private string statusMessageUpper = "Awaiting status..";
        private string statusMessageLower = "Awaiting database..";
        public string StatusMessageUpper
        {
            get => statusMessageUpper;
            set
            {
                if (statusMessageUpper != value)
                {
                    statusMessageUpper = value;
                    OnPropertyChanged();
                }
            }
        }
        public string StatusMessageLower
        {
            get => statusMessageLower;
            set
            {
                if (statusMessageLower != value)
                {
                    statusMessageLower = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public void Upper(string message, bool priority = false, bool ignoreIfTimerOn = false)
        {
            if (ignoreIfTimerOn == true && timerUpper.IsEnabled == true)
                return;

            if (priority == true)
            {
                timerUpper.Stop();
                permanentUpperMessage = message;
                StatusMessageUpper = permanentUpperMessage;
            }
            else if (priority == false)
            {
                StatusMessageUpper = message;

                timerUpper.Interval = TimeSpan.FromSeconds(5);
                timerUpper.Tick += (s, e) =>
                {
                    if (StatusMessageUpper == message)
                        StatusMessageUpper = permanentUpperMessage;
                    timerUpper.Stop();
                };
                timerUpper.Start();
            }
        }
        public void Lower(string message, bool priority = false, bool ignoreIfTimerOn = false)
        {
            if (ignoreIfTimerOn == true && timerLower.IsEnabled == true)
                return;

            if (priority == true)
            {
                timerLower.Stop();
                permanentLowerMessage = message;
                StatusMessageLower = permanentLowerMessage;
            }
            else if (priority == false)
            {
                StatusMessageLower = message;

                timerLower.Interval = TimeSpan.FromSeconds(5);
                timerLower.Tick += (s, e) =>
                {
                    if (StatusMessageLower == message)
                        StatusMessageLower = permanentLowerMessage;
                    timerLower.Stop();
                };
                timerLower.Start();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
