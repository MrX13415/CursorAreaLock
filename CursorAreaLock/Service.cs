using Microsoft.WinAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ConsoleExtentions;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CursorAreaLock
{
    class Service
    {
        public enum ServiceMode{
            NormalMode, CaptureMode
        }

        private const string saveFile = "options.dat";

        private List<Executable> _executables = new List<Executable>();
        public List<Executable> Executables { get { return _executables; } }

        public bool IsCurrentExecutableMatching { get { return Executables.Contains(CurrentExecutable); } }

        public Executable ServiceExecutable { get; private set; }
        public Executable CurrentExecutable { get; private set; }
        public Executable LastCapturedExecutable { get; private set; }
        public IntPtr LastCapturedWindowHandle { get; private set; }
        public IntPtr CurrentWindowHandle { get; private set; }
        public ServiceMode Mode { get; set; }

        private IntPtr _altTabWindowHandle;

        private bool _serviceState;
        private bool _lockState;
        private bool _clipState;
        private bool keys_AltTab;


        private User32.POINT _cursorPos;
        private User32.RECT _defaultCursorBounds;
        private User32.RECT _currentWindowBounds;

        private IntPtr m_hhook;
        private User32.WinEventDelegate _procDelegate;

        public User32.POINT CursorPosition { get { return _cursorPos; } }
        public User32.RECT DefaultCursorBounds { get { return _defaultCursorBounds; } }
        public User32.RECT CurrentWindowBounds { get { return _currentWindowBounds; } }

        public bool Active { get { return _serviceState; } set { _serviceState = value; } }
        public bool IsLockActive { get { return _lockState; } }
        public bool IsClipApplied { get { return _clipState; } }
        public bool CapturedExecutableAvailable { get { return LastCapturedExecutable != null; } }

        public Service(){
            //obtain the executable name which runs this service ...
            ServiceExecutable = new Executable(Process.GetCurrentProcess().MainModule.FileName);
            
            //set default running mode ...
            Mode = ServiceMode.NormalMode;

            //init. window event hook ...
            InitHook();
        }

        private void InitHook(){
            //make sure the event delegate isn't getting lost ...
            _procDelegate = new User32.WinEventDelegate(WinEventProc);

            m_hhook = User32.SetWinEventHook(
                User32.EVENT_SYSTEM_SWITCHSTART, // eventMin
                User32.EVENT_SYSTEM_SWITCHEND,   // eventMax
                IntPtr.Zero,                     // hmodWinEventProc
                _procDelegate,                   // lpfnWinEventProc
                0,                               // idProcess
                0,                               // idThread
                User32.WINEVENT_OUTOFCONTEXT);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == User32.EVENT_SYSTEM_SWITCHSTART)
            {
                keys_AltTab = true;
            }

            if (eventType == User32.EVENT_SYSTEM_SWITCHEND)
            {
                keys_AltTab = false;
            }
        }

        public void Update()
        {
            try
            {
                bool asd = keys_AltTab;

                //event message loop ...
                User32.MSG _eventMessage = new User32.MSG();
                while (User32.PeekMessage(ref _eventMessage, IntPtr.Zero, 0, 0, User32.PM_REMOVE))
                {
                    User32.TranslateMessage(ref _eventMessage);
                    User32.DispatchMessage(ref _eventMessage);
                }

                //set current curosr pos ...
                User32.GetCursorPos(out _cursorPos);

                //get current window with focus ...
                CurrentWindowHandle = User32.GetForegroundWindow();
                if (CurrentWindowHandle == null) return;

                if (keys_AltTab) _altTabWindowHandle = CurrentWindowHandle;

                Process process = GetProcessByHandle(CurrentWindowHandle);
                if (process == null) return;

                ProcessModule pModule = process.MainModule;
                if (pModule == null) return;

                //set current exe ...
                CurrentExecutable = new Executable(pModule.FileName);
            }catch { }
        }

        public void ProcessService(){
            if (CurrentWindowHandle == null ||
                CurrentExecutable == null) return;

            if (_altTabWindowHandle == CurrentWindowHandle)
                return;

            switch (Mode)
            {
                case ServiceMode.NormalMode:
                    DoNormalMode();
                    break;
                case ServiceMode.CaptureMode:
                    DoCaptureMode();
                    break;
            }

            //set lock if needed ...
            if (_lockState)
            {
                //backup current cursor bounds first ...
                if (!_clipState) User32.GetClipCursor(out _defaultCursorBounds);

                //set new cursor bounds ...
                User32.ClipCursor(ref _currentWindowBounds);
                _clipState = true;
            }
            else if (_clipState)
            {
                //restore default cursor bounds ...
                User32.ClipCursor(ref _defaultCursorBounds);
                _clipState = false;
            }
        }

        public void SetNormalMode()
        {
            Mode = ServiceMode.NormalMode;
        }

        public void SetCaptureMode()
        {
            Mode = ServiceMode.CaptureMode;
        }

        public void AddCapturedExecutable()
        {
            if (LastCapturedExecutable != null)
                Executables.Add(LastCapturedExecutable);
            LastCapturedExecutable = null;
            //go back to normal mode ...
            SetNormalMode();
        }

        private void DoCaptureMode()
        {
            if (!CurrentExecutable.Equals(ServiceExecutable) && !keys_AltTab)
            {
                //mark captured executable for adding ...
                LastCapturedExecutable = CurrentExecutable;
                LastCapturedWindowHandle = CurrentWindowHandle;
            }
        }

        private void DoNormalMode()
        {
            if (IsCurrentExecutableMatching)
            {
                //reacalc current window bounds ...
                User32.GetWindowRect(CurrentWindowHandle, out _currentWindowBounds);

                //activate cursor lock if the service is active ...
                _lockState = _serviceState;
            }
            else _lockState = false;
        }

        public string GetCurrentWindowTitle()
        {
            if (CurrentWindowHandle == null) return "";
            StringBuilder sb = new StringBuilder(1024);
            User32.GetWindowText(CurrentWindowHandle, sb, sb.MaxCapacity);
            return sb.ToString();
        }

        public string GetWindowTitle(IntPtr windowHandle)
        {
            if (windowHandle == null) return "";
            StringBuilder sb = new StringBuilder(1024);
            User32.GetWindowText(windowHandle, sb, sb.MaxCapacity);
            return sb.ToString();
        }

        public bool SaveToDisk()
        {
            if (File.Exists(saveFile))
                File.Delete(saveFile);

            System.IO.StreamWriter file = null;
            try
            {
                file = new System.IO.StreamWriter(saveFile);

                foreach (Executable exe in _executables)
	            {
                    file.WriteLine(exe.ExecutablePath);
	            }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (file != null) file.Close();
            }

            return true;
        }

        public bool LoadFromDisk()
        {
            if (!File.Exists(saveFile)) return false;

            System.IO.StreamReader file = null;
            try 
	        {
                file = new System.IO.StreamReader(saveFile);              

                string line;

                while ((line = file.ReadLine()) != null)
                {
                    Executables.Add(new Executable(line));
                }
	        }
	        catch (Exception)
	        {
                return false;
            }
            finally
            {
                if (file != null) file.Close();
            }

            return Executables.Count > 0;
        }

        public void CleanUp()
        {
            if (_clipState)
            {
                User32.ClipCursor(ref _defaultCursorBounds);
                _clipState = false;
            }

            User32.UnhookWinEvent(m_hhook);
        }

        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                User32.GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }
    }
}
