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
    public class RefTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public RefTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/many/$ref"), TestPriority(0)]
        public async Task Get()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/Categories/$ref");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var refs = result.SelectTokens("$.value[:]['@odata.id']");

            Assert.Single(refs);
            Assert.Equal("http://localhost/sample/Categories(1)", refs.First());
        }
    }
}
