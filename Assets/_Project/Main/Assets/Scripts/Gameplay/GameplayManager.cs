using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public class GameplayManager : SingletonNetwork<GameplayManager>
{
    public static Action<ulong> OnPlayerDefeated;
    
    [Header("Player Data")]
    [SerializeField] private CharacterDataSO[] m_charactersData;
    [SerializeField] private PlayerUI[] m_playersUI;
    [SerializeField] private Transform[] m_playerStartingPositions;
    [SerializeField] private GameObject m_deathUI;
    [SerializeField] private GameObject m_winnerUi;

    [Header("Player Connect Management")]
    private int m_numberOfPlayerConnected;
    private List<ulong> m_connectedClients = new List<ulong>();
    public List<PlayerTurnOnline> m_players = new List<PlayerTurnOnline>();
    public int m_currentPlayerIndex = 0;

    [Header("Ui")]
    public TMP_Text uiTurn;
    public TextMeshProUGUI _DicePointTMP;
    public Button PanelRoll;

    [Header("TurnManage")] 
    public int currentTurn = 0;
    public int maxTurn;
    public TMP_Text UiTurnCount;

    [Header("Corner Grids")]
    public GameObject mapTopLeftGrid;
    public GameObject mapTopRightGrid;
    public GameObject mapBottomLeftGrid;
    public GameObject mapBottomRightGrid;

    [Header("Current Position")]
    public GameObject playerGrid;
    public GameObject enemyGrid;
    

    [Header("Dice Management")]
    public int currentDicePoint;
    public GameObject uiRoll;

    private void Start()
    {

    }

    private void OnEnable()
    {
        if (!IsServer)
            return;

        OnPlayerDefeated += PlayerDeath;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDisable()
    {
        if (!IsServer)
            return;

        OnPlayerDefeated -= PlayerDeath;

        // Since the NetworkManager could potentially be destroyed before this component, only
        // remove the subscriptions if that singleton still exists.
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    public void PlayerDeath(ulong clientId)
    {
        /*m_numberOfPlayerConnected--;

        if (m_numberOfPlayerConnected <= 0)
        {
            LoadClientRpc();
            LoadingSceneManager.Instance.LoadScene(SceneName.Defeat);
        }
        else
        {
            // Send a client rpc to check which client was defeated, and activate their death UI
            ActivateDeathUIClientRpc(clientId);
        }*/
        
        //Load Victory Scene
        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Victory);
        
        
    }


    private void OnClientDisconnect(ulong clientId)
    {
        foreach (var player in m_players)
        {
            if (player != null)
            {
                if (player.characterData.clientId == clientId)
                {
                    player.HitServerRpc(999); // Do critical damage
                }
            }
        }
    }
    
    [ClientRpc]
    private void ActivateDeathUIClientRpc(ulong clientId)
    {
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Winner();
        }
        
    }
    
    
    [ClientRpc]
    private void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
    }

    [ClientRpc]
    private void SetPlayerUIClientRpc(int charIndex, string playerName)
    {
        GameObject player = GameObject.Find(playerName);

        PlayerTurnOnline playerTurnOnline = player.GetComponent<PlayerTurnOnline>();
        
        m_playersUI[m_charactersData[charIndex].playerId].SetUI(
            m_charactersData[charIndex].playerId,
            m_charactersData[charIndex].characterName,
            m_charactersData[charIndex].iconSprite,
            m_charactersData[charIndex].iconDeathSprite,
            playerTurnOnline.health.Value,
            m_charactersData[charIndex].darkColor,
            m_charactersData[charIndex].foodScore);
        
        playerTurnOnline.playerUI = m_playersUI[m_charactersData[charIndex].playerId];
    }
    

    private IEnumerator HostShutdown()
    {

        // Tell the clients to shutdown
        ShutdownClientRpc();

        // Wait some time for the message to get to clients
        yield return new WaitForSeconds(0.5f);

        // Shutdown server/host
        Shutdown();
    }

    

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuV2, false);
    }

    [ClientRpc]
    private void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }

    public void ExitToMenu()
    {
        if (IsServer)
        {
            StartCoroutine(HostShutdown());
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuV2, false);
        }
    }

    #region Spawner Player In to m_Player

    

    
    //=============================================================================================== Spawner Player In to m_Player =================================
    public void ServerSceneInit(ulong clientId)
    {
        // Save the clients 
        m_connectedClients.Add(clientId);

        // Check if is the last client
        if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        // For each client spawn and set UI
        foreach (var client in m_connectedClients)
        {
            int index = 0;

            foreach (CharacterDataSO data in m_charactersData)
            {
                if (data.isSelected && data.clientId == client)
                {
                    GameObject player =
                        NetworkObjectSpawner.SpawnNewPlayerObjectToClient(
                            data.characterPrefab,
                            m_playerStartingPositions[m_numberOfPlayerConnected].position,
                            data.clientId,
                            true);

                    PlayerTurnOnline playerTurnOnline =
                        player.GetComponent<PlayerTurnOnline>();
                    playerTurnOnline.characterData = data;
                    playerTurnOnline.gameplayManager = this;

                    m_players.Add(playerTurnOnline);
                    SetPlayerUIClientRpc(index, playerTurnOnline.name);


                    m_numberOfPlayerConnected++;
                }
                index++;
            }
        }
        
        int randomFirstPlayer = 0;//Random.Range(0, 2);
        SelectFirstPlayer(m_players[randomFirstPlayer]);
        m_currentPlayerIndex = randomFirstPlayer;
    }
    //=============================================================================================== Spawner Player In to m_Player =================================
    
    #endregion
    
    
    public void SelectFirstPlayer(PlayerTurnOnline player)
    {
        player.isMyTurn.Value = true;
        player.isDiceRollPhase.Value = true;
        player.isMovePhase.Value = false;
        player.isCheckUiRoll.Value = true;

        
        if (m_currentPlayerIndex == 0)
        {
            CheckEnemyGridServerRpc(1);
        }
        else if (m_currentPlayerIndex ==1)
        {
            CheckEnemyGridServerRpc(0);
        }
        UpdateRollPointStartClientRpc();
        // Set the current player's turn to true
        Debug.Log("Switch to player " + player.OwnerClientId+1 + "!");
        Debug.Log("Switch to player ");
        
        foreach (var p in m_players)
        {
            if (p != player)
            {
                Debug.Log("Other Player Can't Move");
                p.isMyTurn.Value = false;
                p.isDiceRollPhase.Value = false;
                p.isMovePhase.Value = false;
                p.isCheckUiRoll.Value = false;
            }
        }
        
        UpdateUiClientRpc(player.OwnerClientId);
        UpdateTurnCountClientRpc();
    }
    
    public void SwitchTurn(PlayerTurnOnline player)
    {
        
        player.isMyTurn.Value = true;
        player.isDiceRollPhase.Value = true;
        player.isMovePhase.Value = false;
        player.isCheckUiRoll.Value = true;
        Debug.Log("========================" + player.name);
        
        UpdateRollPointStartClientRpc();
        // Set the current player's turn to true
        Debug.Log("Switch to player " + player.OwnerClientId+1 + "!");
        Debug.Log("Switch to player ");
        
        foreach (var p in m_players)
        {
            if (p != player)
            {
                Debug.Log("Other Player Can't Move");
                p.isMyTurn.Value = false;
                p.isDiceRollPhase.Value = false;
                p.isMovePhase.Value = false;
                p.isCheckUiRoll.Value = false;
                Debug.Log("========================" + p.name);
            }
        }
        UpdateUiClientRpc(player.OwnerClientId);
        UpdateTurnCountClientRpc();
        CheckTurnServerRpc();
    }



    //---------------------------------------------------------------------------------------------------- Turn UI ----------------------------------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    void updateRollUIServerRpc(bool input)
    {
        updateRollUIClientRpc(input);
    }
    
    [ClientRpc]
    void updateRollUIClientRpc(bool input)
    {
        uiRoll.gameObject.SetActive(input);
    }
    
    [ClientRpc]
    private void UpdateUiClientRpc(ulong index)
    {
        uiTurn.text = string.Format("Player {0}'s Turn", index+1);
    }

    [ClientRpc]
    private void UpdateTurnCountClientRpc()
    {
        UiTurnCount.text = string.Format("Turn : " + currentTurn + " / " + maxTurn);
    }

    [ServerRpc]
    private void CheckTurnServerRpc()
    {
        Debug.Log("Update Turn " +currentTurn +" / "+maxTurn);
        CheckTurnClientRpc();
    }

    [ClientRpc]
    public void UpdateRollButtonClientRpc(bool input)
    {
        PanelRoll.gameObject.SetActive(input);
    }

    [ClientRpc]
    private void CheckTurnClientRpc()
    {
        currentTurn++;
        UpdateTurnCountClientRpc();
        if (currentTurn >= maxTurn)
        {
            StartCoroutine(WaitToEndGameScene());
        }
    }

    IEnumerator WaitToEndGameScene()
    {
        Debug.Log("Wait For End Scene");
        yield return new WaitForSeconds(3);
        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Victory);
        
    }
    
    //---------------------------------------------------------------------------------------------------- Original ----------------------------------------------------------------------------------------------------
    /*public void UpdateRollPoint()
    {
        _DicePointTMP.text = string.Format("You can move " + m_players[m_currentPlayerIndex].currentPointToMove.Value);
    }
    
    public void UpdateRollPointStart()
    {
        currentDicePoint = m_players[m_currentPlayerIndex].currentPointToMove.Value;
        _DicePointTMP.text = "Roll";
    }*/
    //---------------------------------------------------------------------------------------------------- Original ----------------------------------------------------------------------------------------------------
    
    
    
    //---------------------------------------------------------------------------------------------------- Test Ui Client ----------------------------------------------------------------------------------------------------
    
  
    
    [ClientRpc]
    public void UpdateRollPointClientRpc(int currentDicePoint)
    {
        _DicePointTMP.text = string.Format("Move points " + currentDicePoint);
        if (currentDicePoint == 1)
        {
            _DicePointTMP.text = string.Format("Move point " + currentDicePoint);
        }
    }
    
    [ClientRpc]
    public void UpdateRollPointStartClientRpc()
    {
        currentDicePoint = 0;
        _DicePointTMP.text = "Roll";
    }
    //---------------------------------------------------------------------------------------------------- Original ----------------------------------------------------------------------------------------------------
    
    
    public void ChangePlayerPhase(PlayerTurnOnline player)
    {
        if (player.isDiceRollPhase.Value == true)
        {
            player.isDiceRollPhase.Value = false;
            player.isMovePhase.Value = true;
        }
    }
    
    /*public void CheckPlayersGrid()
    {
        RaycastHit2D playerHitGrid =
            Physics2D.Raycast(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (playerHitGrid)
        {
            Debug.DrawRay(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Player " + m_players[m_currentPlayerIndex].OwnerClientId+1 +" current position : " + playerHitGrid.collider.name);
            playerGrid = (GameObject.Find(playerHitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player "+ m_players[m_currentPlayerIndex].OwnerClientId+1 +" current position.");
        }


        m_players[m_currentPlayerIndex].GetComponent<PlayerTurnOnline>().CheckWhereCanMove();
    }*/
    
    public void CheckPlayersGrid(int player)
    {
        Debug.Log("Check Player " + player);
        RaycastHit2D playerHitGrid =
            Physics2D.Raycast(m_players[player].transform.position, m_players[player].transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (playerHitGrid)
        {
            Debug.DrawRay(m_players[player].transform.position, m_players[player].transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Player " + m_players[player].OwnerClientId+1 +" current position : " + playerHitGrid.collider.name);
            playerGrid = (GameObject.Find(playerHitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(m_players[player].transform.position, m_players[player].transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player "+ m_players[player].OwnerClientId+1 +" current position.");
        }
        m_players[player].GetComponent<PlayerTurnOnline>().CheckWhereCanMove();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CheckEnemyGridServerRpc(int enemy)
    {
        Debug.Log("Check Enemy " + enemy);
        RaycastHit2D enemyHitGrid =
            Physics2D.Raycast(m_players[enemy].transform.position, m_players[enemy].transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (enemyHitGrid)
        {
            Debug.DrawRay(m_players[enemy].transform.position, m_players[enemy].transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Enemy grid " + m_players[enemy].OwnerClientId+1 +" current position : " + enemyHitGrid.collider.name);
            enemyGrid = (GameObject.Find(enemyHitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(m_players[enemy].transform.position, m_players[enemy].transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player "+ m_players[enemy].OwnerClientId+1 +" current position.");
        }
    }

    [ClientRpc]
    public void CheckEnemyGridClientRpc(int enemy)
    {
        /*Debug.Log("Check Enemy " + enemy);
        RaycastHit2D enemyHitGrid =
            Physics2D.Raycast(m_players[enemy].transform.position, m_players[enemy].transform.TransformDirection(Vector3.forward), 10f);


$
        else
        {
            Debug.DrawRay(m_players[enemy].transform.position, m_players[enemy].transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player "+ m_players[enemy].OwnerClientId+1 +" current position.");
        }*/
    }

    /*[ServerRpc(RequireOwnership = false)] 
    public void NormalDiceRollServerRpc()
    {
        NormalDiceRollClientRpc();
    }
    
    [ClientRpc]
    public void NormalDiceRollClientRpc()
    {
        if (m_players[m_currentPlayerIndex].isMyTurn.Value  && m_players[m_currentPlayerIndex].isDiceRollPhase.Value && currentTurn < maxTurn)
            
        {
            ChangePlayerPhase(m_players[m_currentPlayerIndex]);
            StartCoroutine(DiceRolling(1, 6));
            m_players[m_currentPlayerIndex].isDiceRollPhase.Value = false;
            uiRoll.gameObject.SetActive(false);
        }
    }*/
    

    

    public void NormalDiceRoll()
    {
        if (m_players[m_currentPlayerIndex].isMyTurn.Value  && m_players[m_currentPlayerIndex].isDiceRollPhase.Value && currentTurn < maxTurn)
            
        {
            ChangePlayerPhase(m_players[m_currentPlayerIndex]);
            StartCoroutine(DiceRolling(1, 6));
            m_players[m_currentPlayerIndex].isDiceRollPhase.Value = false;
            uiRoll.gameObject.SetActive(false);
        }
    }
    
    /*[ServerRpc(RequireOwnership = false)]
    public void DicePointChangeServerRpc(int value)
    {
        DicePointChangeClientRpc(value);
    }

    
    [ClientRpc]
    public void DicePointChangeClientRpc(int currentPoints)
    {
        m_players[m_currentPlayerIndex].currentPointToMove.Value += currentPoints;
        UpdateRollPointClientRpc(currentPoints);
        Debug.Log("Current Dice Point from int currentPoints" + currentPoints);
        Debug.Log("Current Dice Point from m_player : " + m_players[m_currentPlayerIndex].currentPointToMove.Value);
    }*/
    
    public void DicePointChange(int currentPoints)
    {
        m_players[m_currentPlayerIndex].currentPointToMove.Value += currentPoints;
        UpdateRollPointClientRpc(currentPoints);
        Debug.Log("Current Dice Point from int currentPoints" + currentPoints);
        Debug.Log("Current Dice Point from m_player : " + m_players[m_currentPlayerIndex].currentPointToMove.Value);
    }
    
    private IEnumerator DiceRolling(int minDicePoint, int maxDicePoint)
    {
        float timeSec = 0;
        while (timeSec <= 2)
        {
            int _DicePoint = Random.Range(minDicePoint, maxDicePoint + 1);
            m_players[m_currentPlayerIndex].currentPointToMove.Value = _DicePoint;
            yield return new WaitForSeconds(0.1f);
            timeSec += 0.1f;
            UpdateRollPointClientRpc(m_players[m_currentPlayerIndex].currentPointToMove.Value);
        }
        yield return new WaitForSeconds(0.5f);
        if (m_players[m_currentPlayerIndex].isMyTurn.Value)
        {
            m_players[m_currentPlayerIndex].isMovePhase.Value = true;
            m_players[m_currentPlayerIndex].isAbleToPressMove.Value = true;
        }
    }

    public void EndTurn()
    {
        if (m_currentPlayerIndex == 0)
        {
            
            m_currentPlayerIndex = 1;
        }
        else if(m_currentPlayerIndex == 1)
        {
            m_currentPlayerIndex = 0;
        }
        SwitchTurn(m_players[m_currentPlayerIndex]);
    }

    public void Winner()
    {
        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Victory);
    }

    public void Loser()
    {
        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Defeat);
    }
}

