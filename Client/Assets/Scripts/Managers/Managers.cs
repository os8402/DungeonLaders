﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents
    InventoryMananger _inven = new InventoryMananger();
    MapManager _map = new MapManager();
    ObjectManager _object = new ObjectManager();
    NetworkManager _network = new NetworkManager();
    WebManager _web = new WebManager(); 

    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();
    InputManager _input = new InputManager();

    public static InventoryMananger Inven { get { return Instance._inven; } }
    public static MapManager Map { get { return Instance._map; } }
    public static ObjectManager Object { get { return Instance._object; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static WebManager Web { get { return Instance._web; } }
    public static DataManager Data { get { return Instance._data; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
  
    public static InputManager Input { get { return Instance._input; } }
   
    #endregion

    #region Core

    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.InputUpdate();
        _network.Update();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

          //  s_instance._network.Init();
            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
        Input.Clear();
    }


    #endregion


}
