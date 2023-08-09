using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimatorOnline : NetworkBehaviour
{
    private Animator animator;
    //private PlayerTurnOnline PlayerTurnOnline;

    void Awake()
    {
        animator = GetComponent<Animator>();
        //PlayerTurnOnline = GetComponent<PlayerTurnOnline>();
    }

    void Start()
    {
        
    }
    
    public void ChangeAnim(string animName)
    {
        if (!IsOwner)
        {
            return;
        }
        if (animName == "Up_Idle")
        {
            animator.CrossFade("Up_Idle", 0, 0);
        }
        else if (animName == "Down_Idle")
        {
            animator.CrossFade("Down_Idle", 0, 0);
        }
        else if (animName == "Left_Idle")
        {
            animator.CrossFade("Left_Idle", 0, 0);
        }
        else if (animName == "Right_Idle")
        {
            animator.CrossFade("Right_Idle", 0, 0);
        }
        else if (animName == "Up_Walk")
        {
            animator.CrossFade("Up_Walk", 0, 0);
        }
        else if (animName == "Down_Walk")
        {
            animator.CrossFade("Down_Walk", 0, 0);
        }
        else if (animName == "Left_Walk")
        {
            animator.CrossFade("Left_Walk", 0, 0);
        }
        else if (animName == "Right_Walk")
        {
            animator.CrossFade("Right_Walk", 0, 0);
        }
        
    }
    
    /*public void ChangeAnim(string animName)
    {
        if (IsOwner)
        {
            if (animName == "Up_Idle")
            {
                animator.CrossFade("Up_Idle", 0, 0);
            }
            else if (animName == "Down_Idle")
            {
                animator.CrossFade("Down_Idle", 0, 0);
            }
            else if (animName == "Left_Idle")
            {
                animator.CrossFade("Left_Idle", 0, 0);
            }
            else if (animName == "Right_Idle")
            {
                animator.CrossFade("Right_Idle", 0, 0);
            }
            else if (animName == "Up_Walk")
            {
                animator.CrossFade("Up_Walk", 0, 0);
            }
            else if (animName == "Down_Walk")
            {
                animator.CrossFade("Down_Walk", 0, 0);
            }
            else if (animName == "Left_Walk")
            {
                animator.CrossFade("Left_Walk", 0, 0);
            }
            else if (animName == "Right_Walk")
            {
                animator.CrossFade("Right_Walk", 0, 0);
            }
        }
        
    }*/

    
    /*public void CheckFacing()
    {
        if (IsOwner)
        {
            if (PlayerTurnOnline.facingUp)
            {
                c("Up_Idle");
            }
            else if (PlayerTurnOnline.facingDown)
            {
                ChangeAnim("Down_Idle");
            }
            else if (PlayerTurnOnline.facingLeft)
            {
                ChangeAnim("Left_Idle");
            }
            else if (PlayerTurnOnline.facingRight)
            {
                ChangeAnim("Right_Idle");
            }
        }
        
    }*/
}