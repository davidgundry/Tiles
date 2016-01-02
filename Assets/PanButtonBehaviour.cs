using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanButtonBehaviour : MonoBehaviour {

    private bool pan;

    public void ToggleButton()
    {
        pan = !pan;
        if (pan)
            GetComponent<Text>().text = "Rotate";
        else
            GetComponent<Text>().text = "Pan";
    }
}
