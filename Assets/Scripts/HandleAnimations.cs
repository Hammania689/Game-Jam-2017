using UnityEngine;
using System.Collections;

public class HandleAnimations : MonoBehaviour
{

    #region Global Variable Declaration

    //public Animator anim;
    StateManager states;

    Vector3 lookDirection;

    #endregion

    void Start()
    {
        states = GetComponent<StateManager>();
        SetupAnimator();
    }

    void FixedUpdate()
    {
        if (!GameMasterObject.isPlayerActive)
        {
            return;
        }

        //anim.SetBool("Aim", states.aiming);


        if (!states.canRun)
        {
            //anim.SetFloat("Forward", states.vertical, 0.1f, Time.deltaTime);
            //anim.SetFloat("Sideways", states.horizontal, 0.1f, Time.deltaTime);
        }
        else
        {
            float movement = Mathf.Abs(states.vertical) + Mathf.Abs(states.horizontal);

            bool sprint = states.sprint;

            if (states.reloading || states.sprint)
            {
                movement = Mathf.Clamp(movement, 0, 1);
            }
            else if (states.aiming && !states.reloading && !states.sprint)
            {
                movement = Mathf.Clamp(movement, 0, 0.5f);
            }

            //anim.SetFloat("Forward", movement, 0.1f, Time.deltaTime);
        }
    }

    void SetupAnimator()
    {
        //anim = GetComponent<Animator>();

        Animator[] anims = GetComponentsInChildren<Animator>();

        for (int i = 0; i < anims.Length; i++)
        {
            //if (anims[i] != anim)
            {
                if (anims[i].enabled)
                {
                    //anim.avatar = anims[i].avatar;
                    Destroy(anims[i]);
                    break;
                }
            }
        }
    }
}