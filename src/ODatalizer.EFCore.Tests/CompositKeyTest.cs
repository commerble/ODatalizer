using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System.Net;
using System.Net.Http;
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
        /**
         * https://docs.oasis-open.org/odata/odata/v4.01/cs01/abnf/odata-abnf-construction-rules.txt
         * CompoundKey(multi-part keys) needs to specify key=value.
         */
        [Theory(DisplayName = "GET ~/entitysets(name1=key1,name2=key2)"), TestPriority(0)]
        [InlineData("/sample/Favorites(2,1)")]
        [InlineData("/sample/Favorites(UserId=1,ProductId=2)")]
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        // [InlineData("/sample/Favorites(UserId=1, ProductId=2)")] // --- not supported yet
        // [InlineData("/sample/Favorites/1/2")] // --- not supported yet
        public async Task Find(string path)
        {
            var response = await _client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "PUT ~/entitysets(name1=key1,name2=key2)"), TestPriority(1)]
        [InlineData("/sample/Favorites(2,1)")]
        [InlineData("/sample/Favorites(UserId=1,ProductId=2)")]
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        // [InlineData("/sample/Favorites(UserId=1, ProductId=2)")] // --- not supported yet
        // [InlineData("/sample/Favorites/1/2")] // --- not supported yet
        public async Task Put(string path)
        {
            var response = await _client.PutAsync(path, Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 2,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory(DisplayName = "PATCH ~/entitysets(name1=key1,name2=key2)"), TestPriority(1)]
        [InlineData("/sample/Favorites(2,1)")]
        [InlineData("/sample/Favorites(UserId=1,ProductId=2)")]
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        // [InlineData("/sample/Favorites(UserId=1, ProductId=2)")] // --- not supported yet
        // [InlineData("/sample/Favorites/1/2")] // --- not supported yet
        public async Task Patch(string path)
        {
            var response = await _client.PatchAsync(path, Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 2,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory(DisplayName = "DELETE ~/entitysets(name1=key1,name2=key2)"), TestPriority(2)]
        [InlineData("/sample/Favorites(3,1)")]
        [InlineData("/sample/Favorites(UserId=1,ProductId=1)")]
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        // [InlineData("/sample/Favorites(UserId=1, ProductId=3)")] // --- not supported yet
        // [InlineData("/sample/Favorites/1/4")] // --- not supported yet
        public async Task Delete(string path)
        {
            var response = await _client.DeleteAsync(path);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
