using UnityEngine;
using System.Collections.Generic;

public enum TileTexture : byte
{
	Dirt, Grass, Stone, Sand
}

public enum TileType : byte
{
	Dirt, Grass, Water, Tree, Stone, Sand
}

public class Tile{

    private const float waterOffsetY = 0.001f;

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
        set {
		    type = value;
		    DirtyType = true;
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

    private byte[] surroundingHeights;
    public byte[] SurroundingHeights
    {
        get
        {
            return surroundingHeights;
        }
        set
        {
            surroundingHeights = value;
        }
    }

    private byte[] surroundingDepths;
    public byte[] SurroundingDepths
    {
        get
        {
            return surroundingDepths;
        }
        set
        {
            surroundingDepths = value;
        }
    }

    public int[] MaxNeighbourHeights
    { get
        {
            int[] maxNeighbourHeights = new int[8];
            maxNeighbourHeights[0] = Mathf.Max(surroundingHeights[14], surroundingHeights[15], surroundingHeights[0]);
            maxNeighbourHeights[1] = surroundingHeights[1];
            maxNeighbourHeights[2] = Mathf.Max(surroundingHeights[2], surroundingHeights[3], surroundingHeights[4]);
            maxNeighbourHeights[3] = surroundingHeights[5];
            maxNeighbourHeights[4] = Mathf.Max(surroundingHeights[6], surroundingHeights[7], surroundingHeights[8]);
            maxNeighbourHeights[5] = surroundingHeights[9];
            maxNeighbourHeights[6] = Mathf.Max(surroundingHeights[10], surroundingHeights[11], surroundingHeights[12]);
            maxNeighbourHeights[7] = surroundingHeights[13];
            return maxNeighbourHeights;
        }
    }

    public int[] MaxNeighbourDepths
    {
        get
        {
            int[] maxNeighbourDepths = new int[8];
            maxNeighbourDepths[0] = Mathf.Max(surroundingDepths[14], surroundingDepths[15], surroundingDepths[0]);
            maxNeighbourDepths[1] = surroundingDepths[1];
            maxNeighbourDepths[2] = Mathf.Max(surroundingDepths[2], surroundingDepths[3], surroundingDepths[4]);
            maxNeighbourDepths[3] = surroundingDepths[5];
            maxNeighbourDepths[4] = Mathf.Max(surroundingDepths[6], surroundingDepths[7], surroundingDepths[8]);
            maxNeighbourDepths[5] = surroundingDepths[9];
            maxNeighbourDepths[6] = Mathf.Max(surroundingDepths[10], surroundingDepths[11], surroundingDepths[12]);
            maxNeighbourDepths[7] = surroundingDepths[13];
            return maxNeighbourDepths;
        }
    }


    public int[] MaxNeighbourWaterHeights
    {
        get
        {
            int[] maxNeighbourWaterHeights = new int[8];
            maxNeighbourWaterHeights[0] = Mathf.Max(surroundingHeights[14] + surroundingDepths[14], surroundingHeights[15] + surroundingDepths[15], surroundingHeights[0] + surroundingDepths[0]);
            maxNeighbourWaterHeights[1] = surroundingHeights[1] + surroundingDepths[1];
            maxNeighbourWaterHeights[2] = Mathf.Max(surroundingHeights[2] + surroundingDepths[2], surroundingHeights[3] + surroundingDepths[3], surroundingHeights[4] + surroundingDepths[4]);
            maxNeighbourWaterHeights[3] = surroundingHeights[5] + surroundingDepths[5];
            maxNeighbourWaterHeights[4] = Mathf.Max(surroundingHeights[6] + surroundingDepths[6], surroundingHeights[7] + surroundingDepths[7], surroundingHeights[8] + surroundingDepths[8]);
            maxNeighbourWaterHeights[5] = surroundingHeights[9] + surroundingDepths[9];
            maxNeighbourWaterHeights[6] = Mathf.Max(surroundingHeights[10] + surroundingDepths[10], surroundingHeights[11] + surroundingDepths[11], surroundingHeights[12] + surroundingDepths[12]);
            maxNeighbourWaterHeights[7] = surroundingHeights[13] + surroundingDepths[13];
            return maxNeighbourWaterHeights;
        }
    }

    public float[] WaterVerticesY
    {
        get
        {
            float[] vertexYs = new float[9];
            int[] mnwh = MaxNeighbourWaterHeights;
            int[] mnd = MaxNeighbourDepths;
            int[] waterHeights = WaterHeights;

            if (waterDepths[0] > 0)
                vertexYs[0] = waterHeights[0] + waterOffsetY;

            for (int i = 1; i < 9; i++)
            {
                if (waterDepths[i] > 0)
                {
                    if (mnd[i - 1] > 0)
                        vertexYs[i] = Mathf.Max(waterHeights[i], mnwh[i - 1]) + waterOffsetY;
                    else
                        vertexYs[i] = waterHeights[i] + waterOffsetY;
                }
            }
            return vertexYs;
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

    public int[] WaterHeights
    {
        get
        {
            byte[] h = Heights;
            byte[] d = WaterDepths;
            int[] output = new int[9];

            for (int i = 0; i < 9; i++)
                output[i] = h[i] + d[i];

            return output;
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

    public void RecalculateWater()
    {
        byte[] h = Heights;
        bool[] waterOut = WaterOutflow;

        waterDepths[0] = 0;
        for (int i = 0; i < 8; i++)
        {
            waterDepths[i + 1] = 0;
            if (!waterOut[i])
                waterDepths[i+1] = WaterDepthForPoint(i);
        }

        if (WaterSource)
            waterDepths[0] = (byte) (Mathf.Max(1,waterDepths[0]));

        if (waterDepths[0] > 0)
        {
            Water = true;
            for (int i = 1; i < 9; i++)
            {
                if (waterDepths[i] > 0)
                    Water = true;
                if (h[0] + waterDepths[0] > h[i])
                    waterDepths[i] = (byte)(waterDepths[0]);// - Mathf.Min(0, h[i] - h[0]));
            }
        }
        DirtyWater = true;

    }

    byte WaterDepthForPoint(int p)
    {
        byte[] h = Heights;

        List<int> pointsToCheck = new List<int>();

        byte newDepth = 0;

        switch (p)
        {
            case 0:
                pointsToCheck.Add(14);
                pointsToCheck.Add(15);
                pointsToCheck.Add(0);
                break;
            case 1:
                pointsToCheck.Add(1);
                break;
            case 2:
                pointsToCheck.Add(2);
                pointsToCheck.Add(3);
                pointsToCheck.Add(4);
                break;
            case 3:
                pointsToCheck.Add(5);
                break;
            case 4:
                pointsToCheck.Add(6);
                pointsToCheck.Add(7);
                pointsToCheck.Add(8);
                break;
            case 5:
                pointsToCheck.Add(9);
                break;
            case 6:
                pointsToCheck.Add(10);
                pointsToCheck.Add(11);
                pointsToCheck.Add(12);
                break;
            case 7:
                pointsToCheck.Add(13);
                break;
        }

        for (int i=0;i<pointsToCheck.Count;i++)
            if (surroundingDepths[pointsToCheck[i]] > 0)
                if (surroundingHeights[pointsToCheck[i]] + surroundingDepths[pointsToCheck[i]] > h[p+1])
                    newDepth = 1;// (byte)Mathf.Max(newDepth, surroundingHeights[pointsToCheck[i]] + surroundingDepths[pointsToCheck[i]] - h[i + 1] - 1);

        if (newDepth > 0)
            if (h[p+1] + newDepth > h[0] + waterDepths[0])
                waterDepths[0] = 1;// (byte)(newDepth);// - Mathf.Min(0, h[0] - h[i+1]));

        return newDepth;
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
