![Unit Tests](https://github.com/commerble/ODatalizer/workflows/test/badge.svg)

The simplest implementation of OData server.

## Consepts

* Simple setup
* Can use even if dynamic load DbContext assembly

## Entitfy Framework Core 3.x

### Install

    $ dotnet add package ODatalizer.EFCore --version 3.*

### Usage

```cs:Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // add DbContext and use lazy loading proxies.
    services.AddDbContext<SampleDbContext>(opt => 
        opts
            .UseLazyLoadingProxies()
            // If your db provider does not suppot AmbientTransaction, ignore the warning.
            .ConfigureWarnings(o => o.Ignore(RelationalEventId.AmbientTransactionWarning)));

    // add ODatalizer services
    services.AddODatalizer();

    ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
{
    // create ODatalizer ep metadata
    var ep = new ODatalizerEndpoint(db:sample, 
                                    routeName: "Sample", 
                                    routePrefix: "sample");

    // load ODatalizer controllers
    app.UseODatalizer(ep);

    ...

    app.UseEndpoints(endpoints =>
    {
        // map ODatalizer routes
        endpoints.MapODatalizer(ep);
        ...
    });
}
```

### Many to Many

Entity Framework Core 3.x has not many to many relatioship feature.

## Entity Framework 6

### Install

    $ dotnet add package ODatalizer.EF6

### Usage

```cs:Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    
    // add DbContext
    services.AddScoped(sp => new SampleDbContext(connection));

    // add ODatalizer services
    services.AddODatalizer();

    ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
{
    // create ODatalizer ep metadata
    var ep = new ODatalizerEndpoint(db:sample, 
                                    routeName: "Sample", 
                                    routePrefix: "sample");

    // load ODatalizer controllers
    app.UseODatalizer(ep);

    ...

    app.UseEndpoints(endpoints =>
    {
        // map ODatalizer routes
        endpoints.MapODatalizer(ep);
        ...
    });
}
```
