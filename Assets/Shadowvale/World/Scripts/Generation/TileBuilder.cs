using Shadowvale.World.Data;
using Shadowvale.World.Runtime;
using UnityEngine;

namespace Shadowvale.World.Generation
{
    public class TileBuilder
    {
        WorldManager world;
        public TileBuilder(WorldManager world)
        {
            this.world = world;
        }

        // Build and assign tiles
        public void BuildTiles(ChunkData chunk)
        {
            if (world.Properties == null || world.Data == null) return;

            int chunkSize = world.Properties.chunkSize;
            // Calculate 
            for (int x = 0; x < chunkSize + 2; x++)
            {
                for (int y = 0; y < chunkSize + 2; y++)
                {
                    Vector2Int tilePos = new Vector2Int(x, y);
                    TileData tile = BuildTile(chunk, tilePos);

                    chunk.Terrain.SetTile(tile, tilePos);

                    // Assign tiles to tile grid
                    if (x > 0 && y > 0 && x < chunkSize + 1 && y < chunkSize + 1)
                    {
                        Vector2Int gridPos = new Vector2Int(chunk.GridPos.x * chunkSize + x - 1, chunk.GridPos.y * chunkSize + y - 1);
                        world.Data.Tiles[gridPos] = tile;
                    }
                }
            }

            CalculateSmooth(chunk);
            CalculateSlopes(chunk);
            CalculateSteps(chunk);
        }

        private TileData BuildTile(ChunkData chunk, Vector2Int pos)
        {
            TileData tile = new TileData(pos, chunk);

            for (int i = 0; i < 4; i++)
            {
                Vector2Int vPos = tile.VertexPosition(i);

                if (vPos.x < 0 || vPos.y < 0 || vPos.x >= chunk.Terrain.Noise.GetLength(0) || vPos.y >= chunk.Terrain.Noise.GetLength(1))
                    continue;

                tile.Noise[i] = chunk.Terrain.Noise[vPos.x, vPos.y];
            }

            AssignObject(tile);
            AssignVertices(tile, chunk.Terrain.Vertices);

            Vector3 chunkOffset = new Vector3(chunk.Position.x, 0, chunk.Position.y);
            // Offset by -1 to accound for padding
            Vector3 worldPos = chunkOffset + new Vector3(pos.x - 1, 0, pos.y - 1);
            //worldPos *= world.Properties.tileScale;
            worldPos.y = tile.Object.WorldHeight * world.Properties.heightScale;

            tile.WorldPosition = worldPos;
            tile.Center = new Vector3(worldPos.x + .5f, worldPos.y, worldPos.z + .5f);
            tile.IsFlat = tile.Object.topoType == TopoType.Stepped || tile.Object.topoType == TopoType.Sloped;

            return tile;
        }

        // Assign tile type based on average noise value
        private void AssignObject(TileData tile)
        {
            float tileNoise = tile.AverageNoise();

            if (world.Properties.tileTypes.Count == 0) return;

            for (int i = 0; i < world.Properties.tileTypes.Count; i++)
            {
                TileConfig tileObject = world.Properties.tileTypes[i];
                if (tileNoise <= tileObject.noiseThesh)
                {
                    tile.Object = tileObject;
                    break;
                }
            }
        }
        
        private void AssignVertices(TileData tile, ChunkVertex[,] vertices)
        {
            if (tile.Object == null) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2Int offset = Defs.corners[i];

                Vector2Int vPos = new Vector2Int(tile.Position.x + offset.x, tile.Position.y + offset.y);

                if (vPos.x < 0 || vPos.x >= vertices.GetLength(0) ||
                    vPos.y < 0 || vPos.y >= vertices.GetLength(1))
                    continue;

                ChunkVertex vertex = vertices[vPos.x, vPos.y];

                if (tile.Object.topoType == TopoType.Sloped || tile.Object.topoType == TopoType.Smooth)
                {
                    tile.Vertices[i] = vertex;
                }
                else if (tile.Object.topoType == TopoType.Stepped)
                {
                    tile.Vertices[i] = new ChunkVertex(new Vector3(vertex.position.x, tile.Object.WorldHeight * world.Properties.heightScale, vertex.position.z));
                }
            }
        }

        private void CalculateSlopes(ChunkData chunk)
        {
            for (int x = 1; x < world.Properties.chunkSize + 1; x++)
            {
                for (int y = 1; y < world.Properties.chunkSize + 1; y++)
                {
                    TileData tile = chunk.Terrain.GetTile(new Vector2Int(x, y));
                    if (tile == null || tile.Object == null || tile.Object.topoType != TopoType.Sloped && tile.Object.topoType != TopoType.Smooth) continue;

                    bool flat = true;

                    for (int v = 0; v < 4; v++)
                    {
                        TileConfig lowestObj = null;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int cornerV = Defs.corners[v];
                            Vector2Int cornerI = Defs.corners[i];
                            Vector2Int otherPos = tile.Position + (cornerV - cornerI);

                            TileData otherTile = chunk.Terrain.GetTile(otherPos);
                            if (otherTile == null || otherTile.Object == null) continue;

                            if (lowestObj == null || otherTile.Object.Index < lowestObj.Index)
                            {
                                lowestObj = otherTile.Object;
                            }
                        }

                        float vHeight = 0;

                        if (lowestObj != null)
                        {
                            vHeight = lowestObj.WorldHeight;
                            if (vHeight < tile.Object.WorldHeight) flat = false;
                        }

                        if (tile.Object.topoType == TopoType.Sloped || (tile.Object.topoType == TopoType.Smooth && lowestObj.topoType != TopoType.Smooth))
                            tile.Vertices[v].SetHeight(vHeight * world.Properties.heightScale);
                    }

                    tile.IsFlat = flat;
                }
            }
        }

        // Calculate the stepped vertices based on adjacent tiles 
        private void CalculateSteps(ChunkData chunk)
        {
            for (int x = 1; x < world.Properties.chunkSize + 1; x++)
            {
                for (int y = 1; y < world.Properties.chunkSize + 1; y++)
                {
                    TileData tile = chunk.Terrain.GetTile(new Vector2Int(x, y));
                    if (tile == null || tile.Object == null) continue;

                    if (tile.Object.topoType != TopoType.Stepped) // Skip non-stepped tiles
                        continue;

                    // First check adjacent tiles
                    foreach(Vector2Int dir in Defs.adjacent)
                    {
                        TileData neighbor = chunk.Terrain.GetTile(new Vector2Int(tile.Position.x + dir.x, tile.Position.y + dir.y));

                        if (neighbor == null) continue;

                        if (neighbor.Object.Index < tile.Object.Index)
                        {
                            int[] adjacentVerts = AdjacentVerts(dir);

                            for (int i = 0; i < 2; i++)
                            {
                                int v = adjacentVerts[i];

                                if (tile.StepVertices[v] == null)
                                {
                                    tile.StepVertices[v] = new ChunkVertex(
                                        new Vector3(
                                            tile.Vertices[v].position.x,
                                            neighbor.Object.WorldHeight * world.Properties.heightScale,
                                            tile.Vertices[v].position.z));
                                }
                                else
                                {
                                    ChunkVertex vertex = tile.StepVertices[v];

                                    vertex.SetHeight(neighbor.Object.WorldHeight * world.Properties.heightScale);
                                }
                            }
                        }
                    }

                    // Check diagonals
                    for (int v = 0; v < 4; v++)
                    {
                        ChunkVertex vertex = tile.StepVertices[v];
                        if (vertex == null) continue;

                        Vector2Int dir = Defs.diagonal[v];
                        TileData neighbor = chunk.Terrain.GetTile(new Vector2Int(tile.Position.x + dir.x, tile.Position.y + dir.y));
                        if (neighbor == null) continue;

                        if (neighbor.Object.Index < tile.Object.Index)
                        {
                            vertex.SetHeight(neighbor.Object.WorldHeight * world.Properties.heightScale);
                        }
                    }
                }
            }
        }

        private int[] AdjacentVerts(Vector2Int dir)
        {
            if (dir == Vector2Int.up)
                return new int[2] { 1, 2 };
            else if (dir == Vector2Int.right)
                return new int[2] { 2, 3 };
            else if (dir == Vector2Int.down)
                return new int[2] { 3, 0 };
            else
                return new int[2] { 0, 1 };
        }

        private void CalculateSmooth(ChunkData chunk)
        {
            for (int x = 1; x < world.Properties.chunkSize + 1; x++)
            {
                for (int y = 1; y < world.Properties.chunkSize + 1; y++)
                {
                    TileData tile = chunk.Terrain.GetTile(new Vector2Int(x, y));
                    if (tile == null || tile.Object == null) continue;

                    if (tile.Object.topoType != TopoType.Smooth)
                        continue;

                    for (int v = 0; v < 4; v++)
                    {
                        Vector2Int offsetPos = tile.Position + Defs.corners[v];
                        float noise = chunk.Terrain.Noise[offsetPos.x, offsetPos.y];
                        float height = noise;

                        if (tile.Object.Prev != null)
                        {
                            noise -= tile.Object.Prev.noiseThesh;

                            height =
                                tile.Object.Prev.WorldHeight +
                                noise * tile.Object.height;
                        }

                        tile.Vertices[v].SetHeight(height * world.Properties.heightScale);
                    }
                }
            }
        }
    }
}