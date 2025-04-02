// NEED TO ADD
//
// Refresh BoostMode after Removing program
// Logging file
// Database backup and recovery
// Startup Arguments
// File Extra Exit
// ComboBox options selection, Dark theme


using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using ComboBox = System.Windows.Controls.ComboBox;
using System.Collections.ObjectModel;
using RegistryManagerLibrary;
using System.Reflection;
using System.IO;
using ProcessorBoostModeManager.Common.shell32;
using ProcessorBoostModeManager.Enums;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Runtime.Versioning;

namespace ProcessorBoostModeManager
{
	[SupportedOSPlatform("windows")]
    public partial class MainWindow : Window
	{
		private string AppName;
		private string AppPath;
		private int UpdateSpeed;
		public ObservableCollection<ProgramModel> ProgramsInUI { get; } = new ObservableCollection<ProgramModel>();
		public ICollectionView ProgramsView { get; private set; }
		private readonly DispatcherTimer timer = new();

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			AppName = Assembly.GetExecutingAssembly().GetName().Name ?? "Processor Boost Mode Manager";
			AppPath = Environment.ProcessPath ?? string.Empty;

			UpdateSpeed = Properties.Settings.Default.UpdateSpeed;

			ProgramsView = CollectionViewSource.GetDefaultView(ProgramsInUI);
			ProgramsView.SortDescriptions.Add(new SortDescription("HighestValue", ListSortDirection.Descending));
			ProgramsView.SortDescriptions.Add(new SortDescription("IsRunning", ListSortDirection.Descending));
			ProgramsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
			ProgramsView.Refresh();

			App.MainWindowInstance = this;
			TextBoxHandling.MainWindowInstance = this;

			InitializeCheckBoxes();
			RegistryManager.GetActivePowerScheme();

			RunTimer();
		}

		public void RunTimer(bool restart = false)
		{
			if (restart == true)
				timer.Stop();

            timer.Interval = TimeSpan.FromSeconds(UpdateSpeed);
            timer.Tick += (s, e) =>
            {
                UpdateProgram();
            };
            timer.Start();
        }

		public void UpdateProgram()
		{
			var (Database, HighestBoostMode) = Processes.GetDatabaseProcessesOC();
			bool refreshNeeded = false;

			if (ProgramsInUI.Count == 0 && Database.Count != 0)
			{
				foreach (var program in Database)
				{
					ProgramsInUI.Add(program);
				}
			}

			if (ProgramsInUI.Count == Database.Count)
			{
				for (int i = 0; i < Database.Count; i++)
				{
					if (ProgramsInUI[i].IsRunning != Database[i].IsRunning)
					{
						ProgramsInUI[i].IsRunning = Database[i].IsRunning;
						refreshNeeded = true;
					}
					if (ProgramsInUI[i].HighestValue != Database[i].HighestValue)
					{
						ProgramsInUI[i].HighestValue = Database[i].HighestValue;
						refreshNeeded = true;
					}
				}
			}
			else
			{
				refreshNeeded = true;

				string[] programsInUIArray = ProgramsInUI.Select(Program => Program.Name).ToArray();
				string[] programsInDBArray = Database.Select(Program => Program.Name).ToArray();
					
				if (ProgramsInUI.Count < Database.Count)
				{
					foreach (var program in Database)
					{
						if (!programsInUIArray.Contains(program.Name))
						{
							ProgramsInUI.Add(program);
						}
					}
				}
				else if (ProgramsInUI.Count > Database.Count)
				{
					foreach (var program in ProgramsInUI)
					{
						if (!programsInDBArray.Contains(program.Name))
						{
							ProgramsInUI.Remove(program);
							break;
						}
					}
				}
			}

			if (refreshNeeded)
				ProgramsView.Refresh();

			int currentBoostMode = RegistryManager.GetProcessorBoostMode();
			if (currentBoostMode != HighestBoostMode)
			{
				try
				{
					RegistryManager.SetProcessorBoostMode(HighestBoostMode);
					currentBoostMode = HighestBoostMode;
					if (WindowsNotificationCheckBox.IsChecked == true)
						App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: " +
							$"{(CPUBoostMode)HighestBoostMode}", ToolTipIcon.None);
				}
				catch (Exception e)
				{
					MessageBox.Show($"Check application permission. {e.Message}", "Error setting Processor Boost Mode", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			int programsRunningCount = ProgramsInUI.Count(p => p.IsRunning);
			TextBoxHandling.Upper($"Current processes running: {programsRunningCount}");

			if (ProcessListBox.SelectedItem == null)
				TextBoxHandling.Lower($"Current mode: {(CPUBoostMode)currentBoostMode}", true, true);
		}

		// Menu Items
		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem MinToTray)
			{
				Properties.Settings.Default.MinimizeToTray = MinToTray.IsChecked;
			}
		}
        private void ChangedTheme_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem selectedTheme)
			{
				foreach (MenuItem item in ThemeMenuItems.Items)
				{
					if ((string)item.Header != (string)selectedTheme.Header)
					{
						item.IsChecked = false;
					}
				}
				selectedTheme.IsChecked = true;
				ChangeTheme(new Uri ($"Resources/Themes/{(string)selectedTheme.Header}.xaml", UriKind.Relative));
			}
		}
		private void BoostModeItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem selectedCheckBox)
			{
				string BoostModes = Properties.Settings.Default.BoostModes;

                if (BoostModes.Split(',').Contains((string)selectedCheckBox.Header))
				{
					selectedCheckBox.IsChecked = false;
					BoostModes = BoostModes.Replace((string)selectedCheckBox.Header, "");
					if (BoostModes[0] == ',')
					{
						BoostModes = BoostModes.Remove(0);
					}
					if (BoostModes[BoostModes.Length - 1] == ',')
					{
                        BoostModes = BoostModes.Remove(BoostModes.Length - 1);
                    }
					if (BoostModes.Contains(",,"))
					{
                        BoostModes = BoostModes.Replace(",,", ",");
					}
					Properties.Settings.Default.BoostModes = BoostModes;
                }
				else
				{
					selectedCheckBox.IsChecked = true;
                    if (BoostModes[BoostModes.Length - 1] != ',')
                    {
                        BoostModes += $",{(string)selectedCheckBox.Header}";
                    }
					else
					{
                        BoostModes += $"{(string)selectedCheckBox.Header}";
                    }
                    Properties.Settings.Default.BoostModes = BoostModes;
                }
            }
		}
		private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
		{
			UpdateProgram();
		}
        private void UpdateSpeedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem selectedUpdateSpeed)
			{
				foreach (MenuItem item in UpdateSpeedMenuItems.Items)
				{
					if ((string)item.Header != (string)selectedUpdateSpeed.Header)
					{
						item.IsChecked = false;
					}
					else
					{
						item.IsChecked = true;
						UpdateSpeed = int.Parse((string)item.Tag);
						Properties.Settings.Default.UpdateSpeed = int.Parse((string)item.Tag);
						RunTimer(true);
					}
				}
			}
        }
		private void AppInfo_Click(object sender, RoutedEventArgs e)
		{

		}
		
		// CheckBoxes
		private void InitializeCheckBoxes()
		{
			AutostartCheckBox.Unchecked -= AutostartCheckBox_Unchecked;
			AutostartCheckBox.Checked -= AutostartCheckBox_Checked;
			WindowsNotificationCheckBox.Unchecked -= WindowsNotificationCheckBox_Unchecked;
			WindowsNotificationCheckBox.Checked -= WindowsNotificationCheckBox_Checked;

			AutostartCheckBox.IsChecked = RegistryManager.IsAppStartupEnabled(AppPath, AppName);
			WindowsNotificationCheckBox.IsChecked = Properties.Settings.Default.WindowsNotificationPopup;
			MinimizeToTrayMenuItem.IsChecked = Properties.Settings.Default.MinimizeToTray;

			AutostartCheckBox.Unchecked += AutostartCheckBox_Unchecked;
			AutostartCheckBox.Checked += AutostartCheckBox_Checked;
			WindowsNotificationCheckBox.Unchecked += WindowsNotificationCheckBox_Unchecked;
			WindowsNotificationCheckBox.Checked += WindowsNotificationCheckBox_Checked;

			foreach (MenuItem item in ThemeMenuItems.Items)
			{
				item.IsChecked = false;
				if ((string)item.Header == $"{Properties.Settings.Default.Theme}")
				{
					item.IsChecked = true;
				}
			}

			string[] savedBoostModes = Properties.Settings.Default.BoostModes.Split(',');
			foreach (MenuItem item in BoostModesMenuItems.Items)
			{
				if (savedBoostModes.Contains((string)item.Header))
					item.IsChecked = true;
				else
					item.IsChecked = false;
			}

            foreach (MenuItem item in UpdateSpeedMenuItems.Items)
            {
                if ((string)item.Tag == Properties.Settings.Default.UpdateSpeed.ToString())
				{
					item.IsChecked = true;
				}
				else
				{
					item.IsChecked = false;
				}
            }
        }
		private void AutostartCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			RegistryManager.RegisterAppToStartup(AppName, AppPath);
			TextBoxHandling.Upper("Application is registered to start with Windows!");
		}
		private void AutostartCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			RegistryManager.UnregisterAppFromStartup(AppName, AppPath);
			TextBoxHandling.Upper("Application is unregistered from starting with Windows!");
		}
		private void WindowsNotificationCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.WindowsNotificationPopup = true;
			TextBoxHandling.Upper("Application will notify changes with Windows Baloon Pop-up!");
		}
		private void WindowsNotificationCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.WindowsNotificationPopup = false;
			TextBoxHandling.Upper("Application will not notify changes with Windows Baloon Pop-up!");
		}

		// Buttons
		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			ProcessSelectionWindow selection = new ProcessSelectionWindow(this);
			selection.Show();
			selection.Activate();
			AddButton.IsEnabled = false;
		}
		private void AddManuallyButton_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new()
			{
				Title = "Select an Application",
				Filter = "Executable files (*.exe)|*.exe" + "|All Files|*.*",
				CheckFileExists = true
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
				string fileLocation = openFileDialog.FileName;
				var program = new ProgramModel
				{
					Name = fileName,
					Location = fileLocation,
					Icon = IconHandler.ExtractIcon(fileLocation)
				};
				Processes.AddProgramToDatabase(program);
				UpdateProgram();
			}
		}
		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			if (ProcessListBox.SelectedItem is ProgramModel selectedProgram)
			{
				Processes.RemoveProgramFromDatabase(selectedProgram);
				UpdateProgram();

				TextBoxHandling.Lower($"Program {selectedProgram.Name} has been removed!");
			}
			else
			{
				TextBoxHandling.Lower("Please select a program to remove!");
			}
		}

		// ListBox
		private void ProcessListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listBoxItem = sender as ListBoxItem;
			if (ProcessListBox.SelectedItem is ProgramModel selectedProcess)
			{
				TextBoxHandling.Lower($"Process selected: {selectedProcess.Name}", true);
			}
		}
		private void ProcessListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Run order: ComboBox check, Deselection.
			// Be able to open the ComboBox when the ListBox Item is selected
			var originalSource = e.OriginalSource as DependencyObject;
			while (originalSource != null)
			{
				if (originalSource is ComboBox)
				{
					return;
				}
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			// Deselect ListBox Item if the Left Button is pressed outside or on the same Item
			if (ProcessListBox.SelectedItem != null)
			{
				var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
				if (clickedItem == null || clickedItem.IsSelected)
				{
					ProcessListBox.SelectedItem = null;
					e.Handled = true;
				}
			}
		}
		private void ProcessListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Deselect ListBox Item if the Right Button is pressed outside an Item
			var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
			if (clickedItem == null)
			{
				ProcessListBox.SelectedItem = null;
				e.Handled = true;
			}
		}
		private void ProcessListBox_ContextMenuOpening(object sender, RoutedEventArgs e)
		{
			// Don't open context menu if right clicked on the ComboBox
			var originalSource = e.OriginalSource as DependencyObject;
			while (originalSource != null)
			{
				if (originalSource is ComboBox || ProcessListBox.SelectedItem is not ProgramModel selectedProcess)
				{
					e.Handled = true;
					return;
				}
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Make changes to the ComboBox only when it was clicked by mouse.
			// If not, the ComboBox will trigger infinite ChangedEvent loop
			var comboBox = sender as ComboBox;
			if (comboBox != null && comboBox.IsMouseOver)
			{
				Processes.SaveDatabase(ProgramsInUI.ToList());
			}
		}
		private void ComboBoxProcess_DropDownClosed(object sender, EventArgs e)
		{
			if (System.Windows.Input.Mouse.DirectlyOver is ComboBox comboBox)
			{
				comboBox.IsDropDownOpen = true;
			}
		}
		private void OpenFileLocationProperty_Click(object sender, RoutedEventArgs e)
		{
			if (ProcessListBox.SelectedItem is ProgramModel selectedProcess)
			{
				string? programPath = selectedProcess.Location;
				FileExplorer.ShowFileInExplorer(programPath);
				// Simpler but opens a new explorer.exe process every time it is called
				// System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{ programPath }\"");
				ProcessListBox.SelectedItem = null;
				e.Handled = true;
			}
		}

		// Window State
		private void Window_StateChanged(object sender, EventArgs e)
		{
			// Add variable in Menu for enabling or disabling this option
			if (WindowState == WindowState.Minimized && Properties.Settings.Default.MinimizeToTray == true)
			{
				Hide();
			}
		}

		// UI Theme
		private void ChangeTheme(Uri themeUri)
		{
			ResourceDictionary Theme = new ResourceDictionary() { Source = themeUri };
			App.Current.Resources.Clear();
			App.Current.Resources.MergedDictionaries.Add(Theme);
		}

		public void SavePropertiesSettings()
		{
			foreach (MenuItem item in ThemeMenuItems.Items)
			{
				if (item.IsChecked == true)
					Properties.Settings.Default.Theme = $"{(string)item.Header}";
			}

            Properties.Settings.Default.Save();
        }
	}
}
