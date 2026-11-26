using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LilTosiPack : Singleton<LilTosiPack>
{
    private static List<LilTosi> m_LilTosis;
    private GameUIManager m_GameUIManager;
    private DynamicGameData m_DynamicGameData;
    private LilTosi m_LastSelectedLilTosi;
    protected override void  Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        m_LilTosis = GetComponentsInChildren<LilTosi>().ToList();
    }

    private void Start()
    {
        ShowLilTosis(false);
        m_GameUIManager = GameUIManager.GetInstance();
        m_DynamicGameData = GameManagerStateMachine.Instance.Context.DynamicData;
        m_LastSelectedLilTosi = GetLilTosi<LilTosi>(LilTosi.TType.Tipi);
    }
    

    private void ShowLilTosis(bool show)
    {
        foreach (LilTosi l_LilTosi in m_LilTosis)
        {
            l_LilTosi.SetExisting(show);
        }
    }

    public T GetLilTosi<T>(LilTosi.TType type) where T : LilTosi
    {
        foreach (var l_LilTosi in m_LilTosis)
        {
            if ((l_LilTosi.Type == type))
            {
                return l_LilTosi as T;
            }
        }

        Debug.LogError($"Could not return the type {type} requested.");
        return null;
    }
}
