using System;
using FMODUnity;
using UnityEngine;

public class LilTosiHealth : MonoBehaviour, ITosiHealthSystem
{
    
    private EventReference m_DeathReference;
    private LilTosi m_LilTosi;
    public Action OnDead;

    private void Awake()
    {
        m_LilTosi = GetComponent<LilTosi>();

        m_DeathReference = UtilsMethods.GetReferenceByGUID("{1fbd355f-a559-4dfb-9ef5-6bbdd6ee6eac}");
    }

    public void GetDamaged(int damage, Vector3 source)
    {
        RuntimeManager.PlayOneShotAttached(m_DeathReference, gameObject);
        m_LilTosi.GetAbsorbed(0);
        m_LilTosi.Controller.enabled = false;
    }

    public bool CanGetDamaged()
    {
        return true;
    }
}