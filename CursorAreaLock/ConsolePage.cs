using System;
using System.Collections.Generic;
using System.Text;

namespace System.ConsoleExtentions
{
    class ConsolePage<T>
    {
        public delegate void PrintHandler(T data);
        private PrintHandler _printHandler;
        public HotkeyBar Hotkeys { get; set; }
        public T DataProvider { get; set; }

        public ConsolePage(PrintHandler handler, T data, HotkeyBar hotkeybar)
        {
            this._printHandler = handler;
            this.DataProvider = data;
            this.Hotkeys = hotkeybar;
        }

        public void Print()
        {
            try
            {
                //print this page ...
                _printHandler(DataProvider);
            }
            catch (Exception) {  }
        }

    }
}
