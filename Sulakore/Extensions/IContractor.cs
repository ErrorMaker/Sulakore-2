using System.Collections.ObjectModel;

using Sulakore.Habbo;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public interface IContractor
    {
        HHotel Hotel { get; }
        HFilters Filters { get; }
        string PlayerName { get; }
        HGameData GameData { get; }
        string FlashClientBuild { get; }
        ReadOnlyCollection<IExtension> Extensions { get; }

        int SendToClient(byte[] data);
        int SendToClient(ushort header, params object[] chunks);

        int SendToServer(byte[] data);
        int SendToServer(ushort header, params object[] chunks);

        void Dispose(IExtension extension);
        object Invoke(object sender, string command, params object[] args);
    }
}