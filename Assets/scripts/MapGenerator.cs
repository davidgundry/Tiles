using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

    public float[,][] tileHeights;
    public TileType[,] tileTypes;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RandomMap(int width, int height)
    {
        tileHeights = new float[width, height][];
        tileTypes = new TileType[width, height];

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                float tileHeight = Random.Range(1, 5) / 2f;
                tileHeights[i, j] = new float[9]; 
                for (int h = 0; h < 9; h++)
                    tileHeights[i, j][h] = tileHeight;
                tileTypes[i, j] = TileType.Grass;
            }
    }
}
