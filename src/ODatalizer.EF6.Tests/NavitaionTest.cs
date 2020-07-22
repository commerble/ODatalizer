using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODatalizer.EF6.Builders;
using ODatalizer.EF6.Tests.Host;
using Sample.EF6;
using Sample.EF6.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ODatalizer.EF6.Tests
{
    /// <summary>
    /// GET ~/entitysets(key)/one
    /// PUT ~/entitysets(key)/one
    /// Patch ~/entitysets(key)/one
    /// Delete ~/entitysets(key)/one
    /// GET ~/entitysets(key)/many
    /// POST ~/entitysets(key)/many
    /// PUT ~/entitysets(key)/many(key)
    /// Patch ~/entitysets(key)/many(key)
    /// Delete ~/entitysets(key)/many(key)
    /// </summary>
    [TestCaseOrderer("ODatalizer.EF6.Tests.PriorityOrderer", "ODatalizer.EF6.Tests")]
    public class NavigationTest : IClassFixture<ODatalizerWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly IEdmModel _edm;

        public NavigationTest(ODatalizerWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();

            using var scope = factory.Server.Services.CreateScope();
            _edm = EdmBuilder.Build(scope.ServiceProvider.GetRequiredService<SampleDbContext>());
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one (principal to optional)"), TestPriority(0)]
        public async Task GetOne1()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one (optional to principal)"), TestPriority(0)]
        public async Task GetOne2()
        {
            var response = await _client.GetAsync("/sample/SalesProducts(1L)/Product");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample 1", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "POST ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PostOne()
        {
            var response = await _client.PostAsync("/sample/Products(2L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "None",
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(2L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "PUT ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PutOne()
        {
            var response = await _client.PutAsync("/sample/Products(1L)/SalesProduct", Helpers.JSON(new {
                ProductId = 1L,
                TaxRoundMode = "Floor",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Floor", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)/one"), TestPriority(1)]
        public async Task PatchOne()
        {
            var response = await _client.PatchAsync("/sample/Products(1L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "Inclusive",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Inclusive", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)/one"), TestPriority(2)]
        public async Task DeleteOne()
        {
            var response = await _client.DeleteAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/many"), TestPriority(0)]
        public async Task GetMany()
        {
            var response = await _client.GetAsync("/sample/SalesPatterns(1)/Products");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(5, result.SelectTokens("$.value[:]").Count());
        }

        [Fact(DisplayName = "POST ~/entitysets(key)/many"), TestPriority(1)]
        public async Task PostMany()
        {
            var response = await _client.PostAsync("/sample/SalesPatterns(1)/Products", Helpers.JSON(new { 
                Name = "Sample X",
                UnitPrice = 4.00m,
            }));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(1, (int)result.SelectToken("$.SalesPatternId"));
        }

        [Fact(DisplayName = "PUT ~/entitysets(key)/many"), TestPriority(1)]
        public async Task PutMany()
        {
            var response = await _client.PutAsync("/sample/SalesPatterns(1)/Products(1L)", Helpers.JSON(new
            {
                Id = 1,
                Name = "Sample X",
                UnitPrice = 4.00m,
                SalesPatternId = 1
            })) ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/SalesPatterns(1)/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Sample X", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)/many"), TestPriority(1)]
        public async Task PatchMany()
        {
            var response = await _client.PatchAsync("/sample/SalesPatterns(1)/Products(1L)", Helpers.JSON(new
            {
                Name = "Patched",
            }));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/SalesPatterns(1)/Products(1L)");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Patched", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "DELETE ~/entitysets(key)/many"), TestPriority(2)]
        public async Task DeletehMany()
        {
            var response = await _client.DeleteAsync("/sample/SalesPatterns(1)/Products(4L)");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/SalesPatterns(1)/Products(4L)");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one/one"), TestPriority(0)]
        public async Task GetOneOne()
        {
            var response = await _client.GetAsync("/sample/SalesProducts(1L)/Product/SalesPattern");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Round", (string)result.SelectToken("$.TaxRoundMode"));
            Assert.Equal(0.1m, (decimal)result.SelectToken("$.TaxRate"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/one/many"), TestPriority(0)]
        public async Task GetOneMany()
        {
            var response = await _client.GetAsync("/sample/SalesProducts(1L)/Product/Categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        }


        [Fact(DisplayName = "POST ~/entitysets(key)/one/many"), TestPriority(1)]
        public async Task PostOneMany()
        {
            var response = await _client.PostAsync("/sample/Products(1L)/SalesPattern/Products", Helpers.JSON(new { 
                Name = "New Product"
            })); ;

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.True(0 < (long)result.SelectToken("$.Id"));
            Assert.Equal("New Product", (string)result.SelectToken("$.Name"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/.../one"), TestPriority(0)]
        public async Task GetLastOne()
        {
            var response = await _client.GetAsync("/sample/Campaigns(1)/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "POST ~/entitysets(key)/.../one"), TestPriority(1)]
        public async Task PostLastOne()
        {
            var response = await _client.PostAsync("/sample/SalesPatterns(1)/Products(3L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "Floor",
                TaxRate = 0.1m
            })); ;

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Floor", (string)result.SelectToken("$.TaxRoundMode"));
            Assert.Equal(0.1m, (decimal)result.SelectToken("$.TaxRate"));
        }

        [Fact(DisplayName = "PUT ~/entitysets(key)/.../one"), TestPriority(1)]
        public async Task PutLastOne()
        {
            var response = await _client.PutAsync("/sample/SalesPatterns(1)/Products(1L)/SalesProduct", Helpers.JSON(new
            {
                ProductId = 1L,
                TaxRoundMode = "None",
                TaxRate = 0.20m,
            })); ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/SalesPatterns(1)/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("None", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)/.../one"), TestPriority(1)]
        public async Task PatchLastOne()
        {
            var response = await _client.PatchAsync("/sample/Campaigns(1)/Products(1L)/SalesProduct", Helpers.JSON(new
            {
                TaxRoundMode = "Floor"
            })); ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Campaigns(1)/Products(1L)/SalesProduct");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Floor", (string)result.SelectToken("$.TaxRoundMode"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/.../many"), TestPriority(0)]
        public async Task GetLastMany()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/Campaigns(1)/Actions(CampaignId=1,CampaignType='Sample')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact(DisplayName = "PUT ~/entitysets(key)/.../many(key)"), TestPriority(1)]
        public async Task PutLastMany()
        {
            var response = await _client.PutAsync("/sample/Products(1L)/Campaigns(1)/Actions(CampaignId=1,CampaignType='Sample')", Helpers.JSON(new
            {
                CampaignId = 1,
                CampaignType = "Sample",
                OptionValue = "Option 2"
            })); ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Campaigns(1)/Actions(CampaignId=1,CampaignType='Sample')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Option 2", (string)result.SelectToken("$.OptionValue"));
        }

        [Fact(DisplayName = "PATCH ~/entitysets(key)/.../many(key)"), TestPriority(1)]
        public async Task PatchLastMany()
        {
            var response = await _client.PatchAsync("/sample/Products(1L)/Campaigns(1)/Actions(CampaignId=1,CampaignType='Sample')", Helpers.JSON(new
            {
                OptionValue = "Option Patched"
            })); ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/Campaigns(1)/Actions(CampaignId=1,CampaignType='Sample')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Option Patched", (string)result.SelectToken("$.OptionValue"));
        }

        [Fact(DisplayName = "GET ~/entitysets(key)/.../many", Skip = "Not implemented"), TestPriority(0)]
        public async Task GetLastMany2()
        {
            var response = await _client.GetAsync("/sample/Products(1L)/CampaignRelations(1)/Campaign/Actions('Sample')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact(DisplayName = "Put ~/entitysets(key)/.../many(key)", Skip = "Not implemented"), TestPriority(0)]
        public async Task PutLastMany2()
        {
            var response = await _client.PutAsync("/sample/Products(1L)/CampaignRelations(1)/Campaign/Actions('Sample')", Helpers.JSON(new
            {
                CampaignId = 1,
                CampaignType = "Sample",
                OptionValue = "Option 2"
            })); ;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.GetAsync("/sample/Products(1L)/CampaignRelations(1)/Campaign/Actions('Sample')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Option 2", (string)result.SelectToken("$.OptionValue"));
        }
    }
}
