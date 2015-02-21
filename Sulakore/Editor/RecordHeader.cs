using Sulakore.Editor.Tags;

namespace Sulakore.Editor
{
    public struct RecordHeader
    {
        private readonly ushort _header;
        public ushort Header
        {
            get { return _header; }
        }

        private readonly FlashTagType _tag;
        public FlashTagType Tag
        {
            get { return _tag; }
        }

        private readonly uint _length;
        public uint Length
        {
            get { return _length; }
        }

        private readonly bool _isLong;
        public bool IsLong
        {
            get { return _isLong; }
        }

        private readonly int _headerStart;
        public int HeaderStart
        {
            get { return _headerStart; }
        }

        private readonly int _headerEnd;
        public int HeaderEnd
        {
            get { return _headerEnd; }
        }

        public RecordHeader(ShockwaveFlash flash)
        {
            _headerStart = flash.Position;
            _header = flash.ReadUI16();
            _tag = (FlashTagType)(_header >> 6);
            _length = (uint)(_header & 0x3f);

            if (_length >= 63)
                _length = flash.ReadUI32();

            _headerEnd = flash.Position;
            _isLong = (_length >= 0x3f);
        }
    }
}