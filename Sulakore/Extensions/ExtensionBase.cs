using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

using Sulakore.Protocol;
using Sulakore.Components;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public abstract class ExtensionBase : HTriggers, IExtension
    {
        /// <summary>
        /// Gets a value indicating whether the extension is running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets the name of the extension given by the Author.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the name(s) of the developer(s) that wrote the extension.
        /// </summary>
        public abstract string Author { get; }
        /// <summary>
        /// Gets the file location of the extension from which the instance was initialized from.
        /// </summary>
        public string Location { get; internal set; }

        /// <summary>
        /// Gets the logo of the extension.
        /// </summary>
        public Bitmap Logo { get; protected set; }
        /// <summary>
        /// Gets the assembly version of the extension.
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// Gets the IContractor instance used for communication between the initializer.
        /// </summary>
        public IContractor Contractor { get; internal set; }
        /// <summary>
        /// Gets the priorty of the extension that determines whether a new thread is spawned when handling the flow of data(High), or whether to pull one from the system's thread pool(Normal).
        /// </summary>
        public ExtensionPriority Priority { get; protected set; }

        /// <summary>
        /// Gets the Type found in the extension's project scope that inherits from SKoreExtensionForm.
        /// </summary>
        public Type UIContextType { get; internal set; }
        /// <summary>
        /// Gets the Form that represents the extension's main GUI.
        /// </summary>
        public SKoreExtensionForm UIContext { get; internal set; }

        protected override void Dispose(bool disposing)
        {
            IsRunning = false;
            base.Dispose(disposing);

            OnDisposed();
            if (UIContext != null)
            {
                UIContext.FormClosed -= UIContext_FormClosed;
                UIContext.Close();
                UIContext = null;
            }
        }
        protected abstract void OnDisposed();

        internal void Initialize()
        {
            if (UIContext != null) { UIContext.BringToFront(); return; }
            else if (UIContextType != null && UIContextType.BaseType == typeof(SKoreExtensionForm))
            {
                UIContext = (SKoreExtensionForm)Activator.CreateInstance(UIContextType, this);

                if (UIContext.Extension == null)
                    UIContext.Extension = this;

                UIContext.FormClosed += UIContext_FormClosed;
                UIContext.Show();
            }

            if (!IsRunning)
            {
                IsRunning = true;
                OnInitialized();
            }
        }
        protected abstract void OnInitialized();

        /// <summary>
        /// Attempts to processes a custom command with arguments.
        /// </summary>
        /// <param name="invoker">The source of the method call.</param>
        /// <param name="command">The command for the invokee to utilize.</param>
        /// <param name="args">The arguments given to be used by the invokee to process the command.</param>
        /// <returns></returns>
        public object Invoke(object invoker, string command, params object[] args)
        {
            if (invoker == this) throw new Exception("DENIED: Self-invocation not allowed, you also probably didn't mean to anyways.");
            return OnInvoked(invoker, command, args);
        }
        protected abstract object OnInvoked(object invoker, string command, params object[] args);

        internal void DataToClient(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Client);
            base.ProcessIncoming(packet);

            OnDataToClient(packet);
        }
        protected abstract void OnDataToClient(HMessage packet);

        internal void DataToServer(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Server);
            base.ProcessOutgoing(packet);

            OnDataToServer(packet);
        }
        protected abstract void OnDataToServer(HMessage packet);

        private void UIContext_FormClosed(object sender, FormClosedEventArgs e)
        {
            UIContext.FormClosed -= UIContext_FormClosed;
            UIContext = null;

            Contractor.Dispose(this);
        }
    }
}