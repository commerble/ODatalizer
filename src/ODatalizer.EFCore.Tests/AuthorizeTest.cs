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

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Products?$select=Id,UnitPrice");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesPattern");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Categories(1)/Products");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Categories(1)/Products?$select=Id,UnitPrice");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1)/Categories(1)/Products");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1)/Categories(1)/Products?$select=Id,UnitPrice");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Categories?$expand=Products");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Categories?$expand=Products($select=Id,UnitPrice)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1)?$select=Id&$expand=Categories($expand=Products)");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1)?$select=Id&$expand=Categories($expand=Products($select=Id))");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact, TestPriority(1)]
        public async Task AfterAuthWrite()
        {
            var response = await _client.GetAsync("/login");

            response = await _client.PatchAsync("/sample/Products(1L)", Helpers.JSON(new
            {
                UnitPrice = 99.99m,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.PatchAsync("/sample/Products(1L)", Helpers.JSON(new
            {
                Name = "Forbidden",
            }));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            response = await _client.PatchAsync("/sample/Categories(1)/Products(1L)", Helpers.JSON(new
            {
                UnitPrice = 99.99m,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.PatchAsync("/sample/Categories(1)/Products(1L)", Helpers.JSON(new
            {
                Name = "Forbidden",
            }));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
