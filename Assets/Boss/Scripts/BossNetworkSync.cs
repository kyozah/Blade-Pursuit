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
    private BossCombat _combat;
    private BossBrain.State _lastSyncedState = BossBrain.State.Idle; // ✅ Track last state for animation sync

    private void Awake()
    {
        _brain = GetComponent<BossBrain>();
        _health = GetComponentInChildren<BossHealth>();
        _combat = GetComponent<BossCombat>();
    }

    public override void Spawned()
    {
        if (_brain == null) _brain = GetComponent<BossBrain>();
        if (_health == null) _health = GetComponentInChildren<BossHealth>();
        if (_combat == null) _combat = GetComponent<BossCombat>();

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

        // Update position với lerp cao hơn để smooth hơn (15 thay vì 10)
        transform.position = Vector3.Lerp(transform.position, NetPos, Runner.DeltaTime * 15f);
        transform.eulerAngles = NetEuler;
        
        // Update health
        _health?.ApplyNetworkHp(NetHp, NetDead);
        
        // ✅ Update boss state - này là quan trọng để client thấy boss animation
        BossBrain.State newState = (BossBrain.State)NetState;
        if (_brain != null)
        {
            _brain.currentState = newState;
            _brain.currentPhase = (BossBrain.Phase)NetPhase;
            
            // ✅ Trigger animations khi state thay đổi
            if (newState != _lastSyncedState && _combat != null)
            {
                SyncAnimationToState(newState, _lastSyncedState);
                _lastSyncedState = newState;
            }
        }
    }
    
    private void SyncAnimationToState(BossBrain.State newState, BossBrain.State oldState)
    {
        Debug.Log($"[BossNetworkSync] 🎬 State changed: {oldState} → {newState}, syncing animation");
        
        // Nếu từ state khác chuyển sang Roar, trigger Roar animation
        if (newState == BossBrain.State.Roar && oldState != BossBrain.State.Roar)
        {
            // Phân biệt Roar1 vs Roar2 dựa trên phase
            if (_brain.currentPhase == BossBrain.Phase.Phase1)
                _combat.DoRoar1();
            else
                _combat.DoRoar2();
        }
        // Nếu attack, trigger animation (sẽ phải track attack type, fallback to Attack1)
        else if (newState == BossBrain.State.Attack && oldState != BossBrain.State.Attack)
        {
            // Fallback - Play Attack1 khi không biết attack type nào
            _combat.DoAttack1();
        }
    }

    public bool IsNetworkReady() => Object != null;

    public void RequestDamage(float damage)
    {
        // NetworkObject phải initialized mới gửi RPC được
        if (Object == null)
        {
            Debug.LogWarning($"[BossNetworkSync] ⚠️ Object null - NetworkObject chưa initialized. Skip damage.");
            return;
        }

        if (HasStateAuthority)
        {
            Debug.Log($"[BossNetworkSync] ✅ Host applying damage: {damage}");
            ApplyDamageAuthority(damage);
            return;
        }
        
        Debug.Log($"[BossNetworkSync] 📤 Sending RPC to host requesting {damage} damage");
        RpcRequestDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestDamage(float damage, RpcInfo info = default)
    {
        Debug.Log($"[BossNetworkSync] 🎯 RPC received - damage: {damage}, from: {info.Source}");
        if (!HasStateAuthority)
        {
            Debug.LogWarning($"[BossNetworkSync] RPC received but không phải StateAuthority? Skip.");
            return;
        }
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