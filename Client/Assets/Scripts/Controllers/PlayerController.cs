using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{

    public float Speed { get; private set; } = 5.0f;
    public int Hp { get; private set; } = 100;

    private SpriteRenderer _sprite;
    private ChasePlayerCam _camera;
    private Animator _animator;
   
    public CreatureState State { get; private set; }

    private Vector3 _pos;
    public Vector3 Pos 
    {
        get { return _pos; }
        set
        {
            State = (value == Vector3.zero ? State = CreatureState.Idle : CreatureState.Move); 
            UpdateAnimation();
        }
    }

    //1.여기서 방향을 받는 김에 회전도 같이 설정
    //2.마우스 위치를 기준으로 합니다. 
    //3.관련 기능 매핑해둘 것
    private int _dir = 0; 
    public int Dir {

        get { return _dir; }
        set
        {
            _dir = value;

            //초기화가 안된 것
            if (_sprite == null)
                return;

            _sprite.flipX = (_dir == 1 ? true : false);  
        } 
    }
 

    void Init()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        Transform camera = Camera.main.transform;
        camera.parent = transform;
        camera.position = new Vector3(0, 1, -10);
        _camera = camera.GetComponent<ChasePlayerCam>();
        _camera.Init();

        
    }
 


    void Start()
    {
        Init();
        Managers.Input.keyMoveEvent -= UpdateMoveInput;
        Managers.Input.keyMoveEvent += UpdateMoveInput;
        Managers.Input.KeyIdleEvent -= UpdateIdle;
        Managers.Input.KeyIdleEvent += UpdateIdle;
    }

    void Update()
    {
        UpdateRotate();
    }
    void UpdateIdle()
    {
        Pos = Vector3.zero; 
    }

    void UpdateMove(Vector3 pos)
    {
        transform.position += pos * Speed * Time.deltaTime;
    }

    private void UpdateRotate()
    {
        Quaternion q = Util.RotateDir2D(transform.position, _camera.MousePos);
   
        if (q.z > Quaternion.identity.z) // 오른쪽
        {
            Dir = 1;
        }
        else if (q.z < Quaternion.identity.z)// 왼쪽
        {
            Dir = -1;
        }

        else return;

    }
    
    void UpdateAnimation()
    {
        if (_animator == null)
            return;

        switch(State)
        {
            case CreatureState.Idle:
                _animator.Play("Warrior_Idle");
                break;
            case CreatureState.Move:
                _animator.Play("Warrior_Move");
                break;
            case CreatureState.Death:
                _animator.Play("Warrior_Death");
                break;
        }
    }

    //My Player Controller 따로 제작 예정
    //거기에 이동관련 부분은 서버에서 제작할 예정이기 떄문에
    // 임시적인 땜빵임
    // 나중에 패킷을 보내면서 이동

    void UpdateMoveInput()
    {
       
        Vector3 pos = Vector3.zero; 

        if(Input.GetKey(KeyCode.W))
        {
            pos = Vector3.up;  
        }
        else if (Input.GetKey(KeyCode.A))
        {
            pos = Vector3.left;

        }
        else if (Input.GetKey(KeyCode.D))
        {
            pos = Vector3.right;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            pos = Vector3.down;
        }
      
        Pos = pos; 
       
        UpdateMove(pos);
    }
}
