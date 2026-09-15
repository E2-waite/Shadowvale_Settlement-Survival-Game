using UnityEngine;
using Shadowvale.World.Data;

namespace Shadowvale.World.Runtime
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        public Mesh Mesh { get; set; }
        public MeshFilter MeshFilter { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }
        public MeshCollider Collider { get; private set; }
        [SerializeField] private MeshFilter meshFilter;

        public ChunkData Data { get; private set; }

        public void Init(WorldProperties properties, Vector2Int pos)
        {
            Vector2 worldPos = new Vector2(pos.x * properties.chunkSize, pos.y * properties.chunkSize);

            Data = new ChunkData(properties.chunkSize, worldPos, pos);

            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            Collider = GetComponent<MeshCollider>();
            transform.position = new Vector3(worldPos.x, 0, worldPos.y);
        }
    }
}