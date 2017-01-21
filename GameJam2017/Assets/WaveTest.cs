using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class WaveTest : MonoBehaviour
{
    public float waveLth, speed = 1, height = 2;
    public float theta;
    public int tick, timeLimit = 50;
    public Vector3 start, waveMvnt = new Vector3(1, 0, 0);

    bool wave;
    RaycastHit ray;
    RaycastHit hit;

    public struct RayWave
    {
        public Vector3 start;
        public float length;

        public RayWave(Vector3 spwnPnt, float lgth)
        {
            this.length = lgth;
            this.start = spwnPnt;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        //WaveGen();
    }

    void FixedUpdate()
    {
        WaveGen();
    }

    void WaveGen()
    {

        tick++;
        if (tick >= timeLimit)
        {
            if (theta >= 6.30f)
            {
                wave = false;
                tick = 0;
                theta = 0;
                height = Random.Range(0, 5);
            }
            else
            {
                wave = true;
                RayWave wavePeice = new RayWave(start, waveLth);
                waveMvnt = new Vector3(0, height * Mathf.Sin(theta), 0);
                start += new Vector3(.01f, .0f, 0);
                theta += speed * Mathf.Sin(.05f) * Time.time;
                Debug.DrawRay(start, waveMvnt, Color.cyan, waveLth);

                wavePeice = new RayWave(start, waveLth);
                waveMvnt = new Vector3(0, .05f, 0);
                start += new Vector3(.01f, .0f, 0);

                if (Physics.Raycast(start,waveMvnt,wavePeice.length))
                    print("Player was hit at " + hit.point);
                Debug.DrawRay(start, waveMvnt, Color.cyan, waveLth);
            }
        }
    }
}
