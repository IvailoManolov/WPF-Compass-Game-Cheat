using System;
using System.Runtime.InteropServices;

namespace CompassTarkov
{
    internal static class MouseHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out System.Drawing.Point lpPoint);
    }
}
