using System;
using System.Text;
using Core.Domain.Locale;

namespace Utils.Extensions {
    public static class StringExtensions {
        public static string LeftPadded(this string str, char padChar, int length) {
            var builder = new StringBuilder(length);
            for (var x = str.Length; x < length; x++) builder.Append(padChar);
            builder.Append(str);
            return builder.ToString();
        }

        public static string RightPadded(this string str, char padChar, int length) {
            var builder = new StringBuilder(str);
            for (var x = str.Length; x < length; x++) builder.Append(padChar);
            return builder.ToString();
        }

        public static string ToCommonString(this short value) => value.ToString("N0");

        public static string ToCommaString(this int value) => value.ToString("N0");

        public static string ToCommaString(this long value) => value.ToString("N0");

        public static string ToCommaString(this float value) => ((int) value).ToString("N0");

        public static string ToCommaString(this double value) => ((int) value).ToString("N0");

        public static string ToPercentage(this int value) => $"{value.ToCommaString()}%";

        public static string ToPercentage(this float value) => $"{value.ToCommaString()}%";

        public static string ToPercentage(this double value) => $"{value.ToCommaString()}%";

        public static string ToTimer(this float value, YisoLocale.Locale locale = YisoLocale.Locale.KR) {
            var span = TimeSpan.FromSeconds(value);
            var min = locale == YisoLocale.Locale.KR ? "분" : "m";
            var sec = locale == YisoLocale.Locale.KR ? "초" : "s";
            if (span.TotalSeconds >= 300) return $"{span:%m}{min}";
            if (span.TotalSeconds > 60 && span.TotalSeconds < 300) return $"{span:%m}{min} {span:%s}{sec}";
            if (span.TotalSeconds > 1 && span.TotalSeconds < 60) return $"{span:%s}{sec}";
            return $"0.{span:%f}{sec}";
        }
    }
}