using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTimeTrailLilTosiAbility : MonoBehaviour
{
    [SerializeField] private float m_ActiveTime = 2.0f;

    [Header("Mesh")]
    [SerializeField] private float m_MeshRefreshRate = 0.1f;
    [SerializeField] private float m_MeshDestroyDelay = 1f;
    [SerializeField] private Transform m_SpawnPos;
    [Header("Shader")]
    [SerializeField] private Material m_Mat;
    [SerializeField] private string m_ShaderVarRef;
    [SerializeField] private float m_ShadeVarRate = 0.1f;
    [SerializeField] private float m_ShaderVarRefreshRate = 0.05f;

    private bool m_IsTrailActive;
    [SerializeField] private SkinnedMeshRenderer[] m_SkinnedMeshRenderers;

    private Player m_Player;

    private void Awake()
    {
        m_IsTrailActive = false;
        m_Player = Player.GetInstance();
    }

    private void OnEnable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged += OnSlowTimeChanged;
    }

    private void OnDisable()     
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged -= OnSlowTimeChanged;
    }
    
    private void OnSlowTimeChanged(bool isActive)
    {
        m_IsTrailActive = isActive;
        StartCoroutine(ActivateSkinnedTrail());
    }

    IEnumerator ActivateSkinnedTrail()
    {
        while (m_IsTrailActive)
        {
            if (CanSpawnTrail())
            {
                m_ActiveTime -= m_MeshRefreshRate;
                if (m_SkinnedMeshRenderers==null)
                {
                    m_SkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                }
            
                for (int i = 0; i < m_SkinnedMeshRenderers.Length; i++)
                {
                    GameObject l_GO = new GameObject();
                    l_GO.transform.SetPositionAndRotation(transform.position - transform.forward * .5f, transform.rotation);

                    MeshRenderer l_MR = l_GO.AddComponent<MeshRenderer>();
                    MeshFilter l_MF = l_GO.AddComponent<MeshFilter>();

                    Mesh l_Mesh = new Mesh();
                    m_SkinnedMeshRenderers[i].BakeMesh(l_Mesh);

                    l_MF.mesh = l_Mesh;
                    l_MR.material = m_Mat;
                    StartCoroutine(AnimateMaterialFloat(l_MR.material, 0, m_ShadeVarRate, m_ShaderVarRefreshRate));

                    Destroy(l_GO, m_MeshDestroyDelay);
                }
                yield return new WaitForSecondsRealtime(m_MeshRefreshRate);
            }
            yield return null;
        }
    }

    private bool CanSpawnTrail()
    {
        return m_IsTrailActive && (m_Player.Controller.HorVelocity.magnitude > 0.2f ||  Mathf.Abs(m_Player.Controller.Velocity.y) > 1)&& NotPaused();
    }

    IEnumerator AnimateMaterialFloat(Material mat, int goal, float rate, float refreshRate)
    {
        float l_ValueToAnimate = m_Mat.GetFloat(m_ShaderVarRef);
        while (l_ValueToAnimate>goal)
        {
            yield return new WaitUntil(NotPaused);
            l_ValueToAnimate -= rate;
            mat.SetFloat(m_ShaderVarRef, l_ValueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private bool NotPaused() => !GameManagerStateMachine.Instance.Context.Paused;

}


