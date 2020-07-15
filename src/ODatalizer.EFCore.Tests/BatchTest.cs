using Newtonsoft.Json.Linq;
using ODatalizer.EFCore.Tests.Host;
using Sample.EFCore;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class BatchTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public BatchTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "Batch"), TestPriority(0)]
        public async Task Batch()
        {
            var batch = new MultipartContent("mixed", "batch" + Guid.NewGuid());
            var changeset = new MultipartContent("mixed", "changeset" + Guid.NewGuid());

            {
                var message = new HttpRequestMessage(HttpMethod.Post, "http://localhost/sample/SalesPatterns")
                {
                    Content = Helpers.JSON(new
                    {
                        TaxRoundMode = "Round",
                        TaxRate = 0.08m
                    })
                };
                message.Headers.Add("Content-ID", "1");

                var content = new HttpMessageContent(message);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                content.Headers.Add("Content-Transfer-Encoding", "binary");

                changeset.Add(content);
            }

            {
                var message = new HttpRequestMessage(HttpMethod.Post, "http://localhost/sample/$1/Products")
                {
                    Content = Helpers.JSON(new
                    {
                        Name = "Sample Batch",
                        UnitPrice = 9.99m
                    })
                };
                message.Headers.Add("Content-ID", "2");

                var content = new HttpMessageContent(message);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                content.Headers.Add("Content-Transfer-Encoding", "binary");

                changeset.Add(content);
            }

            batch.Add(changeset);
            var response = await _client.PostAsync("/sample/$batch", batch);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("multipart/mixed", response.Content.Headers.ContentType.MediaType);

            var parts = await Helpers.ParseMultipartMixedAsync(response);

            Assert.Equal(2, parts.Length);

            Assert.Equal(HttpStatusCode.Created, parts[0].StatusCode);
            var salesPattern = await parts[0].Content.ReadAsAsync<dynamic>();
            Assert.True((int)salesPattern.Id > 0);
            Assert.Equal("Round", (string)salesPattern.TaxRoundMode);

            Assert.Equal(HttpStatusCode.Created, parts[1].StatusCode);
            var product = await parts[1].Content.ReadAsAsync<dynamic>();
            Assert.True((long)product.Id > 0);
            Assert.Equal("Sample Batch", (string)product.Name);
            Assert.Equal((int)salesPattern.Id, (int)product.SalesPatternId);

        }
    }
}
