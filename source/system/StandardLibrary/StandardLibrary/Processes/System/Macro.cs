using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StandardLibrary.Processes.System
{
    public static class Macro
    {
        [DllImport(
            "user32.dll", 
            CharSet = CharSet.Auto, 
            CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(
            uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        // Mouse actions.
        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void LeftMouseClick()
        {
            var x = (uint)Cursor.Position.X;
            var y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }
}