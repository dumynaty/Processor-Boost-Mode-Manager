# Processor Boost Mode Manager

## Description: 
The application sets the Processor Performance Boost Mode depending on a selected set of programs that are running and their associated boost mode preferences.

## Use
When running low performance, casual tasks (browsing, listening to music etc.) the boost mode can be set to Disabled. This limits the processor's maximum frequency to its base speed and modifies how it reaches that speed. When running heavy, demanding tasks (simulations, CAD programs, gaming etc.) the boost mode can be set to Aggressive or to an other preffered state.
#### Note: Designed for Windows 10 systems. Not tested, but should also support Windows 8 and Windows 11.


## Available Boost Modes & Effect
#### Default Boost Mode: Aggressive
* **Disabled** - Don't select target frequencies above maximum frequency
* **Enabled** - Select target frequencies above maximum frequency
* **Aggressive** - Always select the highest possible target frequency above nominal frequency
* **EfficientEnabled** - Select target frequencies above maximum frequency if hardware supports doing so efficiently
* **EfficientAggressive** - Always select the highest possible target frequency above nominal frequency if hardware supports doing so efficiently
* **AggressiveAtGuaranteed** - Always select the highest possible target frequency above guaranteed frequency
* **EfficientAggressiveAtGuaranteed** - Always select the highest possible target frequency above guaranteed frequency if hardware supports doing so efficiently

## UI

### Menu Items 
#### File
* Exit - Closes the application

#### Options
* Minimize To Tray - If checked, application will minimize to tray instead of the taskbar
* Boost Modes - Select the boost modes you want to be able to assign to your programs (minimum of 2 required)

#### View
* Refresh Now - Checks for changes and update the UI
* Update Speed - Sets the interval (in seconds) for the application to check for changes

#### Help
* App info - Opens a link to this README in the default browser
* Reset Settings to Default - Resets menu item selections
* Clear Database - Clears all programs from the list

### CheckBoxes
* Autostart with Windows - If checked, registers the application to start with windows (program will start minimized)
* Windows Pop-up Notifications - If checked, will pop-up windows notifications when the boost mode changes

### Buttons
* Add - Opens a window listing currently running processes, allowing the user to select one to add to the main list
* Add Manually - Opens a File Dialog to choose a (.exe) program to be added to the main list
* Remove - Removes selected program from the list

### TextBoxes
* Upper text box: Displays the count of currently running programs and other messages
* Lower text box: Shows current windows boost mode and other messages

### ListBox
Each item in the list will have the following characteristics:
* Icon
* Name
* ComboBox
Pressing **Right-Click** on a program will open a context menu with an option to open the program's file location.

Each program name will have different colouring:
* Black - Program is not running
* Green - Program is running but does not have the highest boost mode
* Blue - Program is running and has the highest boost mode
* Red - Program is not found (a strikeout will also be applied)

### Tray Icon
The application can be minimized to tray. Pressing **Left-Click** will show and bring the application on top. Pressing **Middle-Click** will shutdown the application. Pressing **Right-Click** will pop up a context menu with the following options:
* Open - Shows and brings the window on top 
* Open File Location - Opens the application path using explorer
* Close - Closes the application
