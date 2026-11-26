using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class LilTosiThrower : MonoBehaviour, IRestartLevelElement
{
    [SerializeField] private EventReference m_ThrowEventReference;
    [SerializeField] private EventReference m_PutDownReference;
    
    public UnityEvent OnPrepareThrowEnter;
    public UnityEvent OnPrepareThrowExit;

    public Action<LilTosi> OnPickUp;
    public Action<LilTosi> OnThrow;
    
    
    private LineRenderer m_Lr;
    private Camera m_MainCam;
    private Player m_Player;

    private float m_LastThrowPressTime;
    private Vector3 m_LandThrowPos;
    
    private Collider[] m_Colliders = new Collider[5];

    private MeshRenderer m_ThrowPointRenderer;
    
    [Header("========= THROW RELATED PROPERTIES =========")] 
    [SerializeField] private float m_TipiVelMagnitude = 17;
    [Range(0.0f, 60.0f)]
    [SerializeField] private float m_TipiThrowAngle = 48;
    [Space]
    [SerializeField] private float m_TenseVelMagnitude = 17;
    [Range(0.0f, 60.0f)]
    [SerializeField] private float m_TenseThrowAngle = 48;
    
    [Header("========= PICK UP RELATED PROPERTIES =========")] 
    [SerializeField] private float m_PickUpRadius;
    
    [Header("========= PUT DOWN RELATED PROPERTIES =========")] 
    [SerializeField] private float m_PressPutDownBufferTime;

    [Header("============ RENDER LINE RELATED PROPERTIES ============")] 
    [SerializeField] private LayerMask m_CollisionLayer;
    [SerializeField] private float m_MaxRenderTime;
    [SerializeField] private int m_MaxLinePoints;
    [SerializeField] private float m_LandingRadius = .25f;
    [SerializeField] private Material m_TenseLineMat;
    [SerializeField] private Material m_TipiLineMat;
    
    private LilTosi m_LilTosiToPick;
    private float ThrowVelocity => HeldLilTosi.Type == LilTosi.TType.Tipi? m_TipiVelMagnitude : m_TenseVelMagnitude;
    private float ThrowAngle => HeldLilTosi.Type == LilTosi.TType.Tipi? m_TipiThrowAngle : m_TenseThrowAngle;

    public bool IsHolding => HeldLilTosi != null;
    public bool IsThrowing { get; private set; }
    public LilTosi HeldLilTosi { get; private set; }
    
    private void Awake()
    {
        m_MainCam = Camera.main;
        m_Player = GetComponent<Player>();
        m_ThrowPointRenderer = GameObject.Find("ThrowPoint").GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        m_Lr = m_Player.Creator.HoldPoint.GetComponentInChildren<LineRenderer>();
    }

    private void OnEnable()
    {
        m_Player.Attack.OnEnterAttack += PutDownLilTosi;
        LevelManager.GetInstance().AddResetElement(this);

    }

    private void OnDisable()
    {
        m_Player.Attack.OnEnterAttack -= PutDownLilTosi; 
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    private void Update()
    {
        m_LilTosiToPick = GetOptimalLilTosiToPick();
        if (m_Player.Input.ThrowInputDown)
        {
            if (CanThrow() && !IsThrowing)
            {
                StartCoroutine(ThrowCoroutine());
            }
        }

        if (m_Player.Input.PickUpInput && !IsThrowing)
        {
            if (CanPickUp())
            {
                m_LilTosiToPick?.GetPickedUp();
                OnPickUp?.Invoke(m_LilTosiToPick);
            }
            else if (CanPutDown())
            {
                PutDownLilTosi();
            }
        }
    }

    private bool CanPutDown()
    {
        return IsHolding && m_Player.Fsm.IsWalking;
    }

    private bool CanPickUp()
    {
        return !IsHolding && !m_Player.Creator.IsCreating;
    }

    private LilTosi GetOptimalLilTosiToPick()
    {
        var l_NumFound = Physics.OverlapSphereNonAlloc(transform.position, m_PickUpRadius, m_Colliders, LayerMask.GetMask("Tipi", "Tense"));
        if (l_NumFound>0)
        {
            var l_LilTosisFound = new List<LilTosi>();
            for (int i = 0; i < l_NumFound; i++)
            {
                var l_LilTosi = m_Colliders[i].GetComponent<LilTosi>();
                if (l_LilTosi != null && l_LilTosi.CanBePickedUp())
                    l_LilTosisFound.Add(l_LilTosi);
            }

            if (l_LilTosisFound.Count > 0)
            {
                if (l_LilTosisFound.Count == 1)
                    return l_LilTosisFound[0];
                else
                    return GetClosestLilTosi(l_LilTosisFound);
            }
        }

        return null;
    }

    private LilTosi GetClosestLilTosi(List<LilTosi> lilTosisFound)
    {
        var l_ClosestInteractable = lilTosisFound[0];
        for (int  i = 1;  i < lilTosisFound.Count; i++)
        {
            var l_Checked = lilTosisFound[i];
            var l_DistToClosest = Vector3.Distance(transform.position, l_ClosestInteractable.transform.position);
            var l_DistToChecked = Vector3.Distance(transform.position, l_Checked.transform.position);

            if (l_DistToChecked < l_DistToClosest)
                l_ClosestInteractable = l_Checked;
        }
        return l_ClosestInteractable;
    }

    private IEnumerator ThrowCoroutine()
    {
        IsThrowing = true;
        m_Lr.enabled = true;
        m_Lr.material = HeldLilTosi.Type == LilTosi.TType.Tense ? m_TenseLineMat : m_TipiLineMat;
        m_ThrowPointRenderer.enabled = true;
        HeldLilTosi.IsPreparedToThrow = true;
        Vector3 l_ThrowVel = Vector3.zero;
        OnPrepareThrowEnter?.Invoke();
        while (!m_Player.Input.ThrowInputUp && CanThrow())
        {
            var l_Dir = m_Player.transform.forward;
            l_Dir.y = m_MainCam.transform.forward.y;
            l_ThrowVel = GetThrowVel(l_Dir.normalized, ThrowVelocity, ThrowAngle);
            RenderParabolicLine(l_ThrowVel);
            m_ThrowPointRenderer.transform.position = m_LandThrowPos;
            yield return null;
        }
        
        HeldLilTosi.IsPreparedToThrow = false;
        if (CanThrow())
            ThrowLilTosi(l_ThrowVel);
        // Hide the trajectory preview
        IsThrowing = false;
        m_ThrowPointRenderer.enabled = false;
        m_Lr.enabled = false;
        OnPrepareThrowExit?.Invoke();
        RuntimeManager.PlayOneShot(m_ThrowEventReference);
        RumbleManager.GetInstance().CreateOneShotPulse(0.5f, 0.5f, 0.2f);
    }

    private Vector3 GetThrowVel(Vector3 dir, float magnitude, float angle)
    {
        Vector3 l_ThrowVel;
        l_ThrowVel =
            UtilsMethods.RotateVectorInAxis(dir, UtilsMethods.RotAxis.Pitch, angle) * magnitude;
        return l_ThrowVel;
    }
    #region BeThrown
    private bool CanThrow()
    {
        return !m_Player.Creator.IsCreating && 
               (m_Player.Fsm.IsWalking || m_Player.Fsm.IsRolling)
               && IsHolding && !m_Player.Input.CancelThrowInput;
    }
    
    public void HoldLilTosi(LilTosi lilTosi)
    {
        if (HeldLilTosi != null)
            HeldLilTosi.transform.SetParent(null);
        HeldLilTosi = lilTosi;
        HeldLilTosi?.BeHeld(m_Player.Creator.HoldPoint);
    }
    
    private void ThrowLilTosi(Vector3 throwVelocity)
    {
        OnThrow?.Invoke(HeldLilTosi);
        HeldLilTosi.BeThrown(throwVelocity);
        HeldLilTosi = null;
    }

    private void PutDownLilTosi()
    {
        if (HeldLilTosi == null) return;
        
        var l_Dir = transform.forward;
        l_Dir.y = 0f;
        l_Dir.Normalize();
        var l_Vel = GetThrowVel(l_Dir, 1, 60f);
        l_Vel += Vector3.Project(m_Player.Controller.Velocity, l_Vel.normalized);
        RuntimeManager.PlayOneShot(m_PutDownReference);
        ThrowLilTosi(l_Vel);
    }
    public void DetachLilTosi(LilTosi lilTosi)
    {
        if (HeldLilTosi != null && HeldLilTosi == lilTosi)
        {
            HeldLilTosi.BeThrown(Vector3.zero);
            HeldLilTosi = null;
        }
    }
    
    private void RenderParabolicLine(Vector3 vel)
    {
        var l_Points = GetParabolicPoints(HeldLilTosi.transform.position + Vector3.up*1, 
            vel, m_MaxLinePoints, 
            m_MaxRenderTime, HeldLilTosi.Controller.Gravity);
        m_Lr.useWorldSpace = true;
        m_Lr.positionCount = GetUnblockedParabolicShootPointsNumber(l_Points);
        m_Lr.SetPositions(l_Points.ToArray());
    }
    
    
    private static List<Vector3> GetParabolicPoints(Vector3 initPos, Vector3 vel, int maxPoints, float maxTime, float gravity)
    {
        List<Vector3> l_Positions = new List<Vector3>();
        float l_SpeedX = vel.x;
        float l_SpeedY = vel.y;
        float l_SpeedZ = vel.z;
        float l_PositionY = initPos.y;
        for (int i = 0; i <= maxPoints; ++i)
        {
            float l_Time = (i / (float)maxPoints) * maxTime;
            float l_DeltaTime = maxTime / (float)maxPoints;
            Vector3 l_Position = new Vector3(initPos.x + l_SpeedX * l_Time, 
                l_PositionY, 
                initPos.z + l_SpeedZ * l_Time);
            l_Positions.Add(l_Position);
            l_PositionY += l_SpeedY * l_DeltaTime;
            l_SpeedY += gravity * l_DeltaTime;
        }
        return l_Positions;
    }
    private int GetUnblockedParabolicShootPointsNumber(List<Vector3> positions)
    {
        int l_Count = 0;
        for (int i = 0; i < positions.Count; i++)
        {
            l_Count++;
            Vector3 l_Point = positions[i];
            bool l_IsBlocked = Physics.OverlapSphere(l_Point, m_LandingRadius, m_CollisionLayer).Length > 0;
            if (l_IsBlocked)
            {
                m_LandThrowPos = l_Point;
                break;
            }
        }
        return l_Count;
    }
    #endregion
    
    private void OnDrawGizmos()
    {
        if (IsThrowing)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_LandThrowPos, m_LandingRadius);
        }
    }

    public void Restart()
    {
        if (HeldLilTosi!=null)
            HeldLilTosi.IsPreparedToThrow = false;
        StopAllCoroutines();
        DetachLilTosi(HeldLilTosi);
        m_Lr.enabled = false;
        m_ThrowPointRenderer.enabled = false;
        IsThrowing = false;
    }
}

