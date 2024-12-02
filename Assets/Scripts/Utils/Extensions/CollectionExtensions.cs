using System.Collections;

namespace Utils.Extensions {
    public static class CollectionExtensions {
        public static bool IsEmpty<T>(this T collection) where T : ICollection => collection.Count == 0;
        public static bool IsNotEmpty<T>(this T collection) where T : ICollection => !collection.IsEmpty();
    }
}