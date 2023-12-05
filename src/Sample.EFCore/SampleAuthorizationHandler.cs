using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ODatalizer.EFCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.EFCore
{
    public class SampleAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, ODatalizerAuthorizationInfo>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, ODatalizerAuthorizationInfo resource)
        {
            
            var claim = context.User.FindFirst("scope");

            if (claim != null)
            {
                var scopes = ParseScope(claim.Value);
                if (CheckScope(scopes, requirement, resource.AccessedResources))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }

        bool CheckScope(IDictionary<string, List<string>> scopes, OperationAuthorizationRequirement requirement, IEnumerable<ODatalizerAccessedResource> resources)
        {
            foreach (var resource in resources)
            {
                var name = $"{resource.Operation}:{resource.Name}".ToLower();
                if (!scopes.ContainsKey(name))
                {
                    return false;
                }
                var disallows = scopes[name];
                if (disallows.Count == 0)
                {
                    // It can access all props
                    continue;
                }

                if (resource.Properties.Count == 0)
                {
                    // Request expect authority to access all properties but it has any disallow props.
                    return false;
                }

                if (resource.Properties.Any(prop => disallows.Contains(prop.ToLower())))
                {
                    // hit disallow prop
                    return false;
                }
            }

            return true;
        }

        IDictionary<string, List<string>> ParseScope(string scope)
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var item in scope.Split(' '))
            {
                var m = item.First();
                if (m == '+')
                {
                    result.Add(item.Substring(1), new List<string>());
                }
                else if (m == '-') 
                {
                    var nameAndProp = item.Substring(1).Split('#');
                    var name = nameAndProp[0];
                    var prop = nameAndProp[1];
                    result[name].Add(prop);
                }
            }
            return result;
        }
    }
}
