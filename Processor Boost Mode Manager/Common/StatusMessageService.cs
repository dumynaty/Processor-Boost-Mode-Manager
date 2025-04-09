﻿using ProcessorBoostModeManager.Views;
using System.Windows.Threading;

namespace ProcessorBoostModeManager.Common
{
    public class StatusMessageService
    {
        MainViewModel MainViewModel;
        public StatusMessageService(MainViewModel _mainViewModel)
        {
            MainViewModel = _mainViewModel;
        }

        private string permanentUpperMessage = string.Empty;
        private string permanentLowerMessage = string.Empty;
        private readonly DispatcherTimer timerUpper = new();
        private readonly DispatcherTimer timerLower = new();

        public void Upper(string message, bool priority = false, bool ignoreIfTimerOn = false)
        {
            if (ignoreIfTimerOn == true && timerUpper.IsEnabled == true)
                return;

            if (priority == true)
            {
                timerUpper.Stop();
                permanentUpperMessage = message;
                MainViewModel.StatusMessageUpper = permanentUpperMessage;
            }
            else if (priority == false)
            {
                MainViewModel.StatusMessageUpper = message;

                timerUpper.Interval = TimeSpan.FromSeconds(5);
                timerUpper.Tick += (s, e) =>
                {
                    if (MainViewModel.StatusMessageUpper == message)
                        MainViewModel.StatusMessageUpper = permanentUpperMessage;
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
                MainViewModel.StatusMessageLower = permanentLowerMessage;
            }
            else if (priority == false)
            {
                MainViewModel.StatusMessageLower = message;

                timerLower.Interval = TimeSpan.FromSeconds(5);
                timerLower.Tick += (s, e) =>
                {
                    if (MainViewModel.StatusMessageLower == message)
                        MainViewModel.StatusMessageLower = permanentLowerMessage;
                    timerLower.Stop();
                };
                timerLower.Start();
            }
        }
    }
}
