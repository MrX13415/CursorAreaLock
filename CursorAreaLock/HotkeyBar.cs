using System;
using System.Collections.Generic;
using System.Text;

namespace System.ConsoleExtentions
{
    public class HotkeyBar
    {
        private List<HotkeyBarItem> _items = new List<HotkeyBarItem>();
        public List<HotkeyBarItem> Items { get { return _items; } }

        private List<int> positionsFilled = new List<int>();

        public ConsoleColor TriggerKeyForegroundColor { get; set; }
        public ConsoleColor ItemNameForegroundColor { get; set; }

        public ConsoleColor TriggerKeyBackgroundColor { get; set; }
        public ConsoleColor ItemNameBackgroundColor { get; set; }

        public bool HotkeyNameSeperatorVisible { get; set; }
        public string HotkeyNameSeperator { get; set; }
        public int Position { get; set; }
        public int GridWidth { get; set; }
        //public bool Debug { get; set; }
        //private bool debug_colortoggled;

        public HotkeyBar() : this(0) { }
        public HotkeyBar(int gridSizeX)
        {
            this.TriggerKeyForegroundColor = ConsoleColor.Yellow;
            this.ItemNameForegroundColor = ConsoleEx.DefaultForegroundColor;

            this.TriggerKeyBackgroundColor = ConsoleEx.DefaultBackgroundColor;
            this.ItemNameBackgroundColor = ConsoleEx.DefaultBackgroundColor;

            this.HotkeyNameSeperator = ": ";
            this.HotkeyNameSeperatorVisible = true;
            this.Position = 3;
            this.GridWidth = gridSizeX;
        }

        /// <summary>
        /// Checks if any key has been pressed and triggers every matching hotkey-item, if any
        /// </summary>
        /// <returns>True if a key was pressed and at least one matching trigger has been found, otherwise False</returns>
        public bool Process()
        {
            if (Console.KeyAvailable) 
                return TriggerHotkeyEvents(Console.ReadKey(true));

            return false;
        }

        private bool TriggerHotkeyEvents(ConsoleKeyInfo input)
        {
            bool triggermatch = false;

            foreach (HotkeyBarItem item in Items)
            {
                //Trigger the EventHandler for each matching hotkey ...
                if (item.Enabled && item.TriggerKey == input.Key)
                {
                    triggermatch = true;
                    item.EventHandler(item);
                }    
            }
            return triggermatch;
        }


        /// <summary>
        /// Prints a message instead of the hotkey bar
        /// </summary>
        /// <param name="message">The message to be printed</param>
        public void Print(string message, ConsoleColor color)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.SetCursorPosition(0, Console.WindowHeight - Position);
            ConsoleEx.WriteLine(message, color);
            Console.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Prints the hotkey bar
        /// </summary>
        public void Print()
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.SetCursorPosition(0, Console.WindowHeight - Position);
            positionsFilled.Clear();

            int lastNoGridCursorLeft = 0;
            int lastGridCursorLeft = 0;

            //draw all available hotkeys ...
            foreach (HotkeyBarItem item in Items)
            {
                if (item.GridPosition >= 0)
                {
                    int itemPos = Console.WindowWidth / GridWidth * item.GridPosition;
                    int length = itemPos - Console.CursorLeft;
                    Console.Write("".PadLeft(length));
                }
                else
                {
                    Console.SetCursorPosition(lastNoGridCursorLeft, Console.WindowHeight - Position);
                }
                
                DrawItem(item);

                if (item.GridPosition == -1)
                    lastNoGridCursorLeft = Console.CursorLeft;
                else
                    lastGridCursorLeft = Console.CursorLeft;
            }
            int lastPos = lastGridCursorLeft > lastNoGridCursorLeft ? lastGridCursorLeft : lastNoGridCursorLeft;
            Console.SetCursorPosition(lastPos, Console.WindowHeight - Position);
            Console.Write("".PadLeft(Console.WindowWidth - lastPos));

            //make sure only thes hotkeys are visible wich have the visiblity option set to true ...
            //for (int i = 0; i < GridWidth; i++)
            //{
            //    //there is already something drawn at this position ...
            //    if (positionsFilled.Contains(i)) continue;

            //    //draw an empty fake hotkey to override previos hotkeys ...
            //    HotkeyBarItem tmpItem = new HotkeyBarItem(ConsoleKey.A, "", "", null, i);
            //    tmpItem.Enabled = false;
            //    tmpItem.Visible = true;
            //    DrawItem(tmpItem);
            //}

            Console.SetCursorPosition(x, y);
        }

        private void DrawItem(HotkeyBarItem item)
        {
            if (!item.Visible) return;

            if (item.GridPosition >= 0)
            {
                int itemPos = Console.WindowWidth / GridWidth * item.GridPosition;
                Console.SetCursorPosition(itemPos, Console.WindowHeight - Position);
            }

            string key = String.Format("{0, 5}", " " + item.TriggerKeyDisplayName);
            string value = item.ItemName;
            if (GridWidth > 0 && item.GridPosition >= 0)
            {
                int cellwidth = (Console.WindowWidth - 1) / GridWidth;
                value = String.Format("{0, " + (cellwidth - 5 - HotkeyNameSeperator.Length) * -1 + "}", value);
            }

            ConsoleColor backColor = Console.BackgroundColor;
            ConsoleColor nameColor = ItemNameForegroundColor;

            if (item.Enabled)
            {
                if (value.Equals(""))
                    ConsoleEx.Write(item.TriggerKeyDisplayName, TriggerKeyForegroundColor);
                else
                    ConsoleEx.WriteKeyValue(key, value, nameColor, TriggerKeyForegroundColor, HotkeyNameSeperator);
            }
            else
            {
                int textspace = key.Length + value.Length + HotkeyNameSeperator.Length;  
                Console.Write(String.Format("{0," + textspace + "}", ""));
            }

            //positionsFilled.Add(item.GridPosition);
        }
    }

    public class HotkeyBarItem
    {
        public delegate void HotkeyEvent(HotkeyBarItem item);
        public ConsoleKey TriggerKey { get; set; }
        public string TriggerKeyDisplayName { get; set; }
        public string ItemName { get; set; }
        public HotkeyEvent EventHandler { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }

        private int _gridPosition;
        public int GridPosition { get { return _gridPosition; } set { _gridPosition = value < -1 ? -1 : value; } }

        public HotkeyBarItem(ConsoleKey key, string name, HotkeyEvent handler) : this(key, key.ToString(), name, handler) { }
        public HotkeyBarItem(ConsoleKey key, string name, HotkeyEvent handler, int gridPosition) : this(key, key.ToString(), name, handler, gridPosition) { }
        public HotkeyBarItem(ConsoleKey key, string keytext, string name, HotkeyEvent handler) : this(key, keytext, name, handler, -1) { }
        public HotkeyBarItem(ConsoleKey key, string keytext, string name, HotkeyEvent handler, int gridPosition)
        {
            this.TriggerKey = key;
            this.TriggerKeyDisplayName = keytext;
            this.ItemName = name;
            this.EventHandler = handler;
            this.Enabled = true;
            this.Visible = true;
            this.GridPosition = gridPosition;
        }

        public void Show()
        {
            this.Enabled = true;
            this.Visible = true;
        }

        public void Hide()
        {
            this.Enabled = false;
            this.Visible = false;
        }
    }
}
