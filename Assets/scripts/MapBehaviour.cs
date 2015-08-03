using UnityEngine;
using System.Collections;

public class MapBehaviour : MonoBehaviour {

    public Transform tilePrefab;

    public int width;
    public int height;

    private Transform[,] tiles;

    private bool updatedTiles = false;

	// Use this for initialization
	void Start () {
        transform.Translate(new Vector3(-width/2f,0f,-height/2f));

        tiles = new Transform[width, height];

        for (int i=0;i<width;i++)
            for (int j = 0; j < height; j++)
            {
                Transform tile = (Transform) Instantiate(tilePrefab, new Vector3(0,0,0), Quaternion.identity);
                tile.Translate(new Vector3(i,0,j));
                float h = Random.Range(1, 5) / 2f;
                float[] ha = { h, h, h, h, h, h, h, h };
                tile.GetComponent<TileBehaviour>().height = ha;
                tile.parent = transform;
                tile.GetComponent<TileBehaviour>().SetPosition(i, j);
                tiles[i, j] = tile;
            }



        transform.Translate(new Vector3(-width / 2f - 0.5f, 0f, -height / 2f - 0.5f));
	}

    // Update is called once per frame
    void Update()
    {
        if (!updatedTiles)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    tiles[i, j].GetComponent<TileBehaviour>().UpdateNeighbourhood();
            updatedTiles = true;
        }
	}

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
    }
}
