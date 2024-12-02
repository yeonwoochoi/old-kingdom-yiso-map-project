using System;

namespace Utils.ObjectId {
    public static class YisoObjectIDUtils {
        private static readonly DateTime unixEpoch;

        static YisoObjectIDUtils() {
            unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static byte[] ParseHexString(string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }

            byte[] bytes;
            if (!TryParseHexString(s, out bytes)) {
                throw new FormatException("String should contain only hexadecimal digits.");
            }

            return bytes;
        }

        public static char ToHexChar(int value) {
            return (char) (value + (value < 10 ? '0' : 'a' - 10));
        }

        public static bool TryParseHexString(string s, out byte[] bytes) {
            bytes = null;

            if (s == null) {
                return false;
            }

            var buffer = new byte[(s.Length + 1) / 2];

            var i = 0;
            var j = 0;

            if ((s.Length % 2) == 1) {
                // if s has an odd length assume an implied leading "0"
                int y;
                if (!TryParseHexChar(s[i++], out y)) {
                    return false;
                }

                buffer[j++] = (byte) y;
            }

            while (i < s.Length) {
                int x, y;
                if (!TryParseHexChar(s[i++], out x)) {
                    return false;
                }

                if (!TryParseHexChar(s[i++], out y)) {
                    return false;
                }

                buffer[j++] = (byte) ((x << 4) | y);
            }

            bytes = buffer;
            return true;
        }

        private static bool TryParseHexChar(char c, out int value) {
            if (c >= '0' && c <= '9') {
                value = c - '0';
                return true;
            }

            if (c >= 'a' && c <= 'f') {
                value = 10 + (c - 'a');
                return true;
            }

            if (c >= 'A' && c <= 'F') {
                value = 10 + (c - 'A');
                return true;
            }

            value = 0;
            return false;
        }

        public static DateTime UnixEpoch => unixEpoch;

        public static DateTime ToLocalTime(DateTime dateTime) {
            if (dateTime == DateTime.MinValue) {
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local);
            }

            if (dateTime == DateTime.MaxValue) {
                return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Local);
            }

            return dateTime.ToLocalTime();
        }

        public static long ToMillisecondsSinceEpoch(DateTime dateTime) {
            var utcDateTime = ToUniversalTime(dateTime);
            return (utcDateTime - UnixEpoch).Ticks / 10000;
        }

        public static long ToSecondsSinceEpoch(DateTime dateTime) {
            var utcDateTime = ToUniversalTime(dateTime);
            return (utcDateTime - UnixEpoch).Ticks / TimeSpan.TicksPerSecond;
        }

        public static DateTime ToUniversalTime(DateTime dateTime) {
            if (dateTime == DateTime.MinValue) {
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }

            if (dateTime == DateTime.MaxValue) {
                return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
            }

            return dateTime.ToUniversalTime();
        }
    }
}