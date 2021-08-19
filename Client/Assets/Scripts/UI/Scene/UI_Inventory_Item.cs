using Data;
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

    
    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log("Click Item");

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            if (itemData == null)
                return;
            //C_USE_ITEM 
            if (itemData.itemType == ItemType.Consumable)
                return;

            C_EquipItem equipItemPacket = new C_EquipItem();
            equipItemPacket.ItemDbId = ItemDbId;
            equipItemPacket.Equipped = !Equipped;

            Managers.Network.Send(equipItemPacket);
        });
    }

    public void SetItem(Item item)
    {
        if(item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);

            return;
        }

        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _icon.gameObject.SetActive(true);
        _frame.gameObject.SetActive(Equipped);


    }
 
}
