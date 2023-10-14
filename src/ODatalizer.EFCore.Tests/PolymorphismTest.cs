using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    public class PolymorphismTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public PolymorphismTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/sample/Products(1)")]
        [InlineData("/sample/Products(Id=1)")]
        //[InlineData("/sample/Favorites(2,1)")] // not supported
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        public async Task Find(string path)
        {
            var response = await _client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/sample/Products(1)")]
        [InlineData("/sample/Products(Id=1)")]
        public async Task PutProduct(string path)
        {
            var response = await _client.PutAsync(path, Helpers.JSON(new
            {
                Id = 1L,
                Name = "Sample 1",
                UnitPrice = 12.00m,
                SalesPatternId = 1,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        //[InlineData("/sample/Favorites(2,1)")] // not supported
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        public async Task PutFavorite(string path)
        {
            var response = await _client.PutAsync(path, Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 2,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData("/sample/Products(1)")]
        [InlineData("/sample/Products(Id=1)")]
        public async Task PatchProduct(string path)
        {
            var response = await _client.PatchAsync(path, Helpers.JSON(new
            {
                Name = "Sample 1",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        //[InlineData("/sample/Favorites(2,1)")] // not supported
        [InlineData("/sample/Favorites(ProductId=2,UserId=1)")]
        public async Task PatchFavorite(string path)
        {
            var response = await _client.PatchAsync(path, Helpers.JSON(new
            {
                UserId = 1,
                ProductId = 2,
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData("/sample/Products(2)")]
        [InlineData("/sample/Products(Id=1)")]
        //[InlineData("/sample/Favorites(2,1)")] // not supported
        [InlineData("/sample/Favorites(ProductId=3,UserId=1)")]
        public async Task Delete(string path)
        {
            var response = await _client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
