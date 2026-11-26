using UnityEngine;

[RequireComponent(typeof(PlayerJump))]
public class PlBallRunState : PlState
{
    public override PlayerMoveFSM.TStates Type => PlayerMoveFSM.TStates.BallRun;
    public override bool NormalizedInput => true;

    protected override bool CanEnterState()
    {
        // return m_PlayerCtrl.Input.RollInput && m_PlayerCtrl.IsGrounded;
        return false;
    }

    protected override bool CanExitState()
    {
        return m_PlayerCtrl.Input.RollInput;
    }
}