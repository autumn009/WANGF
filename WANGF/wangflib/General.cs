using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using ANGFLib;

using System.Threading.Tasks;
using System.Numerics;
using wangflib;
using System.Linq.Expressions;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace ANGFLib
{
    /// <summary>
    /// 装備情報のラッパーを提供します。
    /// </summary>
    public class EquipWrapper
    {
        private string targetPersonId = DefaultPersons.主人公.Id;
        public string TargetPersonId
        {
            get { return targetPersonId; }
            set { targetPersonId = value; }
        }
        /// <summary>
        /// ある装備部位の情報を読み書きします。
        /// 複数の部位に装備できるアイテムは、同時に複数の部員に装備されます。
        /// </summary>
        /// <param name="index">装備部位です。</param>
        /// <returns>装備されているアイテムのIdです。</returns>
        public string this[int index]
        {
            get { return Flags.GetRawEquip(targetPersonId, index); }
            set
            {
                var oldItem = Item.GetItemById(Flags.GetRawEquip(targetPersonId, index));
                Item item = Item.GetItemById(value);
                ItemArray.SetOperation(item, index,
                    (i) => Item.GetItemById(Flags.GetRawEquip(targetPersonId, i)),
                    (i, v) =>
                    {
                        Flags.SetRawEquip(targetPersonId, i, v.Id);
                        return default;
                    });
                // StatusHideかDisableEquipReportingの時は通知しない
                if (!DisableEquipReporting && !ANGFLib.State.CurrentPlace.IsStatusHide)
                {
                    var currentPerson = Person.List[Flags.Equip.TargetPersonId];
                    if (item.IsItemNull && oldItem.IsItemNull) return;
                    if (oldItem.Id == item.Id) return;
                    if (item.IsItemNull)
                    {
                        DefaultPersons.システム.Say($"{currentPerson.HumanReadableName}は{oldItem.HumanReadableName}を外した。");
                    }
                    else
                        DefaultPersons.システム.Say($"{currentPerson.HumanReadableName}は{item.HumanReadableName}を装備した。");
                }
            }
        }
        /// <summary>
        /// trueなら装備変更の通知は行われない。
        /// セーブされない情報なので一時的な変更に使うこと。
        /// </summary>
        public static bool DisableEquipReporting { get; set; }

        internal EquipSet ToEquipSet()
        {
            var set = new EquipSet();
            for (int i = 0; i < EquipType.List.Count; i++)
            {
                var id = this[i];
                if (string.IsNullOrWhiteSpace(id))
                    set.AllItems[i] = Items.ItemNull;
                else
                    set.AllItems[i] = Item.List[id];
            }
            return set;
        }

        internal void FromEquipSet(EquipSet set)
        {
            for (int i = 0; i < EquipType.List.Count; i++)
            {
                this[i] = set.AllItems[i].Id;
            }
        }
    }

    /// <summary>
    /// 装備アイテムの情報一式です。
    /// </summary>
    public class ItemArray
    {
        /// <summary>
        /// 指定されたアイテムを装備しているか判定する
        /// </summary>
        /// <param name="item">調査するアイテム。ただしnullなアイテムは不可</param>
        /// <returns>そのアイテムが使用されていたらtrue</returns>
        public bool IsEquipedItem(Item item)
        {
            Item found = allItems.FirstOrDefault((i) => i.Id == item.Id);
            return found != null;
        }

        /// <summary>
        /// 装備アイテムをセットします。複数装備部位に対応します。
        /// </summary>
        /// <param name="val">セットするアイテムです。</param>
        /// <param name="index">セットする装備部位です。</param>
        /// <param name="getter">装備情報をゲットします。</param>
        /// <param name="setter">装備情報をセットします。</param>
        public static void SetOperation(Item val, int index, Func<int, Item> getter, Func<int, Item, Dummy> setter)
        {
            // remove operation
            // 全ての部位から除去される必要がある
            Item toRemove = getter(index);
            setter(index, Items.ItemNull);
            bool[] canEquipToRemove = toRemove.SameTimeEquipMap;
            for (int i = 0; i < canEquipToRemove.Length; i++)
            {
                if (canEquipToRemove[i])
                {
                    setter(i, Items.ItemNull);
                }
            }
            // add operation
            // 全ての部位に追加される
            for (int i = 0; i < val.SameTimeEquipMap.Length; i++)
            {
                if (val.SameTimeEquipMap[i])
                {
                    setter(i, val);
                }
            }
            // 少なくとも目的の場所に入る
            setter(index, val);
        }

        private Item[] allItems;
        /// <summary>
        /// 装備アイテムの情報を読み書きします。複数装備可能なアイテムも1つだけ読み書きします。
        /// </summary>
        /// <param name="index">装備部位です</param>
        /// <returns>装備アイテムです</returns>
        public Item this[int index]
        {
            get { return allItems[index]; }
            set { SetOperation(value, index, (i) => allItems[i], (i, v) => { allItems[i] = v; return new Dummy(); } ); }
            //set { allItems[index] = value; }
        }
        /// <summary>
        /// 収納されている装備部位の数を返します
        /// </summary>
        public int Length { get { return allItems.Length; } }
        /// <summary>
        /// コンストラクタです
        /// </summary>
        public ItemArray()
        {
            allItems = new Item[32];	// ここは決め打ちであり好ましくない
            for (int i = 0; i < allItems.Length; i++) allItems[i] = Items.ItemNull;
        }
    }

    /// <summary>
    /// 装備品一式を一時的に保管する
    /// </summary>
    public class EquipSet
    {
        /// <summary>
        /// 装備情報一覧です。
        /// </summary>
        public ItemArray AllItems = new ItemArray();

        /// <summary>
        /// 複製を作ります
        /// </summary>
        /// <returns>別インスタンスの複製です</returns>
        public EquipSet Duplicate()
        {
            var r = new EquipSet();
            for (int i = 0; i < AllItems.Length; i++) r.AllItems[i] = AllItems[i];
            return r;
        }

        /// <summary>
        /// 2つのEquipSetオブジェクトが同じであるかを判定します。
        /// </summary>
        /// <param name="target">比較する対象です。</param>
        /// <returns>同じならtrueを返します。</returns>
        public bool Equals(EquipSet target)
        {
            for (int i = 0; i < AllItems.Length; i++)
            {
                if (AllItems[i] != target.AllItems[i]) return false;
            }
            return true;
        }
    }

    public interface ILinkMoveCalc
    {
        int リンク移動時間計算(Place currentPlace, Place distPlace);
    }

    /// <summary>
    /// 特定の場所や人に関係のない汎用的な行動やリアクションのコードを集める
    /// </summary>
    public static class General
    {
        /// <summary>
        /// SuperTalkの条件ジャンプに使用される汎用のフラグです
        /// </summary>
        public static bool TheFlag = false;

        /// <summary>
        /// 現在の拡張子です。一般アプリからは読み出しのみを推奨します。
        /// </summary>
        public static string FileExtention = "angf";
        /// <summary>
        /// 汎用の乱数提供用です。現在の日付時刻で初期化されるので、ほぼ予測不可能な値を得られます
        /// </summary>
        public static readonly Random Rand = new Random(unchecked((int)DateTime.Now.Ticks));

        public static string GameTitle = "WANGF";

        /// <summary>
        /// Utility Method for convert IAsyncEnumerable to Array
        /// </summary>
        /// <typeparam name="T">Type for collection</typeparam>
        /// <param name="enu">Items Enumaration</param>
        /// <returns></returns>
        public static async Task<T[]> IAsyncEnumerableToArrayAsync<T>(IAsyncEnumerable<T> enu)/* DIABLE ASYNC WARN */
        {
            var list = new List<T>();
            await foreach (var item in enu) list.Add(item);
            return list.ToArray();
        }

        internal static async Task<bool> RestAsync(int minute)
        {
            DefaultPersons.システム.Say("{0}分、身体を休めました。", minute);
            bool result = await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now.AddMinutes(minute));
            if (!result) State.GoTime(minute);
            return true;
        }

        internal static async Task<bool> Rest60Async()
        {
            return await RestAsync(60);
        }

        internal static async Task<bool> Rest15Async()
        {
            return await RestAsync(15);
        }

        internal static async Task<bool> RestUntilAsync(int hour, bool nextDay)
        {
            if (nextDay == false && Flags.Now.Hour >= hour)
            {
                DefaultPersons.システム.Say("既に{0}時を過ぎています。", hour);
                return false;
            }
            DateTime eta = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, hour, 0, 0);
            if (nextDay) eta = eta.AddDays(1.0);
            int minutes = (int)(eta - Flags.Now).TotalMinutes;
            DefaultPersons.システム.Say("{0}時まで、身体を休めました。", hour);
            bool result = await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now.AddMinutes(minutes));
            if (!result) State.GoTime(minutes);
            return true;
        }

        internal static DateTime GetDateOnly(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }

        /// <summary>
        /// 自動セーブを行います。(セーブ数0の場合は何もしません)
        /// </summary>
        public static async Task 自動セーブAsync()
        {
            await UI.Actions.AutoSaveFileAsync();
        }

        /// <summary>
        /// 一人称の私を返します。
        /// </summary>
        /// <returns>一人称の私</returns>
        public static string GetMyName()
        {
            string watasi = "私";
            foreach (var n in State.loadedModules)
            {
                if (n.GetMyPersonName != null)
                {
                    watasi = n.GetMyPersonName();
                }
            }
            return watasi;
        }

        /// <summary>
        /// セーブを行うファイル名のデフォルトを返します。
        /// </summary>
        /// <param name="isAuto">自動セーブならTrueです。</param>
        /// <returns>自動生成されたファイル名の候補です。</returns>
        public static string GenerateSuggestedFileName(bool isAuto)
        {
            string filename = GenerateSuggestedFileNameWithoutExt(isAuto);
            return Path.ChangeExtension(filename + ".ZZZ", General.FileExtention);
        }

        /// <summary>
        /// セーブを行うファイル名のデフォルトを返します。(拡張子抜き)
        /// </summary>
        /// <param name="isAuto">自動セーブならTrueです。</param>
        /// <returns>自動生成されたファイル名の候補です。</returns>
        public static string GenerateSuggestedFileNameWithoutExt(bool isAuto)
        {
            string filename = string.Format("{0} {1} {2}{3}",
                DateTime.Now.ToString("yyyyMMdd HHmmss"),
                GetMyName(), Flags.Now.ToString("MM月dd日"),
                isAuto ? ".auto" : "");
            return filename;
        }

        /// <summary>
        /// UI込みで標準的な購入を行います。
        /// </summary>
        /// <param name="shopID">ショップのIdです</param>
        /// <param name="getPrice">値段を取得するデリゲート型です。値段を加工しない場合、そのままの金額を返します。</param>
        /// <returns>常にtrueを返します。</returns>
        public static async Task<bool> StandardBuyAsync(string shopID, GetPriceInvoker getPrice)
        {
            Item[] items = ShopAndItemReleations.GetItems(shopID);
            await UI.Actions.shopBuyMenuAsync(items, getPrice);
            return true;
        }

        /// <summary>
        /// 標準的な購入を行うメニューを生成します
        /// </summary>
        /// <param name="list">メニューを追加すべきコレクションです</param>
        /// <param name="shopID">ショップのIdです</param>
        /// <param name="getPrice">値段を取得するデリゲート型です。値段を加工しない場合、そのままの金額を返します。</param>
        /// <param name="isOpen">店が開いているときはtrueを返すデリゲート型です</param>
        public static void StandardBuyMenu(List<SimpleMenuItem> list, string shopID, GetPriceInvoker getPrice, Func<bool> isOpen)
        {
            if (isOpen())
            {
                list.Add(new SimpleMenuItem("買い物", async () => await General.StandardBuyAsync(shopID, getPrice)));
            }
            else
            {
                DefaultPersons.システム.Say("店は閉まっています。");
            }
        }

        /// <summary>
        /// UI込みで標準的な売却を行います。
        /// </summary>
        /// <param name="shopID">ショップのIdです</param>
        /// <param name="getPrice">値段を取得するデリゲート型です。値段を加工しない場合、そのままの金額を返します。</param>
        /// <returns>常にtrueを返します。</returns>
        public static async Task<bool> StandardSellAsync(string shopID, GetPriceInvoker getPrice)
        {
            await UI.Actions.shopSellMenuAsync(getPrice);
            return true;
        }

        /// <summary>
        /// 標準的な売却を行うメニューを生成します
        /// </summary>
        /// <param name="list">メニューを追加すべきコレクションです</param>
        /// <param name="shopID">ショップのIdです</param>
        /// <param name="getPrice">値段を取得するデリゲート型です。値段を加工しない場合、そのままの金額を返します。</param>
        /// <param name="isOpen">店が開いているときはtrueを返すデリゲート型です</param>
        public static void StandardSellMenu(List<SimpleMenuItem> list, string shopID, GetPriceInvoker getPrice, Func<bool> isOpen)
        {
            if (isOpen())
            {
                list.Add(new SimpleMenuItem("売る", async () => await General.StandardSellAsync(shopID, getPrice)));
            }
            else
            {
                DefaultPersons.システム.Say("店は閉まっています。");
            }
        }

        /// <summary>
        /// 読み込まれて管理下にある全てのモジュールに対して指定のアクションを行います。
        /// </summary>
        /// <param name="c"></param>
        public static void CallAllModuleMethod(Func<Module, Dummy> c)
        {
            if (State.loadedModules == null) return;
            foreach (var m in State.loadedModules)
            {
                c(m);
            }
        }
        /// <summary>
        /// 読み込まれて管理下にある全てのモジュールに対して指定のアクションを行います。
        /// </summary>
        /// <param name="c"></param>
        public static async Task CallAllModuleMethodAsync(Func<Module, Task> c)
        {
            if (State.loadedModules == null) return;
            foreach (var m in State.loadedModules)
            {
                await c(m);
            }
        }

        /// <summary>
        /// 新ゲーム開始を読み込まれた全モジュールに通知します。
        /// このメソッドを明示的に呼ばないと通知されることはありません。
        /// (ロードやセーブは明示的に呼ばずとも通知されます)
        /// </summary>
        public static void NotifyNewGame()
        {
            ANGFLib.General.CallAllModuleMethod((m) => { m.OnNewGame(); return default; });
        }

        /// <summary>
        /// 現在の装備セットをコピーします。
        /// </summary>
        /// <returns>コピーされた装備セット</returns>
        public static EquipSet CopyEquipSet()
        {
            EquipSet old = new EquipSet();
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                old.AllItems[i] = Items.GetItemByNumber(Flags.Equip[i]);
            }
            return old;
        }

        /// <summary>
        /// 装備セットを書き戻します。
        /// </summary>
        /// <param name="set">書き戻す装備セット</param>
        public static void SetEquipSet(EquipSet set)
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                Flags.Equip[i] = set.AllItems[i].Id;
            }
        }

        /// <summary>
        /// レポートに地図情報を含める場合、このメソッドを呼び出すことができます。結果はx座標でソートされます。
        /// </summary>
        /// <param name="writer">出力先を指定します</param>
        /// <param name="forDebug">Trueであれば不可視でも全ての場所が報告されます。</param>
        public static void WriteReportMap(System.IO.TextWriter writer, bool forDebug)
        {
            foreach (var world in World.List.Values)
            {
                writer.Write("★ {0}の地図", world.HumanReadableName);
                if (forDebug) writer.Write(" (デバッグ版)");
                writer.WriteLine();
                writer.WriteLine();
                var query = from x in SimpleName<Place>.List.Values where world.Id == x.World && (x.ParentVisible || forDebug) && x.HasParentDistance orderby x.GetParentDistance().x * 2 + (x.HasDistance ? 0 : 1) select x;
                foreach (Place p in query)
                {
                    if (forDebug || p.Visible)
                    {
                        Position pos = p.GetDistance();
                        if (pos != null)
                        {
                            writer.Write("({0,7},{1,7}) ",
                                Coockers.MapLengthCoocker(pos.x),
                                Coockers.MapLengthCoocker(pos.y));
                        }
                        else
                        {
                            writer.Write("({0,7},{1,7}) ", "", "");
                        }
                        writer.Write(p.HumanReadableName);
                        if (forDebug)
                        {
                            writer.Write(p.Visible ? " Visible" : " Not Visible");
                        }
                        writer.WriteLine();
                    }
                }
                writer.WriteLine();
            }
            return;
        }

        class collectionWalkerItem
        {
            internal string Name, Subname, State, Owner, Id, OwnerModuleId;
        }

        private static string moduleIdToName(string moduleId)
        {
            var found = State.loadedModules.FirstOrDefault(c => c.Id == moduleId);
            if (found == null) return moduleId;
            return found.GetXmlModuleData().Name;
        }

        private static IEnumerable<collectionWalkerItem> collectionWalker(Collection collection, bool forDebug)
        {
            foreach (var collectionItem in collection.Collections)
            {
                if (collectionItem.GetRawSubItems != null || collectionItem.GetRawSubItems != null)
                {
                    foreach (var subitem in collectionItem.GetSubItems())
                    {
                        var state = State.HasCollection(collection.Id, collectionItem.Id, subitem.Id);
                        if (forDebug || state != State.CollectionState.None)
                        {
                            yield return new collectionWalkerItem() { Name = collectionItem.Name, Subname = subitem.Name, State = state.ToString(), Owner = moduleIdToName(subitem.OwnerModuleId), Id = subitem.Id, OwnerModuleId = subitem.OwnerModuleId };
                        }
                    }
                }
                else
                {
                    var state = State.HasCollection(collection.Id, collectionItem.Id, "");
                    if (forDebug || state != State.CollectionState.None)
                    {
                        yield return new collectionWalkerItem() { Name = collectionItem.Name, Subname = null, State = state.ToString(), Owner = moduleIdToName(collectionItem.OwnerModuleId), Id = collectionItem.Id, OwnerModuleId = collectionItem.OwnerModuleId };
                    }
                }
            }
        }

        /// <summary>
        /// 生活サイクルの基点時間を強制的に変更します
        /// </summary>
        /// <param name="hour"></param>
        public static void ForceChangeCycle(int hour)
        {
            DateTime 新今日の就寝時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, (hour + 16) % 24, 0, 0);
            if (新今日の就寝時刻 < Flags.Now)
            {
                新今日の就寝時刻.AddDays(1.0);
            }
            Flags.生活サイクル起点時間 = hour;
            State.今日の就寝時刻 = 新今日の就寝時刻;
        }

        /// <summary>
        /// C#で有効な名前にエンコードする
        /// たまたま同じ名前にエンコードされる可能性は否定できない
        /// </summary>
        /// <param name="src">元の名前</param>
        /// <returns>エンコードされた名前</returns>
        public static string ToCSKeyword(string src)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(src);
            return Convert.ToBase64String(bytes).Replace("+", "_P").Replace("/", "_S").Replace("=", "_E");
        }

        /// <summary>
        /// SuperTalkの#CSSに対応するメソッド名を得ます
        /// </summary>
        /// <param name="id">#CSSのID</param>
        /// <returns>メソッド名</returns>
        public static string GenerateProcName(string id)
        {
            return "proc" + ToCSKeyword(id);
        }

        /// <summary>
        /// 内部利用専用です
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static Place[] GetCandidatePlaceList(MyXmlDoc doc, System.Xml.Linq.XName elementName)
        {
            XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
            Dictionary<string, Place> list = new Dictionary<string, Place>();
            foreach (var n in doc.moduleEx.Descendants(elementName))
            {
                var id = n.Element(ns + "Id").Value;
                var name = n.Element(ns + "Name").Value;
                list.Add(id, new SimplePlace() { IdGetter = () => id, NameGetter = () => name });
            }
            foreach (var n in SimpleName<Place>.List.Values)
            {
                if (!list.Keys.Contains(n.Id))
                {
                    list.Add(n.Id, n);
                }
            }
            return list.Values.ToArray();
        }

        /// <summary>
        /// 内部利用専用です
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Person[] GetCandidatePersonList(MyXmlDoc doc)
        {
            XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
            Dictionary<string, Person> list = new Dictionary<string, Person>();
            foreach (var n in doc.moduleEx.Descendants(ns + "Person"))
            {
                var id = n.Element(ns + "Id").Value;
                var name = n.Element(ns + "Name").Value;
                list.Add(id, new SimplePerson(id, name, Sex.Female, null, null, null));
            }
            foreach (var n in SimpleName<Person>.List.Values)
            {
                if (!list.Keys.Contains(n.Id))
                {
                    list.Add(n.Id, n);
                }
            }
            return list.Values.ToArray();
        }


        /// <summary>
        /// レポートにスケジュール一覧を含めます。直接呼び出すべきではありません。
        /// </summary>
        /// <param name="writer">出力先を指定します</param>
        /// <param name="forDebug">Trueであれば不可視でも全ての場所が報告されます。</param>
        public static void WriteReportSchedules(System.IO.TextWriter writer, bool forDebug)
        {
            if (Schedule.List.Values.Count() == 0) return;
            writer.WriteLine("★ スケジュール");
            foreach (var schedule in Schedule.List.Values)
            {
                if (State.IsScheduleVisible(schedule.Id))
                {
                    writer.WriteLine($"・{schedule.HumanReadableName} {schedule.Description} ({schedule.StartTime}-{schedule.StartTime + schedule.Length})");
                }
                else if (forDebug)
                {
                    writer.WriteLine($"・{schedule.HumanReadableName} {schedule.Description}");
                }
            }
        }

        /// <summary>
        /// レポートにコレクション一覧を含めます。直接呼び出すべきではありません。
        /// </summary>
        /// <param name="writer">出力先を指定します</param>
        /// <param name="forDebug">Trueであれば不可視でも全ての場所が報告されます。</param>
        public static void WriteReportCollections(System.IO.TextWriter writer, bool forDebug)
        {
            foreach (var collection in SimpleName<Collection>.List.Values)
            {
                writer.Write("★ {0} (コレクション)", collection.HumanReadableName);
                if (forDebug) writer.Write(" (デバッグ版)");
                writer.WriteLine();
                writer.WriteLine();

                var query = from n in collectionWalker(collection, forDebug) orderby n.Owner select n;
                int count = 0;
                Dictionary<string, int> dic = new Dictionary<string, int>();
                foreach (var item in query)
                {
                    writer.WriteLine("{0} {1} [{2}] ({3})", item.Name, item.Subname, item.State, item.Owner);
                    count++;
                    if (dic.Keys.Contains(item.OwnerModuleId))
                        dic[item.OwnerModuleId]++;
                    else
                        dic.Add(item.OwnerModuleId, 1);
                }
                if (forDebug)
                {
                    foreach (var key in dic.Keys)
                    {
                        var mod = State.loadedModules.First(c => c.Id == key);
                        var xml = ModuleClassExtenderEx.GetAngfRuntimeXml(mod);
                        writer.WriteLine("{0}({1}) Items: {2}", (xml == null) ? "(???)" : xml.name, key, dic[key]);
                    }
                    writer.WriteLine("All Items: {0}", count);
                    writer.WriteLine("Top Level Items: {0}", collection.Collections.Length);
                }
                writer.WriteLine();
            }
        }

        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static void Write名前ID対応表(string filename)
        {
            using (var writer = XmlWriter.Create(filename))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("root");

                foreach (var n in State.loadedModules)
                {
                    writer.WriteStartElement("module");
                    writer.WriteAttributeString("ID", n.Id);
                    writer.WriteAttributeString("Name", n.GetXmlModuleData().Name);
                    writer.WriteEndElement();   // end of module
                }

                foreach (var collection in SimpleName<Collection>.List.Values)
                {
                    writer.WriteStartElement("collection");
                    writer.WriteAttributeString("ID", collection.Id);
                    writer.WriteAttributeString("Name", collection.HumanReadableName);
                    foreach (var item in collectionWalker(collection, true))
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("ID", item.Id);
                        writer.WriteElementString("Name", item.Name);
                        if (!string.IsNullOrWhiteSpace(item.Subname))
                        {
                            writer.WriteElementString("SubName", item.Subname);
                        }
                        writer.WriteElementString("Owner", item.Owner);
                        writer.WriteEndElement();   // end of item
                    }
                    writer.WriteEndElement();   // end of collection
                }
                writer.WriteEndElement();   // end of root
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// 移動可能か判定する
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool candMoveMenu(Place p)
        {
            // 距離を持っていない場所は対象外である
            if (!p.HasDistance) return false;
            // 距離を持っていても自分とParentを共有していると対象外である
            if (State.CurrentPlace.ParentTopID == p.ParentTopID) return false;
            // 対象である
            return true;
        }

        /// <summary>
        /// サブ移動可能か判定する
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool candSubMoveMenu(Place p)
        {
            // Parentが同じIdであると無条件に対象になる
            if (State.CurrentPlace.ParentTopID == p.ParentTopID) return true;
            // 距離を持っていても対象外とはいえない
            // ここまで来れば距離の有無に関係なく対象外
            return false;
        }

        /// <summary>
        /// MoveMenuに出そうな候補がある
        /// </summary>
        /// <param name="checker"></param>
        /// <param name="HowToMove"></param>
        /// <returns></returns>
        public static bool CanMove(Func<Place, bool> checker, HowToMoveInvoker HowToMove, bool isSub = false)
        {
            // サブ移動じゃないときだけ
            if (!isSub)
            {
                // 可視のリンクがあったら対象
                if (State.CurrentPlace.GetLinkedPlaceIds().Any(c => Place.List[c].Visible)) return true;
            }
            // 違うWorldは対象外
            // 同じ場所は対象外
            // 見えない場所は対象外
            return SimpleName<Place>.List.Values.AsParallel().Any(c => Flags.CurrentWorldId == c.World && Flags.CurrentPlaceId != c.Id && c.Visible && checker(c) && HowToMove(State.CurrentPlace, c).IsAvailable);
        }

        /// <summary>
        /// 移動メニュー
        /// </summary>
        /// <param name="cand"></param>
        /// <param name="HowToMove"></param>
        /// <returns></returns>
        public static async Task<bool> MoveMenuAsync(Func<Place, bool> cand, HowToMoveInvoker HowToMove, bool isSub)
        {
            List<SimpleMenuItem> menuList = new List<SimpleMenuItem>();
            bool isByLink = false;
            if (!isSub)
            {
                // 可視のリンクがあったら対象
                foreach (var p in State.CurrentPlace.GetLinkedPlaceIds().Select(c => Place.List[c]).Where(c => c.Visible))
                {
                    menuList.Add(new SimpleMenuItem(p.HumanReadableName, () => isByLink = true, p));
                }
            }

            // 処理の高速化のためにthresholdを越えるxyの差分は候補から落とす
            const int threshold = 10000; // 10000m=10km
            var query = Place.List.Values.Where(c => cand(c) && Flags.CurrentWorldId == c.World && Flags.CurrentPlaceId != c.Id && c.Visible).Where(c => Math.Abs(c.GetParentDistance().x - State.CurrentPlace.GetParentDistance().x) <= threshold).Where(c => Math.Abs(c.GetParentDistance().y - State.CurrentPlace.GetParentDistance().y) <= threshold).OrderBy(c => c.GetParentDistance().GetDistanceFromSquared(State.CurrentPlace.GetParentDistance()));

            foreach (var p in query)
            {
                MoveInfo moveInfo = HowToMove(State.CurrentPlace, p);
                if (moveInfo.IsAvailable) menuList.Add(new SimpleMenuItem(p.HumanReadableName + moveInfo.SupplyDescription, null, p));
            }
            int selection = await UI.SimpleMenuWithCancelAsync("どこに行こうかな?", menuList.ToArray());
            if (selection < 0) return false; // どこにも行かない

            string result = State.CurrentPlace.FatalLeaveConfim((Place)menuList[selection].UserParam);
            if (result != null)
            {
                DefaultPersons.独白.Say(result);
                return false;
            }

            return await ConfirmFashionAndGotoAsync((Place)menuList[selection].UserParam, isByLink);
        }

        /// <summary>
        /// ファッションチェックと移動
        /// </summary>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static async Task<bool> ConfirmFashionAndGotoAsync(Place dst, bool byLink)
        {
            string result = State.CurrentPlace.LeaveConfim(dst);
            if (result != null)
            {
                DefaultPersons.システム.Say(result);
                if (!await UI.YesNoMenuAsync("本当に移動しますか?", "YES", "NO")) return false; // どこにも行かない
            }
            if (byLink) await State.GoToMyLinkAsync(dst);
            else await State.GoToAsync(dst);
            return true;
        }

        /// <summary>
        /// 会話メニューの作成
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> TalkAsync()
        {
            List<SimpleMenuItem> list = new List<SimpleMenuItem>();
            var list2 = General.FindPersonsWithPlace(State.CurrentPlace.Id);
            foreach (var person in list2)
            {
                if (person is PersonWithPlace && ((PersonWithPlace)person).IsAvailable())
                {
                    var capturedPerson = person;
                    list.Add(new SimpleMenuItem(person.MyName, async () => { await ((PersonWithPlace)capturedPerson).TalkAsync(); return true; }));
                }
            }
            int index = await UI.SimpleMenuWithCancelAsync("誰と話そうか", list.ToArray());
            if (index < 0) return false;
            return true;
        }

        /// <summary>
        /// その場所のその人を探す
        /// </summary>
        /// <param name="placeID"></param>
        /// <returns></returns>
        public static Person[] FindPersonsWithPlace(string placeID)
        {
            return Person.List.Values.OfType<PersonWithPlace>().Where(c => c.PlaceID == placeID && c.IsAvailable()).ToArray();
        }

        /// <summary>
        /// ソートされた装備部位リスト
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EquipType> GetSortedQeuipTypes()
        {
            return from z in SimpleName<EquipType>.List.Values orderby z.Priority select z;
        }

        /// <summary>
        /// そのアイテムは所持数の全てを誰かが装備しているか?
        /// </summary>
        /// <param name="itemId">アイテム</param>
        /// <returns>trueなら全て装備済み</returns>
        public static bool IsAnyoneEquippedAllItems(string itemId, string targetPersonId = null)
        {
            int usedCont = 0;
            foreach (var partymember in Person.List.Values.OfType<IPartyMember>())
            {
                //　除外指定されたPersonの装備品はアカウントしない。
                if (partymember.GetPerson().Id == targetPersonId) continue;
                foreach (var targetItemId in partymember.GetEquippedItemIds())
                {
                    if (targetItemId == itemId)
                    {
                        usedCont++;
                        break;  // 1人につき1つカウントアップする (2つの場所を占めるアイテムがあるため)
                    }
                }
            }
            return State.GetItemCount(itemId) <= usedCont;
        }

        /// <summary>
        ///  装備アイテムの候補リスト
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static Item[] GetCandidateEquipItems(int targetType)
        {
            List<Item> list = new List<Item>();
            foreach (Item item in Items.GetItemList())
            {
                if (targetType < item.AvailableEquipMap.Length && item.AvailableEquipMap[targetType])
                    if (State.GetItemCount(item) > 0)   // それを今持っているか?
                        if (!IsAnyoneEquippedAllItems(item.Id)) // 誰も装備していない
                            list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// アイテム消費メニューのラベル文字列作成
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateItemConsumeLabelText(Item item)
        {
            if (item == null || item.IsItemNull) return "----";
            return string.Format("{0,2}個 {1} ({2}回使用)", State.GetItemCount(item), item.HumanReadableName, State.GetUsedCount(item));
        }

        /// <summary>
        /// アイテム装備メニューのラベル文字列作成
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateItemEquipLabelText(Item item)
        {
            if (item == null || item.IsItemNull) return "----";
            return string.Format("{0} ({1}回使用)", item.HumanReadableName, State.GetUsedCount(item));
        }

        /// <summary>
        /// マニフェストだけをロードする
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
#if MYBLAZORAPP
        public static Tuple<XDocument, Version> LoadManifestOnly(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
#pragma warning disable SYSLIB0018 // 型またはメンバーが旧型式です
            var assem = Assembly.ReflectionOnlyLoad(buffer);
#pragma warning restore SYSLIB0018 // 型またはメンバーが旧型式です
            foreach (var n in assem.GetManifestResourceNames())
            {
                if (n.ToLower().EndsWith(".angfruntime.xml"))
                {
                    using (var reader = new StreamReader(assem.GetManifestResourceStream(n)))
                    {
                        string str = reader.ReadToEnd();
                        if (str == null) return null;
                        return new Tuple<XDocument, Version>(XDocument.Parse(str), assem.GetName().Version);
                    }
                }
            }
            return null;
        }
#else
        public static Tuple<XDocument, Version> LoadManifestOnly(string filepath)
        {
            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            setup.ApplicationName = "ANGF" + Guid.NewGuid().ToString();
            setup.PrivateBinPath += ";" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //setup.PrivateBinPath = @"C:\xcodeplex\angf\ANGF\ANGFLib\bin\Release";
            AppDomain domain = AppDomain.CreateDomain("MyDomain", AppDomain.CurrentDomain.Evidence, setup);
            try
            {
                var sep = (Sep)domain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(Sep).FullName
                );
                sep.FileName = filepath;
                var str = sep.loadSub();
                if (str == null) return null;
                return new Tuple<XDocument, Version>(XDocument.Parse(str), sep.FileVersion);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
#endif

        /// <summary>
        /// ファイル名からファイル番号を得る　(WebPlayer用)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static int ExtractFileNumber(string filename)
        {
            int indexPlus1;
            int.TryParse(filename.Substring(4, 2), out indexPlus1);
            return indexPlus1 - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string CreateCssColorString(System.Drawing.Color color)
        {
            return string.Format("#{0,0:X2}{1,0:X2}{2,0:X2}", color.R, color.G, color.B);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="resourceNamee"></param>
        /// <returns></returns>
        public static string LoadEmbededResourceAsText(Assembly asm, string resourceNamee)
        {
            using (var reader = new StreamReader(asm.GetManifestResourceStream(resourceNamee)))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// ジャーナリング結果(全)
        /// </summary>
        public static string JounalResults { get; internal set; }

        public static IEnumerable<string> EnumJounalResults()
        {
            if (JounalResults == null) yield break;
            var all = new StringReader(JounalResults);
            for (; ; )
            {
                var s = all.ReadLine();
                if (s == null) break;
                yield return s;
            }
        }

        /// <summary>
        /// エラーを報告します。(実装はUI依存)
        /// 初期状態で入っているのは大ざっぱなデフォルト
        /// </summary>
        public static Func<string, Dummy> ReportError = (message) =>
        {
            System.Diagnostics.Trace.Fail(message);
            return default;
        };

        /// <summary>
        /// テスト(ジャーナリング)機能が有効である場合Trueです。
        /// </summary>
        [Obsolete]
        public static bool IsJournalingEnabled
        {
            get { return State.JournalingPlayer != null; }
        }
        /// <summary>
        /// 互換性のために残されているフィールドで何ら機能を持たない。
        /// 代わりにIsTestingModeExを使用すべき
        /// </summary>
        [Obsolete]
        public static bool IsTestingMode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Category"></param>
        /// <returns></returns>
        public static string GetExtentionByCategory(string Category)
        {
            if (Category == "SystemFile" || Category == "skip") return "bin";
            return General.FileExtention;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="usedCount"></param>
        public static void SoldNotifyAll(Item selectedItem, int usedCount)
        {
            if (selectedItem == null) return;
            foreach (var moduleEx in State.LoadedModulesEx)
            {
                var ar = moduleEx.QueryObjects<SoldNotify>();
                foreach (var item in ar)
                {
                    item.NotifyItemWasSold(selectedItem.Id, usedCount);
                }
            }
        }

        private static void confirmLastSleepDate()
        {
            var today = DateTime.Now.ToString(Constants.DateFormat);
            if (LastSleepDateYyyyMmDd == today) return;
            LastSleepDateYyyyMmDd = today;
            SystemFile.SetFlag("LastSleepDateCount", 0);
            SystemFile.SetFlag("EnabledSleepCount", Constants.FreeSleepCount);
        }

        /// <summary>
        /// 最後に就寝した「現実世界」の日付をyyyyMMdd形式で文字列化したもの
        /// </summary>
        public static string LastSleepDateYyyyMmDd
        {
            get => SystemFile.GetFlagString("LastSleepDateYyyyMmDd");
            set => SystemFile.SetFlagString("LastSleepDateYyyyMmDd", value);
        }

        /// <summary>
        /// 【最後に就寝した「現実世界」の日付】で就寝した回数
        /// 日付が変わっていれば値は無視して0に戻ったものとして扱う
        /// </summary>
        public static int GetLastSleepDateCount()
        {
            confirmLastSleepDate();
            return SystemFile.GetFlag("LastSleepDateCount");
        }

        /// <summary>
        /// 【最後に就寝した「現実世界」の日付】で就寝した回数を加算する
        /// </summary>
        public static void IncrementLastSleepDateCount()
        {
            SystemFile.SetFlag("LastSleepDateCount", GetLastSleepDateCount() + 1);

        }

        /// <summary>
        /// 現実世界の1日で許される就寝回数
        /// この値はスターを追加購入することで更新できる
        /// 日付が変わっていれば値は無視して規定値に戻ったものとして扱う
        /// </summary>
        public static int GetEnabledSleepCount()
        {
            confirmLastSleepDate();
            return SystemFile.GetFlag("EnabledSleepCount");
        }
        /// <summary>
        /// 現実世界の1日で許される就寝回数を加算する
        /// </summary>
        public static void IncrementEnabledSleepCount()
        {
            SystemFile.SetFlag("EnabledSleepCount", GetEnabledSleepCount() + 1);
        }


        /// <summary>
        /// ANGFLibのアセンブリを返す
        /// </summary>
        /// <returns>ANGFLibのアセンブリ</returns>
        public static Assembly GetAngfLibAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// 強制時間移動
        /// </summary>
        /// <param name="newDate">新しい時間(ノーチェック)</param>
        public static async Task TimeWarpAsync(DateTime newDate)
        {
            Flags.Now = newDate;
            State.今日の起床時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, State.起床時刻, 0, 0);
            State.今日の就寝時刻 = State.今日の起床時刻.AddHours(16);
            //JournalingWriter.WriteEx(State.SeekModule(this), "TIME", Flags.Now.ToString(Constants.DateFormat));
            Flags.Now = Flags.Now.AddHours(State.起床時刻);
            State.今日の起床時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, State.起床時刻, 0, 0);
            State.今日の就寝時刻 = State.今日の起床時刻.AddHours(16);
            DefaultPersons.システム.Say("システムの整合性確保のために次の朝に行きます。");
            await State.GoNextDayMorningAsync();
            UI.Actions.ResetGameStatus();
        }

        public static EquipSet 装備品情報のコピー()
        {
            EquipSet old = new EquipSet();
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                old.AllItems[i] = Items.GetItemByNumber(Flags.Equip[i]);
            }
            return old;
        }

        public static bool IsEquippableItem(int equipOrder, string personId, string ItemId)
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var r = item.QueryObjects<IEquipChecker>();
                if (r.Length > 0) return r[0].IsEquippableItem(equipOrder, personId, ItemId);
            }
            return true;
        }

        // リンク移動時の消費時間をモジュールに問い合わせる
        internal static int リンク移動時間計算(Place currentPlace, Place distPlace)
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var r = item.QueryObjects<ILinkMoveCalc>();
                if (r.Length > 0) return r[0].リンク移動時間計算(currentPlace, distPlace);
            }
            return Constants.DefaultLinkMoveMin;
        }

        public static IEnumerable<RoadPlace> RoadPlaceGenerator(string baseName, string baseId, string fromId, string toId, int count, Func<string, string, string, string, RoadPlace> creater)
        {
            for (int i = 1; i <= count; i++)
            {
                string from = baseId + "_" + (i - 1).ToString();
                if (i == 1) from = fromId;
                string to = baseId + "_" + (i + 1).ToString();
                if (i == count) to = toId;
                yield return creater(baseName + i.ToString(), baseId + "_" + i.ToString(), from, to);
            }
        }

        public static IEnumerable<RoadPlace> GetAllRoads(Type t)
        {
            foreach (var item in t.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var target = item.GetValue(null) as RoadPlace[];
                if (target == null) continue;
                foreach (var item2 in target)
                {
                    yield return item2;
                }
            }
        }

        public static bool IsExpandEquipRequied()
        {
            return EquipType.List.Values.Any(c => c.IsVisibleIfExpanded != false);
        }

        public static Func<EquipSet, string> EquipCustomValudation(string personId)
        {
            foreach (var item in State.loadedModules)
            {
                var r = item.GetEquipCustomValidator(personId);
                if (r != null) return r;
            }
            return (eq) => null;    // always success
        }

        public static bool IsStatusHide()
        {
            var place = State.CurrentPlace;
            if (place == null) return true;
            return place.IsStatusHide;
        }

        public static string GetTotalReport()
        {
            StringWriter writer = new StringWriter();
            foreach (var n in State.loadedModules)
            {
                n.WriteReport(writer, SystemFile.IsDebugMode);
            }
            General.WriteReportCollections(writer, SystemFile.IsDebugMode);
            General.WriteReportSchedules(writer, SystemFile.IsDebugMode);
            writer.Close();
            return writer.ToString();
        }

        // Batch Testing Support
        public static async Task<bool> JournalingFileEnqueueIfBatchTestingRequestedAsync(Assembly assembly, string filename)
        {
            if (await wangflib.BatchTest.BatchTestingForEachTitle.IsBatchTestingAsync() && !UI.Actions.isJournalFilePlaying())
            {
                JournalingFileEnqueue(assembly, filename);
                return true;
            }
            return false;
        }
        public static void JournalingFileEnqueue(Assembly assembly, string filename)
        {
            JournalPlaybackQueue.Enqueue(new JournalingInputDescripter(assembly, filename));
        }

        /// <summary>
        /// 指定された埋め込みリソースを読み込んでスプラッシュ画像として表示する
        /// </summary>
        /// <param name="assembly">対象アセンブリ</param>
        /// <param name="resourcePath">対象リソースのパス(モジュール名.ファイル名)</param>
        /// <returns>読み込めたらtrue</returns>
        public static bool CommonSplashLoader(Assembly assembly, string resourcePath)
        {
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream != null)
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    UI.Actions.SetPictureUrl("data:image/jpg;base64," + Convert.ToBase64String(buffer));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// BlazorWebAssemblyで実行中か判定する
        /// </summary>
        /// <returns>BlazorWebAssemblyで実行中である</returns>
        public static bool IsBlazorWebAssembly()
        {
            return RuntimeInformation.OSDescription == "Browser";
        }

        /// <summary>
        /// 完全なダミーメンバー。何を書き込んでも良い
        /// 何かを読み出せると期待してはいけない
        /// </summary>
        public static object DummyOfDummy = null;

        /// <summary>
        /// Azureの基本インスタンスで実行中でかを判定する
        /// </summary>
        /// <remarks>
        /// AzureのWANGF基本インスタンスのHomeにPOSTリクエストを投げることでSQLサーバに
        /// SystemFileのイメージを格納するので、このホスト以外に投げることは基本的に意味がない
        /// またソースオリジン(CORS)の問題があるので、他からここへは投げられない
        /// だから、まさにこのURLで実行している時以外はネット送信できないのである。
        /// </remarks>
        /// <returns>Azureの基本インスタンスで実行中</returns>
        public static bool IsInAzure() => UI.Actions.GetUri().ToLower().Contains("wangf.azurewebsites.net");

        /// <summary>
        /// データ保存の共用ディレクトリのルートを得る (Maui only)
        /// </summary>
        /// <returns>ディレクトリ</returns>
        public static string GetDataRootDirectory()
        {
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".WANGF");
            if (Directory.Exists(p)) return p;
            Directory.CreateDirectory(p);   // create it if not exist
            var di = new DirectoryInfo(p);
            // make it hidden when it was created
            di.Attributes |= FileAttributes.Hidden;
            return p;
        }

        /// <summary>
        /// ProgramData以下の共用ディレクトリのルートを得る (Maui only)
        /// </summary>
        /// <returns>ディレクトリ</returns>
        public static string GetCommonRootDirectory()
        {
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "autumn", "WANGF");
            Directory.CreateDirectory(p);   // create it if not exist
            return p;
        }

        /// <summary>
        /// ジャーナリングファイルの共用ディレクトリを得る (Maui only)
        /// </summary>
        /// <returns></returns>
        internal static string GetJournalingDirectory()
        {
            var p = Path.Combine(GetDataRootDirectory(), "Journaling");
            Directory.CreateDirectory(p);
            return p;
        }

        public static Func<Task> StartJournalingPlaybackAsync { get; set; }/* DIABLE ASYNC WARN */

        /// <summary>
        /// ジャーナリングのインクルードで探索するアセンブリを指定する
        /// </summary>
        public static Assembly TestingAssembly { get; set; } = null;

        public static Func<Task> CallToRestoreActionSetAsync;/* DIABLE ASYNC WARN */
        public static Func<string, Task> CallToTellAssertionFailedAsync;/* DIABLE ASYNC WARN */
        public static Func<string, Task> CallToProgressStatusAsync;/* DIABLE ASYNC WARN */
    }
}
