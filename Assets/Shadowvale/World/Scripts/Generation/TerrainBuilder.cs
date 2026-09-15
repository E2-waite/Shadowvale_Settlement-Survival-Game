using Shadowvale.World.Data;
using Shadowvale.World.Runtime;
using UnityEngine;

namespace Shadowvale.World.Generation
{
    public class TerrainBuilder
    {
        [SerializeField] private WorldManager world;
        MeshGenerator meshGenerator = new();
        public TerrainBuilder(WorldManager world)
        {
            this.world = world;
        }
 
        public Chunk Generate(Vector2Int position)
        {
            Chunk chunk = world.SpawnChunk();
            chunk.name = "Chunk (" + position.x + ":" + position.y + ")";
            chunk.Init(world.Properties, position);

            CalculateNoise(chunk.Data.Terrain, chunk.Data.Position);
            BuildVertices(chunk.Data.Terrain);
            
            world.TileBuilder.BuildTiles(chunk.Data);

            meshGenerator.Generate(chunk, world.Properties);
            

            world.Data.Chunks[position] = chunk;
            chunk.transform.parent = world.transform;
            chunk.Collider.sharedMesh = chunk.Mesh;

            world.ResourceBuilder.Build(chunk.Data);

            return chunk;
        }

        // Calculates chunk's terrain noise
        private void CalculateNoise(ChunkTerrainData terrain, Vector2 position)
        {
            for (int x = 0; x < terrain.Size + 3; x++)
            {
                for (int y = 0; y < terrain.Size + 3; y++)
                {
                    terrain.Noise[x, y] = Noise.GetNoise(x - 1 + position.x, y - 1 + position.y, world.Properties.noiseScale, world.Properties.SeedOffset) - world.Properties.seaLevel;
                    if (terrain.Noise[x, y] > 0) terrain.Noise[x, y] *= world.Properties.heightMultiplier;
                    terrain.Noise[x, y] = Mathf.Clamp01(terrain.Noise[x, y]);
                }
            }
        }

        // Create vertex grid
        private void BuildVertices(ChunkTerrainData terrain)
        {
            for (int x = 0; x < terrain.Size + 3; x++)
            {
                for (int z = 0; z < terrain.Size + 3; z++)
                {
                    terrain.Vertices[x, z] = new ChunkVertex(new Vector3((x - 1), 0, (z - 1)));
                }
            }
        }
    }
}