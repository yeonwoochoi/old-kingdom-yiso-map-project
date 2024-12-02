using System;
using UnityEngine;
using Random = System.Random;

namespace Utils.ObjectId {
    public class YisoObjectID : IComparable<YisoObjectID>, IConvertible {
        private static readonly YisoObjectID emptyInstance = default;
        private static readonly long random = CalculateRandomValue();
        private static int staticIncrement = (new Random()).Next();

        private readonly int a;
        private readonly int b;
        private readonly int c;

        private YisoObjectID(byte[] bytes) {
            if (bytes == null) {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length != 12) {
                throw new ArgumentException("Byte array must be 12 bytes long", "bytes");
            }

            FromByteArray(bytes, 0, out a, out b, out c);
        }

        internal YisoObjectID(byte[] bytes, int index) {
            FromByteArray(bytes, index, out a, out b, out c);
        }

        public YisoObjectID(string value) {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var bytes = YisoObjectIDUtils.ParseHexString(value);
            FromByteArray(bytes, 0, out a, out b, out c);
        }

        private YisoObjectID(int a, int b, int c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public static YisoObjectID Empty => emptyInstance;

        public int Timestamp => a;

        public DateTime CreationTime => YisoObjectIDUtils.UnixEpoch.AddSeconds((uint) Timestamp);

        public static bool operator <(YisoObjectID lhs, YisoObjectID rhs) => lhs.CompareTo(rhs) < 0;

        public static bool operator <=(YisoObjectID lhs, YisoObjectID rhs) => lhs.CompareTo(rhs) <= 0;

        public static bool operator ==(YisoObjectID lhs, YisoObjectID rhs) => lhs.Equals(rhs);

        public static bool operator !=(YisoObjectID lhs, YisoObjectID rhs) => !(lhs == rhs);

        public static bool operator >=(YisoObjectID lhs, YisoObjectID rhs) => lhs.CompareTo(rhs) >= 0;

        public static bool operator >(YisoObjectID lhs, YisoObjectID rhs) => lhs.CompareTo(rhs) > 0;

        public static YisoObjectID Generate() => Generate(DateTime.UtcNow);

        public static YisoObjectID Generate(DateTime timestamp) => Generate(GetTimestampFromDateTime(timestamp));

        public static string GenerateString() => Generate().ToString();

        public static YisoObjectID Generate(int timestamp) {
            var inc = (staticIncrement++) & 0x00ffffff;
            return Create(timestamp, random, inc);
        }

        public static YisoObjectID Parse(string s) {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (TryParse(s, out var objectID)) return objectID;
            else {
                var message = $"'{s}' is not a valid 24 digit hex string.";
                throw new FormatException(message);
            }
        }

        public static bool TryParse(string s, out YisoObjectID objectID) {
            if (s != null && s.Length == 24) {
                byte[] bytes;
                if (YisoObjectIDUtils.TryParseHexString(s, out bytes)) {
                    objectID = new YisoObjectID(bytes);
                    return true;
                }
            }

            objectID = default(YisoObjectID);
            return false;
        }

        public int CompareTo(YisoObjectID other) {
            var result = ((uint) a).CompareTo((uint) other.a);
            if (result != 0) return result;
            result = ((uint) b).CompareTo((uint) other.b);
            if (result != 0) return result;
            return ((uint) c).CompareTo((uint) other.c);
        }

        public bool Equals(YisoObjectID rhs) {
            return a == rhs.a && b == rhs.b && c == rhs.c;
        }

        public override bool Equals(object obj) {
            return obj is YisoObjectID id && Equals(id);
        }

        public override int GetHashCode() {
            var hash = 17;
            hash = 37 * hash + a.GetHashCode();
            hash = 37 * hash + b.GetHashCode();
            hash = 37 * hash + c.GetHashCode();
            return hash;
        }

        public TypeCode GetTypeCode() => TypeCode.Object;

        public override string ToString() {
            var result = new char[24];
            result[0] = YisoObjectIDUtils.ToHexChar((a >> 28) & 0x0f);
            result[1] = YisoObjectIDUtils.ToHexChar((a >> 24) & 0x0f);
            result[2] = YisoObjectIDUtils.ToHexChar((a >> 20) & 0x0f);
            result[3] = YisoObjectIDUtils.ToHexChar((a >> 16) & 0x0f);
            result[4] = YisoObjectIDUtils.ToHexChar((a >> 12) & 0x0f);
            result[5] = YisoObjectIDUtils.ToHexChar((a >> 8) & 0x0f);
            result[6] = YisoObjectIDUtils.ToHexChar((a >> 4) & 0x0f);
            result[7] = YisoObjectIDUtils.ToHexChar(a & 0x0f);
            result[8] = YisoObjectIDUtils.ToHexChar((b >> 28) & 0x0f);
            result[9] = YisoObjectIDUtils.ToHexChar((b >> 24) & 0x0f);
            result[10] = YisoObjectIDUtils.ToHexChar((b >> 20) & 0x0f);
            result[11] = YisoObjectIDUtils.ToHexChar((b >> 16) & 0x0f);
            result[12] = YisoObjectIDUtils.ToHexChar((b >> 12) & 0x0f);
            result[13] = YisoObjectIDUtils.ToHexChar((b >> 8) & 0x0f);
            result[14] = YisoObjectIDUtils.ToHexChar((b >> 4) & 0x0f);
            result[15] = YisoObjectIDUtils.ToHexChar(b & 0x0f);
            result[16] = YisoObjectIDUtils.ToHexChar((c >> 28) & 0x0f);
            result[17] = YisoObjectIDUtils.ToHexChar((c >> 24) & 0x0f);
            result[18] = YisoObjectIDUtils.ToHexChar((c >> 20) & 0x0f);
            result[19] = YisoObjectIDUtils.ToHexChar((c >> 16) & 0x0f);
            result[20] = YisoObjectIDUtils.ToHexChar((c >> 12) & 0x0f);
            result[21] = YisoObjectIDUtils.ToHexChar((c >> 8) & 0x0f);
            result[22] = YisoObjectIDUtils.ToHexChar((c >> 4) & 0x0f);
            result[23] = YisoObjectIDUtils.ToHexChar(c & 0x0f);
            return new string(result);
        }

        public bool ToBoolean(IFormatProvider provider) => false;

        public byte ToByte(IFormatProvider provider) => 0;

        public char ToChar(IFormatProvider provider) => (char) 0;

        public DateTime ToDateTime(IFormatProvider provider) => DateTime.Now;

        public decimal ToDecimal(IFormatProvider provider) => 0;

        public double ToDouble(IFormatProvider provider) => 0;

        public short ToInt16(IFormatProvider provider) => 0;

        public int ToInt32(IFormatProvider provider) => 0;

        public long ToInt64(IFormatProvider provider) => 0;

        public sbyte ToSByte(IFormatProvider provider) => 0;

        public float ToSingle(IFormatProvider provider) => 0;

        public string ToString(IFormatProvider provider) => string.Empty;

        public object ToType(Type conversionType, IFormatProvider provider) {
            switch (Type.GetTypeCode(conversionType)) {
                case TypeCode.String:
                    return ((IConvertible) this).ToString(provider);
                case TypeCode.Object:
                    if (conversionType == typeof(object) || conversionType == typeof(YisoObjectID)) return this;
                    break;
            }

            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider) => 0;

        public uint ToUInt32(IFormatProvider provider) => 0;

        public ulong ToUInt64(IFormatProvider provider) => 0;

        private static int GetTimestampFromDateTime(DateTime timestamp) {
            var secondsSinceEpoch =
                (long) Math.Floor((YisoObjectIDUtils.ToUniversalTime(timestamp) - YisoObjectIDUtils.UnixEpoch)
                    .TotalSeconds);
            if (secondsSinceEpoch < uint.MinValue || secondsSinceEpoch > uint.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(timestamp));
            }

            return (int)(uint)secondsSinceEpoch;
        }

        private static long CalculateRandomValue() {
            var seed = (int) DateTime.UtcNow.Ticks ^ GetMachineHash();
            var r = new Random(seed);
            var high = r.Next();
            var low = r.Next();
            var combined = (long) ((ulong) (uint) high << 32 | (ulong) (uint) low);
            return combined & 0xffffffffff;
        }

        private static int GetMachineHash() {
            var machineName = GetMachineName();
            return 0x00ffffff & machineName.GetHashCode();
        }

        private static string GetMachineName() {
            return SystemInfo.deviceName;
        }

        private static void FromByteArray(byte[] bytes, int offset, out int a, out int b, out int c) {
            a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
            c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
        }

        private static YisoObjectID Create(int timestamp, long random, int increment) {
            if (random < 0 || random > 0xffffffffff)
            {
                throw new ArgumentOutOfRangeException(nameof(random), "The random value must be between 0 and 1099511627775 (it must fit in 5 bytes).");
            }
            if (increment < 0 || increment > 0xffffff)
            {
                throw new ArgumentOutOfRangeException(nameof(increment), "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            var a = timestamp;
            var b = (int)(random >> 8);
            var c = (int)(random << 24) | increment;
            return new YisoObjectID(a, b, c);
        }
    }
}