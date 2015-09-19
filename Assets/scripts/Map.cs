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
	
	private TileType[,] tileTypes;
	public TileType[,] TileTypes
	{
		get{
			return tileTypes;
		}
		set{
			tileTypes = value;
		}
	}
	private float[,][] tileHeights;
	public float[,][] TileHeights
	{
		get{
			return tileHeights;
		}
		set{
			tileHeights = value;
		}
	}
	
	Map(float x, float z, int width, int height, TileType[,]tileTypes, float[,][] tileHeights)
	{
		this.width = width;
		this.height = height;
		this.x = x;
		this.z = z;
		this.tileTypes = tileTypes;
		this.tileHeights = tileHeights;
	}

	public static Map Random(float x, float z, int width, int height)
	{
		float[,][]  tileHeights = new float[width, height][];
		TileType[,]  tileTypes = new TileType[width, height];
		
		for (int i = 0; i < width; i++)
			for (int j = 0; j < height; j++)
			{
			float tileHeight = UnityEngine.Random.Range(1, 5) / 2f;
				tileHeights[i, j] = new float[9]; 
				for (int h = 0; h < 9; h++)
					tileHeights[i, j][h] = tileHeight;
				tileTypes[i, j] = TileType.Grass;
			}

		return new Map(x,z,width,height,tileTypes,tileHeights);
	}

}
