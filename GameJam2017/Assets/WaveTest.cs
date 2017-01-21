using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class WaveTest : MonoBehaviour
{
    public float waveLth, speed = 1, height = 2;
    public float theta;
    public GameObject prefab;
    public int tick, timeLimit = 50;
    public Vector3 start, waveMvnt = new Vector3(1, 0, 0);

    bool wave;
    RaycastHit ray;
    RaycastHit hit;

    public struct RayWave
    {
        public Vector3 start;
        public float length;
        public GameObject wavePeice;

        public RayWave(Vector3 spwnPnt, float lgth, GameObject pref)
        {
            this.length = lgth;
            this.start = spwnPnt;
            this.wavePeice = pref;
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
            if (theta >= Mathf.PI)
            {
                wave = false;
                tick = 0;
                theta = 0;
                height = Random.Range(0, 2);
            }
            else
            {
                wave = true;
                RayWave wavePeice = new RayWave(start, waveLth,prefab);
                // Fixed Amplitude and Thickness of wave

                //We want the range of 0 to pi
                waveMvnt = new Vector3(0, height * Mathf.Sin(theta), 0);
                start += new Vector3(.01f, .0f, 0);
                theta += speed * Mathf.Sin(.05f) * Time.time;

                theta = Mathf.Clamp(theta, 0, Mathf.PI);
                prefab.transform.localScale = new Vector3(0.1f,Mathf.Sin(theta),0.1f);

                Debug.DrawRay(start, waveMvnt, Color.cyan, waveLth);
                Instantiate(prefab, wavePeice.start, transform.rotation, transform);

                wavePeice = new RayWave(start, waveLth, prefab);
                waveMvnt = new Vector3(0, .05f, 0);
                start += new Vector3(.01f, .0f, 0);

                if (Physics.Raycast(start,waveMvnt,wavePeice.length))
                    print("Player was hit at " + hit.point);
                Debug.DrawRay(start, waveMvnt, Color.cyan, waveLth);
            }
        }
    }

    void ShowWave()
    {

    }
}
