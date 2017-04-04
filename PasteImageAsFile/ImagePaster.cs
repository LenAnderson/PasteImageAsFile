using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasteImageAsFile
{
    class ImagePaster
    {
        private IntPtr hookId = IntPtr.Zero;
        private bool lCtrl = false; // "LControlKey"
        private bool rCtrl = false; // "RControlKey"


        public ImagePaster()
        {
            hookId = SetHook(HookCallback);
        }

        private void Paste()
        {
            // get the currently active window
            IntPtr curWindow = GetForegroundWindow();
            // check the class to see if it could be a file explorer
            if (GetClassName(curWindow) == "CabinetWClass")
            {
                // find the address bar
                IntPtr addressBar = FindChildWindow(curWindow, new string[] {"WorkerW", "ReBarWindow32", "Address Band Root", "msctls_progress32", "Breadcrumb Parent", "ToolbarWindow32"});
                if (addressBar != IntPtr.Zero)
                {
                    Regex reg = new Regex("^[^:]+:\\s+");
                    string path = reg.Replace(GetWindowText(addressBar), "");
                    if (path != "")
                    {
                        // check the clipboard
                        if (Clipboard.ContainsImage())
                        {
                            Clipboard.GetImage().Save(path + @"\Clipboard Data " + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".gif", ImageFormat.Gif);
                        }
                    }
                }
            }
        }



        #region: window handles
        private string GetClassName(IntPtr hwnd)
        {
            StringBuilder builder = new StringBuilder(256);
            if (GetClassName(hwnd, builder, builder.Capacity) != 0)
            {
                return builder.ToString();
            }
            return "";
        }

        private IntPtr FindChildWindow(IntPtr hwnd, string className)
        {
            return FindWindowEx(hwnd, IntPtr.Zero, className, null);
        }
        private IntPtr FindChildWindow(IntPtr hwnd, string[] classNames)
        {
            IntPtr cur = hwnd;
            int idx = 0;
            while (cur != IntPtr.Zero && idx < classNames.Count())
            {
                cur = FindChildWindow(cur, classNames[idx]);
                idx++;
            }
            return cur;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                StringBuilder builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }
            return String.Empty;
        }
        #endregion



        #region: key hook
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProc = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProc.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    string key = ((Keys)Marshal.ReadInt32(lParam)).ToString();
                    if (key == "LControlKey")
                    {
                        lCtrl = true;
                    }
                    else if (key == "RControlKe")
                    {
                        rCtrl = true;
                    } else if (key == "V" && (lCtrl || rCtrl))
                    {
                        Paste();
                    }
                } else if (wParam == (IntPtr)WM_KEYUP)
                {
                    string key = ((Keys)Marshal.ReadInt32(lParam)).ToString();
                    if (key == "LControlKey")
                    {
                        lCtrl = false;
                    }
                    else if (key == "RControlKe")
                    {
                        rCtrl = false;
                    }
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion




        #region: constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        #endregion



        #region: imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        // get active window
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        // get window class
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // find child window
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        // get window caption / title
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        // get length of window caption / title
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        #endregion
    }
}
