using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace FirstOtsukai
{
    public class ConsumeItem : Item
    {
        public override bool IsConsumeItem { get { return true; } }
        public override async Task<bool> 消費Async()
        {
            DefaultPersons.独白.Say("ボクは{0}を飲んだ。", this.HumanReadableName);
            DefaultPersons.独白.Say("うーん、美味い。");
            await Task.Delay(0);
            return true;
        }
        public static implicit operator ConsumeItem(ItemTemplate t)
        {
            return new ConsumeItem(t);
        }
        public ConsumeItem(ItemTemplate t)
            : base(t)
        {
        }
    }
    public static class HaoItems
    {
        public static Item Itemしょうゆ = new ItemTemplate()
        {
            Max = 1,
            Id = "{AD3FBEDA-E892-466d-91CD-5749ED337055}",
            Name = "しょうゆ",
            BaseDescription = "普通のおしょうゆです。",
            Price = 500,
        };
        public static Item Tシャツ = new ItemTemplate()
        {
            AvailableEquipMap = new[] { true },
            Max = 1,
            Id = "{9D03FA3B-63B3-483f-8261-49785484E6B1}",
            Name = "Tシャツ",
            BaseDescription = "普通の白いTシャツです。",
            Price = 500,
        };
        public static Item ズボン = new ItemTemplate()
        {
            AvailableEquipMap = new[] {false, true },
            Max = 1,
            Id = "{9717BF50-6948-424e-852F-E68A83BC4AE0}",
            Name = "ズボン",
            BaseDescription = "普通のズボンです。",
            Price = 1500,
        };
        public static Item つなぎ = new ItemTemplate()
        {
            AvailableEquipMap = new[] { true, true },
            SameTimeEquipMap = new [] {true,true},
            Max = 1,
            Id = "{E6D6B910-24FC-4945-AF39-8F68B48C4E54}",
            Name = "つなぎ",
            BaseDescription = "かっこいいつなぎだ。いかにも整備士らしいぞ。今日からもメカニックだ。",
            Price = 5000,
        };
        public static ConsumeItem Itemジュース = new ItemTemplate()
        {
            Max = 1,
            Id = "{37581C95-0430-48ae-B9A4-772BA417A7A5}",
            Name = "ジュース",
            BaseDescription = "普通のジュースです。",
            Price = 100,
        };
    }
}
