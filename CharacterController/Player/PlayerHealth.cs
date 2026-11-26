using System;
using FMODUnity;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerHealth : BasePlayerComponent, ITosiHealthSystem, IRestartLevelElement
{
    private EventReference m_RespawnRef;
    [SerializeField] private ParticleSystem m_DeathParticles;
    
    [SerializeField] private EventReference m_HurtReference;
    [SerializeField] private EventReference m_DeathReference;

    [SerializeField] private SkinnedMeshRenderer m_MeshRenderer;
    [SerializeField] private float m_DamagedStateDuration = 0.1f;
    [SerializeField] private int m_MaxHealth;
    [SerializeField] private int m_HitDirVerticalAngle;
    [FormerlySerializedAs("m_KockUpForce")]
    [SerializeField] private int m_KnockUpForce;
    [SerializeField] private float m_StunDuration;
    private int m_Health;

    [SerializeField] private UnityEvent m_GetHit;
    [SerializeField] private UnityEvent m_Die;
    [SerializeField] private UnityEvent m_OnHeal;
    public Action OnHit;
    public static Action OnDeath;
    
    //DEBUG
    private TextMeshProUGUI m_HealthText;
    private Vector3 m_StartPos;
    private Quaternion m_StartRot;
    private bool m_Immune;

    protected override void Awake()
    {
        base.Awake();
        m_Health = m_MaxHealth;
        m_HealthText = UtilsMethods.ReturnRepeatChild(gameObject, "Health Text").GetComponent<TextMeshProUGUI>();
        m_StartPos = transform.position;
        m_StartRot = transform.rotation;

        m_RespawnRef = UtilsMethods.GetReferenceByGUID("{ebdd6dc4-1ed1-4aa7-a90f-825f68548f1e}");
    }

    private void Start()
    {
        GameUIManager.GetInstance().SetMaxHeartAmount(m_MaxHealth, true, m_MaxHealth);
    }

    private void OnEnable()
    {
        LevelManager.GetInstance().AddResetElement(this);

    }

    private void OnDisable()
    {        
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    public void GetHit(Vector3 hitPoint, int damage)
    {
        if (m_Immune)
            return;
        
        KnockUp(hitPoint, m_KnockUpForce, m_StunDuration);
        TakeDamage(damage);
        EnterDamaged();
        Invoke(nameof(ExitDamaged), m_DamagedStateDuration);
        m_Player.Fsm.ForceExitGlide();
    }

    private void EnterDamaged()
    {
        m_MeshRenderer.material.SetFloat("_HasGradient", 1.0f);
    }
    private void ExitDamaged()
    {
        m_MeshRenderer.material.SetFloat("_HasGradient", 0.0f);
    }

    private void TakeDamage(int damage)
    {
        m_Health -= damage;
        GameUIManager.GetInstance().TryUpdateHearts(m_Health);
        RumbleManager.GetInstance().CreateOneShotPulse(0.1f, 0.1f, 0.2f);
        if (m_Health < 1)
        {
            RuntimeManager.PlayOneShot(m_DeathReference);
            OnDeath?.Invoke();
            Instantiate(m_DeathParticles, transform.position + Vector3.up * 1f, m_DeathParticles.transform.rotation);
        }
        else
        {
            RuntimeManager.PlayOneShot(m_HurtReference);
            m_GetHit?.Invoke();
            OnHit?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        m_Health += amount;
        GameUIManager.GetInstance().TryUpdateHearts(m_Health);
        if (m_Health > m_MaxHealth)
            m_Health = m_MaxHealth;
        m_OnHeal.Invoke();
        RuntimeManager.PlayOneShot("{ed601415-92be-4c68-9e3e-2e22379e8975}");
    }

    private void KnockUp(Vector3 hitPoint, float impactForce, float stunDuration)
    {
        var l_ImpactForward = UtilsMethods.GetDirection(hitPoint, transform.position);
        l_ImpactForward.y = 0f;
        l_ImpactForward.Normalize();
        l_ImpactForward =
            UtilsMethods.RotateVectorInAxis(l_ImpactForward, UtilsMethods.RotAxis.Pitch, m_HitDirVerticalAngle);
        m_Player.Controller.AddImpulse(l_ImpactForward * impactForce, true, true);
        m_Player.Controller.LockMovement(stunDuration);
        Debug.DrawLine(transform.position, transform.position + l_ImpactForward * 100, Color.black, 3);
    }

    public void GetDamaged(int damage, Vector3 source)
    {
        if (m_Player.Controller.IsLocked) return;
        GetHit(source, damage);
    }

    public void Restart()
    {
        m_Health = m_MaxHealth;
        GameUIManager.GetInstance().TryUpdateHearts(m_Health);
        TimerManager.GetInstance().AddTimer
                (
                    GameManagerStateMachine.Instance.Context.StaticData.Player.RespawnTime,
                    MoveToPosition,
                    "MovePlayer"
                );
    }

    public void MoveToPosition()
    {
        RuntimeManager.PlayOneShot(m_RespawnRef);
        var l_SpawnTransform = LevelManager.GetInstance().SpawnTransform;
        m_Player.Controller.enabled = false;
        transform.position = l_SpawnTransform.position;
        m_Player.Controller.enabled = true;
        m_Player.transform.rotation = l_SpawnTransform.rotation;
        Vector3 l_Offset = l_SpawnTransform.rotation * new Vector3(0, 3, -10);
        CinemachineCameraSwitcher.GetInstance().GetCurrentCamera().ForceCameraPosition(transform.position + l_Offset, transform.rotation); //
    }

    public void EnterImmune()
    {
        m_Immune = true;
    }
    public void ExitImmune()
    {
        m_Immune = false;
    }
}
