![Unit Tests](https://github.com/commerble/ODatalizer/workflows/test/badge.svg)

The simplest implementation of OData server.

## Install
no nuget yet.

<!-- 
    $ dotnet add package ODatalizer.EFCore
    PS> Install-Package  ODatalizer.EFCore
-->

## Consepts

* Simple setup
* Can use even if dynamic load DbContext assembly

## Usage

```cs:Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // add DbContext
    services.AddDbContext<SampleDbContext>();

    // add ODatalizer services
    services.AddODatalizer();

    ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
{
    // create ODatalizer ep metadata
    var sample = new ODatalizerEndpoint(sample, "Sample", "sample");

    // load ODatalizer controllers
    app.UseODatalizer(sample);

    ...

    app.UseEndpoints(endpoints =>
    {
        // map ODatalizer routes
        endpoints.MapODatalizer(sample);
        ...
    });
}
```
