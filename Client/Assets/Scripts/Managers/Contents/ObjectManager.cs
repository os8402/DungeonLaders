
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

	public void Add(PlayerInfo info , bool myPlayer = false)
    {
		if (myPlayer)
        {
			CreateCreature("MyWarrior", info);
	
		}
		else
        {
			CreateCreature("Warrior", info);
		}
    }
	public void CreateCreature(string prefabName, PlayerInfo info)
	{

		GameObject go = Managers.Resource.Instantiate
		($"Character/{prefabName}", name: $"{prefabName}_{info.PlayerId.ToString("000")}");
		CreatureController cc = go.GetComponent<CreatureController>();

	
		cc.PosInfo = info.PosInfo;
		cc.Id = info.PlayerId;
		cc.TeamId = info.TeamId;
		cc.SyncPos();

		cc.CreateWeapon(info.WeaponInfo, 1);

		//Vector3Int initPos;
		//int loop = 0; //무한루프 방지용

		//while (true)
		//{

		//	initPos = new Vector3Int
		//	{
		//		x = UnityEngine.Random.Range(Managers.Map.MinX + 1, Managers.Map.MaxX - 1),
		//		y = UnityEngine.Random.Range(Managers.Map.MinY + 1, Managers.Map.MaxY - 1),
		//	};

		//	loop++;

		//	if (Managers.Map.CanGo(initPos) || loop >= 200)
		//		break;

		//}


	}

	public void Add(int id , GameObject go)
	{
		_objects.Add(id , go);
	}



	public void RemoveMyPlayer()
    {
		if (MyPlayer == null)
			return;

		Remove(MyPlayer.Id);
		MyPlayer = null; 
    }
	public void Remove(int id)
	{
		GameObject go = null;
		if (_objects.TryGetValue(id, out go) == false)
			return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);

	}
	public GameObject FindById(int id)
    {
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go; 
    }
	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (obj == null)
				continue;

			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}


	public PlayerController Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj.GetComponent<PlayerController>();
		}

		return null;
	}



	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
	}


}
