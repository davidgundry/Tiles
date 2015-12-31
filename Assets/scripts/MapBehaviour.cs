using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MapBehaviour : MonoBehaviour, Clickable
{
    public Transform treePrefab;

    private Map map;
	public Map Map {
		get {
			return map;
		}
	}
    private Transform tileMarker;
    private const int verticesPerTile = 24;
	private MeshFilter mf;
    private MeshFilter bmf;
    private MeshFilter wmf;
	private Vector3[] verts;
    private Vector3[] waterVerts;
	private int scale = 2;

    public void NewMap(Map map)
    {
        this.map = map;
        transform.position = new Vector3(map.X, 0, map.Z);
        SetMapVertices();
        SetMapTextures();
    }

    void Start () {
		tileMarker = GameObject.Find ("TileMarker").transform;
		mf = GetComponent<MeshFilter>();
        bmf = transform.GetChild(0).GetComponent<MeshFilter>();
        wmf = transform.GetChild(1).GetComponent<MeshFilter>();
        wmf.mesh.bounds = new Bounds(new Vector3(map.X + map.Width/2,0,map.Z+map.Height/2), new Vector3(map.Width, 256, map.Height));
        verts = mf.mesh.vertices;
        waterVerts = wmf.mesh.vertices;
    }

    void UpdateTile(int i, int j, out bool someDirtyMesh, out bool someDirtyWater)
    {
        someDirtyMesh = false;
        someDirtyWater = false;

        if (map.Tiles[i, j] != null)
        {
            if (map.Tiles[i, j].Water)
                if (map.Tiles[i, j].Type != TileType.Sand)
                    map.Tiles[i, j].Type = TileType.Sand;

            if (map.Tiles[i, j].Type == TileType.Stone)
            {
                bool sandNearby = false;
                bool treeNearby = false;
                TileType[] neighbours = GetNeighbourTypes(i, j, 1);
                foreach (TileType neighbourType in neighbours)
                {
                    if (neighbourType == TileType.Sand)
                        sandNearby = true;
                    if (neighbourType == TileType.Tree)
                        treeNearby = true;
                }
                if (sandNearby)
                    map.Tiles[i, j].Type = TileType.Grass;
                if (treeNearby)
                    if (map.Tiles[i, j].Type == TileType.Stone)
                        map.Tiles[i, j].Type = TileType.Grass;
            }


            byte[] surroundingHeights = new byte[16];
            byte[] surroundingDepths = new byte[16];
            bool[] surroundingWaterEdgePresence = new bool[16];

            if ((i>=0) && (j>=0) && (i < map.Width) && (j<map.Height))
            {
                if (j > 0)
                {
                    surroundingHeights[0] = map.Tiles[i, j - 1].Heights[7];
                    surroundingHeights[1] = map.Tiles[i, j - 1].Heights[6];
                    surroundingHeights[2] = map.Tiles[i, j - 1].Heights[5];
                    surroundingDepths[0] = map.Tiles[i, j - 1].WaterDepths[7];
                    surroundingDepths[1] = map.Tiles[i, j - 1].WaterDepths[6];
                    surroundingDepths[2] = map.Tiles[i, j - 1].WaterDepths[5];
                    surroundingWaterEdgePresence[0] = map.Tiles[i, j - 1].WaterEdges[7-1];
                    surroundingWaterEdgePresence[1] = map.Tiles[i, j - 1].WaterEdges[6 - 1];
                    surroundingWaterEdgePresence[2] = map.Tiles[i, j - 1].WaterEdges[5 - 1];
                }

                if ((j > 0 ) && (i < map.Width - 1))
                {
                    surroundingHeights[3] = map.Tiles[i + 1, j - 1].Heights[7];
                    surroundingDepths[3] = map.Tiles[i + 1, j - 1].WaterDepths[7];
                }

                if (i < map.Width - 1)
                {
                    surroundingHeights[4] = map.Tiles[i + 1, j].Heights[1];
                    surroundingHeights[5] = map.Tiles[i + 1, j].Heights[8];
                    surroundingHeights[6] = map.Tiles[i + 1, j].Heights[7];
                    surroundingDepths[4] = map.Tiles[i + 1, j].WaterDepths[1];
                    surroundingDepths[5] = map.Tiles[i + 1, j].WaterDepths[8];
                    surroundingDepths[6] = map.Tiles[i + 1, j].WaterDepths[7];
                    surroundingWaterEdgePresence[4] = map.Tiles[i + 1, j].WaterEdges[1 - 1];
                    surroundingWaterEdgePresence[5] = map.Tiles[i + 1, j].WaterEdges[8 - 1];
                    surroundingWaterEdgePresence[6] = map.Tiles[i + 1, j].WaterEdges[7 - 1];
                }

                if ((j < map.Height - 1) && (i < map.Width - 1))
                {
                    surroundingHeights[7] = map.Tiles[i + 1, j + 1].Heights[1];
                    surroundingDepths[7] = map.Tiles[i + 1, j + 1].WaterDepths[1];
                    surroundingWaterEdgePresence[7] = map.Tiles[i + 1, j + 1].WaterEdges[1 - 1];
                }

                if (j < map.Height - 1)
                {
                    surroundingHeights[8] = map.Tiles[i, j + 1].Heights[3];
                    surroundingHeights[9] = map.Tiles[i, j + 1].Heights[2];
                    surroundingHeights[10] = map.Tiles[i, j + 1].Heights[1];
                    surroundingDepths[8] = map.Tiles[i, j + 1].WaterDepths[3];
                    surroundingDepths[9] = map.Tiles[i, j + 1].WaterDepths[2];
                    surroundingDepths[10] = map.Tiles[i, j + 1].WaterDepths[1];
                    surroundingWaterEdgePresence[8] = map.Tiles[i, j + 1].WaterEdges[3 - 1];
                    surroundingWaterEdgePresence[9] = map.Tiles[i, j + 1].WaterEdges[2 - 1];
                    surroundingWaterEdgePresence[10] = map.Tiles[i, j + 1].WaterEdges[1 - 1];
                }

                if ((j < map.Height - 1) && (i > 0))
                {
                    surroundingHeights[11] = map.Tiles[i - 1, j + 1].Heights[3];
                    surroundingDepths[11] = map.Tiles[i - 1, j + 1].WaterDepths[3];
                    surroundingWaterEdgePresence[11] = map.Tiles[i - 1, j + 1].WaterEdges[3 - 1];
                }

                if (i > 0)
                {
                    surroundingHeights[12] = map.Tiles[i - 1, j].Heights[5];
                    surroundingHeights[13] = map.Tiles[i - 1, j].Heights[4];
                    surroundingHeights[14] = map.Tiles[i - 1, j].Heights[3];
                    surroundingDepths[12] = map.Tiles[i - 1, j].WaterDepths[5];
                    surroundingDepths[13] = map.Tiles[i - 1, j].WaterDepths[4];
                    surroundingDepths[14] = map.Tiles[i - 1, j].WaterDepths[3];
                    surroundingWaterEdgePresence[12] = map.Tiles[i - 1, j].WaterEdges[5 - 1];
                    surroundingWaterEdgePresence[13] = map.Tiles[i - 1, j].WaterEdges[4 - 1];
                    surroundingWaterEdgePresence[14] = map.Tiles[i - 1, j].WaterEdges[3 - 1];
                }

                if ((j > 0) && (i > 0))
                {
                    surroundingHeights[15] = map.Tiles[i - 1, j - 1].Heights[5];
                    surroundingDepths[15] = map.Tiles[i - 1, j - 1].WaterDepths[5];
                    surroundingWaterEdgePresence[15] = map.Tiles[i - 1, j - 1].WaterEdges[5 - 1];
                }

                map.Tiles[i, j].SurroundingHeights = surroundingHeights;
                map.Tiles[i, j].SurroundingDepths = surroundingDepths;
                map.Tiles[i, j].SurroundingWaterEdgePresence = surroundingWaterEdgePresence;
                map.Tiles[i, j].RecalculateWater();
            }


            if (map.Tiles[i, j].DirtyMesh)
            {
                someDirtyMesh = true;
                verts = SetTileVertices(verts, i, j, map.Tiles[i, j].Heights);
                map.Tiles[i, j].DirtyMesh = false;
            }
            if (map.Tiles[i, j].DirtyType)
            {
                TextureTile(i, j, TileTypeToTileTexture(map.Tiles[i, j].Type));
                ClutterTile(i, j, map.Tiles[i, j].Type);
                map.Tiles[i, j].DirtyType = false;
            }
            if (map.Tiles[i, j].DirtyWater)
            {
                someDirtyWater = true;
                waterVerts = SetTileVertices(waterVerts, i, j, map.Tiles[i, j].WaterVerticesY);
                map.Tiles[i, j].DirtyWater = false;
            }
        }
    }

    void Update()
    {
        if (map != null)
        {
            bool someDirtyMesh = false;
            bool someDirtyWater = false;

            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                    if (Random.value > 0.95f)
                        UpdateTile(i,j, out someDirtyMesh, out someDirtyWater);

            if (someDirtyMesh)
            {
                mf.mesh.vertices = verts;
                mf.mesh.RecalculateNormals();
                UpdateMeshCollider();
                SetBaseVertices();
            }
            if (someDirtyWater)
            {
                wmf.mesh.vertices = waterVerts;
                wmf.mesh.RecalculateNormals();
            }
        }

        /*if (!updatedTiles)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    tiles[i, j].GetComponent<TileBehaviour>().UpdateNeighbourhood();
            updatedTiles = true;
        }*/
	
    }

    private void TextureTile(int x, int y, TileTexture texture)
    {
        mf.mesh.uv = SetTileUV(mf.mesh.uv, x, y, texture);
    }

    private void ClutterTile(int x, int y, TileType type)
    {
		/*if (map.Tiles [x, y].Clutter != null) {
			Object.Destroy (map.Tiles [x, y].Clutter.gameObject);
			map.Tiles [x, y].Clutter = null;
		}

        switch (type)
        {
            case TileType.Tree:
				map.Tiles[x,y].Clutter = (Transform)Instantiate(treePrefab, new Vector3((x+0.5f)*scale+transform.position.x, map.Tiles[x,y].Heights[0], (y+0.5f)*scale+transform.position.z), Quaternion.identity);
                break;
        }
		if (map.Tiles[x,y].Clutter != null)
			map.Tiles[x,y].Clutter.parent = transform;*/
    }

    private void SetMapVertices()
    {
        mf = GetComponent<MeshFilter>();
        bmf = transform.GetChild(0).GetComponent<MeshFilter>();
        wmf = transform.GetChild(1).GetComponent<MeshFilter>();
        verts = new Vector3[map.Width * map.Height * verticesPerTile];
        waterVerts = new Vector3[map.Width * map.Height * verticesPerTile];

        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
                if (map.Tiles[i, j] != null)
                {
                    SetTileVertices(verts, i, j, map.Tiles[i, j].Heights);
                    //SetTileVertices(waterVerts, i, j, map.Tiles[i, j].WaterHeights);
                }

        mf.mesh.vertices = verts;
        wmf.mesh.vertices = waterVerts;

        int[] newTriangles = new int[mf.mesh.vertices.Length];
        for (int i = 0; i < mf.mesh.vertices.Length; i++)
            newTriangles[i] = ((i / 3) + 1) * 3 - 1 - i % 3;

        mf.mesh.triangles = newTriangles;
        wmf.mesh.triangles = newTriangles;
        mf.mesh.RecalculateNormals();
        wmf.mesh.RecalculateNormals();

        UpdateMeshCollider();

        SetBaseVertices();
    }

    private int TileCoordsToVertexIndex(int x, int y, int r)
    {
        return verticesPerTile * (x + y * map.Width) + r;
    }

    private void UpdateMeshCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mf.mesh;
    }

    private Vector3[] SetTileVertices(Vector3[] vertices, int x, int y, byte[] tileHeight)
    {
        float[] floats = new float[tileHeight.Length];
        for (int i = 0; i < tileHeight.Length; i++)
            floats[i] = tileHeight[i];
        return SetTileVertices(vertices, x, y,floats);
    }

    private Vector3[] SetTileVertices(Vector3[] vertices, int x, int y, float[] tileHeight)
    {
		float halfTileWidth = 0.5f*scale;
		float halfTileDepth = 0.5f*scale;

        int vert = TileCoordsToVertexIndex(x, y, 0);
		x *= scale;
		y *= scale;
        // 0
        vertices[vert] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 1] = new Vector3(x, tileHeight[1], y);
        vertices[vert + 2] = new Vector3(x + halfTileWidth, tileHeight[2], y);

        // 1
        vertices[vert + 3] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 4] = new Vector3(x + halfTileWidth, tileHeight[2], y);
        vertices[vert + 5] = new Vector3(x + halfTileWidth * 2, tileHeight[3], y);

        //2
        vertices[vert + 6] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 7] = new Vector3(x + halfTileWidth * 2, tileHeight[3], y);
        vertices[vert + 8] = new Vector3(x + halfTileWidth * 2, tileHeight[4], y+ halfTileDepth);

        //3
        vertices[vert + 9] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 10] = new Vector3(x + halfTileWidth * 2, tileHeight[4], y+ halfTileDepth);
        vertices[vert + 11] = new Vector3(x + halfTileWidth * 2, tileHeight[5], y+ halfTileDepth * 2);

        //4
        vertices[vert + 12] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 13] = new Vector3(x + halfTileWidth * 2, tileHeight[5], y+ halfTileDepth * 2);
        vertices[vert + 14] = new Vector3(x + halfTileWidth, tileHeight[6], y+ halfTileDepth * 2);

        //5
        vertices[vert + 15] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 16] = new Vector3(x + halfTileWidth, tileHeight[6], y+ halfTileDepth * 2);
        vertices[vert + 17] = new Vector3(x, tileHeight[7], y+ halfTileDepth * 2);

        //6
        vertices[vert + 18] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 19] = new Vector3(x, tileHeight[7], y+ halfTileDepth * 2);
        vertices[vert + 20] = new Vector3(x, tileHeight[8], y+ halfTileDepth);

        //7
        vertices[vert + 21] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        vertices[vert + 22] = new Vector3(x, tileHeight[8], y+ halfTileDepth);
        vertices[vert + 23] = new Vector3(x, tileHeight[1], y);

        return vertices;
    }


    private void SetMapTextures()
    {
        TileTexture[,] textures = new TileTexture[map.Width,map.Height];
        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
                if (map.Tiles[i, j] != null)
                    textures[i,j] = TileTypeToTileTexture(map.Tiles[i,j].Type);

        mf.mesh.uv = UVsFromTextureMap(textures);
    }

    private static TileTexture TileTypeToTileTexture(TileType t)
    {
        switch (t)
        {
            case TileType.Stone:
                return TileTexture.Stone;
            case TileType.Grass:
                return TileTexture.Grass;
            case TileType.Dirt:
                return TileTexture.Dirt;
            case TileType.Sand:
                return TileTexture.Sand;
        }
        return TileTexture.Grass;
    }

    private Vector2[] UVsFromTextureMap(TileTexture[,] texture)
    {
        Vector2[] uvs = new Vector2[map.Width * map.Height * verticesPerTile];

        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
                uvs = SetTileUV(uvs, i, j, texture[i, j]);

        return uvs;
    }

    private static Vector2 TextureToTexCoordinates(TileTexture texture)
    {
        switch (texture)
        {
            case TileTexture.Grass:
                return new Vector2(0.05f, 0.05f);
            case TileTexture.Dirt:
                return new Vector2(0.05f, 0.55f);
            case TileTexture.Stone:
                return new Vector2(0.55f, 0.55f);
            case TileTexture.Sand:
                return new Vector2(0.55f, 0.05f);
        }
        return new Vector2(0f, 0f);
    }

    private Vector2[] SetTileUV(Vector2[] uvs, int x, int y, TileTexture texture)
    {
        int vert = TileCoordsToVertexIndex(x, y, 0);
        float textureSize = 0.4f;
        float halfTextureSize = 0.25f;

        Vector2 texCoordinates = TextureToTexCoordinates(texture);
        float texX = texCoordinates.x;
        float texY = texCoordinates.y;

        // 0
        uvs[vert] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 1] = new Vector2(texX + 0, texY + 0);
        uvs[vert + 2] = new Vector2(texX + halfTextureSize, texY + 0);

        // 1
        uvs[vert + 3] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 4] = new Vector2(texX + halfTextureSize, texY + 0);
        uvs[vert + 5] = new Vector2(texX + textureSize, texY + 0);

        //2
        uvs[vert + 6] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 7] = new Vector2(texX + textureSize, texY + 0);
        uvs[vert + 8] = new Vector2(texX + textureSize, texY + halfTextureSize);

        //3
        uvs[vert + 9] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 10] = new Vector2(texX + textureSize, texY + halfTextureSize);
        uvs[vert + 11] = new Vector2(texX + textureSize, texY + textureSize);

        //4
        uvs[vert + 12] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 13] = new Vector2(texX + textureSize, texY + textureSize);
        uvs[vert + 14] = new Vector2(texX + halfTextureSize, texY + textureSize);

        //5
        uvs[vert + 15] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 16] = new Vector2(texX + halfTextureSize, texY + textureSize);
        uvs[vert + 17] = new Vector2(texX + 0, texY + textureSize);

        //6
        uvs[vert + 18] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 19] = new Vector2(texX + 0, texY + textureSize);
        uvs[vert + 20] = new Vector2(texX + 0, texY + halfTextureSize);

        //7
        uvs[vert + 21] = new Vector2(texX + halfTextureSize, texY + halfTextureSize);
        uvs[vert + 22] = new Vector2(texX + 0, texY + halfTextureSize);
        uvs[vert + 23] = new Vector2(texX + 0, texY + 0);

        return uvs;
    }

    private void SetBaseVertices()
    {
        Vector3[] surfaceVerts = mf.mesh.vertices;

        // Create base besh
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        int tri = 0;
        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
            {
                if (map.Tiles[i, j] != null)
                {
                    if ((i > 0) && (map.Tiles[i - 1, j] != null)) // Left edge
                    {
                        if (map.Tiles[i, j].Heights[1] != map.Tiles[i - 1, j].Heights[3])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 7)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 23)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 22)]);

                            triangles.Add(tri + 2);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 0);
                            tri += 3;
                        }
                        if (map.Tiles[i, j].Heights[8] != map.Tiles[i - 1, j].Heights[4])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 7)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 22)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 8)]);

                            triangles.Add(tri + 2);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 0);

                            tri += 3;

                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 10)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 20)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 11)]);

                            triangles.Add(tri + 2);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 0);

                            tri += 3;
                        }
                        if (map.Tiles[i, j].Heights[7] != map.Tiles[i - 1, j].Heights[5])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 11)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 20)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 19)]);

                            triangles.Add(tri + 2);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 0);
                            tri += 3;
                        }
                    }
                    else
                    {
                        Vector3 v = surfaceVerts[TileCoordsToVertexIndex(i, j, 23)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 23)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 22)]);

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);
                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 23)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 22)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 22)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);

                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 20)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 20)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 19)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);

                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 19)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 20)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 19)]);


                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);
                        tri += 3;
                    }
                    if (i == map.Width - 1)
                    {
                        Vector3 v = surfaceVerts[TileCoordsToVertexIndex(i, j, 7)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 7)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 8)]);

                        triangles.Add(tri + 0);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);
                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 7)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 8)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 8)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 0);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);

                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 10)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 10)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 11)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 0);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);

                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 11)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 10)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 11)]);

                        triangles.Add(tri + 0);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);
                        tri += 3;
                    }

                    if ((j > 0)  && (map.Tiles[i, j-1] != null)) // Top edge
                    {
                        if (map.Tiles[i, j].Heights[1] != map.Tiles[i, j - 1].Heights[7])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 1)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 2)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 19)]);

                            triangles.Add(tri);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 2);
                            tri += 3;
                        }
                        if (map.Tiles[i, j].Heights[2] != map.Tiles[i, j - 1].Heights[6])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 17)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 2)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 16)]);


                            triangles.Add(tri);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 2);

                            tri += 3;

                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 4)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 13)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 14)]);


                            triangles.Add(tri);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 2);

                            tri += 3;
                        }
                        if (map.Tiles[i, j].Heights[3] != map.Tiles[i, j - 1].Heights[5])
                        {
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 4)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 5)]);
                            verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 13)]);

                            triangles.Add(tri);
                            triangles.Add(tri + 1);
                            triangles.Add(tri + 2);
                            tri += 3;
                        }
                    }
                    else
                    {

                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 1)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 2)]);
                        Vector3 v = surfaceVerts[TileCoordsToVertexIndex(i, j, 1)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);
                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 1)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 2)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 2)];
                        verts.Add(new Vector3(v.x, 0, v.z));


                        triangles.Add(tri);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);

                        tri += 3;

                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 4)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 5)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 4)];
                        verts.Add(new Vector3(v.x, 0, v.z));


                        triangles.Add(tri);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);

                        tri += 3;

                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 4)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 5)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 5)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);
                        tri += 3;
                    }
                    if (j == map.Height - 1)
                    {
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 17)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 16)]);
                        Vector3 v = surfaceVerts[TileCoordsToVertexIndex(i, j, 17)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);
                        tri += 3;

                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 17)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 16)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 16)];
                        verts.Add(new Vector3(v.x, 0, v.z));


                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);

                        tri += 3;

                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 14)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 13)];
                        verts.Add(new Vector3(v.x, 0, v.z));
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 16)];
                        verts.Add(new Vector3(v.x, 0, v.z));


                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);

                        tri += 3;

                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 16)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 13)]);
                        v = surfaceVerts[TileCoordsToVertexIndex(i, j, 13)];
                        verts.Add(new Vector3(v.x, 0, v.z));

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);
                        tri += 3;
                    }
                }
            }

        Vector3[] meshVerts = new Vector3[verts.Count];
        for (int i = 0; i < meshVerts.Length; i++)
            meshVerts[i] = verts[i];

        if (bmf != null)
        {
			bmf.mesh.Clear ();
            bmf.mesh.vertices = meshVerts;

			int[] newTriangles = new int[triangles.Count];
            for (int i = 0; i < newTriangles.Length; i++)
                newTriangles[i] = triangles[i];
            bmf.mesh.triangles = newTriangles;

            bmf.mesh.RecalculateNormals();
        }

        MeshCollider cmc = transform.GetChild(0).GetComponent<MeshCollider>();
        if (cmc != null)
        {
            cmc.sharedMesh = null;
            cmc.sharedMesh = mf.mesh;
        }
    }

	/*public void runRiversFrom (int x, int z)
	{

	}

	private byte Mirror(byte b)
	{
		byte o = 0;
		for (int s = 0; s<8; s++) {
			switch (s)
			{
				case 0:if ((b & (1 << s)) == 1) o+=64;break;
				case 1:if ((b & (1 << s)) == 1) o+=32;break;
				case 2:if ((b & (1 << s)) == 1) o+=16;break;
				case 3:if ((b & (1 << s)) == 1) o+=8;break;
				case 4:if ((b & (1 << s)) == 1) o+=4;break;
				case 5:if ((b & (1 << s)) == 1) o+=2;break;
				case 6:if ((b & (1 << s)) == 1) o+=1;break;
				case 7:if ((b & (1 << s)) == 1) o+=128;break;
			}
		}
		return o;
	}

	private void RunRiver(int x, int z, byte inflow)
	{
		if ((x>=0) && (z>=0) && (x<map.Width) && (z < map.Height) && (Map.Tiles[x, z] != null))
		{
			byte outflow = 0;
			
			for (int s = 0;s<8;s++)
			{
				int i=0; int j=0;
				switch (s)
				{
					case 0:i=-1;j=-1;break;
					case 1:i=-1;j=0;break;
					case 2:i=-1;j=1;break;
					case 3:i=0;j=1;break;
					case 4:i=1;j=1;break;
					case 5:i=1;j=0;break;
					case 6:i=1;j=-1;break;
					case 7:i=0;j=-1;break;
				}
				if ((i+x>=0) && (i+x<map.Width) && (j+z>=0) && (j+z<map.Height))
				{
                    if (map.Tiles[i + x, j + z] == null)
                    {
                        outflow += (byte)(Mathf.Pow(2, (s + 2) % 8));
                        RunRiver(i + x, j + z, (byte)(Mathf.Pow(2, (s + 6) % 8)));
                    }
                    else if (map.Tiles[i+x,j+z].Top <= Map.Tiles[x,z].Top-1)
					{
						outflow += (byte) (Mathf.Pow(2,(s+2)%8));
						RunRiver(i+x, j+z, (byte) (Mathf.Pow(2,(s+6)%8)));
					}
				}
			}
			map.Tiles[x,z].AddRiver(inflow,outflow);
		}
	}*/


    public void OnClickFromCamera(Vector3 point)
    {
        int x = (int)((point.x - transform.position.x) / scale);
        int z = (int)((point.z - transform.position.z) / scale);

        //RunRiver(x, z,0);
        if (map.Tiles[x,z].Type == TileType.Stone)
            map.Tiles[x, z].AddSpring();

        if ((map.Tiles[x, z].Type == TileType.Grass) && (!map.Tiles[x, z].Water))
            map.Tiles[x, z].Type = TileType.Tree;

        bool a, b;
        UpdateTile(x, z, out a, out b);


        //map.Tiles [(int)(point.x/scale - transform.position.x), (int)(point.z/scale - transform.position.z)].ChangeTypeClick();
        //
        //float x = point.x/scale - transform.position.x;
        //float z = point.z/scale - transform.position.z;
        //map.Tiles[(int)x, (int)z].ChangeHeightClick((x-(int)x)*scale,(z-(int)z)*scale);
    }

    public void OnClickUpFromCamera(Vector3 point)
    {

    }

    public void OnRightClickFromCamera(Vector3 point)
    {

    }

    public void OnRightClickUpFromCamera(Vector3 point)
    {
	

    }

    public void OnMouseOverFromCamera(Vector3 point)
    {
        tileMarker.gameObject.SetActive(true);
		tileMarker.transform.position = new Vector3(Mathf.Floor(point.x/scale)*scale + 0.5f*scale, point.y+0.01f, Mathf.Floor(point.z/scale)*scale + 0.5f*scale);

    }

    public void OnMouseExit()
    {
        tileMarker.gameObject.SetActive(false);
    }



    /*
    public TileBehaviour[,] GetEuclidianNeighbours(int x, int z, int squareDistance)
    {
        TileBehaviour[,] neighbours = new TileBehaviour[squareDistance+1, squareDistance+1];

        for (int i=-squareDistance/2;i<=squareDistance/2;i++)
            for (int j = -squareDistance/2; j <=squareDistance/2; j++)
            {
                if ((i + x >= 0) && (i + x < width) && (j + z >= 0) && (j + z < height))
                {
                    float square2dDistance = Mathf.Pow(i, 2) + Mathf.Pow(j, 2);
                    if (square2dDistance <= squareDistance)
                    {
                        float tileY = tiles[i + x, j + z].GetComponent<TileBehaviour>().GetHeight();
                        float square3dDistance = square2dDistance + Mathf.Pow(tiles[x, z].GetComponent<TileBehaviour>().GetHeight() - tileY, 2);
                        if (square3dDistance <= squareDistance)
                        {
                            neighbours[i + squareDistance / 2, j + squareDistance / 2] = tiles[i + x, j + z].GetComponent<TileBehaviour>();
                        }
                    }
                }
            }
        return neighbours;
    }

    // Not working properly in two directions - doesn't work where there's a change in height
    public TileBehaviour[,] GetManhattanNeighbours(int x, int z, float distance)
    {
        int ceilDistance = (int)Mathf.Ceil(distance);
        float myHeight = tiles[x, z].GetComponent<TileBehaviour>().GetHeight();
        TileBehaviour[,] neighbours = new TileBehaviour[ceilDistance * 2 + 1, ceilDistance * 2 + 1];
        for (int i = -ceilDistance; i <= ceilDistance; i++)
            for (int j = -ceilDistance; j <= ceilDistance; j++)
            {
                if ((i + x >= 0) && (i + x < width) && (j + z >= 0) && (j + z < height))
                {
                    float twodDistance = i + j;
                    if (twodDistance <= distance)
                    {
                        float tileY = tiles[i + x, j + z].GetComponent<TileBehaviour>().GetHeight();
                        float threedDistance = twodDistance + Mathf.Abs(myHeight - tileY);
                        if (threedDistance <= distance)
                        {
                            neighbours[i + ceilDistance, j + ceilDistance] = tiles[i + x, j + z].GetComponent<TileBehaviour>();
                        }
                    }
                }
            }
        return neighbours;
    }*/

     public TileType[] GetNeighbourTypes(int x, int z, int distance)
    {
        int ceilDistance = distance;
        float myHeight = map.Tiles[x, z].Top;
        List<TileType> neighbours = new List<TileType>();
        for (int i = -ceilDistance; i <= ceilDistance; i++)
            for (int j = -ceilDistance; j <= ceilDistance; j++)
            {
                if ((i + x >= 0) && (i + x < map.Width) && (j + z >= 0) && (j + z < map.Height))
                {
                    float twodDistance = i + j;
                    if (twodDistance <= distance)
                    {
                        //float tileY = map.Tiles[i + x, j + z].Top;
                        //float threedDistance = twodDistance + Mathf.Abs(myHeight - tileY);
                        //if (threedDistance <= distance)
                        //{
                            neighbours.Add(map.Tiles[i + x, j + z].Type);
                        //}
                    }
                }
            }
        return neighbours.ToArray();
    }
}
