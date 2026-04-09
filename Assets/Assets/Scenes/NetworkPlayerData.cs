using Fusion;
using UnityEngine;

[System.Serializable]
public struct NetworkPlayerData : INetworkStruct
{
    public NetworkString<_16> playerName;  // Tối đa 16 ký tự
    public PlayerRef playerRef;
    
    public NetworkPlayerData(string name, PlayerRef player)
    {
        playerName = name;
        playerRef = player;
    }
}