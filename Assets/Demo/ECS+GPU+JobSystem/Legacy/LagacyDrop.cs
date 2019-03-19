using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagacyDrop : MonoBehaviour {

    public float mass;
    public float delay;
    public float velocity;

    void Update()
    {
        if (this.delay > 0)
        {
            this.delay -= Time.deltaTime;
        }
        else
        {
            Vector3 pos = transform.position;
            float v = this.velocity + InitDemo.G * this.mass * Time.deltaTime;
            pos.y += v;
            if (pos.y < InitDemo.bottomY)
            {
                pos.y = InitDemo.topY;
                this.velocity = 0f;
                this.delay = 5;
            }
            transform.position = pos;
        }
    }
}
