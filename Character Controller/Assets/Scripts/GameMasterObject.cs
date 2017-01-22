using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameMasterObject : MonoBehaviour
{
    #region Global Variable Declaration

    #region static members
    public static List<Transform> enemies;
    public static List<Transform> enemyCreeps;
    public static List<Transform> dragonsMonsters;
    public static List<Transform> defenseItems;
    public static List<GameObject> impacts;

	public static List<GameObject> bullets = new List<GameObject>();
	public static Dictionary<GameObject, long> bullets_life = new Dictionary<GameObject, long>();
	public static WaterOnTerrain fluid_background;
    
    public static GameObject playerUse;
    public static Camera camInUse;
        
    public static List<Transform> enemySpawnPoints;

    public static Slider heroesSlider;

    public static int playerLevel = 0;
    public static int enemyLevel = 0;

    public static bool isPlayerActive = true;

    public static bool isTPSState = false;
    public static bool isTWODEEState = false;

    public static bool setPlayerPosition = false;
    #endregion

    #region nonstatic public members

    [Header("Players and Cameras")]
    public GameObject dannyTPS;
    public GameObject tpsCam;

    public bool isTPS = false;
    public bool isTWODEE = false;
    
    public bool needImpacts = false;

    public GameObject impactPrefab;
    public float pooledImpactsAmount = 100;

    public Text timerText;
    public Text timerText2;

    public Text PlayerPoints;
    public Text EnemyPoints;

    public Text PlayerLevels;
    public Text EnemyLevels;

    public GameObject ridicule;

    public bool lockCursor = false;
    public GameObject inactiveCanvas;


	public GameObject fluid_background_quad;
    #endregion

    #region nonstatic private members

    bool tps = false;
    bool twodee = false;
    InputHandler ih;

    #endregion

    #endregion

    public static GameMasterObject Instance;

    public static GameMasterObject GetInstance()
    {
        return Instance;
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        enemies = new List<Transform>();
        defenseItems = new List<Transform>();
        enemySpawnPoints = new List<Transform>();
        enemyCreeps = new List<Transform>();
        dragonsMonsters = new List<Transform>();

        if (needImpacts)
        {
            impacts = new List<GameObject>();
        }

        string todaysDate = DateTime.Now.ToString("MM-dd-yyyy");

        //		Debug.Log (inventory + " awake");
    }

    void Start()
    {
        ih = dannyTPS.GetComponent<InputHandler>();
        #region pooled impact prefabs
        if (needImpacts)
        {
            for (int i = 0; i < pooledImpactsAmount; i++)
            {
                GameObject impact = (GameObject)Instantiate(impactPrefab);
                impact.SetActive(false);
                impacts.Add(impact);
            }
        }
        #endregion
        
        
        if (isTPS && !isTWODEE)
        {
            SetToTPS();
        }
        else if (!isTPS && isTWODEE)
        {
            SetToTWODEE();
        }
        
        // Debug.Log ();

        isTPSState = isTPS;
        isTWODEEState = isTWODEE;

    }

    void Update()
    {

        // Debug.Log(playerLevel);
        // Debug.Log ();
        // Debug.Log();

        isTPSState = isTPS;
        isTWODEEState = isTWODEE;

        playerUse = dannyTPS;

        if (tps && !twodee)
        {
            isTPS = true;
            isTWODEE = false;

            if (Input.GetButtonDown("Chat"))
            {
                lockCursor = !lockCursor;
            }

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isPlayerActive = true;
                inactiveCanvas.SetActive(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isPlayerActive = false;
                inactiveCanvas.SetActive(true);
            }
        }
        else if (twodee && !tps)
        {
            isTWODEE = true;
            isTPS = false; 

            if (Input.GetButtonDown("Chat"))
            {
                lockCursor = !lockCursor;
            }

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isPlayerActive = true;
                inactiveCanvas.SetActive(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isPlayerActive = false;
                inactiveCanvas.SetActive(true);
            }
        }


        if (isTPS)
        {
            if (Input.GetButtonDown("Orient"))
            {
                SetToTWODEE();
                Debug.Log("2-D");
            }
        }
        else if (isTWODEE)
        {
            if (Input.GetButtonDown("Orient"))
            {
                SetToTPS();
                Debug.Log("TPS");
            }
        }
        
        
        if (EnemyLevels != null)
        {
            EnemyLevels.text = enemyLevel.ToString("00");
        }
        if (PlayerLevels != null)
        {
            PlayerLevels.text = playerLevel.ToString("00");
        }

        float timerSeconds = 0.0f;
        float timerMinutes = 0.0f;
        timerSeconds = Time.time;
        timerMinutes = Time.time;
        string minutes = Mathf.Floor(timerMinutes / 60).ToString("00");
        string seconds = Mathf.Floor(timerSeconds % 60).ToString("00");

        if (timerText != null)
        {
            timerText.text = seconds;
        }
        if (timerText2 != null)
        {
            timerText2.text = minutes;
        }   

		// Interact with background.
		long curr_tick = System.DateTime.Now.Ticks;
		const long LIFELIMIT = 500 * 10000; // 10000 Ticks = 1 Millisecond
		for (int i=0; i<bullets.Count; i++) {
			GameObject bullet = bullets [i];
			if (bullets_life [bullet] < curr_tick - LIFELIMIT)
				continue;
			Vector3 l = fluid_background_quad.transform.InverseTransformPoint (bullet.transform.position);
			fluid_background.AddSomeFluid (l.x, l.y);
		}
    }       

    public void SetToTPS()
    {
        tps = true;
        twodee = false;
        
        //ridicule.SetActive(true);

        //lockCursor = true;
    }

    public void SetToTWODEE()
    {
        ih.SetCamToPos();

        tps = false;
        twodee = true;
        
        //ridicule.SetActive(true);

        //lockCursor = true;
    }    
}
