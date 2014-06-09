using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WinAPI
{
    public static class User32
    {
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const uint EVENT_SYSTEM_SWITCHEND = 0x0015;
        public const uint EVENT_SYSTEM_SWITCHSTART = 0x0014;

        //Messages are not removed from the queue after processing by PeekMessage.
        public const uint PM_NOREMOVE = 0x0000;

        //Messages are removed from the queue after processing by PeekMessage.
        public const uint PM_REMOVE = 0x0001;

        // Prevents the system from releasing any thread that is waiting for the caller to go idle (see WaitForInputIdle).
        public const uint PM_NOYIELD = 0x0002;

        //windowLong flags
        public const int GWL_STYLE = -16;

        //window style
        public const UInt32 WS_MAXIMIZE = 0x1000000;

        [DllImport("user32")]
        public static extern bool GetWindowInfo(IntPtr hWnd, out WINDOWINFO pwi);

        [DllImport("user32")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        public static extern bool PeekMessage(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32")]
        public static extern bool TranslateMessage(ref MSG lpMsg);
        
        [DllImport("user32")]
        public static extern IntPtr DispatchMessage(ref MSG lpmsg);

        [DllImport("user32")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32")]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32")]
        public static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32")]
        public static extern bool ClipCursor(ref RECT lpRect);
        
        [DllImport("user32")]
        public static extern bool GetClipCursor(out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public UInt32 message;
            public IntPtr wParam;
            public IntPtr lParam;
            public UInt32 time;
            public POINT pt;
        }
    }
}
