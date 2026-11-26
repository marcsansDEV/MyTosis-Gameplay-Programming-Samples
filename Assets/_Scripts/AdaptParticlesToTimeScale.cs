using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdaptParticlesToTimeScale : MonoBehaviour, IPaused
{
    private Dictionary<ParticleSystem, bool> m_AnimatorsDictionary = new();
    private bool m_TimeIsSlowed;
    private bool m_IsPaused;
    private bool ShouldGoUnscale => !m_IsPaused && m_TimeIsSlowed;
    void Start()
    {
        var l_Animators = GetComponentsInChildren<ParticleSystem>().ToList();
        foreach (var l_Animator in l_Animators)
            m_AnimatorsDictionary.Add(l_Animator, l_Animator.main.useUnscaledTime);
    }
    private void OnEnable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged += SlowTimeChanged;
    }

    private void SlowTimeChanged(bool obj)
    {
        m_TimeIsSlowed = obj;
        UpdateAnimatorsUpdateMode();
    }

    private void UpdateAnimatorsUpdateMode()
    {
        foreach (var l_Pair in m_AnimatorsDictionary)
        {
            var l_Main = l_Pair.Key.main;
            l_Main.useUnscaledTime = ShouldGoUnscale || l_Pair.Value;
        }
    }

    private void OnDisable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged -= SlowTimeChanged;
    }

    public void OnPause(bool pauseState)
    {
        m_IsPaused = pauseState;
        UpdateAnimatorsUpdateMode();
    }
}
