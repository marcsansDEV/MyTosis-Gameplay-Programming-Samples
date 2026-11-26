using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class AdaptBrainToTimeScale : MonoBehaviour, IPaused
{
    private CinemachineBrain m_Brain;
    [SerializeField] private CinemachineBrain.UpdateMethod m_UpdateMethodInSlowTime;
    [SerializeField] private CinemachineBrain.BrainUpdateMethod m_BlendUpdateMethodInSlowTime;

    private CinemachineBrain.UpdateMethod m_DefaultUpdateMethod;
    private CinemachineBrain.BrainUpdateMethod m_DefaultBlendUpdateMethod;
    private void Awake()
    {
        m_Brain = GetComponent<CinemachineBrain>();
        m_DefaultUpdateMethod = m_Brain.m_UpdateMethod;
        m_DefaultBlendUpdateMethod = m_Brain.m_BlendUpdateMethod;

    }

    private void OnEnable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged += SlowTimeChanged;
    }
    
    private void OnDisable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged -= SlowTimeChanged;
    }

    private void Update()
    {
        if (m_Brain.m_UpdateMethod == CinemachineBrain.UpdateMethod.ManualUpdate)
        {
            
        }
    }

    private void SlowTimeChanged(bool timeIsSlowed)
    {
        m_Brain.m_UpdateMethod = timeIsSlowed
            ? m_UpdateMethodInSlowTime
            : m_DefaultUpdateMethod;
        m_Brain.m_BlendUpdateMethod = timeIsSlowed
            ? m_BlendUpdateMethodInSlowTime
            : m_DefaultBlendUpdateMethod;
    }

    public void OnPause(bool pauseState)
    {
        m_Brain.m_IgnoreTimeScale = !pauseState;
    }
}
