using System.Collections.Generic;
using UnityEngine;
using Shadowvale.Resource.Data;

namespace Shadowvale.World.Data
{
    [CreateAssetMenu(fileName = "New Tile Def", menuName = "World Builder/Tile")]
    public class TileConfig : ScriptableObject
    {
        public List<ResourceConfig> resourceTypes = new List<ResourceConfig>();
        public SurfaceType surfaceType;
        public TopoType topoType;
        public Material material;
        [Range(0f, 1f)] public float noiseThesh = 0f;
        [Range(1f, 10f)] public float noiseMulti = 1f;
        [Range(0f, 10f)] public float height = 0f;
        public float WorldHeight => worldHeight;
        public int Index => index;
        private int index = 0;
        public TileConfig Prev => prevObject;
        private TileConfig prevObject; // The previous object (lower tile)
        private float worldHeight; // The total world height
        public bool walkable = true;

        public void Init(int index, TileConfig prev)
        {
            this.index = index;
            prevObject = prev;
            if (prevObject == null)
                worldHeight = height;
            else
                worldHeight = prev.WorldHeight + height;
        }
    }
}
