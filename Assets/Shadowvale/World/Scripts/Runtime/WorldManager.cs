using UnityEngine;
using System;
using Shadowvale.World.Data;
using Shadowvale.World.Generation;
using Shadowvale.Resource.Rendering;

namespace Shadowvale.World.Runtime
{
    /// <summary>
    /// Represents the current game world, including its configuration and runtime data.
    /// Responsible for generating and managing the world's state.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        public WorldSystem System { get; private set; }
        public WorldData Data { get; private set; }
        public TerrainBuilder TerrainBuilder { get; private set; }
        public TileBuilder TileBuilder { get; private set; }
        public ResourceBuilder ResourceBuilder { get; private set; }
        public WorldProperties Properties => properties;
        [SerializeField] private WorldProperties properties;
        [SerializeField] private Chunk chunkPrefab;

        public void Generate()
        {
            Clear();
            properties?.Init();
            System = new WorldSystem(this);
            TerrainBuilder = new TerrainBuilder(this);
            TileBuilder = new TileBuilder(this);
            ResourceBuilder = new ResourceBuilder(this);
            Data = new WorldData();

            for (int x = 0; x < properties.worldSize.x; x++)
            {
                for (int y = 0; y < properties.worldSize.y; y++)
                {
                    TerrainBuilder.Generate(new Vector2Int(x, y));
                }
            }
        }

        public void Update()
        {
            if (System != null && System.ActiveChunks != null)
            {
                foreach (Vector2Int position in System.ActiveChunks)
                {
                    Data.Chunks.TryGetValue(position, out Chunk chunk);
                    ResourceRenderer.Render(chunk.Data.Resources);
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Data = null;
        }

        public Chunk SpawnChunk()
        {
            return Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
