using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Extensions {
    public static class EnumExtensions {
        public static T ToEnum<T>(this string value) where T : Enum => (T) Enum.Parse(typeof(T), value);

        public static T ToEnum<T>(this int value) where T : Enum => (T) Enum.ToObject(typeof(T), value);

        public static IEnumerable<T> Values<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();

        public static int Counts<T>() where T : Enum => Values<T>().Count();

        public static int IndexOf<T>(this T value) where T : Enum => Values<T>().ToList().IndexOf(value);
    }
}