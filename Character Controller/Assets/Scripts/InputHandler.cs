using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour
{
    #region Global Variable Declaration

    [Header("Input Holders")]
    public float horizontal;
    public float vertical;
    public float fire;
    public float aim;
    public float scrollWheel;
    public float horRot;
    public float verRot;
    public float sprint;
	public bool jump;

	public GameObject propulsion_quad;

    [Header("Camera Components")]
    public FreeCameraLook camProperties;
    public Transform camPivot;
    public Transform camTrans;
    public Transform cameraPositionHandler;

    [Header("Camera FOV")]
    public float normalFov = 60f;
    public float aimingFov = 40f;
    public float sprintFov = 100f;

    [Header("Camera Collision")]
    public float currentXOffset = 0.5f;
    public float cameraNormalZ = -2f;
    public float cameraOrthoZ = -40f;
    public float cameraAimingZ = 0.96f;
    public Vector3 initialPosition;

    public float shakeRecoil = 0.5f;
    public float shakeMovement = 0.3f;
    public float shakeMin = 0.1f;

    [Header("Shooting parts")]
    public Transform bulletSpawnPoint;
    public LayerMask layerMask;

    float targetZ;
    float actualZ;
    float curZ;
    float targetFov;
    float curFov;
    float targetShake;
    float curShake;

    [HideInInspector]
    ShakeCamera shakeCam;
    [HideInInspector]
    StateManager states;


    
    #endregion

    void Start()
    {
        camProperties = FreeCameraLook.GetInstance();
        camPivot = camProperties.transform.GetChild(0);
        camTrans = camPivot.GetChild(0);
        shakeCam = camPivot.GetComponentInChildren<ShakeCamera>();

        states = GetComponent<StateManager>();
        
        states.layerMask = layerMask;

        initialPosition = new Vector3(0f, 0f, -40f);
    }

    void Update()
    {
        if (!GameMasterObject.isPlayerActive)
        {
            return;
        }

        if (GameMasterObject.isTPSState)
        {
            currentXOffset = 0.5f;
            cameraNormalZ = -2f;
            cameraAimingZ = 0.96f;
        }
        else if (GameMasterObject.isTWODEEState)
        {
            currentXOffset = 0.0f;
            cameraNormalZ = cameraOrthoZ;
            cameraAimingZ = cameraOrthoZ;
        }

        //		Debug.Log (states.onGround);
        //		Debug.Log (states.sprint);
    }

    void FixedUpdate()
    {
        if (!GameMasterObject.isPlayerActive)
        {
            return;
        }

        HandleInput();
        UpdateStates();
        HandleFOV();

        //Find where the camera is looking
        Ray ray = new Ray(camTrans.position, camTrans.forward);
        states.lookPosition = ray.GetPoint(100);
        RaycastHit hit;

        // Debug.DrawRay (ray.origin, ray.direction);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100, layerMask))
        {
            states.lookHitPosition = hit.point;
        }
        else
        {
            states.lookHitPosition = states.lookPosition;
        }
        CameraCollision(layerMask);

        if (!states.sprint && !states.aiming)
        {
            curZ = Mathf.Lerp(curZ, actualZ, Time.deltaTime * 5);
            camTrans.localPosition = new Vector3(currentXOffset, 0f, curZ);
        }
        else if (states.sprint && !states.aiming)
        {
            curZ = Mathf.Lerp(curZ, actualZ, Time.deltaTime * 5);
            camTrans.localPosition = new Vector3(currentXOffset, 0f, curZ);
        }
        else if (states.aiming && !states.sprint)
        {
            curZ = Mathf.Lerp(curZ, actualZ, Time.deltaTime * 5);
            camTrans.localPosition = new Vector3(currentXOffset, 0f, curZ);
        }        
    }

    void HandleInput()
    {
        horizontal = Input.GetAxisRaw("horizontal");
        vertical = Input.GetAxisRaw("vertical");
        horRot = Input.GetAxis("horRot");
        verRot = Input.GetAxis("verRot");
        scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        fire = Input.GetAxis("Fire");
        aim = Input.GetAxisRaw("Aim");
        sprint = Input.GetAxisRaw("Sprint");
        jump = Input.GetButtonDown("Jump");
    }

    void UpdateStates()
    {
        if (states.onGround)
        {
            if (sprint > 0 && !states.jumping)
            {
                states.sprint = true;
            }
            else if (sprint <= 0)
            {
                states.sprint = false;
            }

            if (sprint <= 0)
            {
                if (aim > 0)
                {
                    states.aiming = true;
                }
                else if (aim <= 0)
                {
                    states.aiming = false;
                }
            }
            else if (sprint > 0 || states.jumping)
            {
                states.aiming = false;
            }

        }
        else
        {
            states.aiming = false;
            states.sprint = false;
        }
        states.canRun = !states.aiming;

        states.horizontal = horizontal;
        states.vertical = vertical;


		if (jump == true && states.jumping == false) {
			FluidSim fs = propulsion_quad.GetComponent<FluidSim> ();
			fs.OnCapsuleJumped ();
		}

        states.jumping = jump;
        

        if (!states.aiming && !states.sprint && !states.reloading)
        {
            targetZ = cameraNormalZ; //update target z position of cam
            targetFov = normalFov;

            if (fire > 0.5 && !states.reloading)
            {
                states.shoot = true;
            }
            else
            {
                states.shoot = false;
            }
        }
        else if (states.aiming && !states.sprint && !states.reloading)
        {
            targetZ = cameraAimingZ; //update target z position of cam
            targetFov = aimingFov;

            if (fire > 0.5 && !states.reloading)
            {
                states.shoot = true;
            }
            else
            {
                states.shoot = false;
            }
        }
        else if (!states.aiming && states.sprint && !states.reloading)
        {
            if (horizontal > 0 || vertical > 0)
            {
                targetFov = sprintFov;
                targetZ = cameraAimingZ;
            }
            else if (horizontal <= 0 || vertical <= 0)
            {
                targetFov = normalFov;
                targetZ = cameraNormalZ;
            }
        }
        else
        {
            states.shoot = false;
            targetZ = cameraNormalZ;
            targetFov = normalFov;
        }
    }

    void HandleFOV()
    {
        if (states.shoot)
        {
            targetShake = shakeRecoil;
            targetFov += 5;
        }
        else
        {
            if (states.vertical != 0)
            {
                targetShake = shakeMovement;
            }
            else
            {
                if (states.horizontal != 0)
                {
                    targetShake = shakeMovement;
                }
                else
                {

                    targetShake = shakeMin;
                }
            }
        }

        curShake = Mathf.Lerp(curShake, targetShake, Time.deltaTime * 10);
        
        curFov = Mathf.Lerp(curFov, targetFov, Time.deltaTime * 5);
        Camera.main.fieldOfView = curFov;
    }    

    void CameraCollision(LayerMask layerMask)
    {
        Vector3 origin = camPivot.TransformPoint(Vector3.zero);
        Vector3 direction = camTrans.TransformPoint(Vector3.zero) - camPivot.TransformPoint(Vector3.zero);
        RaycastHit hit;

        //distance controlled by if we are or are not aiming
        actualZ = targetZ;

        if (Physics.Raycast(origin, direction, out hit, Mathf.Abs(targetZ), layerMask))
        {
            //Debug.Log(hit.collider.gameObject.name);
            float dis = Vector3.Distance(camPivot.position, hit.point);
            actualZ = -dis;     //the opposite of that is where we want to place our camera
        }
    }
    
    public void SetCamToPos()
    {
        if (camTrans != null)
        {
            camTrans.localPosition = cameraPositionHandler.position;
        }
    }
}
