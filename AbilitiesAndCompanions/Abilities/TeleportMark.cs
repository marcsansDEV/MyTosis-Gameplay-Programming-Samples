using System.Collections.Generic;
using UnityEngine;

public class TeleportMark : MonoBehaviour
{
    private SkinnedMeshRenderer m_PlayerRenderer;
    private SkinnedMeshRenderer m_TipiRenderer;

    private List<GameObject> m_AfterImages = new List<GameObject>();
            
    [SerializeField] private Material m_AppliedMaterial;
    
    private void Awake()
    {
        m_PlayerRenderer = Player.GetInstance().GetComponentInChildren<SkinnedMeshRenderer>();
        m_TipiRenderer = LilTosiPack.GetInstance().GetLilTosi<LilTosi>(LilTosi.TType.Tipi).GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    

    private void OnEnable()
    {
        GenerateAfterImageBody();
    }

    private void OnDisable()
    {
        Disappear();
    }

    public void Place(Transform pos)
    {
        transform.position = pos.position;
        transform.rotation = pos.rotation;
    }

    public void Disappear()
    {
        if (gameObject != null && !gameObject.Equals(null))
            Destroy(gameObject);
    }
    
    void GenerateAfterImageBody()
    {
        
        OneAfterImage(m_PlayerRenderer, true);
        OneAfterImage(m_TipiRenderer, false);
    }
    
    private void OneAfterImage(SkinnedMeshRenderer skinnedMeshRenderer, bool isPlayer)
    {
        GameObject l_Obj = new GameObject(nameof(skinnedMeshRenderer));

        l_Obj.transform.SetParent(this.transform);
        l_Obj.transform.localPosition = isPlayer ? Vector3.zero : Vector3.forward * 1 + Vector3.up * .5f;
        l_Obj.transform.rotation = transform.rotation;
            
        var l_MeshRenderer = l_Obj.AddComponent<MeshRenderer>();
        MeshFilter l_MeshFilter = l_Obj.AddComponent<MeshFilter>();

        Mesh l_Mesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(l_Mesh);
        
        l_MeshFilter.mesh = l_Mesh;
        l_MeshRenderer.material = m_AppliedMaterial;
        m_AfterImages.Add(l_Obj);
    }
}