using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;
    [SerializeField]
    Image _frame = null;

    public int ItemDbId { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
    public bool Equipped{ get; set; }

    //아틀라스라서 그냥 임시뗌빵으로 들고있어야 할 듯
    public Sprite[] Weapons { get; set; }
    public Sprite[] Armors { get; set; }

    
    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log("Click Item");

            C_EquipItem equipItemPacket = new C_EquipItem();
            equipItemPacket.ItemDbId = ItemDbId;
            equipItemPacket.Equipped = !Equipped;

            Managers.Network.Send(equipItemPacket);
        });
    }

    public void SetItem(Item item)
    {
        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _frame.gameObject.SetActive(Equipped);
       

    }
 
}
