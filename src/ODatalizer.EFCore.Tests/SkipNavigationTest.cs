using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    /// <summary>
    /// GET ~/entitysets(key)/skipnavigations
    /// GET ~/entitysets(key)/skipnavigations(key)
    /// GET ~/entitysets(key)/skipnavigations/$ref
    /// POST ~/entitysets(key)/skipnavigations/$ref
    /// Delete ~/entitysets(key)/skipnavigations(key)/$ref
    /// </summary>
    [TestCaseOrderer("ODatalizer.EFCore.Tests.PriorityOrderer", "ODatalizer.EFCore.Tests")]
    public class SkipNavigationTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public SkipNavigationTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/skipnavigations"), TestPriority(0)]
        public async Task Get()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/Categories?$count=true");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var total = (int)result.SelectToken("$['@odata.count']");
            var products = result.SelectTokens("$.value[:]");

            Assert.Equal(1, total);
            Assert.Equal("Category 1", (string)products.First().SelectToken("Name"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/skipnavigations(key)"), TestPriority(0)]
        public async Task Find()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/Categories(1)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Category 1", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/skipnavigations/$ref"), TestPriority(0)]
        public async Task GetRef()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/Categories/$ref");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var refs = result.SelectTokens("$.value[:]");

            Assert.Equal(1, refs.Count());
            Assert.Equal("http://localhost/sample/Categories(1)", (string)refs.First().SelectToken("['@odata.id']"));
        }

        [Fact(DisplayName = "POST ~/entitysets(key)/skipnavigations/$ref"), TestPriority(1)]
        public async Task Post()
        {
            var response = await _client.PostAsync("/sample/Products(1L)/Categories/$ref", Helpers.JSON(new Dictionary<string, object> { 
                ["@odata.id"] = "http://localhost/sample/Categories(2)"
            }));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories(2)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)/skipnavigations(key)/$ref"), TestPriority(2)]
        public async Task Delete()
        {
            var response = await _client.DeleteAsync("/sample/Products(1L)/Categories(1)/$ref");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories(1)");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
