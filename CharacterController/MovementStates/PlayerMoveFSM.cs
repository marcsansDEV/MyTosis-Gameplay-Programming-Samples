using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMoveFSM : BasePlayerComponent
{
    private readonly TStates m_InitialState = TStates.Walk;
    private List<PlState> m_PlStatesList = new();
    
    private Vector3 m_Acceleration;
    private float m_MaxAcceleration;

    [SerializeField] private bool m_DragInAir;

    public bool IsWalking => m_CurrentState.Type == TStates.Walk;
    public bool IsRolling => m_CurrentState.Type == TStates.BallRun;
    public bool IsGliding => m_CurrentState.Type == TStates.Glide;
    public float MaxSpeed { get; private set; }
    
    private float m_ExtSpeedMod = 1f;
    private float m_ExtGravMod = 1f;
    public enum TStates
    {
        Walk = 0, BallRun, Glide
    }

    private PlState m_CurrentState;

    protected override void Awake()
    {
        base.Awake();
        m_PlStatesList = GetComponents<PlState>().ToList();
    }
    private void OnEnable()
    {
        m_Player.Controller.OnBeforeMove += OnBeforeMove;
        m_Player.Jump.OnJump += ResetExtGravMod;
        m_Player.Attack.OnEnterAttack += SetAttackExtGravMod;
        m_Player.Attack.OnEnterAttack += SetAttackExtSpeedMod;
        m_Player.Attack.OnExitAttack += ResetExtGravMod;
        m_Player.Attack.OnExitAttack += ResetExtSpeedMod;
    }

    #region  Modifiers
    private void ResetExtSpeedMod()
    {
        m_ExtSpeedMod = 1f;
    }

    private void SetAttackExtSpeedMod()
    {
        m_ExtSpeedMod = m_Player.Attack.SpeedMultiplier;
    }

    private void SetAttackExtGravMod()
    {
        m_ExtGravMod = m_Player.Attack.GravityMultiplier;
    }

    private void ResetExtGravMod()
    {
        m_ExtGravMod = 1f;
    }
    #endregion

    private void OnDisable()
    {
        m_Player.Controller.OnBeforeMove -= OnBeforeMove;
        m_Player.Jump.OnJump -= ResetExtGravMod;
        m_Player.Attack.OnEnterAttack -= SetAttackExtGravMod;
        m_Player.Attack.OnEnterAttack -= SetAttackExtSpeedMod;
        m_Player.Attack.OnExitAttack -= ResetExtGravMod;
        m_Player.Attack.OnExitAttack -= ResetExtSpeedMod;
    }
    protected virtual void Start()
    {
        DisableStates();
        m_CurrentState = GetState(m_InitialState);
        ChangeState(m_InitialState);
    }
    private void Update()
    {
        Vector2 l_LastHorVelocity = new Vector2(m_Player.Controller.LastVel.x, m_Player.Controller.LastVel.z);
        if (l_LastHorVelocity.magnitude != m_Player.Controller.HorVelocity.magnitude)
        {
            //if(m_Player.Controller.HorVelocity.magnitude <= 0) { audio }
            //else
        }
    }

    private void OnBeforeMove()
    {
        //Attack things...
        var l_CurrentMaxSpeed = MaxSpeed * m_ExtSpeedMod;
        var l_CurrentGravMult = m_CurrentState.GravityMod * m_ExtGravMod;
        m_Player.Controller.GravityModifier = l_CurrentGravMult;
        //  
        
        var l_Input = m_Player.Input.Horizontal;

        if (m_Player.Controller.IsLocked)
            l_Input = Vector3.zero;
        
        if (m_CurrentState.NormalizedInput)
            l_Input.Normalize();
        
        var l_DesiredVel = l_Input * l_CurrentMaxSpeed;
        var l_Vel = m_Player.Controller.HorVelocity;

        var l_VelOffset = l_DesiredVel - l_Vel;
        var l_RequiredAcceleration = l_VelOffset / 0.1f;

        m_Acceleration = Vector3.ClampMagnitude(l_RequiredAcceleration, m_MaxAcceleration);

        if (!m_Player.Controller.IsGrounded)
        {
            if (l_Input == Vector3.zero || m_Player.Controller.IsSliding)
            {
                if (!m_DragInAir)
                    m_Acceleration = Vector3.zero;
            }
        }
        m_Player.Controller.AddImpulse(m_Acceleration * (m_Player.Controller.AdaptedFixedDeltaTime), false, false);
    }
    public void ChangeState(TStates newStateType)
    {
        var l_NewState = GetState(newStateType);
        m_CurrentState.OnExit();
        m_CurrentState = l_NewState;
        m_CurrentState.OnEnter();
    }
    public PlState GetState(TStates newStateType)
    {
        foreach (var l_State in m_PlStatesList)
            if (l_State.Type == newStateType) return l_State;
        return null;
    }
    public void SetMaxSpeed(float maxSpeed)
    {
        MaxSpeed = maxSpeed;
    }
    public void SetMaxAcceleration(float acceleration)
    {
        m_MaxAcceleration = acceleration;
    }
    private void DisableStates()
    {
        foreach (var l_State in m_PlStatesList)
            l_State.enabled = false;
    }


    public void ForceExitGlide()
    {
        if (IsGliding)
        {
            ChangeState(TStates.Walk);
        }
    }
}