using System;
using UnityEngine;

public class PlayerAnimation : BasePlayerComponent
{
    private Animator m_Animator;

    private float m_LastFallingVelocity;
    protected override void Awake()
    {
        base.Awake();
        m_Animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        m_Player.Attack.OnEnterAttack += PlayAttackAnim;
        m_Player.Controller.OnLanded += PlayerLandAnim;
        m_Player.Jump.OnJump += PlayerJumpAnim;
        m_Player.Creator.OnCreate.AddListener(PlayCreateAnim);
        m_Player.Thrower.OnThrow += PlayThrowAnim;
        m_Player.HealthSystem.OnHit += OnGetHit;
        Bouncer.OnBounce += Bounce;
    }
    
    private void OnDisable()
    {
        m_Player.Attack.OnEnterAttack -= PlayAttackAnim;
        m_Player.Controller.OnLanded -= PlayerLandAnim;
        m_Player.Jump.OnJump -= PlayerJumpAnim;
        m_Player.Creator.OnCreate.RemoveListener(PlayCreateAnim);
        m_Player.Thrower.OnThrow -= PlayThrowAnim;
        m_Player.HealthSystem.OnHit -= OnGetHit;
        Bouncer.OnBounce -= Bounce;
    }

    private void Bounce(EntityController obj)
    {
        if (obj == m_Player.Controller)
        {
            PlayerLandAnim();
            Invoke("PlayerJumpAnim", 0.1f);
        }
    }

    private void OnGetHit()
    {
        m_Animator.SetTrigger("OnGetHit");
    }

    private void PlayThrowAnim(LilTosi obj)
    {
        m_Animator.SetTrigger("OnThrowed");
    }

    private void PlayCreateAnim(LilTosi arg0)
    {
        m_Animator.SetTrigger("OnCreated");
    }

    private void PlayerJumpAnim()
    {
        m_Animator.SetTrigger("OnJump");
    }

    private void PlayerLandAnim()
    {
        m_Animator.SetTrigger("OnLanded");
    }

    private void PlayAttackAnim()
    {
        m_Animator.SetTrigger("OnAttack");
    }

    private void LateUpdate()
    {
        UpdateSpeedH();
        UpdateSpeedV();
        UpdateIsGrounded();
        UpdateIsHolding();
        UpdateIsGliding();
        UpdateIsPossessing();
    }

    private void UpdateIsPossessing()
    {
        m_Animator.SetBool("IsPossessing", PossessionLilTosiAbility.IsActivated);
    }

    private void UpdateIsGliding()
    {
        m_Animator.SetBool("IsGliding", m_Player.Fsm.IsGliding);
    }

    private void UpdateIsHolding()
    {
        m_Animator.SetBool("IsHolding", m_Player.Thrower.IsHolding && !m_Player.Creator.IsCreating);
    }

    private void UpdateIsGrounded()
    {
        m_Animator.SetBool("IsGrounded", m_Player.Controller.IsGrounded);
    }

    private void UpdateSpeedV()
    {
        if (!m_Player.Controller.IsGrounded)
            m_LastFallingVelocity = m_Player.Controller.Velocity.y;
        if (m_LastFallingVelocity < 0)
            m_Animator.SetFloat("SpeedVUnrealistic", m_LastFallingVelocity);
        m_Animator.SetFloat("SpeedV", m_Player.Controller.Velocity.y);
    }

    private void UpdateSpeedH()
    {
        var l_Fraction = m_Player.Controller.HorVelocity.magnitude / m_Player.Fsm.MaxSpeed;
        m_Animator.SetFloat("SpeedH", Mathf.Clamp01(l_Fraction));
    }

    public void ResetAnimator()
    {
        m_Animator.Play("Idle_Walk_Run Blend Tree", -1, 0);
    }
}
