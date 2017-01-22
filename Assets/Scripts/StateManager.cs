using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour
{
    #region Global Variable Declaration

    [Header("Different States")]
    public bool aiming;
    public bool canRun;
    public bool sprint;
    public bool shoot;
    public bool reloading;
    public bool onGround;
    public bool jumping;
    public bool notFacing;

    [Header("Positional Data")]
    public float horizontal;
    public float vertical;
    public Vector3 lookPosition;
    public Vector3 lookHitPosition;
    public LayerMask layerMask;

    [Header("Gun Audio Manager")]
    //public CharacterAudioManager audioManager;

    [HideInInspector]
    public HandleShooting handleShooting;
    [HideInInspector]
    public HandleAnimations handleAnim;

    [Header("Display Color Info")]
    public GameObject mesh;
    public Color initalColor;
    public Color sprintDarkColor;
    public float initalRimPower = 0f;
    Renderer rend;
    Vector3 direction = new Vector3(0f, -1f, 0f);
    #endregion

    void Start()
    {
        //audioManager = GetComponent<CharacterAudioManager>();
        handleShooting = GetComponent<HandleShooting>();
        handleAnim = GetComponent<HandleAnimations>();

        if (mesh != null)
        {
            rend = mesh.GetComponent<Renderer>();
            initalColor = rend.material.GetColor("_RimColor");
            initalRimPower = rend.material.GetFloat("_RimPower");
        }
    }

    void Update()
    {
        if (rend != null)
        {
            if (sprint && !reloading && vertical > 0 || sprint && !reloading && horizontal > 0)
            {
                rend.material.SetColor("_RimColor", Color.Lerp(sprintDarkColor, Color.yellow, Mathf.PingPong(Time.time * 3.7f, 1f)));
                rend.material.SetFloat("_RimPower", 0.5f);
            }
            else if (aiming && !reloading)
            {
                rend.material.SetColor("_RimColor", Color.Lerp(Color.black, Color.yellow, Mathf.PingPong(Time.time * 2, 1f)));
                rend.material.SetFloat("_RimPower", initalRimPower);
            }
            else if (reloading || reloading && aiming || notFacing)
            {
                rend.material.SetColor("_RimColor", Color.red);
                rend.material.SetFloat("_RimPower", initalRimPower);
            }
            else
            {
                rend.material.SetColor("_RimColor", initalColor);
                rend.material.SetFloat("_RimPower", initalRimPower);
            }
        }
    }

    void FixedUpdate()
    {
        onGround = IsOnGround();
    }

    bool IsOnGround()
    {
        bool retVal = false;

        Vector3 origin = transform.position + new Vector3(0f, 0.05f, 0f);
        RaycastHit hit;

        //Debug.DrawRay(origin, -Vector3.up);

        if (Physics.Raycast(origin, direction, out hit, 1f, layerMask))
        {
            retVal = true;
        }
        return retVal;
    }
}
