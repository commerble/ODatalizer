namespace ODatalizer.EFCore.Routing
{
    public class ODatalizerControllerNameAccessor
    {
        public ODatalizerControllerNameAccessor(string name) 
        {
            ControllerName = name;
        }

        public string ControllerName { get; private set; }
    }
}
