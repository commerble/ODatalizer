![Unit Tests](https://github.com/commerble/ODatalizer/workflows/test/badge.svg)

The simplest implementation of OData server.

## Consepts

* Simple setup
* Can use even if dynamic load DbContext assembly

## Install

    $ dotnet add package ODatalizer.EFCore
    $ dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 6.0.*

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
    services.AddODatalizer(sp =>
    {
        // create ODatalizer ep metadata
        var ep = new ODatalizerEndpoint(
                    db: sp.GetRequiredService<SampleDbContext>(),
                    routeName: "Sample",
                    routePrefix: "sample");

        return new[] { ep }; 
    });

    ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
{
    // use $batch endpoint
    app.UseODatalizer();

    ...
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

Auto generate first level navigationlink property endpoint.

- generate : ~/entityset/key/navigation/$ref
- generate : ~/entityset/key/navigation/key/$ref
- not generate : ~/entityset/key/navigation/navigation/$ref
- not generate : ~/entityset/key/navigation/navigation/navigation/$ref

Dynamic controller does not support navigationlink segment.
If you need not generate path, create controller action by self in ODataController or ODatalizerController.

## Others

See sample project https://github.com/commerble/ODatalizer/tree/master/src/Sample.EFCore

- Authorize
- TypeConverter(ModelBinder)
