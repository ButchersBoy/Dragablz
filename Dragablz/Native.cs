using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Dragablz
{
    internal static class Native
    {
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

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }   

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPos()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        public static IEnumerable<Window> SortWindowsTopToBottom(IEnumerable<Window> windows)
        {
            var windowsByHandle = windows.Select(window =>
            {
                var hwndSource = PresentationSource.FromVisual(window) as HwndSource;
                var handle = hwndSource != null ? hwndSource.Handle : IntPtr.Zero;
                return new {window, handle};
            }).Where(x => x.handle != IntPtr.Zero)
                .ToDictionary(x => x.handle, x => x.window);

            for (var hWnd = GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = GetWindow(hWnd, GW_HWNDNEXT))
                if (windowsByHandle.ContainsKey((hWnd)))
                    yield return windowsByHandle[hWnd];
        }

        public const int SW_SHOWNORMAL = 1;
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        private const uint GW_HWNDNEXT = 2;
        [DllImport("User32")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("User32")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT minPosition;
            public POINT maxPosition;
            public RECT normalPosition;
        }
    }
}
