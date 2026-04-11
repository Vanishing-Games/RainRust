using System.Collections.Generic;

namespace Core
{
    public class SaveContainer
    {
        public SaveContainer()
        {
            Meta = new SaveMeta("default");
            Data = new Dictionary<string, object>();
        }

        public SaveMeta Meta;
        public Dictionary<string, object> Data;
    }
}
