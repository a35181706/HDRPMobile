using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneInfo
{
    public string sceneName;
    public string sceneDesc;
}

public class Boot : MonoBehaviour
{

    public List<SceneInfo> sceneList = new List<SceneInfo>();
    // Start is called before the first frame update
    void Start()
    {
        //if (!Application.isEditor)
        //{
        //    Screen.SetResolution((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f), true);
        //}
        
    }

    Vector2 scrollPos = Vector2.zero;
    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach(var info in sceneList)
        {
            if (GUILayout.Button(info.sceneDesc,GUILayout.Width(200),GUILayout.Height(100)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(info.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        GUILayout.EndScrollView();
    }
}
