using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// あるショップにある売り物があるという関係性を記述します。
    /// </summary>
    public class ShopAndItemReleation : SimpleName<ShopAndItemReleation>
    {
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return "relation " + ShopID + " " + Items.GetItemByNumber(ItemID); }
        }
        /// <summary>
        /// 一意のIdです。このIdには重複チェックという以上の意味はありません。
        /// </summary>
        public override string Id
        {
            get { return ItemID + ShopID; }
        }
        /// <summary>
        /// ショップのIDを指定します。(場所のIDではありません)
        /// </summary>
        public readonly string ShopID;
        /// <summary>
        /// アイテムのIDを指定します。
        /// </summary>
        public readonly string ItemID;
        /// <summary>
        /// ショップに並んでいる条件を指定します。trueを返すとショップに並ぶことを意味します。
        /// nullのまま残すと常にtrue扱いされます。
        /// </summary>
        public readonly Func<bool> IsAvalable;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="shopID">ショップのIDを指定します。(場所のIDではありません)</param>
        /// <param name="itemID">アイテムのIDを指定します。</param>
        public ShopAndItemReleation(string shopID, string itemID)
        {
            ShopID = shopID;
            ItemID = itemID;
        }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="shopID">ショップのIDを指定します。(場所のIDではありません)</param>
        /// <param name="itemID">アイテムのIDを指定します。</param>
        /// <param name="isAvalable">ショップに並ぶ有効性を示すメソッドを指定します</param>
        public ShopAndItemReleation(string shopID, string itemID, Func<bool> isAvalable)
        {
            ShopID = shopID;
            ItemID = itemID;
            IsAvalable = isAvalable;
        }
    }

    /// <summary>
    /// ショップとアイテムの関係を扱います。
    /// </summary>
    public class ShopAndItemReleations
    {
        /// <summary>
        /// あるIDのショップで扱うアイテムの一覧を返します。
        /// </summary>
        /// <param name="shopID">ショップのIDです</param>
        /// <returns>アイテムの一覧です</returns>
        public static Item[] GetItems(string shopID)
        {
            List<Item> result = new List<Item>();
            foreach (var n in SimpleName<ShopAndItemReleation>.List.Values)
            {
                if (n.ShopID == shopID && (n.IsAvalable == null || n.IsAvalable()) ) result.Add(Items.GetItemByNumber(n.ItemID));
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// ショップの情報を記述します。
    /// </summary>
    public class Shop : SimpleName<Shop>
    {
        /// <summary>
        /// 一意の識別子です。
        /// </summary>
        public override string Id
        {
            get { return ShopID; }
        }
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return Name; }
        }
        /// <summary>
        /// 一意の識別子です。
        /// </summary>
        public readonly string ShopID;
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="shopID">一意の識別子です。</param>
        /// <param name="name">人間に可読の名前です。</param>
        public Shop(string shopID, string name)
        {
            ShopID = shopID;
            Name = name;
        }
    }
}
