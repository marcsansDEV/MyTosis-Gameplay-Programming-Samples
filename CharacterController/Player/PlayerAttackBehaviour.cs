using UnityEngine;

public class PlayerAttackBehaviour : StateMachineBehaviour
{
    [Range(0.0f, 1.0f)]
    public float m_StartAttackPct;
    [Range(0.0f, 1.0f)]
    public float m_EndAttackPct;
    private Player m_Player;
    
     // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_Player == null)
            m_Player = animator.GetComponentInParent<Player>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool l_Active = stateInfo.normalizedTime>=m_StartAttackPct && stateInfo.normalizedTime<=m_EndAttackPct;
        m_Player.Attack.ActivateAttackTrigger(l_Active);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_Player.Attack.SetIsOnAttack(false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
