using Fusion;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyNetworkSync : NetworkBehaviour
{
    [Networked] private Vector3 NetPos { get; set; }
    [Networked] private Vector3 NetEuler { get; set; }
    [Networked] private float NetHealth { get; set; }
    [Networked] private NetworkBool NetDead { get; set; }
    [Networked] private NetworkBool NetInitialized { get; set; }

    private Enemy _enemy;
    private Rigidbody _rb;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _rb = GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        if (_enemy == null)
            _enemy = GetComponent<Enemy>();

        if (HasStateAuthority)
        {
            NetPos = transform.position;
            NetEuler = transform.eulerAngles;
            NetHealth = _enemy != null ? _enemy.GetCurrentHealth() : 0f;
            NetDead = _enemy != null && _enemy.IsDead;
            NetInitialized = true;
            _enemy?.SetSimulationEnabled(true);
        }
        else
        {
            _enemy?.SetSimulationEnabled(false);
            if (_rb != null)
                _rb.isKinematic = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            NetPos = transform.position;
            NetEuler = transform.eulerAngles;
            if (_enemy != null)
            {
                NetHealth = _enemy.GetCurrentHealth();
                NetDead = _enemy.IsDead;
            }
            return;
        }

        if (!NetInitialized)
            return;

        transform.position = Vector3.Lerp(transform.position, NetPos, Runner.DeltaTime * 12f);
        transform.eulerAngles = NetEuler;
        _enemy?.ApplyNetworkMirrorState(NetHealth, NetDead);
    }

    public bool IsNetworkReady()
    {
        return Object != null;
    }

    public void RequestDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (HasStateAuthority)
        {
            ApplyDamageAuthority(damage, attackerPosition, attackerForward);
            return;
        }

        RpcRequestDamage(damage, attackerPosition, attackerForward);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        ApplyDamageAuthority(damage, attackerPosition, attackerForward);
    }

    private void ApplyDamageAuthority(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (!HasStateAuthority || _enemy == null || _enemy.IsDead)
            return;

        _enemy.ApplyDamageAuthority(damage, attackerPosition, attackerForward);
        NetHealth = _enemy.GetCurrentHealth();
        NetDead = _enemy.IsDead;
    }
}
