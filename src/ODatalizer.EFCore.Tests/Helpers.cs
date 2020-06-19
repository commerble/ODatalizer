using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ODatalizer.EFCore.Tests
{
    class Helpers
    {
        public static HttpContent JSON(object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }
    }
}
