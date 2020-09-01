using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
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
    [TestCaseOrderer("ODatalizer.EFCore.Tests.PriorityOrderer", "ODatalizer.EFCore.Tests")]
    public class NestedTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public NestedTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "POST ~/entitysets"), TestPriority(1)]
        public async Task Post()
        {
            var response = await _client.PostAsync("/sample/Products", Helpers.JSON(new
            {
                Name = "Sample X",
                UnitPrice = 9.99m,
                SalesPatternId = 1,
                SalesProduct = new
                {
                    TaxRoundMode = "Ceil",
                    TaxRate = 0.20m,
                }
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample X", (string)result.SelectToken("$.Name"));

            var id = (long)result.SelectToken("$.Id");

            response = await _client.GetAsync($"/sample/SalesProducts({id}L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Ceil", (string)result.SelectToken("$.TaxRoundMode"));
            Assert.Equal(0.20m, (decimal)result.SelectToken("$.TaxRate"));

        }
    }
}
