using System;
using System.Collections;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "LilTosiAbilities/Explosion", fileName = "ExplosionAbility")]
public class ExplosionLilTosiAbility : BaseLilTosiAbility
{

    private EventReference m_ExplosionReference;
    
    public static Action<LilTosi> OnStartExplosion;
    
    [Header("========= EXPLOSION RELATED PROPERTIES =========")]
    [Space(5)]
    [SerializeField] private int m_Damage = 10;
    [SerializeField] private float m_ExplosionRadius = 10f;
    [SerializeField] private float m_PropulsionForce = 10f;
    [SerializeField] private float m_PlayerPropulsionForce = 0;
    [SerializeField] private float m_DelayAfterActivation = .3f;
    [SerializeField]   private GameObject m_DecalPrefab;
    TenseExplosionSphere m_ExplosionSphere;

    private void Awake()
    {
        m_ExplosionReference = UtilsMethods.GetReferenceByGUID("{2bdb9460-17b5-4dd0-9e1c-1c35f286b77f}");
    }

    protected override void Activate()
    {
        Icon = m_Icons[1];
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        OnStartExplosion?.Invoke(m_LilTosi);
        m_LilTosi.Controller.enabled = false;
        RuntimeManager.PlayOneShotAttached(m_ExplosionReference, m_LilTosi.gameObject);
        yield return new WaitForSeconds(m_DelayAfterActivation);
        m_ExplosionSphere = new TenseExplosionSphere(m_Damage, m_PropulsionForce, m_PlayerPropulsionForce, m_LilTosi.transform.position, m_ExplosionRadius);
        m_ExplosionSphere.CreateExplosion();
        InstantiateDecal();
        m_LilTosi.SetExisting(false);
        RumbleManager.GetInstance().CreateOneShotPulse(0.4f, 0.4f, 1.0f);
        
        Active = false;
        Icon = m_Icons[0];
    }

    private void InstantiateDecal()
    {
        var l_Decal = Instantiate(m_DecalPrefab, m_LilTosi.transform.position, quaternion.identity);
        l_Decal.transform.localScale = Vector3.one * m_ExplosionRadius * 2;
    }
}
public class TenseExplosionSphere
{
    public Collider[] CurrentCubes;
    Collider[] m_CubesInsideZone;

    TenseExplosionHit m_Hit;

    Ray m_Ray;

    int m_Damage;
    float m_ForceApplied;
    float m_SelfForceApplied;
    Vector3 m_Position;

    public float SphereCenterHight = 0.5f;

    float m_ExplosionRange;

    public TenseExplosionSphere(int damage, float forceApplied, float selfForceApplied, Vector3 point, float explosionRange)
    {
        m_Damage = damage;
        m_ForceApplied = forceApplied;
        m_SelfForceApplied = selfForceApplied;
        m_Position = point;

        m_ExplosionRange = explosionRange;
    }
    public void CreateExplosion()
    {
        m_CubesInsideZone = Physics.OverlapSphere(m_Position, m_ExplosionRange); // ~(1 << layer) would exclude a layer. A layer mask may not be needed, but it would reduce the foreach iterations

        foreach (var l_ObjectsAffectedByTense in m_CubesInsideZone) 
        {
            var l_Explode = l_ObjectsAffectedByTense.GetComponents<ITenseExplode>();
            if (l_Explode.Length>0) 
            {
                m_Hit = new TenseExplosionHit(m_Damage, m_ForceApplied, m_SelfForceApplied, m_Position);
                foreach (var l_ExplodeInstance in l_Explode)
                {
                    l_ExplodeInstance.HandleTenseExplosion(m_Hit);
                }
            }
        }
    }
}

public class TenseExplosionHit
{
    public int Damage { get; private set; }
    public float ForceApplied { get; private set; }
    public float SelfForceApplied { get; private set; }
    public Vector3 Point { get; private set; }
    public TenseExplosionHit(int damage, float forceApplied, float selfForceApplied, Vector3 point)
    {
        Damage = damage;
        ForceApplied = forceApplied;
        SelfForceApplied = selfForceApplied;
        Point = point;
    }
}

public interface ITenseExplode
{
    public void HandleTenseExplosion(TenseExplosionHit hit);
}
