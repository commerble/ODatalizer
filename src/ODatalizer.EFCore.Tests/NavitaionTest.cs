using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using Sample.EFCore.Data;
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

        [Fact(DisplayName = "~/entitysets(key)/one")]
        public async Task GetOne()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }
    }
}
