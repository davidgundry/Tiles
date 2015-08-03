using UnityEngine;
using System.Collections;

public class TileTopBehaviour : MonoBehaviour, Clickable {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClickFromCamera(Vector3 point)
    {
        transform.parent.gameObject.GetComponent<Clickable>().OnClickFromCamera(point);
    }

    public void OnClickUpFromCamera(Vector3 point)
    {
        transform.parent.gameObject.GetComponent<Clickable>().OnClickUpFromCamera(point);
    }

    public void OnRightClickFromCamera(Vector3 point)
    {
        transform.parent.gameObject.GetComponent<Clickable>().OnRightClickFromCamera(point);
    }

    public void OnRightClickUpFromCamera(Vector3 point)
    {
        transform.parent.gameObject.GetComponent<Clickable>().OnRightClickUpFromCamera(point);
    }

    public void OnMouseOverFromCamera(Vector3 point)
    {
        transform.parent.gameObject.GetComponent<Clickable>().OnMouseOverFromCamera(point);
    }

    public void OnMouseExit()
    {
        transform.parent.gameObject.GetComponent<TileBehaviour>().OnMouseExit();
    }
}
