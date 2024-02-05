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
        public bool UseNumbers { get; set; } = true;
        public int MenuItemWidth { get; set; } = 11;
        public int MenuItemMaxCount { get; set; } = 7;
        public bool RunLoop { get; set; } = true;

        // Debug and Warning Variables
        static bool warnAddItem = false;

        struct MenuItem
        {
            public List<String> commandStrings;
            public Action? actionOnSelect;
            public MenuItem() => (commandStrings, actionOnSelect) = (new List<String>(), null);
            public MenuItem(List<String> commandStrings, Action actionOnSelect) { this.commandStrings = commandStrings; this.actionOnSelect = actionOnSelect; }
        }

        MenuItem[] menuItems = new MenuItem[0];

        // CRUD Menu
        // 1.  Create
        // 2.1 Read
        // 2.2 Details  // [3:Details] |
        // 3.  Update
        // 4.  Delete
        // 5.  Exit

        // Default Action is the First Action
        public void AddItem(string command, Action actionOnSelect) => AddItem(new List<string> { command }, actionOnSelect);
        // public void AddItem (string[] commands, Action actionOnSelect) => AddItem(new List<string>(commands), actionOnSelect);
        public void AddItem(List<String> commands, Action actionOnSelect)
        {
            if (!warnAddItem && menuItems.Length >= MenuItemMaxCount)
            {
                Debug.WriteLine($"MenuCliAddItem: Menu is Full.  Maximum MenuItems is set to {MenuItemMaxCount}");
                warnAddItem = true;
            }
            Array.Resize(ref menuItems, menuItems.Length + 1);
            menuItems[^1] = new MenuItem(commands, actionOnSelect);
        }

        public void PrintMenu()
        {
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i != 0 && i % MenuItemMaxCount == 0) Console.WriteLine();
                string command = "[";
                if (UseNumbers) command += $"{i + 1}:";
                command += menuItems[i].commandStrings[0];
                command = command.PadRight(MenuItemWidth - 2);
                if (command.Length > MenuItemWidth - 2) command = command.Substring(0, MenuItemWidth - 2);
                command += "] ";
                Console.Write($"{command}");
            }
            Console.WriteLine();
        }

        // public Action Exit() => Exit(true);
        // public Action Exit(bool T)
        public Action? Exit()
        {
            Console.WriteLine("Exiting Menu");
            RunLoop = false;
            return null;
        }
        public Action? Loop() => Run();
        public Action? Run()
        {
            RunLoop = true;
            while (RunLoop)
            {
                PrintMenu();
                Console.Write("Enter Command: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int index))
                {
                    if (index > 0 && index <= menuItems.Length)
                    {
                        menuItems[index - 1].actionOnSelect?.Invoke();
                    }
                    else
                    {
                        Console.WriteLine("Invalid Command:  Index out of Range");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Command:  Failed to Parse Input");
                }
            }
            return null;
        }

    }




}
