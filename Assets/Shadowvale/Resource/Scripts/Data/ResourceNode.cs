using UnityEngine;

namespace Shadowvale.Resource.Data
{
    public class ResourceNode
    {
        public int Index => index;
        public ResourceConfig Config => config;
        private int index = 0;
        private ResourceConfig config;
        public int nodeSize = 0;

        public ResourceNode(ResourceConfig config, int index, int size)
        {
            this.config = config;
            this.index = index;
            nodeSize = size;
        }
    }
}
