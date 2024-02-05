using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AngelHornetLibrary.CLI
{
    public class MenuCli
    {
        public struct MenuItem
        {
            public List<String> commandStrings;
            public Action? actionOnSelect;
            public MenuItem() => (commandStrings, actionOnSelect) = (new List<String>(), null);
            public MenuItem(List<String> commandStrings, Action actionOnSelect) { this.commandStrings = commandStrings; this.actionOnSelect = actionOnSelect; }
        }

        public MenuItem[] menuItems = new MenuItem[0];

        public Action? EntryAction { get; set; } = null;
        public Action? ExitAction { get; set; } = null;
        public Action? DefaultAction { get; set; } = null;

        public bool UseNumbers { get; private set; } = true;  // Use Numbers for Menu Items ... will fix this later
        public bool PadMenuItems { get; set; } = false;        // Pad Menu Items to MenuItemWidth   
        public int MenuItemWidth { get; set; } = 14;        // 6 menu items vs Standard old VT100 Terminal Width
        public int MenuMaxWidth { get; set; } = 90;         // Standard old VT100 Terminal Width
        public bool RunLoop { get; set; } = false;
        public string ErrorMsg { get; set; } = "";



        public void AddOnEntry(Action actionOnSelect)
        {
            if (actionOnSelect == null) throw new ArgumentNullException("actionOnSelect");
            EntryAction = actionOnSelect;
        }
        public void AddOnEntry(int index) => EntryAction = menuItems[index].actionOnSelect;
        public void AddOnExit(Action actionOnSelect) => ExitAction = actionOnSelect;
        public void AddOnExit(int index) => ExitAction = menuItems[index].actionOnSelect;
        public void AddDefault(Action actionOnSelect) => DefaultAction = actionOnSelect;
        public void AddDefault(int index) => DefaultAction = menuItems[index].actionOnSelect;
        public Action? GetEntryAction() => EntryAction;
        public Action? GetExitAction() => ExitAction;
        public Action? GetDefaultAction() => DefaultAction;
        public Action? GetAction(int index) => menuItems[index].actionOnSelect;


        public void AddItem(string command, Action actionOnSelect) => AddItem(new List<string> { command }, actionOnSelect);
        public void AddItem(List<String> commands, Action actionOnSelect)
        {
            Array.Resize(ref menuItems, menuItems.Length + 1);
            menuItems[^1] = new MenuItem(commands, actionOnSelect);
        }


        public void PrintMenu()
        {
            Console.WriteLine(ErrorMsg);
            ErrorMsg = "";
            String sum = "";
            for (int i = 0; i < menuItems.Length; i++)
            {
                string command = "[";
                if (UseNumbers) command += $"{i + 1}:";
                command += menuItems[i].commandStrings[0];
                if (PadMenuItems) command = command.PadRight(MenuItemWidth - 2);
                if (command.Length > MenuItemWidth - 2) command = command.Substring(0, MenuItemWidth - 2);
                command += "] ";
                sum += command;
                if (sum.Length > MenuMaxWidth)
                {
                    Console.WriteLine();
                    sum = "";
                }
                Console.Write($"{command}");
            }
            Console.WriteLine();
        }


        // public Action Exit() => Exit(true);
        // public Action Exit(bool T)
        public Action? Exit()
        {
            RunLoop = false;
            return null;
        }


        public void DefaultActionInvoke()
        {
            if (DefaultAction != null)
                DefaultAction.Invoke();
            else if (menuItems.Length >= 1 && menuItems[0].actionOnSelect != null)
                menuItems[0].actionOnSelect.Invoke();
        }


        public Action? Loop()
        {
            RunLoop = true;

            if (EntryAction != null) EntryAction.Invoke();
            while (RunLoop)
            {
                PrintMenu();
                Console.Write("Enter Command: ");
                string input = Console.ReadLine();
                if (input == "") DefaultActionInvoke();
                else if (int.TryParse(input, out int index))
                {
                    if (index > 0 && index <= menuItems.Length)
                        menuItems[index - 1].actionOnSelect?.Invoke();
                    else
                    {
                        ErrorMsg = "Invalid Command:  Index out of Range";
                        DefaultActionInvoke();
                    }
                }
                else
                {
                    ErrorMsg = "Invalid Command:  Failed to Parse Input";
                    DefaultActionInvoke();
                }
            }
            if (ExitAction != null) ExitAction.Invoke();
            return null;
        }

    }
}
