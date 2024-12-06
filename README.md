# Processor Boost Mode Manager

Description and purpose:
This app is meant to control the Windows Processor Performance Boost Mode depending on specifically added and running applications.
Practical example: when doing casual tasks on my laptop I want the temperature and fan noise to be low.
If gaming or running heavy tasks, I want the processor to give 100% irregardless of temperature or fan speed.

The default Windows Processor Performance Boost mode is: Aggresive (2)
Due to various tests I have found that only the 3 modes -Disabled, Enabled and Aggresive- are useful.
These are the possibilities to choose from in the application.
They correspond to the following processor settings: 
0 - Disabled - Don't select target frequencies above maximum frequency.
1 - Enabled - Select target frequencies above maximum frequency.
2 - Aggressive - Always select the highest possible target frequency above nominal frequency.

UI:
Add Button - Gives a list of currently running processes and gives you the possibility to select one to add to the list.
Add Manually Button - Allows the user to select a program through windows's OpenFileDialog to add to the list.
Remove Button - Removes a program from the list.

You can set the modes of each program in the list through it's own ComboBox.

When a prorgam is running it's name is displayed with the color Green.
If the running program has the highest mode of the 3 options it will have the color Blue.

On startup the program gets the current Windows Power Scheme and sets the desired value to the Processor Perfomance Boost Mode ACSettingIndex (preffered on laptops).
Every second, it checks the currently running programs and adjusts the value if needed.

Made for Windows 10 systems.
Not tested but might also run in: Windows 8, Windows 11
