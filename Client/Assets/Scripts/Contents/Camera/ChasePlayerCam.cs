using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePlayerCam : MonoBehaviour
{
    private Camera _camera;
    private PlayerController _player;

    public Vector3 MousePos { get; private set; }
    [SerializeField]
    private float _range = 3f;
 //   [SerializeField]
  //  private float _lerp_speed = 5f;

    Vector3 _destMove;

    public Vector3 DestMove { 
        get
        {
            float x = Mathf.Clamp(MousePos.x, _player.transform.position.x - _range, _player.transform.position.x + _range);
            float y = Mathf.Clamp(MousePos.y, _player.transform.position.y - _range, _player.transform.position.y + _range);
            
            return _destMove = new Vector3(x , y , -10);
        }
    } 

    public void Init()
    {
        _camera = GetComponent<Camera>();

        //TODO : 나중에 MyPlayerController로 교체
        MyPlayerController player = FindObjectOfType<MyPlayerController>();
        if (player == null)
            return;

        _player = player; 

    }
    void LateUpdate()
    {
        if (_player == null)
            return;

        UpdateMoveCamera();
    }
 
    public void UpdateMoveCamera()
    {         
        //플레이어를 찍고 있는 카메라 안에서 마우스 포지션을 뽑음
        MousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        // transform.position = Vector3.Lerp(transform.position, DestMove, _lerp_speed * Time.deltaTime);
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, -10);
    }
}
