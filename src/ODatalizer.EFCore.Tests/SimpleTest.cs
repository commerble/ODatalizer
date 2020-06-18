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
        private readonly ODatalizerWebApplicationFactory<Startup> _factory;

        public SimpleTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// GET ~/entitysets
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task _00_Get()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/sample/Products?$count=true");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var total = (int)result.SelectToken("$['@odata.count']");
            var products = result.SelectTokens("$.value[:]");

            Assert.Equal(5, total);
            Assert.Equal(5, products.Count());
        }

        /// <summary>
        /// GET ~/entitysets(key)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task _01_Find()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/sample/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample 1", (string)result.SelectToken("$.Name"));
        }

        /// <summary>
        /// POST ~/entitysets
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task _02_Post()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/sample/Products", Helpers.JSON(new
            {
                Name = "Sample X",
                UnitPrice = 9.99m,
                SalesPatternId = 1,
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample X", (string)result.SelectToken("$.Name"));
        }
    }
}
