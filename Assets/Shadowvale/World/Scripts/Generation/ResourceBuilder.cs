using Shadowvale.World.Data;
using Shadowvale.World.Runtime;
using UnityEngine;
using Shadowvale.Resource.Data;

namespace Shadowvale.World.Generation
{
    public class ResourceBuilder
    {
        private WorldManager world;

        public ResourceBuilder(WorldManager world)
        {
            this.world = world;
        }

        public void Build(ChunkData chunk)
        {
            int index = 0;

            for (int x = 0; x < chunk.Size; x++)
            {
                for (int z = 0; z < chunk.Size; z++)
                {
                    // Offset by 1 to account for padding
                    TileData tile = chunk.Terrain.GetTile(x + 1, z + 1);

                    if (tile.Object.resourceTypes.Count == 0) continue;

                    foreach (ResourceConfig config in tile.Object.resourceTypes)
                    {
                        if (config == null) continue;

                        float noise = Noise.GetNoise(x - 1 + chunk.Position.x, z - 1 + chunk.Position.y, world.Properties.resourceScale, config.noiseOffset);


                        if (noise > config.noiseThresh)
                        {
                            float rand = Random.Range(0f, 1f);

                            //float thresh = 1f - resourceConfig.rate;

                            if (config.sizes.Count > 0)
                            {
                                ResourceNode node = new ResourceNode(config, index, Random.Range(0, config.sizes.Count));
                                chunk.Resources.SetNode(index, node);
                                chunk.Resources.ids.Add(index);
                                UpdateMatrix(chunk.Resources, node, tile, index);
                                index++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateMatrix(ChunkResourceData resources, ResourceNode node, TileData tile, int id)
        {
            ResourceConfig config = node.Config;
            if (config == null) return;

            float heightVariation = config.heightVariation * config.height;
            float widthVariation = config.widthVariation * config.width;
            float scaleVariation = config.scaleVariation * config.scale;

            float height = Random.Range(config.height - heightVariation, config.height + heightVariation);
            float width = Random.Range(config.width - widthVariation, config.width + widthVariation);
            Vector3 scale = new Vector3(width, height, width) * Random.Range(config.scale - scaleVariation, config.scale + scaleVariation);


            //scale *= properties.tileScale; // Scale to match tile scale
            scale *= node.Config.sizes[node.nodeSize].Scale; // Scale to match node size's scale

            Vector3 pos = tile.Center;
            Vector3 offset = Vector3.zero;
            offset.x = Random.Range(-config.offsetVariation, config.offsetVariation);
            offset.z = Random.Range(-config.offsetVariation, config.offsetVariation);
            //offset *= properties.tileScale;
            pos += offset;

            Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            resources.Matrices[id] =
                Matrix4x4.TRS(
                    pos,
                    rotation,
                    scale
                );
        }
    }
}
