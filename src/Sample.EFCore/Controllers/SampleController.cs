using System;
using Sample.EFCore.Data;
using ODatalizer.EFCore;
using Sample.EFCore.Entities;
using Microsoft.AspNet.OData.Routing;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Sample.EFCore.Controllers
{
    public class SampleController : ODatalizerController<SampleDbContext>
    {
        public SampleController(IServiceProvider sp) : base(sp)
        {
        }
    }
}