using System.Drawing;

namespace Sulakore.Habbo
{
    public struct HPoint
    {
        private readonly Point _point;
        public static readonly HPoint Empty;

        public bool IsEmpty
        {
            get { return Empty.Equals(this); }
        }

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

        private readonly string _z;
        public string Z
        {
            get { return string.IsNullOrEmpty(_z) ? "0.0" : _z; }
        }

        public static bool operator ==(HPoint x, HPoint y)
        {
            return Equals(x, y);
        }
        public static bool operator !=(HPoint x, HPoint y)
        {
            return !(x == y);
        }

        static HPoint()
        {
            Empty = new HPoint(0, 0, "0.0");
        }
        public HPoint(int x, int y)
            : this(x, y, "0.0")
        { }
        public HPoint(int x, int y, string z)
        {
            _x = x;
            _y = y;
            _z = z;
            _point = new Point(x, y);
        }

        public Point ToPoint()
        {
            return _point;
        }
        public bool Equals(HPoint other)
        {
            return _point.Equals(other._point)
                && int.Equals(_x, other._x)
                && int.Equals(_y, other._y)
                && string.Equals(_z, other._z);
        }

        public override string ToString()
        {
            return string.Format("{{X={0},Y={1},Z={2}}}", X, Y, Z);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _point.GetHashCode();
                hashCode = (hashCode * 397) ^ _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ (_z != null ? _z.GetHashCode() : 0);
                return hashCode;
            }
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HPoint && Equals((HPoint)obj);
        }
    }
}