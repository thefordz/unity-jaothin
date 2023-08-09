using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour {
    [SerializeField] private float moveSpeed = 3;
    public Transform movePoint;
    private Animator animator;


    private void Start()
    {
        movePoint.parent = null;
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed*Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            if (Input.GetAxisRaw("Horizontal") == 1f)
            {
                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                animator.CrossFade("Right_Walk", 0, 0);
                if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") ==0) 
                {
                    animator.CrossFade("Right_Idle", 0, 0);
                }
            }
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                animator.CrossFade("Up_Walk", 0, 0);
                if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") ==0) 
                {
                    animator.CrossFade("Up_Idle", 0, 0);
                }
            }
            if (Input.GetAxisRaw("Horizontal") == -1f)
            {
                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                animator.CrossFade("Left_Walk", 0, 0);
                if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") ==0) 
                {
                    animator.CrossFade("Left_Idle", 0, 0);
                }
            }
            if (Input.GetAxisRaw("Vertical") == -1f)
            {
                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                animator.CrossFade("Down_Walk", 0, 0);
                if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") ==0) 
                {
                    animator.CrossFade("Down_Idle", 0, 0);
                }
            }
            
        }
        
    }
    
    

    public override void OnNetworkSpawn() {
        if (!IsOwner) Destroy(this);
    }
    
    

}