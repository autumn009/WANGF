using ANGFLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waFirstRPG
{
    public class HarItem : Item
    {
        public readonly int Value;

        /// <summary>
        /// テンプレートからアイテムを生成します。
        /// </summary>
        /// <param name="t">テンプレートオブジェクトです。</param>
        /// <returns>生成されたオブジェクトです。</returns>
        public static implicit operator HarItem(HarItemTemplate t) => new HarItem(t);
        public HarItem(HarItemTemplate t) : base(t)
        {
            this.Value = t.Value;
        }
    }
    public class HarItemTemplate : ItemTemplate
    {
        public int Value { get; init; }
    }

    public static class HarItems
    {
        public static HarItem 青銅の鎧 = new HarItemTemplate()
        {
            AvailableEquipMap = new[] { false, true },
            Max = 99,
            Id = "{671f7d76-aadf-47b4-b37e-ae0979ff9ce0}",
            Name = "青銅の鎧",
            BaseDescription = "青銅でできた鎧です。",
            Price = 1000,
            Value = 10,
        };
        public static HarItem 青銅の剣 = new HarItemTemplate()
        {
            AvailableEquipMap = new[] { true },
            Max = 99,
            Id = "{{e683cae4-04dd-4e5f-bc6e-0667084b5e50}",
            Name = "青銅の剣",
            BaseDescription = "青銅でできた剣です。",
            Price = 1500,
            Value = 10,
        };
        public static HarItem 鉄の鎧 = new HarItemTemplate()
        {
            AvailableEquipMap = new[] { false, true },
            Max = 99,
            Id = "{bdf92a33-4f64-472a-a091-6d45f58109cd}",
            Name = "鉄の鎧",
            BaseDescription = "鉄でできた鎧です。",
            Price = 2000,
            Value = 100,
        };
        public static HarItem 鉄の剣 = new HarItemTemplate()
        {
            AvailableEquipMap = new[] { true },
            Max = 99,
            Id = "{4b729b11-b301-4d06-8844-11b34887168f}",
            Name = "鉄の剣",
            BaseDescription = "鉄でできた剣です。",
            Price = 3000,
            Value = 100,
        };
    }
}
