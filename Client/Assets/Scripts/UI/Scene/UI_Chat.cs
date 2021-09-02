using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chat : UI_Base
{
    [SerializeField]
    Text _chatText = null;

    public InputField _playerInput = null;


    Queue<string> _mentQueue = new Queue<string>();

    public override void Init()
    {
        _playerInput.gameObject.SetActive(false);
    }



    public void RefreshUI(string msg)
    {

        //TODO : 체력
        _mentQueue.Enqueue(msg);
        _playerInput.text = string.Empty;
        _chatText.text = string.Empty;
        //_news_animator.Play("News_Text", 0, 0);

        Queue<string> _remainQueue = new Queue<string>();

        while (_mentQueue.Count > 0)
        {
            string p = _mentQueue.Dequeue();

            if (_mentQueue.Count <= 9)
            {
                _chatText.text += $"{p}\n";
                _remainQueue.Enqueue(p);
            }
        }

        _mentQueue = _remainQueue;


    }

}
