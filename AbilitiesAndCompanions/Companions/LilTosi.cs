using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public abstract class LilTosi : MonoBehaviour, IRestartLevelElement
{
    public BaseLilTosiAbility ReleasedAbility { get; private set;}
    public BaseLilTosiAbility HeldAbility { get; private set;}
    [SerializeField] private BaseLilTosiAbility m_HeldAbilityRef;
    [SerializeField] private BaseLilTosiAbility m_ReleasedAbilityRef;
    [SerializeField] private float m_GetAbsorvedDelay;
    private bool m_Possessed;

    
    public enum TType
    {
        Tipi = 0,
        Tense,
    }
 

    protected Player m_Player;
    protected GameObject m_Mesh;
    private bool m_IsGettingAbsorved;

    [SerializeField] private Sprite[] m_AbilityIcons = new Sprite[2];

    public abstract TType Type { get; }
    public bool IsPreparedToThrow { get; set; }
    public bool IsHeldByPlayer { get; protected set; }
    public bool IsExisting { get; protected set; }

    public Action OnThrown;
    public Action OnCreated;
    public Action OnAbsorbed;

    [SerializeField] private UnityEvent m_OnAbsorbed;
    [SerializeField] private UnityEvent m_OnCreated;
    protected virtual void Awake()
    {
        m_Player = Player.GetInstance();
        m_Mesh = UtilsMethods.ReturnRepeatChild(gameObject, "Renderer");
        Controller = GetComponent<EntityController>();
    }


    private void Start()
    {
        InitAbilities();
    }
    

    public EntityController Controller { get; private set; }

    public bool IsUnlocked =>
        GameManagerStateMachine.Instance.Context.DynamicData.Gameplay.UnlockedLilTosis.Contains(Type);

    public bool IsBeingAbsorbed => m_IsGettingAbsorved;


    private void InitAbilities()
    {
        if (m_ReleasedAbilityRef != null)
        {
            ReleasedAbility = ScriptableObject.Instantiate(m_ReleasedAbilityRef);
            ReleasedAbility.Init(this);
        }

        if (m_HeldAbilityRef != null)
        {
            HeldAbility = ScriptableObject.Instantiate(m_HeldAbilityRef);
            HeldAbility.Init(this);
        }
    }

    private void OnEnable()
    {
        m_Player.Creator.OnActivateAbility.AddListener(ActivateAbility);
        m_Player.Creator.OnCreate.AddListener(BeCreated);
        m_Player.Creator.OnAbsorb.AddListener(GetAbsorbedEvent);
        PossessionLilTosiAbility.OnStartPossession += EnterPossessionState;
        PossessionLilTosiAbility.OnEndPossession += ExitPossessionState;
        Controller.OnLanded += StopVel;
        
        LevelManager.GetInstance().AddResetElement(this);
    }

    private void GetAbsorbedEvent()
    {
        GetAbsorbed(m_GetAbsorvedDelay);
    }

    private void OnDisable()
    {
        m_Player.Creator.OnActivateAbility.RemoveListener(ActivateAbility);
        m_Player.Creator.OnCreate.RemoveListener(BeCreated);
        m_Player.Creator.OnAbsorb.RemoveListener(GetAbsorbedEvent);
        PossessionLilTosiAbility.OnStartPossession -= EnterPossessionState;
        PossessionLilTosiAbility.OnEndPossession -= ExitPossessionState;
        Controller.OnLanded -= StopVel;
        
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    private void ExitPossessionState(LilTosi obj)
    {
        if (obj == this)
        {
            m_Possessed = false;
        }
    }

    private void EnterPossessionState(LilTosi obj)
    {
        if (obj == this)
        {
            m_Possessed = true;
        }
    }

    private void StopVel()
    {
        if (!m_Possessed)
        {
            Controller.AddImpulse(Vector3.zero, true, false);
        }
    }


    private void ActivateAbility()
    {
        if (m_IsGettingAbsorved || !IsExisting)
            return;
        if (IsHeldByPlayer)
        {
            HeldAbility?.TryActivate();
        }
        else
        {
            ReleasedAbility?.TryActivate();
        }
    }

    public void SetExisting(bool show)
    {
        GetComponentInChildren<MeshHider>().SetActiveMesh(show);
        // m_Mesh.gameObject.SetActive(show);

        IsExisting = show;
        Controller.AddImpulse(Vector3.zero, true, true);
        Controller.enabled = show;
        GameUIManager.GetInstance().TrySetAbilityToContainer(IsExisting? this : null);
    }

    public virtual void BeCreated(LilTosi lilTosi)
    {
        if (lilTosi != this) return;
        IsExisting = true;
        SetExisting(true);
        StopVel();
        OnCreated?.Invoke();
        m_OnCreated?.Invoke();
    }

    public virtual void BeHeld(Transform parent)
    {
        Controller.enabled = false;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        IsHeldByPlayer = parent == m_Player.Creator.HoldPoint;
        Controller.UseGravity = false;
    }
    

    public virtual void BeThrown(Vector3 throwVelocity, bool invokeAction  = true)
    {
        if (invokeAction) OnThrown?.Invoke();
        Controller.enabled = true;
        transform.SetParent(null); //maybe fer que parent sigui liltosipack
        IsHeldByPlayer = false;
        Controller.UseGravity = true;
        Controller.AddImpulse(throwVelocity, true, true);
        Controller.IsGrounded = false;
    }
    

    public virtual void GetPickedUp()
    {
        m_Player.Thrower.HoldLilTosi(this);
    }
    public virtual bool CanBePickedUp()
    {
        return !IsHeldByPlayer && IsExisting && !IsUsingAbility();
    }

    public bool IsUsingAbility()
    {
        return (HeldAbility != null && HeldAbility.Active) || (ReleasedAbility != null && ReleasedAbility.Active);
    }

    
    public void GetAbsorbed(float l_Time = -1)
    {
        if (!IsExisting) return;

        if (l_Time == -1)
        {
            l_Time = m_GetAbsorvedDelay;
        }
        
        // Debug.Log("Get Absorbed: " + name);
        
        StopAllCoroutines();
        HeldAbility.Reset();
        ReleasedAbility.Reset();
        StartCoroutine(GetAbsorbedCoroutine(l_Time));
    }

    private IEnumerator GetAbsorbedCoroutine(float m_waitTime)
    {
        m_IsGettingAbsorved = true;
        yield return new WaitForSeconds(m_waitTime);
        //PLAY PARTICLES HERE
        BeAbsorbed();
        m_IsGettingAbsorved = false;
    }

    protected virtual void BeAbsorbed()
    {
        // Debug.Log("Be Absorbed: " + name);
        BeThrown(Vector3.zero, false);
        SetExisting(false);
        OnAbsorbed?.Invoke();
        m_OnAbsorbed?.Invoke();
    }

    public void Restart()
    {
        BeThrown(Vector3.zero);
        SetExisting(false);
        HeldAbility.Reset();
        ReleasedAbility.Reset();
    }

    public BaseLilTosiAbility GetActiveAbility()
    {
        return IsHeldByPlayer ? HeldAbility : ReleasedAbility;
    }

    private void LateUpdate()
    {
        if (Controller.IsGrounded && !IsHeldByPlayer)
        {
            StopVel();
        }
    }
}

