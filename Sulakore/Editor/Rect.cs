namespace Sulakore.Editor
{
    public struct Rect
    {
        private readonly byte[] _data;

        private readonly int _x;
        public int X
        {
            get { return _x; }
        }

        private readonly int _y;
        public int Y
        {
            get { return _y; }
        }

        private readonly int _twipsWidth;
        public int TwipsWidth
        {
            get { return _twipsWidth; }
        }

        private readonly int _width;
        public int Width
        {
            get { return _width; }
        }

        private readonly int _twipsHeight;
        public int TwipsHeight
        {
            get { return _twipsHeight; }
        }

        private readonly int _height;
        public int Height
        {
            get { return _height; }
        }

        public Rect(ShockwaveFlash flash)
        {
            int pos = flash.Position;
            int nBits = (int)flash.ReadUB(5);

            _x = flash.ReadSB(nBits);
            _twipsWidth = flash.ReadSB(nBits);
            _width = _twipsWidth / 20;

            _y = flash.ReadSB(nBits);
            _twipsHeight = flash.ReadSB(nBits);
            _height = _twipsHeight / 20;

            _data = flash.ReadUI8Block(flash.Position - pos, pos);
        }

        public byte[] ToBytes()
        {
            return _data;
        }
    }
}