using System.Windows.Threading;

namespace ProcessorBoostModeManager.Common
{
    public static class TextBoxHandling
    {
        public static MainWindow? MainWindowInstance { get; set; }
        private static string permanentUpperMessage = string.Empty;
        private static string permanentLowerMessage = string.Empty;
        private static readonly DispatcherTimer timerLower = new();
        private static readonly DispatcherTimer timerUpper = new();

        public static void Upper(string message, bool priority = false, bool ignoreIfTimerOn = false)
        {
            if (MainWindowInstance != null)
            {
                if (ignoreIfTimerOn && timerUpper.IsEnabled == true)
                    return;

                if (!priority)
                {
                    MainWindowInstance.UpperMainInfo.Text = message;

                    timerUpper.Interval = TimeSpan.FromSeconds(5);
                    timerUpper.Tick += (s, e) =>
                    {
                        if (MainWindowInstance.UpperMainInfo.Text == message)
                            MainWindowInstance.UpperMainInfo.Text = permanentUpperMessage;
                        timerUpper.Stop();
                    };
                    timerUpper.Start();
                }
                else
                {
                    timerUpper.Stop();
                    permanentUpperMessage = message;
                    MainWindowInstance.UpperMainInfo.Text = permanentUpperMessage;
                }
            }
        }

        public static void Lower(string message, bool permanent = false, bool ignoreIfTimerOn = false)
        {
            if (ignoreIfTimerOn && timerLower.IsEnabled == true)
                return;

            if (MainWindowInstance != null)
            {
                if (!permanent)
                {
                    MainWindowInstance.LowerMainInfo.Text = message;

                    timerLower.Interval = TimeSpan.FromSeconds(5);
                    timerLower.Tick += (s, e) =>
                    {
                        MainWindowInstance.LowerMainInfo.Text = permanentLowerMessage;
                        timerLower.Stop();
                    };
                    timerLower.Start();
                }
                else
                {
                    permanentLowerMessage = message;
                    MainWindowInstance.LowerMainInfo.Text = permanentLowerMessage;
                }
            }
        }
    }
}