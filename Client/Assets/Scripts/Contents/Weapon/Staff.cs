using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;


public class Staff : EquipWeapon
{


    protected override void UpdateRotation()
    {

        Quaternion prevQ = _q;

        _q = Util.LookAt2D(transform.position, _targetPos, FacingDirection.UP);
        transform.rotation = Quaternion.Slerp(prevQ, _q, 1f);
    }

    Coroutine _coRandShoot;

    public override void SkillEvent(S_Skill skillPacket)
    {
        _coRandShoot = StartCoroutine(CoRandShoot(skillPacket));


    }
    IEnumerator CoRandShoot(S_Skill skillPacket)
    {
        List<AttackPos> attackList = skillPacket.AttackList.ToList();
        System.Random random = new System.Random();

        AttackPos[] shupplePos = attackList.OrderBy(num => random.Next()).ToArray();


        _attackDir = skillPacket.AttackDir;

        //프레임 시간 차로 바로 갱신이 안 되서 여기서도 적용
        float targetX = skillPacket.TargetInfo.TargetX;
        float targetY = skillPacket.TargetInfo.TargetY;
        _targetPos = new Vector3(targetX, targetY);

        string weaponName = GetType().Name;

        foreach (AttackPos pos in shupplePos)
        {
            //이펙트 출력
            GameObject go = Managers.Resource.Instantiate($"Effect/{weaponName}/{weaponName}_Eff_{Id.ToString("000")}");
            _ec = go.GetComponent<EffectController>();
            ////실제 공격범위는 서버에서 처리 예정
            _ec.AttackList.Add(pos);
            
            //TODO : 서버로 옮김

            if (_owner == null)
                yield break;

            //실제 좌표 + 소유자 등록 [누가 공격했는지 전달해줘야 함 ] 
            _ec.CellPos = new Vector3Int(pos.AttkPosX, pos.AttkPosY, 0);
          //  _ec.transform.position = _ec.CellPos + (Vector3.one * 0.5f);
            _ec.transform.position = _ec.CellPos;

            _ec.Owner = _owner;

            yield return new WaitForSeconds(.1f);
        }

     


    }


}
