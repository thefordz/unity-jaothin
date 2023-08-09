using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;
using Random = UnityEngine.Random;

public class PlayerTurnOnline : NetworkBehaviour
{
    [Header("players statuses")] public NetworkVariable<int> health = new NetworkVariable<int>();

    [Header("Check Player Turn")] public NetworkVariable<bool> isMyTurn = new NetworkVariable<bool>();

    [Header("Per move speed")]
    //move speed
    public float moveToGridSpeed = 5f;

    [Header("Grids")] public GameObject currentGrid;
    public GameObject enemyGrid;
    public GameObject leftGrid;
    public GameObject rightGrid;
    public GameObject upperGrid;
    public GameObject upperLeftGrid;
    public GameObject upperRightGrid;
    public GameObject lowerGrid;
    public GameObject lowerLeftGrid;
    public GameObject lowerRightGrid;

    [Header("Where character looking")] public bool facingUp;
    public bool facingDown;
    public bool facingLeft;
    public bool facingRight;

    [Header("Where can move")] public bool canMoveUp;
    public bool canMoveDown;
    public bool canMoveLeft;
    public bool canMoveRight;

    [Header("Move")]
    //Only offline _ableToPressMove and myturn
    public NetworkVariable<bool> isDiceRollPhase = new NetworkVariable<bool>();

    public NetworkVariable<bool> isMovePhase = new NetworkVariable<bool>();
    public NetworkVariable<bool> isAbleToPressMove = new NetworkVariable<bool>();
    public NetworkVariable<int> currentPointToMove = new NetworkVariable<int>();
    public NetworkVariable<bool> isCheckUiRoll = new NetworkVariable<bool>();

    [Header("Move indicators")]
    //player direction indicator
    public GameObject _indiUp;

    public GameObject _indiDown;
    public GameObject _indiLeft;
    public GameObject _indiRight;


    public bool m_isPlayerDefeated;

    [Header("Animator")] [SerializeField] private Animator m_animator;


    [SerializeField] CharacterDataSO m_characterData;

    [Header("Runtime set")] [HideInInspector]
    public PlayerUI playerUI;

    [HideInInspector] public CharacterDataSO characterData;
    [HideInInspector] public GameplayManager gameplayManager;

    [Header("Player Sound")] [SerializeField]
    AudioClip hitSound;

    [SerializeField] AudioClip walkSound;
    [SerializeField] private AudioClip getFoodSound;

    [SerializeField] private CameraShaking _camShaking;

    [Header("Another player")] [SerializeField]
    public GameObject currentGridForAll;

    [SerializeField] public PlayerTurnOnline anotherPlayer;
    [SerializeField] public bool knockbackUp = false;
    [SerializeField] public bool knockbackDown = false;
    [SerializeField] public bool knockbackLeft = false;
    [SerializeField] public bool knockbackRight = false;

    private void Start()
    {
        if (this.gameObject.name == "Orange(Clone)")
        {
            anotherPlayer = GameObject.Find("SriNuan(Clone)").GetComponent<PlayerTurnOnline>();
            Debug.Log(anotherPlayer.name);
        }

        if (this.gameObject.name == "SriNuan(Clone)")
        {
            anotherPlayer = GameObject.Find("Orange(Clone)").GetComponent<PlayerTurnOnline>();
            Debug.Log(anotherPlayer.name);
        }

        _camShaking = GameObject.Find("Main Camera").GetComponent<CameraShaking>();
        isAbleToPressMove.Value = false;
        //Default Set to flase , Set up True For Test

        //Check Where Character Facing (Just for game start state)
        CheckMyGrid();
        if (currentGrid.name == GameplayManager.Instance.mapTopLeftGrid.name)
        {
            UpdateFaceingServerRpc(false, false, false, true);
        }
        else if (currentGrid.name == GameplayManager.Instance.mapTopRightGrid.name)
        {
            UpdateFaceingServerRpc(false, true, false, false);
        }
        else if (currentGrid.name == GameplayManager.Instance.mapBottomLeftGrid.name)
        {
            UpdateFaceingServerRpc(true, false, false, false);
        }
        else if (currentGrid.name == GameplayManager.Instance.mapBottomRightGrid.name)
        {
            UpdateFaceingServerRpc(false, false, true, false);
        }
        
        CheckFacing();
        CheckGridsAroundPlayer();
        CheckWhereCanMove();

        if (OwnerClientId == 0)
        {
            CheckPlayersGridServerRpc(0);
        }
        else if (OwnerClientId == 1)
        {
            CheckPlayersGridServerRpc(1);
        }
    }

    private void Update()
    {
        if (isMyTurn.Value && IsOwner && !m_isPlayerDefeated)
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsServer)
                {
                    StartCoroutine(HostShutdown());
                }
                else
                {
                    Shutdown();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                NormalDiceRollServerRpc();
                //GameplayManager.Instance.NormalDiceRollServerRpc();
            }

            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                int randomFirstPlayer = Random.Range(0, 2);
                Debug.Log("" + randomFirstPlayer);
                //Debug.Log(GameplayManager.Instance.m_players.Count);

            }*/

            /*if (Input.GetKeyDown(KeyCode.Q))
            {
                //GameplayManager.Instance.EndTurn();
                EndTurnServerRpc();
            }*/
            
            if (isMovePhase.Value && isAbleToPressMove.Value)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (canMoveUp)
                    {
                        MoveUp();
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (canMoveDown)
                    {
                        MoveDown();
                    }
                }

                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (canMoveLeft)
                    {
                        MoveLeft();
                    }
                }

                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (canMoveRight)
                    {
                        MoveRight();
                    }
                }
            }
        }

        if (knockbackUp == true)
        {
            Debug.Log("Knockbacked Up!!!");
            if (upperGrid.tag != "Wall")
            {
                StartCoroutine(Knockback(upperGrid.transform.position));
            }
            else if (upperGrid.tag == "Wall")
            {
                StartCoroutine(Knockback(currentGrid.transform.position));
            }

            knockbackUp = false;
        }
        else if (knockbackDown == true)
        {
            Debug.Log("Knockbacked Down!!!");
            if (lowerGrid.tag != "Wall")
            {
                StartCoroutine(Knockback(lowerGrid.transform.position));
            }
            else if (lowerGrid.tag == "Wall")
            {
                StartCoroutine(Knockback(currentGrid.transform.position));
            }

            knockbackDown = false;
        }
        else if (knockbackLeft == true)
        {
            Debug.Log("Knockbacked Left!!!");
            if (leftGrid.tag != "Wall")
            {
                StartCoroutine(Knockback(leftGrid.transform.position));
            }
            else if (leftGrid.tag == "Wall")
            {
                StartCoroutine(Knockback(currentGrid.transform.position));
            }

            knockbackLeft = false;
        }
        else if (knockbackRight == true)
        {
            Debug.Log("Knockbacked Right!!!");
            if (rightGrid.tag != "Wall")
            {
                StartCoroutine(Knockback(rightGrid.transform.position));
            }
            else if (rightGrid.tag == "Wall")
            {
                StartCoroutine(Knockback(currentGrid.transform.position));
            }

            knockbackRight = false;
        }
    }


    IEnumerator HostShutdown()
    {
        // Tell the clients to shutdown
        ShutdownClientRpc();

        // Wait some time for the message to get to clients
        yield return new WaitForSeconds(0.5f);

        // Shutdown server/host
        Shutdown();
    }

    // Shutdown the network session and load the menu scene
    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuV2, false);
    }

    [ClientRpc]
    void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }


    private void MoveUp()
    {
        StartCoroutine(Moving(upperGrid.transform.position));
        UpdateFaceingServerRpc(true, false, false, false);
        ChangeAnim("Up_Walk");
        UpdateCurrentFaceUiServerRpc(false, false, false, false);
    }


    private void MoveDown()
    {
        StartCoroutine(Moving(lowerGrid.transform.position));
        UpdateFaceingServerRpc(false, true, false, false);
        ChangeAnim("Down_Walk");
        UpdateCurrentFaceUiServerRpc(false, false, false, false);
    }


    private void MoveLeft()
    {
        Debug.Log("MoveLeft");
        StartCoroutine(Moving(leftGrid.transform.position));
        UpdateFaceingServerRpc(false, false, true, false);
        ChangeAnim("Left_Walk");
        UpdateCurrentFaceUiServerRpc(false, false, false, false);
    }


    private void MoveRight()
    {
        Debug.Log("MoveRight");
        StartCoroutine(Moving(rightGrid.transform.position));
        UpdateFaceingServerRpc(false, false, false, true);
        ChangeAnim("Right_Walk");
        UpdateCurrentFaceUiServerRpc(false, false, false, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IsAbleMoveServerRpc(bool checkAble)
    {
        isAbleToPressMove.Value = checkAble;
    }

    private IEnumerator Moving(Vector2 target)
    {
        WalkSoundServerRpc();
        //Debug.Log("IEnum Is Working");
        Vector2 current = transform.position;
        //Debug.Log(current);
        Vector2 direction = target - current;
        //isAbleToPressMove.Value = false;
        IsAbleMoveServerRpc(false);

        while (Vector2.Distance(current, target) >= 0.01)
        {
            current = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, target, moveToGridSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.001f);
        }

        CheckMyGrid();
        CheckGridsAroundPlayer();
        CheckFacing();
        //CheckPlayersGridServerRpc();
        CheckWhereCanMove();
        AfterMove();
        if (OwnerClientId == 0)
        {
            CheckPlayersGridServerRpc(0);
        }
        else if (OwnerClientId == 1)
        {
            CheckPlayersGridServerRpc(1);
        }
    }

    IEnumerator Knockback(Vector2 target)
    {
        Vector2 current = transform.position;
        Vector2 direction = target - current;
        CamShackingClientRpc();

        while (Vector2.Distance(current, target) >= 0.01)
        {
            current = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, target, 5 * Time.deltaTime);
            yield return new WaitForSeconds(0.001f);
        }

        CheckMyGrid();

        if (!currentGrid.CompareTag("Hole"))
        {
            CheckGridsAroundPlayer();
            //_gameControllerScript.CheckPlayersGrid();
            //CheckFacing();
            CheckWhereCanMove();
        }
        else if (currentGrid.CompareTag("Hole"))
        {
            HitServerRpc(100);
            //anotherPlayer.health.Value -= 1;
        }
    }

    [ClientRpc]
    void CamShackingClientRpc()
    {
        _camShaking.ShakeTheScreen(0.5f, 0.3f);
    }

    public void AfterMove()
    { 
        StopWalkSoundServerRpc();
        DicePointChangeServerRpc(-1);
        UpdateRollPointServerRpc(currentPointToMove.Value);
        if (currentPointToMove.Value != 0)
        {
            IsAbleMoveServerRpc(true);
        }
        else if (currentPointToMove.Value <= 0)
        {
            enemyGrid = anotherPlayer.currentGridForAll;
            IsAbleMoveServerRpc(false);
            if (facingUp && enemyGrid == upperGrid)
            {
                anotherPlayer.knockbackUp = true;
                anotherPlayer.HitServerRpc(1);
                Debug.Log(knockbackUp);
            }
            else if (facingDown && enemyGrid == lowerGrid)
            {
                anotherPlayer.knockbackDown = true;
                anotherPlayer.HitServerRpc(1);
                Debug.Log(knockbackDown);
            }
            else if (facingLeft && enemyGrid == leftGrid)
            {
                anotherPlayer.knockbackLeft = true;
                anotherPlayer.HitServerRpc(1);
                Debug.Log(knockbackLeft);
            }
            else if (facingRight && enemyGrid == rightGrid)
            {
                anotherPlayer.knockbackRight = true;
                anotherPlayer.HitServerRpc(1);
                Debug.Log(knockbackRight);
            }


            if (currentGrid.tag == "Food")
            {
                if (currentPointToMove.Value == 0)
                {
                    
                    GetFoodScoreServerRpc();
                    GetFoodSoundServerRpc();
                }
            }

            EndTurnServerRpc();
            UpdateScoreServerRpc();
            UpdateKnockbackStateServerRpc(anotherPlayer.knockbackUp, anotherPlayer.knockbackDown,
                anotherPlayer.knockbackLeft, anotherPlayer.knockbackRight);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateKnockbackStateServerRpc(bool up, bool down, bool left, bool right)
    {
        UpdateKnockbackStateClientRpc(up, down, left, right);
    }

    [ClientRpc]
    public void UpdateKnockbackStateClientRpc(bool up, bool down, bool left, bool right)
    {
        anotherPlayer.knockbackUp = up;
        anotherPlayer.knockbackDown = down;
        anotherPlayer.knockbackLeft = left;
        anotherPlayer.knockbackRight = right;
    }

    public void CheckMyGrid()
    {
        RaycastHit2D hitGrid =
            Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (hitGrid)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.green);
            Debug.Log("Player : " + this.OwnerClientId + "HIT : " + hitGrid.collider.name);
            currentGrid = (GameObject.Find(hitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red);
            Debug.Log("Don't Hit");
        }
        
        currentGridForAll = currentGrid;
        CheckMyGridForEnemyServerRpc(currentGrid.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckMyGridForEnemyServerRpc(string currentGridString)
    {
        CheckMyGridForEnemyClientRpc(currentGridString);
    }

    [ClientRpc]
    public void CheckMyGridForEnemyClientRpc(string currentGridString)
    {
        currentGridForAll = GameObject.Find(currentGridString);
    }

    public void CheckGridsAroundPlayer()
    {
        string current;
        var num = currentGrid.name.Substring(1);
        int i = int.Parse(num);

        if (currentGrid.name.Contains("A"))
        {
            current = ("A" + i);
            upperGrid = GameObject.Find("-A" + i);
            upperLeftGrid = GameObject.Find(("-A") + (i - 1));
            upperRightGrid = GameObject.Find(("-A") + (i + 1));
            leftGrid = GameObject.Find(("A") + (i - 1));
            rightGrid = GameObject.Find(("A") + (i + 1));
            lowerGrid = GameObject.Find(("B") + i);
            lowerLeftGrid = GameObject.Find(("B") + (i - 1));
            lowerRightGrid = GameObject.Find("B" + (i + 1));
        }
        else if (currentGrid.name.Contains("B"))
        {
            current = ("B" + i);
            upperGrid = GameObject.Find("A" + i);
            upperLeftGrid = GameObject.Find("A" + (i - 1));
            upperRightGrid = GameObject.Find("A" + (i + 1));
            leftGrid = GameObject.Find(("B") + (i - 1));
            rightGrid = GameObject.Find(("B") + (i + 1));
            lowerGrid = GameObject.Find(("C") + i);
            lowerLeftGrid = GameObject.Find(("C") + (i - 1));
            lowerRightGrid = GameObject.Find("C" + (i + 1));
        }
        else if (currentGrid.name.Contains("C"))
        {
            current = ("C" + i);
            upperGrid = GameObject.Find("B" + i);
            upperLeftGrid = GameObject.Find("B" + (i - 1));
            upperRightGrid = GameObject.Find("B" + (i + 1));
            leftGrid = GameObject.Find(("C") + (i - 1));
            rightGrid = GameObject.Find(("C") + (i + 1));
            lowerGrid = GameObject.Find(("D") + i);
            lowerLeftGrid = GameObject.Find(("D") + (i - 1));
            lowerRightGrid = GameObject.Find("D" + (i + 1));
        }
        else if (currentGrid.name.Contains("D"))
        {
            current = ("D" + i);
            upperGrid = GameObject.Find("C" + i);
            upperLeftGrid = GameObject.Find("C" + (i - 1));
            upperRightGrid = GameObject.Find("C" + (i + 1));
            leftGrid = GameObject.Find(("D") + (i - 1));
            rightGrid = GameObject.Find(("D") + (i + 1));
            lowerGrid = GameObject.Find(("E") + i);
            lowerLeftGrid = GameObject.Find(("E") + (i - 1));
            lowerRightGrid = GameObject.Find("E" + (i + 1));
        }
        else if (currentGrid.name.Contains("E"))
        {
            current = ("E" + i);
            upperGrid = GameObject.Find("D" + i);
            upperLeftGrid = GameObject.Find("D" + (i - 1));
            upperRightGrid = GameObject.Find("D" + (i + 1));
            leftGrid = GameObject.Find(("E") + (i - 1));
            rightGrid = GameObject.Find(("E") + (i + 1));
            lowerGrid = GameObject.Find(("F") + i);
            lowerLeftGrid = GameObject.Find(("F") + (i - 1));
            lowerRightGrid = GameObject.Find("F" + (i + 1));
        }
        else if (currentGrid.name.Contains("F"))
        {
            current = ("F" + i);
            upperGrid = GameObject.Find("E" + i);
            upperLeftGrid = GameObject.Find("E" + (i - 1));
            upperRightGrid = GameObject.Find("E" + (i + 1));
            leftGrid = GameObject.Find(("F") + (i - 1));
            rightGrid = GameObject.Find(("F") + (i + 1));
            lowerGrid = GameObject.Find(("G") + i);
            lowerLeftGrid = GameObject.Find(("G") + (i - 1));
            lowerRightGrid = GameObject.Find("G" + (i + 1));
        }
        else if (currentGrid.name.Contains("G"))
        {
            current = ("G" + i);
            upperGrid = GameObject.Find("F" + i);
            upperLeftGrid = GameObject.Find("F" + (i - 1));
            upperRightGrid = GameObject.Find("F" + (i + 1));
            leftGrid = GameObject.Find(("G") + (i - 1));
            rightGrid = GameObject.Find(("G") + (i + 1));
            lowerGrid = GameObject.Find(("H") + i);
            lowerLeftGrid = GameObject.Find(("H") + (i - 1));
            lowerRightGrid = GameObject.Find("H" + (i + 1));
        }
        else if (currentGrid.name.Contains("H"))
        {
            current = ("H" + i);
            upperGrid = GameObject.Find("G" + i);
            upperLeftGrid = GameObject.Find("G" + (i - 1));
            upperRightGrid = GameObject.Find("G" + (i + 1));
            leftGrid = GameObject.Find(("H") + (i - 1));
            rightGrid = GameObject.Find(("H") + (i + 1));
            lowerGrid = GameObject.Find(("I") + i);
            lowerLeftGrid = GameObject.Find(("I") + (i - 1));
            lowerRightGrid = GameObject.Find("I" + (i + 1));
        }
        else if (currentGrid.name.Contains("I"))
        {
            current = ("I" + i);
            upperGrid = GameObject.Find("H" + i);
            upperLeftGrid = GameObject.Find("H" + (i - 1));
            upperRightGrid = GameObject.Find("H" + (i + 1));
            leftGrid = GameObject.Find(("I") + (i - 1));
            rightGrid = GameObject.Find(("I") + (i + 1));
            lowerGrid = GameObject.Find(("J") + i);
            lowerLeftGrid = GameObject.Find(("J") + (i - 1));
            lowerRightGrid = GameObject.Find("J" + (i + 1));
        }
        else if (currentGrid.name.Contains("J"))
        {
            current = ("J" + i);
            upperGrid = GameObject.Find("I" + i);
            upperLeftGrid = GameObject.Find("I" + (i - 1));
            upperRightGrid = GameObject.Find("I" + (i + 1));
            leftGrid = GameObject.Find(("J") + (i - 1));
            rightGrid = GameObject.Find(("J") + (i + 1));
            lowerGrid = GameObject.Find(("K") + i);
            lowerLeftGrid = GameObject.Find(("K") + (i - 1));
            lowerRightGrid = GameObject.Find("K" + (i + 1));
        }
    }

    public void CheckWhereCanMove()
    {
        if (facingUp)
        {
            if (upperGrid != null && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall" && upperGrid != enemyGrid)
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || upperGrid.tag == "Hole" ||
                     upperGrid.tag == "Wall" || upperGrid == enemyGrid)
            {
                canMoveUp = false;
            }

            canMoveDown = false;

            if (leftGrid != null && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall" && leftGrid != enemyGrid)
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || leftGrid.tag == "Hole" ||
                     leftGrid.tag == "Wall" || leftGrid == enemyGrid)
            {
                canMoveLeft = false;
            }

            if (rightGrid != null && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall" && rightGrid != enemyGrid)
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || rightGrid.tag == "Hole" ||
                     rightGrid.tag == "Wall" || rightGrid == enemyGrid)
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveDown = true;
            }

            UpdateCurrentFaceUiServerRpc(true, false, false, false);
        }
        else if (facingDown)
        {
            canMoveUp = false;

            if (lowerGrid != null && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall" && lowerGrid != enemyGrid)
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag == "Wall" || lowerGrid == enemyGrid)
            {
                canMoveDown = false;
            }

            if (leftGrid != null && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall" && leftGrid != enemyGrid)
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || leftGrid.tag == "Hole" ||
                     leftGrid.tag == "Wall" || leftGrid == enemyGrid)
            {
                canMoveLeft = false;
            }

            if (rightGrid != null && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall" && rightGrid != enemyGrid)
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || rightGrid.tag == "Hole" ||
                     rightGrid.tag == "Wall" || rightGrid == enemyGrid)
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveUp = true;
            }

            UpdateCurrentFaceUiServerRpc(false, true, false, false);
        }
        else if (facingLeft)
        {
            if (upperGrid != null && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall" && upperGrid != enemyGrid)
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || upperGrid.tag == "Hole" ||
                     upperGrid.tag == "Wall" || upperGrid == enemyGrid)
            {
                canMoveUp = false;
            }

            if (lowerGrid != null && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall" && lowerGrid != enemyGrid)
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag == "Wall" || lowerGrid == enemyGrid)
            {
                canMoveDown = false;
            }

            if (leftGrid != null && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall" && leftGrid != enemyGrid)
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || leftGrid.tag == "Hole" ||
                     leftGrid.tag == "Wall" || leftGrid == enemyGrid)
            {
                canMoveLeft = false;
            }

            canMoveRight = false;

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveRight = true;
            }

            UpdateCurrentFaceUiServerRpc(false, false, true, false);
        }
        else if (facingRight)
        {
            if (upperGrid != null && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall" && upperGrid != enemyGrid)
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || upperGrid.tag == "Hole" ||
                     upperGrid.tag == "Wall" || upperGrid == enemyGrid)
            {
                canMoveUp = false;
            }

            if (lowerGrid != null && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall" && lowerGrid != enemyGrid)
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag == "Wall" || lowerGrid == enemyGrid)
            {
                canMoveDown = false;
            }

            canMoveLeft = false;

            if (rightGrid != null && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall" && rightGrid != enemyGrid)
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || rightGrid.tag == "Hole" ||
                     rightGrid.tag == "Wall" || rightGrid == enemyGrid)
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveLeft = true;
            }

            UpdateCurrentFaceUiServerRpc(false, false, false, true);
        }

    }

    // Sync the hit effect to all clients


    [ServerRpc(RequireOwnership = false)]
    public void HitServerRpc(int damage)
    {
        HitClientRpc(damage);
    }
    
    [ClientRpc]
    public void HitClientRpc(int damage)
    {
        if (!IsServer || m_isPlayerDefeated)
        {
            return;
        }

        // Update health var
        health.Value -= damage;
        HitSoundServerRpc();
        // Update UI
        playerUI.UpdateHealth(health.Value);

        // Sync on client

        if (health.Value > 0)
        {

        }
        else
        {
            m_isPlayerDefeated = true;

            // Tell the Gameplay manager that I've been defeated
            gameplayManager.PlayerDeath(m_characterData.clientId);

            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }
    }

    //---------------------------------------------------------------------------------------------------- Animator ----------------------------------------------------------------------------------------------------
    public void ChangeAnim(string animName)
    {
        if (animName == "Up_Idle")
        {
            m_animator.CrossFade("Up_Idle", 0, 0);
        }
        else if (animName == "Down_Idle")
        {
            m_animator.CrossFade("Down_Idle", 0, 0);
        }
        else if (animName == "Left_Idle")
        {
            m_animator.CrossFade("Left_Idle", 0, 0);
        }
        else if (animName == "Right_Idle")
        {
            m_animator.CrossFade("Right_Idle", 0, 0);
        }
        else if (animName == "Up_Walk")
        {
            m_animator.CrossFade("Up_Walk", 0, 0);
        }
        else if (animName == "Down_Walk")
        {
            m_animator.CrossFade("Down_Walk", 0, 0);
        }
        else if (animName == "Left_Walk")
        {
            m_animator.CrossFade("Left_Walk", 0, 0);
        }
        else if (animName == "Right_Walk")
        {
            m_animator.CrossFade("Right_Walk", 0, 0);
        }
    }

    //---------------------------------------------------------------------------------------------------- CheckFace ----------------------------------------------------------------------------------------------------

    public void CheckFacing()
    {
        if (facingUp)
        {
            ChangeAnim("Up_Idle");
        }
        else if (facingDown)
        {
            ChangeAnim("Down_Idle");
        }
        else if (facingLeft)
        {
            ChangeAnim("Left_Idle");
        }
        else if (facingRight)
        {
            ChangeAnim("Right_Idle");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateCurrentFaceUiServerRpc(bool up, bool down, bool left, bool right)
    {
        UpdateCurrentFaceUiClientRpc(up, down, left, right);
    }

    [ClientRpc]
    public void UpdateCurrentFaceUiClientRpc(bool up, bool down, bool left, bool right)
    {
        _indiUp.SetActive(up);
        _indiDown.SetActive(down);
        _indiLeft.SetActive(left);
        _indiRight.SetActive(right);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateFaceingServerRpc(bool up, bool down, bool left, bool right)
    {
        UpdateFaceingClientRpc(up, down, left, right);
    }

    [ClientRpc]
    public void UpdateFaceingClientRpc(bool up, bool down, bool left, bool right)
    {
        facingUp = up;
        facingDown = down;
        facingLeft = left;
        facingRight = right;
    }

    //---------------------------------------------------------------------------------------------------- CheckFace ----------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------- Original DicePoint ----------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------- Original DicePoint ----------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------- Test DicePoint ----------------------------------------------------------------------------------------------------
    /*[ServerRpc(RequireOwnership = false)]
    void DicePointChangeServerRpc(int value)
    {
        //GameplayManager.Instance.DicePointChange(value);
        DicePointChangeClientRpc(value);
    }

    [ClientRpc]
    void DicePointChangeClientRpc(int currentPoints)
    {
        currentPointToMove.Value += currentPoints;
    }*/
    //---------------------------------------------------------------------------------------------------- Test DicePoint ----------------------------------------------------------------------------------------------------

    [ServerRpc(RequireOwnership = false)]
    void NormalDiceRollServerRpc()
    {
        GameplayManager.Instance.NormalDiceRoll();
    }

    [ServerRpc(RequireOwnership = false)]
    void EndTurnServerRpc()
    {
        GameplayManager.Instance.EndTurn();
        Debug.Log("Player " + OwnerClientId + 1 + " End Turn");

    }

    [ServerRpc(RequireOwnership = false)]
    void DicePointChangeServerRpc(int value)
    {
        GameplayManager.Instance.DicePointChange(value);
    }



    [ServerRpc(RequireOwnership = false)]
    void CheckPlayersGridServerRpc(int player)
    {
        GameplayManager.Instance.CheckPlayersGrid(player);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateRollPointServerRpc(int value)
    {
        GameplayManager.Instance.UpdateRollPointClientRpc(value);
    }



    #region FoodScore Sync

    [ServerRpc(RequireOwnership = false)]
    void GetFoodScoreServerRpc()
    {
        GetFoodScoreClientRpc();
    }

    [ClientRpc]
    void GetFoodScoreClientRpc()
    {
        m_characterData.foodScore++;
    }

    #endregion


    [ServerRpc(RequireOwnership = false)]
    void UpdateScoreServerRpc()
    {
        UpdateScoreClientRpc();
    }

    [ClientRpc]
    void UpdateScoreClientRpc()
    {
        playerUI.UpdateScore(m_characterData.foodScore);
    }

    #region Sound Sync

    //---------------------------------------------------------------------------------------------------- Hit Sound ----------------------------------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    void HitSoundServerRpc()
    {
        HitSoundClientRpc();
    }

    [ClientRpc]
    void HitSoundClientRpc()
    {
        AudioManager.Instance?.PlaySoundEffect(hitSound);
    }
    //---------------------------------------------------------------------------------------------------- Hit Sound ----------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------- Walk Sound ----------------------------------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    void WalkSoundServerRpc()
    {
        WalkSoundClientRpc();
    }

    [ClientRpc]
    void WalkSoundClientRpc()
    {
        AudioManager.Instance?.PlaySoundEffect(walkSound);
    }

    [ServerRpc(RequireOwnership = false)]
    void StopWalkSoundServerRpc()
    {
        StopWalkSoundClientRpc();
    }

    [ClientRpc]
    void StopWalkSoundClientRpc()
    {
        AudioManager.Instance?.StopSoundEffect(walkSound);
    }
    //---------------------------------------------------------------------------------------------------- Walk Sound ----------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------- Get Food Sound ----------------------------------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    void GetFoodSoundServerRpc()
    {
        GetFoodSoundClientRpc();
    }

    [ClientRpc]
    void GetFoodSoundClientRpc()
    {
        AudioManager.Instance?.PlaySoundEffect(getFoodSound);
    }
    //---------------------------------------------------------------------------------------------------- Get Food Sound ----------------------------------------------------------------------------------------------------

    #endregion
}