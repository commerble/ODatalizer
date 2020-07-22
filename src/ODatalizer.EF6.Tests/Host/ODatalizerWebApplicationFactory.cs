using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sample.EF6.Data;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Linq;

namespace ODatalizer.EF6.Tests.Host
{
    public class ODatalizerWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _ = builder.ConfigureServices(services =>
              {
                  // Create a new service provider.
                  var serviceProvider = new ServiceCollection().BuildServiceProvider();

                  // Add a database context (AppDbContext) using an in-memory database for testing.
                  services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(SampleDbContext)));

				  //DbProviderFactories.UnregisterFactory("System.Data.SQLite");
				  //DbProviderFactories.RegisterFactory("System.Data.SQLite", new SQLiteFactory());
				  //DbProviderFactories.UnregisterFactory("System.Data.SQLite.EF6");
				  //DbProviderFactories.RegisterFactory("System.Data.SQLite.EF6", new SQLiteProviderFactory());

				  SampleDbConfiguration.ProviderFactories.TryAdd("System.Data.SQLite", SQLiteFactory.Instance);
                  SampleDbConfiguration.ProviderFactories.TryAdd("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
                  SampleDbConfiguration.ProviderServices.TryAdd("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));

                  var connection = new SQLiteConnection("datasource=:memory:");
				  connection.Open();

                  var sample = new SampleDbContext(connection);

                  sample.Database.ExecuteSqlCommand(_init);

                  SampleDbInitializer.Initialize(sample);

                  services.AddSingleton(sp => sample);
              });
        }

        private const string _init =
@"CREATE TABLE Campaigns(
	 Id        INTEGER PRIMARY KEY
	,Name      nvarchar(256)
	,StartDate datetime2
	,EndDate   datetime2
);

CREATE TABLE CampaignActions(
	 CampaignId   int
	,CampaignType nvarchar(256)
	,OptionValue	nvarchar(256)
	, CONSTRAINT PK_CampaignActions PRIMARY KEY (CampaignId, CampaignType)
	, CONSTRAINT FK_CampaignActions_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns (Id)
);

CREATE TABLE Categories(
	 Id        INTEGER PRIMARY KEY
	,Name      nvarchar(256)
);

CREATE TABLE SalesPatterns(
	 Id           INTEGER PRIMARY KEY
	,TaxRoundMode int not null
	,TaxRate      money not null
);

CREATE TABLE Products(
	 Id             INTEGER PRIMARY KEY
	,Name           nvarchar(256)
	,UnitPrice      money not null
	,SalesPatternId int not null
);

CREATE TABLE SalesProducts(
	 ProductId    bigint
	,TaxRoundMode int null
	,TaxRate      money null
	, CONSTRAINT PK_SalesProducts PRIMARY KEY (ProductId)
	, CONSTRAINT FK_SalesProducts_Products FOREIGN KEY (ProductId) REFERENCES Products (Id)
);


CREATE TABLE ProductCategoryRelations(
	 ProductId  bigint
	,CategoryId int
	, CONSTRAINT PK_ProductCategoryRelations PRIMARY KEY (ProductId, CategoryId)
	, CONSTRAINT FK_ProductCategoryRelations_Products FOREIGN KEY (ProductId) REFERENCES Products (Id)
	, CONSTRAINT FK_ProductCategoryRelations_Categories FOREIGN KEY (CategoryId) REFERENCES Categories (Id)
);

CREATE TABLE CampaignCategoryRelations(
	 CampaignId int
	,CategoryId int
	, CONSTRAINT PK_CampaignCategoryRelations PRIMARY KEY (CampaignId, CategoryId)
	, CONSTRAINT FK_CampaignCategoryRelations_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns (Id)
	, CONSTRAINT FK_CampaignCategoryRelations_Categories FOREIGN KEY (CategoryId) REFERENCES Categories (Id)
);

CREATE TABLE CampaignProductRelations(
	 CampaignId int
	,ProductId  bigint
	, CONSTRAINT PK_CampaignProductRelations PRIMARY KEY (CampaignId, ProductId)
	, CONSTRAINT FK_CampaignProductRelations_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns (Id)
	, CONSTRAINT FK_CampaignProductRelations_Products FOREIGN KEY (ProductId) REFERENCES Products (Id)
);
";
    }
}
