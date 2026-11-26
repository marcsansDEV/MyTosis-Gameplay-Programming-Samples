using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
[CreateAssetMenu(menuName = "LilTosiAbilities/SlowTime", fileName = "SlowTimeAbility")]
public class SlowTimeLilTosiAbility : BaseLilTosiAbility
{
    private EventReference m_StartSlowRef;
    private EventReference m_EndSlowRef;
    
    public static Action<bool> OnSlowTimeChanged;

    private float m_FixedDeltaTime;
    private static bool m_TimeIsSlowed;
    [Header("Slow Time Settings")]
    [SerializeField] private float m_RecoverTime = 2f;
    [SerializeField] private float m_FadeInOutDuration = 1f;
    [SerializeField] private float m_SlowdownScale = 0.05f;

    private void Awake()
    {
        m_StartSlowRef = UtilsMethods.GetReferenceByGUID("{c66df0ba-dbaa-46ca-9cf1-6c1f24d3e128}");
        m_EndSlowRef = UtilsMethods.GetReferenceByGUID("{ac3fae9f-5f84-428d-8f73-996c3313f389}");
    }

    public static float AdaptedDeltaTime
    {
        get
        {
            if (GameManagerStateMachine.Instance.Context.Paused)
                return 0f;
            return m_TimeIsSlowed
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
        }
    }

    public static float AdaptedFixedDeltaTime
    {
        get
        {
            if (GameManagerStateMachine.Instance.Context.Paused)
                return 0f;
            return m_TimeIsSlowed
                ? Time.fixedUnscaledDeltaTime
                : Time.fixedDeltaTime;
        }
    }

    public override void Init(LilTosi lilTosi)
    {
        base.Init(lilTosi);
        m_FixedDeltaTime = Time.fixedDeltaTime;
        StartCoroutine(FillChargeWhenPossible());
    }

    private IEnumerator FillChargeWhenPossible()
    {
        while (true)
        {
            if (!Active && m_Charge < m_MaxCharge)
                m_Charge += Time.deltaTime * (m_MaxCharge / m_RecoverTime);
            yield return null;
        }
    }

    protected override void Activate()
    {
        StartCoroutine(StartSlowTime(1.0f, m_SlowdownScale, m_FadeInOutDuration));
        m_TimeIsSlowed = true;
        Icon = m_Icons[1];
    }

    protected override bool Canceled()
    {
        return base.Canceled() || !m_LilTosi.IsHeldByPlayer || m_Player.Input.Ability;
    }

    IEnumerator StartSlowTime(float start, float end, float time)
    {
        
        yield return null;
        RuntimeManager.PlayOneShot(m_StartSlowRef);
        OnSlowTimeChanged?.Invoke(true);
        float l_Timer = 0.0f;
        while (l_Timer < time)
        {
            Time.timeScale = Mathf.Lerp(start, end, l_Timer / time); 
            SetFixedDeltaTime();
            l_Timer += AdaptedDeltaTime;
            if (Canceled())
            {
                StartCoroutine(EndSlowTime(m_SlowdownScale, 1f, m_FadeInOutDuration));
                yield break;
            }
            yield return null;
        }

        Time.timeScale = end;
        SetFixedDeltaTime();
        StartCoroutine(SlowTime());
    }

    private void SetFixedDeltaTime()
    {
        Time.fixedDeltaTime = GameManagerStateMachine.Instance.Context.Paused
            ? m_FixedDeltaTime
            : m_FixedDeltaTime * Time.timeScale;
    }

    IEnumerator SlowTime()
    {
        yield return null;
        Time.timeScale = m_SlowdownScale;
        SetFixedDeltaTime();

        m_Charge = m_MaxCharge;
        while (m_Charge > 0)
        {
            m_Charge -= AdaptedDeltaTime;
            if (Canceled())
            {
                StartCoroutine(EndSlowTime(m_SlowdownScale, 1f, m_FadeInOutDuration));
                yield break;
            }
            yield return null;
        }
        StartCoroutine(EndSlowTime(m_SlowdownScale, 1f, m_FadeInOutDuration));
    }

    IEnumerator EndSlowTime(float start, float end, float time)
    {
        float l_Timer = 0.0f;
        m_Charge = 0f;
        RuntimeManager.PlayOneShot(m_EndSlowRef);
        while (l_Timer < time)
        {
            Time.timeScale = Mathf.Lerp(start, end, l_Timer / time);
            SetFixedDeltaTime();

            l_Timer += AdaptedDeltaTime;
            yield return null;
        }

        Time.timeScale = end;
        SetFixedDeltaTime();


        RestartTime();
    }
    void RestartTime()
    {
        Icon = m_Icons[0];
        Time.timeScale = 1f;
        SetFixedDeltaTime();
        m_TimeIsSlowed = false;
        OnSlowTimeChanged?.Invoke(false);
        m_Charge = 0f;
        Active = false;
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        StartCoroutine(FillChargeWhenPossible());
        RestartTime();
        m_TimeIsSlowed = false;
    }
}
