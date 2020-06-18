using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using Sample.EFCore.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    public class MetadataTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly IEdmModel _edm;

        public MetadataTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();

            using var scope = factory.Server.Services.CreateScope();
            _edm = EdmBuilder.Build(scope.ServiceProvider.GetRequiredService<SampleDbContext>());
        }

        [Fact]
        public async Task GetServiceDocument()
        {
            var response = await _client.GetAsync("/sample/");

            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var actual = result.SelectTokens("$.value[:].name").Select(o => (string)o).OrderBy(o => o);
            var expected = _edm.EntityContainer.EntitySets().Select(set => set.Name).OrderBy(o => o);

            Assert.True(Enumerable.SequenceEqual(expected, actual));
        }

        [Fact]
        public async Task GetMetadata()
        {
            var response = await _client.GetAsync("/sample/$metadata");
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
        }
    }
}
