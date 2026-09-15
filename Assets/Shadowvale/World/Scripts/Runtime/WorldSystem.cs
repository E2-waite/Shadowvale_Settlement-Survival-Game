using Shadowvale.World.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Shadowvale.World.Runtime
{
    /// <summary>
    /// System class for handling chunk streaming
    /// </summary>
    public class WorldSystem
    {
        public HashSet<Vector2Int> ActiveChunks { get; private set; }
        private ChunkData lastChunk = null;
        private Vector2Int lastPos = Vector2Int.zero;
        private HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        private WorldManager world;

        public WorldSystem(WorldManager worldManager)
        {
            world = worldManager;
            ActiveChunks = new HashSet<Vector2Int>();
        }

        // Streams surrounding chunks, enabling/creating valid chunks and disabling invalid chunks
        public void StreamChunks(ChunkData chunk)
        {
            // TODO: clear far chunks from memory (serialize and destroy GameObject)
            if (chunk == null || chunk == lastChunk) return; // Ignore if already handled or null
            lastChunk = chunk;

            requiredChunks.Clear();

            int dist = 1;

            if (world.Properties != null)
            {
                dist = world.Properties.streamDist;
            }

            for (int x = chunk.GridPos.x - dist; x <= chunk.GridPos.x + dist; x++)
            {
                for (int y = chunk.GridPos.y - dist; y <= chunk.GridPos.y + dist; y++)
                {
                    requiredChunks.Add(new Vector2Int(x, y));
                }
            }

            // Activate/generate required chunks
            foreach (Vector2Int pos in requiredChunks)
            {
                if (!world.Data.Chunks.TryGetValue(pos, out Chunk other))
                {
                    other = world.TerrainBuilder.Generate(pos);
                }

                if (!other.isActiveAndEnabled)
                {
                    other.gameObject.SetActive(true);
                }
            }

            // Disable non-required chunks
            foreach (Vector2Int pos in ActiveChunks)
            {
                if (requiredChunks.Contains(pos)) continue;
                world.Data.Chunks[pos]?.gameObject.SetActive(false);
            }

            ActiveChunks.Clear();
            ActiveChunks.UnionWith(requiredChunks);
        }

    }
}
