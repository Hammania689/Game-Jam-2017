using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGen2 : MonoBehaviour
{

    // Use this for initialization

    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {
        transform.Translate(-1f, 0, 0);
        if (transform.position.x <= 39)
            transform.localScale += new Vector3(0, -0.2f, 0);
        else if (transform.localScale.y < .9f)
            transform.localScale += new Vector3(0, 0.2f, 0);
        if (transform.position.x <= 35f)
        {
            transform.Translate(14f, 0, 0);
        }
    }
}