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
    /// ���s���A�T�[�g���s�̒ʒm�p�ł��B��ʂ̃��W���[���͎g�p���ׂ��ł͂���܂���B
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
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="message">���b�Z�[�W�ł��B</param>
        /// <param name="node">�ΏۂƂ����m�[�h�ł��B</param>
        public JournalingAssertionException(string message, JournalingNode node)
            : base(message + " : " + (node == null ? "(EOF)" : node.SourceFileName.FileName + " (" + node.LineNumber.ToString() + ")"))
        {
            JournalPlaybackQueue.Clear();
        }
        public static async Task<JournalingAssertionException> CreateAsync(string message, JournalingNode node)
        {
            // �o�b�`�e�X�g���Ă���ƁArestoreActionSetAsync����i���ɖ߂��ė��Ȃ��̂ŁA
            // �����Ńe�X�g���s�𖾊m�ɂ��Ă���
            // �����restoreActionSetAsync�����ŎQ�Ƃ���ă��[�U�[�ɓ`���
            JournalingDocumentPlayer.IsSuccess = false;
            var r = new JournalingAssertionException(message, node);
            await UI.Actions.restoreActionSetAsync();
            return r;
        }

    }

    /// <summary>
    /// �W���[�i�����O���s���m�[�h�̎�ނł��B
    /// </summary>
    public enum JournalingNodeType
    {
        /// <summary>
        /// �V���v���ȃ��j���[�ł��B
        /// </summary>
        SimpleMenu,
        /// <summary>
        /// ���t���A�T�[�g���܂�
        /// </summary>
        DateAssert,
        /// <summary>
        /// �������A�T�[�g���܂��B
        /// </summary>
        TimeAssert,
        //FlagAssert,
        /// <summary>
        /// ���z���A�T�[�g���܂�
        /// </summary>
        MoneyAssert,
        /// <summary>
        /// �A�C�e�����g�p���܂��B
        /// </summary>
        ConsumeItem,
        /// <summary>
        /// �������܂��B���ʂ̓C���f�b�N�X0�ł��B
        /// </summary>
        Equip,
        /// <summary>
        /// �g���p�ł��B
        /// </summary>
        Extra,
        /// <summary>
        /// �w�����܂��B
        /// </summary>
        Buy,
        /// <summary>
        /// ���p���܂��B
        /// </summary>
        Sell,
        /// <summary>
        /// ���O����͂��܂��B
        /// </summary>
        NameEntry,
        /// <summary>
        /// �V�X�e���t�@�C���̓��e�����������܂�
        /// </summary>
        �V�X�e���t�@�C��������,
        /// <summary>
        /// �ʂ̃W���[�i�����O�t�@�C������荞�݂܂��B
        /// </summary>
        Include,
        /// <summary>
        /// �K�C�h���b�Z�[�W��\�����܂��B
        /// </summary>
        GuideMessage,
        /// <summary>
        /// 1�̃I�u�W�F�N�g��I�����܂��B
        /// </summary>
        SelectOneItem,
        /// <summary>
        /// �����T�C�N����ύX���܂� (��Βl)
        /// </summary>
        ChangeCycleRelative,
        /// <summary>
        /// 1�̃R���N�V�������A�T�[�g���܂�
        /// </summary>
        CollectionAssert,
        /// <summary>
        /// �R���N�V�����̎擾�����A�T�[�g���܂�
        /// </summary>
        RateAssert,
        /// <summary>
        /// �R���N�V�����̐^�̎擾�����A�T�[�g���܂�
        /// </summary>
        TrueRateAssert,
        /// <summary>
        /// ���W���[���P�ʂŃR���N�V�������N���A���܂�
        /// </summary>
        InitCollectionsByModule,
        /// <summary>
        /// �X�^�[���w��l�ɃZ�b�g���܂�
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
            Map["IS"] = JournalingNodeType.�V�X�e���t�@�C��������;
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
            throw new ApplicationException(((int)type).ToString() + "�͂��蓾�Ȃ�JournalingNodeType�ł��B");
        }
    }

    /// <summary>
    /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
    /// </summary>
    public class JournalingNode
    {
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public JournalingNodeType CommandType;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public Module Module;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public bool Negative;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public string ArgumentString;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public string ArgumentString2;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public string ArgumentString3;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public int ArgumentInt;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public DateTime ArgumentDateTime;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public string ArgumentModuleId;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public string[] ArgumentExtra;
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public JournalingInputDescripter SourceFileName;	// �C���N���[�h�@�\���g���ƕK�v�ɂȂ���
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public int LineNumber;
    }

    /// <summary>
    /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
    /// </summary>
    public class JournalingDocument
    {
        private List<JournalingNode> nodes = new List<JournalingNode>();
        private int sequenceCounter = 0;

        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public static Item GetItemFromName(string name)
        {
            var r = Item.List.Values.FirstOrDefault(c => c.HumanReadableName == name);
            if (r != null) return r;
            throw new JournalingDocumentException(name + "�̓A�C�e�����ł͂���܂���B");
        }

        private static void validateItemName(string name)
        {
            // ���s���ɓ��I�Ɍ��܂�A�C�e�����͂��̕��@�ł̓o���f�[�V�����ł��Ȃ�
            //// �����I�ɂ��̎����ŏ\��
            //GetItemFromName(name);
        }

        private static void validateCollectionName(string moduleId, string collectionName, string mainname, string subname = null)
        {
            Collection collection = SimpleName<Collection>.List.Values.FirstOrDefault((c) => c.HumanReadableName == collectionName);
            if (collection == null)
            {
                throw new JournalingDocumentException(collectionName + "�̓R���N�V�������ł͂���܂���B");
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
            if (!found) throw new JournalingDocumentException(mainname + "�̓R���N�V�����Ɋ܂܂�܂���B");
        }

        private static void validateModuleId(string id)
        {
            if (!State.loadedModules.Any(c => c.Id == id)) throw new JournalingDocumentException(id + "�̓��W���[����ID���ł͂���܂���B");
        }

        private static void validateEquipItemName(string name, int target /*��������*/ )
        {
            if (string.IsNullOrWhiteSpace(name)) return;	// �󕶎����"��������"�������L���Ȗ��O
            var r = Item.List.Values.FirstOrDefault(c => c.HumanReadableName == name);
            if (r == null)
                throw new JournalingDocumentException(name + "�͓K�؂ȑ����A�C�e�����ł͂���܂���B(���O�����t����Ȃ�)");
            if (r.AvailableEquipMap.Length <= target)
                throw new JournalingDocumentException(name + $"�͓K�؂ȑ����A�C�e�����ł͂���܂���B(r.AvailableEquipMap.Length({r.AvailableEquipMap.Length}) <= target({target}))");
            if (r.AvailableEquipMap[target]) return;
            throw new JournalingDocumentException(name + "�͓K�؂ȑ����A�C�e�����ł͂���܂���(r.AvailableEquipMap[target]==false)�B");
        }

        private static void validatePersonId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;  // �󔒂͍��@
            if (Person.List.Keys.Contains(id)) return;
            throw new JournalingDocumentException(id + "�͓K�؂�Person ID�ł͂���܂���B");
        }

        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        public async System.Threading.Tasks.Task JournalingDocumentInitAsync(JournalingInputDescripter filename)// �ǂݍ��݂ƃR���p�C�����s��
        {
            char[] whiteSpaces = { ' ', '\t', '�@' };
            int lineNumber = 0;	// ��ɃJ�E���g�A�b�v�������̂ŁA1�ł͂Ȃ�0����n�߂�
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

                        if (s.Trim().Length == 0) continue; // ��s
                        if (s.StartsWith("*")) continue;    // �R�����g�s

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
                            throw new JournalingDocumentException(commandName + "�͗L���ȃR�}���h���ł͂���܂���B", filename.FileName, lineNumber);
                        }

                        JournalingNode newNode = new JournalingNode();
                        newNode.SourceFileName = filename;
                        newNode.LineNumber = lineNumber;
                        newNode.CommandType = JournalingCommandNameMap.Map[commandName];
                        newNode.Negative = negativeCondition;
                        switch (newNode.CommandType)
                        {
                            // �A�[�M�������g�����̃O���[�v
                            case JournalingNodeType.�V�X�e���t�@�C��������:
                                break;
                            // �A�[�M�������g�͔C�ӂ̕�����̃O���[�v
                            case JournalingNodeType.SimpleMenu:
                                newNode.ArgumentString = argument;
                                break;
                            case JournalingNodeType.NameEntry:
                                goto case JournalingNodeType.SimpleMenu;
                            case JournalingNodeType.GuideMessage:
                                goto case JournalingNodeType.SimpleMenu;
                            default:
                                // ��O�𓊂���\��������R�[�h���܂Ƃ߂ď���
                                try
                                {
                                    switch (newNode.CommandType)
                                    {
                                        // �A�[�M�������g�͓��t
                                        case JournalingNodeType.DateAssert:
                                            newNode.ArgumentDateTime = DateTime.ParseExact(argument, "yyyy/MM/dd", null);
                                            break;
                                        // �A�[�M�������g�͎���
                                        case JournalingNodeType.TimeAssert:
                                            newNode.ArgumentDateTime = DateTime.ParseExact(argument, "HH:mm", null);
                                            break;
                                        // �R���N�V�����A�T�[�g��p
                                        case JournalingNodeType.CollectionAssert:
                                            string[] args = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args.Length < 3 || args.Length > 4)
                                            {
                                                throw new JournalingDocumentException("�����̐�����v���܂���B");
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
                                        //throw new JournalingDocumentException("�����̐�����v���܂���B");
                                        //}
                                        //State.ValidateFlagName(args[0]);
                                        //newNode.ArgumentString = args[0];
                                        //newNode.ArgumentInt = int.Parse(args[1]);
                                        //break;
                                        // �A�[�M�������g�������̃O���[�v
                                        case JournalingNodeType.MoneyAssert:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        case JournalingNodeType.ChangeCycleRelative:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        // �A�[�M�������g�̓��W���[��ID
                                        case JournalingNodeType.InitCollectionsByModule:
                                            validateModuleId(argument);
                                            newNode.ArgumentModuleId = argument;
                                            break;
                                        // �A�[�M�������g�̓X�^�[��
                                        case JournalingNodeType.SetStar:
                                            newNode.ArgumentInt = int.Parse(argument);
                                            break;
                                        // �A�[�M�������g�̓A�C�e����
                                        case JournalingNodeType.ConsumeItem:
                                            validateItemName(argument);
                                            newNode.ArgumentString = argument;
                                            break;
                                        case JournalingNodeType.SelectOneItem:
                                            goto case JournalingNodeType.ConsumeItem;
                                        // �A�[�M�������g�͌�+�A�C�e����
                                        case JournalingNodeType.Buy:
                                            string[] args2 = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args2.Length != 2)
                                            {
                                                throw new JournalingDocumentException("�����̐�����v���܂���B");
                                            }
                                            validateItemName(args2[1]);
                                            newNode.ArgumentString = args2[1];
                                            newNode.ArgumentInt = int.Parse(args2[0]);
                                            break;
                                        case JournalingNodeType.Sell:
                                            goto case JournalingNodeType.Buy;
                                        // �A�[�M�������g��ID+�����̕�����
                                        case JournalingNodeType.RateAssert:
                                        case JournalingNodeType.TrueRateAssert:
                                        case JournalingNodeType.Extra:
                                            string[] args3 = argument.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                                            if (args3.Length <= 1)
                                            {
                                                throw new JournalingDocumentException("�����̐�����v���܂���B");
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
                                        // �A�[�M�������g�͓��葕�����ʑ����i�܂��͋�
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
                                                        throw new JournalingDocumentException("�����̐�����v���܂���B");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new JournalingDocumentException("�����̐�����v���܂���B");
                                                }
                                            }
                                            break;
                                        // �C���N���[�h�݂̂��̃��C���[�ŋ@�\����̂ŗ�O�I�ɏ���
                                        case JournalingNodeType.Include:
#if true
                                            JournalingDocument subdoc = new JournalingDocument();
                                            await subdoc.JournalingDocumentInitAsync(new JournalingInputDescripter(filename.ModuleId, argument));
                                            this.nodes.AddRange(subdoc.nodes);
                                            continue;   // nodes.Add(newNode)�͎��s�������[�v���ɐi�߂�
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
                                        continue;   // nodes.Add(newNode)�͎��s�������[�v���ɐi�߂�
                                                    //break;
#endif
                                    }
                                }
                                catch (Exception e)
                                {
                                    throw new JournalingDocumentException($"�W���[�i�����O�t�@�C���̉�̓G���[ {filename.FileName}", e);
                                    //throw new JournalingDocumentException($"�W���[�i�����O�t�@�C���̉�̓G���[ {filename.FileName} ({filename.ModuleEx.GetXmlModuleData().Name})", e);
                                }
                                break;
                        };
                        nodes.Add(newNode);
                    }
                }
            }
            finally
            {
                //DefaultPersons.�V�X�e��.Say();
                var fn = filename.FileName;
                if (fn.StartsWith(Constants.JournalingDirectHeader)) fn = "[INPUTBOX]";
                await UI.Actions.NotifyStatusMessageAsync($"Total {lineNumber} lines detected in {fn}");
            }
        }
        /// <summary>
        /// �W���[�i�����O�p�B��ʃA�v������g�p���ׂ��ł͂���܂���B
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
                        condition = nodes[sequenceCounter].ArgumentInt <= Flags.������;
                        break;
                    case JournalingNodeType.�V�X�e���t�@�C��������:
                        // ����̓A�T�[�g�ł͂Ȃ����A�A�T�[�g�ɏ�����^�C�~���O�ŏ�������˂΂Ȃ�Ȃ�
                        SystemFile.AllClearForNewPlay();
                        // �g���b�N�����A��Ƀt�F�C�����Ȃ�������^����
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.SetStar:
                        // ����̓A�T�[�g�ł͂Ȃ����A�A�T�[�g�ɏ�����^�C�~���O�ŏ�������˂΂Ȃ�Ȃ�
                        StarManager.AddStar(-StarManager.GetStars() + nodes[sequenceCounter].ArgumentInt);
                        // �g���b�N�����A��Ƀt�F�C�����Ȃ�������^����
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.InitCollectionsByModule:
                        // ����̓A�T�[�g�ł͂Ȃ����A�A�T�[�g�ɏ�����^�C�~���O�ŏ�������˂΂Ȃ�Ȃ�
                        SystemFile.InitCollectionsByModule(nodes[sequenceCounter].ArgumentModuleId);
                        // �g���b�N�����A��Ƀt�F�C�����Ȃ�������^����
                        condition = !nodes[sequenceCounter].Negative;
                        break;
                    case JournalingNodeType.GuideMessage:
                        // ����̓A�T�[�g�ł͂Ȃ����A�A�T�[�g�ɏ�����^�C�~���O�ŏ�������˂΂Ȃ�Ȃ�
                        this.supplyMessage = nodes[sequenceCounter].ArgumentString;
                        // �g���b�N�����A��Ƀt�F�C�����Ȃ�������^����
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
                        // �Y�����Ȃ��R�}���h�ɓ���������A�A�T�[�g�����͏I���
                        return;
                }
                if (nodes[sequenceCounter].Negative) condition = !condition;
                if (!condition)
                {
                    throw await JournalingAssertionException.CreateAsync("�A�T�[�g" +
                    JournalingCommandNameMap.ReveseReference(nodes[sequenceCounter].CommandType) +
                    "���t�F�C�����܂����B",
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
                    throw await JournalingAssertionException.CreateAsync($"�Ώۑ�����{total}�ł����A����ł͊������v�Z�ł��܂���B", node);
                }
                int actualValue = count * 100 / total;
                bool result = rate != actualValue;
                if (node.Negative) result = !result;
                if (result)
                {
                    throw await JournalingAssertionException.CreateAsync($"{node.ArgumentExtra[1]}�����҂���܂������A���ۂ�{actualValue}�ł����B({count}/{total})[hit={string.Join(", ", hitList)}/nohit={string.Join(", ", nohitList)}]", node);
                }
            }
            else
            {
                throw await JournalingAssertionException.CreateAsync(node.ArgumentExtra[1] + "�͐����ł͂���܂���B", node);
            }

            // node.Negative==true�̏ꍇ�A���Ƃ��甽�]��������̂ŁA�\�ߔ��肵���l��Ԃ��Ă����B
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
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="journalingNodeType">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
        public async System.Threading.Tasks.Task<JournalingNode> GetNextRecordAsync(JournalingNodeType journalingNodeType)
        {
            await progressUpdateAsync();

            await ProcessingAssertsAsync();

            if (IsEndOfRecords)
            {
                throw await JournalingAssertionException.CreateAsync("�R�}���h" +
                JournalingCommandNameMap.ReveseReference(journalingNodeType) +
                "�����҂���܂������A�m�[�h�͂�������܂���B",
                null);
            }

            //Console.WriteLine($"{nodes[sequenceCounter].CommandType} {sequenceCounter}");
            if (nodes[sequenceCounter].CommandType != journalingNodeType)
            {
                throw await JournalingAssertionException.CreateAsync("�R�}���h" +
                    JournalingCommandNameMap.ReveseReference(journalingNodeType) +
                    "�����҂���܂������A���ۂɎ��̃m�[�h�ɂ������̂�" +
                    JournalingCommandNameMap.ReveseReference(nodes[sequenceCounter].CommandType) + "�ł����B",
                    nodes[sequenceCounter]);
            }
            JournalingNode result = nodes[sequenceCounter++];
            if (IsEndOfRecords) await UI.Actions.restoreActionSetAsync();
            return result;
        }

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="journalingNodeType">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
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
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
        public async Task<JournalingNode> PeekCurrentRecordAsync()
        {
            int s = sequenceCounter - 1;
            if( s < 0 )
            {
                throw await JournalingAssertionException.CreateAsync("�m�[�h�͂���܂���B", null);
            }
            if (sequenceCounter >= nodes.Count)
            {
                throw await JournalingAssertionException.CreateAsync("�m�[�h�͂�������܂���B", null);
            }
            return nodes[sequenceCounter];
        }

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
        public async Task<JournalingNode> PeekNextRecordAsync()
        {
            if (sequenceCounter >= nodes.Count)
            {
                throw await JournalingAssertionException.CreateAsync("�m�[�h�͂�������܂���B", null);
            }
            return nodes[sequenceCounter];
        }
        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
        public async System.Threading.Tasks.Task IncrementNextRecordAsync()
        {
            await progressUpdateAsync();

            await ProcessingAssertsAsync();

            sequenceCounter++;
            if (IsEndOfRecords) await UI.Actions.restoreActionSetAsync();
        }
        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <returns>��ʃA�v������g���ׂ��ł͂���܂���B</returns>
        public bool IsEndOfRecords
        {
            get { return sequenceCounter >= nodes.Count; }
        }
    }

    // �ے�A�T�[�g���������ދ@�\�͕s�v�ł��邱�Ƃɒ���
    // ���ۂɍs���������ʂ̋L�^�ɔے�A�T�[�g�͂��蓾�Ȃ�
    // ���̃N���X�͕K�������قȂ�X���b�h����Ă΂��\�������邪�A�X���b�h�Z�[�t�ł͂Ȃ�
    // �����I�ɓ����ɃW���[�i�����O�����\���͖����ƍl������̂Ŗ��͂Ȃ�
    /// <summary>
    /// ��ʃA�v������g���ׂ��ł͂���܂���B
    /// </summary>
    public class MyJournalingFileWriter : IJournalingWriter
    {
        private TextWriter jounalWriter = null;

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="filename">��ʃA�v������g���ׂ��ł͂���܂���B</param>
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
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="commandName">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        /// <param name="arguments">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        void IJournalingWriter.Write(string commandName, params object[] arguments)
        {
            if (jounalWriter == null) return;
            var sb = new StringBuilder();
            sb.Append(commandName);
            foreach (var item in arguments) sb.AppendFormat(" {0}", item);
            jounalWriter.WriteLine(sb);
        }

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
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

    // �ے�A�T�[�g���������ދ@�\�͕s�v�ł��邱�Ƃɒ���
// ���ۂɍs���������ʂ̋L�^�ɔے�A�T�[�g�͂��蓾�Ȃ�
// ���̃N���X�͕K�������قȂ�X���b�h����Ă΂��\�������邪�A�X���b�h�Z�[�t�ł͂Ȃ�
// �����I�ɓ����ɃW���[�i�����O�����\���͖����ƍl������̂Ŗ��͂Ȃ�
/// <summary>
/// ��ʃA�v������g���ׂ��ł͂���܂���B
/// </summary>
public class MyJournalingStringBuilderWriter: IJournalingWriter
    {
        private bool isRecording = false;
        private StringBuilder lines = null;

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="filename">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        void IJournalingWriter.Create(string filename)
        {
            lines = new StringBuilder();
            isRecording = true;
            JournalingWriter.WriteComment("Record Start " + DateTime.Now.ToString());
        }
        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="commandName">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        /// <param name="arguments">��ʃA�v������g���ׂ��ł͂���܂���B</param>
        void IJournalingWriter.Write(string commandName, params object[] arguments)
        {
            if (!isRecording) return;
            var sb = new StringBuilder();
            sb.Append(commandName);
            foreach (var item in arguments) sb.AppendFormat(" {0}", item);
            lines.AppendLine(sb.ToString());
        }

        /// <summary>
        /// ��ʃA�v������g���ׂ��ł͂���܂���B
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
