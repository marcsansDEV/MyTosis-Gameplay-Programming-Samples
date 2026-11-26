using System;
using System.Collections;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "LilTosiAbilities/Teleport", fileName = "TeleportAbility")]

public class TeleportLilTosiAbility : BaseLilTosiAbility
{
    private EventReference m_MarkRef;
    private EventReference m_TPRef;
    
    public static Action<LilTosi> OnPutMark;
    public static Action<LilTosi> OnTeleport;
    
    private TeleportMark m_TeleportMark;
    [Header("================ TELEPORT RELATED PROPERTIES ================")] 
    [SerializeField] private GameObject m_TeleportMarkPrefab;
    [SerializeField] private float m_InDelay;
    [SerializeField] private float m_OutDelay;

    private void Awake()
    {
        m_MarkRef = UtilsMethods.GetReferenceByGUID("{17c36def-72fc-4269-98b6-a4bff4a1232c}");
        m_TPRef = UtilsMethods.GetReferenceByGUID("{9398efeb-2c64-4294-a67b-fe4f84874010}");
    }

    protected override void Activate()
    {
        m_TeleportMark = Instantiate(m_TeleportMarkPrefab).GetComponent<TeleportMark>();
        RuntimeManager.PlayOneShot(m_MarkRef);
        OnPutMark?.Invoke(m_LilTosi);
        m_TeleportMark.Place(m_Player.transform);
        Icon = m_Icons[1];
        StartCoroutine(WaitUntilActivation());
    }

    private IEnumerator WaitUntilActivation()
    {
        yield return null;
        yield return new WaitUntil(() => m_Player.Input.Ability || Canceled());
        if (!Canceled())
        {  
            m_Player.Controller.AddImpulse(Vector3.zero,true,true);
            m_Player.Controller.ForceLock(true);
            // m_Player.Controller.enabled = false;
            OnTeleport?.Invoke(m_LilTosi);
            yield return new WaitForSeconds(m_InDelay);
            m_Player.transform.position = m_TeleportMark.transform.position;
            m_Player.transform.rotation = m_TeleportMark.transform.rotation;
            yield return new WaitForSeconds(m_OutDelay);
            // m_Player.Controller.enabled = true;
            m_Player.Controller.ForceLock(false);
        }
        
        RuntimeManager.PlayOneShot(m_TPRef);
        m_TeleportMark.enabled = false;
        m_TeleportMark = null;
        Active = false;
        Icon = m_Icons[0];
    }

    protected override bool Canceled()
    {
        return base.Canceled() || !m_LilTosi.IsHeldByPlayer;
    }

    public override void Reset()
    {
        base.Reset();
        Icon = m_Icons[0];
        if (m_TeleportMark != null)
            m_TeleportMark.enabled = false;
        StopAllCoroutines();
    }
}