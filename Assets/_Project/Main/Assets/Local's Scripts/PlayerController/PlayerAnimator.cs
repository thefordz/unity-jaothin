using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerMove playerMove;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMove = GetComponent<PlayerMove>();
    }

    void Start()
    {
        
    }

    public void ChangeAnim(string animName)
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

    public void CheckFacing()
    {
        if (playerMove.facingUp)
        {
            ChangeAnim("Up_Idle");
        }
        else if (playerMove.facingDown)
        {
            ChangeAnim("Down_Idle");
        }
        else if (playerMove.facingLeft)
        {
            ChangeAnim("Left_Idle");
        }
        else if (playerMove.facingRight)
        {
            ChangeAnim("Right_Idle");
        }
    }
}