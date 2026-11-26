using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAttack : BasePlayerComponent
{
    [SerializeField] private EventReference m_AttackEventReference;
    [SerializeField] private GameObject m_ImpactParticles;
    

    [SerializeField] private UnityEvent OnAttack;
    public Action OnEnterAttack;
    public Action OnExitAttack;
    private float m_LastTimeAttacked;
    
    [Header("========== ATTACK RELATED PROPERTIES ==========")]
    [Space(3)]
    [SerializeField] private int m_Damage;
    [SerializeField] private float m_Cooldown;
    [Min(0)] [SerializeField] private float m_SpeedMultiplier;
    [Min(0)] [SerializeField] private float m_GravityMultiplier;

    [Header("========== OTHER PROPERTIES ==========")]
    [Space(3)]
    private ExternalTriggerCall m_ExternalTriggerCall;

    private SphereCollider m_TriggerCollider;

    public bool IsOnAttack { get; private set; } = false;
    public float SpeedMultiplier => m_SpeedMultiplier;
    public float GravityMultiplier => m_GravityMultiplier;

    protected override void Awake()
    {
        base.Awake();
        // m_ExternalTriggerCall = UtilsMethods.ReturnRepeatChild<ExternalTriggerCall>(gameObject);
        m_ExternalTriggerCall = GetComponentInChildren<ExternalTriggerCall>();
        m_TriggerCollider = m_ExternalTriggerCall.GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        m_ExternalTriggerCall.OnEnterTrigger += CheckIfAttackable;
    }
    private void OnDisable()
    {
        m_ExternalTriggerCall.OnEnterTrigger -= CheckIfAttackable;
    }

    private void Start()
    {
        m_ExternalTriggerCall.gameObject.SetActive(false);
    }

    private void CheckIfAttackable(Collider obj)
    {
        var l_Attackables = obj.GetComponents<IAttackable>();
        foreach (var l_Attackable in l_Attackables)
            l_Attackable.GetAttacked(m_Damage);

        if (l_Attackables.Length > 0)
        {
            var l_Dir = obj.transform.position - m_TriggerCollider.transform.position;
            l_Dir.y = 0f;
            var l_Point = obj.ClosestPointOnBounds(m_TriggerCollider.transform.position);
            var l_Instance = Instantiate(m_ImpactParticles);
            l_Instance.transform.position = l_Point;
        }
    }

    private void Update()
    {
        if (CanAttack())
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time - m_LastTimeAttacked > m_Cooldown && !IsOnAttack && m_Player.Input.AttackInput)
        {
            Attack();
        }
    }

    private bool CanAttack()
    {
        return m_Player.Fsm.IsWalking && !m_Player.Creator.IsCreating && !m_Player.Controller.IsLocked && !m_Player.Thrower.IsThrowing;
    }

    private void Attack()
    {
        OnAttack?.Invoke();
        SetIsOnAttack(true);
        RuntimeManager.PlayOneShot(m_AttackEventReference);
        RumbleManager.GetInstance().CreateOneShotPulse(0.2f, 0.9f, 0.8f);
        
        //Stop vertical vel
        var l_VelY = m_Player.Controller.Velocity.y;
        float l_M = 1f;
        l_VelY = l_VelY <= 0 ? 0 : l_VelY * l_M;
        m_Player.Controller.AddImpulse(new Vector3( 0, l_VelY, 0), false, true);
    }

    public void ActivateAttackTrigger(bool activate)
    {
        if (activate)
            m_ExternalTriggerCall.gameObject.SetActive(true);
        else
        {
            m_ExternalTriggerCall.gameObject.SetActive(false);
            m_LastTimeAttacked = Time.time;
        }
    }

    public void SetIsOnAttack(bool value)
    {
        if (value) OnEnterAttack?.Invoke();
        else OnExitAttack?.Invoke();
        IsOnAttack = value;
    }

    public void AttackEnd()
    {
        OnExitAttack?.Invoke();
    }
}

public interface IAttackable
{
    public void GetAttacked(int damage);
}
