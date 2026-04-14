using Fusion;
using UnityEngine;

[RequireComponent(typeof(BossBrain))]
public class BossNetworkSync : NetworkBehaviour
{
    [Networked] private Vector3 NetPos { get; set; }
    [Networked] private Vector3 NetEuler { get; set; }
    [Networked] private float NetHp { get; set; }
    [Networked] private NetworkBool NetDead { get; set; }
    [Networked] private NetworkBool NetInitialized { get; set; }
    [Networked] private int NetState { get; set; }  // ✅ Boss AI state (Idle/Roar/Move/Attack/Cooldown/Dead)
    [Networked] private int NetPhase { get; set; } // ✅ Boss phase (Phase1/Phase2)

    private BossBrain _brain;
    private BossHealth _health;

    private void Awake()
    {
        _brain = GetComponent<BossBrain>();
        _health = GetComponentInChildren<BossHealth>();
    }

    public override void Spawned()
    {
        if (_brain == null) _brain = GetComponent<BossBrain>();
        if (_health == null) _health = GetComponentInChildren<BossHealth>();

        if (HasStateAuthority)
        {
            NetPos = transform.position;
            NetEuler = transform.eulerAngles;
            if (_health != null)
            {
                NetHp = _health.CurrentHP;
                NetDead = _health.CurrentHP <= 0f;
            }
            NetState = (int)BossBrain.State.Idle;
            NetPhase = (int)BossBrain.Phase.Phase1;
            NetInitialized = true;
            _brain?.SetSimulationEnabled(true);
            Debug.Log("[BossNetworkSync] ✅ Host boss initialized");
        }
        else
        {
            _brain?.SetSimulationEnabled(false);
            Debug.Log("[BossNetworkSync] ✅ Client boss initialized (simulation disabled)");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            // HOST: Collect boss state
            NetPos = transform.position;
            NetEuler = transform.eulerAngles;
            if (_health != null)
            {
                NetHp = _health.CurrentHP;
                NetDead = _health.CurrentHP <= 0f;
            }
            if (_brain != null)
            {
                NetState = (int)_brain.currentState;
                NetPhase = (int)_brain.currentPhase;
            }
            return;
        }

        // CLIENT: Apply boss state
        if (!NetInitialized) return;

        // Update position
        transform.position = Vector3.Lerp(transform.position, NetPos, Runner.DeltaTime * 10f);
        transform.eulerAngles = NetEuler;
        
        // Update health
        _health?.ApplyNetworkHp(NetHp, NetDead);
        
        // ✅ Update boss state - này là quan trọng để client thấy boss animation
        if (_brain != null)
        {
            _brain.currentState = (BossBrain.State)NetState;
            _brain.currentPhase = (BossBrain.Phase)NetPhase;
        }
    }

    public bool IsNetworkReady() => Object != null;

    public void RequestDamage(float damage)
    {
        // NetworkObject phải initialized mới gửi RPC được
        if (Object == null)
        {
            Debug.LogWarning($"[BossNetworkSync] Object null - không thể gửi damage RPC. Applying locally.");
            ApplyDamageAuthority(damage);
            return;
        }

        if (HasStateAuthority)
        {
            ApplyDamageAuthority(damage);
            return;
        }
        
        Debug.Log($"[BossNetworkSync] Client requesting {damage} damage to boss");
        RpcRequestDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestDamage(float damage, RpcInfo info = default)
    {
        Debug.Log($"[BossNetworkSync] RPC received - damage: {damage}, from: {info.Source}");
        ApplyDamageAuthority(damage);
    }

    private void ApplyDamageAuthority(float damage)
    {
        if (!HasStateAuthority || _health == null || _health.CurrentHP <= 0f)
        {
            if (HasStateAuthority)
                Debug.LogWarning($"[BossNetworkSync] Boss already dead, ignoring damage");
            return;
        }
        
        _health.TakeDamageAuthority(damage);
        NetHp = _health.CurrentHP;
        NetDead = _health.CurrentHP <= 0f;
        Debug.Log($"[BossNetworkSync] Boss HP: {NetHp}/{_health.MaxHP}");
    }
}