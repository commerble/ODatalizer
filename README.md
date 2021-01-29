![Unit Tests](https://github.com/commerble/ODatalizer/workflows/test/badge.svg)

The simplest implementation of OData server.

## Consepts

* Simple setup
* Can use even if dynamic load DbContext assembly

## Install

### Entity Framework Core 5.x

    $ dotnet add package ODatalizer.EFCore --version 5.*

### Entity Framework Core 3.x

    $ dotnet add package ODatalizer.EFCore --version 3.*

## Usage

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

## Navigation Resource Path
When you need to read and write all accesses, custom dynamic controller inherits `ODatalizerController<TDbContext>`.

```cs:SampleController
public class SampleController : ODatalizerController<SampleDbContext>
{
    public SampleController(IServiceProvider sp) : base(sp)
    {
    }
}
```

And you set the controller name to endpoint settings.

```cs:Startup.cs
// create ODatalizer ep metadata
var ep = new ODatalizerEndpoint(db:sample, 
                                routeName: "Sample", 
                                routePrefix: "sample",
                                controller: nameof(SampleController));
```


## Many to Many (Skip navigation)

### Entity Framework Core 5.x

Auto generate first level navigationlink property endpoint.

- generate : ~/entityset/key/navigation/$ref
- generate : ~/entityset/key/navigation/key/$ref
- not generate : ~/entityset/key/navigation/navigation/$ref
- not generate : ~/entityset/key/navigation/navigation/navigation/$ref

Dynamic controller does not support navigationlink segment.
If you need not generate path, create controller action by self in ODataController or ODatalizerController.

### Entity Framework Core 3.x

Entity Framework Core 3.x has not many to many relatioship feature.