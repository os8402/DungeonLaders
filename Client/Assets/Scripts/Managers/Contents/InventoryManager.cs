using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMananger
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }

    public Item Get(int itemDbId)
    {
        Item item = null;
        Items.TryGetValue(itemDbId, out item);
        return item;
    }
    public bool Remove(Item item)
    {
        return Items.Remove(item.ItemDbId);
    }

    public void RefreshSlot(Item item, int num)
    {
        if (Items.ContainsKey(item.ItemDbId) == false)
            return;

        Items[item.ItemDbId].Slot = num;
    }

    public Item Find(Func<Item, bool> condition)
    {
        foreach (Item item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }
        return null;
    }

    public void Clear()
    {
        Items.Clear();
    }
}

