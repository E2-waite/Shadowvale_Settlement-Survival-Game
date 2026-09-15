using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shadowvale.World.Data
{
    [CreateAssetMenu(fileName = "WorldProperties", menuName = "Scriptable Objects/WorldProperties")]
    public class WorldProperties : ScriptableObject
    {
        public List<TileConfig> tileTypes = new List<TileConfig>();
        [Range(0.001f, .3f)] public float noiseScale = 0.05f;
        [Range(0.001f, .3f)] public float resourceScale = 0.05f;
        [Range(0.01f, 10f)] public float heightMultiplier = 2.5f;
        [Range(0.01f, 10f)] public float heightScale = 1f;
        [Range(0.01f, 1f)] public float seaLevel = 0.35f;
        [Range(1, 100)] public int chunkSize = 20;
        [Range(1, 10)] public int streamDist = 2; 
        public Vector2Int worldSize = new Vector2Int(1, 1);
        public Vector2 SeedOffset = new Vector2(100000f, 100000f);

        public void Init()
        {
            for (int i = 0; i < tileTypes.Count; i++)
            {
                TileConfig prev = null;
                if (i > 0) prev = tileTypes[i - 1];
                tileTypes[i]?.Init(i, prev);
            }
        }
    }
}
