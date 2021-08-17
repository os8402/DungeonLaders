using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager 
{
    public Action keyInputEvent = null;

    public bool W = false;
    public bool A = false;
    public bool D = false;
    public bool S = false;
    public bool Mouse_Left = false;
    public bool I = false; 
        
    public float H = 0.0f;
    public float V = 0.0f;
    public Vector3 GetAxis { get { return new Vector3(H, V); } }

    public void InputUpdate()
    {
        
        if (Input.anyKey == false)
        {
            InitInputData();
            return;
        }
            
        KeyMapping();
        keyInputEvent?.Invoke();
            
       
    }
    public void InitInputData()
    {
        W = false;
        A = false;
        S = false;
        D = false;
        I = false;
        Mouse_Left = false;
        H = 0.0f;
        V = 0.0f;
    }


    public void KeyMapping()
    {
        W = Input.GetKey(KeyCode.W);
        A = Input.GetKey(KeyCode.A);
        S = Input.GetKey(KeyCode.S);
        D = Input.GetKey(KeyCode.D);
        I = Input.GetKeyDown(KeyCode.I); 
        Mouse_Left = Input.GetMouseButtonDown(0);
        H = Input.GetAxisRaw("Horizontal");
        V = Input.GetAxisRaw("Vertical");

    }


    public bool PressMoveKey()
    {
        bool press = false;
        if (W || A || S || D)
            press = true;

        return press;
    }

   
    public void Clear()
    {

        keyInputEvent = null; 
    }


}
