using System;
using System.Collections.Generic;
using System.Text;

namespace System.ConsoleExtentions
{
    class ItemList
    {
        private List<String> _items = new List<String>();
        public List<String> Items { get { return _items; } }

        public ConsoleColor ItemForegroundColor { get; set; }
        public ConsoleColor SelectionForegroundColor { get; set; }
        public ConsoleColor ItemBackgroundColor { get; set; }
        public ConsoleColor SelectionBackgroundColor { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private int vStartIndex;
        private int vEndIndex;

        private int _selectionIndex;
        public int SelectionIndex
        {
            get { return _selectionIndex; }
            set
            {
                _selectionIndex = value > Items.Count - 1 ? Items.Count - 1
                : value < 0 ? 0 : value;
            }
        }

        public ItemList()
        {
            ItemForegroundColor = ConsoleEx.DefaultForegroundColor;
            ItemBackgroundColor = ConsoleEx.DefaultBackgroundColor;
            SelectionForegroundColor = ConsoleColor.Black;
            SelectionBackgroundColor = ConsoleColor.Yellow;
        }

        public void Print()
        {
            if (Items.Count == 0) {
                Items.Add("(empty)");
            }

            int _x = Console.CursorLeft;
            int _y = Console.CursorTop;
            ConsoleColor bColor = Console.BackgroundColor;
            ConsoleColor fColor = Console.ForegroundColor;

            vEndIndex = vStartIndex + Height - 1;

            if (SelectionIndex >= Items.Count)
                SelectionIndex = Items.Count - 1;

            if (SelectionIndex > vEndIndex)
            {
                vEndIndex = SelectionIndex;
                vStartIndex = vEndIndex - (Height - 1);
            }

            if (SelectionIndex < vStartIndex)
            {
                vStartIndex = SelectionIndex;
                vEndIndex = vStartIndex + Height - 1;
            }

            for (int index = vStartIndex; index <= vEndIndex; index++)
            {
                Console.SetCursorPosition(X, Y + index - vStartIndex);
                Console.BackgroundColor = ItemBackgroundColor;
                Console.ForegroundColor = ItemForegroundColor;

                if (index >= Items.Count)
                {
                    Console.Write("".PadRight(Width));
                    continue;
                }

                //is selected ...
                if (index == _selectionIndex)
                {
                    Console.BackgroundColor = SelectionBackgroundColor;
                    Console.ForegroundColor = SelectionForegroundColor;
                }

                string itemDisplayText = String.Format(" {0, " + (Width - 3) * -1 + "} ", ConsoleEx.CutText(Items[index], Width - 2));
                Console.Write(itemDisplayText);
            }

            //Console.SetCursorPosition(_x, _y);
            Console.BackgroundColor = bColor;
            Console.ForegroundColor = fColor;
        }
    }
}
