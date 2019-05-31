using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSManager : MonoBehaviour {

    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames = 0;
    public static float fps;
    void Awake()
    {
        //		Application.runInBackground = true;
        //		Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //		Application.targetFrameRate = 30;
    }

    private void OnGUI()
    {
        GUILayout.Label("FPS:" + FPSManager.fps);
        if(GUILayout.Button("返回主页", GUILayout.Width(200), GUILayout.Height(100)))
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Boot", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
    }
}
