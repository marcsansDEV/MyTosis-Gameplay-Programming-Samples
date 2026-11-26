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
    

    private void ShowLilTosis(bool _Show)
    {
        foreach (LilTosi lilTis in m_LilTosis)
        {
            lilTis.SetExisting(_Show);
        }
    }

    public T GetLilTosi<T>(LilTosi.TType _Type) where T : LilTosi
    {
        foreach (var lilTosi in m_LilTosis)
        {
            if ((lilTosi.Type == _Type))
            {
                return lilTosi as T;
            }
        }

        Debug.LogError(("Could not return the type {0} requested.", _Type.ToString()));
        return null;
    }
}
