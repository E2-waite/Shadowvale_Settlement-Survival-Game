using UnityEngine;

namespace Shadowvale.World.Data
{
    public class ChunkData
    {
        public Vector2 Position { get; private set; } // World position
        public Vector2Int GridPos { get; private set; } // Grid position
        public ChunkTerrainData Terrain { get; private set; }
        public ChunkResourceData Resources { get; private set; }

        public int Size { get; private set; }

        public ChunkData(int size, Vector2 pos, Vector2Int gridPos)
        {
            Size = size;
            Position = pos;
            GridPos = gridPos;
            Terrain = new ChunkTerrainData(size);
            Resources = new ChunkResourceData(size);
        }
    }
}