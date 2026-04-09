using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public NetworkBool sprint;
    public float cameraYaw;  // ✅ Gửi hướng camera từ Client
}