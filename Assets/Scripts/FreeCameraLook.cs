using UnityEngine;
using System.Collections;

public class FreeCameraLook : Pivot 
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] float turnSpeed = 3.5f;
	[SerializeField] public float turnSmoothing = 0.1f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;

    [Header("Public Members")]
    public GameObject player;
    public ParticleSystem burst;
    public bool keyOrJoystick = false;
    public bool tps = false;
    public bool twoDEE = true;
    public float setTurnSpeed;
    public float crosshairOffsetWiggle = 0.2f;
    public Camera camera;

    private float lookAngle;
	private float tiltAngle;
    
	private float smoothX = 0;
	private float smoothY = 0;
	private float smoothXVelocity = 0;
	private float smoothYVelocity = 0;

    private const float lookDistance = 100f;
    
	GameMasterObject gammaO;
	StateManager states;

    public static FreeCameraLook instance;
    public static FreeCameraLook GetInstance()
	{
		return instance;
	}

	protected override void Awake()
	{
		instance = this;

		base.Awake ();

		Cursor.lockState = CursorLockMode.Confined;

		cam = GetComponentInChildren<Camera> ().transform;
		pivot = cam.parent.parent;

		setTurnSpeed = turnSpeed;
		camera = GetComponentInChildren<Camera> ();
	}

	protected override void Start () 
	{
		base.Start ();
	}

	protected override void Update () 
	{
		base.Update ();

        if (GameMasterObject.isTPSState)
        {
            HandleRotationMovement();
        }
        else if (GameMasterObject.isTWODEEState)
        {
            HandleSideLock();
        }
		player = GameMasterObject.playerUse;
		if(player != null)
		{
			states = player.GetComponent<StateManager> ();

			if(states != null)
			{
				if(states.sprint)
				{
					turnSpeed = 2.25f;
				}
				else if(!states.sprint)
				{
					turnSpeed = 3.5f;
				}
			}
		}
	}

	void OnEnable()
	{		
		GameMasterObject.camInUse = camera;
	}

	void OnDisable()
	{
	    //Cursor.lockState = CursorLockMode.None;
	}

	protected override void Follow(float deltaTime)
	{
		transform.position = Vector3.Lerp (transform.position, target.position, deltaTime * moveSpeed);
	}

    void HandleSideLock()
    {

    }


    void HandleRotationMovement()
	{
		if(keyOrJoystick)
		{
			float x = Input.GetAxis ("horRot");
			float y = Input.GetAxis ("verRot");

			if (turnSmoothing > 0) 
			{
				smoothX = Mathf.SmoothDamp (smoothX, x, ref smoothXVelocity, turnSmoothing);	
				smoothY = Mathf.SmoothDamp (smoothY, y, ref smoothYVelocity, turnSmoothing);	
			} 
			else 
			{
				smoothX = x;
				smoothY = y;
			}

			lookAngle += smoothX * turnSpeed;

			transform.rotation = Quaternion.Euler (0f, lookAngle, 0f);

			tiltAngle -= smoothY * turnSpeed;
			tiltAngle = Mathf.Clamp (tiltAngle, - tiltMin, tiltMax);

			pivot.localRotation = Quaternion.Euler (tiltAngle, 0f, 0f);
		}
		else if(!keyOrJoystick)
		{
			float x = Input.GetAxis ("horRot");
			float y = Input.GetAxis ("verRot");

			if (turnSmoothing > 0) 
			{
				smoothX = Mathf.SmoothDamp (smoothX, x, ref smoothXVelocity, turnSmoothing);	
				smoothY = Mathf.SmoothDamp (smoothY, y, ref smoothYVelocity, turnSmoothing);	
			} 
			else 
			{
				smoothX = x;
				smoothY = y;
			}

			lookAngle += smoothX * turnSpeed;

			transform.rotation = Quaternion.Euler (0f, lookAngle, 0f);

			tiltAngle -= smoothY * turnSpeed;
			tiltAngle = Mathf.Clamp (tiltAngle, - tiltMin, tiltMax);

			pivot.localRotation = Quaternion.Euler (tiltAngle, 0f, 0f);	
		}
	}
}
