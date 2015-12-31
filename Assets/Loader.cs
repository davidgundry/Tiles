using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Loader : MonoBehaviour {

    public Text percentText;

	// Use this for initialization
	void Start () {
        StartCoroutine("loadLevel");
	}
	
	// Update is called once per frame
	void Update () {
         
	}

    IEnumerator loadLevel()
    {
        AsyncOperation async = Application.LoadLevelAsync("main");
        async.allowSceneActivation = true;
        while (!async.isDone)
        {
            int progress = (int)(async.progress * 100);
            percentText.text = progress + "%";
            yield return (0);
        }


    }
}
