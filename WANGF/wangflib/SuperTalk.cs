using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ANGFLib
{
    /// <summary>
    /// 内部使用専用です
    /// </summary>
    public class SuperTalkSource
    {
        /// <summary>
        /// 内部使用専用です
        /// </summary>
        public string Source;
        /// <summary>
        /// 内部使用専用です
        /// </summary>
        public readonly string Id;
        /// <summary>
        /// 内部使用専用です
        /// </summary>
        public SuperTalkSource()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 簡易な発言を行う強化されたラッパーを提供する
    /// </summary>
    public class SuperTalk
    {
        private Dictionary<string, Person> talkers = new Dictionary<string, Person>();
        private Dictionary<string, string> macros = new Dictionary<string, string>();

        private void definePerson(string[] args)
        {
            if (args.Length != 2 && args.Length != 3) throw new ApplicationException("number of Arguments must be 2 or 3");
            string id = null;
            Sex sex = Sex.Male;
            switch (args[1])
            {
                case "MALE": sex = Sex.Male; break;
                case "FEMALE": sex = Sex.Female; break;
                case "NUTRAL": sex = Sex.Nutral; break;
                case "SHEMALE": sex = Sex.Shemale; break;
                default:
                    id = args[1];
                    break;
            }
            Person person;
            if (id != null) person = Person.List[id];
            else
            {
                if (args.Length != 3) throw new ApplicationException("number of Arguments must be 3");
                person = new Person("DUMMY" + Guid.NewGuid(), args[2], sex);
            }
            AddTalker(args[0], person);
        }

        private void jump(string text, ref StringReader reader, string label)
        {
            reader = new StringReader(text);
            for (; ; )
            {
                string s = reader.ReadLine();
                if (s == null)
                {
                    throw new ApplicationException("label " + label + "not found");
                }

                s = s.Trim();
                // 空行は無視
                if (s.Length == 0) continue;

                // 汎用コメントを除去
                int commentIndex = s.IndexOf("//");
                if (commentIndex >= 0) s = s.Substring(0, commentIndex);

                // #で始まる行は無条件にコマンド
                if (s.StartsWith("#"))
                {
                    string command, argument = "";
                    // paesr 1st token
                    // 最初の空白文字までがコマンド名、あとがアーギュメント
                    int firstWhiteSpacePos2 = s.IndexOfAny(Constants.WhiteSpaces);
                    if (firstWhiteSpacePos2 < 0) command = s.Substring(1);
                    else
                    {
                        command = s.Substring(1, firstWhiteSpacePos2 - 1);
                        argument = s.Substring(firstWhiteSpacePos2 + 1).Trim();
                    }
                    string[] args = argument.Trim().Split(Constants.WhiteSpaces, StringSplitOptions.None);
                    switch (command)
                    {
                        case "LBL":
                            if (args[0] == label) return;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// メッセージ群を台詞として表示する
        /// 引数の文字列は1行単位で分解される
        /// 空行は無視される
        /// 半角空白、全角空白、タブまでは話者と見なされる
        /// 話者はAddTakerメソッドで登録する
        /// 本文の$に続く1文字はマクロとして置換される
        /// マクロはAddMacroメソッドで登録される
        /// 半角空白、全角空白、タブが含まれない行は、独白が話者と見なされる。このとき、本文に半角空白、全角空白、タブが含まれと判定が誤動作する
        /// #で始まる行はコマンドと見なされる
        /// </summary>
        /// <param name="text">複数行の文字列</param>
        /// <param name="startCollectionID">プレイバックするコレクションのID(オプション)</param>
        public void Play(string text, string startCollectionID = null)
        {
            bool active = startCollectionID == null;
            StringReader reader = new StringReader(text);
            for (; ; )
            {
                string s = reader.ReadLine();
                if (s == null) break;

                s = s.Trim();
                // 空行は無視
                if (s.Length == 0) continue;

                // 汎用コメントを除去
                int commentIndex = s.IndexOf("//");
                if (commentIndex >= 0) s = s.Substring(0, commentIndex);

                Person talker = null;
                string body;
                // #で始まる行は無条件にコマンド
                if (s.StartsWith("#"))
                {
                    string command, argument = "";
                    // paesr 1st token
                    // 最初の空白文字までがコマンド名、あとがアーギュメント
                    int firstWhiteSpacePos2 = s.IndexOfAny(Constants.WhiteSpaces);
                    if (firstWhiteSpacePos2 < 0) command = s.Substring(1);
                    else
                    {
                        command = s.Substring(1, firstWhiteSpacePos2 - 1);
                        argument = s.Substring(firstWhiteSpacePos2 + 1).Trim();
                    }
                    string[] args = argument.Trim().Split(Constants.WhiteSpaces, StringSplitOptions.None);
                    switch (command)
                    {
                        case "P":
                            definePerson(args);
                            break;
                        case "C":
                            if (args.Length == 3)
                            {
                                if (active) State.SetCollection(args[0], args[2], null);
                                else active = startCollectionID == args[2];
                            }
                            else if (args.Length == 4)
                            {
                                if (active) State.SetCollection(args[0], args[2], args[3]);
                                else active = startCollectionID == args[3];
                            }
                            else throw new ApplicationException("#C must have 3 or 4 arguments");
                            break;
                        case "CG":
                            if (args.Length != 3) throw new ApplicationException("#CG must have 3 arguments");
                            // no action required here
                            break;
                        case "CE":
                            // terminate if collectionPlayed
                            if (active && startCollectionID != null) return;
                            break;
                        case "E":
                            return;
                        case "CSS":
                            if (args.Length != 1) throw new ApplicationException("#CSS must have 1 argument");
                            // skip until CSE
                            for (; ; )
                            {
                                string s2 = reader.ReadLine();
                                if (s2 == null) break;
                                s2 = s2.Trim();
                                if (s2 == "#CSE") break;
                            }
                            // method call
                            var procName = General.GenerateProcName(args[0]);
                            foreach (var item in State.loadedModules)
                            {
                                // ダイナミック生成されたモジュールしか探さない
                                if (item.GetType().Assembly.Location != "") continue;
                                var methodInfo = item.GetType().GetMethod(procName);
                                if (methodInfo == null) continue;
                                methodInfo.Invoke(item, null);
                                break;
                            }
                            break;
                        case "JA":
                            if (args.Length != 1) throw new ApplicationException("#JA must have 1 argument");
                            jump(text, ref reader, args[0]);
                            break;
                        case "JT":
                            if (args.Length != 1) throw new ApplicationException("#JT must have 1 argument");
                            if (General.TheFlag) jump(text, ref reader, args[0]);
                            break;
                        case "JF":
                            if (args.Length != 1) throw new ApplicationException("#JF must have 1 argument");
                            if (!General.TheFlag) jump(text, ref reader, args[0]);
                            break;
                        case "LBL":
                            // ignore here
                            break;
                        case "MF":
                            if (args.Length != 2) throw new ApplicationException("#MF must have 2 argument");
                            if (args[0].Length != 1) throw new ApplicationException("first argument of #MF must have 1 charater");
                            object val = 0;
                            if (defaultMacro1 != null && defaultMacro1.Data.Keys.Contains(args[1])) val = defaultMacro1.Data[args[1]];
                            else if (defaultMacro2 != null && defaultMacro2.Data.Keys.Contains(args[1])) val = defaultMacro2.Data[args[1]];
                            macros[args[0]] = val.ToString();
                            break;
                        // TBW
                        default:
                            throw new ApplicationException("Unknwon command " + command);
                    }
                    continue;
                }
                if (!active) continue;

                // 最初の空白文字までがTalker名、あとが本文
                int firstWhiteSpacePos = s.IndexOfAny(Constants.WhiteSpaces);
                // 空白が2文字目より後にある場合は空白と承認しない (長いshortNameは認めない)
                // これを行わないと本文中に半角空白を使えない
                if (firstWhiteSpacePos < 0 || firstWhiteSpacePos > 2)
                {
                    talker = DefaultPersons.独白;
                    body = s;
                }
                else
                {
                    string shortName = s.Substring(0, firstWhiteSpacePos);
                    if (talkers.ContainsKey(shortName))
                    {
                        talker = talkers[shortName];
                    }
                    else
                    {
                        foreach (var n in State.loadedModules)
                        {
                            string id = n.GetQuickTalkPerson(shortName);
                            if (id != null)
                            {
                                talker = SimpleName<Person>.List[id];
                                break;
                            }
                        }
                    }
                    if (talker == null)
                    {
                        throw new ApplicationException(string.Format("SuperTalk構文エラー。{0}は定義されていない短いTalker名です。", shortName));
                    }
                    body = s.Substring(firstWhiteSpacePos + 1).Trim();
                }

                StringBuilder sb = new StringBuilder();
                bool macroFlag = false;
                foreach (char ch in body)
                {
                    if (ch == '$')
                    {
                        macroFlag = true;
                    }
                    else if (macroFlag)
                    {
                        string result = null;
                        foreach (var n in State.loadedModules)
                        {
                            result = n.GetQuickTalkMacro(new string(ch, 1));
                            if (result != null) break;
                        }
                        if (result == null)
                        {
                            if (!macros.ContainsKey(ch.ToString())) throw new ApplicationException(string.Format("SuperTalk構文エラー。{0}は定義されていないマクロ名です。", ch));
                            sb.Append(macros[ch.ToString()]);
                        }
                        else
                        {
                            sb.Append(result);
                        }
                        macroFlag = false;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                talker.Say(sb.ToString());
            }
        }
        /// <summary>
        /// 話者を登録する
        /// wは主人公。dは独白が定義済みである
        /// </summary>
        /// <param name="shortName">便宜上の短い名前</param>
        /// <param name="talker">真の話者を示すPersonオブジェクト</param>
        public void AddTalker(string shortName, Person talker)
        {
            talkers[shortName] = talker;
        }

        /// <summary>
        /// マクロを登録する
        /// </summary>
        /// <param name="macroName">マクロの名前となる1文字</param>
        /// <param name="val">マクロの値</param>
        public void AddMacro(string macroName, string val)
        {
            macros[macroName] = val;
        }

        /// <summary>
        /// コレクションのプレイバック
        /// </summary>
        /// <param name="text">SuperTalkソーステキスト</param>
        /// <param name="collectionId">プレイバックするコレクションID</param>
        /// <param name="ownerModuleId">プレイバックするコレクションを含むモジュール</param>
        public static void CollectionPlay(string text, string collectionId, string ownerModuleId)
        {
            var talk = SuperTalk.CreateSuperTalkFromOwnerModuleId(ownerModuleId);
            talk.Play(text, collectionId);
        }
        private SimpleDynamicObject defaultMacro1;
        private SimpleDynamicObject defaultMacro2;
        /// <summary>
        /// 上書き可能な話者として、w=主人公とd=独白を定義する
        /// </summary>
        public SuperTalk(SimpleDynamicObject defaultMacro1 = null, SimpleDynamicObject defaultMacro2 = null)
        {
            AddTalker("w", DefaultPersons.主人公);
            AddTalker("d", DefaultPersons.独白);
            this.defaultMacro1 = defaultMacro1;
            this.defaultMacro2 = defaultMacro2;
        }
        /// <summary>
        /// 所有者モジュールのIDからマクロの参照への情報を解決してSuperTalkオブジェクトを作成する
        /// </summary>
        /// <param name="ownerModuleId">所有者モジュールのID</param>
        /// <returns>SuperTalkオブジェクト</returns>
        public static SuperTalk CreateSuperTalkFromOwnerModuleId(string ownerModuleId)
        {
            SimpleDynamicObject a = null, b = null;
            var owner = State.loadedModules.FirstOrDefault((m) => m.Id == ownerModuleId);
            if (owner != null)
            {
                var field = owner.GetType().GetField("MyFlags", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field != null) a = field.GetValue(null) as SimpleDynamicObject;
                var field2 = owner.GetType().GetField("MyTempFlags", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field2 != null) b = field2.GetValue(null) as SimpleDynamicObject;
            }
            return new SuperTalk(a, b);
        }
    }
    /// <summary>
    /// SuperTalkテキストに含まれるコレクション情報を抽出する
    /// </summary>
    public class SuperTalkCollections
    {
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public class CG
        {
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string KindId;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string Id;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string Name;
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public class C
        {
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string KindId;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string Id;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string ParentId;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string Name;
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public class ErrorInfo
        {
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public int LineNumber;
            /// <summary>
            /// 内部利用専用です。
            /// </summary>
            public string Reason;
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public Dictionary<string, CG> CollectionGroups = new Dictionary<string, CG>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public Dictionary<string, C> Collections = new Dictionary<string, C>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public Dictionary<string, string> CodeFragments = new Dictionary<string, string>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public List<ErrorInfo> ErrorInfos = new List<ErrorInfo>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public readonly SuperTalkSource SuperTalk;
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public SuperTalkCollections(string superTalk)
        {
            SuperTalk = new SuperTalkSource() { Source = superTalk };
            int lineNumber = 0;
            StringReader reader = new StringReader(superTalk);
            for (; ; )
            {
                string s = reader.ReadLine();
                if (s == null) break;
                lineNumber++;

                s = s.Trim();
                // 空行は無視
                if (s.Length == 0) continue;

                // 汎用コメントを除去
                int commentIndex = s.IndexOf("//");
                if (commentIndex >= 0) s = s.Substring(0, commentIndex);

                // #で始まる行は無条件にコマンド
                if (!s.StartsWith("#")) continue;

                string command, argument = "";
                // paesr 1st token
                // 最初の空白文字までがコマンド名、あとがアーギュメント
                int firstWhiteSpacePos2 = s.IndexOfAny(Constants.WhiteSpaces);
                if (firstWhiteSpacePos2 < 0) command = s.Substring(1);
                else
                {
                    command = s.Substring(1, firstWhiteSpacePos2 - 1);
                    argument = s.Substring(firstWhiteSpacePos2 + 1).Trim();
                }
                string[] args = argument.Trim().Split(Constants.WhiteSpaces, StringSplitOptions.None);
                switch (command)
                {
                    case "C":
                        if (args.Length == 3)
                        {
                            Collections.Add(args[2], new C() { Id = args[2], KindId = args[0], Name = args[1], ParentId = null });
                        }
                        else if (args.Length == 4)
                        {
                            Collections.Add(args[3], new C() { Id = args[3], KindId = args[0], Name = args[1], ParentId = args[2] });
                        }
                        else ErrorInfos.Add(new ErrorInfo() { LineNumber = lineNumber, Reason = "#C must have 3 or 4 arguments" });
                        break;
                    case "CG":
                        if (args.Length != 3) ErrorInfos.Add(new ErrorInfo() { LineNumber = lineNumber, Reason = "#CG must have 3 arguments" });
                        CollectionGroups.Add(args[1], new CG() { Id = args[1], KindId = args[0], Name = args[2] });
                        break;
                    case "CSS":
                        if (args.Length != 1) ErrorInfos.Add(new ErrorInfo() { LineNumber = lineNumber, Reason = "#CSS must have 1 argument" });
                        var sb = new StringBuilder();
                        for (; ; )
                        {
                            string s2 = reader.ReadLine();
                            if (s2 == null) break;
                            lineNumber++;
                            s2 = s2.Trim();
                            if (s2 == "#CSE") break;
                            sb.AppendLine(s2);
                        }
                        CodeFragments.Add(args[0], sb.ToString());
                        break;
                }
                continue;
            }
        }
    }
}
