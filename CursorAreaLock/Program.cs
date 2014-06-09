
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.WinAPI;
using System.ConsoleExtentions;

namespace CursorAreaLock
{
    class Program
    {
        /* 
         * CursorAreaLock v2.0
         * 
         * --System requirements--
         * 
         *   OS: Windows 2000 Professional or greater
         *   
         *  Version: 2.0
         *      * [Code cleanup]
         *      * ADD: Handle Alt+Tab
         *      * ADD: Exe list
         *      
         *  Version: 1.1
         *      * FIX: cursor visible
         *      * FIX: handle console window resize
         */

        private static Program app;

        static void Main(string[] args)
        {
            app = new Program();
            app.Start(args);
        }

        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Copyright { get; private set; }

        private Kernel32.ConsoleCloseEventHandler handler;

        private bool exitRequested;
        private bool cleanedUp;
        private bool mainLoopRunning = true;
        private int exitCode = 0;

        private int consoleWindowWidth = Console.WindowWidth;
        private int consoleWindowHeight = Console.WindowHeight;

        private Service service = new Service();
        
        private ConsolePage<Service> page;
        private ConsolePage<Service> nextPage;

        private ConsolePage<Service> mainPage;
        private ConsolePage<Service> optionsPage;
        private ConsolePage<Service> addExePage;

        //data holder for Executable list ...
        private ItemList exeViewList = new ItemList();

        public Program()
        {
            Name = "CursorAreaLock";
            Version = "2.1";
            Copyright = "(c) MrX13415 2014";
        }

        public void Exit()
        {
            exitRequested = true;
        }

        private void ProcessArgs(string[] args)
        {

        }

        private void Initialize()
        {
            //setup exit event handler ...
            handler += new Kernel32.ConsoleCloseEventHandler(ConsoleCloseEvent);
            Kernel32.SetConsoleCtrlHandler(handler, true);

            //make sure we are using default colors ...
            ConsoleEx.ResetColors();

            //set console settings ...
            Console.Title = String.Format("{0} v{1}", this.Name, this.Version);
            Console.CursorVisible = false;

            //init. console pages ...
            InitPages();

            //set default page;
            page = mainPage;

            //load service options file ...
            service.LoadFromDisk();
        }

        private void InitPages(){

            //** MAIN PAGE ***********************************
  
            HotkeyBarItem startStopKey = new HotkeyBarItem(ConsoleKey.F4, "Start / Stop", HotKeyEventHandler_StateToggle);
            HotkeyBarItem optinsKey = new HotkeyBarItem(ConsoleKey.F6, "Options", HotKeyEventHandler_Options);
            HotkeyBarItem exitKey = new HotkeyBarItem(ConsoleKey.F10, "Exit", HotKeyEventHandler_Exit, 3);

            HotkeyBar mainBar = new HotkeyBar(4);
            mainBar.Items.Add(startStopKey);
            mainBar.Items.Add(optinsKey);
            mainBar.Items.Add(exitKey);

            mainPage = new ConsolePage<Service>(MainPagePrintHandler, service, mainBar);

            //** OPTIONS PAGE ********************************

            HotkeyBarItem escKey1 = new HotkeyBarItem(ConsoleKey.Escape, "ESC", "Back", HotKeyEventHandler_OptionsCancel);
            HotkeyBarItem addKey = new HotkeyBarItem(ConsoleKey.Insert, "INS", "Add", HotKeyEventHandler_ListAdd);
            HotkeyBarItem removeKey = new HotkeyBarItem(ConsoleKey.Delete, "DEL", "Remove", HotKeyEventHandler_ListRem);
            HotkeyBarItem upKey = new HotkeyBarItem(ConsoleKey.UpArrow, "   ↑", "", HotKeyEventHandler_ListUp);
            HotkeyBarItem downKey = new HotkeyBarItem(ConsoleKey.DownArrow, " ↓", "", HotKeyEventHandler_ListDown);
            HotkeyBarItem homeKey = new HotkeyBarItem(ConsoleKey.Home, "HOME", "", HotKeyEventHandler_ListStart);
            homeKey.Visible = false;
            HotkeyBarItem endKey = new HotkeyBarItem(ConsoleKey.End, "END", "", HotKeyEventHandler_ListEnd);
            endKey.Visible = false;

            HotkeyBar optionsBar = new HotkeyBar(4);
            optionsBar.Items.Add(escKey1);
            optionsBar.Items.Add(addKey);
            optionsBar.Items.Add(removeKey);
            optionsBar.Items.Add(exitKey);
            optionsBar.Items.Add(upKey);
            optionsBar.Items.Add(downKey);
            optionsBar.Items.Add(homeKey);
            optionsBar.Items.Add(endKey);

            optionsPage = new ConsolePage<Service>(OptionsPagePrintHandler, service, optionsBar);

            //** ADD EXECUTABLE PAGE *************************

            HotkeyBarItem escKey2 = new HotkeyBarItem(ConsoleKey.Escape, "ESC", "Cancel", HotKeyEventHandler_CaptureModeCancel);
            HotkeyBarItem okKey = new HotkeyBarItem(ConsoleKey.Enter, " ENTER","Add Executable", HotKeyEventHandler_CaptureModeOK);

            HotkeyBar addExeBar = new HotkeyBar(4);
            addExeBar.Items.Add(escKey2);
            addExeBar.Items.Add(okKey);
            addExeBar.Items.Add(exitKey);

            addExePage = new ConsolePage<Service>(AddExePagePrintHandler, service, addExeBar);
        }

        private bool SwitchPage(ConsolePage<Service> _page)
        {
            if (nextPage != null) return false;
            nextPage = _page;
            return true;
        }

        public void Start(string[] args)
        {
            //process cmd args if any ...
            ProcessArgs(args);

            //do basic stuff and setup things for start ...
            Initialize();
           
            //start and initialize the main loop
            //will block here until Application exit ...
            StartMainLoop();

            //*** The Main Loop has been stoped ***

            //save service options file ...
            service.SaveToDisk();

            //Wait a bit before exit ...
            Thread.Sleep(100);

            //Exit now ...
            Environment.Exit(exitCode);
        }

        private void StartMainLoop()
        {
            //*** MAIN LOOP [START] **********************************************
            while (mainLoopRunning)
            {
                //Wait a bit ...
                Thread.Sleep(10);

                //prepare for exit ...
                if (exitRequested) CleanUp();

                //detact console window size changhes ...
                CheckConsoleWindowSize();

                //make sure the page content will fit ...
                if (ConsoleContentSizeMatching())
                {
                    //PrepareConsole console for page redprint ...
                    PrepareConsole();

                    //*** SERVICE ************************************************
                    service.Update();
                    service.ProcessService();

                    //*** PAGES **************************************************
                    page.Print();

                    //print the current available hotkeys, print "Exit" instead, if exit was requested  ...
                    if (exitRequested)
                        page.Hotkeys.Print("   Exiting ...", ConsoleColor.Yellow);
                    else 
                        page.Hotkeys.Print();
                }
 
                //handle the current available hotkeys
                if (!exitRequested)
                    page.Hotkeys.Process();

                //switch safely to an other console page ...
                if (nextPage != null)
                {
                    page = nextPage;
                    nextPage = null;
                    Console.Clear();
                }

                //exit main loop ...
                if (exitRequested && cleanedUp) mainLoopRunning = false;
            }
            //*** MAIN LOOP [END] ************************************************
        }
        
        private void CleanUp()
        {
            service.Active = false;
            service.CleanUp();
            cleanedUp = true;
        }

        public void PrepareConsole()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(String.Format("\n  {0}  ", this.Name));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Format("v{0}  {1}\n\n\n", this.Version, this.Copyright));
            ConsoleEx.ResetColors();
        }

        private bool ConsoleContentSizeMatching()
        {
            //TODO: recalc min size ...

            //Check if the console window is big enough ...
            if (Console.WindowWidth < 56 || Console.WindowHeight <= 18)
            {
                Console.SetCursorPosition(0, 2);
                ConsoleEx.WriteLineCenter("- Console window to small -", ConsoleColor.Yellow);
                return false;
            }

            return true;
        }

        private void CheckConsoleWindowSize()
        {
            //Console size has changed ... ?
            if (Console.WindowWidth != consoleWindowWidth || Console.WindowHeight != consoleWindowHeight)
            {
                //update the new size ...
                consoleWindowWidth = Console.WindowWidth;
                consoleWindowHeight = Console.WindowHeight;
                //everything has to be reprinted ...
                Console.Clear();
            }
        }

        //*** EVENT HANDLERS [START] *********************************************

        private void HotKeyEventHandler_Exit(HotkeyBarItem item)
        {
            app.Exit();
        }

        private static bool ConsoleCloseEvent(Kernel32.CtrlType sig)
        {
            app.Exit();
            return true;
        }

        //*** MAIN PAGE **********

        private void MainPagePrintHandler(Service data)
        {
            string exeName = data.CurrentExecutable.DisplayName;

            if (data.CurrentExecutable.Equals(data.ServiceExecutable))
                exeName = "(this)";

            Console.SetCursorPosition(0, 6);

            ConsoleEx.WriteKeyValueLine("          State", service.Active ? "Active" : "Stoped", service.Active ? ConsoleColor.Green : ConsoleColor.Red);
            ConsoleEx.WriteLine();
            
            ConsoleEx.WriteKeyValueLine("     Executable", exeName, ConsoleColor.White);
            ConsoleEx.WriteLine();

            ConsoleEx.WriteKeyValueLine("    Cursor Lock", service.IsLockActive ? "Active" : "Inactive", service.IsLockActive ? ConsoleColor.Yellow : ConsoleColor.White);
            ConsoleEx.WriteLine();

            ConsoleEx.WriteKeyValueLine("         Cursor", String.Format("{0,5}, {1,5}", service.CursorPosition.X, service.CursorPosition.Y), ConsoleColor.White);
            ConsoleEx.WriteLine();

            if (data.IsCurrentExecutableMatching) 
            {
                ConsoleEx.WriteKeyValueLine("         Bounds",
                    String.Format("{0,5}, {1,5}, {2,5}, {3,5}",
                        service.CurrentWindowBounds.Left,
                        service.CurrentWindowBounds.Top,
                        service.CurrentWindowBounds.Right,
                        service.CurrentWindowBounds.Bottom),
                    ConsoleColor.White);
                 ConsoleEx.WriteLine();
            }
            else
            {
                ConsoleEx.WriteLine();
            }

        }

        private void HotKeyEventHandler_StateToggle(HotkeyBarItem item)
        {
            service.Active = !service.Active;
        }

        private void HotKeyEventHandler_Options(HotkeyBarItem item)
        {
            SwitchPage(optionsPage);
        }

        //*** OPTIONS PAGE *******

        private void OptionsPagePrintHandler(Service data)
        {
            Console.SetCursorPosition(10, 5);

            Console.WriteLine("Target Executables: ");
            ConsoleEx.WriteLine();

            exeViewList.Items.Clear();
            exeViewList.X = 10;
            exeViewList.Y = Console.CursorTop;
            exeViewList.Width = Console.WindowWidth - (exeViewList.X * 2);
            exeViewList.Height = Console.WindowHeight - exeViewList.Y - 7;

            /////DEBUG/////
            //if (service.Executables.Count == 0)
            //{
            //    for (int i = 0; i < 10000; i++)
            //    {
            //        service.Executables.Add(new Executable("exe.displayname_" + i + ".exe", "#" + i + " [exe.displayname]"));
            //    }
            //}
            /////DEBUG/////

            foreach (Executable exe in service.Executables)
            {
                exeViewList.Items.Add(exe.DisplayName);
            }

            exeViewList.Print();

            Console.SetCursorPosition(0, Console.WindowHeight - 6);
            if (service.Executables.Count > 0)
            {                
                ConsoleEx.WriteLine("          " + ConsoleEx.CutText(service.Executables[exeViewList.SelectionIndex].ExecutablePath, exeViewList.Width), ConsoleColor.White);
            }
            else
            {
                ConsoleEx.WriteLine();
            }
        }

        private void HotKeyEventHandler_OptionsCancel(HotkeyBarItem item)
        {
            SwitchPage(mainPage);
        }

        private void HotKeyEventHandler_ListAdd(HotkeyBarItem item)
        {
            SwitchPage(addExePage);
            service.SetCaptureMode();
        }

        private void HotKeyEventHandler_ListRem(HotkeyBarItem item)
        {
            if (service.Executables.Count > 0)
                service.Executables.RemoveAt(exeViewList.SelectionIndex);
        }
        private void HotKeyEventHandler_ListUp(HotkeyBarItem item)
        {
            exeViewList.SelectionIndex -= 1;
        }
        private void HotKeyEventHandler_ListDown(HotkeyBarItem item)
        {
            exeViewList.SelectionIndex += 1;
        }
        private void HotKeyEventHandler_ListStart(HotkeyBarItem item)
        {
            exeViewList.SelectionIndex = 0;
        }
        private void HotKeyEventHandler_ListEnd(HotkeyBarItem item)
        {
            exeViewList.SelectionIndex = exeViewList.Items.Count - 1;
        }

        //*** ADD EXE PAGE *******

        private void AddExePagePrintHandler(Service data)
        {
            string exeName = "(none)";
            string windowTitle = "";

            if (data.CapturedExecutableAvailable)
            {
                exeName = data.LastCapturedExecutable.DisplayName;
                windowTitle = data.GetWindowTitle(data.LastCapturedWindowHandle);
            }

            if (windowTitle.Equals(""))
                windowTitle = "(none)";
            else
                windowTitle = ConsoleEx.CutText(windowTitle, Console.WindowWidth - 20);

            Console.SetCursorPosition(0, 6);

            ConsoleEx.WriteKeyValueLine("     Executable", exeName, ConsoleColor.White);
            ConsoleEx.WriteLine();

            ConsoleEx.WriteKeyValueLine("   Window title", windowTitle, ConsoleColor.White);
            ConsoleEx.WriteLine();

            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLineCenter("- Switch to the program, which should be added to the list -", ConsoleColor.Yellow);
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine();
        }

        private void HotKeyEventHandler_CaptureModeOK(HotkeyBarItem item)
        {
            service.AddCapturedExecutable();
            SwitchPage(optionsPage);
        }

        private void HotKeyEventHandler_CaptureModeCancel(HotkeyBarItem item)
        {
            SwitchPage(optionsPage);
            service.SetNormalMode();
        }

        //*** EVENT HANDLERS [END] ***********************************************

    }
}
