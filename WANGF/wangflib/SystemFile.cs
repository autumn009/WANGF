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
    /// �V�X�e���t�@�C���𑀍삷��@�\��񋟂��܂��B
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
        /// SystemFile�̃t���O���擾���܂��B
        /// ���݂��Ȃ��t���O���w�肳�ꂽ�ꍇ��0��Ԃ��܂��B
        /// </summary>
        /// <param name="name">�t���O�̖��O�ł��B</param>
        /// <returns>�t���O�̒l�ł��B</returns>
        public static int GetFlag(string name)
        {
            if (flags.ContainsKey(name))
            {
                return flags[name];
            }
            return 0;
        }

        /// <summary>
        /// SystemFile�̃t���O���擾���܂��B
        /// ���݂��Ȃ��t���O���w�肳�ꂽ�ꍇ��""��Ԃ��܂��B
        /// </summary>
        /// <param name="name">�t���O�̖��O�ł��B</param>
        /// <returns>�t���O�̒l�ł��B</returns>
        public static string GetFlagString(string name)
        {
            if (sflags.ContainsKey(name))
            {
                return sflags[name];
            }
            return "";
        }

        /// <summary>
        /// SystemFile�̃t���O��ݒ肵�܂����A�Z�[�u�͍s���܂���B
        /// </summary>
        /// <param name="name">�t���O�̖��O�ł��B</param>
        /// <param name="newValue">�t���O�̒l�ł��B</param>
        public static void SetFlag(string name, int newValue)
        {
            flags[name] = newValue;
            isDirty = true;
            //JournalingWriter.WriteComment("�t���O " + name + "=" + newValue.ToString() );
        }

        /// <summary>
        /// SystemFile�̃t���O��ݒ肵�܂����A�Z�[�u�͍s���܂���B
        /// </summary>
        /// <param name="name">�t���O�̖��O�ł��B</param>
        /// <param name="newValue">�t���O�̒l�ł��B</param>
        public static void SetFlagString(string name, string newValue)
        {
            sflags[name] = newValue;
            isDirty = true;
            //JournalingWriter.WriteComment("s�t���O " + name + "=" + newValue.ToString() );
        }

        /// <summary>
        /// ����R���N�V�������擾�������Ƃ�ʒm���܂��B�Z�[�u�͍s���܂���B
        /// </summary>
        /// <param name="collectionID">�R���N�V������ID</param>
        /// <param name="keyID">�L�[��ID</param>
        /// <param name="subkeyID">�T�u�L�[��ID</param>
        public static void SetCollection(string collectionID, string keyID, string subkeyID)
		{
            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            SetFlag("col_" + collectionID + "_" + keyID + "_" + subkeyID,1);
		}

        /// <summary>
        /// ����R���N�V�����̒l���擾�ς݂����肵�܂��B
        /// </summary>
        /// <param name="collectionID">�R���N�V������ID</param>
        /// <param name="keyID">�L�[��ID</param>
        /// <param name="subkeyID">�T�u�L�[��ID</param>
        /// <returns>�擾���Ă�����True</returns>
        public static bool HasCollection(string collectionID, string keyID, string subkeyID)
        {
            System.Diagnostics.Trace.Assert(!collectionID.Contains("_"));
            System.Diagnostics.Trace.Assert(!keyID.Contains("_"));
            System.Diagnostics.Trace.Assert(!subkeyID.Contains("_"));
            return 0 != GetFlag("col_" + collectionID + "_" + keyID + "_" + subkeyID);
        }

        /// <summary>
        /// �w�肳�ꂽ���W���[���̃R���N�V������S�Ď����Ă��Ȃ����Ƃɂ��܂�
        /// </summary>
        /// <param name="moduleId">�ΏۂƂ��郂�W���[����Id�ł�</param>
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

        // �f�t�H���g�̎����Z�[�u��
        private const int DefaultAutoSaveCount = 3;
		private const string AutoSaveCountFlagName = "AutoSaveCount";
        /// <summary>
        /// �����Z�[�u���鐔�B�Z�b�g���Ă��Z�[�u�͍s���܂���B
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
        /// �f�o�b�O���[�h���ۂ�
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
        /// �V�~�����[�^���Ȃ�True
        /// </summary>
		public static bool InSimulationMode
		{
			get { return inSimulationMode; }
		}
        /// <summary>
        /// �V�~�����[�^���J�n���ꂽ���Ƃ�ʒm���܂��B
        /// </summary>
		public static void StartSimulation()
		{
			inSimulationMode = true;

		}
        /// <summary>
        /// �V�~�����[�^���I���������Ƃ�ʒm���܂��B
        /// </summary>
        public static async Task EndSimulationAsync()
        {
            inSimulationMode = false;
            // �V�~�����[�V�������I�������A�ύX���L�����Z�����邽�߂ɓǂݒ���
            await LoadAsync();
        }

		private static bool inPlaybackMode = false;
        public static bool InPlaybackMode=>inPlaybackMode;
        /// <summary>
        /// �W���[�i�����O�̃v���C�o�b�N���J�n���ꂽ���Ƃ�ʒm���܂��B
        /// </summary>
		public static void StartPlayback()
		{
			inPlaybackMode = true;

		}
        /// <summary>
        /// �W���[�i�����O�̃v���C�o�b�N���I���������Ƃ�ʒm���܂��B
        /// </summary>
        public static void EndPlayback()
		{
			inPlaybackMode = false;
            // �v���C�o�b�N���I�������ASystemFile�ɕۑ�����Ȃ��ύX���܂Ƃ߂ď����o��
            // �ׂ������A���C�����[�v�̃R�}���h�҂��O��SaveIfDirty�����邩�炻��ŏ\���ł���B
		}

        private static bool loaded = false;

        /// <summary>
        /// ���[�h���Ă��Ȃ���Ԃɏ�Ԃ�߂�
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
				// �t�@�C����������Ȃ��ꍇ�͋󂩂�n�߂�
			}
            catch (System.IO.DirectoryNotFoundException)
            {
                //System.Diagnostics.Trace.Fail("DirectoryNotFoundException in loadSub");
                // �f�B���N�g����������Ȃ��ꍇ�͋󂩂�n�߂�
            }

            // �t���O�̓ǂݍ���
            foreach (XmlNode node in doc.SelectNodes("//flag"))
            {
                string key = node.Attributes.GetNamedItem("name").Value;
                int result;
                if (int.TryParse(node.InnerText, out result))
                {
                    flags[key] = result;
                }
            }

            // s�t���O�̓ǂݍ���
            foreach (XmlNode node in doc.SelectNodes("//sflag"))
            {
                string key = node.Attributes.GetNamedItem("name").Value;
                sflags[key] = node.InnerText;
            }

            // �^�C���X�^���v�̓ǂݍ���
            timeStamps.Clear();
            foreach (XmlNode node in doc.SelectNodes("//timeStamp"))
            {
                string id = node.Attributes["id"].Value;
                DateTime val = DateTime.ParseExact(node.Attributes["val"].Value, Constants.DateTimeFormat, null);
                timeStamps.Add(id, val);
            }

			// GUID�̓ǂݍ���
			guid = getStringValue(doc, "//guid", Guid.Empty.ToString());

            loaded = true;
		}

        /// <summary>
        /// �V�X�e���t�@�C���̓ǂݍ��݂ƃo���f�[�V�������s���܂��B��ʃA�v������͌Ăяo���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="doc">�ǂݍ��ޑΏۂ�DOM�ł�</param>
        /// <param name="body">�ǂݍ��ޑΏۂ̃o�C�i���ł�</param>
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
                        loaded = true;  // �ǂݍ��ݍς݂ƌ��Ȃ�
                        throw new ApplicationException("���ɑ��݂���V�X�e�� �t�@�C���͂��̃v���O�����ł͈������Ƃ��ł��܂���B"
							+ "���̂܂܃Q�[����i�s����Ə�����Ԃ���Q�[�����J�n����A�V�X�e�� �t�@�C���͏㏑������܂��B");
					}
				}
				// �t�@�C���̃T�C�Y��4�o�C�g�ɖ����Ȃ��ꍇ�A��O�Ŋ��ɒe����Ă���͂��ł���
				System.Diagnostics.Debug.Assert(readStream.Length >= 4);
				fileImage = new byte[readStream.Length - magicHeader.Length];
				readStream.Read(fileImage, 0, fileImage.Length);
			}
			string decriptedString = EncryptUtil.DecryptString(fileImage, diver81);

			doc.Load(new StringReader(decriptedString));
		}

        /// <summary>
        /// �V�X�e���t�@�C���̓ǂݍ���
        /// (�t�@�C�������݂��Ȃ��ꍇ�̓f�t�H���g�l�ł̏�����)
        /// GUID�̊m���ȕt�^�̂��߂ɑ��݂��郁�\�b�h
        /// GUID�������Ȃ��V�X�e���t�@�C���ł����Ă��A
        /// �V�X�e���t�@�C�������݂��Ȃ��ꍇ�ł�
        /// ������GUID������ĕۑ����Ēl���m�肳����
        /// �V�X�e���t�@�C���̃C���|�[�g���̃����[�h�ɂ��g��
        /// </summary>
        public static async Task LoadAsync()
        {
            await loadSubAsync();
            if (guid != Guid.Empty.ToString()) return;  // ����Guid�擾�ς�
            guid = Guid.NewGuid().ToString();   // �V����Guid�̍쐬
            await SaveAsync();	// �Z�[�u���Ċm�肳����
        }

        private static async Task saveAsync(bool forceToSave)
        {
            if (!loaded) return;    // ���[�h�O�Ȃ�Z�[�u�̓L�����Z�������
            if (!forceToSave && (inSimulationMode || inPlaybackMode)) return;   //	�V�~�����[�V����/�v���C�o�b�N���[�h�̂Ƃ��A�ύX���t�@�C���ɔ��f���Ȃ�

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

                // �t���O�̏�������
                foreach (string key in flags.Keys)
                {
                    writer.WriteStartElement("flag");
                    writer.WriteAttributeString("name", key);
                    writer.WriteString(flags[key].ToString());
                    writer.WriteEndElement();
                }

                // s�t���O�̏�������
                foreach (string key in sflags.Keys)
                {
                    writer.WriteStartElement("sflag");
                    writer.WriteAttributeString("name", key);
                    writer.WriteString(sflags[key]);
                    writer.WriteEndElement();
                }

                // �^�C���X�^���v�̏�������
                foreach (var t in timeStamps.Keys)
                {
                    writer.WriteStartElement("timeStamp");
                    writer.WriteAttributeString("id", t);
                    writer.WriteAttributeString("val", timeStamps[t].ToString(Constants.DateTimeFormat));
                    writer.WriteEndElement();
                }

                // GUID�̏�������
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
        /// �V�X�e���t�@�C�����������݂܂��B��ʓI�ɂ�����Ăяo���܂ŕύX�̓�������݂̂ł���A�t�@�C���ɔ��f����܂���B
        /// </summary>
        /// <param name="forceToSave">�V�~�����[�V����/�v���C�o�b�N���[�h�ł������I�ɏ������ނƂ���true</param>
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
            // ��p�����ł���
            await UI.Actions.tellAssertionFailedAsync(msg + ":" + eventType + ":" + id);
        }
#else
        private static void writeEventLog(string msg, EventLogEntryType eventType = EventLogEntryType.Warning, int id = 1)
        {
            // �{����ANGF�̖��O�ŏ������݂������A��ʃ��[�U�[�����ł̓C�x���g�\�[�X��
            // �����A�쐬�ł��Ȃ��̂ŁA��ނȂ�Application�̖��O�ŏ�������
            EventLog.WriteEntry("Application", "ANGF: " + msg, EventLogEntryType.Warning, id);
        }
#endif

        /// <summary>
        /// �V�X�e���t�@�C�����������݂܂��B��ʓI�ɂ�����Ăяo���܂ŕύX�̓�������݂̂ł���A�t�@�C���ɔ��f����܂���B
        /// �V�~�����[�V����/�v���C�o�b�N���[�h�̂Ƃ��A�������݂͎��s����܂���B
        /// </summary>
        public static async Task SaveAsync()
        {
            await SaveAsync(false);
        }

        /// <summary>
        /// isDirty��true�̏ꍇ�̂�Save���s���܂��B
        /// </summary>
        /// <returns></returns>
        public static async Task SaveIfDirtyAsync()
        {
            if (isDirty) await SaveAsync(false);
        }

        /// <summary>
        /// �V�X�e���t�@�C���̃t���O�����������܂��B��ʃA�v������Ăяo���ׂ��ł͂���܂���B
        /// </summary>
		public static void AllClearForNewPlay()
		{
			// ����ŏ\�����͗v�������낤
			flags.Clear();
            isDirty = true;
        }

        private static Dictionary<string, DateTime> timeStamps = new Dictionary<string, DateTime>();
        /// <summary>
        /// ����V�i���I���Ō�Ƀv���C�������t������ݒ肵�܂��B
        /// </summary>
        /// <param name="guid">�X�^�[�g�A�b�v���W���[����Id�ł�</param>
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
        /// ���郂�W���[�����Ō�Ƀv���C�������t�����𓾂܂��B
        /// </summary>
        /// <param name="id">�X�^�[�g�A�b�v���W���[����Id�ł�</param>
        /// <returns>���t�����ł��B�v���C���Ă��Ȃ����̂�DateTime.MinValue.AddMinutes(1.0)�ł��B�V�X�e���݂̂͏��DateTime.MinValue�ł�</returns>
        public static DateTime GetTimeStamp(string id)
        {
            if (timeStamps.ContainsKey(id)) return timeStamps[id];
			if (id == "{93029964-704B-4d38-BE1D-EDB6182602E8}") return DateTime.MinValue;
            return DateTime.MinValue.AddMinutes(1.0);
        }

        /// <summary>
        /// ���[�U�[��Id��Ԃ��܂��B
        /// </summary>
        public static string UserID
		{
			get { return guid; }
		}
    }
}
