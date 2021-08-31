
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

	public static GameObjectType GetObjectTypeById(int id)
    {
		int type = (id >> 24) & 0x07F;
		return (GameObjectType)type; 
    }

	public void Add(ObjectInfo info , bool myPlayer = false)
    {

        if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
            return;

        if (_objects.ContainsKey(info.ObjectId))
            return;

		
		

		GameObjectType objectType = GetObjectTypeById(info.ObjectId);
		if(objectType == GameObjectType.Player)
        {
            int lastIndex = info.Name.LastIndexOf('_');
            string subStr = info.Name.Substring(0, lastIndex);
			string jobName = subStr;

			if (myPlayer == false)
            {
				jobName = subStr.Replace("My", string.Empty);
			}
          
			CreateObject(jobName, info, myPlayer);

		}
		else if(objectType == GameObjectType.Monster)
        {
			CreateObject("Skeleton", info);
		}
        else if (objectType == GameObjectType.Projectile)
        {
			CreateObject(info.Name, info);
		}

    }
	public void CreateObject(string prefabName, ObjectInfo info , bool myPlayer = false)
	{

		GameObject go = Managers.Resource.Instantiate
		($"Character/{prefabName}", name: $"{prefabName}_{info.ObjectId.ToString("000")}");

		BaseController bc = go.GetComponent<BaseController>();

		bc.Id = info.ObjectId;
		bc.TeamId = info.TeamId;
		bc.TotalHp = info.TotalHp; 
		bc.PosInfo.MergeFrom(info.PosInfo);
		bc.Stat.MergeFrom(info.StatInfo);
	
		bc.SyncPos();

		CreatureController cc = bc.GetComponent<CreatureController>();

		

		if(cc != null)
        {
			cc.CreateWeapon(info.WeaponId);
            cc.JobName = prefabName;

        }
		



		_objects.Add(info.ObjectId, go);

		if(myPlayer)
        {
			MyPlayer = bc as MyPlayerController;
			MyPlayer.JobName = prefabName.Replace("My", string.Empty);

		}

	}

	public void Remove(int id)
	{
        if (MyPlayer != null && MyPlayer.Id == id)
            return;

        if (_objects.ContainsKey(id) == false)
            return;

        GameObject go = FindById(id);
        if (go == null)
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
	public GameObject FindCreature(Vector3Int cellPos)
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
		MyPlayer = null;
	}


}
