/*
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GameplayManager : SingletonNetwork<GameplayManager>
{
    [Header("Player Data")]
    [SerializeField] private CharacterDataSO[] m_charactersData;
    [SerializeField] private PlayerUI[] m_playersUI;
    [SerializeField] private Transform[] m_playerStartingPositions;

    [Header("Player Connect Management")]
    private int m_numberOfPlayerConnected;
    private List<ulong> m_connectedClients = new List<ulong>();
    public List<PlayerTurnOnline> m_players = new List<PlayerTurnOnline>();
    public int m_currentPlayerIndex = 0;

    [Header("Ui")]
    public TMP_Text uiTurn;
    public TextMeshProUGUI _DicePointTMP;
    public Button uiRollDiceButton;
    //public NetworkVariable<int> currentPoints = new NetworkVariable<int>();

    [Header("Corner Grids")]
    //Corner Grids
    public GameObject mapTopLeftGrid;
    public GameObject mapTopRightGrid;
    public GameObject mapBottomLeftGrid;
    public GameObject mapBottomRightGrid;

    [Header("Current Position")] 
    public GameObject playerCurrentGrid;
    
    [Header("Dice Management")]
    public int currentDicePoint;
    //public NetworkVariable<int> currentDicePoint = new NetworkVariable<int>();


    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

    }


    private void OnEnable()
    {
        if (!IsServer)
            return;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }


    // Start is called before the first frame update

    // Update is called once per frame

    private void OnClientDisconnect(ulong clientId)
    {
        foreach (var player in m_players)
        {
            if (player != null)
            {

            }
        }
    }

    [ClientRpc]
    private void SetPlayerUIClientRpc(int charIndex, string playerName)
    {
        m_playersUI[m_charactersData[charIndex].playerId].SetUI(
            m_charactersData[charIndex].playerId, playerName,
            m_charactersData[charIndex].characterColor);
    }

    [ClientRpc]
    private void SetPlayerRollButtonClientRpc(int charIndex, bool input)
    {
        m_playersUI[m_charactersData[charIndex].playerId].SetRollButtonUI(input);
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

    [ClientRpc]
    private void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
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


    public void ServerSceneInit(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }
        
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

                    PlayerTurnOnline playerController =
                        player.GetComponent<PlayerTurnOnline>();


                    m_players.Add(playerController);
                    SetPlayerUIClientRpc(index, data.characterName);


                    m_numberOfPlayerConnected++;

                }

                index++;
            }
        }

        int randomFirstPlayer = 0;//Random.Range(0, 2);
        SwitchTurn(m_players[randomFirstPlayer]);
        Debug.Log("Player " + m_players[randomFirstPlayer].OwnerClientId+1);
        Debug.Log(m_players[m_currentPlayerIndex]);
    }


    
    public void SwitchTurn(PlayerTurnOnline player)
    {
        player.isMyTurn.Value = true;
        player.isDiceRollPhase.Value = true;
        player.isMovePhase.Value = false;
        SetPlayerRollButtonClientRpc(m_currentPlayerIndex, true);
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
                SetPlayerRollButtonClientRpc(m_currentPlayerIndex, false);
            }
        }
        UpdateUiClientRpc(player.OwnerClientId);
    }

    [ClientRpc]
    private void UpdateUiClientRpc(ulong index)
    {
        uiTurn.text = string.Format("Player {0}'s Turn", index+1);
    }
    
    [ClientRpc]
    public void UpdateRollPointClientRpc()
    {
        _DicePointTMP.text = currentDicePoint.ToString();
        // _DicePointTMP.text = PlayerTurnOnline.currentPointToMove.Value.ToString();
       //_DicePointTMP.text = m_players[0].currentPointToMove.Value.ToString();
    }
    public void ChangePlayerPhase(PlayerTurnOnline player)
    {
        if (player.isDiceRollPhase.Value == true)
        {
            player.isDiceRollPhase.Value = false;
            player.isMovePhase.Value = true;
        }
    }
    
    public void CheckPlayersGrid()
    {
        RaycastHit2D playerHitGrid =
            Physics2D.Raycast(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (playerHitGrid)
        {
            Debug.DrawRay(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward),
                Color.green);
            Debug.Log("Player " + m_players[m_currentPlayerIndex].OwnerClientId+1 +" current position : " + playerHitGrid.collider.name);
            playerCurrentGrid = (GameObject.Find(playerHitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(m_players[m_currentPlayerIndex].transform.position, m_players[m_currentPlayerIndex].transform.TransformDirection(Vector3.forward), Color.red);
            Debug.LogError("Cannot Find Player "+ m_players[m_currentPlayerIndex].OwnerClientId+1 +" current position.");
        }


        m_players[m_currentPlayerIndex].GetComponent<PlayerTurnOnline>().CheckWhereCanMove();
    }
    
    public void NormalDiceRoll()
    {
        if (m_players[m_currentPlayerIndex].isMyTurn.Value  && m_players[m_currentPlayerIndex].isDiceRollPhase.Value)
            
        {
            ChangePlayerPhase(m_players[m_currentPlayerIndex]);
            StartCoroutine(DiceRolling(1, 6));
            m_players[m_currentPlayerIndex].isDiceRollPhase.Value = false;
        }
    }
    
    
    public void DicePointChange(int currentPoints)
    {
        m_players[m_currentPlayerIndex].currentPointToMove.Value += currentPoints;
        //_DicePointTMP.text = currentDicePoint.ToString();
        UpdateRollPointClientRpc();
        Debug.Log("Current Dice Point : " + m_players[m_currentPlayerIndex].currentPointToMove.Value);
    }
    
    private IEnumerator DiceRolling(int minDicePoint, int maxDicePoint)
    {
        float timeSec = 0;
        while (timeSec <= 2)
        {
            uiRollDiceButton.interactable = false;
            m_players[m_currentPlayerIndex].currentPointToMove.Value = Random.Range(minDicePoint, maxDicePoint + 1);
            yield return new WaitForSeconds(0.1f);
            timeSec += 0.1f;
            Debug.Log("Current Dice Point : " + m_players[m_currentPlayerIndex].currentPointToMove.Value);
            UpdateRollPointClientRpc();
            //m_players[m_currentPlayerIndex].UpdateRollPointServerRpc();
        }
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Dice number : {m_players[m_currentPlayerIndex].currentPointToMove.Value}, now {m_players[m_currentPlayerIndex]} can move");
        if (m_players[m_currentPlayerIndex].isMyTurn.Value)
        {
            m_players[m_currentPlayerIndex].isMovePhase.Value = true;
            m_players[m_currentPlayerIndex].isAbleToPressMove.Value = true;
        }
    }
    
    public void EndTurn()
    {
        uiRollDiceButton.interactable = true;
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
    

    

}
*/

