using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game
{
    public class Inventory
    {
        public Dictionary<int, Item> Items { get; set; } = new Dictionary<int, Item>();

        public HashSet<int?> ReserveSlotList { get; set; } = new HashSet<int?>();

        public void Add(Item item)
        {
            Items.Add(item.ItemDbId, item);

            CancleReserveSlot(item.Slot);
        }
        public void Remove(Item item)
        {
            Items.Remove(item.ItemDbId);

            CancleReserveSlot(item.Slot);
        }

        public Item Get(int itemDbId)
        {
            Item item = null;
            Items.TryGetValue(itemDbId, out item);
            return item;
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

        public void RefreshSlot(Item item, int num)
        {
            if (Items.ContainsKey(item.ItemDbId) == false)
                return;

            Items[item.ItemDbId].Slot = num;
        }

        public int? GetEmptySlot()
        {
            for (int slot = 0; slot < 16; slot++)
            {
                if (ReserveSlotList.Contains(slot))
                    continue;

                Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);

                if (item == null)
                {
                
                    ReserveSlotList.Add(slot);
                    return slot;
                }

            }

            return null;
        }

        public void CancleReserveSlot(int? slot)
        {
            ReserveSlotList.Remove(slot);
        }
    }
}
