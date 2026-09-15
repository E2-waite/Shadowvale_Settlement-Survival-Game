using Shadowvale.World.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Shadowvale.World.Runtime
{
    public class MeshGenerator
    {
        private class ChunkFace
        {
            public Vector3[] vertices = new Vector3[4];
            public int[] indices = new int[4];
        }

        // Tile type sub-meshes
        private class TypeMesh
        {
            public List<int> triangles = new List<int>();
        }

        public void Generate(Chunk chunk, WorldProperties properties)
        {
            int size = chunk.Data.Size;
            chunk.Mesh = new Mesh();
            List<Vector3> vertexList = new List<Vector3>();
            List<TypeMesh> typeMeshes = new List<TypeMesh>();
            List<Vector2> uvList = new List<Vector2>();

            foreach (TileConfig tileType in properties.tileTypes)
            {

                typeMeshes.Add(ConstructTypeMesh(chunk, tileType, vertexList, size));


                chunk.Mesh.Clear();

                chunk.Mesh.SetVertices(vertexList);
                chunk.Mesh.subMeshCount = typeMeshes.Count;

                for (int i = 0; i < typeMeshes.Count; i++)
                {
                    chunk.Mesh.SetTriangles(typeMeshes[i].triangles, i);
                }
                chunk.Mesh.RecalculateNormals();
                chunk.Mesh.RecalculateBounds();


                Material[] materials = new Material[properties.tileTypes.Count];

                for (int i = 0; i < properties.tileTypes.Count; i++)
                {
                    materials[i] = properties.tileTypes[i].material;
                }

                chunk.MeshRenderer.sharedMaterials = materials;

                chunk.MeshFilter.sharedMesh = chunk.Mesh;
            }
        }

        private TypeMesh ConstructTypeMesh(Chunk chunk, TileConfig tileType, List<Vector3> vertexList, int size)
        {
            TypeMesh typeMesh = new TypeMesh();

            for (int x = 1; x < size + 1; x++)
            {
                for (int y = 1; y < size + 1; y++)
                {
                    TileData tile = chunk.Data.Terrain.GetTile(new Vector2Int(x, y));

                    if (tile == null || tile.Object == null) continue;

                    if (tile.Object != tileType)
                        continue;

                    ConstructFace(tile, typeMesh, vertexList);

                    // Construct side faces
                    if (tile.Object.topoType == TopoType.Stepped)
                    {
                        ConstructStepFaces(tile, typeMesh, vertexList);
                    }
                }

            }

            return typeMesh;
        }

        private void ConstructFace(TileData tile, TypeMesh typeMesh, List<Vector3> vertexList)
        {
            if (tile.Object == null) return;

            // Contruct top face
            int[] faceIndices = new int[4];
            for (int v = 0; v < 4; v++)
            {
                faceIndices[v] = vertexList.Count;
                vertexList.Add(tile.Vertices[v].position);
            }

            float diag1 = Mathf.Abs(tile.Vertices[0].position.y - tile.Vertices[2].position.y);
            float diag2 = Mathf.Abs(tile.Vertices[1].position.y - tile.Vertices[3].position.y);
            if (diag1 < diag2)
            {
                // Use A-D diagonal
                typeMesh.triangles.Add(faceIndices[0]);
                typeMesh.triangles.Add(faceIndices[1]);
                typeMesh.triangles.Add(faceIndices[3]);

                typeMesh.triangles.Add(faceIndices[1]);
                typeMesh.triangles.Add(faceIndices[2]);
                typeMesh.triangles.Add(faceIndices[3]);
            }
            else
            {
                // Use B-C diagonal
                typeMesh.triangles.Add(faceIndices[0]);
                typeMesh.triangles.Add(faceIndices[1]);
                typeMesh.triangles.Add(faceIndices[2]);

                typeMesh.triangles.Add(faceIndices[0]);
                typeMesh.triangles.Add(faceIndices[2]);
                typeMesh.triangles.Add(faceIndices[3]);
            }

        }

        private void ConstructStepFaces(TileData tile, TypeMesh typeMesh, List<Vector3> vertexList)
        {
            List<ChunkFace> stepFaces = new List<ChunkFace>();

            // Calculate face vertices
            for (int v = 0; v < 4; v++)
            {
                int next = (v + 1) % 4;

                if (tile.StepVertices[v] == null || tile.StepVertices[next] == null) // No face
                    continue;

                ChunkFace face = new ChunkFace();

                face.vertices[0] = tile.StepVertices[v].position;
                face.vertices[1] = tile.Vertices[v].position;
                face.vertices[2] = tile.StepVertices[next].position;
                face.vertices[3] = tile.Vertices[next].position;
                stepFaces.Add(face);
            }

            // Calculate face indices/quads
            for (int i = 0; i < stepFaces.Count; i++)
            {
                ChunkFace face = stepFaces[i];
                if (face == null) continue;

                for (int v = 0; v < 4; v++)
                {
                    face.indices[v] = vertexList.Count;
                    vertexList.Add(face.vertices[v]);
                }

                CreateQuad(
                    typeMesh.triangles,
                    face.indices[0],
                    face.indices[1],
                    face.indices[2],
                    face.indices[3]);
            }
        }

        private void CreateQuad(List<int> triangles, int topLeft, int topRight, int btmLeft, int btmRight)
        {
            triangles.Add(topLeft);
            triangles.Add(btmLeft);
            triangles.Add(btmRight);

            triangles.Add(topLeft);
            triangles.Add(btmRight);
            triangles.Add(topRight);
        }
    }
}
