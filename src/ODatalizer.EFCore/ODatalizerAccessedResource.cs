using System.Collections.Generic;

namespace ODatalizer.EFCore
{
    public class ODatalizerAccessedResource
    {
        public string Name { get; set; }

        public string Operation { get; set; } = "Read";
        public List<string> Properties { get; set; } = new List<string>();
    }
}