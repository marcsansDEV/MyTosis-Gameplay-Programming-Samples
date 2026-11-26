using UnityEngine;

public class Player : Singleton<Player>, IRestartLevelElement
{
    public EntityController Controller{ get; private set; }
    public PlayerInputManager Input { get; private set; }
    public PlayerMoveFSM Fsm { get; private set; }
    public PlayerJump Jump { get; private set; }
    public LilTosiThrower Thrower { get; private set; }
    public LilTosiCreator Creator { get; private set; }
    public PlayerAttack Attack { get; private set; }
    public PlayerHealth HealthSystem { get; private set; }
    public PlayerRotation PlayerRotation { get; private set; }
    public PlayerAnimation Animation { get; private set; }

    private MeshHider m_MeshHider;
    private GameObject m_Projector;

    private void OnEnable()
    {
        LevelManager.GetInstance().AddResetElement(this);

    }

    private void OnDisable()
    {
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    protected override void Awake()
    {
        base.Awake();

        m_MeshHider = GetComponentInChildren<MeshHider>();
        m_Projector = UtilsMethods.ReturnRepeatChild(gameObject, "Projector");
        
        Input = GetComponent<PlayerInputManager>();
        Controller = GetComponent<EntityController>();
        Fsm = GetComponent<PlayerMoveFSM>();
        Thrower = GetComponent<LilTosiThrower>();
        Creator = GetComponent<LilTosiCreator>();
        Jump = GetComponent<PlayerJump>();
        Attack = GetComponent<PlayerAttack>();
        HealthSystem = GetComponent<PlayerHealth>();
        PlayerRotation = GetComponent<PlayerRotation>();
        Animation = GetComponent<PlayerAnimation>();
    }
    public void Restart()
    {
        transform.SetParent(null);
        Input.enabled = false;
        Creator.enabled = false;
        Controller.enabled = false;
        m_MeshHider.SetActiveMesh(false);
        m_Projector.SetActive(false);
        Attack.ActivateAttackTrigger(false);
        Animation.ResetAnimator();
        TimerManager.GetInstance().AddTimer
            (
                GameManagerStateMachine.Instance.Context.StaticData.Player.RespawnTime + 0.01f,
                () =>
                {
                    Input.enabled = true; 
                    Creator.enabled = true; 
                    Controller.enabled = true;
                    m_MeshHider.SetActiveMesh(true);
                    m_Projector.SetActive(true);
                    Attack.SetIsOnAttack(false);
                },
                "EnablingPlayer"
            );
    }
}