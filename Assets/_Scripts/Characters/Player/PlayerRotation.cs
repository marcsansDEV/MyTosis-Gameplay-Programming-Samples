using UnityEngine;

public class PlayerRotation : BasePlayerComponent
{
    private Camera m_Camera;
    [SerializeField] private PlRotPolicy m_RotationPolicy = PlRotPolicy.LWYG;

    [Header("============= ROTATION RELATED PROPERTIES ================")]
    [Space(3)]
    [Range(0f, 1f)]
    [SerializeField] private float m_LerpRotation;

    private float m_BlockTime;
    private PlRotPolicy m_DesiredRot;

    public void SetDesiredRot(PlRotPolicy d) => m_DesiredRot = d;
    public enum PlRotPolicy
    {
        NONE, LWYG, LAC, LAI, LWYM
    }

    protected override void Awake()
    {
        base.Awake();
        m_Camera = Camera.main;
    }

    private void OnEnable()
    {
        m_Player.Controller.OnAfterMove += UpdateRotation;
        m_Player.Thrower.OnPrepareThrowEnter.AddListener(ThrowEnter);
        m_Player.Thrower.OnPrepareThrowExit.AddListener(ThrowExit);
        PossessionLilTosiAbility.OnStartPossession += StartedPos;
        PossessionLilTosiAbility.OnEndPossession += EndedPos;
    }
    private void OnDisable()
    {
        m_Player.Controller.OnAfterMove -= UpdateRotation;
        m_Player.Thrower.OnPrepareThrowEnter.RemoveListener(ThrowEnter);
        m_Player.Thrower.OnPrepareThrowExit.RemoveListener(ThrowExit);
        PossessionLilTosiAbility.OnStartPossession -= StartedPos;
        PossessionLilTosiAbility.OnEndPossession -= EndedPos;
    }

    public void SetPolicyDuringTime(PlRotPolicy plRotPolicy,float time)
    {
        SetPolicy(plRotPolicy);
        if (m_BlockTime < time)
            m_BlockTime = time;
    }
    
    private void StartedPos(LilTosi obj) => SetPolicy(PlRotPolicy.NONE);
    private void EndedPos(LilTosi obj) => SetPolicy(PlRotPolicy.LWYG);

    private void ThrowExit() => SetPolicy(PlRotPolicy.LWYG);
    private void ThrowEnter() => SetPolicy(PlRotPolicy.LAC);

    private void SetPolicy(PlRotPolicy type)
    {
        m_DesiredRot = type;
        if (m_BlockTime > 0)
            return;
        m_RotationPolicy = m_DesiredRot;
    }

    private void UpdateRotation()
    {
        // if (m_Player.Attack.IsOnAttack)
        //     return;
        Vector3 l_Dir;
        switch (m_RotationPolicy)
        {
            case PlRotPolicy.NONE:
                return;
            case PlRotPolicy.LWYG:
                l_Dir = m_Player.Input.Horizontal == Vector3.zero && m_BlockTime>0 ? Vector3.zero : m_Player.Controller.HorVelocity;
                break;
            case PlRotPolicy.LWYM:
                l_Dir = m_Player.Controller.HorVelocity;
                break;
            case PlRotPolicy.LAC:
                l_Dir = m_Camera.transform.forward;
                l_Dir.y = 0f;
                break;
            case PlRotPolicy.LAI:
                l_Dir = m_Player.Input.Horizontal;
                break;
            default:
                l_Dir = m_Player.Controller.HorVelocity.normalized;
                break;
        }

        if (l_Dir == Vector3.zero) return;
        Quaternion l_DesiredRotation = Quaternion.LookRotation(l_Dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, l_DesiredRotation, m_LerpRotation);
    }

    private void Update()
    {
        if (m_BlockTime > 0)
        {
            m_BlockTime -= m_Player.Controller.AdaptedDeltaTime;
            if (m_BlockTime <=0)
            {
                SetPolicy(m_DesiredRot);
            }
        }
    }

    public void LookTowards(Vector3 forward)
    {
        forward.y = 0f;
        forward.Normalize();
        transform.rotation = Quaternion.LookRotation(forward);
    }
}
