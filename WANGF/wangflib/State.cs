using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using ANGFLib;

using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;

namespace ANGFLib
{
    /// <summary>
    /// 変化する情報をまとめて扱います。フラグ以外でロードとセーブの対象です。
    /// </summary>
    public class State
    {
        /// <summary>
        /// Webプレイヤーで稼働中ならtrue
        /// </summary>
        //public static bool IsInWebPlayer { get; set; }

        /// <summary>
        /// プラットフォーム名
        /// </summary>
        public static string PlatformName { get; set; }

        /// <summary>
        /// ログインしているユーザーのID。Webプレイヤーでのみ使用される、
        /// デスクトップまたはログインしていない場合はnullのまま
        /// </summary>
        public static string UserId { get; set; }

        /// <summary>
        /// ゲストならtrue
        /// </summary>
        public static bool IsGuestAccess => string.IsNullOrWhiteSpace(State.UserId);

        /// <summary>
        /// 現在位置と読み書きします。
        /// </summary>
        public static Place CurrentPlace
        {
            get
            {
                if (Flags.CurrentPlaceId == null || Flags.CurrentPlaceId == "") return Places.PlaceNull;
                return SimpleName<Place>.List[Flags.CurrentPlaceId];
            }
            set
            {
                Flags.CurrentPlaceId = value.Id;
            }
        }

        private static string diver81
        {
            get
            {
                byte[] milk = { (byte)'S', (byte)'A', (byte)'Y', (byte)'0', 
                    (byte)'C', (byte)'H', (byte)'A', (byte)'N', (byte)'3' };
                char[] milk2 = new char[milk.Length];
                for (int i = 0; i < milk.Length; i++)
                {
                    milk2[i] = (char)milk[i];
                }
                return new string(milk2);
            }
        }

        /// <summary>
        /// 強くてニューゲームのための初期化を行います。
        /// </summary>
        public static void ClearFor強くてニューゲーム()
        {
            今日の起床時刻 = DateTime.MinValue;
            今日の就寝時刻 = DateTime.MinValue;
        }

        /// <summary>
        /// 全ての情報をクリアし、新しいゲームに備えます。
        /// </summary>
        /// <param name="enableProc">指定フィールドの初期化をするか否かを判定します</param>
        /// <exception cref="ApplicationException">使用できない型が検出されました。</exception>
        public static void Clear(Func<FieldInfo,bool> enableProc)
        {
            // CurrentPlaceIdは一時待避の必要あり
            string id = Flags.CurrentPlaceId;
            AutoCollect.WalkAll((field, modid, name) =>
                {
                    if (enableProc(field))
                    {
                        var n = field.FieldType;
                        if (n == typeof(object)) ((SimpleDynamicObject)field.GetValue(null)).Data.Clear();
                        else if (n == typeof(FlagCollection<int>)) ((FlagCollection<int>)field.GetValue(null)).Clear();
                        else if (n == typeof(FlagCollection<bool>)) ((FlagCollection<bool>)field.GetValue(null)).Clear();
                        else if (n == typeof(FlagCollection<string>)) ((FlagCollection<string>)field.GetValue(null)).Clear();
                        else if (n == typeof(int)) field.SetValue(null, 0);
                        else if (n == typeof(bool)) field.SetValue(null, false);
                        else if (n == typeof(string)) field.SetValue(null, null);
                        else throw new ApplicationException("型" + n.FullName + "は自動ロードセーブに使用できません。");
                    }
                    return default;
                });
            equipSetItems.Clear();
            SetEquipSet(State.装備なし, new EquipSet());
            ClearFor強くてニューゲーム();
            Flags.CurrentPlaceId = id;
        }
        /// <summary>
        /// 全ての情報をクリアし、新しいゲームに備えます。
        /// </summary>
        public static void Clear()
        {
            Clear((field) => true);
        }

        /// <summary>
        /// サービス用です。引数と返却値の無いデリゲートを提供します。
        /// </summary>
        delegate void MyMethodInvoker();

        /// <summary>
        /// 指定されたPlaceの場所へ移動します。
        /// Worldが現在Worldと違う場合はWorldも移動します
        /// </summary>
        /// <param name="distPlace">移動先の場所です。</param>
        public static async Task WarpToAsync(Place distPlace)
        {
            if (distPlace.World == Flags.CurrentPlaceId)
                await goToAsync(distPlace, () => { });
            else
                await goToAsync(distPlace, () => {
                    Flags.CurrentWorldId = distPlace.World;
                });
            //JournalingWriter.WriteComment("WarpTo " + distPlace.HumanReadableName);
        }
        /// <summary>
        /// 指定されたIdの場所へ移動します。
        /// </summary>
        /// <param name="id">移動先の場所です。</param>
        public static async Task WarpToAsync(string id)
        {
            if (id == null || id == "")
            {
                Flags.CurrentPlaceId = "";
                return;
            }
            Place distPlace = SimpleName<Place>.List[id];
            await goToAsync(distPlace, () => { });
            //JournalingWriter.WriteComment("WarpTo " + distPlace.HumanReadableName);
        }
        /// <summary>
        /// 指定されたIdの場所へ移動します。
        /// </summary>
        /// <param name="worldid">移動先のワールドです。</param>
        /// <param name="placeid">移動先の場所です。</param>
        public static async Task WarpToAsync(string worldid, string placeid)
        {
            if (placeid == null || placeid == "")
            {
                Flags.CurrentPlaceId = "";
                return;
            }
            Place distPlace = SimpleName<Place>.List[placeid];
            await goToAsync(distPlace, () => {
                Flags.CurrentWorldId = worldid;
            });
            //Flags.CurrentPlaceId = placeid;
#if COMMENT_TO_JOURNAL_WARP
            Place distPlace = SimpleName<Place>.List[placeid];
            JournalingWriter.WriteComment("WarpTo " + distPlace.HumanReadableName + " in world " + worldid);
#endif
        }

        /// <summary>
        /// 一切の処理を抜きにして現在位置を更新します
        /// </summary>
        /// <param name="distPlace">新しい現在位置です。</param>
        public static void SetPlaceForSimulator(Place distPlace)
        {
            CurrentPlace = distPlace;
        }

        /// <summary>
        /// 指定されたIdの場所へリンクで移動しますが、所要時間を計算してゲーム内現在時刻に加算します。
        /// </summary>
        /// <param name="distPlace"></param>
        public static async Task GoToMyLinkAsync(Place distPlace)
        {
            await goToAsync(distPlace,
                ()=>
                {
                    State.GoTime(General.リンク移動時間計算(State.CurrentPlace, distPlace), false, false);
                });
        }
        /// <summary>
        /// 指定されたIdの場所へ移動しますが、所要時間を計算してゲーム内現在時刻に加算します。
        /// </summary>
        /// <param name="distPlace"></param>
        public static async Task GoToAsync(Place distPlace)
        {
            await goToAsync(distPlace,
                ()=>
                {
                    MoveInfo info = Moves.HowToMove(State.CurrentPlace, distPlace);
                    State.GoTime((int)info.TimeToWillGo.TotalMinutes, false, false);
                });
            //JournalingWriter.WriteComment("GoTo " + distPlace.HumanReadableName);
        }
        private static async Task goToAsync(Place distPlace, MyMethodInvoker 移動時間計算)
        {
            if (CurrentPlace == distPlace) return;
            await CurrentPlace.OnLeaveingAsync();
            移動時間計算();
            foreach (var item in State.loadedModules)
            {
                bool r = await item.OnAfterMoveAsync(CurrentPlace.Id, distPlace.Id);
                if (r == false) break;
            }
            CurrentPlace = distPlace;
            await CurrentPlace.OnEnteringAsync();
        }

        /// <summary>
        /// プログラムの実行を中断させる
        /// (currentPlaceがPlaceNullオブジェクトの時
        /// メインプログラムのメインループは終了しなければならない)
        /// </summary>
        public static void Terminate()
        {
            CurrentPlace = Places.PlaceNull;
        }

        /// <summary>
        /// 指定アイテムの所有数を返します。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定します。</param>
        /// <returns>現在所有している数です</returns>
        public static int GetItemCount(Item item)
        {
            return Flags.アイテム所有数フラグ群[item.Id.ToString()];
        }

        /// <summary>
        /// 指定アイテムの所有数を返します。
        /// </summary>
        /// <param name="id">対象となるアイテムのidを指定します。</param>
        /// <returns>現在所有している数です</returns>
        public static int GetItemCount(string id)
        {
            return Flags.アイテム所有数フラグ群[id];
        }

        /// <summary>
        /// 現在所有しているアイテムの個数を変更します。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定します。</param>
        /// <param name="count">新しい所有している数です。</param>
        public static void SetItemCount(Item item, int count)
        {
            Flags.アイテム所有数フラグ群[item.Id.ToString()] = count;
        }

        /// <summary>
        /// 現在所有しているアイテムの個数を変更します。
        /// </summary>
        /// <param name="id">対象となるアイテムのidを指定します。</param>
        /// <param name="count">新しい所有している数です。</param>
        public static void SetItemCount(string id, int count)
        {
            Flags.アイテム所有数フラグ群[id] = count;
        }

        /// <summary>
        /// アイテムの使用回数を返します。使用回数の意味は応用ソフト依存です。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定します。</param>
        /// <returns>アイテムの使用回数です。</returns>
        public static int GetUsedCount(Item item)
        {
            return Flags.アイテム使用回数フラグ群[item.Id.ToString()];
        }

        /// <summary>
        /// アイテムの使用回数を返します。使用回数の意味は応用ソフト依存です。
        /// </summary>
        /// <param name="id">対象となるアイテムを指定しています。</param>
        /// <returns>アイテムの使用回数です。</returns>
        public static int GetUsedCount(string id)
        {
            return Flags.アイテム使用回数フラグ群[id];
        }

        /// <summary>
        /// アイテムの使用回数を設定します。使用回数の意味は応用ソフト依存です。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定しています。</param>
        /// <param name="count">新しいアイテムの使用回数です。</param>
        public static void SetUsedCount(Item item, int count)
        {
            Flags.アイテム使用回数フラグ群[item.Id.ToString()] = count;
        }

        /// <summary>
        /// アイテムの使用回数を加算します。使用回数の意味は応用ソフト依存です。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定しています。</param>
        /// <param name="count">加算する使用回数です。</param>
        public static void AddUsedCount(Item item, int count)
        {
            if (item.IsItemNull) return;
            Flags.アイテム使用回数フラグ群[item.Id.ToString()] += count;
        }

        /// <summary>
        /// アイテムを入手した場合に使用します。
        /// 指定アイテムの所有数に1を加算し、使用回数を0にします。
        /// 最大数を超えた場合は何もしません。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定しています。</param>
        public static void GetItem(Item item)
        {
            // 既に最大数持っていたら、これ以上は持てない
            if (GetItemCount(item) >= item.Max) return;
            SetItemCount(item, GetItemCount(item) + 1);
            SetUsedCount(item, 0);
        }

        /// <summary>
        /// アイテムを失った場合に使用します。
        /// 指定アイテムの所有数から1を引き、残った個数が0なら使用回数を0にします。
        /// もともと0個以下の場合は何もしません。
        /// </summary>
        /// <param name="item">対象となるアイテムを指定しています。</param>
        public static void LostItem(Item item)
        {
            if (GetItemCount(item) <= 0) return;
            SetItemCount(item, GetItemCount(item) - 1);
            if (GetItemCount(item) == 0)
            {
                SetUsedCount(item, 0);	// もう持っていないので使用カウントに意味はない
            }
        }

        private static int getIntValue(XmlNode doc, string xpath, int defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            int result = 0;
            int.TryParse(node.InnerText, out result);   // パースできない場合は0にしておく
            return result;
        }

        private static string getStringValue(XmlNode doc, string xpath, string defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            return node.InnerText;
        }

        private static DateTime getDateTimeValue(XmlNode doc, string xpath, DateTime defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            DateTime resultDate;
            if (DateTime.TryParseExact(node.InnerText,
                Constants.DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out resultDate))
            {
                return resultDate;
            }
            return defaultValue;
        }

        private static Item getItemValue(XmlNode doc, string xpath, Item defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;

            try
            {
                return Items.GetItemByNumber(node.InnerText);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private static void loadCollection<T>(XmlDocument doc, System.Reflection.FieldInfo field, string xmlName, Func<string, T> conversion)
        {
            var c = ((FlagCollection<T>)field.GetValue(null));
            c.Clear();
            foreach (XmlNode node in doc.SelectNodes("//f[@n='" + xmlName + "']/v"))
            {
                string m = node.Attributes["n"].Value;
                c[m] = conversion(node.InnerText);
            }
        }

        private static void loadDynamicCollection(XmlDocument doc, System.Reflection.FieldInfo field, string xmlName)
        {
            var c = ((SimpleDynamicObject)field.GetValue(null));
            c.Data.Clear();
            foreach (XmlNode node in doc.SelectNodes("//f[@n='" + xmlName + "']/v"))
            {
                string m = node.Attributes["n"].Value;  // name (key)
                string t = node.Attributes["t"].Value;  // type (int/string/bool)
                switch (t)
                {
                    case "int":
                        int v;
                        int.TryParse(node.InnerText, out v);
                        c.Data[m] = v;
                        break;
                    case "string":
                        c.Data[m] = node.InnerText;
                        break;
                    case "bool":
                        bool b;
                        bool.TryParse(node.InnerText, out b);
                        c.Data[m] = b;
                        break;
                }
            }
        }

        /// <summary>
        /// ファイルの読み込みを行います。
        /// </summary>
        /// <param name="filename">ファイル名です。</param>
        /// <param name="category">カテゴリ名です。</param>
        public static async Task<string> LoadAsync(string category, string filename)
        {
            byte[] fileImage;
            byte[] rawFileImage = await UI.Actions.LoadFileAsync(category, filename);
            if (rawFileImage == null || rawFileImage.Length == 0) return null;

            using (var readStream = new MemoryStream(rawFileImage))
            {
                byte[] magicHeader = new byte[4];
                int readBytes = readStream.Read(magicHeader, 0, magicHeader.Length);
                for (int i = 0; i < 4; i++)
                {
                    if (magicHeader[i] != Constants.FileMagicHeader[i] || i >= readBytes)
                    {
                        throw new ApplicationException("読み込んだファイルはこのプログラムでは扱うことができません。");
                    }
                }
                // ファイルのサイズが4バイトに満たない場合、例外で既に弾かれているはずである
                System.Diagnostics.Debug.Assert(readStream.Length >= 4);
                fileImage = new byte[readStream.Length - magicHeader.Length];
                readStream.Read(fileImage, 0, fileImage.Length);
            }
        
            string decriptedString = EncryptUtil.DecryptString(fileImage, diver81);

            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(decriptedString));

            General.CallAllModuleMethod((m) => { m.OnLoadStart(doc); return default; });

            // フラグの読み込み
            AutoCollect.WalkAll((field, id, name) =>
                {
                    string xmlName = id + "_" + name;
                    if (field.FieldType == typeof(object))    // it means DynamicObjectFlagAttribute with dynamic
                    {
                        loadDynamicCollection(doc, field, xmlName);
                    }
                    if (field.FieldType == typeof(FlagCollection<string>))
                    {
                        loadCollection<string>(doc, field, xmlName, (x) => x);
                    }
                    if (field.FieldType == typeof(FlagCollection<int>))
                    {
                        loadCollection<int>(doc, field, xmlName, (x) =>
                        {
                            int r;
                            if (!int.TryParse(x, out r)) r = 0;
                            return r;
                        });
                    }
                    if (field.FieldType == typeof(FlagCollection<bool>))
                    {
                        loadCollection<bool>(doc, field, xmlName, (x) =>
                        {
                            int r;
                            if (!int.TryParse(x, out r)) r = 0;
                            return r != 0;
                        });
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(null, getStringValue(doc, "//f[@n='" + xmlName + "']", null));
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        field.SetValue(null, getIntValue(doc, "//f[@n='" + xmlName + "']", 0));
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(null, getIntValue(doc, "//f[@n='" + xmlName + "']", 0) != 0);
                    }
                    return default;
                });

            // Worldの無い旧バージョンで書いたデータを読み込んだ場合でも互換を取る
            if (string.IsNullOrWhiteSpace(Flags.CurrentWorldId)) Flags.CurrentWorldId = Constants.DefaultWordId;

            // 日付時刻のNowとの同期
            //Flags.ResetNow();

            // 今日の起床時刻
            今日の起床時刻 = getDateTimeValue(doc, "//wakeup", DateTime.MinValue);
            今日の就寝時刻 = getDateTimeValue(doc, "//sleep", DateTime.MinValue);

            // スケジュールの可変日付時刻
            // コーディネート
            equipSetItems.Clear();
            foreach (XmlNode node in doc.SelectNodes("//coordinate"))
            {
                EquipListItem item = new EquipListItem();
                item.Name = getStringValue(node, "./name", "(no name)");
                item.LastRefered = getDateTimeValue(node, "./lastRefered", DateTime.MinValue);
                item.Set = new EquipSet();
                for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
                {
                    item.Set.AllItems[i] = getItemValue(node, "./eq" + i.ToString(), Items.ItemNull);
                }
                equipSetItems.Add(item);
            }

            // ここに続きを書く

            General.CallAllModuleMethod((m) => { m.OnLoadEnd(doc); return default; });
            return filename;
        }

        private static void writeFlagString(XmlTextWriter writer, string name, string subname, string val)
        {
            writer.WriteStartElement("f");
            writer.WriteAttributeString("n", name);
            if (subname != null)
            {
                writer.WriteStartElement("v");
                writer.WriteAttributeString("n", subname);
            }
            writer.WriteString(val);
            if (subname != null)
            {
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void writeFlagDynamic(XmlTextWriter writer, string name, string subname, object val)
        {
            writer.WriteStartElement("f");
            writer.WriteAttributeString("n", name);
            if (subname != null)
            {
                writer.WriteStartElement("v");
                writer.WriteAttributeString("n", subname);
                string typename = null;
                if (val is string) typename = "string";
                else if (val is int) typename = "int";
                else if (val is bool) typename = "bool";
                else throw new ApplicationException("ダイナミックなフラグでサポートされていない型です。" + val.GetType().FullName + "\r\n型はbool, int stringのいずれかである必要があります。");
                writer.WriteAttributeString("t", typename);
            }
            writer.WriteString(val.ToString());
            if (subname != null)
            {
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// ファイルのセーブを行います。
        /// </summary>
        /// <param name="Category">カテゴリです。</param>
        /// <param name="filename">ファイル名です。</param>
        public static async Task<string> SaveAsync(string Category, string filename)
        {
            StringWriter sWriter = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sWriter);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("states");
                General.CallAllModuleMethod((m) => { m.OnSaveStart(writer); return default; });

                AutoCollect.WalkAll((field, id, name) =>
                    {
                        string xmlName = id + "_" + name;
                        if (field.FieldType == typeof(object))    // it means DynamicObjectFlagAttribute with dynamic
                        {
                            var c = (SimpleDynamicObject)field.GetValue(null);
                            foreach (var pair in c.Data)
                            {
                                writeFlagDynamic(writer, xmlName, pair.Key, pair.Value);
                            }
                        }
                        if (field.FieldType == typeof(FlagCollection<string>))
                        {
                            var c = ((FlagCollection<string>)field.GetValue(null));
                            foreach (var m in c.Keys)
                            {
                                writeFlagString(writer, xmlName, m, c[m]);
                            }
                        }
                        if (field.FieldType == typeof(FlagCollection<int>))
                        {
                            var c = ((FlagCollection<int>)field.GetValue(null));
                            foreach (var m in c.Keys)
                            {
                                writeFlagString(writer, xmlName, m, c[m].ToString());
                            }
                        }
                        if (field.FieldType == typeof(FlagCollection<bool>))
                        {
                            var c = ((FlagCollection<bool>)field.GetValue(null));
                            foreach (var m in c.Keys)
                            {
                                writeFlagString(writer, xmlName, m, c[m] ? "1" : "0");
                            }
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            writeFlagString(writer, xmlName, null, (string)field.GetValue(null));
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            writeFlagString(writer, xmlName, null, ((int)field.GetValue(null)).ToString());
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            writeFlagString(writer, xmlName, null, (((bool)field.GetValue(null))) ? "1" : "0");
                        }
                        return default;
                    });

                // 今日の起床時刻
                writer.WriteElementString("wakeup", 今日の起床時刻.ToString(Constants.DateTimeFormat));
                writer.WriteElementString("sleep", 今日の就寝時刻.ToString(Constants.DateTimeFormat));

                // コーディネート
                foreach (EquipListItem item in equipSetItems)
                {
                    writer.WriteStartElement("coordinate");
                    writer.WriteElementString("name", item.Name);
                    writer.WriteElementString("lastRefered", item.LastRefered.ToString(Constants.DateTimeFormat));
                    for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
                    {
                        writer.WriteElementString("eq" + i.ToString(), item.Set.AllItems[i].Id);
                    }
                    writer.WriteEndElement();
                }

                // ここに続きを書く

                General.CallAllModuleMethod((m) => { m.OnSaveEnd(writer); return default; });
            }
            finally
            {
                writer.Close();
            }
            byte[] fileImage = EncryptUtil.EncryptString(sWriter.ToString(), diver81);
            var memoryStream = new MemoryStream();
            using (Stream writeStream = memoryStream)
            {
                writeStream.Write(Constants.FileMagicHeader, 0, Constants.FileMagicHeader.Length);
                writeStream.Write(fileImage, 0, fileImage.Length);
            }
            string result = await UI.Actions.SaveFileAsync(Category, filename, memoryStream.ToArray());
            if (result == null) return null;
            await SystemFile.SaveAsync();
            return result;
        }

        /// <summary>
        /// 起床時刻です。時のみで示されます。
        /// </summary>
        public static int 起床時刻
        {
            get { return Flags.生活サイクル起点時間; }
        }

        /// <summary>
        /// 起床予定時刻です。時のみで示されます。
        /// </summary>
        public static int 就寝時刻
        {
            get { return (起床時刻 + 16) % 24; }
        }

        /// <summary>
        /// 今日の起床時刻です。
        /// </summary>
        public static DateTime 今日の起床時刻;
        /// <summary>
        /// 今日の就寝予定時刻です。
        /// </summary>
        public static DateTime 今日の就寝時刻;

        private static void 起床()
        {
            今日の起床時刻 = Flags.Now;
            今日の就寝時刻 = 今日の起床時刻.AddHours(16);
            Flags.起床回数++;
        }

        /// <summary>
        /// 明日の起床時刻です。OnBeforeSleepAsyncで参照するためのものです。
        /// </summary>
        public static DateTime NextMorning;

        /// <summary>
        /// 時間を進める。時間を変更する全ての作業はこのメソッドを経由して行う必要がある
        /// </summary>
        /// <param name="minutes">進める時間(分単位)</param>
        /// <param name="hasOtherEyes">他人の目があるか</param>
        /// <param name="enableEvent">このメソッド中でイベントの発動をさせるか</param>
        public static void GoTime(int minutes, bool hasOtherEyes, bool enableEvent)
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var ar = item.QueryObjects<CustomGoTimeProcessor>();
                if (ar.Length > 0)
                {
                    // if found custom GoTime, call it and return
                    ar[0].GoTime(minutes, hasOtherEyes, enableEvent);
                    return;
                }
            }

            DateTime endTime = Flags.Now.AddMinutes(minutes);
            foreach (var n in State.loadedModules)
            {
                n.OnGoTime(Flags.Now, endTime);
            }
            Flags.Now = endTime;
            foreach (var n in State.loadedModules)
            {
                n.OnAfterGoTime(Flags.Now, endTime);
            }
        }

        /// <summary>
        /// 指定分だけ時間を進めます。
        /// </summary>
        /// <param name="minutes">進める時間を分で指定します。</param>
        public static void GoTime(int minutes)
        {
            GoTime(minutes, false, true);
        }

        /// <summary>
        /// 翌朝に時間を進めます。
        /// </summary>
        public static async Task GoNextDayMorningAsync()
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var ar = item.QueryObjects<CustomGoNextDayMorningAsyncProcessor>();/* DIABLE ASYNC WARN */
                if (ar.Length > 0)
                {
                    // if found custom GoNextDayMorningAsync,
                    // call it and return
                    await ar[0].GoNextDayMorningAsync();
                    return;
                }
            }

            string defaultPlaceId = await Places.GetDefaultPlaceIDAsync();
            if (defaultPlaceId != null)
            {
                // 自動帰還機能発動
                await State.WarpToAsync(defaultPlaceId);
            }

            // 次の起床時刻は、今日の就寝時刻+8時間である
            DateTime nextMorning = 今日の就寝時刻.AddHours(8);
            // 既にその時間を過ぎていれば翌日以降に繰り越す
            if (Flags.Now >= nextMorning)
            {
                // この実装だと2日以上飛ぶことができない
                var days = 1;
                //System.Diagnostics.Debug.WriteLine($"GoNextDayMorningAsync: days={days}");
                nextMorning = nextMorning.AddDays(days);
            }
            State.NextMorning = nextMorning;

            // もし、今日の就寝時刻に達していなければ、今日の就寝時刻まで時間を進める
            if (今日の就寝時刻 > Flags.Now)
            {
                int minutes = (int)(今日の就寝時刻 - Flags.Now).TotalMinutes;
                await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now.AddMinutes(minutes));
                // イベントの結果に関係なくもう一度計算して時間を進める
                if (今日の就寝時刻 > Flags.Now)
                {
                    int minutes2 = (int)(今日の就寝時刻 - Flags.Now).TotalMinutes;
                    State.GoTime(minutes2);
                }
            }
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnBeforeSleepAsync()) return;
            }

            GoTime((int)(nextMorning - Flags.Now).TotalMinutes);
            DefaultPersons.システム.Say("……{0}は眠りに落ちました。", General.GetMyName());
            await UI.Actions.sleepFlashAsync();
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnSleepingAsync()) return;
            }
            起床();
            DefaultPersons.システム.Say("{0}は目覚めました。", General.GetMyName());

            //JournalingWriter.WriteComment(Flags.Now.ToString("MMddHHmm") + "の目覚め");

            General.IncrementLastSleepDateCount();

            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnAfterSleepAsync()) return;
            }
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnStartTodayAsync()) return;
            }

            // ジャーナリングファイルのプレイバック中は
            // 自動セーブさせない
            if (!UI.Actions.isJournalFilePlaying())
            {
                try
                {
                    await General.自動セーブAsync();
                }
                catch (Exception e)
                {
                    DefaultPersons.システム.Say("自動セーブに失敗しました。({0})", e.Message);
                }
            }
        }

        class EquipListItem
        {
            internal string Name;
            internal DateTime LastRefered;
            internal EquipSet Set;
        }

        private static List<EquipListItem> equipSetItems = new List<EquipListItem>();

        // 以下のEquipSetsのメソッド群はスレッドセーフではない
        // のだが、実質的にスレッド間でのアクセス競合はない……と考えて放置
        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static string[] GetEquipSets()
        {
            // 先にソートしておく
            equipSetItems.Sort(delegate(EquipListItem x, EquipListItem y)
            {
                return Math.Sign(x.LastRefered.Ticks - y.LastRefered.Ticks);
            });
            List<string> result = new List<string>();
            foreach (EquipListItem item in equipSetItems)
            {
                result.Add(item.Name);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static bool IsEquipSetName(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name) return true;
            }
            return false;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="oldName">一般アプリからは使うべきではありません</param>
        /// <param name="newName">一般アプリからは使うべきではありません</param>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static bool RenameEquipSetName(string oldName, string newName)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == oldName)
                {
                    item.Name = newName;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static EquipSet GetEquipSet(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name) return item.Set;
            }
            return null;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        /// <param name="newSet">一般アプリからは使うべきではありません</param>
        public static void SetEquipSet(string name, EquipSet newSet)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name)
                {
                    item.Set = newSet;
                    return;
                }
            }
            // 見つからない時は新規作成
            EquipListItem newItem = new EquipListItem();
            newItem.Name = name;
            newItem.LastRefered = DateTime.Now;
            newItem.Set = newSet;
            equipSetItems.Add(newItem);
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        public static void TouchEquipSet(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name)
                {
                    item.LastRefered = DateTime.Now;
                    return;
                }
            }
        }

        /// <summary>
        /// 既に存在する装備セットの内容を装備します
        /// </summary>
        /// <param name="name">装備セットの名前です</param>
        /// <returns>不足アイテムリスト</returns>
        public static Item[] LoadEquipSetIfItemExist(string personId, string name)
        {
            EquipListItem targetEquipSet = equipSetItems.FirstOrDefault(c=>c.Name == name);
            if (targetEquipSet == null) throw new ApplicationException($"内部エラー: LoadEquipSetIfItemExisthaメソッドで{name}はequipSetItemsに見つかりません。");

            var lackItemsList = new List<Item>();
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                // そのアイテムが既に装備済みなら何もしない
                if (targetEquipSet.Set.AllItems[i].Id == Flags.GetRawEquip(personId, i)) continue;

                // 取りはずし不可のアイテムが既に装備されていたら飛ばす
                if (Items.GetItemByNumber(Flags.GetRawEquip(personId, i)).Is取り外し不可能Item) continue;

                if (!General.IsEquippableItem(i, personId, targetEquipSet.Set.AllItems[i].Id))
                {
                    // 装備が許可されていないアイテムは自動的にnull扱いされる
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                }
                else if (targetEquipSet.Set.AllItems[i].IsItemNull)
                {
                    // NULLアイテムはNULLアイテムに設定するが不足アイテム扱いはしない
                    // NULLアイテムは所持できないから別扱い
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                }
                else if (State.GetItemCount(targetEquipSet.Set.AllItems[i]) == 0 || General.IsAnyoneEquippedAllItems(targetEquipSet.Set.AllItems[i].Id, personId))
                {
                    // 不足アイテム
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                    lackItemsList.Add(targetEquipSet.Set.AllItems[i]);
                }
                else
                {
                    // そのアイテムを装備
                    Flags.SetRawEquip(personId, i, targetEquipSet.Set.AllItems[i].Id);
                }
            }
            return lackItemsList.ToArray();
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        public static void RemoveEquipSet(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name)
                {
                    equipSetItems.Remove(item);
                    return;
                }
            }
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// 操作が許可されない特別な装備セットの名前を取得します
        /// </summary>
        /// <returns>名前のコレクション</returns>
        public static string[] DisabledEquipSetNames()
        {
            return new string[] { State.装備なし };
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static DateTime LastRefered()
        {
            DateTime result = DateTime.MinValue;
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.LastRefered > result) result = item.LastRefered;
            }
            return result;
        }

        /// <summary>
        /// 文字列"カスタム"です。
        /// </summary>
        public const string カスタム = "カスタム";
        /// <summary>
        /// 文字列"装備なし"です。
        /// </summary>
        public const string 装備なし = "全裸にする";

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="name">一般アプリからは使うべきではありません</param>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static bool IsReadOnlyEquipSet(string name)
        {
            return name == 装備なし || name == カスタム;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        public class OwnItem
        {
            /// <summary>
            /// 一般アプリからは使うべきではありません
            /// </summary>
            public string Id;
            /// <summary>
            /// 一般アプリからは使うべきではありません
            /// </summary>
            public int OwnCount;
            /// <summary>
            /// 一般アプリからは使うべきではありません
            /// </summary>
            public int UsedCount;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <param name="ownItemList">一般アプリからは使うべきではありません</param>
        public static void SetOwnItemList(OwnItem[] ownItemList)
        {
            foreach (OwnItem oitem in ownItemList)
            {
                Item item = Items.GetItemByNumber(oitem.Id);
                State.SetItemCount(item, oitem.OwnCount);
                State.SetUsedCount(item, oitem.UsedCount);
            }
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <returns>一般アプリからは使うべきではありません</returns>
        public static OwnItem[] GetOwnItemList()
        {
            List<OwnItem> result = new List<OwnItem>();
            foreach (string id in Items.GetItemIDList())
            {
                Item item = Items.GetItemByNumber(id);
                if (State.GetItemCount(item) > 0)
                {
                    OwnItem oitem = new OwnItem();
                    oitem.Id = id;
                    oitem.OwnCount = State.GetItemCount(item);
                    oitem.UsedCount = State.GetUsedCount(item);
                    result.Add(oitem);
                }
            }
            return result.ToArray();
        }

        // 全てのアイテムを持っていなかったことにする
        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        public static void ClearAllItems()
        {
            foreach (string id in Items.GetItemIDList())
            {
                Item item = Items.GetItemByNumber(id);
                if (State.GetItemCount(item) > 0)
                {
                    State.SetItemCount(item, 0);
                    State.SetUsedCount(item, 0);
                }
            }
        }

        /// <summary>
        /// 読み込んだモジュール一覧
        /// </summary>
        public static Module[] loadedModules = new Module[0];
        /// <summary>
        /// 読み込んだモジュール一覧
        /// </summary>
        public static ModuleEx[] LoadedModulesEx = new ModuleEx[0];

        /// <summary>
        /// 引数targetに渡された型を定義しているモジュールを判定して返します
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Module SeekModule(object target)
        {
            System.Reflection.Module t = target.GetType().Module;
            for (int i = 0; i < loadedModules.Length; i++)
            {
                if (loadedModules[i].GetType().Module == t) return loadedModules[i];
            }
            return null;
        }

        /// <summary>
        /// 指定アイテムが装備されているか?
        /// </summary>
        /// <param name="targetItem">対象とするアイテム</param>
        /// <returns>装備されていたらTrue</returns>
        public static bool IsEquipedItem(Item targetItem)
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                if (targetItem.Id == Flags.Equip[i]) return true;
            }
            return false;
        }

        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        public static MenuStopControls MenuStopMaps
        {
            get
            {
                MenuStopControls n = 0;
                foreach (var m in State.loadedModules)
                {
                    n |= m.StopMenus();
                }
                return n;
            }
        }

        /// <summary>
        /// 指定コレクションが入手されたことを記録します。システムファイルへのセーブも行います。
        /// </summary>
        /// <param name="collectionID">コレクションのIdです</param>
        /// <param name="keyID">キーのIdです</param>
        /// <param name="subkeyID">サブキーのIdです</param>
        public static void SetCollection(string collectionID, string keyID, string subkeyID)
        {
            if (collectionID == null) collectionID = "";
            if (keyID == null) keyID = "";
            if (subkeyID == null) subkeyID = "";

            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            Flags.Collections["col_" + collectionID + "_" + keyID + "_" + subkeyID] = 1;

            if (!SystemFile.HasCollection(collectionID, keyID, subkeyID))
            {
                SystemFile.SetCollection(collectionID, keyID, subkeyID);
                StarManager.AddStar(1);
                DefaultPersons.システム.Say("おめでとうございます。スターを1つ入手して合計{0}個になりました。", StarManager.GetStars());
            }
        }

        /// <summary>
        /// 所有している状態です。
        /// </summary>
        [System.Reflection.ObfuscationAttribute(Exclude = true)]
        public enum CollectionState
        {
            /// <summary>
            /// 所有していません。
            /// </summary>
            None,
            /// <summary>
            /// 所有していませんが、システムファイルに記録があって名前は見えます。
            /// </summary>
            NameVisible,
            /// <summary>
            /// 所有しています。
            /// </summary>
            Own
        }
        /// <summary>
        /// 指定コレクションを既に取得済みか判定
        /// </summary>
        /// <param name="collectionID">コレクションのIdです</param>
        /// <param name="keyID">キーのIdです</param>
        /// <param name="subkeyID">サブキーのIdです</param>
        /// <returns>取得の状況</returns>
        public static CollectionState HasCollection(string collectionID, string keyID, string subkeyID)
        {
            if (collectionID == null) collectionID = "";
            if (keyID == null) keyID = "";
            if (subkeyID == null) subkeyID = "";

            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            if (Flags.Collections["col_" + collectionID + "_" + keyID + "_" + subkeyID] != 0) return CollectionState.Own;
            if (SystemFile.HasCollection(collectionID, keyID, subkeyID)) return CollectionState.NameVisible;
            return CollectionState.None;
        }

        /// <summary>
        /// あるスケジュールが可視であるかをセットします
        /// この情報は消費されることで自動的にリセットされ、くりかえし実行されることを抑止します
        /// </summary>
        /// <param name="scheduleId">スケジュールのIDです</param>
        /// <param name="availability">可視であるかを指定します</param>
        public static void SetScheduleVisible(string scheduleId, bool availability)
        {
            Flags.ScheduleVisilbles[scheduleId] = availability;
        }

        /// <summary>
        /// あるスケジュールが可視であるかを判定します
        /// </summary>
        /// <param name="scheduleId">スケジュールのIDです</param>
        /// <returns>可視であればtrueを返します</returns>
        public static bool IsScheduleVisible(string scheduleId)
        {
            return Flags.ScheduleVisilbles[scheduleId];
        }

        public static Func<Dummy> EquipChangeNotify { get; set; }

        /// <summary>
        ///  ジャーナリングプレイヤーを保持する
        /// </summary>
        public static Func<JournalingInputDescripter,Task> JournalingPlayer { get; set; }
    }
}
