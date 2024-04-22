using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// アイテムを記述する基底クラスを提供します。
    /// </summary>
    [Serializable]
    public class Item : SimpleName<Item>
    {
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        public override string HumanReadableName
        {
#pragma warning disable
            get { return Name; }
#pragma warning resotore
        }
        /// <summary>
        /// 識別用の一意のIdを返します。
        /// </summary>
        public override string Id
        {
            get { return id; }
        }
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        [Obsolete]
        public readonly string Name;
        /// <summary>
        /// Guid文字列ならIdを示します。nullまたは空文字列ならNullアイテムを示します。
        /// </summary>
        private readonly string id;
        /// <summary>
        /// 基本的な説明文字列です。
        /// </summary>
        public readonly string BaseDescription;
        /// <summary>
        /// 一度に所有できる最大数を指定します。
        /// </summary>
        public readonly int Max;
        /// <summary>
        /// 価格を指定します。単位はモジュール依存です。0は売買不可のアイテムを示します。
        /// </summary>
        public readonly int Price;
        /// <summary>
        /// 外すことができないアイテムを示します。ただし明示的なプログラムによる書き換えはできます。
        /// </summary>
        public readonly bool Is取り外し不可能Item;
        /// <summary>
        /// NullアイテムならTrueを返します。
        /// </summary>
        public bool IsItemNull { get { return Id == null || Id == ""; } }
        /// <summary>
        /// 完全な説明文字列を返します。
        /// 追加する内容がある場合はオーバーライドして加工します。
        /// </summary>
        public virtual string FullDescription
        {
            get { return BaseDescription; }
        }
        /// <summary>
        /// メニューより使用できるアイテムです。
        /// </summary>
        public virtual bool IsConsumeItem { get { return false; } }
        /// <summary>
        /// アイテムを使用します。
        /// </summary>
        /// <returns>消費して消滅したらtrue。残ったらfalse。</returns>
        public virtual async Task<bool> 消費Async()
        {
            DefaultPersons.システム.Say(HumanReadableName + "は使用できません。", HumanReadableName);
            return false;
        }
        /// <summary>
        /// テンプレートからアイテムを生成します。
        /// </summary>
        /// <param name="t">テンプレートオブジェクトです。</param>
        /// <returns>生成されたオブジェクトです。</returns>
        public static implicit operator Item(ItemTemplate t)
        {
            return new Item(t);
        }
        /// <summary>
        /// 特定の装備部位に「今」装備できるかを判定します。
        /// 特定の装備部位に装備できるかではないことに注意。
        /// それは別のレイヤーで処理される
        /// このメソッドの意義は、たとえばある条件を満たさないと装備できないアイテムが
        /// 判定するような使い方
        /// 引数のpartは、2箇所以上に装備可能なアイテムで、別々に条件が設定されている
        /// ケースに対応するため
        /// </summary>
        /// <param name="part">装備しようとしている部位</param>
        /// <returns>nullなら装備可能。文字列なら装備できない理由</returns>
        public virtual string CanEquip(int part) { return null; }
        /// <summary>
        /// 特定の装備部位に「今」装備できるかを判定します。
        /// 特定の装備部位に装備できるかではないことに注意。
        /// それは別のレイヤーで処理される
        /// このメソッドの意義は、たとえばある条件を満たさないと装備できないアイテムが
        /// 判定するような使い方
        /// 引数のpartは、2箇所以上に装備可能なアイテムで、別々に条件が設定されている
        /// ケースに対応するため
        /// </summary>
        /// <param name="part">装備しようとしている部位</param>
        /// <param name="personId">装備しようとしている対象人物</param>
        /// <param name="equipSet">その時の装備セット全体</param>
        /// <returns>nullなら装備可能。文字列なら装備できない理由</returns>
        public virtual string CanEquipEx(int part, string personId, EquipSet equipSet) { return null; }
        /// <summary>
        /// 静的な装備可能部位マップを提供します。
        /// 配列を返すのは2つ以上の部位に装備できる場合に対応するため
        /// サイズ外の要素はfalseを仮定され、サイズ0の配列は装備不可能アイテムを意味する
        /// ここで指定されたアイテムは装備時に見えるが、CanEquipメソッドで理由を付けて拒否できる
        /// </summary>
        /// <returns>装備可能部位の配列</returns>
        public bool[] AvailableEquipMap = new bool[0];
        /// <summary>
        /// 同時に装備する部位を示します。上下服など
        /// サイズ外の要素はfalseを仮定されるが、AvailableEquipMapで指定された場所は
        /// 暗黙的に装備可能となるため、提供しない場合は1箇所だけ装備可能となる
        /// </summary>
        /// <returns></returns>
        public bool[] SameTimeEquipMap = new bool[0];

        /// <summary>
        /// IDからアイテムを得ます。アイテムが無い場合は自動的にItemNullが返されます
        /// </summary>
        /// <param name="id">アイテムのIDです</param>
        /// <returns>発見したアイテムです</returns>
        public static Item GetItemById(string id)
        {
            if (id != null && id != "" && Item.List.TryGetValue(id, out var item)) return item;
            return Items.ItemNull;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="t">テンプレート・オブジェクトです。</param>
        public Item(ItemTemplate t)
        {
#pragma warning disable
            Name = t.Name;
#pragma warning resotore
            id = t.Id;
            Max = t.Max;
            Price = t.Price;
            BaseDescription = t.BaseDescription;
            AvailableEquipMap = t.AvailableEquipMap;
            SameTimeEquipMap = t.SameTimeEquipMap;
            Is取り外し不可能Item = t.Is取り外し不可能Item;
        }

        public Item CloneExactItem()
        {
            var clone = new ItemTemplate();
            clone.Name = Name;
            clone.Id = Id;
            clone.Max = Max;
            clone.Price = Price;
            clone.BaseDescription = BaseDescription;
            clone.AvailableEquipMap = AvailableEquipMap;
            clone.SameTimeEquipMap = SameTimeEquipMap;
            clone.Is取り外し不可能Item = Is取り外し不可能Item;
            return new Item(clone);
        }
    }

    /// <summary>
    /// アイテムを生成するテンプレートになります。
    /// </summary>
	public class ItemTemplate
	{
        /// <summary>
        /// アイテムの名前です。
        /// </summary>
		public string Name ="(no name)";
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public string Id;
        /// <summary>
        /// 最大所有数です。
        /// </summary>
		public int Max=1;
        /// <summary>
        /// 価格です。0は売買不可です。
        /// </summary>
		public int Price;
        /// <summary>
        /// 外すことができないアイテムを示します。ただし明示的なプログラムによる書き換えはできます。
        /// </summary>
        public bool Is取り外し不可能Item;
        /// <summary>
        /// ベースとなる説明文です。
        /// </summary>
		public string BaseDescription = "";
        /// <summary>
        /// 装備可能な部位はtrueになります。
        /// </summary>
        public bool[] AvailableEquipMap = new bool[0];
        /// <summary>
        /// 同時に装備される部位がtrueになります。
        /// </summary>
        public bool[] SameTimeEquipMap = new bool[0];
    }

    /// <summary>
    /// アイテムのコレクションを管理します。
    /// </summary>
	public static class Items
	{
        /// <summary>
        /// 便宜上提供する存在しないことを示すアイテム
        /// Id=nullまたはId=""のアイテムは、装備設定時に何も装備していないことを示す特殊アイテム
        /// </summary>
        public static Item ItemNull
        {
            get { return itemNull; }
        }
        private static Item itemNull = new ItemTemplate()
		{
			Id = "",
			Name = "Null",
			BaseDescription = "(無し)",
			Max = 0,
			Price = 0,
		};

        /// <summary>
        /// Idからアイテムを引く。Idはnullまたは""の場合はItemNullを返す。
        /// </summary>
        /// <param name="number">Idを示す</param>
        /// <returns>発見されたアイテム</returns>
		public static Item GetItemByNumber(string number)
		{
			if (number == null || number == "") return ItemNull;
			return SimpleName<Item>.List[number];
		}

        /// <summary>
        /// アイテムのIdの一覧を返す
        /// </summary>
        /// <returns>アイテムのIdの一覧</returns>
		public static IEnumerable<string> GetItemIDList()
		{
            return Item.List.Keys;
		}

        /// <summary>
        /// アイテムの一覧を返す
        /// </summary>
        /// <returns>アイテムの一覧</returns>
        public static IEnumerable<Item> GetItemList()
        {
            return Item.List.Values;
        }
	}
}
