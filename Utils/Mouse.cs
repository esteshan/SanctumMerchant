using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MyPlugin.Utils
{
    internal class Mouse
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public const int MOUSEEVENTF_MIDDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDUP = 0x0040;

        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSE_EVENT_WHEEL = 0x800;

        private const int MOVEMENT_DELAY = 5;  // ✅ Reduced delay for faster movement
        private const int CLICK_DELAY = 1;

        public static async Task<bool> SetCursorPosAsync(int x, int y, RectangleF gameWindow)
        {
            bool result = SetCursorPos(x + (int)gameWindow.X, y + (int)gameWindow.Y);
            await Task.Delay(1); // ✅ Prevents UI lag
            return result;
        }

        public static async Task<bool> SetCursorPosToCenterOfRecAsync(RectangleF position, RectangleF gameWindow)
        {
            bool result = SetCursorPos(
                (int)(gameWindow.X + position.X + position.Width / 2),
                (int)(gameWindow.Y + position.Y + position.Height / 2)
            );
            await Task.Delay(1); // ✅ Prevents UI lag
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            GetCursorPos(out POINT lpPoint);
            return lpPoint;
        }

        public static void LeftMouseDown() => mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        public static void LeftMouseUp() => mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        public static void RightMouseDown() => mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        public static void RightMouseUp() => mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);

        public static async Task SetCursorPosAndLeftClickAsync(Vector2 pos, int extraDelay, Vector2 offset)
        {
            await SetCursorPosAsync((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), new RectangleF());
            await Task.Delay(MOVEMENT_DELAY + extraDelay);
            await LeftClickAsync();
        }

        public static async Task SetCursorPosAndRightClickAsync(Vector2 pos, int extraDelay, Vector2 offset)
        {
            await SetCursorPosAsync((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), new RectangleF());
            await Task.Delay(MOVEMENT_DELAY + extraDelay);
            await RightClickAsync();
        }

        public static async Task VerticalScrollAsync(bool forward, int clicks)
        {
            int scrollAmount = clicks * 120;
            mouse_event(MOUSE_EVENT_WHEEL, 0, 0, forward ? scrollAmount : -scrollAmount, 0);
            await Task.Delay(10); // ✅ Reduced delay for faster responsiveness
        }

        public static async Task LeftClickAsync()
        {
            LeftMouseDown();
            await Task.Delay(CLICK_DELAY);
            LeftMouseUp();
        }

        public static async Task RightClickAsync()
        {
            RightMouseDown();
            await Task.Delay(CLICK_DELAY);
            RightMouseUp();
        }
    }
}
