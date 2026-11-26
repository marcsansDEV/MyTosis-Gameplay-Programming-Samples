using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

[CreateAssetMenu(menuName = "LilTosiAbilities/Possession", fileName = "PossessionAbility")]
public class PossessionLilTosiAbility : BaseLilTosiAbility
{
    private EventInstance m_PossessionSound;
    
    public static Action<LilTosi> OnStartPossession;
    public static Action<LilTosi> OnEndPossession;
    public static Action<LilTosi> OnJump;

    public static Action OnPossessionInteract;
    public static bool IsActivated;
    public static LilTosi PossessedLilTosi;
    
    private float m_PreviousGravity;
    
    //Movement
    private Vector3 m_Acceleration;
    
    //Jump
    private int m_AirJumps;
    private float m_LastJumpPressTime;
    private bool m_TryingToJump;
    private float m_LastGroundedTime;
    private bool m_IsOrWasGrounded;
    
    //Attack
    private float m_LastTimeAttacked;
    private readonly Collider[] m_Colliders = new Collider[20];
        
    [Header("========== MOVEMENT RELATED PROPERTIES ==========")]
    [Space(5)]
    [SerializeField] private float m_MaxSpeed;
    [SerializeField] private float m_MaxAcceleration;
    [SerializeField] private float m_Gravity;
    
    [Header("========== JUMP RELATED PROPERTIES ==========")]
    [Space(5)] 
    
    [Header("-Height-")]
    [Space(3)]
    [SerializeField] private float m_JumpHeight;
    [SerializeField] private float m_AirJumpHeight;
    
    [Header("-Input-")]
    [Space(3)]
    [SerializeField] private float m_JumpPressBufferTime = 0.3f;
    [SerializeField] private float m_CoyoteTime = 0.2f;
    
    [Header("-Other-")]
    [Space(3)]
    [SerializeField] private int m_MaxAirJumps = 2;

    [Header("========== ATTACK RELATED PROPERTIES ==========")] 
    [Space(5)] 
    [SerializeField] private float m_AttackCooldown;
    [SerializeField] private LayerMask m_AttackLayerMask;
    [SerializeField] private float m_AttackRange;
    
    [Header("========== ROTATION RELATED PROPERTIES ==========")] 
    [Space(5)] 
    [Range(0,0.2f)]
    [SerializeField] private float m_LerpRotation;

    private bool m_Blocked;

    private void Awake()
    {
    }

    public override void Init(LilTosi lilTosi)
    {
        base.Init(lilTosi);
        m_PossessionSound = UtilsMethods.GetInstanceByGUID("{4934250d-a973-439a-bbcc-d5ac37440b6b}");
        RuntimeManager.AttachInstanceToGameObject(m_PossessionSound, lilTosi.transform);
    }

    protected override void Activate()
    {
        StartPossession();
        StartCoroutine(PossessionCoroutine());
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && !m_Blocked;
    }

    private void StartPossession()
    {
        m_PossessionSound.start();

        IsActivated = true;
        m_Blocked = true;
        PossessedLilTosi = m_LilTosi;
        m_AirJumps = 0;
        m_LilTosi.Controller.OnBeforeMove += UpdatePossession;
        SetPlayerActive(false);
        RuntimeManager.PlayOneShot("{01706293-46e9-452e-ab6c-8fff9a7f6aef}");
        m_PreviousGravity = m_LilTosi.Controller.Gravity;
        m_LilTosi.Controller.Gravity = m_Gravity;
        Icon = m_Icons[1];
    }

    private void SetPlayerActive(bool active)
    {
        m_Player.Controller.ForceLock(!active);
    }

    private IEnumerator PossessionCoroutine()
    {
        yield return null;
        OnStartPossession?.Invoke(m_LilTosi);
        m_Charge = m_MaxCharge;
        while (m_Charge > 0f && !Canceled())
        {
            m_Charge -= Time.unscaledDeltaTime;
            UpdateInput();
            yield return null;
        }
        Active = false;
        StopPossession();
        OnEndPossession?.Invoke(m_LilTosi);
    }

    private void UpdateInput()
    {
        if (m_Player.Input.JumpInput)
        {
            m_TryingToJump = true;
            m_LastJumpPressTime = Time.unscaledTime;
        }

        if (m_Player.Input.AttackInput)
        {
            if (CanAttack())
            {
                TryAttack();
            }
        }
    }

    #region  Attack

    private bool CanAttack()
    {
        return m_LilTosi.Controller.IsGrounded;
    }

    private void TryAttack()
    {
        if (Time.unscaledTime - m_LastTimeAttacked > m_AttackCooldown || m_LastTimeAttacked == 0f)
        {
            Attack();
            m_LastTimeAttacked = Time.unscaledTime;
        }
    }

    private void Attack()
    {
        m_LastTimeAttacked = Time.time;
        OnPossessionInteract?.Invoke();
        
        var l_NumberFound = Physics.OverlapSphereNonAlloc(m_LilTosi.transform.position, m_AttackRange, m_Colliders, m_AttackLayerMask);
        if (l_NumberFound > 0)
        {
            foreach (var l_Collider in m_Colliders)
            {
                if (l_Collider != null)
                    AttackIfAttackable(l_Collider);
            }
        }
    }
    private void AttackIfAttackable(Collider obj)
    {
        var l_Attackables = obj.GetComponents<IAttackable>();
        
        foreach (var l_Attackable in l_Attackables)
            l_Attackable.GetAttacked(0);
    }
    #endregion
    private void StopPossession()
    {
        m_PossessionSound.stop(STOP_MODE.ALLOWFADEOUT);
        TimerManager.GetInstance().AddTimer(new Timer(1.2f, "UnBLOCK Possession"), action: ResetLiquid);
        m_LilTosi.Controller.OnBeforeMove -= UpdatePossession;
        m_LilTosi.Controller.Gravity = m_PreviousGravity;
        SetPlayerActive(true);
        PossessedLilTosi = null;
        IsActivated = false;
        Icon = m_Icons[0];
        m_Charge = m_MaxCharge;
    }

    private void ResetLiquid()
    {
        m_Blocked = false;
    }

    private void UpdatePossession()
    {
        UpdateJump();
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        var l_Input = m_Player.Input.Horizontal;
        if (!m_LilTosi.Controller.IsGrounded)
        {
            if (l_Input == Vector3.zero)
            {
                m_Acceleration = Vector3.zero;
                return;
            }
        }
        var l_DesiredVel = l_Input * m_MaxSpeed;
        var l_Vel = m_LilTosi.Controller.HorVelocity;

        var l_VelOffset = l_DesiredVel - l_Vel;
        var l_RequiredAcceleration = l_VelOffset / 0.1f;

        m_Acceleration = Vector3.ClampMagnitude(l_RequiredAcceleration, m_MaxAcceleration);
        Debug.DrawLine(m_LilTosi.transform.position, m_LilTosi.transform.position + m_Acceleration, Color.red);
        m_LilTosi.Controller.AddImpulse(m_Acceleration * (m_LilTosi.Controller.AdaptedFixedDeltaTime), false, false);
    }

    private void UpdateJump()
    {
        if (m_LilTosi.Controller.IsGrounded)
        {
            m_LastGroundedTime = Time.unscaledTime;
        }
        
        if (m_LilTosi.Controller.IsGrounded)
        {
            m_AirJumps = 0;
        }
        bool l_WasTryingToJump = Time.unscaledTime - m_LastJumpPressTime < m_JumpPressBufferTime && m_LastJumpPressTime != 0;
        bool l_WasGrounded = Time.unscaledTime - m_LastGroundedTime < m_CoyoteTime && m_LastGroundedTime != 0;

        bool l_IsOrWasTryingToJump = m_TryingToJump || (l_WasTryingToJump && m_LilTosi.Controller.IsGrounded);
        m_IsOrWasGrounded = m_LilTosi.Controller.IsGrounded || l_WasGrounded;

        bool l_AirJumpAllowed = m_AirJumps < m_MaxAirJumps;
        
        bool l_CanJump = (l_AirJumpAllowed) && l_IsOrWasTryingToJump && !m_LilTosi.Controller.IsLocked;
        if (l_CanJump)
        {
            //Jump
            float l_JumpSeed;
            if (m_IsOrWasGrounded)
                l_JumpSeed = UtilsMethods.HeightToForce(m_JumpHeight, m_LilTosi.Controller.Gravity);
            else
            {
                l_JumpSeed = UtilsMethods.HeightToForce(m_AirJumpHeight, m_LilTosi.Controller.Gravity);
            }
            m_AirJumps++;
            m_LilTosi.Controller.AddImpulse(new Vector3(0f, l_JumpSeed, 0f), false, true);
            OnJump?.Invoke(m_LilTosi);
        }
        m_TryingToJump = false; 
    }

    protected override bool Canceled()
    {
        return base.Canceled()
               || m_Player.Input.Ability;
    }

    public override void Reset()
    {
        if (!Active) return;
        OnEndPossession?.Invoke(m_LilTosi);
        StopAllCoroutines();
        StopPossession();
        base.Reset();
    }
}
