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
    /// GET ~/entitysets(key)/one
    /// PUT ~/entitysets(key)/one
    /// Patch ~/entitysets(key)/one
    /// Delete ~/entitysets(key)/one
    /// GET ~/entitysets(key)/many
    /// POST ~/entitysets(key)/many
    /// PUT ~/entitysets(key)/many(key)
    /// Patch ~/entitysets(key)/many(key)
    /// Delete ~/entitysets(key)/many(key)
    /// </summary>
    [TestCaseOrderer("ODatalizer.EFCore.Tests.PriorityOrderer", "ODatalizer.EFCore.Tests")]
    public class NavigationTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly IEdmModel _edm;

        public NavigationTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();

            using var scope = factory.Server.Services.CreateScope();
            _edm = EdmBuilder.Build(scope.ServiceProvider.GetRequiredService<SampleDbContext>());
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one (principal to optional)"), TestPriority(0)]
        public async Task GetOne1()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one (optional to principal)"), TestPriority(0)]
        public async Task GetOne2()
        {
            var response = await _client.GetAsync("/sample/SalesProducts(1L)/Product");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample 1", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "POST ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PostOne()
        {
            var response = await _client.PostAsync("/sample/Products(2L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "None",
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(2L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "PUT ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PutOne()
        {
            var response = await _client.PutAsync("/sample/Products(1L)/SalesProduct", Helpers.JSON(new {
                ProductId = 1L,
                TaxRoundMode = "Floor",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Floor", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PatchOne()
        {
            var response = await _client.PatchAsync("/sample/Products(1L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "Inclusive",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Inclusive", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)/one"), TestPriority(2)]
        public async Task DeleteOne()
        {
            var response = await _client.DeleteAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/many (one to many)"), TestPriority(0)]
        public async Task GetOneToMany()
        {
            var response = await _client.GetAsync("/sample/SalesPatterns(1)/Products");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(5, result.SelectTokens("$.value[:]").Count());
        }
    }
}
