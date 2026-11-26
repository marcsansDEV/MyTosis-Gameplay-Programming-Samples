using UnityEngine;

public abstract class VolumeController : MonoBehaviour
{
    protected Vector3 m_InactiveLocalPos;
    protected Vector3 m_ActiveLocalPos = Vector3.zero;
    protected Vector3 m_DesiredLocalPos;
    [SerializeField] private float m_LerpValue = .05f;

    private void Awake()
    {
        m_InactiveLocalPos = new Vector3(0, 2f, 0);
        Deactivate();
    }

    protected void Activate() => m_DesiredLocalPos = m_ActiveLocalPos;
    protected void Deactivate() => m_DesiredLocalPos = m_InactiveLocalPos;
    private void Update()
    {
        if (transform.localPosition != m_DesiredLocalPos)
        {
            m_LerpValue = 0.05f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, m_DesiredLocalPos, m_LerpValue);
        }
    }
}