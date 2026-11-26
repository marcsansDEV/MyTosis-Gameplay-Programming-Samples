using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseLilTosiAbility : ScriptableObject
{
    protected LilTosi m_LilTosi;
    protected Player m_Player;

    [HideInInspector]
    public bool IsUnlocked = false;
    
    protected float m_Charge;
    [Header("======= BASE PROPERTIES ========")] 
    [FormerlySerializedAs("m_MaxTimeLiquid")]
    [SerializeField] protected float m_MaxCharge;
    [SerializeField] protected List<Sprite> m_Icons;
    [SerializeField] private Color m_Color;
    
    public bool Active { get; protected set; }
    public LilTosi LilTosi => m_LilTosi;
    public Sprite Icon { get; protected set; }
    public Color Color => m_Color;
    public float ChargeFraction => m_MaxCharge == 0? 1 : Mathf.Clamp01(m_Charge / m_MaxCharge);
    public virtual void Init(LilTosi lilTosi)
    {
        m_LilTosi = lilTosi;
        m_Player = Player.GetInstance();
        Icon = m_Icons[0];
        m_Charge = m_MaxCharge;
        if (m_LilTosi.IsUnlocked)
        {
            IsUnlocked = true;
        }
    }

    public void TryActivate()
    {
        if (CanActivate())
        {
            Active = true;
            ResetCooldown();
            Activate();
        }
    }

    public virtual bool CanActivate()
    {
        return IsUnlocked && !Active && m_Charge >= m_MaxCharge && m_LilTosi.IsExisting && !m_LilTosi.IsBeingAbsorbed;
    }

    protected abstract void Activate();

    protected virtual bool Canceled()
    {
        return !m_LilTosi.IsExisting;
    }

    protected void StartCoroutine(IEnumerator func) => m_LilTosi.StartCoroutine(func);
    protected void StopAllCoroutines() => m_LilTosi.StopAllCoroutines();
    protected void ResetCooldown()
    {
        m_Charge = m_MaxCharge;
    }

    public virtual void Reset()
    {
        Active = false;
    }
}
