//using UnityEngine;
//using System.Collections;

//public enum TileType
//{
//    Dirt, Grass, Water, Tree
//}

//public class TileBehaviour : MonoBehaviour,Clickable {

//    private Vector3[] baseVertices;
//    private Vector3[] topVertices;

//    private Mesh baseMesh;
//    private Mesh topMesh;

//    public Transform tileBasePrefab;
//    public Transform tileTopPrefab;
//    public Transform treePrefab;

//    private Transform tileBase;
//    private Transform tileTop;

//    public Material grass;
//    public Material water;
//    public Material dirt;

//    public TileType tileType;
//    public float[] height;

//    private Transform clutter;

//    private TileBehaviour[,] neighbourhood;

//    private int x;
//    private int z;
    
//    // Use this for initialization
//    void Start () {
//        tileBase = (Transform) Instantiate(tileBasePrefab, transform.position, Quaternion.identity);
//        tileBase.parent = transform;
//        tileTop = (Transform)Instantiate(tileTopPrefab, transform.position, Quaternion.identity);
//        tileTop.parent = transform;

//        MeshFilter mf = (MeshFilter) tileBase.GetComponent(typeof(MeshFilter));
//        baseMesh = mf.mesh;
//        baseVertices = baseMesh.vertices;

//        mf = (MeshFilter)tileTop.GetComponent(typeof(MeshFilter));
//        topMesh = mf.mesh;
//        topVertices = topMesh.vertices;

//        setHeight(height);

//        setTileType(tileType);
//    }

//    public void UpdateNeighbourhood()
//    {
//        MapBehaviour mb = transform.parent.GetComponent<MapBehaviour>();
//        if (mb != null)
//            neighbourhood = mb.GetEuclidianNeighbours(x, z, 2);
//    }

//    // Update is called once per frame
//    void Update () {

//    }

//    public void OnClickFromCamera(Vector3 point)
//    {
//        Vector3 relativePos = point - topVertices[0];

//        if (Mathf.Pow(relativePos.x,2) + Mathf.Pow(relativePos.y,2) < 1f)
//        {
//            setHeight(topVertices[0].y+1);
//            Highlight();
//        }


//        /*if (tileType == TileType.Grass)
//        {
//                bool failed = false;
//                for (int i = 0; i < neighbourhood.GetLength(0); i++)
//                    for (int j = 0; j < neighbourhood.GetLength(1); j++)
//                    {
//                        if ((i!=x) && (j!=z) && (neighbourhood[i, j] != null))
//                        {
//                            if (neighbourhood[i, j].tileType == TileType.Tree)
//                                failed = true;
//                        }
//                    }
//                if (!failed)
//                    setTileType(TileType.Tree);
//        }*/
//    }

//    public void OnClickUpFromCamera(Vector3 point)
//    {
//        ClearHighlight();
//    }

//    public void OnRightClickFromCamera(Vector3 point)
//    {

//    }

//    public void OnRightClickUpFromCamera(Vector3 point)
//    {

//    }

//    public void OnMouseOverFromCamera(Vector3 point)
//    {
//       /* for (int i = 0; i < neighbourhood.GetLength(0); i++)
//            for (int j = 0; j < neighbourhood.GetLength(1); j++)
//            {
//                if (neighbourhood[i, j] != null)
//                {
//                    neighbourhood[i, j].Highlight();
//                }
//            }*/
//    }

//    public void OnMouseExit()
//    {
//       /* for (int i = 0; i < neighbourhood.GetLength(0); i++)
//            for (int j = 0; j < neighbourhood.GetLength(1); j++)
//            {
//                if (neighbourhood[i, j] != null)
//                {
//                    neighbourhood[i, j].ClearHighlight();
//                }
//            }*/
//    }

//    public void Highlight()
//    {
//        tileTop.GetComponent<LineRenderer>().enabled = true;
//    }

//    public void ClearHighlight()
//    {
//        tileTop.GetComponent<LineRenderer>().enabled = false;
//    }

//    private void setTileType(TileType type)
//    {
//        Material material = dirt;

//        if (clutter != null)
//            Destroy(clutter.gameObject);

//        switch (type)
//        {
//            case TileType.Dirt:
//                material = dirt;
//                tileType = TileType.Dirt;
//                break;
//            case TileType.Grass:
//                material = grass;
//                tileType = TileType.Grass;
//                break;
//            case TileType.Water:
//                material = water;
//                tileType = TileType.Water;
//                break;
//            case TileType.Tree:
//                material = grass;
//                clutter = (Transform) Instantiate(treePrefab, topVertices[0]+transform.position, Quaternion.identity);
//                tileType = TileType.Tree;
//                break;
//        }
//        if (clutter != null)
//            clutter.parent = transform;
//        tileTop.GetComponent<Renderer>().material = material;
//    }

//    private void setHeight(float h)
//    {
//        float[] hs = { h, h, h, h, h, h, h, h };
//        setHeight(hs);
//    }

//    private void setHeight(float[] h)
//    {

//        setVertexHeight(baseVertices, 3, h[0]);
//        setVertexHeight(baseVertices, 4, h[1]);
//        setVertexHeight(baseVertices, 2, h[2]);

//        setVertexHeight(baseVertices, 9, h[2]);
//        setVertexHeight(baseVertices, 8, h[3]);
//        setVertexHeight(baseVertices, 7, h[4]);

//        setVertexHeight(baseVertices, 10, h[4]);
//        setVertexHeight(baseVertices, 14, h[5]);
//        setVertexHeight(baseVertices, 13, h[6]);

//        setVertexHeight(baseVertices, 16, h[6]);
//        setVertexHeight(baseVertices, 15, h[7]);
//        setVertexHeight(baseVertices, 19, h[0]);

//        float mean = 0;
//        for (int i = 0; i < h.Length; i++)
//        {
//            mean = mean + h[i];
//        }
//        mean = mean / h.Length;

//        setVertexHeight(topVertices, 0, mean);
//        setVertexHeight(topVertices, 1, h[4]);
//        setVertexHeight(topVertices, 2, h[5]);
//        setVertexHeight(topVertices, 3, h[3]);
//        setVertexHeight(topVertices, 4, h[1]);
//        setVertexHeight(topVertices, 5, h[2]);
//        setVertexHeight(topVertices, 6, h[0]);
//        setVertexHeight(topVertices, 7, h[7]);
//        setVertexHeight(topVertices, 8, h[6]);

//        baseMesh.vertices = baseVertices;
//        topMesh.vertices = topVertices;

//        tileBase.GetComponent<MeshCollider>().sharedMesh = null;
//        tileBase.GetComponent<MeshCollider>().sharedMesh = baseMesh;
//        tileTop.GetComponent<MeshCollider>().sharedMesh = null;
//        tileTop.GetComponent<MeshCollider>().sharedMesh = topMesh;

//        LineRenderer lineRenderer = tileTop.GetComponent<LineRenderer>();
//        lineRenderer.SetVertexCount(8);
//        for (int i=0;i<8;i++)
//        {
//            lineRenderer.SetPosition(i, topVertices[i]);
//        }
//    }

//    private void setVertexHeight(Vector3[] v, int i, float h)
//    {
//        v[i] = new Vector3(v[i].x, h, v[i].z);
//    }

//    public float GetHeight()
//    {
//        return topVertices[0].y;
//    }

//    public void SetPosition(int x, int z)
//    {
//        this.x = x;
//        this.z = z;
//    }
//}
