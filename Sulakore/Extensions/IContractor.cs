using System.Collections.ObjectModel;

using Sulakore.Habbo;

namespace Sulakore.Extensions
{
    public interface IContractor
    {
        HHotel Hotel { get; }
        HGameData GameData { get; }
        ReadOnlyCollection<IExtension> Extensions { get; }
        ReadOnlyCollection<IExtension> ExtensionsRunning { get; }

        int SendToServer(byte[] data);
        int SendToServer(ushort header, params object[] chunks);

        int SendToClient(byte[] data);
        int SendToClient(ushort header, params object[] chunks);

        void Dispose(IExtension extension);
        object Invoke(object sender, string command, params object[] args);
    }
}