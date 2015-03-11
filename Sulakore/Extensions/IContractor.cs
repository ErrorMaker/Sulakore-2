using System.Collections.ObjectModel;

using Sulakore.Habbo;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public interface IContractor
    {
        HHotel Hotel { get; }
        HGameData GameData { get; }
        IHConnection Connection { get; }
        ReadOnlyCollection<IExtension> Extensions { get; }
        ReadOnlyCollection<IExtension> ExtensionsRunning { get; }

        void Dispose(IExtension extension);
        object Invoke(object sender, string command, params object[] args);
    }
}