using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// ���ʂ��L�q���܂��B
    /// </summary>
    [System.Reflection.ObfuscationAttribute(Exclude = true)]
    public enum Sex
    {
        /// <summary>
        /// �j��
        /// </summary>
        Male,
        /// <summary>
        /// ����
        /// </summary>
        Female,
        /// <summary>
        /// ���ʂȂ� (�P�Ȃ�@�ނȂǂ�l��������ꍇ�Ȃ�)
        /// </summary>
        Nutral,
        /// <summary>
        /// �V�[���[��
        /// </summary>
        Shemale,
        /// <summary>
        /// �g�����X�x�X�^�C�g(�ِ�����)
        /// ���̒�`�͐Q��点�l�b�g�ł̂ݎg�p��������Ȓl�B���̃^�C�g���ł͎g�p���Ă͂Ȃ�Ȃ��B
        /// </summary>
        Tv
    }
    /// <summary>
    /// �l���������N���X�ł��B
    /// </summary>
    public class Person : SimpleName<Person>
    {
        private string id;
        private string name;
        private Color color;
        private Sex sex;
        /// <summary>
        /// ��ӂ�Id���w�肵�܂��B
        /// </summary>
        public override string Id => id;
        /// <summary>
        /// �l�Ԃɉǂ̖��O�ł��B
        /// </summary>
        public override string HumanReadableName => name;
        /// <summary>
        /// �l�Ԃɉǂ̖��O�ł��B
        /// </summary>
        public virtual string MyName => HumanReadableName;
        /// <summary>
        /// 2���ȏア��ꍇ�ɁA��ʍ����̑�����ʂɏo�閼�O�ł��B
        /// </summary>
        public virtual string HumanReadableNameForEquipArea => HumanReadableName;
        /// <summary>
        /// ���b�Z�[�W�̐F��Ԃ��܂��B
        /// </summary>
        public virtual Color MessageColor => color;
        /// <summary>
        /// ���ʂ�Ԃ��܂��B
        /// </summary>
        public virtual Sex ���� => sex;
        /// <summary>
        /// ���̐l����HP�Ȃǂ̐퓬�����t�@�C���ɃZ�[�u���ׂ����������B
        /// ���̏���waSimpleRPGBase�̊g�����\�b�h�̓�������߂邽�߂ɎQ�Ƃ����B
        /// </summary>
        public virtual bool IsRequirePersistence => false;

        /// <summary>
        /// �������s���܂��Bstring.Format�����̏����̐��`���s���܂��B
        /// </summary>
        /// <param name="format">�������܂ޕ�����ł��B</param>
        /// <param name="arg">�C�ӂ̃I�u�W�F�N�g��ł��B</param>
        public virtual void Say(string format, params Object[] arg)
        {
            UI.M(this, string.Format(format, arg));
        }
        /// <summary>
        /// �������s���܂��Bstring.Format�����̏����̐��`�͍s���܂���B
        /// </summary>
        /// <param name="message">���b�Z�[�W�ł��B</param>
        public virtual void Say(string message)
        {
            UI.M(this, message);
        }
        /// <summary>
        /// �����_���ɃJ���[�𐶐����܂��B
        /// </summary>
        /// <param name="name">���O�ł��B�F�̑O��Ƃ��Ďg���܂��B</param>
        /// <param name="sex">���ʂł��B���ʂ��ƂɐF��ς��܂��B</param>
        /// <returns>�������ꂽ�F�ł��B</returns>
        protected Color generateRandomColor(string name, Sex sex)
        {
            this.sex = sex;
            Color[] baseTable;
            switch (sex)
            {
                case Sex.Male:
                    baseTable = new Color[] { 
						Color.FromArgb(64,64,128),
						Color.FromArgb(64,128,128),
						Color.FromArgb(64,128,64),
					};
                    break;
                case Sex.Female:
                    baseTable = new Color[] { 
						Color.FromArgb(128,64,128),
						Color.FromArgb(96,96,64),
						Color.FromArgb(128,64,64),
					};
                    break;
                case Sex.Shemale:
                    goto case Sex.Female;
                default:
                    baseTable = new Color[] { 
						Color.FromArgb(128,128,128),
					};
                    break;
            }
            uint baseNumber = 0;
            foreach (char ch in name)
            {
                baseNumber += ch;
            }
            // baseNumber�̗L���r�b�g���͑����Ȃ��B8bit���炢�Ǝv���ėǂ��̂���
            Color baseColor = baseTable[baseNumber % baseTable.Length];
            Color underLimit = Color.FromArgb(32, 32, 32);
            Color ���邳�␳�l = Color.FromArgb(0, 0, 0);
            int R�␳�l = (int)((baseNumber >> 4) & 0x03);
            int G�␳�l = (int)((baseNumber >> 6) & 0x03);
            int B�␳�l = (int)((baseNumber >> 6) & 0x03);
            return Color.FromArgb(baseColor.R + (underLimit.R - baseColor.R) * R�␳�l / 4 + ���邳�␳�l.R,
                baseColor.G + (underLimit.G - baseColor.G) * G�␳�l / 4 + ���邳�␳�l.G,
                baseColor.B + (underLimit.B - baseColor.B) * B�␳�l / 4 + ���邳�␳�l.B);
        }

        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł��B</param>
        /// <param name="sex">���ʂł��B</param>
        public Person(string id, string name, Sex sex) : this(id, name, Color.Empty, sex) { }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł��B</param>
        /// <param name="color">�F�ł��B</param>
        public Person(string id, string name, Color color) : this(id, name, color, Sex.Nutral) { }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł��B</param>
        /// <param name="color">�F�ł��B</param>
        /// <param name="sex">���ʂł��B</param>
        public Person(string id, string name, Color color, Sex sex)
        {
            this.id = id;
            this.name = name;
            this.sex = sex;
            if (color.IsEmpty)
                this.color = generateRandomColor(name, sex);
            else
                this.color = color;
        }
    }

    /// <summary>
    /// �u���v�������b�҂ł��B
    /// </summary>
    public class PersonWatashi : Person, IPartyMember
    {
        /// <summary>
        /// General.GetMyName()�̒l��Ԃ��܂��B
        /// </summary>
        public override string HumanReadableName
        {
            get
            {
                return General.GetMyName();
            }
        }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        public PersonWatashi()
            : base("{4F698BAA-4082-4123-9A0B-9997B5B8E041}", null, Color.FromArgb(96, 64, 64))
        { }

        // �p�[�e�B�[�����o�[���b�p�B������Ԃ�
        public Person GetPerson() => this;

        // �p�[�e�B�[�����o�[���b�p�B�f�t�H���g�����i��Ԃ�
        public IEnumerable<string> GetEquippedItemIds()
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                yield return Flags.Equip[i];
            }
        }

        public string GetRawEquip(int index)
        {
            return Flags.equip[index.ToString()];
        }

        public void SetRawEquip(int index, string id)
        {
            Flags.equip[index.ToString()] = id;
        }
        public override Sex ����
        {
            get
            {
                foreach (var item in State.LoadedModulesEx)
                {
                    var r = item.QueryObjects<IMySexProvider>();
                    if (r.Length > 0) return r[0].GetMySex();
                }
                return base.����;
            }
        }
        public override bool IsRequirePersistence => true;
    }

    /// <summary>
    /// �u���v�̐��ʂ�񋟂���J�X�^���v���o�C�_
    /// ModuleEx�o�R�Œ񋟂���
    /// </summary>
    public interface IMySexProvider
    {
        public Sex GetMySex();
    }

    /// <summary>
    /// �Ɣ����s���b�҂ł��B
    /// </summary>
    public class PersonDokuhaku : Person
    {
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        public PersonDokuhaku()
            : base("{5D8C7D54-2AFE-4dab-98DD-DF0119481A62}", null, Color.FromArgb(96, 32, 32))
        { }
    }

    /// <summary>
    /// �Ɣ����s���b��(�����p)�ł��B
    /// </summary>
    public class PersonSuper : Person
    {
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        public PersonSuper()
            : base("{5D8C7D54-2AFE-4dab-98DD-DF0119481A62}", null, Color.FromArgb(255, 255, 255))
        { }
    }

    /// <summary>
    /// �b�҂ł��B
    /// </summary>
    public class PersonRefer : Person
    {
        private Func<string> nameGetter;
        public override string HumanReadableName
        {
            get
            {
                if (nameGetter == null) return "�_�~�[���O";
                var s = nameGetter();
                if (string.IsNullOrWhiteSpace(s)) return "�_�~�[���O";
                return nameGetter();
            }
        }

        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        public PersonRefer(string id, Func<string> nameGetter, Sex sex )
            : base(id, "�_�~�[���O", sex)
        {
            this.nameGetter = nameGetter;
        }
    }

    /// <summary>
    /// ����̏ꏊ�ɏo������l�����L�q���܂��B
    /// ���W���[���͂��̃N���X���g�p���Ă��g�p���Ȃ��Ă��ł��B
    /// </summary>
    public abstract class AbstractPersonWithPlace : Person
    {
        /// <summary>
        /// �I�[�o�[���C�h���ďo������ꏊ��Ԃ��܂��B
        /// </summary>
        public abstract string PlaceID { get; }
        /// <summary>
        /// �I�[�o�[���C�h���ėL���ł���Γ��I�ɔ��肵��True��Ԃ��܂��B
        /// </summary>
        /// <returns>�L���ł����True�ł��B</returns>
        public abstract bool IsAvailable();
        /// <summary>
        /// �b�����������Ɏ��s���ׂ����e���I�[�o�[���C�h���܂��B
        /// </summary>
        public abstract Task TalkAsync();/* DIABLE ASYNC WARN */
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł�</param>
        /// <param name="sex">���ʂł�</param>
        public AbstractPersonWithPlace(string id, string name, Sex sex) : base(id, name, sex) { }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł�</param>
        /// <param name="color">�\���F�ł��B</param>
        public AbstractPersonWithPlace(string id, string name, Color color) : base(id, name, color) { }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł�</param>
        /// <param name="color">�\���F�ł��B</param>
        /// <param name="sex">���ʂł�</param>
        public AbstractPersonWithPlace(string id, string name, Color color, Sex sex) : base(id, name, color, sex) { }
    }

    /// <summary>
    /// ����̏ꏊ�ɏo������l�����L�q���܂��B
    /// ���W���[���͂��̃N���X���g�p���Ă��g�p���Ȃ��Ă��ł��B
    /// </summary>
    public class PersonWithPlace : AbstractPersonWithPlace
    {
        private string placeID;
        private Func<Person, bool> isAvailable;
        private Func<Person,Task> talkAsync;/* DIABLE ASYNC WARN */
        /// <summary>
        /// ���݈ʒu��Ԃ��܂��B
        /// </summary>
        public override string PlaceID { get { return placeID; } }
        /// <summary>
        /// �g�p�\�Ȃ�True�ł��B
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            return this.isAvailable(this);
        }
        /// <summary>
        /// ���j���[�ŉ�b��I�������Ƃ��Ɏ��s����܂��B
        /// </summary>
        public override async Task TalkAsync()
        {
            await talkAsync(this);
        }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł�</param>
        /// <param name="sex">���ʂł�</param>
        /// <param name="placeID">�ꏊ�ł��B</param>
        /// <param name="isAvailable">�o������ꍇ��True��Ԃ��܂�</param>
        /// <param name="talk">���j���[�I�����̃A�N�V�����ł�</param>
        public PersonWithPlace(string id, string name, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person,Task> talk) : this(id, name, Color.Empty, sex, placeID, isAvailable, talk) { }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="id">��ӂ�Id�ł��B</param>
        /// <param name="name">���O�ł�</param>
        /// <param name="color">�\���F�ł��B</param>
        /// <param name="sex">���ʂł�</param>
        /// <param name="placeID">�ꏊ�ł��B</param>
        /// <param name="isAvailable">�o������ꍇ��True��Ԃ��܂�</param>
        /// <param name="talkAsync">���j���[�I�����̃A�N�V�����ł�</param>
        public PersonWithPlace(string id, string name, Color color, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person,Task> talkAsync)/* DIABLE ASYNC WARN */
            : base(id, name, color, sex)
        {
            this.placeID = placeID;
            this.isAvailable = isAvailable;
            this.talkAsync = talkAsync;/* DIABLE ASYNC WARN */
        }
    }

    /// <summary>
    /// �f�t�H���g�ŗp�ӂ����l���ł��B
    /// </summary>
    public class DefaultPersons
    {
        /// <summary>
        /// ��l���̔����ł��B
        /// </summary>
        public static Person ��l�� = new PersonWatashi();
        /// <summary>
        /// ��l���̓Ɣ��ł��B
        /// </summary>
        public static Person �Ɣ� = new PersonDokuhaku();
        /// <summary>
        /// �V�X�e���ł��B
        /// </summary>
        public static Person �V�X�e�� = new Person("{C7244E5A-9C96-4017-9528-3CA7CBAA5F86}", "SYSTEM", Sex.Nutral);
        /// <summary>
        /// �x����p�V�X�e���ł��B
        /// </summary>
        public static Person �V�X�e��Warn = new Person("{31d5fe8a-2258-4e4d-82ee-70fd6b52b8b1}","SYSTEM", Color.Red, Sex.Nutral);

        /// <summary>
        /// ���ʋ������b�Z�[�W�ł��B
        /// </summary>
        public static Person Super = new PersonSuper();

        static DefaultPersons()
        {
            // Person.AddAndCheck�͎g���Ȃ��B
            // �����̃I�u�W�F�N�g�ɃI�[�i�[���W���[��ID�͑��݂��Ȃ�����B
            Person.List.Add(��l��.Id, ��l��);
            Person.List.Add(�Ɣ�.Id, �Ɣ�);
            Person.List.Add(�V�X�e��.Id, �V�X�e��);
        }
    }
}
