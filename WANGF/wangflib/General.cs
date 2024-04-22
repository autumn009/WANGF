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
    /// �������̃��b�p�[��񋟂��܂��B
    /// </summary>
    public class EquipWrapper
    {
        private string targetPersonId = DefaultPersons.��l��.Id;
        public string TargetPersonId
        {
            get { return targetPersonId; }
            set { targetPersonId = value; }
        }
        /// <summary>
        /// ���鑕�����ʂ̏���ǂݏ������܂��B
        /// �����̕��ʂɑ����ł���A�C�e���́A�����ɕ����̕����ɑ�������܂��B
        /// </summary>
        /// <param name="index">�������ʂł��B</param>
        /// <returns>��������Ă���A�C�e����Id�ł��B</returns>
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
                // StatusHide��DisableEquipReporting�̎��͒ʒm���Ȃ�
                if (!DisableEquipReporting && !ANGFLib.State.CurrentPlace.IsStatusHide)
                {
                    var currentPerson = Person.List[Flags.Equip.TargetPersonId];
                    if (item.IsItemNull && oldItem.IsItemNull) return;
                    if (oldItem.Id == item.Id) return;
                    if (item.IsItemNull)
                    {
                        DefaultPersons.�V�X�e��.Say($"{currentPerson.HumanReadableName}��{oldItem.HumanReadableName}���O�����B");
                    }
                    else
                        DefaultPersons.�V�X�e��.Say($"{currentPerson.HumanReadableName}��{item.HumanReadableName}�𑕔������B");
                }
            }
        }
        /// <summary>
        /// true�Ȃ瑕���ύX�̒ʒm�͍s���Ȃ��B
        /// �Z�[�u����Ȃ����Ȃ̂ňꎞ�I�ȕύX�Ɏg�����ƁB
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
    /// �����A�C�e���̏��ꎮ�ł��B
    /// </summary>
    public class ItemArray
    {
        /// <summary>
        /// �w�肳�ꂽ�A�C�e���𑕔����Ă��邩���肷��
        /// </summary>
        /// <param name="item">��������A�C�e���B������null�ȃA�C�e���͕s��</param>
        /// <returns>���̃A�C�e�����g�p����Ă�����true</returns>
        public bool IsEquipedItem(Item item)
        {
            Item found = allItems.FirstOrDefault((i) => i.Id == item.Id);
            return found != null;
        }

        /// <summary>
        /// �����A�C�e�����Z�b�g���܂��B�����������ʂɑΉ����܂��B
        /// </summary>
        /// <param name="val">�Z�b�g����A�C�e���ł��B</param>
        /// <param name="index">�Z�b�g���鑕�����ʂł��B</param>
        /// <param name="getter">���������Q�b�g���܂��B</param>
        /// <param name="setter">���������Z�b�g���܂��B</param>
        public static void SetOperation(Item val, int index, Func<int, Item> getter, Func<int, Item, Dummy> setter)
        {
            // remove operation
            // �S�Ă̕��ʂ��珜�������K�v������
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
            // �S�Ă̕��ʂɒǉ������
            for (int i = 0; i < val.SameTimeEquipMap.Length; i++)
            {
                if (val.SameTimeEquipMap[i])
                {
                    setter(i, val);
                }
            }
            // ���Ȃ��Ƃ��ړI�̏ꏊ�ɓ���
            setter(index, val);
        }

        private Item[] allItems;
        /// <summary>
        /// �����A�C�e���̏���ǂݏ������܂��B���������\�ȃA�C�e����1�����ǂݏ������܂��B
        /// </summary>
        /// <param name="index">�������ʂł�</param>
        /// <returns>�����A�C�e���ł�</returns>
        public Item this[int index]
        {
            get { return allItems[index]; }
            set { SetOperation(value, index, (i) => allItems[i], (i, v) => { allItems[i] = v; return new Dummy(); } ); }
            //set { allItems[index] = value; }
        }
        /// <summary>
        /// ���[����Ă��鑕�����ʂ̐���Ԃ��܂�
        /// </summary>
        public int Length { get { return allItems.Length; } }
        /// <summary>
        /// �R���X�g���N�^�ł�
        /// </summary>
        public ItemArray()
        {
            allItems = new Item[32];	// �����͌��ߑł��ł���D�܂����Ȃ�
            for (int i = 0; i < allItems.Length; i++) allItems[i] = Items.ItemNull;
        }
    }

    /// <summary>
    /// �����i�ꎮ���ꎞ�I�ɕۊǂ���
    /// </summary>
    public class EquipSet
    {
        /// <summary>
        /// �������ꗗ�ł��B
        /// </summary>
        public ItemArray AllItems = new ItemArray();

        /// <summary>
        /// ���������܂�
        /// </summary>
        /// <returns>�ʃC���X�^���X�̕����ł�</returns>
        public EquipSet Duplicate()
        {
            var r = new EquipSet();
            for (int i = 0; i < AllItems.Length; i++) r.AllItems[i] = AllItems[i];
            return r;
        }

        /// <summary>
        /// 2��EquipSet�I�u�W�F�N�g�������ł��邩�𔻒肵�܂��B
        /// </summary>
        /// <param name="target">��r����Ώۂł��B</param>
        /// <returns>�����Ȃ�true��Ԃ��܂��B</returns>
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
        int �����N�ړ����Ԍv�Z(Place currentPlace, Place distPlace);
    }

    /// <summary>
    /// ����̏ꏊ��l�Ɋ֌W�̂Ȃ��ėp�I�ȍs���⃊�A�N�V�����̃R�[�h���W�߂�
    /// </summary>
    public static class General
    {
        /// <summary>
        /// SuperTalk�̏����W�����v�Ɏg�p�����ėp�̃t���O�ł�
        /// </summary>
        public static bool TheFlag = false;

        /// <summary>
        /// ���݂̊g���q�ł��B��ʃA�v������͓ǂݏo���݂̂𐄏����܂��B
        /// </summary>
        public static string FileExtention = "angf";
        /// <summary>
        /// �ėp�̗����񋟗p�ł��B���݂̓��t�����ŏ����������̂ŁA�قڗ\���s�\�Ȓl�𓾂��܂�
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
            DefaultPersons.�V�X�e��.Say("{0}���A�g�̂��x�߂܂����B", minute);
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
                DefaultPersons.�V�X�e��.Say("����{0}�����߂��Ă��܂��B", hour);
                return false;
            }
            DateTime eta = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, hour, 0, 0);
            if (nextDay) eta = eta.AddDays(1.0);
            int minutes = (int)(eta - Flags.Now).TotalMinutes;
            DefaultPersons.�V�X�e��.Say("{0}���܂ŁA�g�̂��x�߂܂����B", hour);
            bool result = await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now.AddMinutes(minutes));
            if (!result) State.GoTime(minutes);
            return true;
        }

        internal static DateTime GetDateOnly(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }

        /// <summary>
        /// �����Z�[�u���s���܂��B(�Z�[�u��0�̏ꍇ�͉������܂���)
        /// </summary>
        public static async Task �����Z�[�uAsync()
        {
            await UI.Actions.AutoSaveFileAsync();
        }

        /// <summary>
        /// ��l�̂̎���Ԃ��܂��B
        /// </summary>
        /// <returns>��l�̂̎�</returns>
        public static string GetMyName()
        {
            string watasi = "��";
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
        /// �Z�[�u���s���t�@�C�����̃f�t�H���g��Ԃ��܂��B
        /// </summary>
        /// <param name="isAuto">�����Z�[�u�Ȃ�True�ł��B</param>
        /// <returns>�����������ꂽ�t�@�C�����̌��ł��B</returns>
        public static string GenerateSuggestedFileName(bool isAuto)
        {
            string filename = GenerateSuggestedFileNameWithoutExt(isAuto);
            return Path.ChangeExtension(filename + ".ZZZ", General.FileExtention);
        }

        /// <summary>
        /// �Z�[�u���s���t�@�C�����̃f�t�H���g��Ԃ��܂��B(�g���q����)
        /// </summary>
        /// <param name="isAuto">�����Z�[�u�Ȃ�True�ł��B</param>
        /// <returns>�����������ꂽ�t�@�C�����̌��ł��B</returns>
        public static string GenerateSuggestedFileNameWithoutExt(bool isAuto)
        {
            string filename = string.Format("{0} {1} {2}{3}",
                DateTime.Now.ToString("yyyyMMdd HHmmss"),
                GetMyName(), Flags.Now.ToString("MM��dd��"),
                isAuto ? ".auto" : "");
            return filename;
        }

        /// <summary>
        /// UI���݂ŕW���I�ȍw�����s���܂��B
        /// </summary>
        /// <param name="shopID">�V���b�v��Id�ł�</param>
        /// <param name="getPrice">�l�i���擾����f���Q�[�g�^�ł��B�l�i�����H���Ȃ��ꍇ�A���̂܂܂̋��z��Ԃ��܂��B</param>
        /// <returns>���true��Ԃ��܂��B</returns>
        public static async Task<bool> StandardBuyAsync(string shopID, GetPriceInvoker getPrice)
        {
            Item[] items = ShopAndItemReleations.GetItems(shopID);
            await UI.Actions.shopBuyMenuAsync(items, getPrice);
            return true;
        }

        /// <summary>
        /// �W���I�ȍw�����s�����j���[�𐶐����܂�
        /// </summary>
        /// <param name="list">���j���[��ǉ����ׂ��R���N�V�����ł�</param>
        /// <param name="shopID">�V���b�v��Id�ł�</param>
        /// <param name="getPrice">�l�i���擾����f���Q�[�g�^�ł��B�l�i�����H���Ȃ��ꍇ�A���̂܂܂̋��z��Ԃ��܂��B</param>
        /// <param name="isOpen">�X���J���Ă���Ƃ���true��Ԃ��f���Q�[�g�^�ł�</param>
        public static void StandardBuyMenu(List<SimpleMenuItem> list, string shopID, GetPriceInvoker getPrice, Func<bool> isOpen)
        {
            if (isOpen())
            {
                list.Add(new SimpleMenuItem("������", async () => await General.StandardBuyAsync(shopID, getPrice)));
            }
            else
            {
                DefaultPersons.�V�X�e��.Say("�X�͕܂��Ă��܂��B");
            }
        }

        /// <summary>
        /// UI���݂ŕW���I�Ȕ��p���s���܂��B
        /// </summary>
        /// <param name="shopID">�V���b�v��Id�ł�</param>
        /// <param name="getPrice">�l�i���擾����f���Q�[�g�^�ł��B�l�i�����H���Ȃ��ꍇ�A���̂܂܂̋��z��Ԃ��܂��B</param>
        /// <returns>���true��Ԃ��܂��B</returns>
        public static async Task<bool> StandardSellAsync(string shopID, GetPriceInvoker getPrice)
        {
            await UI.Actions.shopSellMenuAsync(getPrice);
            return true;
        }

        /// <summary>
        /// �W���I�Ȕ��p���s�����j���[�𐶐����܂�
        /// </summary>
        /// <param name="list">���j���[��ǉ����ׂ��R���N�V�����ł�</param>
        /// <param name="shopID">�V���b�v��Id�ł�</param>
        /// <param name="getPrice">�l�i���擾����f���Q�[�g�^�ł��B�l�i�����H���Ȃ��ꍇ�A���̂܂܂̋��z��Ԃ��܂��B</param>
        /// <param name="isOpen">�X���J���Ă���Ƃ���true��Ԃ��f���Q�[�g�^�ł�</param>
        public static void StandardSellMenu(List<SimpleMenuItem> list, string shopID, GetPriceInvoker getPrice, Func<bool> isOpen)
        {
            if (isOpen())
            {
                list.Add(new SimpleMenuItem("����", async () => await General.StandardSellAsync(shopID, getPrice)));
            }
            else
            {
                DefaultPersons.�V�X�e��.Say("�X�͕܂��Ă��܂��B");
            }
        }

        /// <summary>
        /// �ǂݍ��܂�ĊǗ����ɂ���S�Ẵ��W���[���ɑ΂��Ďw��̃A�N�V�������s���܂��B
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
        /// �ǂݍ��܂�ĊǗ����ɂ���S�Ẵ��W���[���ɑ΂��Ďw��̃A�N�V�������s���܂��B
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
        /// �V�Q�[���J�n��ǂݍ��܂ꂽ�S���W���[���ɒʒm���܂��B
        /// ���̃��\�b�h�𖾎��I�ɌĂ΂Ȃ��ƒʒm����邱�Ƃ͂���܂���B
        /// (���[�h��Z�[�u�͖����I�ɌĂ΂��Ƃ��ʒm����܂�)
        /// </summary>
        public static void NotifyNewGame()
        {
            ANGFLib.General.CallAllModuleMethod((m) => { m.OnNewGame(); return default; });
        }

        /// <summary>
        /// ���݂̑����Z�b�g���R�s�[���܂��B
        /// </summary>
        /// <returns>�R�s�[���ꂽ�����Z�b�g</returns>
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
        /// �����Z�b�g�������߂��܂��B
        /// </summary>
        /// <param name="set">�����߂������Z�b�g</param>
        public static void SetEquipSet(EquipSet set)
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                Flags.Equip[i] = set.AllItems[i].Id;
            }
        }

        /// <summary>
        /// ���|�[�g�ɒn�}�����܂߂�ꍇ�A���̃��\�b�h���Ăяo�����Ƃ��ł��܂��B���ʂ�x���W�Ń\�[�g����܂��B
        /// </summary>
        /// <param name="writer">�o�͐���w�肵�܂�</param>
        /// <param name="forDebug">True�ł���Εs���ł��S�Ă̏ꏊ���񍐂���܂��B</param>
        public static void WriteReportMap(System.IO.TextWriter writer, bool forDebug)
        {
            foreach (var world in World.List.Values)
            {
                writer.Write("�� {0}�̒n�}", world.HumanReadableName);
                if (forDebug) writer.Write(" (�f�o�b�O��)");
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
        /// �����T�C�N���̊�_���Ԃ������I�ɕύX���܂�
        /// </summary>
        /// <param name="hour"></param>
        public static void ForceChangeCycle(int hour)
        {
            DateTime �V�����̏A�Q���� = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, (hour + 16) % 24, 0, 0);
            if (�V�����̏A�Q���� < Flags.Now)
            {
                �V�����̏A�Q����.AddDays(1.0);
            }
            Flags.�����T�C�N���N�_���� = hour;
            State.�����̏A�Q���� = �V�����̏A�Q����;
        }

        /// <summary>
        /// C#�ŗL���Ȗ��O�ɃG���R�[�h����
        /// ���܂��ܓ������O�ɃG���R�[�h�����\���͔ے�ł��Ȃ�
        /// </summary>
        /// <param name="src">���̖��O</param>
        /// <returns>�G���R�[�h���ꂽ���O</returns>
        public static string ToCSKeyword(string src)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(src);
            return Convert.ToBase64String(bytes).Replace("+", "_P").Replace("/", "_S").Replace("=", "_E");
        }

        /// <summary>
        /// SuperTalk��#CSS�ɑΉ����郁�\�b�h���𓾂܂�
        /// </summary>
        /// <param name="id">#CSS��ID</param>
        /// <returns>���\�b�h��</returns>
        public static string GenerateProcName(string id)
        {
            return "proc" + ToCSKeyword(id);
        }

        /// <summary>
        /// �������p��p�ł�
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
        /// �������p��p�ł�
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
        /// ���|�[�g�ɃX�P�W���[���ꗗ���܂߂܂��B���ڌĂяo���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="writer">�o�͐���w�肵�܂�</param>
        /// <param name="forDebug">True�ł���Εs���ł��S�Ă̏ꏊ���񍐂���܂��B</param>
        public static void WriteReportSchedules(System.IO.TextWriter writer, bool forDebug)
        {
            if (Schedule.List.Values.Count() == 0) return;
            writer.WriteLine("�� �X�P�W���[��");
            foreach (var schedule in Schedule.List.Values)
            {
                if (State.IsScheduleVisible(schedule.Id))
                {
                    writer.WriteLine($"�E{schedule.HumanReadableName} {schedule.Description} ({schedule.StartTime}-{schedule.StartTime + schedule.Length})");
                }
                else if (forDebug)
                {
                    writer.WriteLine($"�E{schedule.HumanReadableName} {schedule.Description}");
                }
            }
        }

        /// <summary>
        /// ���|�[�g�ɃR���N�V�����ꗗ���܂߂܂��B���ڌĂяo���ׂ��ł͂���܂���B
        /// </summary>
        /// <param name="writer">�o�͐���w�肵�܂�</param>
        /// <param name="forDebug">True�ł���Εs���ł��S�Ă̏ꏊ���񍐂���܂��B</param>
        public static void WriteReportCollections(System.IO.TextWriter writer, bool forDebug)
        {
            foreach (var collection in SimpleName<Collection>.List.Values)
            {
                writer.Write("�� {0} (�R���N�V����)", collection.HumanReadableName);
                if (forDebug) writer.Write(" (�f�o�b�O��)");
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
        /// �������p��p�ł��B
        /// </summary>
        public static void Write���OID�Ή��\(string filename)
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
        /// �ړ��\�����肷��
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool candMoveMenu(Place p)
        {
            // �����������Ă��Ȃ��ꏊ�͑ΏۊO�ł���
            if (!p.HasDistance) return false;
            // �����������Ă��Ă�������Parent�����L���Ă���ƑΏۊO�ł���
            if (State.CurrentPlace.ParentTopID == p.ParentTopID) return false;
            // �Ώۂł���
            return true;
        }

        /// <summary>
        /// �T�u�ړ��\�����肷��
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool candSubMoveMenu(Place p)
        {
            // Parent������Id�ł���Ɩ������ɑΏۂɂȂ�
            if (State.CurrentPlace.ParentTopID == p.ParentTopID) return true;
            // �����������Ă��Ă��ΏۊO�Ƃ͂����Ȃ�
            // �����܂ŗ���΋����̗L���Ɋ֌W�Ȃ��ΏۊO
            return false;
        }

        /// <summary>
        /// MoveMenu�ɏo�����Ȍ�₪����
        /// </summary>
        /// <param name="checker"></param>
        /// <param name="HowToMove"></param>
        /// <returns></returns>
        public static bool CanMove(Func<Place, bool> checker, HowToMoveInvoker HowToMove, bool isSub = false)
        {
            // �T�u�ړ�����Ȃ��Ƃ�����
            if (!isSub)
            {
                // ���̃����N����������Ώ�
                if (State.CurrentPlace.GetLinkedPlaceIds().Any(c => Place.List[c].Visible)) return true;
            }
            // �ႤWorld�͑ΏۊO
            // �����ꏊ�͑ΏۊO
            // �����Ȃ��ꏊ�͑ΏۊO
            return SimpleName<Place>.List.Values.AsParallel().Any(c => Flags.CurrentWorldId == c.World && Flags.CurrentPlaceId != c.Id && c.Visible && checker(c) && HowToMove(State.CurrentPlace, c).IsAvailable);
        }

        /// <summary>
        /// �ړ����j���[
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
                // ���̃����N����������Ώ�
                foreach (var p in State.CurrentPlace.GetLinkedPlaceIds().Select(c => Place.List[c]).Where(c => c.Visible))
                {
                    menuList.Add(new SimpleMenuItem(p.HumanReadableName, () => isByLink = true, p));
                }
            }

            // �����̍������̂��߂�threshold���z����xy�̍����͌�₩�痎�Ƃ�
            const int threshold = 10000; // 10000m=10km
            var query = Place.List.Values.Where(c => cand(c) && Flags.CurrentWorldId == c.World && Flags.CurrentPlaceId != c.Id && c.Visible).Where(c => Math.Abs(c.GetParentDistance().x - State.CurrentPlace.GetParentDistance().x) <= threshold).Where(c => Math.Abs(c.GetParentDistance().y - State.CurrentPlace.GetParentDistance().y) <= threshold).OrderBy(c => c.GetParentDistance().GetDistanceFromSquared(State.CurrentPlace.GetParentDistance()));

            foreach (var p in query)
            {
                MoveInfo moveInfo = HowToMove(State.CurrentPlace, p);
                if (moveInfo.IsAvailable) menuList.Add(new SimpleMenuItem(p.HumanReadableName + moveInfo.SupplyDescription, null, p));
            }
            int selection = await UI.SimpleMenuWithCancelAsync("�ǂ��ɍs��������?", menuList.ToArray());
            if (selection < 0) return false; // �ǂ��ɂ��s���Ȃ�

            string result = State.CurrentPlace.FatalLeaveConfim((Place)menuList[selection].UserParam);
            if (result != null)
            {
                DefaultPersons.�Ɣ�.Say(result);
                return false;
            }

            return await ConfirmFashionAndGotoAsync((Place)menuList[selection].UserParam, isByLink);
        }

        /// <summary>
        /// �t�@�b�V�����`�F�b�N�ƈړ�
        /// </summary>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static async Task<bool> ConfirmFashionAndGotoAsync(Place dst, bool byLink)
        {
            string result = State.CurrentPlace.LeaveConfim(dst);
            if (result != null)
            {
                DefaultPersons.�V�X�e��.Say(result);
                if (!await UI.YesNoMenuAsync("�{���Ɉړ����܂���?", "YES", "NO")) return false; // �ǂ��ɂ��s���Ȃ�
            }
            if (byLink) await State.GoToMyLinkAsync(dst);
            else await State.GoToAsync(dst);
            return true;
        }

        /// <summary>
        /// ��b���j���[�̍쐬
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
            int index = await UI.SimpleMenuWithCancelAsync("�N�Ƙb������", list.ToArray());
            if (index < 0) return false;
            return true;
        }

        /// <summary>
        /// ���̏ꏊ�̂��̐l��T��
        /// </summary>
        /// <param name="placeID"></param>
        /// <returns></returns>
        public static Person[] FindPersonsWithPlace(string placeID)
        {
            return Person.List.Values.OfType<PersonWithPlace>().Where(c => c.PlaceID == placeID && c.IsAvailable()).ToArray();
        }

        /// <summary>
        /// �\�[�g���ꂽ�������ʃ��X�g
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EquipType> GetSortedQeuipTypes()
        {
            return from z in SimpleName<EquipType>.List.Values orderby z.Priority select z;
        }

        /// <summary>
        /// ���̃A�C�e���͏������̑S�Ă�N�����������Ă��邩?
        /// </summary>
        /// <param name="itemId">�A�C�e��</param>
        /// <returns>true�Ȃ�S�đ����ς�</returns>
        public static bool IsAnyoneEquippedAllItems(string itemId, string targetPersonId = null)
        {
            int usedCont = 0;
            foreach (var partymember in Person.List.Values.OfType<IPartyMember>())
            {
                //�@���O�w�肳�ꂽPerson�̑����i�̓A�J�E���g���Ȃ��B
                if (partymember.GetPerson().Id == targetPersonId) continue;
                foreach (var targetItemId in partymember.GetEquippedItemIds())
                {
                    if (targetItemId == itemId)
                    {
                        usedCont++;
                        break;  // 1�l�ɂ�1�J�E���g�A�b�v���� (2�̏ꏊ���߂�A�C�e�������邽��)
                    }
                }
            }
            return State.GetItemCount(itemId) <= usedCont;
        }

        /// <summary>
        ///  �����A�C�e���̌�⃊�X�g
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static Item[] GetCandidateEquipItems(int targetType)
        {
            List<Item> list = new List<Item>();
            foreach (Item item in Items.GetItemList())
            {
                if (targetType < item.AvailableEquipMap.Length && item.AvailableEquipMap[targetType])
                    if (State.GetItemCount(item) > 0)   // ������������Ă��邩?
                        if (!IsAnyoneEquippedAllItems(item.Id)) // �N���������Ă��Ȃ�
                            list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// �A�C�e������j���[�̃��x��������쐬
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateItemConsumeLabelText(Item item)
        {
            if (item == null || item.IsItemNull) return "----";
            return string.Format("{0,2}�� {1} ({2}��g�p)", State.GetItemCount(item), item.HumanReadableName, State.GetUsedCount(item));
        }

        /// <summary>
        /// �A�C�e���������j���[�̃��x��������쐬
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateItemEquipLabelText(Item item)
        {
            if (item == null || item.IsItemNull) return "----";
            return string.Format("{0} ({1}��g�p)", item.HumanReadableName, State.GetUsedCount(item));
        }

        /// <summary>
        /// �}�j�t�F�X�g���������[�h����
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
#if MYBLAZORAPP
        public static Tuple<XDocument, Version> LoadManifestOnly(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
#pragma warning disable SYSLIB0018 // �^�܂��̓����o�[�����^���ł�
            var assem = Assembly.ReflectionOnlyLoad(buffer);
#pragma warning restore SYSLIB0018 // �^�܂��̓����o�[�����^���ł�
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
        /// �t�@�C��������t�@�C���ԍ��𓾂�@(WebPlayer�p)
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
        /// �W���[�i�����O����(�S)
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
        /// �G���[��񍐂��܂��B(������UI�ˑ�)
        /// ������Ԃœ����Ă���̂͑傴���ςȃf�t�H���g
        /// </summary>
        public static Func<string, Dummy> ReportError = (message) =>
        {
            System.Diagnostics.Trace.Fail(message);
            return default;
        };

        /// <summary>
        /// �e�X�g(�W���[�i�����O)�@�\���L���ł���ꍇTrue�ł��B
        /// </summary>
        [Obsolete]
        public static bool IsJournalingEnabled
        {
            get { return State.JournalingPlayer != null; }
        }
        /// <summary>
        /// �݊����̂��߂Ɏc����Ă���t�B�[���h�ŉ���@�\�������Ȃ��B
        /// �����IsTestingModeEx���g�p���ׂ�
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
        /// �Ō�ɏA�Q�����u�������E�v�̓��t��yyyyMMdd�`���ŕ����񉻂�������
        /// </summary>
        public static string LastSleepDateYyyyMmDd
        {
            get => SystemFile.GetFlagString("LastSleepDateYyyyMmDd");
            set => SystemFile.SetFlagString("LastSleepDateYyyyMmDd", value);
        }

        /// <summary>
        /// �y�Ō�ɏA�Q�����u�������E�v�̓��t�z�ŏA�Q������
        /// ���t���ς���Ă���Βl�͖�������0�ɖ߂������̂Ƃ��Ĉ���
        /// </summary>
        public static int GetLastSleepDateCount()
        {
            confirmLastSleepDate();
            return SystemFile.GetFlag("LastSleepDateCount");
        }

        /// <summary>
        /// �y�Ō�ɏA�Q�����u�������E�v�̓��t�z�ŏA�Q�����񐔂����Z����
        /// </summary>
        public static void IncrementLastSleepDateCount()
        {
            SystemFile.SetFlag("LastSleepDateCount", GetLastSleepDateCount() + 1);

        }

        /// <summary>
        /// �������E��1���ŋ������A�Q��
        /// ���̒l�̓X�^�[��ǉ��w�����邱�ƂōX�V�ł���
        /// ���t���ς���Ă���Βl�͖������ċK��l�ɖ߂������̂Ƃ��Ĉ���
        /// </summary>
        public static int GetEnabledSleepCount()
        {
            confirmLastSleepDate();
            return SystemFile.GetFlag("EnabledSleepCount");
        }
        /// <summary>
        /// �������E��1���ŋ������A�Q�񐔂����Z����
        /// </summary>
        public static void IncrementEnabledSleepCount()
        {
            SystemFile.SetFlag("EnabledSleepCount", GetEnabledSleepCount() + 1);
        }


        /// <summary>
        /// ANGFLib�̃A�Z���u����Ԃ�
        /// </summary>
        /// <returns>ANGFLib�̃A�Z���u��</returns>
        public static Assembly GetAngfLibAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// �������Ԉړ�
        /// </summary>
        /// <param name="newDate">�V��������(�m�[�`�F�b�N)</param>
        public static async Task TimeWarpAsync(DateTime newDate)
        {
            Flags.Now = newDate;
            State.�����̋N������ = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, State.�N������, 0, 0);
            State.�����̏A�Q���� = State.�����̋N������.AddHours(16);
            //JournalingWriter.WriteEx(State.SeekModule(this), "TIME", Flags.Now.ToString(Constants.DateFormat));
            Flags.Now = Flags.Now.AddHours(State.�N������);
            State.�����̋N������ = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day, State.�N������, 0, 0);
            State.�����̏A�Q���� = State.�����̋N������.AddHours(16);
            DefaultPersons.�V�X�e��.Say("�V�X�e���̐������m�ۂ̂��߂Ɏ��̒��ɍs���܂��B");
            await State.GoNextDayMorningAsync();
            UI.Actions.ResetGameStatus();
        }

        public static EquipSet �����i���̃R�s�[()
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

        // �����N�ړ����̏���Ԃ����W���[���ɖ₢���킹��
        internal static int �����N�ړ����Ԍv�Z(Place currentPlace, Place distPlace)
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var r = item.QueryObjects<ILinkMoveCalc>();
                if (r.Length > 0) return r[0].�����N�ړ����Ԍv�Z(currentPlace, distPlace);
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
        /// �w�肳�ꂽ���ߍ��݃��\�[�X��ǂݍ���ŃX�v���b�V���摜�Ƃ��ĕ\������
        /// </summary>
        /// <param name="assembly">�ΏۃA�Z���u��</param>
        /// <param name="resourcePath">�Ώۃ��\�[�X�̃p�X(���W���[����.�t�@�C����)</param>
        /// <returns>�ǂݍ��߂���true</returns>
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
        /// BlazorWebAssembly�Ŏ��s�������肷��
        /// </summary>
        /// <returns>BlazorWebAssembly�Ŏ��s���ł���</returns>
        public static bool IsBlazorWebAssembly()
        {
            return RuntimeInformation.OSDescription == "Browser";
        }

        /// <summary>
        /// ���S�ȃ_�~�[�����o�[�B������������ł��ǂ�
        /// ������ǂݏo����Ɗ��҂��Ă͂����Ȃ�
        /// </summary>
        public static object DummyOfDummy = null;

        /// <summary>
        /// Azure�̊�{�C���X�^���X�Ŏ��s���ł��𔻒肷��
        /// </summary>
        /// <remarks>
        /// Azure��WANGF��{�C���X�^���X��Home��POST���N�G�X�g�𓊂��邱�Ƃ�SQL�T�[�o��
        /// SystemFile�̃C���[�W���i�[����̂ŁA���̃z�X�g�ȊO�ɓ����邱�Ƃ͊�{�I�ɈӖ����Ȃ�
        /// �܂��\�[�X�I���W��(CORS)�̖�肪����̂ŁA�����炱���ւ͓������Ȃ�
        /// ������A�܂��ɂ���URL�Ŏ��s���Ă��鎞�ȊO�̓l�b�g���M�ł��Ȃ��̂ł���B
        /// </remarks>
        /// <returns>Azure�̊�{�C���X�^���X�Ŏ��s��</returns>
        public static bool IsInAzure() => UI.Actions.GetUri().ToLower().Contains("wangf.azurewebsites.net");

        /// <summary>
        /// �f�[�^�ۑ��̋��p�f�B���N�g���̃��[�g�𓾂� (Maui only)
        /// </summary>
        /// <returns>�f�B���N�g��</returns>
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
        /// ProgramData�ȉ��̋��p�f�B���N�g���̃��[�g�𓾂� (Maui only)
        /// </summary>
        /// <returns>�f�B���N�g��</returns>
        public static string GetCommonRootDirectory()
        {
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "autumn", "WANGF");
            Directory.CreateDirectory(p);   // create it if not exist
            return p;
        }

        /// <summary>
        /// �W���[�i�����O�t�@�C���̋��p�f�B���N�g���𓾂� (Maui only)
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
        /// �W���[�i�����O�̃C���N���[�h�ŒT������A�Z���u�����w�肷��
        /// </summary>
        public static Assembly TestingAssembly { get; set; } = null;

        public static Func<Task> CallToRestoreActionSetAsync;/* DIABLE ASYNC WARN */
        public static Func<string, Task> CallToTellAssertionFailedAsync;/* DIABLE ASYNC WARN */
        public static Func<string, Task> CallToProgressStatusAsync;/* DIABLE ASYNC WARN */
    }
}
