using Microsoft.AspNetCore.Mvc.Testing;

namespace ODatalizer.EFCore.Tests.Host
{
    public class ODatalizerWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
    }
}
