using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class LilTosiCreator : MonoBehaviour, IRestartLevelElement
{
    private EventReference m_CreateRef;
    
    public UnityEvent<LilTosi> OnCreate;
    public UnityEvent OnAbsorb;
    public UnityEvent OnActivateAbility;

    private Player m_Player;
    private LilTosiPack m_LilTosiPack;
    private Transform m_HoldPoint;
    private Transform m_CreatePoint;
    [SerializeField] private float m_CreateDelayToMatchAnim;

    private bool NoTosisAlive => !TypeExists(LilTosi.TType.Tipi) && !TypeExists(LilTosi.TType.Tense);
    public bool IsCreating { get; private set; }
    public Transform HoldPoint => m_HoldPoint;

    private void Awake()
    {
        m_HoldPoint = UtilsMethods.ReturnRepeatChild(gameObject, "Hold Point").transform;
        m_CreatePoint = UtilsMethods.ReturnRepeatChild(gameObject, "Create Point").transform;
        m_Player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        LevelManager.GetInstance().AddResetElement(this);
    }

    private void OnDisable()
    {
        LevelManager.GetInstance().RemoveResetElement(this);
    }

    private void Start()
    {
        m_LilTosiPack = LilTosiPack.GetInstance();

        m_CreateRef = UtilsMethods.GetReferenceByGUID("{64d3333a-769c-44fe-9a86-694da944a3b0}");
    }

    private void ActivateAbility()
    {
        OnActivateAbility?.Invoke();
    }

    private void Update()
    {
        if (!IsCreating && !PossessionLilTosiAbility.IsActivated && !m_Player.Thrower.IsThrowing && m_Player.Fsm.IsWalking)
        {
            if (m_Player.Input.CreateLeftInput)
            {
                TryCreateTosi(LilTosi.TType.Tipi);
            }
            else if (m_Player.Input.CreateRightInput)
            {
                TryCreateTosi(LilTosi.TType.Tense);
            }
            else if (m_Player.Input.Ability)
            {
                ActivateAbility();
            }
        } 
    }

    #region Create

    private void TryCreateTosi(LilTosi.TType type)
    {
        IsCreating = true;
        if (TypeIsUnlocked(type))
        {
            OnAbsorb?.Invoke();
            StartCoroutine(CreateLilTosiDelayed(type));
        }
        else
            IsCreating = false;
    }

    private IEnumerator CreateLilTosiDelayed(LilTosi.TType type)
    {
        yield return new WaitUntil(() =>
        {
            return NoTosisAlive;
        });
        OnCreate?.Invoke(null);
        yield return new WaitForSeconds(m_CreateDelayToMatchAnim);
        CreateLilTosi(type);
        IsCreating = false;
    }

    public bool TypeExists(LilTosi.TType type)
    {
        return m_LilTosiPack.GetLilTosi<LilTosi>(type).IsExisting;
    }

    public bool TypeIsUnlocked(LilTosi.TType type)
    {
        return GameManagerStateMachine.Instance.Context.DynamicData.Gameplay.UnlockedLilTosis.Contains(type);
    }

    private void CreateLilTosi(LilTosi.TType type)
    {
        var l_LilTosiCreated = m_LilTosiPack.GetLilTosi<LilTosi>(type);
        l_LilTosiCreated.BeCreated(l_LilTosiCreated);
        l_LilTosiCreated.transform.parent = m_CreatePoint;
        RuntimeManager.PlayOneShot(m_CreateRef);
        m_Player.Thrower.HoldLilTosi(l_LilTosiCreated);
    }
    #endregion

    public void Restart()
    {
        StopAllCoroutines();
        IsCreating = false;
    }
}