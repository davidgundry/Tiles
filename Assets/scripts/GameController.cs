using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    public Transform skyController;
    private skydomeScript2 skyScript;

	// Use this for initialization
	void Start () {
        //skyScript = skyController.GetComponent<skydomeScript2>();
	}
	
	// Update is called once per frame
	void Update () {
        //skyScript.TIME += 0.25f*Time.deltaTime;
	}
}
