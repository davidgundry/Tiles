using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    private MapBehaviour mapBehaviour;
    public Transform mapPrefab;
	public Transform cameraBounds;
	public float levelChangeSpeed;
	private bool changingMap = false;
	public Transform mainCamera;
	private CameraBehaviour camera;

	private Vector3 target;
	
	private Transform[] maps;

	public int activeMap = 0;
	
    void Start()
    {
		camera = mainCamera.GetComponent<CameraBehaviour> ();
		maps = new Transform[3];
		createMap (0, 0, 0);
        ChangeActiveMap(0);

        Map m = maps[activeMap].GetComponent<MapBehaviour>().Map;
        target = new Vector3(m.X + m.Width / 2, m.Tiles[m.Width / 2, m.Height / 2].Top/2, m.Z + m.Height / 2);
        cameraBounds.transform.position = target;

		//createMap (1, 50, 50);
		//createMap (2, 50, 10);
    }
	
	void createMap(int mapID, float x, float z)
	{
		if ((mapID >= 0) && (mapID < maps.Length)) {
			if (mapPrefab != null) {
				maps [mapID] = Instantiate (mapPrefab);
				mapBehaviour = maps [mapID].GetComponent<MapBehaviour> ();
				mapBehaviour.NewMap(Map.Random(x,z, 15, 15));
			} else
				Debug.Log ("Map prefab not found!");
		}
	}
	
	void Update () {
		if (changingMap) {
			if (cameraBounds.transform.position != target)
				cameraBounds.transform.position = Vector3.MoveTowards (cameraBounds.transform.position, target, levelChangeSpeed);
			else
			{
				changingMap = false;
				camera.inputEnabled = true;
			}
		}
	}

	void LateUpdate()
	{
		if (Input.GetKeyDown ("n")) {
			if (activeMap < maps.Length-1)
				ChangeActiveMap (activeMap + 1);
			else
				ChangeActiveMap(0);
		}
	}

	void ChangeActiveMap(int newMap)
	{
		Debug.Log ("Level " + newMap);
		activeMap = newMap;
		Map m = maps [activeMap].GetComponent<MapBehaviour> ().Map;
		target = new Vector3(m.X + m.Width/2, m.Tiles[m.Width/2,m.Height/2].Top/2,m.Z + m.Height/2);
		changingMap = true;
		camera.inputEnabled = false;
		camera.target.transform.localPosition = new Vector3 (0, 0, 0);
	}


}
