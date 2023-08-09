using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

[CreateAssetMenu(fileName = "Lobby Data",menuName ="Lobby Data", order = 3)]
public class LobbyDataSO : ScriptableObject
{
    [Header("Data")]
    public string playerId;
   
    public Lobby connectedLobby;

   
}
