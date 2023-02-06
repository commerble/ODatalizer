using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    [TestCaseOrderer("ODatalizer.EFCore.Tests.PriorityOrderer", "ODatalizer.EFCore.Tests")]
    public class CompositKeyTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public CompositKeyTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "GET ~/entitysets(key1,key2)"), TestPriority(0)]
        public async Task Find()
        {
            var response = await _client.GetAsync("/sample/Favorites(1,1)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "PUT ~/entitysets(key1,key2)"), TestPriority(1)]
        public async Task Put()
        {
            var response = await _client.PutAsync("/sample/Favorites(1,1)", Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 1,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key1,key2)"), TestPriority(1)]
        public async Task Patch()
        {
            var response = await _client.PatchAsync("/sample/Favorites(1,1)", Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 1,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key1,key2)"), TestPriority(2)]
        public async Task Delete()
        {
            var response = await _client.DeleteAsync("/sample/Favorites(1,1)");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
