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
				tiles[i,j] = new Tile(i,j,(int) (UnityEngine.Random.Range(1, 5) / 2f),TileType.Grass);
			}

		return new Map(x,z, tiles);
	}

}
