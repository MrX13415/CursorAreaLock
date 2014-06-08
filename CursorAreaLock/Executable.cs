using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CursorAreaLock
{
    class Executable
    {
        public string DisplayName { get; set; }
        public string ExecutablePath { get; set; }

        public Executable(string exe) : this(exe, Path.GetFileName(exe)) { }
        public Executable(string exe, string displayName)
        {
            ExecutablePath = exe;
            DisplayName = displayName;
        }

        public override bool Equals(object obj)
        {
            if (obj is Executable)
                return this.ExecutablePath.ToLower().Equals(((Executable)obj).ExecutablePath.ToLower());
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.ExecutablePath.ToLower().GetHashCode();
        }
    }
}
