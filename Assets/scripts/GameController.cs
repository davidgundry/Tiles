using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MapGenerator))]
public class GameController : MonoBehaviour {

    private MapBehaviour mapBehaviour;
    public Transform map; 

	// Use this for initialization
    void Start()
    {
        mapBehaviour = map.GetComponent<MapBehaviour>();
        MapGenerator mapGen= GetComponent<MapGenerator>();
        mapGen.RandomMap(5, 5);
        mapBehaviour.NewMap(5,5,mapGen.tileHeights, mapGen.tileTypes);
    }
	
	// Update is called once per frame
	void Update () {
        
	}


}
