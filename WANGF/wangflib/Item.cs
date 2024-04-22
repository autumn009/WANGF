using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// �A�C�e�����L�q������N���X��񋟂��܂��B
    /// </summary>
    [Serializable]
    public class Item : SimpleName<Item>
    {
        /// <summary>
        /// �l�Ԃ��ǂ߂閼�O�ł��B
        /// </summary>
        public override string HumanReadableName
        {
#pragma warning disable
            get { return Name; }
#pragma warning resotore
        }
        /// <summary>
        /// ���ʗp�̈�ӂ�Id��Ԃ��܂��B
        /// </summary>
        public override string Id
        {
            get { return id; }
        }
        /// <summary>
        /// �l�Ԃ��ǂ߂閼�O�ł��B
        /// </summary>
        [Obsolete]
        public readonly string Name;
        /// <summary>
        /// Guid������Ȃ�Id�������܂��Bnull�܂��͋󕶎���Ȃ�Null�A�C�e���������܂��B
        /// </summary>
        private readonly string id;
        /// <summary>
        /// ��{�I�Ȑ���������ł��B
        /// </summary>
        public readonly string BaseDescription;
        /// <summary>
        /// ��x�ɏ��L�ł���ő吔���w�肵�܂��B
        /// </summary>
        public readonly int Max;
        /// <summary>
        /// ���i���w�肵�܂��B�P�ʂ̓��W���[���ˑ��ł��B0�͔����s�̃A�C�e���������܂��B
        /// </summary>
        public readonly int Price;
        /// <summary>
        /// �O�����Ƃ��ł��Ȃ��A�C�e���������܂��B�����������I�ȃv���O�����ɂ�鏑�������͂ł��܂��B
        /// </summary>
        public readonly bool Is���O���s�\Item;
        /// <summary>
        /// Null�A�C�e���Ȃ�True��Ԃ��܂��B
        /// </summary>
        public bool IsItemNull { get { return Id == null || Id == ""; } }
        /// <summary>
        /// ���S�Ȑ����������Ԃ��܂��B
        /// �ǉ�������e������ꍇ�̓I�[�o�[���C�h���ĉ��H���܂��B
        /// </summary>
        public virtual string FullDescription
        {
            get { return BaseDescription; }
        }
        /// <summary>
        /// ���j���[���g�p�ł���A�C�e���ł��B
        /// </summary>
        public virtual bool IsConsumeItem { get { return false; } }
        /// <summary>
        /// �A�C�e�����g�p���܂��B
        /// </summary>
        /// <returns>����ď��ł�����true�B�c������false�B</returns>
        public virtual async Task<bool> ����Async()
        {
            DefaultPersons.�V�X�e��.Say(HumanReadableName + "�͎g�p�ł��܂���B", HumanReadableName);
            return false;
        }
        /// <summary>
        /// �e���v���[�g����A�C�e���𐶐����܂��B
        /// </summary>
        /// <param name="t">�e���v���[�g�I�u�W�F�N�g�ł��B</param>
        /// <returns>�������ꂽ�I�u�W�F�N�g�ł��B</returns>
        public static implicit operator Item(ItemTemplate t)
        {
            return new Item(t);
        }
        /// <summary>
        /// ����̑������ʂɁu���v�����ł��邩�𔻒肵�܂��B
        /// ����̑������ʂɑ����ł��邩�ł͂Ȃ����Ƃɒ��ӁB
        /// ����͕ʂ̃��C���[�ŏ��������
        /// ���̃��\�b�h�̈Ӌ`�́A���Ƃ��΂�������𖞂����Ȃ��Ƒ����ł��Ȃ��A�C�e����
        /// ���肷��悤�Ȏg����
        /// ������part�́A2�ӏ��ȏ�ɑ����\�ȃA�C�e���ŁA�ʁX�ɏ������ݒ肳��Ă���
        /// �P�[�X�ɑΉ����邽��
        /// </summary>
        /// <param name="part">�������悤�Ƃ��Ă��镔��</param>
        /// <returns>null�Ȃ瑕���\�B������Ȃ瑕���ł��Ȃ����R</returns>
        public virtual string CanEquip(int part) { return null; }
        /// <summary>
        /// ����̑������ʂɁu���v�����ł��邩�𔻒肵�܂��B
        /// ����̑������ʂɑ����ł��邩�ł͂Ȃ����Ƃɒ��ӁB
        /// ����͕ʂ̃��C���[�ŏ��������
        /// ���̃��\�b�h�̈Ӌ`�́A���Ƃ��΂�������𖞂����Ȃ��Ƒ����ł��Ȃ��A�C�e����
        /// ���肷��悤�Ȏg����
        /// ������part�́A2�ӏ��ȏ�ɑ����\�ȃA�C�e���ŁA�ʁX�ɏ������ݒ肳��Ă���
        /// �P�[�X�ɑΉ����邽��
        /// </summary>
        /// <param name="part">�������悤�Ƃ��Ă��镔��</param>
        /// <param name="personId">�������悤�Ƃ��Ă���Ώېl��</param>
        /// <param name="equipSet">���̎��̑����Z�b�g�S��</param>
        /// <returns>null�Ȃ瑕���\�B������Ȃ瑕���ł��Ȃ����R</returns>
        public virtual string CanEquipEx(int part, string personId, EquipSet equipSet) { return null; }
        /// <summary>
        /// �ÓI�ȑ����\���ʃ}�b�v��񋟂��܂��B
        /// �z���Ԃ��̂�2�ȏ�̕��ʂɑ����ł���ꍇ�ɑΉ����邽��
        /// �T�C�Y�O�̗v�f��false�����肳��A�T�C�Y0�̔z��͑����s�\�A�C�e�����Ӗ�����
        /// �����Ŏw�肳�ꂽ�A�C�e���͑������Ɍ����邪�ACanEquip���\�b�h�ŗ��R��t���ċ��ۂł���
        /// </summary>
        /// <returns>�����\���ʂ̔z��</returns>
        public bool[] AvailableEquipMap = new bool[0];
        /// <summary>
        /// �����ɑ������镔�ʂ������܂��B�㉺���Ȃ�
        /// �T�C�Y�O�̗v�f��false�����肳��邪�AAvailableEquipMap�Ŏw�肳�ꂽ�ꏊ��
        /// �ÖٓI�ɑ����\�ƂȂ邽�߁A�񋟂��Ȃ��ꍇ��1�ӏ����������\�ƂȂ�
        /// </summary>
        /// <returns></returns>
        public bool[] SameTimeEquipMap = new bool[0];

        /// <summary>
        /// ID����A�C�e���𓾂܂��B�A�C�e���������ꍇ�͎����I��ItemNull���Ԃ���܂�
        /// </summary>
        /// <param name="id">�A�C�e����ID�ł�</param>
        /// <returns>���������A�C�e���ł�</returns>
        public static Item GetItemById(string id)
        {
            if (id != null && id != "" && Item.List.TryGetValue(id, out var item)) return item;
            return Items.ItemNull;
        }

        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="t">�e���v���[�g�E�I�u�W�F�N�g�ł��B</param>
        public Item(ItemTemplate t)
        {
#pragma warning disable
            Name = t.Name;
#pragma warning resotore
            id = t.Id;
            Max = t.Max;
            Price = t.Price;
            BaseDescription = t.BaseDescription;
            AvailableEquipMap = t.AvailableEquipMap;
            SameTimeEquipMap = t.SameTimeEquipMap;
            Is���O���s�\Item = t.Is���O���s�\Item;
        }

        public Item CloneExactItem()
        {
            var clone = new ItemTemplate();
            clone.Name = Name;
            clone.Id = Id;
            clone.Max = Max;
            clone.Price = Price;
            clone.BaseDescription = BaseDescription;
            clone.AvailableEquipMap = AvailableEquipMap;
            clone.SameTimeEquipMap = SameTimeEquipMap;
            clone.Is���O���s�\Item = Is���O���s�\Item;
            return new Item(clone);
        }
    }

    /// <summary>
    /// �A�C�e���𐶐�����e���v���[�g�ɂȂ�܂��B
    /// </summary>
	public class ItemTemplate
	{
        /// <summary>
        /// �A�C�e���̖��O�ł��B
        /// </summary>
		public string Name ="(no name)";
        /// <summary>
        /// ��ӂ̎��ʖ��ł��B
        /// </summary>
        public string Id;
        /// <summary>
        /// �ő及�L���ł��B
        /// </summary>
		public int Max=1;
        /// <summary>
        /// ���i�ł��B0�͔����s�ł��B
        /// </summary>
		public int Price;
        /// <summary>
        /// �O�����Ƃ��ł��Ȃ��A�C�e���������܂��B�����������I�ȃv���O�����ɂ�鏑�������͂ł��܂��B
        /// </summary>
        public bool Is���O���s�\Item;
        /// <summary>
        /// �x�[�X�ƂȂ�������ł��B
        /// </summary>
		public string BaseDescription = "";
        /// <summary>
        /// �����\�ȕ��ʂ�true�ɂȂ�܂��B
        /// </summary>
        public bool[] AvailableEquipMap = new bool[0];
        /// <summary>
        /// �����ɑ�������镔�ʂ�true�ɂȂ�܂��B
        /// </summary>
        public bool[] SameTimeEquipMap = new bool[0];
    }

    /// <summary>
    /// �A�C�e���̃R���N�V�������Ǘ����܂��B
    /// </summary>
	public static class Items
	{
        /// <summary>
        /// �֋X��񋟂��鑶�݂��Ȃ����Ƃ������A�C�e��
        /// Id=null�܂���Id=""�̃A�C�e���́A�����ݒ莞�ɉ����������Ă��Ȃ����Ƃ���������A�C�e��
        /// </summary>
        public static Item ItemNull
        {
            get { return itemNull; }
        }
        private static Item itemNull = new ItemTemplate()
		{
			Id = "",
			Name = "Null",
			BaseDescription = "(����)",
			Max = 0,
			Price = 0,
		};

        /// <summary>
        /// Id����A�C�e���������BId��null�܂���""�̏ꍇ��ItemNull��Ԃ��B
        /// </summary>
        /// <param name="number">Id������</param>
        /// <returns>�������ꂽ�A�C�e��</returns>
		public static Item GetItemByNumber(string number)
		{
			if (number == null || number == "") return ItemNull;
			return SimpleName<Item>.List[number];
		}

        /// <summary>
        /// �A�C�e����Id�̈ꗗ��Ԃ�
        /// </summary>
        /// <returns>�A�C�e����Id�̈ꗗ</returns>
		public static IEnumerable<string> GetItemIDList()
		{
            return Item.List.Keys;
		}

        /// <summary>
        /// �A�C�e���̈ꗗ��Ԃ�
        /// </summary>
        /// <returns>�A�C�e���̈ꗗ</returns>
        public static IEnumerable<Item> GetItemList()
        {
            return Item.List.Values;
        }
	}
}
