using System;

namespace Sulakore.Extensions
{
    public class InvokedEventArgs : EventArgs
    {
        public object Result { get; set; }

        private readonly object[] _args;
        public object[] Args
        {
            get { return _args; }
        }

        private readonly string _command;
        public string Command
        {
            get { return _command; }
        }

        private readonly object _invoker;
        public object Invoker
        {
            get { return _invoker; }
        }

        public InvokedEventArgs(object invoker, string command, params object[] args)
        {
            _args = args;
            _command = command;
            _invoker = invoker;
        }
    }
}