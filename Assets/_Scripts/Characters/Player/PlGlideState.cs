using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class PlGlideState : PlState
{
    private EventInstance m_GlideInst;
    
    [Header("========== OTHER PROPERTIES ==========")]
    [Space(5)]
    [SerializeField] private float m_MaxFallingSpeed;

    [SerializeField] private EventReference m_EventReference;
    
    public override bool NormalizedInput => false;
    public override PlayerMoveFSM.TStates Type => PlayerMoveFSM.TStates.Glide;

    protected override void Awake()
    {
        base.Awake();

        m_GlideInst = UtilsMethods.GetInstanceByGUID("{fe132e9d-0cf9-462a-be3e-8eace9e8f49d}");
    }

    public override void OnEnter()
    {
        base.OnEnter();
        var l_Vel = m_PlayerCtrl.Controller.Velocity;
        l_Vel.y *= 0.5f;
        m_PlayerCtrl.Controller.Velocity = l_Vel;
        RuntimeManager.PlayOneShot(m_EventReference);
        m_GlideInst.start();
        
        CinemachineCameraShaker.GetInstance().StartShake("GlideShake", 1, 0.1f);
        RumbleManager.GetInstance().StartRumblePulse(0.01f, 0.01f);
    }

    public override void OnExit()
    {
        base.OnExit();

        m_GlideInst.stop(STOP_MODE.ALLOWFADEOUT);
        CinemachineCameraShaker.GetInstance().EndShake("GlideShake");
        RumbleManager.GetInstance().EndRumblePulse();
    }

    protected override void OnBeforeMove()
    {
        base.OnBeforeMove();
        if (-m_PlayerCtrl.Controller.Velocity.y > m_MaxFallingSpeed)
        {
            var l_Vel = m_PlayerCtrl.Controller.Velocity;
            l_Vel.y = Mathf.MoveTowards(l_Vel.y, -m_MaxFallingSpeed, m_Acceleration * m_PlayerCtrl.Controller.AdaptedFixedDeltaTime);
            m_PlayerCtrl.Controller.Velocity = l_Vel;
        }
    }

    protected override bool CanEnterState()
    {
        return m_PlayerCtrl.Input.EnterGlideInput && !m_PlayerCtrl.Controller.IsGrounded &&
               !m_PlayerCtrl.Thrower.IsHolding && !m_PlayerCtrl.Creator.IsCreating;
    }

    protected override bool CanExitState()
    {
        return m_PlayerCtrl.Input.ExitGlideInput || m_PlayerCtrl.Controller.IsGrounded;
    }
}