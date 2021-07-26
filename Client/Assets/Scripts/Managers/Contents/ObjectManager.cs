
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	//Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	List<GameObject> _objects = new List<GameObject>();

	public void Add(GameObject go)
	{
		_objects.Add(go);
	}



	public void Remove(GameObject go)
	{
		if (go == null)
			return;

		_objects.Remove(go);
		Managers.Resource.Destroy(go);

	}


	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.Pos == cellPos)
				return obj;
		}

		return null;
	}


	public PlayerController Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects)
		{
			if (condition.Invoke(obj))
				return obj.GetComponent<PlayerController>();
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
	}
}
