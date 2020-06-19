using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using Sample.EFCore.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    /// <summary>
    /// GET ~/entitysets
    /// GET ~/entitysets(key)
    /// POST ~/entitysets
    /// PUT ~/entitysets(key)
    /// Patch ~/entitysets(key)
    /// Delete ~/entitysets(key)
    /// </summary>
    public class SimpleTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public SimpleTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "GET ~/entitysets")]
        public async Task _00_Get()
        {
            var response = await _client.GetAsync("/sample/Products?$count=true");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var total = (int)result.SelectToken("$['@odata.count']");
            var products = result.SelectTokens("$.value[:]");

            Assert.Equal(5, total);
            Assert.Equal(5, products.Count());
        }

        [Fact(DisplayName = "GET ~/entitysets(key)")]
        public async Task _01_Find()
        {
            var response = await _client.GetAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample 1", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "POST ~/entitysets")]
        public async Task _02_Post()
        {
            var response = await _client.PostAsync("/sample/Products", Helpers.JSON(new
            {
                Name = "Sample X",
                UnitPrice = 9.99m,
                SalesPatternId = 1,
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample X", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "PUT ~/entitysets(key)")]
        public async Task _03_Put()
        {
            var response = await _client.PutAsync("/sample/Products(1L)", Helpers.JSON(new
            {
                Id = 1L,
                Name = "Sample 1",
                UnitPrice = 12.00m,
                SalesPatternId = 1,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(12.00m, (decimal)result.SelectToken("$.UnitPrice"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)")]
        public async Task _04_Patch()
        {
            var response = await _client.PatchAsync("/sample/Products(1L)", Helpers.JSON(new
            {
                UnitPrice = 99.99m,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(99.99m, (decimal)result.SelectToken("$.UnitPrice"));
            Assert.Equal("Sample 1", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)")]
        public async Task _05_DELETE()
        {
            var response = await _client.DeleteAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
