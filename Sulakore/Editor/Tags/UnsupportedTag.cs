namespace Sulakore.Editor.Tags
{
    public class UnsupportedTag : IFlashTag
    {
        private readonly bool _forceLong;
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public RecordHeader Header { get; private set; }

        public byte[] Data { get; private set; }

        public UnsupportedTag(ShockwaveFlash flash, RecordHeader header, bool forceLong = false)
        {
            _flash = flash;
            _forceLong = forceLong;

            Header = header;
            Position = _flash.Position;

            Data = _flash.ReadUI8Block((int)header.Length);
        }

        public byte[] ToBytes()
        {
            return ShockwaveFlash.ConstructTagData(Header.Tag, Data, _forceLong);
        }
    }
}