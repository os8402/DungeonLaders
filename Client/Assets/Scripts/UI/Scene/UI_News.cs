using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_News : UI_Base
{
    [SerializeField]
    Text _news_text = null;
    Animator _news_animator = null; 

    Queue<string> _mentQueue = new Queue<string>();
   

    public override void Init()
    {
        _news_animator = _news_text.GetComponent<Animator>();

    }

    int _index = 1;
    public void RefreshUI(string ment)
    {

        _mentQueue.Enqueue($"({_index++}) {ment}");

        _news_text.text = string.Empty; 
        _news_animator.Play("News_Text", 0, 0);

        Queue<string> _remainQueue = new Queue<string>();

        while(_mentQueue.Count > 0)
        {
            string p = _mentQueue.Dequeue();

            if (_mentQueue.Count <= 3)
            {
           
                _news_text.text += $"{p}\n";
                _remainQueue.Enqueue(p);

            }

        }

        _mentQueue = _remainQueue;


    }
}


