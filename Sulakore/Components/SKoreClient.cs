using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Sulakore.Habbo;

namespace Sulakore.Components
{
    public class SKoreClient : WebBrowser
    {
        private IntPtr _ieHandle;
        private IntPtr IEHandle
        {
            get
            {
                if (_ieHandle != IntPtr.Zero) return _ieHandle;

                var bcName = new StringBuilder(100);
                IntPtr handle = Handle;
                do NativeMethods.GetClassName((handle = NativeMethods.GetWindow(handle, 5)), bcName, bcName.MaxCapacity);
                while (bcName.ToString() != "Internet Explorer_Server");

                return (_ieHandle = handle);
            }
        }

        public SKoreClient()
        {
            ScriptErrorsSuppressed = true;
            ScrollBarsEnabled = false;
        }

        new public void Enter()
        {
            NativeMethods.PostMessage(IEHandle, 256u, 13, IntPtr.Zero);
        }
        new public void Click(int x, int y)
        {
            NativeMethods.SendMessage(IEHandle, 0x201, IntPtr.Zero, (IntPtr)((y << 16) | x));
            NativeMethods.SendMessage(IEHandle, 0x202, IntPtr.Zero, (IntPtr)((y << 16) | x));
        }
        new public void Click(Point coordinate)
        {
            Click(coordinate.X, coordinate.Y);
        }

        public void Say(string message)
        {
            Speak(message, false);
        }
        public void Shout(string message)
        {
            Speak(message, true);
        }
        public void Speak(string message, bool shout)
        {
            if (string.IsNullOrEmpty(message)) return;

            Enter();
            if (shout) message = ":shout " + message;
            foreach (char c in message) NativeMethods.PostMessage(IEHandle, 0x102, c, IntPtr.Zero);
            Enter();
        }

        public void Sign(HSign sign)
        {
            Say(":sign " + sign.Juice());
        }
        public void Stance(HStance stance)
        {
            Say(":" + stance.ToString());
        }
        public void Gesture(HGesture gesture)
        {
            switch (gesture)
            {
                case HGesture.Wave: Say("o/"); break;
                case HGesture.Idle: Say(":idle"); break;
                case HGesture.ThumbsUp: Say("_b"); break;
                case HGesture.BlowKiss: Say(":kiss"); break;
                case HGesture.Laugh: Say(":whisper  :D"); break;
            }
        }
    }
}