using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    public CharacterController controller;

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (GetInput(out NetworkInputData input))
        {
            var move = new Vector3(input.move.x, 0, input.move.y);
            if (move.sqrMagnitude > 0)
                controller.Move(move * speed * Runner.DeltaTime);
        }
    }
}