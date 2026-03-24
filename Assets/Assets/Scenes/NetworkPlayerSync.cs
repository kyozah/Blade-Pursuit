using Fusion;
using UnityEngine;

// Gán script này vào PlayerPrefab cùng với ThirdPersonController
// NetworkObject phải có trên PlayerPrefab
public class NetworkPlayerSync : NetworkBehaviour
{
    private ThirdPersonController _controller;
    private ThirdPersonCamera _camera;

    [Networked] private NetworkButtons PrevButtons { get; set; }
    [Networked] public Vector2 NetworkedMoveInput { get; set; }
    [Networked] public NetworkBool NetworkedSprint { get; set; }

    public override void Spawned()
    {
        _controller = GetComponent<ThirdPersonController>();

        if (HasInputAuthority)
        {
            // Chỉ máy sở hữu nhân vật này mới cần camera
            _camera = FindFirstObjectByType<ThirdPersonCamera>();
            if (_camera != null)
                _controller.cameraController = _camera;

            // Bật Input System chỉ cho máy sở hữu
            _controller.enabled = true;
        }
        else
        {
            // Máy khác: tắt ThirdPersonController, để NetworkPlayerSync điều khiển
            _controller.enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            // Truyền input từ Fusion vào ThirdPersonController
            _controller.SetNetworkInput(input.move, input.sprint);
        }
    }

    public override void Render()
    {
        // Sync vị trí/rotation cho các máy khác (interpolation)
        if (!HasInputAuthority)
        {
            // Fusion tự sync transform nếu có NetworkTransform component
        }
    }
}