using UnityEngine;

public enum TileTexture : byte
{
	Dirt, Grass
}

public enum TileType : byte
{
	Dirt, Grass, Water, Tree
}

public class Tile{

    private static float waterOffset = -0.001f;

	private byte top;
	private byte vertUp;
	private byte vertDown;
    private byte[] waterDepths;
    private byte waterOutflow; // flags if water is leaving in each direction, so it isn't counted when recalculating water height
	private TileType type;
    private byte flags; // 1=dirty mesh, 2=dirty type, 4=water, 8 = dirty water, 16 = water source

    private Transform clutter;

    public byte[] WaterDepths
    {
        get
        {
            return waterDepths;
        }
        set
        {
            waterDepths = value;
        }
    }

    public bool DirtyMesh {
		get {
			return ((flags & (1 << 0)) != 0);
		}
		set {
            if (value && !DirtyMesh)
                flags += 1;
            else if (!value && DirtyMesh)
                flags -= 1;
		}
	}

	public bool DirtyType {
		get {
            return ((flags & (1 << 1)) != 0);
        }
		set {
            if (value && !DirtyType)
                flags += 2;
            else if (!value && DirtyType)
                flags -= 2;
        }
	}

    public bool Water
    {
        get
        {
            return ((flags & (1 << 2)) != 0);
        }
        set {
            if (value && !Water)
                flags += 4;
            else if (!value && Water)
                flags -= 4;
        }
    }

    public bool DirtyWater
    {
        get
        {
            return ((flags & (1 << 3)) != 0);
        }
        set
        {
            if (value && !DirtyWater)
                flags += 8;
            else if (!value && DirtyWater)
                flags -= 8;
        }
    }

    public bool WaterSource
    {
        get
        {
            return ((flags & (1 << 4)) != 0);
        }
        set
        {
            if (value && !WaterSource)
                flags += 16;
            else if (!value && WaterSource)
                flags -= 16;
        }
    }

    public byte Top {
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

    public Tile(byte top, TileType type)
    {
        this.top = top;
        this.type = type;

        vertUp = 0;
        vertDown = 0;
        flags = 0;
        waterDepths = new byte[9];
        for (int i = 0; i < 9; i++)
        {
            waterDepths[i] = 0;
        }
    }
	
	/**
	 * 	An array of z coordinates expresed as byte. For each bit, if vertUp isn't set, checks vertDown.
	 *  If neither, that vert is the same height as the middle (top).
	 **/
	public byte[] Heights {
		get {
			byte[] h = new byte[9];
			h [0] = top;

            int t = 0;

			for (int i=1; i<9; i++) {
				t = (top + (((vertUp & (1 << i - 1)) != 0) ? 1 : 0));
				if (t == top)
					t = (top - (((vertDown & (1 << i - 1)) != 0) ? 1 : 0));

                if (t < 0)
                    t = 0;
                if (t > 255)
                    t = 255;
                h[i] = (byte) t;
			}

			return h;
		}
	}

    public bool[] WaterOutflow
    {
        get
        {
            bool[] o = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                o[i] = ((waterOutflow & (1 << i)) != 0);
            }
            return o;
        }
    }

    public float[] WaterHeights
    {
        get
        {
            byte[] h = Heights;
            byte[] d = WaterDepths;
            float[] output = new float[9];

            for (int i = 0; i < 9; i++)
                output[i] = h[i] + d[i] + waterOffset;

            return output;
        }
    }

    public void ChangeTypeClick()
	{
		switch (type)
		{
			case TileType.Dirt:
				type = TileType.Grass;
				DirtyType = true;
				break;
			case TileType.Grass:
				type = TileType.Tree;
                DirtyType = true;
				break;
			case TileType.Tree:
				type = TileType.Dirt;
                DirtyType = true;
				break;
		}
	}

	public void ChangeHeightClick(float x, float z)
	{
		if ((x < 0.25f) && (z < 0.25f))
			if (((vertUp & (1 << 1 - 1)) == 0)) {
				vertUp += 1;
				DirtyMesh = true;
			}
		if ((x > 0.75f) && (z < 0.25f))
			if (((vertUp & (1 << 3 - 1)) == 0)) {
				vertUp += 4;
                DirtyMesh = true;
			}
		if ((x > 0.75f) && (z >0.75f))
			if (((vertUp & (1 << 5 - 1)) == 0)) {
				vertUp += 16;
                DirtyMesh = true;
			}

		if ((x < 0.25f) && (z >0.75f))
			if (((vertUp & (1 << 7 - 1)) == 0)) {
				vertUp += 64;
                DirtyMesh = true;
			}
	}


    public void AddSpring()
    {
        Water = true;
        WaterSource = true;
    }

    public void RecalculateWater(byte[] surroudingHeights, byte[] surroudingDepths)
    {
        byte[] h = Heights;
        bool[] waterOut = WaterOutflow;

        waterDepths[0] = 0;
        for (int i = 0; i < 8; i++)
        {
            waterDepths[i + 1] = 0;
            if (!waterOut[i])
                if (surroudingHeights[i]+surroudingDepths[i] > h[i+1])
                    waterDepths[i+1] = (byte) (surroudingDepths[i]);// - Mathf.Min(0, h[i+1]-surroudingHeights[i]));
            if (h[i + 1] + waterDepths[i + 1] > h[0])
                waterDepths[0] = (byte)(waterDepths[i + 1]);// - Mathf.Min(0, h[0] - h[i+1]));
        }

        if (WaterSource)
            waterDepths[0] = (byte) (Mathf.Max(1,waterDepths[0]));

        for (int i = 1; i < 9; i++)
        {
            if (h[0] + waterDepths[0] > h[i])
                waterDepths[i] = (byte)(waterDepths[0]);// - Mathf.Min(0, h[i] - h[0]));
        }
        DirtyWater = true;

    }

	/*public void AddRiver(byte inflow, byte outflow)
	{
        if (!Water)
        {
            Water = true;
        }

		for (int i=0; i<8; i++) {
			if (((inflow & vertUp) & (1 << i)) != 0)
			{
				vertUp -= (byte) Mathf.Pow (2,i);
                waterDepths[i] = 1;
			}
			else if (((inflow & ~vertDown) & (1 << i)) != 0)
			{
				vertDown += (byte) Mathf.Pow (2,i);
                waterDepths[i] = 1;
            }
		}
		if (vertUp == 0) {
            if (top > 0)
            {
                top -= 1;
                vertUp = (byte)~vertDown;
                vertDown = 0;
            }

            for (int i = 0; i < 8; i++)
            {
                if (((outflow & vertUp) & (1 << i)) != 0)
                {
                    vertUp -= (byte)Mathf.Pow(2, i);
                    waterDepths[i] = 1;
                }
                else if (((outflow & ~vertDown) & (1 << i)) != 0)
                {
                    vertDown += (byte)Mathf.Pow(2, i);
                    waterDepths[i] = 1;
                }
            }

		}
		DirtyMesh = true;
        DirtyWater = true;
    }*/

}
