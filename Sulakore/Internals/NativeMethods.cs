using System;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Sulakore
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, int wparam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private static bool settingsReturn, refreshReturn;

        private static readonly RegistryKey ProxyRegistry;

        private const string HTTPSProxyServerFormat = "https=127.0.0.1:{0}";

        static NativeMethods()
        {
            ProxyRegistry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        }

        public static void DisableProxy()
        {
            if (ProxyRegistry.GetValue("ProxyServer") != null)
                ProxyRegistry.DeleteValue("ProxyServer");

            ProxyRegistry.SetValue("ProxyEnable", 0);
            ProxyRegistry.SetValue("ProxyOverride", "<-loopback>");
            RefreshIESettings();
        }
        public static void EnableProxy(int httpPort)
        {
            const string singlePort = "http=127.0.0.1:{0}";
            EnableProxy(string.Format(singlePort, httpPort));
        }
        public static void EnableProxy(int httpPort, int httpsPort)
        {
            const string multiplePorts = "http=127.0.0.1:{0};https=127.0.0.1:{1}";
            EnableProxy(string.Format(multiplePorts, httpPort, httpsPort));
        }

        private static void EnableProxy(string proxyServerSettings)
        {
            ProxyRegistry.SetValue("ProxyServer", proxyServerSettings);

            ProxyRegistry.SetValue("ProxyEnable", 1);
            ProxyRegistry.SetValue("ProxyOverride", "<-loopback>;<local>");
            RefreshIESettings();
        }
        private static void RefreshIESettings()
        {
            settingsReturn = InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
        }
    }
}