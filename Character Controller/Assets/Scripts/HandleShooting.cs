using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HandleShooting : MonoBehaviour
{
    #region Global Variable Declaration

    StateManager states;
    public float fireRate;
    public float reloadTime = 2;

    [SerializeField]
    float timer;

    public Transform bulletSpawnPoint;
    public GameObject bullets;
    public Transform tracerBulletSpawn;
    public GameObject smokeParticle;
    public ParticleSystem[] muzzle;
    public GameObject casingPrefab;
    public Transform caseSpawn;
    public LayerMask layerMask;
    public float damage = 30;
    public float shootDistance = 250f;
    public float bulletSpeed = 1f;

    public Transform graphicBulletSpawn;

    //public Image ridicule;
    
    public float amplitude;
    public float duration;

    public float hitForce = 50f;

    bool shoot;
    bool dontShoot;
    
    public IKHandler ikhandler;

    Color initialColor;

    #endregion

    void Start()
    {
        states = GetComponent<StateManager>();
        ikhandler = GetComponent<IKHandler>();
        
        //initialColor = ridicule.color;
    }

    void Update()
    {
        if (!GameMasterObject.isPlayerActive)
        {
            return;
        }

        shoot = states.shoot;

        if (shoot /* && !ikhandler.notFacing*/)
        {
            if (timer <= 0)
            {
                // weaponAnim.SetBool("Shoot", false);
                
                // states.audioManager.PlayGunSound();

                ShakeCamera.InstanceSM1.ShakeSM1(amplitude, duration);

                if (casingPrefab != null)
                {
                    GameObject go = Instantiate(casingPrefab, caseSpawn.position, caseSpawn.rotation) as GameObject;
                    Rigidbody rig = go.GetComponent<Rigidbody>();
                    rig.AddForce(transform.right.normalized * 2 + Vector3.up * 1.3f, ForceMode.Impulse);
                    rig.AddRelativeTorque(go.transform.right * 1.5f, ForceMode.Impulse);
                }
                for (int i = 0; i < muzzle.Length; i++)
                {
                    muzzle[i].Emit(1);
                }

                GameObject thisobj = Instantiate((GameObject)bullets, graphicBulletSpawn.position, graphicBulletSpawn.rotation);

                Rigidbody rb = thisobj.GetComponent<Rigidbody>();                
                if (rb != null)
                {
                    rb.velocity = graphicBulletSpawn.forward * bulletSpeed;
                }

                RaycastShoot();

                timer = fireRate;
            }
            else if (timer > 0)
            {
                timer -= Time.deltaTime;
            }

        }
        else if (!shoot)
        {
            timer = -1;
        }       
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        Transform raySpawnPoint = bulletSpawnPoint;

        if (Physics.Raycast(raySpawnPoint.position, raySpawnPoint.forward, out hit))
        {
            // if (hit.collider.gameObject.tag == "Enemy")
            // {
            //     ridicule.color = Color.red;
            // }
            // else
            // {
            //     ridicule.color = initialColor;
            // }
        }

    }
       
    void RaycastShoot()
    {
        Vector3 direction = states.lookHitPosition - bulletSpawnPoint.position;
        RaycastHit hit;

        if (Physics.Raycast(bulletSpawnPoint.position, direction, out hit, shootDistance, layerMask))
        {
            // Debug.Log(hit.collider.gameObject.name);

            if (smokeParticle != null)
            {
                GameObject go = Instantiate(smokeParticle, hit.point, Quaternion.identity) as GameObject;
                go.transform.LookAt(bulletSpawnPoint.position);
            }
            if (hit.collider.tag == "Enemy")
            {
                IDamageable enemy = hit.transform.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    // Debug.Log("Enemy");
                    enemy.TakeDamage(damage, hit.point);
                }
            }

            if (hit.collider.tag == "Foe")
            {
                IDamageable enemy = hit.transform.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    //					Debug.Log ("Foe");
                    enemy.TakeDamage(damage, hit.point);
                }
            }
            
            if (hit.collider.tag == "Damageable Prop")
            {
                // Debug.Log ("Damageable Prop");
                IDamageable prop = hit.transform.GetComponent<IDamageable>();
                Rigidbody propBody = hit.transform.GetComponent<Rigidbody>();
                if (prop != null)
                {
                    // Debug.Log ("Hit");
                    prop.TakeDamage(damage, hit.point);
                }
                if (propBody != null)
                {
                    // Debug.Log ("Push");
                    propBody.AddForce(-hit.normal * hitForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnEnable()
    {
        timer = 2f;
    }

}