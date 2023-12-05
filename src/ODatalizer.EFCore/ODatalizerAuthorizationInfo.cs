using System.Collections.Generic;
using System.Linq;

namespace ODatalizer.EFCore
{
    public class ODatalizerAuthorizationInfo
    {
        public List<ODatalizerAccessedResource> AccessedResources = new List<ODatalizerAccessedResource>();

        public void Add(string name)
        {
            AccessedResources.Add(new ODatalizerAccessedResource { Name = name });
        }

        public void Add(string name, string operation)
        {
            AccessedResources.Add(new ODatalizerAccessedResource { Name = name, Operation = operation });
        }

        public void BindProp(string propName)
        {
            AccessedResources.LastOrDefault()?.Properties.Add(propName);
        }

        public void BindProps(IEnumerable<string> propNames)
        {
            AccessedResources.LastOrDefault()?.Properties.AddRange(propNames);
        }

        public void SetLastOperation(string operation)
        {
            var last = AccessedResources.LastOrDefault();
            if (last != null)
            {
                last.Operation = operation;
            }
        }
    }
}