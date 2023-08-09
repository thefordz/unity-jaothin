using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [Header("Other Scripts")]
    //Scripts
    public PlayerMove player1PlayerMove;
    public PlayerMove player2PlayerMove;
    private CameraShaking _camShaking;

    [Header("Corner Grids")]
    //Corner Grids
    public GameObject mapTopLeftGrid;
    public GameObject mapTopRightGrid;
    public GameObject mapBottomLeftGrid;
    public GameObject mapBottomRightGrid;

    
    [Header("Player GameObjects")]
    //Define the players
    public GameObject player1;
    public GameObject player2;

    [Header("Current player variables")]
    public GameObject currentPlayer;
    public GameObject player1CurrentGrid;
    public GameObject player2CurrentGrid;

    [Header("players statuses")]
    //Players Health and character status
    public int player1MaxHealth = 5;
    public int player1CurrentHealth;

    public int player2MaxHealth = 5;
    public int player2CurrentHealth;

    public bool player1Alive;
    public bool player2Alive;

    [Header("UIs")]
    //UI
    [SerializeField] private TextMeshProUGUI _player1HP_TMP;
    [SerializeField] private TextMeshProUGUI _player2HP_TMP;
    [SerializeField] private TextMeshProUGUI _popupMessage_TMP;

    void Awake()
    {
        player1PlayerMove = player1.GetComponent<PlayerMove>();
        player2PlayerMove = player2.GetComponent<PlayerMove>();

        _camShaking = GameObject.Find("Main Camera").GetComponent<CameraShaking>();
        player1CurrentHealth = player1MaxHealth;
        player2CurrentHealth = player2MaxHealth;
        player1Alive = player2Alive = true;
        UpdatePlayersStatus();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = player2;
        SwitchPlayer();
        _DicePointTMP = GameObject.Find("Dice Point").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void SwitchPlayer()
    {
        if (currentPlayer == player1)
        {
            currentPlayer = player2;
            player2PlayerMove.diceRollPhase = true;
            player2PlayerMove.movePhase = false;
            Debug.Log("Switched to player 2!");
        }
        else if (currentPlayer == player2)
        {
            currentPlayer = player1;
            player1PlayerMove.diceRollPhase = true;
            player1PlayerMove.movePhase = false;
            Debug.Log("Switched to player 1!");
        }
    }

    public void ChangePlayerPhase()
    {
        if (player1PlayerMove.diceRollPhase == true)
        {
            player1PlayerMove.diceRollPhase = false;
            player1PlayerMove.movePhase = true;
        }
        else if (player2PlayerMove.diceRollPhase == true)
        {
            player2PlayerMove.diceRollPhase = false;
            player2PlayerMove.movePhase = true;
        }
    }

    public void AbnormalSwitchPlayer(int playerNumber)
    {
        currentPlayer = GameObject.Find("player" + playerNumber);
    }

    public void CheckPlayersGrid()
    {
        RaycastHit2D player1Ray =
            Physics2D.Raycast(player1.transform.position, player1.transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (player1Ray)
        {
            Debug.DrawRay(player1.transform.position, player1.transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Player1 current position : " + player1Ray.collider.name);
            player1CurrentGrid = (GameObject.Find(player1Ray.collider.name));
        }
        else
        {
            Debug.DrawRay(player1.transform.position, player1.transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player1 current position.");
        }

        RaycastHit2D player2Ray =
            Physics2D.Raycast(player2.transform.position, player2.transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (player2Ray)
        {
            Debug.DrawRay(player2.transform.position, player2.transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Player2 current position : " + player2Ray.collider.name);
            player2CurrentGrid = (GameObject.Find(player2Ray.collider.name));
        }
        else
        {
            Debug.DrawRay(player2.transform.position, player2.transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player2 current position.");
        }

        player1.GetComponent<PlayerMove>().CheckWhereCanMove();
        player2.GetComponent<PlayerMove>().CheckWhereCanMove();
    }

    public void PlayerHealthChange(int playerNum, int healthChange)
    {
        if (playerNum == 1)
        {
            player1CurrentHealth += healthChange;
        }
        else if (playerNum == 2)
        {
            player2CurrentHealth += healthChange;
        }

        if (player1CurrentHealth <= 0)
        {
            PlayerDefeated(1, "No Health");
        }
        else if (player2CurrentHealth <= 0)
        {
            PlayerDefeated(2, "No Health");
        }
        UpdatePlayersStatus();
    }

    public void CheckPlayersStatus()
    {
        if (player1Alive && player1CurrentHealth <= 0)
        {
            player1Alive = false;
        }

        if (player2Alive && player2CurrentHealth <= 0)
        {
            player2Alive = false;
        }
    }

    public void PlayerDefeated(int playerNum, string reason)
    {
        if (playerNum == 1)
        {
            player1Alive = false;
            player1PlayerMove.ableToPressMove = false;
            player1PlayerMove.diceRollPhase = false;
            player1PlayerMove.movePhase = false;

            _camShaking.ShakeTheScreen(0.5f, 0.7f);
            _popupMessage_TMP.color = Color.green;

            switch (reason)
            {
                case "Hit Hole":
                    _popupMessage_TMP.text = "Player1 Defeated!\nFell into a hole";
                    break;
                case "No Health":
                    _popupMessage_TMP.text = "Player1 Defeated!\n0 health";
                    break;
            }
            
        }
        else if (playerNum == 2)
        {
            player2Alive = false;
            player2PlayerMove.ableToPressMove = false;
            player2PlayerMove.diceRollPhase = false;
            player2PlayerMove.movePhase = false;
            _camShaking.ShakeTheScreen(0.5f, 0.7f);
            _popupMessage_TMP.color = Color.green;

            switch (reason)
            {
                case "Hit Hole":
                    _popupMessage_TMP.text = "Player2 Defeated!\nFell into a hole";
                    break;
                case "No Health":
                    _popupMessage_TMP.text = "Player2 Defeated!\n0 health";
                    break;
            }
        }
    }

    public void UpdatePlayersStatus()
    {
        _player1HP_TMP.text = $"{player1CurrentHealth}/{player1MaxHealth}";
        _player2HP_TMP.text = $"{player2CurrentHealth}/{player2MaxHealth}";
    }

    //================================//
    //================================//
    //================================//
    // Dice and Random number section //
    //================================//
    //================================//
    //================================//

    private TextMeshProUGUI _DicePointTMP;
    public int currentDicePoint;

    public void NormalDiceRoll()
    {
        if (player1PlayerMove.diceRollPhase ||
            player2PlayerMove.diceRollPhase)
        {
            ChangePlayerPhase();
            StartCoroutine(DiceRolling(1, 6));
            player1PlayerMove.diceRollPhase = false;
            player2PlayerMove.diceRollPhase = false;
        }
    }

    public void DicePointChange(int changePoint)
    {
        currentDicePoint += changePoint;
        _DicePointTMP.text = currentDicePoint.ToString();
    }

    private IEnumerator DiceRolling(int minDicePoint, int maxDicePoint)
    {
        float timeSec = 0;
        while (timeSec <= 2)
        {
            currentDicePoint = Random.Range(minDicePoint, maxDicePoint + 1);
            yield return new WaitForSeconds(0.1f);
            timeSec += 0.1f;
            _DicePointTMP.text = currentDicePoint.ToString();
        }
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"Dice number : {currentDicePoint}, now {currentPlayer.name} can move");
        if (currentPlayer == player1)
        {
            player1PlayerMove.movePhase = true;
            player1PlayerMove.ableToPressMove = true;
        }
        else if (currentPlayer == player2)
        {
            player2PlayerMove.movePhase = true;
            player2PlayerMove.ableToPressMove = true;
        }
    }
}