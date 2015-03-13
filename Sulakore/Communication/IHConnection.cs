using System;
using Sulakore.Protocol;
using Sulakore.Protocol.Encryption;

namespace Sulakore.Communication
{
    public interface IHConnection
    {
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<DataToEventArgs> DataToClient;
        event EventHandler<DataToEventArgs> DataToServer;

        int Port { get; }
        string Host { get; }
        HFilters Filters { get; }
        string[] Addresses { get; }

        Rc4 IncomingDecrypt { get; set; }
        Rc4 IncomingEncrypt { get; set; }

        Rc4 OutgoingEncrypt { get; set; }
        Rc4 OutgoingDecrypt { get; set; }

        bool IsConnected { get; }
        bool IsOutgoingEncrypted { get; }
        bool IsIncomingEncrypted { get; }

        int SendToServer(byte[] data);
        int SendToServer(ushort header, params object[] chunks);

        int SendToClient(byte[] data);
        int SendToClient(ushort header, params object[] chunks);
    }
}