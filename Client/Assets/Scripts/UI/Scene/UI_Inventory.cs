using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : UI_Base
{
    enum Images
    {
        Slot_ItemInfo
        
    }
    enum Texts
    {
        Item_Name_Text,
        Item_Info_Text,
        Stat01_ValueText,
        Stat02_ValueText,
        Stat03_ValueText,
        RequestText

    }
    enum Buttons
    {
        Request_Btn,
        Remove_Btn
    }

    public List<UI_Inventory_Item> Items { get; }  = new List<UI_Inventory_Item>();

    public Item SelectItem { get; set; }
    public int SelectIndex { get; set; }

    void ResetInfo(bool next = false)
    {
        if(next == false)
        {
            SelectItem = null;
        }
        else
        {
            Item item = Managers.Inven.Find((i) =>
            {
                if (i.Slot == SelectItem.Slot)
                    return true;

                return false; 
            });

            SelectItem = item;
        }

        GetImage((int)Images.Slot_ItemInfo).enabled = false;
        GetText((int)Texts.Item_Name_Text).text = string.Empty;
        GetText((int)Texts.Item_Info_Text).text = string.Empty;
        GetText((int)Texts.Stat01_ValueText).text = string.Empty;
        GetText((int)Texts.Stat02_ValueText).text = string.Empty;
        GetText((int)Texts.Stat03_ValueText).text = string.Empty;
        GetButton((int)Buttons.Request_Btn).interactable = false;
        GetButton((int)Buttons.Remove_Btn).interactable = false;
    }

    private void OnEnable()
    {
        ResetInfo();
    }

    public void MakeItem()
    {
        Items.Clear();
        ResetInfo(next : true);

        GameObject grid = Util.FindChild(gameObject, "ItemGrid", true);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < 16; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }

        RefreshUI();
    }

    public override void Init()
    {
        Items.Clear();

    
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        GetImage((int)Images.Slot_ItemInfo).enabled = false;
        GetText((int)Texts.Item_Name_Text).text = string.Empty;
        GetText((int)Texts.Item_Info_Text).text = string.Empty;
        GetText((int)Texts.Stat01_ValueText).text = string.Empty;
        GetText((int)Texts.Stat02_ValueText).text = string.Empty;
        GetText((int)Texts.Stat01_ValueText).text = string.Empty;

        BindEvent(GetButton((int)Buttons.Request_Btn).gameObject, (e) => { RequestEquipOrUseItem(); });
        BindEvent(GetButton((int)Buttons.Remove_Btn).gameObject, (e) => { RemoveItem(); });

        MakeItem();


    }

    public void RefreshUI()
    {
        
        if (Items.Count == 0)
            return;

        List<Item> items =  Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        if(SelectItem != null)
        {     
            SetItemInfo(SelectItem);
            Items[SelectItem.Slot].Selected = true;
        }
    

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 16)
                continue;

           
            Items[item.Slot].SetItem(item);
        }

 

    }

    public void SetItemInfo(Item item)
    {
        //셀렉트 상태 전부 해제
        foreach (UI_Inventory_Item i in Items)
            i.SelectedCancle();

        ResetInfo(); 

        if (item == null)
            return;

        GetButton((int)Buttons.Request_Btn).interactable = true;
        GetButton((int)Buttons.Remove_Btn).interactable = true;

        SelectItem = item;
        
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);


        GetImage((int)Images.Slot_ItemInfo).enabled = true;
        GetImage((int)Images.Slot_ItemInfo).sprite = icon;

        GetText((int)Texts.Item_Name_Text).text = itemData.name;
        GetText((int)Texts.Item_Info_Text).text = itemData.info;

        GetText((int)Texts.RequestText).text = "장착하기";

        if (itemData.itemType == ItemType.Weapon)
        {
            WeaponData weapon = (WeaponData)itemData;
            GetText((int)Texts.Stat01_ValueText).text = $"Damage : + {weapon.damage.ToString()}";
            GetText((int)Texts.Stat02_ValueText).text = $"Range : + {weapon.attackRange.ToString()}";
            GetText((int)Texts.Stat03_ValueText).text = $"Cooltime : + {weapon.cooldown.ToString()}";

            if(item.Equipped)
                GetText((int)Texts.RequestText).text = "해제하기";

        }
        else if(itemData.itemType == ItemType.Armor)
        {
            ArmorData armor = (ArmorData)itemData;
            GetText((int)Texts.Stat01_ValueText).text = $"Defence : + {armor.defence.ToString()}";
            GetText((int)Texts.Stat02_ValueText).text = $"Hp : + {armor.hp.ToString()}";
            GetText((int)Texts.Stat03_ValueText).text = $"Speed : + {armor.speed.ToString()}";

            if (item.Equipped)
                GetText((int)Texts.RequestText).text = "해제하기";

        }
        else if(itemData.itemType == ItemType.Consumable)
        {
            ConsumableData consumable = (ConsumableData)itemData;
            GetText((int)Texts.Stat01_ValueText).text = $"Heal : + {consumable.heal.ToString()}";

            GetText((int)Texts.RequestText).text = "사용하기";
        }


    }

    public void RequestEquipOrUseItem()
    {
      
        if (SelectItem == null)
            return;

        Debug.Log("Request Item");

        //C_USE_ITEM 
        if (SelectItem.ItemType == ItemType.Consumable)
        {
     
            C_UseItem useItemPacket = new C_UseItem();
            useItemPacket.ItemDbId = SelectItem.ItemDbId;
            useItemPacket.UseCount = 1; 
            Managers.Network.Send(useItemPacket); 
        }
           
        //C_EQUIP_ITEM
        else
        {

            C_EquipItem equipItemPacket = new C_EquipItem();
            equipItemPacket.ItemDbId = SelectItem.ItemDbId;
            equipItemPacket.Equipped = !SelectItem.Equipped;
            Managers.Network.Send(equipItemPacket);

        }
    }

    public void RemoveItem()
    {
        if (SelectItem == null)
            return;

 

        //장착여부했으면 삭제 안하고 
        //TODO : 안내문구 : 정말 삭제할건지
        if (SelectItem.Equipped)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.NewsUI.RefreshUI($"장착 중인 아이템은 버릴 수 없습니다.");
            return;
        }
          

        Debug.Log("Delete Item");

        C_RemoveItem removeItemPacket = new C_RemoveItem();
        removeItemPacket.ItemDbId = SelectItem.ItemDbId;
        Managers.Network.Send(removeItemPacket);

    }
}
