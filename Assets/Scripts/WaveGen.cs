using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGen : MonoBehaviour {

    // Use this for initialization

    void Start()
    {
       
    }



        // Update is called once per frame
        void Update () {
        transform.Translate(1f, 0, 0);
        if (transform.position.x >=6)
            transform.localScale += new Vector3(0, -0.2f, 0);
        else if (transform.localScale.y <1.1f)
                transform.localScale += new Vector3(0, 0.2f,0);
        if (transform.position.x >=10f)
        {
            transform.Translate(-26f,0,0);
        }
	}
}

