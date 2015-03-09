using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Sulakore.Habbo;
using Sulakore.Protocol;
using Sulakore.Components;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class Contractor : IContractor
    {
        public event EventHandler<InvokedEventArgs> Invoked;
        protected virtual object OnInvoked(InvokedEventArgs e)
        {
            EventHandler<InvokedEventArgs> handler = Invoked;
            if (handler != null) handler(this, e);
            return e.Result;
        }

        private readonly IHConnection _connection;
        private readonly IList<IExtension> _installedExtensions;
        private readonly IList<ExtensionBase> _runningExtensions;

        private static readonly string _currentAsmName;
        private const string ExtDirName = "Extensions";

        public HHotel Hotel { get; private set; }
        public HFilters Filters { get; private set; }
        public string PlayerName { get; private set; }
        public HGameData GameData { get; private set; }
        public string FlashClientBuild { get; private set; }

        private readonly ReadOnlyCollection<IExtension> _extensions;
        public ReadOnlyCollection<IExtension> Extensions
        {
            get { return _extensions; }
        }

        static Contractor()
        {
            _currentAsmName = Assembly.GetExecutingAssembly().FullName;
        }
        public Contractor(IHConnection connection, HGameData gameData)
        {
            _connection = connection;
            _installedExtensions = new List<IExtension>();
            _runningExtensions = new List<ExtensionBase>();
            _extensions = new ReadOnlyCollection<IExtension>(_installedExtensions);

            GameData = gameData;
            PlayerName = gameData.PlayerName;

            if (connection != null)
            {
                Filters = connection.Filters;
                Hotel = SKore.ToHotel(connection.Host);
                FlashClientBuild = _connection.FlashClientBuild;
            }
        }

        public int SendToClient(byte[] data)
        {
            return _connection.SendToClient(data);
        }
        public int SendToClient(ushort header, params object[] chunks)
        {
            return _connection.SendToClient(HMessage.Construct(header, chunks));
        }

        public int SendToServer(byte[] data)
        {
            return _connection.SendToServer(data);
        }
        public int SendToServer(ushort header, params object[] chunks)
        {
            return _connection.SendToServer(HMessage.Construct(header, chunks));
        }

        public void ProcessIncoming(byte[] data)
        {
            if (_installedExtensions.Count < 1) return;
            foreach (ExtensionBase extension in _runningExtensions)
            {
                Task.Factory.StartNew(() => extension.DataToClient(data),
                    (TaskCreationOptions)(extension.Priority == ExtensionPriority.Normal ? 0 : 2));
            }
        }
        public void ProcessOutgoing(byte[] data)
        {
            if (_installedExtensions.Count < 1) return;
            foreach (ExtensionBase extension in _runningExtensions)
            {
                Task.Factory.StartNew(() => extension.DataToServer(data),
                    (TaskCreationOptions)(extension.Priority == ExtensionPriority.Normal ? 0 : 2));
            }
        }

        public void Dispose(IExtension extension)
        {
            var ext = (extension as ExtensionBase);
            if (ext != null) ext.Dispose();
            else extension.Invoke(this, "Dispose");

            if (!extension.IsRunning && _runningExtensions.Contains(ext))
                _runningExtensions.Remove(ext);
        }
        public void Initialize(IExtension extension)
        {
            var ext = (extension as ExtensionBase);
            if (ext != null) ext.Initialize();
            else extension.Invoke(this, "Initialize");

            if (extension.IsRunning && !_runningExtensions.Contains(ext))
                _runningExtensions.Add(ext);
        }

        public ExtensionBase Install(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !path.EndsWith(".dll"))
                return null;

            ExtensionBase extension = null;
            if (!Directory.Exists(ExtDirName))
                Directory.CreateDirectory(ExtDirName);

            string extensionPath = path;
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, ExtDirName, Path.GetFileName(path))))
            {
                string extensionId = Guid.NewGuid().ToString();
                string extensionName = Path.GetFileNameWithoutExtension(path);
                extensionPath = Path.Combine(Environment.CurrentDirectory, ExtDirName, string.Format("{0}({1}).dll", extensionName, extensionId));
                File.Copy(path, extensionPath);
            }

            byte[] extensionData = File.ReadAllBytes(extensionPath);
            Assembly extensionAssembly = Assembly.Load(extensionData);

            Type extensionFormType = null;
            Type[] extensionTypes = extensionAssembly.GetTypes();
            foreach (Type extensionType in extensionTypes)
            {
                if (extensionType.IsInterface || extensionType.IsAbstract) continue;
                if (extensionFormType == null && extensionType.BaseType == typeof(SKoreForm))
                {
                    if (extension == null) extensionFormType = extensionType;
                    else extension.UIContextType = extensionFormType = extensionType;
                }

                if (extensionType.BaseType == typeof(ExtensionBase))
                {
                    extension = (ExtensionBase)Activator.CreateInstance(extensionType);
                    extension.Contractor = this;
                    extension.Location = extensionPath;
                    extension.UIContextType = extensionFormType;
                    extension.Version = new Version(FileVersionInfo.GetVersionInfo(extensionPath).FileVersion);
                }
            }

            if (extension != null) _installedExtensions.Add(extension);
            else File.Delete(extensionPath);

            return extension;
        }
        public void Uninstall(IExtension extension)
        {
            if (File.Exists(extension.Location))
                File.Delete(extension.Location);

            Dispose(extension);
            _installedExtensions.Remove(extension);
        }

        public object Invoke(object invoker, string command, params object[] args)
        {
            return OnInvoked(new InvokedEventArgs(invoker, command, args));
        }
    }
}