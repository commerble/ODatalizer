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
    public class AuthorizeTest : IClassFixture<AuthorizeODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public AuthorizeTest(AuthorizeODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact, TestPriority(0)]
        public async Task BeforeAuth()
        {
            var response = await _client.GetAsync("/sample/Products");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact, TestPriority(1)]
        public async Task AfterAuth()
        {
            var response = await _client.GetAsync("/login");

            response = await _client.GetAsync("/sample/Products");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesPattern");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
