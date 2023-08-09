using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
* Singleton to control the changes on the char displays and the flow of the scene
*/

// States the player can have on the game
public enum ConnectionState : byte
{
    connected,
    disconnected,
    ready
}

// Struct for better serialization on the player connection
[Serializable]
public struct PlayerConnectionState
{
    public ConnectionState playerState;             // State of the player
    public PlayerCharSelection playerObject;        // The NetworkObject of the client use for the disconnection of the client
    public string playerName;                       // The name of the player when spawn
    public ulong clientId;                          // Id of the client
}

// Struct for better serialization on the container of the character
[Serializable]
public struct CharacterContainer
{
    public Image imageContainer;                    // The image of the character container
    public TextMeshProUGUI nameContainer;           // Character name container
    public Image playerIcon;                        // The background icon of the player (p1, p2)
    public GameObject waitingText;                  // The waiting text on the container were no client connected
}

public class CharacterSelectionManager : SingletonNetwork<CharacterSelectionManager>
{
    public CharacterDataSO[] charactersData;

    [SerializeField]
    CharacterContainer[] m_charactersContainers;

    [SerializeField]
    GameObject m_readyButton;

    [SerializeField]
    GameObject m_cancelButton;

    [SerializeField]
    float m_timeToStartGame;

    [SerializeField]
    SceneName m_nextScene = SceneName.Gameplay;

    [SerializeField]
    Color m_clientColor;

    [SerializeField]
    Color m_playerColor;

    [SerializeField]
    PlayerConnectionState[] m_playerStates;

    [SerializeField]
    GameObject m_playerPrefab;

    bool m_isTimerOn;
    float m_timer;

    private readonly Color k_selectedColor = new Color32(74, 74, 74, 255);

    // Start is called before the first frame update
    void Start()
    {
        m_timer = m_timeToStartGame;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;

        if (m_isTimerOn)
        {
            m_timer -= Time.deltaTime;

            if (m_timer < 0f)
            {
                m_isTimerOn = false;
                StartGame();
            }
        }
    }
    void OnDisable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnects;
        }
    }

    void StartGame()
    {
        StartGameClientRpc();
        LoadingSceneManager.Instance.LoadScene(m_nextScene);
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        LoadingFadeEffect.Instance.FadeAll();
    }

    

    [ClientRpc]
    void UpdatePlayerStateClientRpc(ulong clientId, int stateIndex, ConnectionState state)
    {
        if (IsServer)
            return;

        m_playerStates[stateIndex].playerState = state;
        m_playerStates[stateIndex].clientId = clientId;
    }

    void RemoveSelectedStates()
    {
        for (int i = 0; i < charactersData.Length; i++)
        {
            charactersData[i].isSelected = false;
        }
    }
    void RemoveReadyStates(ulong clientId, bool disconected)
    {
        for (int i = 0; i < m_playerStates.Length; i++)
        {
            if (m_playerStates[i].playerState == ConnectionState.ready &&
                m_playerStates[i].clientId == clientId)
            {

                if (disconected)
                {
                    m_playerStates[i].playerState = ConnectionState.disconnected;
                    UpdatePlayerStateClientRpc(clientId, i, ConnectionState.disconnected);
                }
                else
                {
                    m_playerStates[i].playerState = ConnectionState.connected;
                    UpdatePlayerStateClientRpc(clientId, i, ConnectionState.connected);
                }
            }
        }
    }

    void StartGameTimer()
    {
        foreach (PlayerConnectionState state in m_playerStates)
        {
            // If a player is connected (not ready)
            if (state.playerState == ConnectionState.connected)
                return;
        }

        // If all players connected are ready
        m_timer = m_timeToStartGame;
        m_isTimerOn = true;
    }

    public bool IsReady(int playerId)
    {
        return charactersData[playerId].isSelected;
    }

    void SetNonPlayableChar(int playerId)
    {
        m_charactersContainers[playerId].imageContainer.sprite = null;
        m_charactersContainers[playerId].imageContainer.color = new Color(1f, 1f, 1f, 0f);
        m_charactersContainers[playerId].nameContainer.text = "";
        m_charactersContainers[playerId].playerIcon.gameObject.SetActive(false);
        m_charactersContainers[playerId].playerIcon.color = m_playerColor;
        m_charactersContainers[playerId].waitingText.SetActive(true);
    }

    public ConnectionState GetConnectionState(int playerId)
    {
        if (playerId != -1)
            return m_playerStates[playerId].playerState;

        return ConnectionState.disconnected;
    }

    public void SetCharacterColor(int playerId, int characterSelected)
    {
        if (charactersData[characterSelected].isSelected)
        {
            m_charactersContainers[playerId].imageContainer.color = k_selectedColor;
            m_charactersContainers[playerId].nameContainer.color = k_selectedColor;
        }
        else
        {
            m_charactersContainers[playerId].imageContainer.color = Color.white;
            m_charactersContainers[playerId].nameContainer.color = Color.white;
        }
    }

    public void SetCharacterUI(int playerId, int characterSelected)
    {
        m_charactersContainers[playerId].imageContainer.sprite =
            charactersData[characterSelected].characterSprite;

        m_charactersContainers[playerId].nameContainer.text =
            charactersData[characterSelected].characterName;

        SetCharacterColor(playerId, characterSelected);
    }

    public void SetPlayebleChar(int playerId, int characterSelected, bool isClientOwner)
    {
        SetCharacterUI(playerId, characterSelected);
        m_charactersContainers[playerId].playerIcon.gameObject.SetActive(true);
        if (isClientOwner)
        {
            m_charactersContainers[playerId].playerIcon.color = m_clientColor;
        }
        else
        {
            m_charactersContainers[playerId].playerIcon.color = m_playerColor;
        }
        
        m_charactersContainers[playerId].waitingText.SetActive(false);
    }

    [ClientRpc]
    void PlayerConnectsClientRpc(
        ulong clientId,
        int stateIndex,
        ConnectionState state,
        NetworkObjectReference player)
    {
        if (IsServer)
            return;

        if (state != ConnectionState.disconnected)
        {
            m_playerStates[stateIndex].playerState = state;
            m_playerStates[stateIndex].clientId = clientId;

            if (player.TryGet(out NetworkObject playerObject))
                m_playerStates[stateIndex].playerObject =
                    playerObject.GetComponent<PlayerCharSelection>();
        }
    }

    public void ServerSceneInit(ulong clientId)
    {
        GameObject go =
            NetworkObjectSpawner.SpawnNewNetworkObjectChangeOwnershipToClient(
                m_playerPrefab,
                transform.position,
                clientId,
                true);

        for (int i = 0; i < m_playerStates.Length; i++)
        {
            if (m_playerStates[i].playerState == ConnectionState.disconnected)
            {
                m_playerStates[i].playerState = ConnectionState.connected;
                m_playerStates[i].playerObject = go.GetComponent<PlayerCharSelection>();
                m_playerStates[i].playerName = go.name;
                m_playerStates[i].clientId = clientId;

                // Force the exit
                break;
            }
        }

        // Sync states to clients
        for (int i = 0; i < m_playerStates.Length; i++)
        {
            if (m_playerStates[i].playerObject != null)
                PlayerConnectsClientRpc(
                    m_playerStates[i].clientId,
                    i,
                    m_playerStates[i].playerState,
                    m_playerStates[i].playerObject.GetComponent<NetworkObject>());
        }

    }
    public int GetPlayerId(ulong clientId)
    {
        for (int i = 0; i < m_playerStates.Length; i++)
        {
            if (m_playerStates[i].clientId == clientId)
                return i;
        }

        //! This should never happen
        Debug.LogError("This should never happen");
        return -1;
    }

    [ClientRpc]
    void PlayerReadyClientRpc(ulong clientId, int playerId, int characterSelected)
    {
        charactersData[characterSelected].isSelected = true;
        charactersData[characterSelected].clientId = clientId;
        charactersData[characterSelected].playerId = playerId;
        m_playerStates[playerId].playerState = ConnectionState.ready;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
        }
        else
        {
        }

        for (int i = 0; i < m_playerStates.Length; i++)
        {
            // Only changes the ones on clients that are not selected            
            if (m_playerStates[i].playerState == ConnectionState.connected)
            {
                if (m_playerStates[i].playerObject.CharSelected == characterSelected)
                {
                    SetCharacterColor(i, characterSelected);
                }
            }
        }

       // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
    }

    [ClientRpc]
    void PlayerNotReadyClientRpc(ulong clientId, int playerId, int characterSelected)
    {
        charactersData[characterSelected].isSelected = false;
        charactersData[characterSelected].clientId = 0UL;
        charactersData[characterSelected].playerId = -1;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {

        }
        else
        {
        }

        
        for (int i = 0; i < m_playerStates.Length; i++)
        {
            // Only changes the ones on clients that are not selected
            if (m_playerStates[i].playerState == ConnectionState.connected)
            {
                if (m_playerStates[i].playerObject.CharSelected == characterSelected)
                {
                    SetCharacterColor(i, characterSelected);
                }
            }
        }
    }

    [ClientRpc]
    public void PlayerDisconnectedClientRpc(int playerId)
    {
        SetNonPlayableChar(playerId);

        // All character data unselected
        RemoveSelectedStates();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnects;
        }
    }

    // Set the player ready if the player is not selected and check if all player are ready to start the countdown
    public void PlayerReady(ulong clientId, int playerId, int characterSelected)
    {
        if (!charactersData[characterSelected].isSelected)
        {
            PlayerReadyClientRpc(clientId, playerId, characterSelected);

            StartGameTimer();
        }
    }
    // Check if the player has selected the character
    public bool IsSelectedByPlayer(int playerId, int characterSelected)
    {
        return charactersData[characterSelected].playerId == playerId ? true : false;
    }

    // Set the players UI button
    public void SetPlayerReadyUIButtons(bool isReady, int characterSelected)
    {
        if (isReady && !charactersData[characterSelected].isSelected)
        {
            m_readyButton.SetActive(false);
            m_cancelButton.SetActive(true);
        }
        else if (!isReady && charactersData[characterSelected].isSelected)
        {
            m_readyButton.SetActive(true);
            m_cancelButton.SetActive(false);
        }
    }

    public void PlayerDisconnects(ulong clientId)
    {
        if (!ClientConnection.Instance.IsExtraClient(clientId))
            return;

        PlayerNotReady(clientId, isDisconected: true);

        m_playerStates[GetPlayerId(clientId)].playerObject.Despawn();

        // The client disconnected is the host
        if (clientId == 0)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void PlayerNotReady(ulong clientId, int characterSelected = 0, bool isDisconected = false)
    {
        int playerId = GetPlayerId(clientId);
        m_isTimerOn = false;
        m_timer = m_timeToStartGame;

        RemoveReadyStates(clientId, isDisconected);

        // Notify clients to change UI
        if (isDisconected)
        {
            PlayerDisconnectedClientRpc(playerId);
        }
        else
        {
            PlayerNotReadyClientRpc(clientId, playerId, characterSelected);
        }
    }
}
