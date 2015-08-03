using UnityEngine;
using System.Collections;

public class OceanBehaviour : MonoBehaviour, Clickable {

    private rippleSharp rippleScript;

	// Use this for initialization
	void Start () {
        rippleScript = GetComponent<rippleSharp>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClickFromCamera(Vector3 point)
    {
        //rippleScript.splashAtPoint((int) point.x, (int) point.z);
        //rippleScript.splashAtPoint(5, 5);
    }

    public void OnClickUpFromCamera(Vector3 point)
    {

    }

    public void OnRightClickFromCamera(Vector3 point)
    {

    }

    public void OnRightClickUpFromCamera(Vector3 point)
    {

    }

    public void OnMouseOverFromCamera(Vector3 point)
    {

    }
}
