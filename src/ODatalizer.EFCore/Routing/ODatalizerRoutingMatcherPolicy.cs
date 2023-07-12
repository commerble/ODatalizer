using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Extensions;

namespace ODatalizer.EFCore.Routing
{
    public class ODatalizerRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IOptions<ODataOptions> _odataOptions;

        public ODatalizerRoutingMatcherPolicy(IOptions<ODataOptions> odataOptions)
        {
            _odataOptions = odataOptions;
        }

        public override int Order => 900;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {

            return endpoints.Any(e => e.Metadata.OfType<ODatalizerRoutingMetadata>().FirstOrDefault() != null);
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var odataFeature = httpContext.ODataFeature();
            if (odataFeature.Path != null)
            {
                return Task.CompletedTask;
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                ref CandidateState candidate = ref candidates[i];
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                var metadata = candidate.Endpoint.Metadata.OfType<ODatalizerRoutingMetadata>().FirstOrDefault();
                if (metadata == null)
                {
                    continue;
                }

                var model = metadata.Model;
                if (model == null)
                {
                    continue;
                }

                try
                {
                    var uri = new Uri(httpContext.Request.GetEncodedUrl());
                    var serviceRoot = new Uri(new Uri(uri.GetLeftPart(System.UriPartial.Authority)), metadata.Prefix);
                    var parser = new ODataUriParser(model, serviceRoot, uri);
                    var odataPath = parser.ParsePath();
                    if (odataPath != null)
                    {
                        odataFeature.RoutePrefix = metadata.Prefix;
                        odataFeature.Model = model;
                        odataFeature.Path = odataPath;

                        //MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                    }
                    else
                    {
                        candidates.SetValidity(i, false);
                    }
                }
                catch (ODataUnrecognizedPathException)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    odataFeature.Model = model;
                    odataFeature.Path = new ODataPath(new UnrecognizedPathSegment());
                }
            }
            return Task.CompletedTask;
        }
    }
}
