
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

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
			if (obj == null)
				continue;

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


	public void CreateCreature(string prefabName, int idx , int teamId,  Weapons weapons = Weapons.Empty)
	{
		Vector3Int initPos;
		int loop = 0; //무한루프 방지용

		while (true)
		{

			initPos = new Vector3Int
			{
				x = UnityEngine.Random.Range(Managers.Map.MinX + 1, Managers.Map.MaxX - 1),
				y = UnityEngine.Random.Range(Managers.Map.MinY + 1, Managers.Map.MaxY - 1),
			};

			loop++;

			if (Managers.Map.CanGo(initPos) || loop >= 200)
				break;

		}

		GameObject go = Managers.Resource.Instantiate($"Character/{prefabName}", name: $"{prefabName}_{idx.ToString("000")}");
		CreatureController cc = go.GetComponent<CreatureController>();

		cc.Pos = initPos;
		cc.Id = idx;
		cc.TeamId = teamId;

		cc.CreateWeapon(weapons, 1);
	}
	public void Clear()
	{
		foreach (GameObject obj in _objects)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
	}


}
