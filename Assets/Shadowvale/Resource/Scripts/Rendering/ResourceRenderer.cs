using Shadowvale.World.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Shadowvale.Resource.Data;

namespace Shadowvale.Resource.Rendering
{
    public static class ResourceRenderer
    {
        private const int MAX_BATCH = 1023;
        private static Matrix4x4[] batchBuffer = new Matrix4x4[MAX_BATCH];

        public static void Render(ChunkResourceData resources)
        {
            List<int> ids = resources.ids;
            if (ids.Count == 0) return;

            // Unity's DrawMeshInstanced limit is 1023 matrices per draw call.
            int batchCount = 0;
            ResourceConfig lastConfig = null;
            int lastSize = -1;
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                ResourceNode node = resources.Nodes[id];
                if (node == null) continue;

                if (lastConfig != null && lastSize != -1 && (node.Config != lastConfig || node.nodeSize != lastSize || batchCount == MAX_BATCH))
                {
                    DrawBatch(batchCount, lastConfig, lastSize);
                    batchCount = 0;
                }

                batchBuffer[batchCount++] = resources.Matrices[id];
                lastConfig = node.Config;
                lastSize = node.nodeSize;
            }

            if (batchCount > 0 && lastConfig != null && lastSize != -1)
            {
                DrawBatch(batchCount, lastConfig, lastSize);
            }
        }

        private static void DrawBatch(int count, ResourceConfig config, int size)
        {
            ResourceSize resourceSize = config.sizes[size];
            if (resourceSize == null || resourceSize.Mesh == null || config.materials.Count == 0) return;

            Mesh mesh = resourceSize.Mesh;

            for (int i = 0; i < Mathf.Min(config.materials.Count, mesh.subMeshCount); i++)
            {
                if (config.materials[i] == null) continue;
                Graphics.DrawMeshInstanced(
                    mesh,
                    i,
                    config.materials[i],
                    batchBuffer,
                    count,
                    null,
                    ShadowCastingMode.On
                    );
            }
        }
    }
}
