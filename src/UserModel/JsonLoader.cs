using Newtonsoft.Json.Linq;
using System;

namespace UserModel
{
    public class JsonLoader<T>
    {
        public static bool TryGetValue<T>(string name, JObject objJson, out T value)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;

            if (objJson.TryGetValue(name, stringCompare, out JToken outJson))
            {
                value = outJson.Value<T>();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }

        }
    }
}
