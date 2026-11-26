using System;
using UnityEngine;
using UnityEngine.Events;

public class ExternalTriggerCall : MonoBehaviour
{
    [SerializeField] private UnityEvent m_OnEnterEvent;
    [SerializeField] private UnityEvent m_OnExitEvent;
    [SerializeField] private UnityEvent m_OnStayEvent;
    
    [SerializeField] private LayerMask m_DetectableMask;

    private Collider m_Col;
    
    private bool m_IsStaying = false;
    
    public Collider Collider { get; private set;}
    
    public Action<Collider> OnEnterTrigger;
    public Action<Collider> OnExitTrigger;
    public Action<Collider> OnStayTriggerUpdate;

    private void Awake()
    {
        Collider = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        if (m_IsStaying)
        {
            OnStayTriggerUpdate?.Invoke(m_Col);
            m_OnStayEvent?.Invoke();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (UtilsMethods.ContainsLayer(m_DetectableMask, other.gameObject.layer))
        {
            m_Col = other;
            m_IsStaying = true;
            OnEnterTrigger?.Invoke(other);
            m_OnEnterEvent?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (UtilsMethods.ContainsLayer(m_DetectableMask, other.gameObject.layer))
        {
            m_Col = null;
            m_IsStaying = false;
            OnExitTrigger?.Invoke(other);
            m_OnExitEvent?.Invoke();
        }
    }

    
}
