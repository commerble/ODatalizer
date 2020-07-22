using Newtonsoft.Json.Linq;
using ODatalizer.EF6.Tests.Host;
using Sample.EF6;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EF6.Tests
{
    /// <summary>
    /// GET ~/entitysets
    /// GET ~/entitysets(key)
    /// POST ~/entitysets
    /// PUT ~/entitysets(key)
    /// Patch ~/entitysets(key)
    /// Delete ~/entitysets(key)
    /// </summary>
    [TestCaseOrderer("ODatalizer.EF6.Tests.PriorityOrderer", "ODatalizer.EF6.Tests")]
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

        [Fact(DisplayName = "POST ~/entitysets(key)/many/$ref"), TestPriority(1)]
        public async Task Post()
        {
            var response = await _client.PostAsync("/sample/Products(1L)/Categories/$ref", Helpers.JSON(new Dictionary<string, string>
            {
                ["@odata.id"] = "http://localhost/sample/Categories(2)"
            }));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories/$ref");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var refs = result.SelectTokens("$.value[:]['@odata.id']");

            Assert.Equal(2, refs.Count());
            Assert.Equal("http://localhost/sample/Categories(1)", refs.First());
            Assert.Equal("http://localhost/sample/Categories(2)", refs.Last());
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)/many/$ref"), TestPriority(2)]
        public async Task Delete()
        {
            var response = await _client.DeleteAsync("/sample/Products(1L)/Categories(1)/$ref");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories/$ref");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var refs = result.SelectTokens("$.value[:]['@odata.id']");

            Assert.True(refs.All(r => (string)r != "http://localhost/sample/Categories(1)"));
        }
    }
}
