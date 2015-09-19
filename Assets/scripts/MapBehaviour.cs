using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileTexture
{
    Dirt, Grass
}

public enum TileType
{
    Dirt, Grass, Water, Tree
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MapBehaviour : MonoBehaviour, Clickable
{
	private Map map;
	public Map Map {
		get {
			return map;
		}
	}
    private Transform tileMarker;

    public Transform treePrefab;

    private const int verticesPerTile = 24;

	void Start () {
		tileMarker = GameObject.Find ("TileMarker").transform;
	}

    void Update()
    {
        /*if (!updatedTiles)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    tiles[i, j].GetComponent<TileBehaviour>().UpdateNeighbourhood();
            updatedTiles = true;
        }*/
	
    }

    private void SetTileType(int x, int y, TileType type)
    {
        map.TileTypes[x, y] = type;
        TextureTile(x, y, TileTypeToTileTexture(type));
        ClutterTile(x, y, type);
    }

    private void TextureTile(int x, int y, TileTexture texture)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh.uv = SetTileUV(mf.mesh.uv, x, y, texture);
    }

    private void ClutterTile(int x, int y, TileType type)
    {
        Transform clutter = null;
        switch (type)
        {
            case TileType.Dirt:
                clutter = (Transform)Instantiate(treePrefab, new Vector3(x+0.5f+transform.position.x, map.TileHeights[x,y][0], y+0.5f+transform.position.z), Quaternion.identity);
                break;
        }
        if (clutter != null)
            clutter.parent = transform;
    }
	
	public void NewMap(Map map)
	{
		this.map = map;
		transform.position = new Vector3 (map.X, 0, map.Z);
		SetMapVertices();
		SetMapTextures();
	}

    private void SetMapVertices()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        Vector3[] verts = new Vector3[map.Width * map.Height * verticesPerTile];

        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
                SetTileVertices(verts, i, j, map.TileHeights[i, j]);

        mf.mesh.vertices = verts;

        int[] newTriangles = new int[mf.mesh.vertices.Length];
        for (int i = 0; i < mf.mesh.vertices.Length; i++)
            newTriangles[i] = ((i / 3) + 1) * 3 - 1 - i % 3;

        mf.mesh.triangles = newTriangles;
        mf.mesh.RecalculateNormals();

        UpdateMeshCollider();

        SetBaseVertices();
    }

    private int TileCoordsToVertexIndex(int x, int y, int r)
    {
        return verticesPerTile * (x + y * map.Width) + r;
    }

    private void UpdateMeshCollider()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mf.mesh;
    }

    private Vector3[] SetTileVertices(Vector3[] verts, int x, int y, float[] tileHeight)
    {
        float halfTileWidth = 0.5f;
        float halfTileDepth = 0.5f;

        int vert = TileCoordsToVertexIndex(x, y, 0);
        // 0
        verts[vert] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 1] = new Vector3(x, tileHeight[1], y);
        verts[vert + 2] = new Vector3(x + halfTileWidth, tileHeight[2], y);

        // 1
        verts[vert + 3] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 4] = new Vector3(x + halfTileWidth, tileHeight[2], y);
        verts[vert + 5] = new Vector3(x + halfTileWidth * 2, tileHeight[3], y);

        //2
        verts[vert + 6] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 7] = new Vector3(x + halfTileWidth * 2, tileHeight[3], y);
        verts[vert + 8] = new Vector3(x + halfTileWidth * 2, tileHeight[4], y+ halfTileDepth);

        //3
        verts[vert + 9] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 10] = new Vector3(x + halfTileWidth * 2, tileHeight[4], y+ halfTileDepth);
        verts[vert + 11] = new Vector3(x + halfTileWidth * 2, tileHeight[5], y+ halfTileDepth * 2);

        //4
        verts[vert + 12] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 13] = new Vector3(x + halfTileWidth * 2, tileHeight[5], y+ halfTileDepth * 2);
        verts[vert + 14] = new Vector3(x + halfTileWidth, tileHeight[6], y+ halfTileDepth * 2);

        //5
        verts[vert + 15] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 16] = new Vector3(x + halfTileWidth, tileHeight[6], y+ halfTileDepth * 2);
        verts[vert + 17] = new Vector3(x, tileHeight[7], y+ halfTileDepth * 2);

        //6
        verts[vert + 18] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 19] = new Vector3(x, tileHeight[7], y+ halfTileDepth * 2);
        verts[vert + 20] = new Vector3(x, tileHeight[8], y+ halfTileDepth);

        //7
        verts[vert + 21] = new Vector3(x + halfTileWidth, tileHeight[0], y+ halfTileDepth);
        verts[vert + 22] = new Vector3(x, tileHeight[8], y+ halfTileDepth);
        verts[vert + 23] = new Vector3(x, tileHeight[1], y);

        return verts;
    }


    private void SetMapTextures()
    {
        TileTexture[,] textures = new TileTexture[map.Width,map.Height];
        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
                textures[i,j] = TileTypeToTileTexture(map.TileTypes[i,j]);

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh.uv = UVsFromTextureMap(textures);
    }

    private static TileTexture TileTypeToTileTexture(TileType t)
    {
        switch (t)
        {
            case TileType.Dirt:
                return TileTexture.Dirt;
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
        MeshFilter mf = GetComponent<MeshFilter>();
        Vector3[] surfaceVerts = mf.mesh.vertices;

        // Create base besh
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        int tri = 0;
        for (int i = 0; i < map.Width; i++)
            for (int j = 0; j < map.Height; j++)
            {
                if (i > 0) // Left edge
                {
                    if (map.TileHeights[i, j][1] != map.TileHeights[i - 1, j][3])
                    {
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i - 1, j, 7)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 23)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 22)]);

                        triangles.Add(tri + 2);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 0);
                        tri += 3;
                    }
					if (map.TileHeights[i, j][8] != map.TileHeights[i - 1, j][4])
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
					if (map.TileHeights[i, j][7] != map.TileHeights[i - 1, j][5])
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

                if (j > 0) // Top edge
                {
					if (map.TileHeights[i, j][1] != map.TileHeights[i, j - 1][7])
                    {
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 1)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j, 2)]);
                        verts.Add(surfaceVerts[TileCoordsToVertexIndex(i, j - 1, 19)]);

                        triangles.Add(tri);
                        triangles.Add(tri + 1);
                        triangles.Add(tri + 2);
                        tri += 3;
                    }
					if (map.TileHeights[i, j][2] != map.TileHeights[i, j - 1][6])
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
					if (map.TileHeights[i, j][3] != map.TileHeights[i, j - 1][5])
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

        Vector3[] meshVerts = new Vector3[verts.Count];
        for (int i = 0; i < meshVerts.Length; i++)
            meshVerts[i] = verts[i];

        MeshFilter cmf = transform.GetChild(0).GetComponent<MeshFilter>();
        if (cmf != null)
        {
            cmf.mesh.vertices = meshVerts;

            int[] newTriangles = new int[triangles.Count];
            for (int i = 0; i < newTriangles.Length; i++)
                newTriangles[i] = triangles[i];
            cmf.mesh.triangles = newTriangles;

            cmf.mesh.RecalculateNormals();
        }

        MeshCollider cmc = transform.GetChild(0).GetComponent<MeshCollider>();
        if (cmc != null)
        {
            cmc.sharedMesh = null;
            cmc.sharedMesh = mf.mesh;
        }
    }

    public void OnClickFromCamera(Vector3 point)
    {
        SetTileType((int) (point.x - transform.position.x), (int) (point.z-transform.position.z), TileType.Dirt);
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
        tileMarker.transform.position = new Vector3(Mathf.Floor(point.x) + 0.5f, point.y+0.01f, Mathf.Floor(point.z) + 0.5f);
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
}
