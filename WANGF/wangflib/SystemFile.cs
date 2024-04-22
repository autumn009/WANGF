using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// システムファイルを操作する機能を提供します。
    /// </summary>
    public class SystemFile
    {
		private static string diver81
		{
			get
			{
				byte[] milk = { (byte)'S', (byte)'A', (byte)'Y', (byte)'0', 
					(byte)'C', (byte)'H', (byte)'A', (byte)'N', (byte)'5' };
				char[] milk2 = new char[milk.Length];
				for (int i = 0; i < milk.Length; i++)
				{
					milk2[i] = (char)milk[i];
				}
				return new string(milk2);
			}
		}

		private static string guid = Guid.Empty.ToString();
		private static Dictionary<string, int> flags = new Dictionary<string, int>();
        private static Dictionary<string, string> sflags = new Dictionary<string, string>();
        private static bool isDirty = false;

        /// <summary>
        /// SystemFileのフラグを取得します。
        /// 存在しないフラグが指定された場合は0を返します。
        /// </summary>
        /// <param name="name">フラグの名前です。</param>
        /// <returns>フラグの値です。</returns>
        public static int GetFlag(string name)
        {
            if (flags.ContainsKey(name))
            {
                return flags[name];
            }
            return 0;
        }

        /// <summary>
        /// SystemFileのフラグを取得します。
        /// 存在しないフラグが指定された場合は""を返します。
        /// </summary>
        /// <param name="name">フラグの名前です。</param>
        /// <returns>フラグの値です。</returns>
        public static string GetFlagString(string name)
        {
            if (sflags.ContainsKey(name))
            {
                return sflags[name];
            }
            return "";
        }

        /// <summary>
        /// SystemFileのフラグを設定しますが、セーブは行いません。
        /// </summary>
        /// <param name="name">フラグの名前です。</param>
        /// <param name="newValue">フラグの値です。</param>
        public static void SetFlag(string name, int newValue)
        {
            flags[name] = newValue;
            isDirty = true;
            //JournalingWriter.WriteComment("フラグ " + name + "=" + newValue.ToString() );
        }

        /// <summary>
        /// SystemFileのフラグを設定しますが、セーブは行いません。
        /// </summary>
        /// <param name="name">フラグの名前です。</param>
        /// <param name="newValue">フラグの値です。</param>
        public static void SetFlagString(string name, string newValue)
        {
            sflags[name] = newValue;
            isDirty = true;
            //JournalingWriter.WriteComment("sフラグ " + name + "=" + newValue.ToString() );
        }

        /// <summary>
        /// あるコレクションを取得したことを通知します。セーブは行いません。
        /// </summary>
        /// <param name="collectionID">コレクションのID</param>
        /// <param name="keyID">キーのID</param>
        /// <param name="subkeyID">サブキーのID</param>
        public static void SetCollection(string collectionID, string keyID, string subkeyID)
		{
            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            SetFlag("col_" + collectionID + "_" + keyID + "_" + subkeyID,1);
		}

        /// <summary>
        /// あるコレクションの値を取得済みか判定します。
        /// </summary>
        /// <param name="collectionID">コレクションのID</param>
        /// <param name="keyID">キーのID</param>
        /// <param name="subkeyID">サブキーのID</param>
        /// <returns>取得していたらTrue</returns>
        public static bool HasCollection(string collectionID, string keyID, string subkeyID)
        {
            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            return 0 != GetFlag("col_" + collectionID + "_" + keyID + "_" + subkeyID);
        }

        /// <summary>
        /// 指定されたモジュールのコレクションを全て持っていないことにします
        /// </summary>
        /// <param name="moduleId">対象とするモジュールのIdです</param>
        public static void InitCollectionsByModule(string moduleId)
        {
            foreach (var collection in SimpleName<Collection>.List.Values)
            {
                foreach (var collectionItem in collection.Collections)
                {
                    if (collectionItem.GetRawSubItems != null)
                    {
                        foreach (var subitem in collectionItem.GetSubItems())
                        {
                            if (HasCollection(collection.Id, collectionItem.Id, subitem.Id))
                            {
                                if (subitem.OwnerModuleId != moduleId) continue;
                                SetFlag("col_" + collection.Id + "_" + collectionItem.Id + "_" + subitem.Id, 0);
                                Flags.Collections["col_" + collection.Id + "_" + collectionItem.Id + "_" + subitem.Id] = 0;
                            }
                        }
                    }
                    else
                    {
                        if (HasCollection(collection.Id, collectionItem.Id, ""))
                        {
                            if (collectionItem.OwnerModuleId != moduleId) continue;
                            SetFlag("col_" + collection.Id + "_" + collectionItem.Id + "_", 0);
                            Flags.Collections["col_" + collection.Id + "_" + collectionItem.Id + "_"] = 0;
                        }
                    }
                }
            }
        }

        // デフォルトの自動セーブ数
        private const int DefaultAutoSaveCount = 3;
		private const string AutoSaveCountFlagName = "AutoSaveCount";
        /// <summary>
        /// 自動セーブする数。セットしてもセーブは行いません。
        /// </summary>
		public static int AutoSaveCount
		{
			get
			{
				if (flags.ContainsKey(AutoSaveCountFlagName))
				{
					return flags[AutoSaveCountFlagName];
				}
				return DefaultAutoSaveCount;
			}
			set
			{
				flags[AutoSaveCountFlagName] = value;
			}
		}

        private const string IsDebugModeFlagName = "IsDebugMode";
        /// <summary>
        /// デバッグモードか否か
        /// </summary>
		public static bool IsDebugMode
        {
            get => flags.ContainsKey(IsDebugModeFlagName) ? flags[IsDebugModeFlagName] != 0 : false;
            set
            {
                flags[IsDebugModeFlagName] = value ? 1 : 0;
                isDirty = true;
            }
        }

        private static string getStringValue(XmlDocument doc, string xpath, string defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            return node.InnerText;
        }

        private static bool getBoolValue(XmlDocument doc, string xpath, bool defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            bool result;
            if (bool.TryParse(node.InnerText, out result)) return result;
            return defaultValue;
        }

		private static bool inSimulationMode = false;
        /// <summary>
        /// シミュレータ内ならTrue
        /// </summary>
		public static bool InSimulationMode
		{
			get { return inSimulationMode; }
		}
        /// <summary>
        /// シミュレータが開始されたことを通知します。
        /// </summary>
		public static void StartSimulation()
		{
			inSimulationMode = true;

		}
        /// <summary>
        /// シミュレータが終了したことを通知します。
        /// </summary>
        public static async Task EndSimulationAsync()
        {
            inSimulationMode = false;
            // シミュレーションが終わったら、変更をキャンセルするために読み直す
            await LoadAsync();
        }

		private static bool inPlaybackMode = false;
        public static bool InPlaybackMode=>inPlaybackMode;
        /// <summary>
        /// ジャーナリングのプレイバックが開始されたことを通知します。
        /// </summary>
		public static void StartPlayback()
		{
			inPlaybackMode = true;

		}
        /// <summary>
        /// ジャーナリングのプレイバックが終了したことを通知します。
        /// </summary>
        public static void EndPlayback()
		{
			inPlaybackMode = false;
            // プレイバックが終わったら、SystemFileに保存されない変更をまとめて書き出す
            // べきだが、メインループのコマンド待ち前にSaveIfDirtyがあるからそれで十分である。
		}

        private static bool loaded = false;

        /// <summary>
        /// ロードしていない状態に状態を戻す
        /// </summary>
        public static void SetAsNotLoaded()
        {
            loaded = false;
        }

        private static async Task loadSubAsync()
        {
			flags.Clear();
			XmlDocument doc = new XmlDocument();
			try
			{
                var body = await UI.Actions.LoadFileAsync("SystemFile","SystemFile.bin");
				LoadSystemFileAndValidation(doc, body);
			}
			catch (System.IO.FileNotFoundException)
			{
                //System.Diagnostics.Trace.Fail("FileNotFoundException in loadSub");
				// ファイルが見つからない場合は空から始める
			}
            catch (System.IO.DirectoryNotFoundException)
            {
                //System.Diagnostics.Trace.Fail("DirectoryNotFoundException in loadSub");
                // ディレクトリが見つからない場合は空から始める
            }

            // フラグの読み込み
            foreach (XmlNode node in doc.SelectNodes("//flag"))
            {
                string key = node.Attributes.GetNamedItem("name").Value;
                int result;
                if (int.TryParse(node.InnerText, out result))
                {
                    flags[key] = result;
                }
            }

            // sフラグの読み込み
            foreach (XmlNode node in doc.SelectNodes("//sflag"))
            {
                string key = node.Attributes.GetNamedItem("name").Value;
                sflags[key] = node.InnerText;
            }

            // タイムスタンプの読み込み
            timeStamps.Clear();
            foreach (XmlNode node in doc.SelectNodes("//timeStamp"))
            {
                string id = node.Attributes["id"].Value;
                DateTime val = DateTime.ParseExact(node.Attributes["val"].Value, Constants.DateTimeFormat, null);
                timeStamps.Add(id, val);
            }

			// GUIDの読み込み
			guid = getStringValue(doc, "//guid", Guid.Empty.ToString());

            loaded = true;
		}

        /// <summary>
        /// システムファイルの読み込みとバリデーションを行います。一般アプリからは呼び出すべきではありません。
        /// </summary>
        /// <param name="doc">読み込む対象のDOMです</param>
        /// <param name="body">読み込む対象のバイナリです</param>
		public static void LoadSystemFileAndValidation(XmlDocument doc, byte[] body)
		{
            if (body == null || body.Length == 0) return;
			byte[] fileImage;
            using (Stream readStream = new MemoryStream(body))
			{
				byte[] magicHeader = new byte[4];
				int readBytes = readStream.Read(magicHeader, 0, magicHeader.Length);
				for (int i = 0; i < 4; i++)
				{
					if (magicHeader[i] != Constants.SystemFileMagicHeader[i] || i >= readBytes)
					{
                        loaded = true;  // 読み込み済みと見なす
                        throw new ApplicationException("既に存在するシステム ファイルはこのプログラムでは扱うことができません。"
							+ "このままゲームを進行すると初期状態からゲームが開始され、システム ファイルは上書きされます。");
					}
				}
				// ファイルのサイズが4バイトに満たない場合、例外で既に弾かれているはずである
				System.Diagnostics.Debug.Assert(readStream.Length >= 4);
				fileImage = new byte[readStream.Length - magicHeader.Length];
				readStream.Read(fileImage, 0, fileImage.Length);
			}
			string decriptedString = EncryptUtil.DecryptString(fileImage, diver81);

			doc.Load(new StringReader(decriptedString));
		}

        /// <summary>
        /// システムファイルの読み込み
        /// (ファイルが存在しない場合はデフォルト値での初期化)
        /// GUIDの確実な付与のために存在するメソッド
        /// GUIDを持たないシステムファイルであっても、
        /// システムファイルが存在しない場合でも
        /// ここでGUIDを作って保存して値を確定させる
        /// システムファイルのインポート時のリロードにも使う
        /// </summary>
        public static async Task LoadAsync()
        {
            await loadSubAsync();
            if (guid != Guid.Empty.ToString()) return;  // 既にGuid取得済み
            guid = Guid.NewGuid().ToString();   // 新しいGuidの作成
            await SaveAsync();	// セーブして確定させる
        }

        private static async Task saveAsync(bool forceToSave)
        {
            if (!loaded) return;    // ロード前ならセーブはキャンセルされる
            if (!forceToSave && (inSimulationMode || inPlaybackMode)) return;   //	シミュレーション/プレイバックモードのとき、変更をファイルに反映しない

            await UI.Actions.SaveFileAsync("SystemFile", "SystemFile.bin", SaveToArray());
            isDirty = false;
        }

        public static byte[] SaveToArray()
        {
            StringWriter sWriter = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sWriter);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("states");

                // フラグの書き込み
                foreach (string key in flags.Keys)
                {
                    writer.WriteStartElement("flag");
                    writer.WriteAttributeString("name", key);
                    writer.WriteString(flags[key].ToString());
                    writer.WriteEndElement();
                }

                // sフラグの書き込み
                foreach (string key in sflags.Keys)
                {
                    writer.WriteStartElement("sflag");
                    writer.WriteAttributeString("name", key);
                    writer.WriteString(sflags[key]);
                    writer.WriteEndElement();
                }

                // タイムスタンプの書き込み
                foreach (var t in timeStamps.Keys)
                {
                    writer.WriteStartElement("timeStamp");
                    writer.WriteAttributeString("id", t);
                    writer.WriteAttributeString("val", timeStamps[t].ToString(Constants.DateTimeFormat));
                    writer.WriteEndElement();
                }

                // GUIDの書き込み
                writer.WriteElementString("guid", guid);
            }
            finally
            {
                writer.Close();
            }
            byte[] fileImage = EncryptUtil.EncryptString(sWriter.ToString(), diver81);
            return Constants.SystemFileMagicHeader.Concat(fileImage).ToArray();
        }

        /// <summary>
        /// システムファイルを書き込みます。一般的にこれを呼び出すまで変更はメモリ上のみであり、ファイルに反映されません。
        /// </summary>
        /// <param name="forceToSave">シミュレーション/プレイバックモードでも強制的に書き込むときはtrue</param>
        public static async Task SaveAsync(bool forceToSave)
        {
            try
            {
                await saveAsync(forceToSave);
                //writeEventLog("check save");
            }
            catch (IOException ex)
            {
                await writeEventLogAsync(ex.ToString());
            }
        }

#if MYBLAZORAPP
        private static async Task writeEventLogAsync(string msg, object eventType = null, int id = 1)
        {
            // 代用処理である
            await UI.Actions.tellAssertionFailedAsync(msg + ":" + eventType + ":" + id);
        }
#else
        private static void writeEventLog(string msg, EventLogEntryType eventType = EventLogEntryType.Warning, int id = 1)
        {
            // 本当はANGFの名前で書き込みたいが、一般ユーザー権限ではイベントソースを
            // 検索、作成できないので、やむなくApplicationの名前で書き込む
            EventLog.WriteEntry("Application", "ANGF: " + msg, EventLogEntryType.Warning, id);
        }
#endif

        /// <summary>
        /// システムファイルを書き込みます。一般的にこれを呼び出すまで変更はメモリ上のみであり、ファイルに反映されません。
        /// シミュレーション/プレイバックモードのとき、書き込みは実行されません。
        /// </summary>
        public static async Task SaveAsync()
        {
            await SaveAsync(false);
        }

        /// <summary>
        /// isDirtyがtrueの場合のみSaveを行います。
        /// </summary>
        /// <returns></returns>
        public static async Task SaveIfDirtyAsync()
        {
            if (isDirty) await SaveAsync(false);
        }

        /// <summary>
        /// システムファイルのフラグを初期化します。一般アプリから呼び出すべきではありません。
        /// </summary>
		public static void AllClearForNewPlay()
		{
			// これで十分かは要検討だろう
			flags.Clear();
            isDirty = true;
        }

        private static Dictionary<string, DateTime> timeStamps = new Dictionary<string, DateTime>();
        /// <summary>
        /// あるシナリオを最後にプレイした日付時刻を設定します。
        /// </summary>
        /// <param name="guid">スタートアップモジュールのIdです</param>
        public static void SetLastPlayDateTime(string guid)
        {
            if (timeStamps.ContainsKey(guid))
            {
                timeStamps[guid] = DateTime.Now;
            }
            else
            {
                timeStamps.Add(guid,DateTime.Now);
            }
            isDirty = true;
        }

        /// <summary>
        /// あるモジュールを最後にプレイした日付時刻を得ます。
        /// </summary>
        /// <param name="id">スタートアップモジュールのIdです</param>
        /// <returns>日付時刻です。プレイしていないものはDateTime.MinValue.AddMinutes(1.0)です。システムのみは常にDateTime.MinValueです</returns>
        public static DateTime GetTimeStamp(string id)
        {
            if (timeStamps.ContainsKey(id)) return timeStamps[id];
			if (id == "{93029964-704B-4d38-BE1D-EDB6182602E8}") return DateTime.MinValue;
            return DateTime.MinValue.AddMinutes(1.0);
        }

        /// <summary>
        /// ユーザーのIdを返します。
        /// </summary>
        public static string UserID
		{
			get { return guid; }
		}
    }
}
