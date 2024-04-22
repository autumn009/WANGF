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
    /// �ω���������܂Ƃ߂Ĉ����܂��B�t���O�ȊO�Ń��[�h�ƃZ�[�u�̑Ώۂł��B
    /// </summary>
    public class State
    {
        /// <summary>
        /// Web�v���C���[�ŉғ����Ȃ�true
        /// </summary>
        //public static bool IsInWebPlayer { get; set; }

        /// <summary>
        /// �v���b�g�t�H�[����
        /// </summary>
        public static string PlatformName { get; set; }

        /// <summary>
        /// ���O�C�����Ă��郆�[�U�[��ID�BWeb�v���C���[�ł̂ݎg�p�����A
        /// �f�X�N�g�b�v�܂��̓��O�C�����Ă��Ȃ��ꍇ��null�̂܂�
        /// </summary>
        public static string UserId { get; set; }

        /// <summary>
        /// �Q�X�g�Ȃ�true
        /// </summary>
        public static bool IsGuestAccess => string.IsNullOrWhiteSpace(State.UserId);

        /// <summary>
        /// ���݈ʒu�Ɠǂݏ������܂��B
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
        /// �����ăj���[�Q�[���̂��߂̏��������s���܂��B
        /// </summary>
        public static void ClearFor�����ăj���[�Q�[��()
        {
            �����̋N������ = DateTime.MinValue;
            �����̏A�Q���� = DateTime.MinValue;
        }

        /// <summary>
        /// �S�Ă̏����N���A���A�V�����Q�[���ɔ����܂��B
        /// </summary>
        /// <param name="enableProc">�w��t�B�[���h�̏����������邩�ۂ��𔻒肵�܂�</param>
        /// <exception cref="ApplicationException">�g�p�ł��Ȃ��^�����o����܂����B</exception>
        public static void Clear(Func<FieldInfo,bool> enableProc)
        {
            // CurrentPlaceId�͈ꎞ�Ҕ��̕K�v����
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
                        else throw new ApplicationException("�^" + n.FullName + "�͎������[�h�Z�[�u�Ɏg�p�ł��܂���B");
                    }
                    return default;
                });
            equipSetItems.Clear();
            SetEquipSet(State.�����Ȃ�, new EquipSet());
            ClearFor�����ăj���[�Q�[��();
            Flags.CurrentPlaceId = id;
        }
        /// <summary>
        /// �S�Ă̏����N���A���A�V�����Q�[���ɔ����܂��B
        /// </summary>
        public static void Clear()
        {
            Clear((field) => true);
        }

        /// <summary>
        /// �T�[�r�X�p�ł��B�����ƕԋp�l�̖����f���Q�[�g��񋟂��܂��B
        /// </summary>
        delegate void MyMethodInvoker();

        /// <summary>
        /// �w�肳�ꂽPlace�̏ꏊ�ֈړ����܂��B
        /// World������World�ƈႤ�ꍇ��World���ړ����܂�
        /// </summary>
        /// <param name="distPlace">�ړ���̏ꏊ�ł��B</param>
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
        /// �w�肳�ꂽId�̏ꏊ�ֈړ����܂��B
        /// </summary>
        /// <param name="id">�ړ���̏ꏊ�ł��B</param>
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
        /// �w�肳�ꂽId�̏ꏊ�ֈړ����܂��B
        /// </summary>
        /// <param name="worldid">�ړ���̃��[���h�ł��B</param>
        /// <param name="placeid">�ړ���̏ꏊ�ł��B</param>
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
        /// ��؂̏����𔲂��ɂ��Č��݈ʒu���X�V���܂�
        /// </summary>
        /// <param name="distPlace">�V�������݈ʒu�ł��B</param>
        public static void SetPlaceForSimulator(Place distPlace)
        {
            CurrentPlace = distPlace;
        }

        /// <summary>
        /// �w�肳�ꂽId�̏ꏊ�փ����N�ňړ����܂����A���v���Ԃ��v�Z���ăQ�[�������ݎ����ɉ��Z���܂��B
        /// </summary>
        /// <param name="distPlace"></param>
        public static async Task GoToMyLinkAsync(Place distPlace)
        {
            await goToAsync(distPlace,
                ()=>
                {
                    State.GoTime(General.�����N�ړ����Ԍv�Z(State.CurrentPlace, distPlace), false, false);
                });
        }
        /// <summary>
        /// �w�肳�ꂽId�̏ꏊ�ֈړ����܂����A���v���Ԃ��v�Z���ăQ�[�������ݎ����ɉ��Z���܂��B
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
        private static async Task goToAsync(Place distPlace, MyMethodInvoker �ړ����Ԍv�Z)
        {
            if (CurrentPlace == distPlace) return;
            await CurrentPlace.OnLeaveingAsync();
            �ړ����Ԍv�Z();
            foreach (var item in State.loadedModules)
            {
                bool r = await item.OnAfterMoveAsync(CurrentPlace.Id, distPlace.Id);
                if (r == false) break;
            }
            CurrentPlace = distPlace;
            await CurrentPlace.OnEnteringAsync();
        }

        /// <summary>
        /// �v���O�����̎��s�𒆒f������
        /// (currentPlace��PlaceNull�I�u�W�F�N�g�̎�
        /// ���C���v���O�����̃��C�����[�v�͏I�����Ȃ���΂Ȃ�Ȃ�)
        /// </summary>
        public static void Terminate()
        {
            CurrentPlace = Places.PlaceNull;
        }

        /// <summary>
        /// �w��A�C�e���̏��L����Ԃ��܂��B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�܂��B</param>
        /// <returns>���ݏ��L���Ă��鐔�ł�</returns>
        public static int GetItemCount(Item item)
        {
            return Flags.�A�C�e�����L���t���O�Q[item.Id.ToString()];
        }

        /// <summary>
        /// �w��A�C�e���̏��L����Ԃ��܂��B
        /// </summary>
        /// <param name="id">�ΏۂƂȂ�A�C�e����id���w�肵�܂��B</param>
        /// <returns>���ݏ��L���Ă��鐔�ł�</returns>
        public static int GetItemCount(string id)
        {
            return Flags.�A�C�e�����L���t���O�Q[id];
        }

        /// <summary>
        /// ���ݏ��L���Ă���A�C�e���̌���ύX���܂��B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�܂��B</param>
        /// <param name="count">�V�������L���Ă��鐔�ł��B</param>
        public static void SetItemCount(Item item, int count)
        {
            Flags.�A�C�e�����L���t���O�Q[item.Id.ToString()] = count;
        }

        /// <summary>
        /// ���ݏ��L���Ă���A�C�e���̌���ύX���܂��B
        /// </summary>
        /// <param name="id">�ΏۂƂȂ�A�C�e����id���w�肵�܂��B</param>
        /// <param name="count">�V�������L���Ă��鐔�ł��B</param>
        public static void SetItemCount(string id, int count)
        {
            Flags.�A�C�e�����L���t���O�Q[id] = count;
        }

        /// <summary>
        /// �A�C�e���̎g�p�񐔂�Ԃ��܂��B�g�p�񐔂̈Ӗ��͉��p�\�t�g�ˑ��ł��B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�܂��B</param>
        /// <returns>�A�C�e���̎g�p�񐔂ł��B</returns>
        public static int GetUsedCount(Item item)
        {
            return Flags.�A�C�e���g�p�񐔃t���O�Q[item.Id.ToString()];
        }

        /// <summary>
        /// �A�C�e���̎g�p�񐔂�Ԃ��܂��B�g�p�񐔂̈Ӗ��͉��p�\�t�g�ˑ��ł��B
        /// </summary>
        /// <param name="id">�ΏۂƂȂ�A�C�e�����w�肵�Ă��܂��B</param>
        /// <returns>�A�C�e���̎g�p�񐔂ł��B</returns>
        public static int GetUsedCount(string id)
        {
            return Flags.�A�C�e���g�p�񐔃t���O�Q[id];
        }

        /// <summary>
        /// �A�C�e���̎g�p�񐔂�ݒ肵�܂��B�g�p�񐔂̈Ӗ��͉��p�\�t�g�ˑ��ł��B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�Ă��܂��B</param>
        /// <param name="count">�V�����A�C�e���̎g�p�񐔂ł��B</param>
        public static void SetUsedCount(Item item, int count)
        {
            Flags.�A�C�e���g�p�񐔃t���O�Q[item.Id.ToString()] = count;
        }

        /// <summary>
        /// �A�C�e���̎g�p�񐔂����Z���܂��B�g�p�񐔂̈Ӗ��͉��p�\�t�g�ˑ��ł��B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�Ă��܂��B</param>
        /// <param name="count">���Z����g�p�񐔂ł��B</param>
        public static void AddUsedCount(Item item, int count)
        {
            if (item.IsItemNull) return;
            Flags.�A�C�e���g�p�񐔃t���O�Q[item.Id.ToString()] += count;
        }

        /// <summary>
        /// �A�C�e������肵���ꍇ�Ɏg�p���܂��B
        /// �w��A�C�e���̏��L����1�����Z���A�g�p�񐔂�0�ɂ��܂��B
        /// �ő吔�𒴂����ꍇ�͉������܂���B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�Ă��܂��B</param>
        public static void GetItem(Item item)
        {
            // ���ɍő吔�����Ă�����A����ȏ�͎��ĂȂ�
            if (GetItemCount(item) >= item.Max) return;
            SetItemCount(item, GetItemCount(item) + 1);
            SetUsedCount(item, 0);
        }

        /// <summary>
        /// �A�C�e�����������ꍇ�Ɏg�p���܂��B
        /// �w��A�C�e���̏��L������1�������A�c��������0�Ȃ�g�p�񐔂�0�ɂ��܂��B
        /// ���Ƃ���0�ȉ��̏ꍇ�͉������܂���B
        /// </summary>
        /// <param name="item">�ΏۂƂȂ�A�C�e�����w�肵�Ă��܂��B</param>
        public static void LostItem(Item item)
        {
            if (GetItemCount(item) <= 0) return;
            SetItemCount(item, GetItemCount(item) - 1);
            if (GetItemCount(item) == 0)
            {
                SetUsedCount(item, 0);	// ���������Ă��Ȃ��̂Ŏg�p�J�E���g�ɈӖ��͂Ȃ�
            }
        }

        private static int getIntValue(XmlNode doc, string xpath, int defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node == null) return defaultValue;
            int result = 0;
            int.TryParse(node.InnerText, out result);   // �p�[�X�ł��Ȃ��ꍇ��0�ɂ��Ă���
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
        /// �t�@�C���̓ǂݍ��݂��s���܂��B
        /// </summary>
        /// <param name="filename">�t�@�C�����ł��B</param>
        /// <param name="category">�J�e�S�����ł��B</param>
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
                        throw new ApplicationException("�ǂݍ��񂾃t�@�C���͂��̃v���O�����ł͈������Ƃ��ł��܂���B");
                    }
                }
                // �t�@�C���̃T�C�Y��4�o�C�g�ɖ����Ȃ��ꍇ�A��O�Ŋ��ɒe����Ă���͂��ł���
                System.Diagnostics.Debug.Assert(readStream.Length >= 4);
                fileImage = new byte[readStream.Length - magicHeader.Length];
                readStream.Read(fileImage, 0, fileImage.Length);
            }
        
            string decriptedString = EncryptUtil.DecryptString(fileImage, diver81);

            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(decriptedString));

            General.CallAllModuleMethod((m) => { m.OnLoadStart(doc); return default; });

            // �t���O�̓ǂݍ���
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

            // World�̖������o�[�W�����ŏ������f�[�^��ǂݍ��񂾏ꍇ�ł��݊������
            if (string.IsNullOrWhiteSpace(Flags.CurrentWorldId)) Flags.CurrentWorldId = Constants.DefaultWordId;

            // ���t������Now�Ƃ̓���
            //Flags.ResetNow();

            // �����̋N������
            �����̋N������ = getDateTimeValue(doc, "//wakeup", DateTime.MinValue);
            �����̏A�Q���� = getDateTimeValue(doc, "//sleep", DateTime.MinValue);

            // �X�P�W���[���̉ϓ��t����
            // �R�[�f�B�l�[�g
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

            // �����ɑ���������

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
                else throw new ApplicationException("�_�C�i�~�b�N�ȃt���O�ŃT�|�[�g����Ă��Ȃ��^�ł��B" + val.GetType().FullName + "\r\n�^��bool, int string�̂����ꂩ�ł���K�v������܂��B");
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
        /// �t�@�C���̃Z�[�u���s���܂��B
        /// </summary>
        /// <param name="Category">�J�e�S���ł��B</param>
        /// <param name="filename">�t�@�C�����ł��B</param>
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

                // �����̋N������
                writer.WriteElementString("wakeup", �����̋N������.ToString(Constants.DateTimeFormat));
                writer.WriteElementString("sleep", �����̏A�Q����.ToString(Constants.DateTimeFormat));

                // �R�[�f�B�l�[�g
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

                // �����ɑ���������

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
        /// �N�������ł��B���݂̂Ŏ�����܂��B
        /// </summary>
        public static int �N������
        {
            get { return Flags.�����T�C�N���N�_����; }
        }

        /// <summary>
        /// �N���\�莞���ł��B���݂̂Ŏ�����܂��B
        /// </summary>
        public static int �A�Q����
        {
            get { return (�N������ + 16) % 24; }
        }

        /// <summary>
        /// �����̋N�������ł��B
        /// </summary>
        public static DateTime �����̋N������;
        /// <summary>
        /// �����̏A�Q�\�莞���ł��B
        /// </summary>
        public static DateTime �����̏A�Q����;

        private static void �N��()
        {
            �����̋N������ = Flags.Now;
            �����̏A�Q���� = �����̋N������.AddHours(16);
            Flags.�N����++;
        }

        /// <summary>
        /// �����̋N�������ł��BOnBeforeSleepAsync�ŎQ�Ƃ��邽�߂̂��̂ł��B
        /// </summary>
        public static DateTime NextMorning;

        /// <summary>
        /// ���Ԃ�i�߂�B���Ԃ�ύX����S�Ă̍�Ƃ͂��̃��\�b�h���o�R���čs���K�v������
        /// </summary>
        /// <param name="minutes">�i�߂鎞��(���P��)</param>
        /// <param name="hasOtherEyes">���l�̖ڂ����邩</param>
        /// <param name="enableEvent">���̃��\�b�h���ŃC�x���g�̔����������邩</param>
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
        /// �w�蕪�������Ԃ�i�߂܂��B
        /// </summary>
        /// <param name="minutes">�i�߂鎞�Ԃ𕪂Ŏw�肵�܂��B</param>
        public static void GoTime(int minutes)
        {
            GoTime(minutes, false, true);
        }

        /// <summary>
        /// �����Ɏ��Ԃ�i�߂܂��B
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
                // �����A�ҋ@�\����
                await State.WarpToAsync(defaultPlaceId);
            }

            // ���̋N�������́A�����̏A�Q����+8���Ԃł���
            DateTime nextMorning = �����̏A�Q����.AddHours(8);
            // ���ɂ��̎��Ԃ��߂��Ă���Η����ȍ~�ɌJ��z��
            if (Flags.Now >= nextMorning)
            {
                // ���̎�������2���ȏ��Ԃ��Ƃ��ł��Ȃ�
                var days = 1;
                //System.Diagnostics.Debug.WriteLine($"GoNextDayMorningAsync: days={days}");
                nextMorning = nextMorning.AddDays(days);
            }
            State.NextMorning = nextMorning;

            // �����A�����̏A�Q�����ɒB���Ă��Ȃ���΁A�����̏A�Q�����܂Ŏ��Ԃ�i�߂�
            if (�����̏A�Q���� > Flags.Now)
            {
                int minutes = (int)(�����̏A�Q���� - Flags.Now).TotalMinutes;
                await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now.AddMinutes(minutes));
                // �C�x���g�̌��ʂɊ֌W�Ȃ�������x�v�Z���Ď��Ԃ�i�߂�
                if (�����̏A�Q���� > Flags.Now)
                {
                    int minutes2 = (int)(�����̏A�Q���� - Flags.Now).TotalMinutes;
                    State.GoTime(minutes2);
                }
            }
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnBeforeSleepAsync()) return;
            }

            GoTime((int)(nextMorning - Flags.Now).TotalMinutes);
            DefaultPersons.�V�X�e��.Say("�c�c{0}�͖���ɗ����܂����B", General.GetMyName());
            await UI.Actions.sleepFlashAsync();
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnSleepingAsync()) return;
            }
            �N��();
            DefaultPersons.�V�X�e��.Say("{0}�͖ڊo�߂܂����B", General.GetMyName());

            //JournalingWriter.WriteComment(Flags.Now.ToString("MMddHHmm") + "�̖ڊo��");

            General.IncrementLastSleepDateCount();

            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnAfterSleepAsync()) return;
            }
            foreach (var n2 in State.loadedModules)
            {
                if (!await n2.OnStartTodayAsync()) return;
            }

            // �W���[�i�����O�t�@�C���̃v���C�o�b�N����
            // �����Z�[�u�����Ȃ�
            if (!UI.Actions.isJournalFilePlaying())
            {
                try
                {
                    await General.�����Z�[�uAsync();
                }
                catch (Exception e)
                {
                    DefaultPersons.�V�X�e��.Say("�����Z�[�u�Ɏ��s���܂����B({0})", e.Message);
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

        // �ȉ���EquipSets�̃��\�b�h�Q�̓X���b�h�Z�[�t�ł͂Ȃ�
        // �̂����A�����I�ɃX���b�h�Ԃł̃A�N�Z�X�����͂Ȃ��c�c�ƍl���ĕ��u
        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
        public static string[] GetEquipSets()
        {
            // ��Ƀ\�[�g���Ă���
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
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
        public static bool IsEquipSetName(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name) return true;
            }
            return false;
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="oldName">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <param name="newName">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
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
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
        public static EquipSet GetEquipSet(string name)
        {
            foreach (EquipListItem item in equipSetItems)
            {
                if (item.Name == name) return item.Set;
            }
            return null;
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <param name="newSet">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
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
            // ������Ȃ����͐V�K�쐬
            EquipListItem newItem = new EquipListItem();
            newItem.Name = name;
            newItem.LastRefered = DateTime.Now;
            newItem.Set = newSet;
            equipSetItems.Add(newItem);
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
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
        /// ���ɑ��݂��鑕���Z�b�g�̓��e�𑕔����܂�
        /// </summary>
        /// <param name="name">�����Z�b�g�̖��O�ł�</param>
        /// <returns>�s���A�C�e�����X�g</returns>
        public static Item[] LoadEquipSetIfItemExist(string personId, string name)
        {
            EquipListItem targetEquipSet = equipSetItems.FirstOrDefault(c=>c.Name == name);
            if (targetEquipSet == null) throw new ApplicationException($"�����G���[: LoadEquipSetIfItemExistha���\�b�h��{name}��equipSetItems�Ɍ�����܂���B");

            var lackItemsList = new List<Item>();
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                // ���̃A�C�e�������ɑ����ς݂Ȃ牽�����Ȃ�
                if (targetEquipSet.Set.AllItems[i].Id == Flags.GetRawEquip(personId, i)) continue;

                // ���͂����s�̃A�C�e�������ɑ�������Ă������΂�
                if (Items.GetItemByNumber(Flags.GetRawEquip(personId, i)).Is���O���s�\Item) continue;

                if (!General.IsEquippableItem(i, personId, targetEquipSet.Set.AllItems[i].Id))
                {
                    // ������������Ă��Ȃ��A�C�e���͎����I��null���������
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                }
                else if (targetEquipSet.Set.AllItems[i].IsItemNull)
                {
                    // NULL�A�C�e����NULL�A�C�e���ɐݒ肷�邪�s���A�C�e�������͂��Ȃ�
                    // NULL�A�C�e���͏����ł��Ȃ�����ʈ���
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                }
                else if (State.GetItemCount(targetEquipSet.Set.AllItems[i]) == 0 || General.IsAnyoneEquippedAllItems(targetEquipSet.Set.AllItems[i].Id, personId))
                {
                    // �s���A�C�e��
                    Flags.SetRawEquip(personId, i, Items.ItemNull.Id);
                    lackItemsList.Add(targetEquipSet.Set.AllItems[i]);
                }
                else
                {
                    // ���̃A�C�e���𑕔�
                    Flags.SetRawEquip(personId, i, targetEquipSet.Set.AllItems[i].Id);
                }
            }
            return lackItemsList.ToArray();
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
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
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// ���삪������Ȃ����ʂȑ����Z�b�g�̖��O���擾���܂�
        /// </summary>
        /// <returns>���O�̃R���N�V����</returns>
        public static string[] DisabledEquipSetNames()
        {
            return new string[] { State.�����Ȃ� };
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
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
        /// ������"�J�X�^��"�ł��B
        /// </summary>
        public const string �J�X�^�� = "�J�X�^��";
        /// <summary>
        /// ������"�����Ȃ�"�ł��B
        /// </summary>
        public const string �����Ȃ� = "�S���ɂ���";

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="name">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
        public static bool IsReadOnlyEquipSet(string name)
        {
            return name == �����Ȃ� || name == �J�X�^��;
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        public class OwnItem
        {
            /// <summary>
            /// ��ʃA�v������͎g���ׂ��ł͂���܂���
            /// </summary>
            public string Id;
            /// <summary>
            /// ��ʃA�v������͎g���ׂ��ł͂���܂���
            /// </summary>
            public int OwnCount;
            /// <summary>
            /// ��ʃA�v������͎g���ׂ��ł͂���܂���
            /// </summary>
            public int UsedCount;
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <param name="ownItemList">��ʃA�v������͎g���ׂ��ł͂���܂���</param>
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
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
        /// </summary>
        /// <returns>��ʃA�v������͎g���ׂ��ł͂���܂���</returns>
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

        // �S�ẴA�C�e���������Ă��Ȃ��������Ƃɂ���
        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
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
        /// �ǂݍ��񂾃��W���[���ꗗ
        /// </summary>
        public static Module[] loadedModules = new Module[0];
        /// <summary>
        /// �ǂݍ��񂾃��W���[���ꗗ
        /// </summary>
        public static ModuleEx[] LoadedModulesEx = new ModuleEx[0];

        /// <summary>
        /// ����target�ɓn���ꂽ�^���`���Ă��郂�W���[���𔻒肵�ĕԂ��܂�
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
        /// �w��A�C�e������������Ă��邩?
        /// </summary>
        /// <param name="targetItem">�ΏۂƂ���A�C�e��</param>
        /// <returns>��������Ă�����True</returns>
        public static bool IsEquipedItem(Item targetItem)
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                if (targetItem.Id == Flags.Equip[i]) return true;
            }
            return false;
        }

        /// <summary>
        /// ��ʃA�v������͎g���ׂ��ł͂���܂���
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
        /// �w��R���N�V���������肳�ꂽ���Ƃ��L�^���܂��B�V�X�e���t�@�C���ւ̃Z�[�u���s���܂��B
        /// </summary>
        /// <param name="collectionID">�R���N�V������Id�ł�</param>
        /// <param name="keyID">�L�[��Id�ł�</param>
        /// <param name="subkeyID">�T�u�L�[��Id�ł�</param>
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
                DefaultPersons.�V�X�e��.Say("���߂łƂ��������܂��B�X�^�[��1���肵�č��v{0}�ɂȂ�܂����B", StarManager.GetStars());
            }
        }

        /// <summary>
        /// ���L���Ă����Ԃł��B
        /// </summary>
        [System.Reflection.ObfuscationAttribute(Exclude = true)]
        public enum CollectionState
        {
            /// <summary>
            /// ���L���Ă��܂���B
            /// </summary>
            None,
            /// <summary>
            /// ���L���Ă��܂��񂪁A�V�X�e���t�@�C���ɋL�^�������Ė��O�͌����܂��B
            /// </summary>
            NameVisible,
            /// <summary>
            /// ���L���Ă��܂��B
            /// </summary>
            Own
        }
        /// <summary>
        /// �w��R���N�V���������Ɏ擾�ς݂�����
        /// </summary>
        /// <param name="collectionID">�R���N�V������Id�ł�</param>
        /// <param name="keyID">�L�[��Id�ł�</param>
        /// <param name="subkeyID">�T�u�L�[��Id�ł�</param>
        /// <returns>�擾�̏�</returns>
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
        /// ����X�P�W���[�������ł��邩���Z�b�g���܂�
        /// ���̏��͏����邱�ƂŎ����I�Ƀ��Z�b�g����A���肩�������s����邱�Ƃ�}�~���܂�
        /// </summary>
        /// <param name="scheduleId">�X�P�W���[����ID�ł�</param>
        /// <param name="availability">���ł��邩���w�肵�܂�</param>
        public static void SetScheduleVisible(string scheduleId, bool availability)
        {
            Flags.ScheduleVisilbles[scheduleId] = availability;
        }

        /// <summary>
        /// ����X�P�W���[�������ł��邩�𔻒肵�܂�
        /// </summary>
        /// <param name="scheduleId">�X�P�W���[����ID�ł�</param>
        /// <returns>���ł����true��Ԃ��܂�</returns>
        public static bool IsScheduleVisible(string scheduleId)
        {
            return Flags.ScheduleVisilbles[scheduleId];
        }

        public static Func<Dummy> EquipChangeNotify { get; set; }

        /// <summary>
        ///  �W���[�i�����O�v���C���[��ێ�����
        /// </summary>
        public static Func<JournalingInputDescripter,Task> JournalingPlayer { get; set; }
    }
}
