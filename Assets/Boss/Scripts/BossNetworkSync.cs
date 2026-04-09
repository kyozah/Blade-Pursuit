using Fusion;
using UnityEngine;

[RequireComponent(typeof(BossBrain))]
public class BossNetworkSync : NetworkBehaviour
{
    [Networked] private Vector3 NetPos { get; set; }
    [Networked] private Vector3 NetEuler { get; set; }
    [Networked] private float NetHp { get; set; }
    [Networked] private NetworkBool NetDead { get; set; }

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
            _brain?.SetSimulationEnabled(true);
        }
        else
        {
            _brain?.SetSimulationEnabled(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            NetPos = transform.position;
            NetEuler = transform.eulerAngles;
            if (_health != null)
            {
                NetHp = _health.CurrentHP;
                NetDead = _health.CurrentHP <= 0f;
            }
            return;
        }

        transform.position = Vector3.Lerp(transform.position, NetPos, Runner.DeltaTime * 10f);
        transform.eulerAngles = NetEuler;
        _health?.ApplyNetworkHp(NetHp, NetDead);
    }

    public void RequestDamage(float damage)
    {
        if (HasStateAuthority)
        {
            ApplyDamageAuthority(damage);
            return;
        }

        RpcRequestDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestDamage(float damage)
    {
        ApplyDamageAuthority(damage);
    }

    private void ApplyDamageAuthority(float damage)
    {
        if (!HasStateAuthority || _health == null || _health.CurrentHP <= 0f)
            return;

        _health.TakeDamageAuthority(damage);
        NetHp = _health.CurrentHP;
        NetDead = _health.CurrentHP <= 0f;
    }
}
