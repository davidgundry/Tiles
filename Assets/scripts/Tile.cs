using UnityEngine;

public enum TileTexture
{
	Dirt, Grass
}

public enum TileType
{
	Dirt, Grass, Water, Tree
}

public class Tile{

	private bool dirtyMesh;
	private bool dirtyType;
	private int x;
	private int z;
	private int top;
	private byte vertUp;
	private byte vertDown;
	private TileType type;
	private Transform clutter;


	public bool DirtyMesh {
		get {
			return dirtyMesh;
		}
		set {
			dirtyMesh = value;
		}
	}

	public bool DirtyType {
		get {
			return dirtyType;
		}
		set {
			dirtyType = value;
		}
	}
	
	public int Top {
		get {
			return top;
		}
		set {
			top = value;
		}
	}

	public TileType Type {
		get {
			return type;
		}
	}
	public Transform Clutter {
		get {
			return clutter;
		}
		set {
			clutter = value;
		}
	}

	public Tile(int x, int z, int top, TileType type)
	{
		this.x = x;
		this.z = z;
		this.top = top;
		this.type= type;

		vertUp = 0;
		vertDown = 0;
	}
	
	/**
	 * 	An array of z coordinates expresed as int. For each bit, if vertUp isn't set, checks vertDown.
	 *  If neither, that vert is the same height as the middle (top).
	 **/
	public int[] Heights {
		get {
			int[] h = new int[9];
			h [0] = top;

			for (int i=1; i<9; i++) {
				h [i] = top + (((vertUp & (1 << i - 1)) != 0) ? 1 : 0);
				if (h[i] == top)
					h [i] = top - (((vertDown & (1 << i - 1)) != 0) ? 1 : 0);
			}

			return h;
		}
	}

	public void ChangeTypeClick()
	{
		switch (type)
		{
			case TileType.Dirt:
				type = TileType.Grass;
				dirtyType = true;
				break;
			case TileType.Grass:
				type = TileType.Tree;
				dirtyType = true;
				break;
			case TileType.Tree:
				type = TileType.Dirt;
				dirtyType = true;
				break;
		}
	}

	public void ChangeHeightClick(float x, float z)
	{
		if ((x < 0.25f) && (z < 0.25f))
			if (((vertUp & (1 << 1 - 1)) == 0)) {
				vertUp += 1;
				dirtyMesh = true;
			}
		if ((x > 0.75f) && (z < 0.25f))
			if (((vertUp & (1 << 3 - 1)) == 0)) {
				vertUp += 4;
				dirtyMesh = true;
			}
		if ((x > 0.75f) && (z >0.75f))
			if (((vertUp & (1 << 5 - 1)) == 0)) {
				vertUp += 16;
				dirtyMesh = true;
			}

		if ((x < 0.25f) && (z >0.75f))
			if (((vertUp & (1 << 7 - 1)) == 0)) {
				vertUp += 64;
				dirtyMesh = true;
			}
	}


	public void AddRiver(byte inflow, byte outflow)
	{
		for (int i=0; i<8; i++) {
			if (((inflow & vertUp) & (1 << i)) != 0)
			{
				vertUp -= (byte) Mathf.Pow (2,i);
			}
			else if (((inflow & ~vertDown) & (1 << i)) != 0)
			{
				vertDown += (byte) Mathf.Pow (2,i);
			}
		}
		if (vertUp == 0) {
			top -= 1;
			vertUp = (byte) ~vertDown;
			vertDown = 0;

			for (int i=0; i<8; i++) {
				if (((outflow & vertUp) & (1 << i)) != 0)
				{
					vertUp -= (byte) Mathf.Pow (2,i);
				}
				else if (((outflow & ~vertDown) & (1 << i)) != 0)
				{
					vertDown += (byte) Mathf.Pow (2,i);
				}
			}

		}
		dirtyMesh = true;
	}



}
