
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


	public GameObject FindCreature(Vector3Int cellPos)
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

	public Dictionary<int, GameObject> FindHitCreature(Vector3Int myPos)
    {
		Dictionary<int, GameObject> list = new Dictionary<int, GameObject>();
		Pos pos = Managers.Map.Cell2Pos(myPos);

		int[] _deltaY = new int[] { 1, -1, 0, 0, -1, 1, 1, -1 };
		int[] _deltaX = new int[] { 0, 0, -1, 1, -1, -1, 1, 1 };


		for(int i = 0; i < _deltaY.Length; i++)
        {
			for(int j = 0; j < _deltaX.Length; j++)
            {
				GameObject obj = Managers.Map.Objects[pos.Y + _deltaY[i], pos.X + _deltaX[j]];
				if (obj  != null)
                {
					CreatureController cc = obj.GetComponent<CreatureController>();
					if (list.ContainsKey(cc.Id) == false)
						list.Add(cc.Id , obj);
				}
					
			}
        }

		return list;
    }


	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects)
		{
			if (condition.Invoke(obj))
				return obj;
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
