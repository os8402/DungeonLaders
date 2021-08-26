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
    [SerializeField]
    Text _countText = null; 

    public int ItemDbId { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
    public bool Equipped{ get; set; }

    public bool Selected { get; set; } = false; 

    public Item Item { get; set; }

    public override void Init()
    {

        UI_Inventory invenUI = GetComponentInParent<UI_Inventory>();
 

        _icon.gameObject.BindEvent((e) =>
        {
           
            invenUI.SetItemInfo(Item);

            if (Item == null)
                return;

            Selected = !Selected;
            _frame.gameObject.SetActive(Selected);



        });
    }

    public void SelectedCancle()
    {
        Selected = false;
        _frame.gameObject.SetActive(Selected);

    }

    public void SetItem(Item item)
    {


        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
            _countText.text = string.Empty;

            return;
        }


        _icon.enabled = true;
        _frame.enabled = true; 
      

        _frame.gameObject.SetActive(Selected);

        Item = item; 

        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _icon.gameObject.SetActive(true);
       
        if(itemData.itemType == ItemType.Consumable)
        {
            _countText.text = $"{Count}";
        }
        else
        {
            string e = (Equipped) ? "E" : string.Empty;
            _countText.text = $"{e}";
        }

    }

}
