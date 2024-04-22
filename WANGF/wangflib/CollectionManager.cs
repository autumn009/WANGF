using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// コレクションされる項目です。
    /// </summary>
    public class CollectionItem
    {
        private string ownerModuleId;

        /// <summary>
        /// 一意のIdです。
        /// </summary>
        public string Id;
        /// <summary>
        /// 可読の名前です。
        /// </summary>
        public string Name;
        /// <summary>
        /// 所属するモジュールのIdです。
        /// </summary>
        public string OwnerModuleId
        {
            get { return ownerModuleId; }
            set
            {
                if (value == null || value == "") System.Diagnostics.Debug.Fail("BAD");
                ownerModuleId = value;
                //System.Diagnostics.Debug.WriteLine("[" + Name+ ":" + value + "]");
            }
        }
        /// <summary>
        /// 割合のカウント外の隠しアイテムであればTrueです。
        /// </summary>
        public bool Hidden; // 隠しアイテムならtrue
        /// <summary>
        /// 階層的な子アイテムのコレクションを取得するメソッドへのデリゲート型です
        /// </summary>
        public Func<CollectionItem[]> GetRawSubItems;
        /// <summary>
        /// 階層的な子アイテムのコレクションを取得する非同期メソッドへのデリゲート型です
        /// </summary>
        //public Func<Task<CollectionItem[]>> GetRawSubItemsAsync;
        private CollectionItem[] collectionItems;
        /// <summary>
        /// 階層的な子アイテムのコレクションを取得するメソッドへのデリゲート型です
        /// </summary>
        public CollectionItem[] GetSubItems()
        {
            if (collectionItems != null) return collectionItems;
            if (GetRawSubItems != null)
            {
                collectionItems = GetRawSubItems();
                return collectionItems;
            }
            //if (GetRawSubItemsAsync != null)
            //{
            //    collectionItems = await GetRawSubItemsAsync();
            //    return collectionItems;
            //}
            return null;
        }
        /// <summary>
        /// シミュレータでの実行本体です。シミュレータを提供しない場合意味がありません。
        /// </summary>
        public Func<string,Task> ProcedureAsync = null;/* DIABLE ASYNC WARN */ //実行本体
        /// <summary>
        /// シミュレータでの実行準備を行います。nullなら呼び出されません。シミュレータを提供しない場合意味がありません。
        /// </summary>
        public Func<string, Task> SetupAsync = null;/* DIABLE ASYNC WARN */ // シミュレーションの準備があれば
    }
    /// <summary>
    /// コレクションされる項目のコレクションです。
    /// </summary>
    public class Collection : SimpleName<Collection>
    {
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return Name; }
        }
        /// <summary>
        /// 一意のIdです。
        /// </summary>
        public override string Id
        {
            get { return RawId; }
        }
        /// <summary>
        /// 一意のIdです。
        /// </summary>
        public string RawId;
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public string Name;
        /// <summary>
        /// 実際にコレクションされる項目の一覧です。
        /// </summary>
        public CollectionItem[] Collections;
    }

    /// <summary>
    /// コレクションされる項目のコレクションです。
    /// </summary>
    public abstract class Collections : SimpleName<Collection>
    {
        private static async Task fillModuleIdAsync(CollectionItem[] items, string ownerModuleId)
        {
            foreach (var subitem in items)
            {
                subitem.OwnerModuleId = ownerModuleId;
                if (subitem.GetRawSubItems != null)
                {
                    CollectionItem[] subs = subitem.GetSubItems();
                    if (subs != null && subs.Length > 0) await fillModuleIdAsync(subs, ownerModuleId);
                }
            }
        }

        /// <summary>
        /// 複数モジュールにオリジンを持つ情報をマージし、重複をチェックします
        /// </summary>
        /// <param name="ownerModuleId">オーナーとなるモジュールのIdです。</param>
        /// <param name="item">マージするコレクションです。</param>
        public static async Task AddAndMergeAndCheckAsync(string ownerModuleId, Collection item)
        {
            if (string.IsNullOrWhiteSpace(item.Id)) throw new ApplicationException(item.HumanReadableName + "のIdはnullか空白のみにはできません。");
            if (string.IsNullOrWhiteSpace(item.HumanReadableName)) throw new ApplicationException(item.Id + "のHumanReadableNameはnullか空白のみにはできません。");
            item.OwnerModuleId = ownerModuleId;

            if (item.ForceOverride)
            {
                if (!List.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は存在しないので、オーバーライドできません。");
                // mergeする
                var list = new List<CollectionItem>();
                list.AddRange(List[item.Id].Collections);
                {
                    await fillModuleIdAsync(((Collection)item).Collections, ownerModuleId);
                    list.AddRange(((Collection)item).Collections);
                }
                ((Collection)item).Collections = list.ToArray();
                List[item.Id] = (Collection)item;
            }
            else
            {
                if (List.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は重複しています。");
                await fillModuleIdAsync(((Collection)item).Collections, ownerModuleId);
                List.Add(item.Id, (Collection)item);
            }

        }

        /// <summary>
        /// AddAndMergeAndCheckメソッドを複数のオブジェクトごとに呼び出します。
        /// </summary>
        /// <param name="ownerModuleId">オーナーとなるモジュールのIdです。</param>
        /// <param name="items">マージされるコレクション群です。</param>
        public static async Task AddAndMergeAndCheckAllAsync(string ownerModuleId, Collection[] items)
        {
            foreach (var item in items) await AddAndMergeAndCheckAsync(ownerModuleId, item);
        }

        /// <summary>
        /// コレクションのIdの重複とNameの確認を行います。
        /// ユーザーモジュールから呼び出す必要はありません。
        /// </summary>
        public static void ValidateCollections()
        {
            List<string> idsMain = new List<string>();
            foreach (var coll in SimpleName<Collection>.List.Values)
            {
                foreach (var n in coll.Collections)
                {
                    if (idsMain.Contains(n.Id)) throw new ApplicationException(n.Id + "は重複しています。");
                    idsMain.Add(n.Id);
                    System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(n.Name));
                    var x = n.GetSubItems();
                    if (x != null)
                    {
                        List<string> idsSub = new List<string>();
                        foreach (var m in x)
                        {
                            if (idsSub.Contains(m.Id)) throw new ApplicationException(m.Id + "は重複しています。");
                            idsSub.Add(m.Id);
                            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(m.Name));
                        }
                    }
                }
            }
        }
    }
}
