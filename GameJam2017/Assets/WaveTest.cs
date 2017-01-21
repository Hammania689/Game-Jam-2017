using UnityEngine;
using UnityEditor;
using System.Collections;

public class WaveTest : MonoBehaviour
{
    public float waveLth;

    RaycastHit ray;
    RaycastHit hit;
    Vector3 start, waveMvnt = new Vector3(1,0,0);
    float theta;

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
	
	void Update ()
    {
        for(int rayCount = 0; rayCount < 10; rayCount++)
        {
            RayWave smallray = new RayWave(start, waveLth);
            waveMvnt = new Vector3(Mathf.Sin(theta), 0,0);
            start += new Vector3(smallray.start.x, .1f ,0);
            theta += Mathf.Sin(30);
            if (Physics.Raycast(start,waveMvnt,waveLth))
                print("The ray " + rayCount);
            Debug.DrawRay(start,waveMvnt, Color.green,waveLth);
        }
    }
}
