using System.Text.Json;

namespace PhoneStore.Web.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this HttpSessionExtensions session, string key, object value)
        {
            // Tạm thời dùng phương thức mở rộng chung bên dưới
        }
    }

    public static class SessionExtensionsMethods
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
