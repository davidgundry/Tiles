using UnityEngine;
using System.Collections;

public class Map {

	private int width;
	public  int Width
	{
		get{
			return width;
		}
		set{
			width = value;
		}
	}
	private int height;
	public  int Height
	{
		get{
			return height;
		}
		set{
			height = value;
		}
	}

	private float x;
	public float X {
		get {
			return x;
		}
		set {
			x = value;
		}
	}

	private float z;
	public float Z {
		get { 
			return z;
		}
		set {
			z = value;
		}
	}

	private Tile[,] tiles;
	public Tile[,] Tiles {
		get {
			return tiles;
		}
		set {
			tiles = value;
		}
	}

	Map(float x, float z, Tile[,] tiles)
	{
		this.x = x;
		this.z = z;
		this.width = tiles.GetLength(0);
		this.height = tiles.GetLength(1);
		this.tiles = tiles;
	}

	public static Map Random(float x, float z, int width, int height)
	{
		Tile[,] tiles = new Tile[width, height];

		for (int i = 0; i < width; i++)
			for (int j = 0; j < height; j++)
			{

                byte top = (byte)(width + 1 - 2*Mathf.Sqrt(Mathf.Pow(i - width / 2, 2) + Mathf.Pow(j - width / 2, 2)));  //(((i % 5 + j % 5) * ((i%20)+1) % ((j%20) + 1) + ((i%20) * (j%20))/ 10));// (byte)(UnityEngine.Random.Range(0, 5));
                if ((i == 0) || (j == 0) || (i == width - 1) || (j == height - 1))
                    top = 0;
                //if (top > 0)
                tiles[i,j] = new Tile(top,TileType.Stone);

			}

		return new Map(x,z, tiles);
	}

}
