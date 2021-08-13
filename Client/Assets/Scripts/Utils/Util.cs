using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
		if (component == null)
            component = go.AddComponent<T>();
        return component;
	}

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        
        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
		}
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
    static float GetAtan(Vector2 startPos, Vector2 targetPos)
    {
        Vector2 direction = targetPos - startPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    public static Quaternion RotateDir2D(Vector2 startPos, Vector2 targetPos)
    {
        float angle = GetAtan(startPos, targetPos);
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public static Quaternion LookAt2D(Vector2 startPos, Vector2 targetPos, FacingDirection facing = FacingDirection.RIGHT)
    {

        float angle = GetAtan(targetPos, startPos);
        angle -= (float)facing;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

}
