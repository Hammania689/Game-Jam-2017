using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWave : MonoBehaviour{

    public Vector3 WaveDirection;

    public float horDis = 1, verDis = 1, horVel, verVel, time,
        maxHeight, range;

    Rigidbody rb;

    const float g = 9.81f;

    void FixedUpdate()
    {
       WaveGen();
    }

    void WaveGen()
    {
        /////////////
        ///WARNING///
        /////////////
        float velMag = horVel + verVel;
        float waveAngle = Mathf.Atan((verDis / horDis));

        time = (2 * velMag * Mathf.Sin(waveAngle)) / g;
        horVel = WaveDirection.x;
        verVel = WaveDirection.y;
        horDis = horVel * time; // Can be overided by the inspector
        verDis = verVel - g * time;
       
        maxHeight = (Mathf.Pow(velMag, 2) * Mathf.Pow(velMag, 2)) / (2 * g);
        range = (Mathf.Pow(velMag, 2) * 2 * Mathf.Sin(waveAngle)) / g;

        //prefab.transform.localScale = new Vector3(0.1f, Mathf.Sin(90), 0.1f);
        rb.velocity = new Vector3(horVel, verVel, 0);
    }
}
