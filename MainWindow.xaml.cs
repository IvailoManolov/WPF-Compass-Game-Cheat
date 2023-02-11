using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace CompassTarkov
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private System.Timers.Timer timer;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelMouseProc _mouseHookDelegate;
        private IntPtr _mouseHookHandle;

        private const int WH_MOUSE_LL = 14;

        public MainWindow()
        {
            InitializeComponent();

            _mouseHookDelegate = MouseHookProc;
            _mouseHookHandle = SetHook(_mouseHookDelegate);

            //timer = new System.Timers.Timer();
            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            //timer.Start();
        }

        //private void timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    System.Drawing.Point p;
        //    if (MouseHelper.GetCursorPos(out p))
        //    {
        //        Canvas_MouseMove(sender, p);
        //    }
        //}

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void Canvas_MouseMove(MSLLHOOKSTRUCT hookStruct)
        {
            var mousePos = hookStruct;
            double angle = Math.Atan2(mousePos.pt.y - 100, mousePos.pt.x - 100) * 360 / Math.PI;
            var angleInt = Convert.ToInt32(angle);
            if (angleInt < 0) { angleInt *= -1; }
            var stringAngle = angleInt.ToString();

            Dispatcher.Invoke(() =>
            {
                DegreesTextBlock.Text = stringAngle;

                degreeIndicator.X2 = 100 + 50 * Math.Cos(angle * Math.PI / 180);
                degreeIndicator.Y2 = 100 - 50 * Math.Sin(angle * Math.PI / 180);
            });
        }
        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int x = hookStruct.pt.x;
                int y = hookStruct.pt.y;

                Canvas_MouseMove(hookStruct);
            }
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }


    }
}
