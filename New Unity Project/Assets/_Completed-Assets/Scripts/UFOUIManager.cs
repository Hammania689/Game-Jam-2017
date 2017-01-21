using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOUIManager : MonoBehaviour {

    public GameObject[] cameras;

    public GameObject[] ufos;

    public FluidSim fluid_navierstokes;

    public void ActivateCam1() { cameras[0].SetActive(true); cameras[1].SetActive(false); }
    public void ActivateCam2() { cameras[1].SetActive(true); cameras[0].SetActive(false); }

    // Use this for initialization
    void Start () {
		
	}

    private Vector3 ufo_prev_position;
	// Update is called once per frame
	void Update () {
        Vector3 ufo_position = ufos[0].transform.position;
        Vector3 l = fluid_navierstokes.transform.InverseTransformPoint(ufo_position),
                lprev = fluid_navierstokes.transform.InverseTransformPoint(ufo_prev_position);
		ufo_prev_position = ufo_position;
        l.x += 0.5f; l.y += 0.5f; // make up for the differences in 1) XY vs. XZ coords and 2) diff't anchor points (center vs bottom-left)
        lprev.x += 0.5f; lprev.y += 0.5f;
		Debug.Log(string.Format("{0} ---> {1} ({2} and {3})", lprev.ToString(), l.ToString(),
			ufo_prev_position.ToString(), ufo_position.ToString()));
		fluid_navierstokes.AddSomething(l, (l - lprev)*10);
	}
}
