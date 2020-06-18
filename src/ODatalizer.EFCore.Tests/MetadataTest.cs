using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    public class BasicTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public BasicTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetServiceDocument()
        {
            var response = await _client.GetAsync("/sample/");

            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var actual = result.SelectTokens("$.value[:].name").Select(o => (string)o).OrderBy(o => o);
            var expected = new[] { "Campaigns", "CampaignActions", "Categories", "Products", "SalesPatterns", "SalesProducts" }.OrderBy(o => o);

            Assert.True(Enumerable.SequenceEqual(expected, actual));
        }
    }
}
