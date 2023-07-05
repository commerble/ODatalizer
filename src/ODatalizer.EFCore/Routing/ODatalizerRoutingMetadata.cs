using Microsoft.OData.Edm;
using System;

namespace ODatalizer.EFCore.Routing
{
    public class ODatalizerRoutingMetadata
    {
        public ODatalizerRoutingMetadata(string prefix, IEdmModel model)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        internal ODatalizerRoutingMetadata()
        {
        }

        public string Prefix { get; }

        public IEdmModel Model { get; }
    }
}
