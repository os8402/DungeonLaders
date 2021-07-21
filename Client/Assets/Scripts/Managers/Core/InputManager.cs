using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager 
{
    public Action keyMoveEvent = null;
    public Action KeyIdleEvent = null;


    public void InputUpdate()
    {

        if (Input.anyKey == false)
        {
            KeyIdleEvent?.Invoke();
            return;
        }

        keyMoveEvent?.Invoke();
            
       
    }

    public void Clear()
    {
        KeyIdleEvent = null; 
        keyMoveEvent = null; 
    }


}
