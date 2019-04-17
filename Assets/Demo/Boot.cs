using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boot : MonoBehaviour
{
    public float WaitTime = 3;
    private float t = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor)
        {
            Screen.SetResolution((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f), true);
        }
        
    }



    // Update is called once per frame
    void Update()
    {
        if (WaitTime <= 0.0f)
        {
            return;
        }
        t += Time.deltaTime;
        if (t > 1.0f)
        {
            t = 0.0f;
            WaitTime--;
        }

        if (WaitTime <= 0.0f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect() { position = new Vector2(Screen.width / 2,Screen.height / 2),size = new Vector2(100,100) },WaitTime + "秒后进入场景...");
    }
}
