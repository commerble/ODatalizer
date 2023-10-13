using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    public class PolymorphismTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public PolymorphismTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("GET /sample/Products({Id0})", routes);
            Assert.Contains("GET /sample/Products(Id={Id0})", routes);
            Assert.Contains("GET /sample/ProductCategoryRelations({CategoryId0},{ProductId0})", routes);
            Assert.Contains("GET /sample/ProductCategoryRelations(CategoryId={CategoryId0},ProductId={ProductId0})", routes);
            Assert.Contains("GET /sample/ProductCategoryRelations(ProductId={ProductId0},CategoryId={CategoryId0})", routes);
        }

        [Fact]
        public async Task Put()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("PUT /sample/Products({Id0})", routes);
            Assert.Contains("PUT /sample/Products(Id={Id0})", routes);
            Assert.Contains("PUT /sample/ProductCategoryRelations({CategoryId0},{ProductId0})", routes);
            Assert.Contains("PUT /sample/ProductCategoryRelations(CategoryId={CategoryId0},ProductId={ProductId0})", routes);
            Assert.Contains("PUT /sample/ProductCategoryRelations(ProductId={ProductId0},CategoryId={CategoryId0})", routes);
        }

        [Fact]
        public async Task Patch()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("PATCH /sample/Products({Id0})", routes);
            Assert.Contains("PATCH /sample/Products(Id={Id0})", routes);
            Assert.Contains("PATCH /sample/ProductCategoryRelations({CategoryId0},{ProductId0})", routes);
            Assert.Contains("PATCH /sample/ProductCategoryRelations(CategoryId={CategoryId0},ProductId={ProductId0})", routes);
            Assert.Contains("PATCH /sample/ProductCategoryRelations(ProductId={ProductId0},CategoryId={CategoryId0})", routes);
        }

        [Fact]
        public async Task Delete()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("DELETE /sample/Products({Id0})", routes);
            Assert.Contains("DELETE /sample/Products(Id={Id0})", routes);
            Assert.Contains("DELETE /sample/ProductCategoryRelations({CategoryId0},{ProductId0})", routes);
            Assert.Contains("DELETE /sample/ProductCategoryRelations(CategoryId={CategoryId0},ProductId={ProductId0})", routes);
            Assert.Contains("DELETE /sample/ProductCategoryRelations(ProductId={ProductId0},CategoryId={CategoryId0})", routes);
        }

        [Fact]
        public async Task GetRef()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("GET /sample/Products({Id0})/Categories/$ref", routes);
            Assert.Contains("GET /sample/Products(Id={Id0})/Categories/$ref", routes);
        }

        [Fact]
        public async Task PostRef()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("POST /sample/Products({Id0})/Categories/$ref", routes);
            Assert.Contains("POST /sample/Products(Id={Id0})/Categories/$ref", routes);
        }

        [Fact]
        public async Task DeleteRef()
        {
            var routes = await _client.GetStringAsync("/debug/routes");

            Assert.Contains("DELETE /sample/Products({Id0})/Categories({Id1})/$ref", routes);
            Assert.Contains("DELETE /sample/Products(Id={Id0})/Categories(Id={Id1})/$ref", routes);

            // not supported
            Assert.DoesNotContain("DELETE /sample/Products(Id={Id0})/Categories({Id1})/$ref", routes);
            Assert.DoesNotContain("DELETE /sample/Products({Id0})/Categories(Id={Id1})/$ref", routes);
        }
    }
}
