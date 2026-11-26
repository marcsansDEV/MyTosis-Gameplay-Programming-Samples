using System;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;

public class EntityController : MonoBehaviour, IRestartLevelElement
{
    public Action OnBeforeMove;
    public Action OnAfterMove;
    public Action<bool> OnIsGroundedChanged;
    public Action OnLanded;
    
    private CharacterController m_CharacterController;
    private Vector3 m_Velocity;
    private Vector3 m_LastHitNormal;
    private Vector3 m_LastHitPoint;

    private float m_MaxMagnitudeHor = 20;
    private float m_MaxMagnitudeVer = 25;
    //Grounded Settings
    private bool m_IsStraightGrounded;
    private Vector3 m_GroundNormal;
    private bool m_WasGrounded;
    private float m_LockDuration;
    
    [Header("========= CONTROLLER RELATED PROPERTIES =========")]
    [Space(5)]
    public float Gravity = -9.81f;
    [Range(0f, 2f)]
    [SerializeField] private float m_BaseWallBounciness;
    // [SerializeField] private float m_MaxSlopeAngle;
    [SerializeField] private bool m_IsSliding;
    [SerializeField] private LayerMask m_GroundedLayerMask;
    [SerializeField] private LayerMask m_SlideCheckLayermask;

    private bool m_ForceLocked = false;
    [SerializeField] private Vector3 m_LeftOverVelocity;
    [SerializeField] private float m_RadiusSphereCast = 0.22f;
    [SerializeField] private float m_MaxDistanceSphereCast = 0.1f;
    public void ForceLock(bool locked) => m_ForceLocked = locked;
    #region  Getters
    public bool IsLocked => m_LockDuration > 0.0f || m_ForceLocked;
    public bool IsGrounded;
    public bool IsSliding => m_IsSliding;

    public float AdaptedFixedDeltaTime => Mathf.Min(0.02f, SlowTimeLilTosiAbility.AdaptedFixedDeltaTime);
    public float AdaptedDeltaTime => SlowTimeLilTosiAbility.AdaptedDeltaTime;
    
    public Vector3 Velocity
    {
        get => m_Velocity;
        set => m_Velocity = value;
    }

    public Vector3 LastVel { get; private set; } = Vector3.zero;
    public Vector3 LastVelNotZero { get; private set; } = Vector3.zero;
    public Vector3 HorVelocity
    {
        get
        {
            var l_Vel = m_Velocity;
            l_Vel.y = 0;
            return l_Vel;
        }
    }

    public bool UseGravity { get; set; } = true;
    public float GravityModifier { get; set; } = 1f;

    #endregion

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        m_CharacterController.enabled = true;
        LevelManager.GetInstance().AddResetElement(this);
    }
    private void OnDisable()
    {
        m_CharacterController.enabled = false;
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    private void Update()
    {
        m_LockDuration -= AdaptedDeltaTime;
        DrawLines();
    }
   
    private void FixedUpdate()
    {
        if (UseGravity)
            UpdateGravity();
        UpdateMovement();
        UpdateGrounded();
        LastVel = m_Velocity;
        if (m_Velocity.y != 0)
            LastVelNotZero = m_Velocity;
    }
    private void UpdateMovement()
    {
        if (m_CharacterController.enabled)
        {
            OnBeforeMove?.Invoke();
            if (m_IsSliding)
            {
                AddSlidingVel();
                m_IsSliding = false;
            }

            //if (HorVelocity.magnitude > m_MaxMagnitudeHor)
            //{
            //    var l_Vel = HorVelocity.normalized * m_MaxMagnitudeHor;
            //    l_Vel.y = m_Velocity.y;
            //    m_Velocity = l_Vel;
            //}

            //if (Mathf.Abs(m_Velocity.y) > m_MaxMagnitudeVer)
            //{
            //    Debug.Log(m_Velocity.y + " " + name);
            //    m_Velocity.y = m_MaxMagnitudeVer * Mathf.Sign(m_Velocity.y);
            //}
            
            m_CharacterController.Move(m_Velocity * AdaptedFixedDeltaTime);
            OnAfterMove?.Invoke();
        }
    }

    private void AddSlidingVel()
    {
        var l_Dir = m_LastHitNormal;
        l_Dir.y = 0f;
        l_Dir.Normalize();
        AddImpulse(l_Dir * (50f * AdaptedFixedDeltaTime), false, false);
    }

    private void UpdateGrounded()
    {
        SphereCastGround();
        var l_SlideGrounded = false;
        if (IsGrounded)
        {
            m_Velocity.y = 0f;
        }
        else if (m_CharacterController.isGrounded)
        {
            var l_Dir = UtilsMethods.GetDirecton(m_LastHitPoint, transform.position);
            l_Dir.y = 0f;
            Ray l_Ray = new Ray(m_LastHitPoint + l_Dir.normalized * 0.05f, Vector3.down);
            var l_Distance = .4f;
            m_IsSliding = !Physics.Raycast(l_Ray, l_Distance, m_SlideCheckLayermask);
            IsGrounded = !m_IsSliding;
            l_SlideGrounded = IsGrounded;
        }
        if (m_WasGrounded != IsGrounded && !l_SlideGrounded)
        {
            OnIsGroundedChanged?.Invoke(IsGrounded);
            if (IsGrounded)
            {
                if (LastVel.y <-0.01f)
                    OnLanded?.Invoke();
                m_Velocity.y = 0f;
            }
            m_WasGrounded = IsGrounded;
        }
    }

    private void SphereCastGround()
    {
        IsGrounded = false;
        m_GroundNormal = Vector3.zero;
        // another way
        var l_Radius = m_RadiusSphereCast;
        var l_CastOrigin = transform.position + Vector3.up * l_Radius;
        if (Physics.SphereCast(l_CastOrigin, l_Radius, Vector3.down, out RaycastHit l_Hit, m_MaxDistanceSphereCast, m_GroundedLayerMask))
        {
            IsGrounded = true;
            m_GroundNormal = l_Hit.normal;
        }
    }

    private void UpdateGravity()
    {
        if (IsGrounded)
            return;

        m_Velocity.y += Gravity * AdaptedFixedDeltaTime * GravityModifier;
    }
    #region WallImpact


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_LastHitNormal = hit.normal;
        m_LastHitPoint = hit.point;
        CheckIfWallImpact(hit);
        if(!IsGrounded && !m_IsSliding)
        {
            m_LeftOverVelocity = Vector3.ProjectOnPlane(Velocity, m_LastHitNormal);
            if (LastVel.y > 0)
                AddImpulse(m_LeftOverVelocity , true, true);
            else
                AddImpulse(m_LeftOverVelocity * AdaptedDeltaTime, false, false);
        }
    }
    
    private void CheckIfWallImpact(ControllerColliderHit hit)
    {
        var l_HitNormal = hit.normal;
        l_HitNormal.y = 0;
        var l_FacingWall = Vector3.Angle(l_HitNormal, HorVelocity) > 90f;
        if (l_FacingWall && IsGrounded && !m_IsSliding)
            HandleWallImpact(hit);
    }
    
    private void HandleWallImpact(ControllerColliderHit hit)
    {
        var l_Normal = hit.normal;
        l_Normal.Normalize();
        var l_Angle = Vector3.Angle(Vector3.up, hit.normal);
        var l_GroundAngle = Vector3.Angle(Vector3.up, m_GroundNormal);
        var l_AnglesAreEqual = Mathf.Abs(l_Angle - l_GroundAngle) < 3f;
        var l_Apply = !l_AnglesAreEqual && l_Angle is > 50f or < 30f;
        
        if (l_Apply)
        {
            var l_ProjectedVel = Vector3.Project(HorVelocity, l_Normal);
            if (LastVel.y <0)
            {
                l_ProjectedVel.y = 0f;
            }
            var l_AppliedVel = -l_ProjectedVel.normalized * (m_BaseWallBounciness);
            Debug.DrawLine(hit.point, hit.point + l_AppliedVel, Color.red, 4f);
            AddImpulse(l_AppliedVel, false, false);
        }
    }

    #endregion

    private void DrawLines()
    {
        Debug.DrawLine(transform.position + Vector3.up * 1, transform.position + Vector3.up * 1 + m_Velocity, Color.yellow);
    }
    
    public void AddImpulse(Vector3 velocity, bool overrideHorizontal, bool overrideVertical)
    {
        if (overrideHorizontal)
        {
            m_Velocity.z = velocity.z;
            m_Velocity.x = velocity.x;
        }
        else
        {
            m_Velocity.z += velocity.z;
            m_Velocity.x += velocity.x;
        }
        
        if (overrideVertical)
        {
            m_Velocity.y = velocity.y;
        }
        else
        {
            m_Velocity.y += velocity.y;
        }
    }
    public void LockMovement(float stunTime)
    {
        if (m_LockDuration < stunTime)
        {
            m_LockDuration = stunTime;
        }
    }

    public void Restart()
    {
        m_Velocity = Vector3.zero;
    }
}


