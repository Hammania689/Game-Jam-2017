using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawn : MonoBehaviour {

    public GameObject prefab;

	// Update is called once per frame
	void FixedUpdate () {
        Instantiate(prefab, this.transform.position, transform.rotation, this.transform);
    }
}
