using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EFCore.Tests
{
    [TestCaseOrderer("ODatalizer.EFCore.Tests.PriorityOrderer", "ODatalizer.EFCore.Tests")]
    public class DateTimeTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public DateTimeTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "GET ~/Holidays"), TestPriority(0)]
        public async Task Get()
        {
            var response = await _client.GetAsync("/sample/Holidays");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains("2023-01-21T00:00:00+", body);
        }

        [Fact(DisplayName = "GET ~/Holidays(key)"), TestPriority(0)]
        public async Task Find()
        {
            var response = await _client.GetAsync("/sample/Holidays(2023-01-21T00:00:00+09:00)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sat", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "POST ~/Holidays"), TestPriority(1)]
        public async Task Post()
        {
            var response = await _client.PostAsync("/sample/Holidays", Helpers.JSON(new
            {
                Date = "2023-01-01T00:00:00+09:00",
                Name = "DateTime X",
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            var datetimeoffset = DateTimeOffset.Parse((string)result.SelectToken("$.Date"));
            
            Assert.Equal("2023-01-01T00:00:00+09:00", datetimeoffset.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }

        [Fact(DisplayName = "PUT ~/Holidays(key)"), TestPriority(1)]
        public async Task Put()
        {
            var response = await _client.PutAsync("/sample/Holidays(2023-01-22T00:00:00+09:00)", Helpers.JSON(new
            {
                Date = "2023-01-22T00:00:00+09:00",
                Name = "Put",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Holidays(2023-01-22T00:00:00+09:00)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Put", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "PATCH ~/Holidays(key)"), TestPriority(1)]
        public async Task Patch()
        {
            var response = await _client.PatchAsync("/sample/Holidays(2023-01-22T00:00:00+09:00)", Helpers.JSON(new
            {
                Name = "Patch",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Holidays(2023-01-22T00:00:00+09:00)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Patch", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "DELETE ~/Holidays(key)"), TestPriority(2)]
        public async Task Delete()
        {
            var response = await _client.DeleteAsync("/sample/Holidays(2023-01-21T00:00:00+09:00)");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Holidays(2023-01-21T00:00:00+09:00)");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
