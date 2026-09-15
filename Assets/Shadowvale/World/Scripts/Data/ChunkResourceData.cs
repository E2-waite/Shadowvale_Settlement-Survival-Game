using Shadowvale.Resource.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Shadowvale.World.Data
{
    public class ChunkResourceData
    {
        public List<int> ids = new List<int>();
        public Matrix4x4[] Matrices { get; private set; }
        public Vector2 seedOffset;
        private int count;
        public ResourceNode[] Nodes { get; private set; }
        private int size = 0;

        public ChunkResourceData(int size)
        {
            this.size = size;
            count = size * size;
            Nodes = new ResourceNode[count];
            Matrices = new Matrix4x4[count];
        }

        public void SetNode(int index, ResourceNode node)
        {
            Nodes[index] = node;
        }

        public ResourceNode GetNode(int x, int y)
        {
            int index = y * size + x;
            if (index < 0 || index >= Nodes.Length) return null;
            else return Nodes[index];
        }
    }
}
