using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace PunchClockIn.ViewModels;


public static class SingletonApp
{
    public static bool IsRunning(string key)
    {
        var k = $"{key}_{Environment.UserName}{(Debugger.IsAttached ? "_Debug" : "")}";

        mutex = new Mutex(true, k);

        return mutex.WaitOne(TimeSpan.Zero, true);
    }

    public static void SendShowCommand()
    {
        NativeMethods.PostMessage(
            (IntPtr)NativeMethods.HWND_BROADCAST,
            NativeMethods.WM_SHOWME,
            IntPtr.Zero,
            IntPtr.Zero);
    }

    private static Dictionary<IntPtr, Window> windowMap = new Dictionary<IntPtr, Window>();
    private static Mutex mutex;

    public static void HandleShow(Window window)
    {
        var handler = new EventHandler((s, e) => HandleShow(window));
        if (!window.IsActive)
        {
            window.Activated += handler;
            return;
        }

        window.Activated -= handler;
        HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
        if (windowMap.ContainsKey(source.Handle)) return;

        windowMap[source.Handle] = window;
        source.AddHook(WndProc);
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == NativeMethods.WM_SHOWME)
        {
            windowMap.TryGetValue(hwnd, out var window);
            window?.Activate();
            window?.Show();
        }

        return IntPtr.Zero;
    }

    public static void Exit()
    {
        mutex?.ReleaseMutex();
    }

    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}