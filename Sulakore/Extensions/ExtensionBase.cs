using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

using Sulakore.Protocol;
using Sulakore.Components;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public abstract class ExtensionBase : IExtension
    {
        /// <summary>
        /// Gets the name of the extension given by the Author.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the name(s) of the developer(s) that worked on the extension.
        /// </summary>
        public abstract string Author { get; }
        /// <summary>
        /// Gets a value indicating whether the extension is running.
        /// </summary>
        public bool IsRunning { get; internal set; }
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
        /// Gets the HTriggers instance associated with the extension.
        /// </summary>
        public HTriggers Triggers { get; protected internal set; }

        /// <summary>
        /// Gets or sets the priorty of the extension that determines whether a new thread is spawned when handling the flow of data(High), or whether to pull one from the system's thread pool(Normal).
        /// </summary>
        public ExtensionPriority Priority { get; set; }
        /// <summary>
        /// Gets the IContractor instance used for communication between the initializer.
        /// </summary>
        public IContractor Contractor { get; internal set; }

        /// <summary>
        /// Gets the Type found in the extension's project scope that inherits from SKoreExtensionForm.
        /// </summary>
        public Type UIContextType { get; internal set; }
        /// <summary>
        /// Gets the Form that represents the extension's main GUI.
        /// </summary>
        public SKoreForm UIContext { get; internal set; }

        public void Dispose()
        {
            IsRunning = false;
            if (UIContext != null)
            {
                UIContext.FormClosed -= UIContext_FormClosed;
                UIContext.Close();
                UIContext = null;
            }
            Triggers.Dispose();
            OnDisposed();
        }
        protected abstract void OnDisposed();

        void IExtension.Initialize()
        {
            if (UIContext != null) { UIContext.BringToFront(); return; }
            else if (UIContextType != null && UIContextType.BaseType == typeof(SKoreForm))
            {
                UIContext = (SKoreForm)Activator.CreateInstance(UIContextType, this);

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
        protected virtual object OnInvoked(object invoker, string command, params object[] args)
        {
            return null;
        }

        void IExtension.DataToClient(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Client);
            Triggers.ProcessIncoming(packet);

            OnDataToClient(packet);
        }
        protected abstract void OnDataToClient(HMessage packet);

        void IExtension.DataToServer(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Server);
            Triggers.ProcessOutgoing(packet);

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