using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ANGFLib.SuperTalkCollections;

namespace ANGFLib
{
    /// <summary>
    /// 実行時アサート失敗の通知用です。一般のモジュールは使用すべきではありません。
    /// </summary>
    public class JournalingAssertionException : ApplicationException
    {
        //public JournalingAssertionException() { }
        //public JournalingAssertionException(string message) : base(message) { }
        //public JournalingAssertionException(string message, Exception inner) : base(message, inner) { }
        //protected JournalingAssertionException(
        //  System.Runtime.Serialization.SerializationInfo info,
        //  System.Runtime.Serialization.StreamingContext context)
        //	: base(info, context) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="message">メッセージです。</param>
        /// <param name="node">対象としたノードです。</param>
        public JournalingAssertionException(string message, JournalingNode node)
            : base(message + " : " + (node == null ? "(EOF)" : node.SourceFileName.FileName + " (" + node.LineNumber.ToString() + ")"))
        {
            JournalPlaybackQueue.Clear();
        }
        public static async Task<JournalingAssertionException> CreateAsync(string message, JournalingNode node)
        {
            // バッチテストしていると、restoreActionSetAsyncから永遠に戻って来ないので、
            // ここでテスト失敗を明確にしておく
            // これはrestoreActionSetAsync内部で参照されてユーザーに伝わる
            JournalingDocumentPlayer.IsSuccess = false;
            var r = new JournalingAssertionException(message, node);
            await UI.Actions.restoreActionSetAsync();
            return r;
        }

    }

    /// <summary>
    /// ジャーナリングを行うノードの種類です。
    /// </summary>
    public enum JournalingNodeType
    {
        /// <summary>
        /// シンプルなメニューです。
        /// </summary>
        SimpleMenu,
        /// <summary>
        /// 日付をアサートします
        /// </summary>
        DateAssert,
        /// <summary>
        /// 時刻をアサートします。
        /// </summary>
        TimeAssert,
        //FlagAssert,
        /// <summary>
        /// 金額をアサートします
        /// </summary>
        MoneyAssert,
        /// <summary>
        /// アイテムを使用します。
        /// </summary>
        ConsumeItem,
        /// <summary>
        /// 装備します。部位はインデックス0です。
        /// </summary>
        Equip,
        /// <summary>
        /// 拡張用です。
        /// </summary>
        Extra,
        /// <summary>
        /// 購入します。
        /// </summary>
        Buy,
        /// <summary>
        /// 売却します。
        /// </summary>
        Sell,
        /// <summary>
        /// 名前を入力します。
        /// </summary>
        NameEntry,
        /// <summary>
        /// システムファイルの内容を初期化します
        /// </summary>
        システムファイル初期化,
        /// <summary>
        /// 別のジャーナリングファイルを取り込みます。
        /// </summary>
        Include,
        /// <summary>
        /// ガイドメッセージを表示します。
        /// </summary>
        GuideMessage,
        /// <summary>
        /// 1つのオブジェクトを選択します。
        /// </summary>
        SelectOneItem,
        /// <summary>
        /// 生活サイクルを変更します (絶対値)
        /// </summary>
        ChangeCycleRelative,
        /// <summary>
        /// 1つのコレクションをアサートします
        /// </summary>
        CollectionAssert,
        /// <summary>
        /// コレクションの取得率をアサートします
        /// </summary>
        RateAssert,
        /// <summary>
        /// コレクションの真の取得率をアサートします
        /// </summary>
        TrueRateAssert,
        /// <summary>
        /// モジュール単位でコレクションをクリアします
        /// </summary>
        InitCollectionsByModule,
        /// <summary>
        /// スターを指定値にセットします
        /// </summary>
        SetStar,
    };

    internal static class JournalingCommandNameMap
    {
        internal static Dictionary<string, JournalingNodeType> Map = new Dictionary<string, JournalingNodeType>();

        static JournalingCommandNameMap()
        {
            Map["M"] = JournalingNodeType.SimpleMenu;
            Map["D"] = JournalingNodeType.DateAssert;
            Map["T"] = JournalingNodeType.TimeAssert;
            Map["S"] = JournalingNodeType.CollectionAssert;
            Map["RS"] = JournalingNodeType.RateAssert;
            Map["TRS"] = JournalingNodeType.TrueRateAssert;
            //Map["F"] = JournalingNodeType.FlagAssert;
            Map["Y"] = JournalingNodeType.MoneyAssert;
            Map["C"] = JournalingNodeType.ConsumeItem;
            Map["EQ"] = JournalingNodeType.Equip;
            Map["EX"] = JournalingNodeType.Extra;
            Map["SB"] = JournalingNodeType.Buy;
            Map["SS"] = JournalingNodeType.Sell;
            Map["N"] = JournalingNodeType.NameEntry;
            Map["L"] = JournalingNodeType.ChangeCycleRelative;
            Map["IS"] = JournalingNodeType.システムファイル初期化;
            Map["I"] = JournalingNodeType.Include;
            Map["G"] = JournalingNodeType.GuideMessage;
            Map["AI"] = JournalingNodeType.SelectOneItem;
            Map["ISM"] = JournalingNodeType.InitCollectionsByModule;
            Map["STAR"] = JournalingNodeType.SetStar;
        }

        public static string ReveseReference(JournalingNodeType type)
        {
            foreach (string key in Map.Keys)
            {
                if (Map[key] == type) return key;
            }
            throw new ApplicationException(((int)type).ToString() + "はあり得ないJournalingNodeTypeです。");
        }
    }

    /// <summary>
    /// ジャーナリング用。一般アプリから使うべきではありません。
    /// </summary>
    public class JournalingNode
    {
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public JournalingNodeType CommandType;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public Module Module;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public bool Negative;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public string ArgumentString;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public string ArgumentString2;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public string ArgumentString3;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public int ArgumentInt;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public DateTime ArgumentDateTime;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public string ArgumentModuleId;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public string[] ArgumentExtra;
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public JournalingInputDescripter SourceFileName;	// インクルード機能を使うと必要になる情報
        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public int LineNumber;
    }

    /// <summary>
    /// ジャーナリング用。一般アプリから使うべきではありません。
    /// </summary>
    public class JournalingDocument
    {
        private List<JournalingNode> nodes = new List<JournalingNode>();
        private int sequenceCounter = 0;

        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public static Item GetItemFromName(string name)
        {
            var r = Item.List.Values.FirstOrDefault(c => c.HumanReadableName == name);
            if (r != null) return r;
            throw new JournalingDocumentException(name + "はアイテム名ではありません。");
        }

        private static void validateItemName(string name)
        {
            // 実行時に動的に決まるアイテム名はこの方法ではバリデーションできない
            //// 実質的にこの実装で十分
            //GetItemFromName(name);
        }

        private static void validateCollectionName(string moduleId, string collectionName, string mainname, string subname = null)
        {
            Collection collection = SimpleName<Collection>.List.Values.FirstOrDefault((c) => c.HumanReadableName == collectionName);
            if (collection == null)
            {
                throw new JournalingDocumentException(collectionName + "はコレクション名ではありません。");
            }

            bool found = false;
            foreach (var collectionItem in collection.Collections)
            {
                if (collectionItem.GetRawSubItems == null)
                {
                    if (collectionItem.Name == mainname && collectionItem.OwnerModuleId == moduleId)
                    {
                        found = true;
                    }
                }
                else
                {
                    CollectionItem[] subs = collectionItem.GetSubItems();
                    foreach (var collectionSubItem in subs)
                    {
                        if (collectionSubItem.Name == subname && collectionSubItem.OwnerModuleId != moduleId)
                        {
                            found = true;
                        }
                    }
                }
            }
            if (!found) throw new JournalingDocumentException(mainname + "はコレクションに含まれません。");
        }

        private static void validateModuleId(string id)
        {
            if (!State.loadedModules.Any(c => c.Id == id)) throw new JournalingDocumentException(id + "はモジュールのID名ではありません。");
        }

        private static void validateEquipItemName(string name, int target /*装備部位*/ )
        {
            if (string.IsNullOrWhiteSpace(name)) return;	// 空文字列は"装備無し"を示す有効な名前
            var r = Item.List.Values.FirstOrDefault(c => c.HumanReadableName == name);
            if (r == null)
                throw new JournalingDocumentException(name + "は適切な装備アイテム名ではありません。(名前が見付からない)");
            if (r.AvailableEquipMap.Length <= target)
                throw new JournalingDocumentException(name + $"は適切な装備アイテム名ではありません。(r.AvailableEquipMap.Length({r.AvailableEquipMap.Length}) <= target({target}))");
            if (r.AvailableEquipMap[target]) return;
            throw new JournalingDocumentException(name + "は適切な装備アイテム名ではありません(r.AvailableEquipMap[target]==false)。");
        }

        private static void validatePersonId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;  // 空白は合法
            if (Person.List.Keys.Contains(id)) return;
            throw new JournalingDocumentException(id + "は適切なPerson IDではありません。");
        }

        /// <summary>
        /// ジャーナリング用。一般アプリから使うべきではありません。
        /// </summary>
        public async System.Threading.Tasks.Task JournalingDocumentInitAsync(JournalingInputDescripter filename)// 読み込みとコンパイルを行う
        {
            char[] whiteSpaces = { ' ', '\t', '　' };
            int lineNumber = 0;	// 先にカウントアップしたいので、1ではなく0から始める
            await progressUpdateAsync();
            try
            {
                var stream = filename.CreateStream();
                if (stream == null)
                {
                    throw new JournalingDocumentException($"File {filename?.FileName} not found in {filename?.ModuleEx?.GetXmlModuleData()?.Name}");
                }
                using (TextReader reader = new StreamReader(stream))
                {
                    for (; ; )
                    {
                        string s = reader.ReadLine();
                        if (s == null) break;
                        lineNumber++;

                        if (s.Trim().Length == 0) continue; // 空行
                        if (s.StartsWith("*")) continue;    // コメント行

                        int whiteSpaceIndex = s.IndexOfAny(whiteSpaces);
                        string commandName, argument;
                        if (whiteSpaceIndex < 0)
                        {
                            commandName = s;
                            argument = "";
                        }
                        else
                        {
                            commandName = s.Substring(0, whiteSpaceIndex).Trim();
                            argument = s.Substring(whiteSpaceIndex).Trim();
                        }

                        bool negativeCondition = commandName.EndsWith("!");
                        if (negativeCondition) commandName = commandName.Substring(0, commandName.Length - 1);

                        if (!JournalingCommandNameMap.Map.ContainsKey(commandName))
                        {
                            throw new JournalingDocumentException(commandName + "は有効なコマンド名ではありません。", filename.FileName, lineNumber);
                        }

                        JournalingNode newNode = new JournalingNode();
                        newNode.SourceFileName = filename;
                        newNode.LineNumber = lineNumber;
                        newNode.CommandType = JournalingCommandNameMap.Map[commandName];
                        newNode.Negative = negativeCondition;
                        switch (newNode.CommandType)
                        {
                            // アーギュメント無しのグループ
                            case JournalingNodeType.システムファイル初期化:
                                break;
                            // アーギュメントは任意の文字列のグループ
                            case JournalingNodeType.SimpleMenu:
                                newNode.ArgumentString = argument;
                                break;
                            case JournalingNodeType.NameEntry:
                                goto case JournalingNodeType.SimpleMenu;
                            case JournalingNodeType.GuideMessage:
                                goto case JournalingNodeType.SimpleMenu;
                            default:
                                // 例外を投げる可能性があるコードをまとめて処理
                                try
                                {
                                    switch (newNode.CommandType)
                                    {
                                        // アーギュメントは日付
                                        case JournalingNodeType.DateAssert:
                                            newNode.ArgumentDateTime = DateTime.ParseExact(argument, "yyyy/MM/dd", null);
                                            break;
                                        // アーギュメントは時刻
                                        case JournalingNodeType.TimeAssert:
                                            newNode.ArgumentDateTime = DateTime.ParseExact(argument, "HH:mm", null);
                                            break;
                                        // コレクションアサート専用
                                        case JournalingNodeType.CollectionAssert:
                                            string[] args = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args.Length < 3 || args.Length > 4)
                                            {
                                                throw new JournalingDocumentException("引数の数が一致しません。");
                                            }
                                            newNode.Module = State.loadedModules.First((m) => m.Id == args[0]);
                                            newNode.ArgumentString = args[1];
                                            newNode.ArgumentString2 = args[2];
                                            if (args.Length == 4)
                                            {
                                                newNode.ArgumentString3 = args[3];
                                                validateCollectionName(args[0], newNode.ArgumentString, newNode.ArgumentString2, newNode.ArgumentString3);
                                            }
                                            else
                                            {
                                                validateCollectionName(args[0], newNode.ArgumentString, newNode.ArgumentString2);
                                            }
                                            break;
                                        //case JournalingNodeType.FlagAssert:
                                        //string[] args = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                        //if (args.Length != 2)
                                        //{
                                        //throw new JournalingDocumentException("引数の数が一致しません。");
                                        //}
                                        //State.ValidateFlagName(args[0]);
                                        //newNode.ArgumentString = args[0];
                                        //newNode.ArgumentInt = int.Parse(args[1]);
                                        //break;
                                        // アーギュメントが整数のグループ
                                        case JournalingNodeType.MoneyAssert:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        case JournalingNodeType.ChangeCycleRelative:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        // アーギュメントはモジュールID
                                        case JournalingNodeType.InitCollectionsByModule:
                                            validateModuleId(argument);
                                            newNode.ArgumentModuleId = argument;
                                            break;
                                        // アーギュメントはスター数
                                        case JournalingNodeType.SetStar:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        // アーギュメントはアイテム名
                                        case JournalingNodeType.ConsumeItem:
                                            validateItemName(argument);
                                            newNode.ArgumentString = argument;
                                            break;
                                        case JournalingNodeType.SelectOneItem:
                                            goto case JournalingNodeType.ConsumeItem;
                                        // アーギュメントは個数+アイテム名
                                        case JournalingNodeType.Buy:
                                            string[] args2 = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args2.Length != 2)
                                            {
                                                throw new JournalingDocumentException("引数の数が一致しません。");
                                            }
                                            validateItemName(args2[1]);
                                            newNode.ArgumentString = args2[1];
                                            newNode.ArgumentInt = int.Parse(args2[0]);
                                            break;
                                        case JournalingNodeType.Sell:
                                            goto case JournalingNodeType.Buy;
                                        // アーギュメントはID+何かの文字列
                                        case JournalingNodeType.RateAssert:
                                        case JournalingNodeType.TrueRateAssert:
                                        case JournalingNodeType.Extra:
                                            string[] args3 = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args3.Length <= 1)
                                            {
                                                throw new JournalingDocumentException("引数の数が一致しません。");
                                            }
                                            validateModuleId(args3[0]);
                                            newNode.ArgumentModuleId = args3[0];
                                            {
                                                string[] newArray = new string[args3.Length - 1];
                                                for (int i = 1; i < args3.Length; i++)
                                                {
                                                    newArray[i - 1] = args3[i];
                                                }
                                                newNode.ArgumentExtra = newArray;
                                            }
                                            break;
                                        // アーギュメントは特定装備部位装備品または空
                                        case JournalingNodeType.Equip:
                                            {
                                                string[] args2e = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                                if(args2e.Length > 0)
                                                {
                                                    string s1;
                                                    var index = args2e[0].IndexOf('_');
                                                    if (index > 0)
                                                    {
                                                        s1 = args2e[0].Substring(0, index);
                                                        newNode.ArgumentString2 = args2e[0].Substring(index+1);
                                                        validatePersonId(newNode.ArgumentString2);
                                                    }
                                                    else
                                                    {
                                                        s1 = args2e[0];
                                                        newNode.ArgumentString2 = "";
                                                    }
                                                    newNode.ArgumentInt = int.Parse(s1);
                                                    if (args2e.Length == 1)
                                                    {
                                                        newNode.ArgumentString = "";    // ItemNull
                                                    }
                                                    else if (args2e.Length == 2)
                                                    {
                                                        newNode.ArgumentString = args2e[1];
                                                        validateEquipItemName(newNode.ArgumentString, newNode.ArgumentInt);
                                                    }
                                                    else
                                                    {
                                                        throw new JournalingDocumentException("引数の数が一致しません。");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new JournalingDocumentException("引数の数が一致しません。");
                                                }
                                            }
                                            break;
                                        // インクルードのみこのレイヤーで機能するので例外的に処理
                                        case JournalingNodeType.Include:
#if true
                                            JournalingDocument subdoc = new JournalingDocument();
                                            await subdoc.JournalingDocumentInitAsync(new JournalingInputDescripter(filename.ModuleId, argument));
                                            this.nodes.AddRange(subdoc.nodes);
                                            continue;   // nodes.Add(newNode)は実行せずループを先に進める
                                                        //break;
#else
                                        string path = Path.GetDirectoryName(Path.GetFullPath(filename));
                                        string oldCurdir = Directory.GetCurrentDirectory();
                                        try
                                        {
                                            Directory.SetCurrentDirectory(path);
                                            JournalingDocument subdoc = new JournalingDocument();
                                            await subdoc.JournalingDocumentInitAsync(argument);
                                            this.nodes.AddRange(subdoc.nodes);
                                        }
                                        finally
                                        {
                                            Directory.SetCurrentDirectory(oldCurdir);
                                        }
                                        continue;   // nodes.Add(newNode)は実行せずループを先に進める
                                                    //break;
#endif
                                    }
                                }
                                catch (Exception e)
                                {
                                    throw new JournalingDocumentException($"ジャーナリングファイルの解析エラー {filename.FileName}", e);
                                    //throw new JournalingDocumentException($"ジャーナリングファイルの解析エラー {filename.FileName} ({filename.ModuleEx.GetXmlModuleData().Name})", e);
                                }
                                break;
                        };
                        nodes.Add(newNode);
                    }
                }
            }
            finally
            {
                //DefaultPersons.システム.Say();
                var fn = filename.FileName;
                if (fn.StartsWith(Constants.JournalingDirectHeader)) fn = "[INPUTBOX]";
                await UI.Actions.NotifyStatusMessageAsync($"Total {lineNumber} lines detected in {fn}");
            }
        }
        /// <summary>
        /// ジャーナリング用。一般アプリから使用すべきではありません。
        /// </summary>
        public async System.Threading.Tasks.Task ProcessingAssertsAsync()
        {
            for (; ; )
            {
                if (IsEndOfRecords) return;
                bool condition = false;
                switch (nodes[sequenceCounter].CommandType)
                {
                    case JournalingNodeType.DateAssert:
                        condition = nodes[sequenceCounter].ArgumentDateTime.Year == Flags.Now.Year
                            && nodes[sequenceCounter].ArgumentDateTime.Month == Flags.Now.Month
                            && nodes[sequenceCounter].ArgumentDateTime.Day == Flags.Now.Day;
                        break;
                    case JournalingNodeType.TimeAssert:
                        condition = nodes[sequenceCounter].ArgumentDateTime.Hour == Flags.Now.Hour
                            && nodes[sequenceCounter].ArgumentDateTime.Minute == Flags.Now.Minute;
                        break;
                    case JournalingNodeType.CollectionAssert:
                        {
                            Collection collection = SimpleName<Collection>.List.Values.First((c) => c.HumanReadableName == nodes[sequenceCounter].ArgumentString);
                            string collectioID = collection.Id;
                            CollectionItem key = collection.Collections.First((c) => c.Name == nodes[sequenceCounter].ArgumentString2);
                            string keyID = key.Id;
                            string subkeyID = null;
                            if (key.GetRawSubItems != null)
                            {
                                CollectionItem subkey = (key.GetSubItems()).First((c) => c.Name == nodes[sequenceCounter].ArgumentString3);
                                subkeyID = subkey.Id;
                            }
                            condition = State.HasCollection(collectioID, keyID, subkeyID) != State.CollectionState.None;
                            break;
                        }
                    case JournalingNodeType.RateAssert:
                        condition = await rateAssertAsync(nodes[sequenceCounter], false);
                        break;
                    case JournalingNodeType.TrueRateAssert:
                        condition = await rateAssertAsync(nodes[sequenceCounter], true);
                        break;
                    //case JournalingNodeType.FlagAssert:
                    //condition = nodes[sequenceCounter].ArgumentInt == ISSFirstStateWrapper.GetFlag(nodes[sequenceCounter].ArgumentString);
                    //break;
                    case JournalingNodeType.MoneyAssert:
                        condition = nodes[sequenceCounter].ArgumentInt <= Flags.所持金;
                        break;
                    case JournalingNodeType.システムファイル初期化:
                        // これはアサートではないが、アサートに準じるタイミングで処理されねばならない
                        SystemFile.AllClearForNewPlay();
                        // トリックだが、常にフェイルしない条件を与える
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.SetStar:
                        // これはアサートではないが、アサートに準じるタイミングで処理されねばならない
                        StarManager.AddStar(-StarManager.GetStars() + nodes[sequenceCounter].ArgumentInt);
                        // トリックだが、常にフェイルしない条件を与える
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.InitCollectionsByModule:
                        // これはアサートではないが、アサートに準じるタイミングで処理されねばならない
                        SystemFile.InitCollectionsByModule(nodes[sequenceCounter].ArgumentModuleId);
                        // トリックだが、常にフェイルしない条件を与える
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.GuideMessage:
                        // これはアサートではないが、アサートに準じるタイミングで処理されねばならない
                        this.supplyMessage = nodes[sequenceCounter].ArgumentString;
                        // トリックだが、常にフェイルしない条件を与える
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.Extra:
                        foreach (var m in State.loadedModules)
                        {
                            if (m.Id == nodes[sequenceCounter].ArgumentModuleId)
                            {
                                condition = await m.OnExtraJurnalPlaybackAsync(nodes[sequenceCounter].ArgumentExtra);
                                break;
                            }
                        }
                        break;
                    default:
                        // 該当しないコマンドに当たったら、アサート処理は終わり
                        return;
                }
                if (nodes[sequenceCounter].Negative) condition = !condition;
                if (!condition)
                {
                    throw await JournalingAssertionException.CreateAsync("アサート" +
                    JournalingCommandNameMap.ReveseReference(nodes[sequenceCounter].CommandType) +
                    "がフェイルしました。",
                    nodes[sequenceCounter]);
                }

                sequenceCounter++;
            }
        }

        private async Task<bool> rateAssertAsync(JournalingNode node, bool isTrueRate)
        {
            Collection collection = SimpleName<Collection>.List.Values.FirstOrDefault((c) => c.HumanReadableName == node.ArgumentExtra[0]);
            if (collection == null) throw await JournalingAssertionException.CreateAsync("No Collection Detected in assertion", node);
            string collectioID = collection.Id;
            int total = 0, count = 0;
            var hitList = new List<string>();
            var nohitList = new List<string>();
            //System.Diagnostics.Debug.Write("#");
            foreach (var item in collection.Collections)
            {
                if (item.GetRawSubItems != null)
                {
                    foreach (var subitem in item.GetSubItems())
                    {
                        //System.Diagnostics.Debug.Write("!");
                        if (subitem.OwnerModuleId != node.ArgumentModuleId)
                        {
                            //System.Diagnostics.Debug.WriteLine($"{subitem.OwnerModuleId}!={node.ArgumentModuleId}");
                            continue;
                        }
                        if (isTrueRate || !subitem.Hidden)
                        {
                            total++;
                            if (State.HasCollection(collectioID, item.Id, subitem.Id) != State.CollectionState.None)
                            {
                                count++;
                                hitList.Add($"{item.Name}/{subitem.Name}");
                            }
                            else
                            {
                                nohitList.Add($"{item.Name}/{subitem.Name}");
                            }
                        }
                    }
                }
                else
                {
                    if (item.OwnerModuleId != node.ArgumentModuleId) continue;
                    if (isTrueRate || !item.Hidden)
                    {
                        total++;
                        if (State.HasCollection(collectioID, item.Id, null) != State.CollectionState.None)
                        {
                            count++;
                            hitList.Add(item.Name);
                        }
                        else
                        {
                            nohitList.Add(item.Name);
                        }
                    }
                }
            }

            int rate;
            if (int.TryParse(node.ArgumentExtra[1], out rate))
            {
                if(total  == 0)
                {
                    throw await JournalingAssertionException.CreateAsync($"対象総数は{total}ですが、これでは割合を計算できません。", node);
                }
                int actualValue = count * 100 / total;
                bool result = rate != actualValue;
                if (node.Negative) result = !result;
                if (result)
                {
                    throw await JournalingAssertionException.CreateAsync($"{node.ArgumentExtra[1]}が期待されましたが、実際は{actualValue}でした。({count}/{total})[hit={string.Join(", ", hitList)}/nohit={string.Join(", ", nohitList)}]", node);
                }
            }
            else
            {
                throw await JournalingAssertionException.CreateAsync(node.ArgumentExtra[1] + "は整数ではありません。", node);
            }

            // node.Negative==trueの場合、あとから反転させられるので、予め判定した値を返しておく。
            return !node.Negative;
        }

        private string supplyMessage = "";
        private int lastUpdatedSequenceCounter = 0;
        private async Task progressUpdateAsync()
        {
#if DEBUG
            //await Console.Out.WriteLineAsync($"sequenceCounter={sequenceCounter}");
#endif
            if (lastUpdatedSequenceCounter == 0 || sequenceCounter >= lastUpdatedSequenceCounter + 128)
            {
                await UI.Actions.progressStatusAsync(string.Format("{0}/{1} {2}", sequenceCounter, this.nodes.Count, supplyMessage));
                lastUpdatedSequenceCounter = sequenceCounter;
            }
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="journalingNodeType">一般アプリから使うべきではありません。</param>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public async System.Threading.Tasks.Task<JournalingNode> GetNextRecordAsync(JournalingNodeType journalingNodeType)
        {
            await progressUpdateAsync();

            await ProcessingAssertsAsync();

            if (IsEndOfRecords)
            {
                throw await JournalingAssertionException.CreateAsync("コマンド" +
                JournalingCommandNameMap.ReveseReference(journalingNodeType) +
                "が期待されましたが、ノードはもうありません。",
                null);
            }

            //Console.WriteLine($"{nodes[sequenceCounter].CommandType} {sequenceCounter}");
            if (nodes[sequenceCounter].CommandType != journalingNodeType)
            {
                throw await JournalingAssertionException.CreateAsync("コマンド" +
                    JournalingCommandNameMap.ReveseReference(journalingNodeType) +
                    "が期待されましたが、実際に次のノードにあったのは" +
                    JournalingCommandNameMap.ReveseReference(nodes[sequenceCounter].CommandType) + "でした。",
                    nodes[sequenceCounter]);
            }
            JournalingNode result = nodes[sequenceCounter++];
            if (IsEndOfRecords) await UI.Actions.restoreActionSetAsync();
            return result;
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="journalingNodeType">一般アプリから使うべきではありません。</param>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public async System.Threading.Tasks.Task<JournalingNode> GetNextRecordIfExistAsync(JournalingNodeType journalingNodeType)
        {
            await progressUpdateAsync();

            await ProcessingAssertsAsync();

            if (IsEndOfRecords)
            {
                await UI.Actions.restoreActionSetAsync();
                return null;
            }
            if (nodes[sequenceCounter].CommandType != journalingNodeType) return null;
            JournalingNode result = nodes[sequenceCounter++];
            if (IsEndOfRecords) await UI.Actions.restoreActionSetAsync();
            return result;
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public async Task<JournalingNode> PeekCurrentRecordAsync()
        {
            int s = sequenceCounter - 1;
            if( s < 0 )
            {
                throw await JournalingAssertionException.CreateAsync("ノードはありません。", null);
            }
            if (sequenceCounter >= nodes.Count)
            {
                throw await JournalingAssertionException.CreateAsync("ノードはもうありません。", null);
            }
            return nodes[sequenceCounter];
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public async Task<JournalingNode> PeekNextRecordAsync()
        {
            if (sequenceCounter >= nodes.Count)
            {
                throw await JournalingAssertionException.CreateAsync("ノードはもうありません。", null);
            }
            return nodes[sequenceCounter];
        }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public async System.Threading.Tasks.Task IncrementNextRecordAsync()
        {
            await progressUpdateAsync();

            await ProcessingAssertsAsync();

            sequenceCounter++;
            if (IsEndOfRecords) await UI.Actions.restoreActionSetAsync();
        }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <returns>一般アプリから使うべきではありません。</returns>
        public bool IsEndOfRecords
        {
            get { return sequenceCounter >= nodes.Count; }
        }
    }

    // 否定アサートを書き込む機能は不要であることに注意
    // 実際に行動した結果の記録に否定アサートはあり得ない
    // このクラスは必ずしも異なるスレッドから呼ばれる可能性があるが、スレッドセーフではない
    // 実質的に同時にジャーナリングされる可能性は無いと考えられるので問題はない
    /// <summary>
    /// 一般アプリから使うべきではありません。
    /// </summary>
    public class MyJournalingFileWriter : IJournalingWriter
    {
        private TextWriter jounalWriter = null;

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="filename">一般アプリから使うべきではありません。</param>
        void IJournalingWriter.Create(string filename)
        {
            if (jounalWriter != null) jounalWriter.Close();
            var ext = Path.GetExtension(filename);
            var dir = General.GetJournalingDirectory();
            var actualFileName = Path.ChangeExtension(Path.Combine(dir, DateTime.Now.ToString(Constants.DateTimeFormat)), ext);
            for (; ; )
            {
                if (!File.Exists(actualFileName)) break;
                actualFileName += "_";
            }
            jounalWriter = File.CreateText(actualFileName);
            JournalingWriter.WriteComment("Record Start " + DateTime.Now.ToString());
        }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="arguments">一般アプリから使うべきではありません。</param>
        void IJournalingWriter.Write(string commandName, params object[] arguments)
        {
            if (jounalWriter == null) return;
            var sb = new StringBuilder();
            sb.Append(commandName);
            foreach (var item in arguments) sb.AppendFormat(" {0}", item);
            jounalWriter.WriteLine(sb);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        void IJournalingWriter.Close()
        {
            JournalingWriter.WriteComment("Record Stop " + DateTime.Now.ToString());
            if (jounalWriter != null) jounalWriter.Close();
            jounalWriter = null;
        }

        bool IJournalingWriter.IsAvailable()
        {
            return jounalWriter != null;
        }
    }

    // 否定アサートを書き込む機能は不要であることに注意
// 実際に行動した結果の記録に否定アサートはあり得ない
// このクラスは必ずしも異なるスレッドから呼ばれる可能性があるが、スレッドセーフではない
// 実質的に同時にジャーナリングされる可能性は無いと考えられるので問題はない
/// <summary>
/// 一般アプリから使うべきではありません。
/// </summary>
public class MyJournalingStringBuilderWriter: IJournalingWriter
    {
        private bool isRecording = false;
        private StringBuilder lines = null;

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="filename">一般アプリから使うべきではありません。</param>
        void IJournalingWriter.Create(string filename)
        {
            lines = new StringBuilder();
            isRecording = true;
            JournalingWriter.WriteComment("Record Start " + DateTime.Now.ToString());
        }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="arguments">一般アプリから使うべきではありません。</param>
        void IJournalingWriter.Write(string commandName, params object[] arguments)
        {
            if (!isRecording) return;
            var sb = new StringBuilder();
            sb.Append(commandName);
            foreach (var item in arguments) sb.AppendFormat(" {0}", item);
            lines.AppendLine(sb.ToString());
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        void IJournalingWriter.Close()
        {
            if (isRecording)
            {
                JournalingWriter.WriteComment("Record Stop " + DateTime.Now.ToString());
                isRecording = false;
                General.JounalResults = lines.ToString();
                lines = null;
            }
        }

        bool IJournalingWriter.IsAvailable()
        {
            return isRecording;
        }
    }
}
