﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPlugin.Utils
{
    internal class Keyboard
    {
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int KEY_PRESSED = 0x8000;
        private const int KEY_TOGGLED = 0x0001;

        private const int CLICK_DELAY = 10; // ✅ Define constant directly to avoid dependency

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        public static void KeyDown(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void KeyUp(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static async Task KeyPressAsync(Keys key, int delay = CLICK_DELAY)
        {
            KeyDown(key);
            await Task.Delay(delay); // ✅ Allows custom delay for better control
            KeyUp(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return (GetKeyState((int)key) & KEY_PRESSED) != 0;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return (GetKeyState((int)key) & KEY_PRESSED) != 0;
        }

        public static bool IsKeyToggled(Keys key)
        {
            return (GetKeyState((int)key) & KEY_TOGGLED) != 0;
        }
    }
}