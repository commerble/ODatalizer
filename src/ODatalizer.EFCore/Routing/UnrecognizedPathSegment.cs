using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer.EFCore.Routing
{
    public class UnrecognizedPathSegment : ODataPathSegment
    {
        public override IEdmType EdmType => null;
        public override void HandleWith(PathSegmentHandler handler)
        {
        }

        public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
        {
            throw new System.NotImplementedException();
        }
    }
}
