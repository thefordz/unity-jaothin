using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndGameManager : SingletonNetwork<EndGameManager>
{
    enum EndGameStatus
    {
        victory,
        defeat,
    };
    
    [SerializeField]
    private EndGameStatus m_status;   
    
    [SerializeField]
    private CharacterDataSO[] m_charactersData; 
    
    [SerializeField]
    private Transform[] m_PlayerPositions; 
    
    private int m_PlayerPositionindex;

    PlayerScore m_BestPlayer;
    private List<ulong> m_connectedClients = new List<ulong>();

    private bool isVictorious;

    [SerializeField] private AudioClip winnerSound;


    private void Start()
    {
        AudioManager.Instance.StopMusic();
    }

    public void ServerSceneInit(ulong clientId)
    {
        // Save the clients 
        m_connectedClients.Add(clientId);

        // Check if is the last client
        if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        // We do this only one time when all clients are connected so they sync correctly
        // Tell all clients instance to set the UI base on the server characters data
        int bestScore = -1;
        for (int i = 0; i < m_charactersData.Length; i++)
        {
            if (m_charactersData[i].isSelected)
            {
                GameObject playerScoreResult = NetworkObjectSpawner.SpawnNewNetworkObject(
                    m_charactersData[i].characterPrefabsScore,
                    m_PlayerPositions[m_PlayerPositionindex].position);

                // Check who has the best score
                // The score is calculated base on the enemies destroyed minus the power-ups the player used
                // Feel free to modify these values
                int foodScore = (m_charactersData[i].foodScore);
                
                var playerScore = playerScoreResult.GetComponent<PlayerScore>();

                if (foodScore > bestScore)
                {
                    m_BestPlayer = playerScore;
                    bestScore = foodScore;
                }



                playerScore.SetPlayer(foodScore);
                
                // Set the values of the score on every instance
                SetPlayerDataClientRpc(foodScore, playerScoreResult.name);
                
                
                m_PlayerPositionindex++;
            }
        }

        CheckWhoWinner(m_charactersData[0].foodScore, m_charactersData[1].foodScore);
        winnerSoundServerRpc();
    }
    
    private void CheckWhoWinner(int input, int input2)
    {
        if (input > input2)
        {
            m_BestPlayer.BestPlayer();
            BestPlayerClientRpc(m_BestPlayer.name);
            
        }
        else if(input2 > input)
        {
            m_BestPlayer.BestPlayer();
            BestPlayerClientRpc(m_BestPlayer.name);
        }
        else
        {
            m_BestPlayer.Draw();
            DrawClientRpc(m_BestPlayer.name);
        }
    }
    
    public void GoToMenu()
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
    
    
    [ClientRpc]
    private void SetPlayerDataClientRpc(
        int score,
        string playerScoreName)
    {
        GameObject playerScore = GameObject.Find(playerScoreName);
        playerScore.GetComponent<PlayerScore>().SetPlayer(score);
    }

    [ClientRpc]
    private void BestPlayerClientRpc(string playerScoreName)
    {
        if (IsServer)
            return;


        GameObject playerScore = GameObject.Find(playerScoreName);
        playerScore.GetComponent<PlayerScore>().BestPlayer();
        
    }

    [ClientRpc]
    private void DrawClientRpc(string playerScoreName)
    {
        if (IsServer)
        {return;
            
        }
        
        GameObject playerScore = GameObject.Find(playerScoreName);
        playerScore.GetComponent<PlayerScore>().Draw();
    }

    private IEnumerator HostShutdown()
    {
        // Tell all clients to shutdown
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
        
        
        [ServerRpc(RequireOwnership = false)]
        void winnerSoundServerRpc()
        {
            winnerSoundClientRpc();
        }

        [ClientRpc]
        void winnerSoundClientRpc()
        {
            AudioManager.Instance?.PlaySoundEffect(winnerSound);
        }
}
