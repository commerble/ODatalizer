using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ODatalizer.EFCore;
using System;
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
                var scopes = claim.Value.Split(' ');
                var needs = resource.AccessedResources.Select(clrName => $"{requirement.Name}:{clrName}".ToLower());
                if (needs.All(scope => scopes.Contains(scope)))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
