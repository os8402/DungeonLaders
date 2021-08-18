using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    enum Images
    {
        Item_Info_Icon,
  
    }
    enum Texts
    {
        Item_Name_Text,
        Item_Info_Text,
        Attack_ValueText,
        Range_ValueText,
        CoolTime_ValueText

    }
    public List<UI_Inventory_Item> Items { get; }  = new List<UI_Inventory_Item>();
    public override void Init()
    {
        Items.Clear();

        GameObject grid = Util.FindChild(gameObject, "ItemGrid", true);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for(int i = 0; i < 16; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item" , grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        List<Item> items =  Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach(Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 16)
                continue;

            Items[item.Slot].SetItem(item);
        }

    }

    public void SetItemInfo()
    {

    }
}
