using System.Collections.Generic;
using System.Linq;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public static class UtilsMethods
{
    private static GameObject m_Descendant = null;
    public static bool ContainsLayer(LayerMask mask, int layer)
    {
         return (mask.value & 1 << layer) > 0;
    }

    public static float HeightToForce(float height, float gravity)
    {
        return Mathf.Sqrt(height * -2 * gravity);
    }

    public static void ResetAllAnimatorTriggers(Animator animator)
    {
        foreach (var l_Trigger in animator.parameters)
        {
            if (l_Trigger.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(l_Trigger.name);
        }
    }
    
    public static Vector3 GetDirection(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }
    
    public static Vector3 GetInitialThrowVelocity(Vector3 posInitial, Vector3 posFinal, float angle, float gravity)
    {
        Vector3 l_DeltaPos = posFinal - posInitial;
        Vector3 l_XZDelta = l_DeltaPos;
        l_XZDelta.y = 0f;
        Vector3 l_ThrowDir = Quaternion.LookRotation(l_XZDelta) * Quaternion.AngleAxis(-angle, Vector3.right) * Vector3.forward;

        float l_Time = Mathf.Sqrt((l_ThrowDir.y * l_DeltaPos.x / l_ThrowDir.x - l_DeltaPos.y) / -gravity * 2);
        float l_Speed = l_DeltaPos.x / l_ThrowDir.x / l_Time;

        if (float.IsNaN(l_Speed))
        {
            Debug.Log("Impossible Trajectory");
            return Vector3.zero;
        }
        return l_Speed * l_ThrowDir;
    }
    
    public static GameObject ReturnRepeatChild(GameObject parent, string childName)
    {
        foreach (Transform l_Child in parent.transform)
        {
            if (l_Child.name == childName)
            {
                m_Descendant = l_Child.gameObject;
                break;
            }
            else
            {
                ReturnRepeatChild(l_Child.gameObject, childName);
            }
        }
        return m_Descendant;
    }

    public static T[] ReturnRepeatChildren<T>(GameObject parent, string childName) where T: Component
    {
        return parent.GetComponentsInChildren<T>().Where(t => t.name == childName).ToArray();
    }

    public static GameObject ReturnRepeatParent(GameObject child, string parentName)
    {
        GameObject l_Parent = child.transform.parent.gameObject;
        if (l_Parent.name == parentName) { return l_Parent; }
        else { return ReturnRepeatParent(l_Parent, parentName); }
    }
    
    public static Vector3 GetMiddlePosition(Vector3 posA, Vector3 posB)
    {
        Vector3 l_MiddlePos = posB - posA;
        l_MiddlePos = posA + l_MiddlePos / 2.0f;
        return l_MiddlePos;
    }
 
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
    
    public enum RotAxis {Pitch, Yaw, Roll}
    public static Vector3 RotateVectorInAxis(Vector3 initVector, RotAxis rotDir, float angle)
    {
        Vector3 l_Rhs = rotDir switch
        {
            RotAxis.Pitch => Vector3.up,
            RotAxis.Yaw => Vector3.right,
            RotAxis.Roll => Vector3.forward,
            _ => Vector3.up
        };
        var l_Axis = Vector3.Cross(initVector, l_Rhs);
        if (l_Axis == Vector3.zero) l_Axis = Vector3.right;
        return Quaternion.AngleAxis(angle, l_Axis) * initVector;
    }

    public static EventInstance GetInstanceByGUID(string GUID)
    {
        return RuntimeManager.CreateInstance(GetReferenceByGUID(GUID));
    }
    
    public static EventReference GetReferenceByGUID(string GUID)
    {
        EventReference l_Reference = new EventReference();
        l_Reference.Guid = GetGUID(GUID);
        return l_Reference;
    }
    
    public static FMOD.GUID GetGUID(string GUID)
    {
        return new FMOD.GUID(new System.Guid(GUID));
    }
}
