using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// 性別を記述します。
    /// </summary>
    [System.Reflection.ObfuscationAttribute(Exclude = true)]
    public enum Sex
    {
        /// <summary>
        /// 男性
        /// </summary>
        Male,
        /// <summary>
        /// 女性
        /// </summary>
        Female,
        /// <summary>
        /// 性別なし (単なる機材などを人扱いする場合など)
        /// </summary>
        Nutral,
        /// <summary>
        /// シーメール
        /// </summary>
        Shemale,
        /// <summary>
        /// トランスベスタイト(異性装者)
        /// この定義は寝取らせネットでのみ使用される特殊な値。他のタイトルでは使用してはならない。
        /// </summary>
        Tv
    }
    /// <summary>
    /// 人物を示すクラスです。
    /// </summary>
    public class Person : SimpleName<Person>
    {
        private string id;
        private string name;
        private Color color;
        private Sex sex;
        /// <summary>
        /// 一意のIdを指定します。
        /// </summary>
        public override string Id => id;
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public override string HumanReadableName => name;
        /// <summary>
        /// 人間に可読の名前です。
        /// </summary>
        public virtual string MyName => HumanReadableName;
        /// <summary>
        /// 2名以上いる場合に、画面左側の装備画面に出る名前です。
        /// </summary>
        public virtual string HumanReadableNameForEquipArea => HumanReadableName;
        /// <summary>
        /// メッセージの色を返します。
        /// </summary>
        public virtual Color MessageColor => color;
        /// <summary>
        /// 性別を返します。
        /// </summary>
        public virtual Sex 性別 => sex;
        /// <summary>
        /// その人物のHPなどの戦闘情報をファイルにセーブすべきかを示す。
        /// この情報はwaSimpleRPGBaseの拡張メソッドの動作を決めるために参照される。
        /// </summary>
        public virtual bool IsRequirePersistence => false;

        /// <summary>
        /// 発言を行います。string.Format相当の書式の整形を行います。
        /// </summary>
        /// <param name="format">書式を含む文字列です。</param>
        /// <param name="arg">任意のオブジェクト列です。</param>
        public virtual void Say(string format, params Object[] arg)
        {
            UI.M(this, string.Format(format, arg));
        }
        /// <summary>
        /// 発言を行います。string.Format相当の書式の整形は行いません。
        /// </summary>
        /// <param name="message">メッセージです。</param>
        public virtual void Say(string message)
        {
            UI.M(this, message);
        }
        /// <summary>
        /// ランダムにカラーを生成します。
        /// </summary>
        /// <param name="name">名前です。色の前提として使われます。</param>
        /// <param name="sex">性別です。性別ごとに色を変えます。</param>
        /// <returns>生成された色です。</returns>
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
            // baseNumberの有効ビット数は多くない。8bitぐらいと思って良いのかも
            Color baseColor = baseTable[baseNumber % baseTable.Length];
            Color underLimit = Color.FromArgb(32, 32, 32);
            Color 明るさ補正値 = Color.FromArgb(0, 0, 0);
            int R補正値 = (int)((baseNumber >> 4) & 0x03);
            int G補正値 = (int)((baseNumber >> 6) & 0x03);
            int B補正値 = (int)((baseNumber >> 6) & 0x03);
            return Color.FromArgb(baseColor.R + (underLimit.R - baseColor.R) * R補正値 / 4 + 明るさ補正値.R,
                baseColor.G + (underLimit.G - baseColor.G) * G補正値 / 4 + 明るさ補正値.G,
                baseColor.B + (underLimit.B - baseColor.B) * B補正値 / 4 + 明るさ補正値.B);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です。</param>
        /// <param name="sex">性別です。</param>
        public Person(string id, string name, Sex sex) : this(id, name, Color.Empty, sex) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です。</param>
        /// <param name="color">色です。</param>
        public Person(string id, string name, Color color) : this(id, name, color, Sex.Nutral) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です。</param>
        /// <param name="color">色です。</param>
        /// <param name="sex">性別です。</param>
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
    /// 「私」を示す話者です。
    /// </summary>
    public class PersonWatashi : Person, IPartyMember
    {
        /// <summary>
        /// General.GetMyName()の値を返します。
        /// </summary>
        public override string HumanReadableName
        {
            get
            {
                return General.GetMyName();
            }
        }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public PersonWatashi()
            : base("{4F698BAA-4082-4123-9A0B-9997B5B8E041}", null, Color.FromArgb(96, 64, 64))
        { }

        // パーティーメンバーラッパ。自分を返す
        public Person GetPerson() => this;

        // パーティーメンバーラッパ。デフォルト装備品を返す
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
        public override Sex 性別
        {
            get
            {
                foreach (var item in State.LoadedModulesEx)
                {
                    var r = item.QueryObjects<IMySexProvider>();
                    if (r.Length > 0) return r[0].GetMySex();
                }
                return base.性別;
            }
        }
        public override bool IsRequirePersistence => true;
    }

    /// <summary>
    /// 「私」の性別を提供するカスタムプロバイダ
    /// ModuleEx経由で提供する
    /// </summary>
    public interface IMySexProvider
    {
        public Sex GetMySex();
    }

    /// <summary>
    /// 独白を行う話者です。
    /// </summary>
    public class PersonDokuhaku : Person
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public PersonDokuhaku()
            : base("{5D8C7D54-2AFE-4dab-98DD-DF0119481A62}", null, Color.FromArgb(96, 32, 32))
        { }
    }

    /// <summary>
    /// 独白を行う話者(強調用)です。
    /// </summary>
    public class PersonSuper : Person
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public PersonSuper()
            : base("{5D8C7D54-2AFE-4dab-98DD-DF0119481A62}", null, Color.FromArgb(255, 255, 255))
        { }
    }

    /// <summary>
    /// 話者です。
    /// </summary>
    public class PersonRefer : Person
    {
        private Func<string> nameGetter;
        public override string HumanReadableName
        {
            get
            {
                if (nameGetter == null) return "ダミー名前";
                var s = nameGetter();
                if (string.IsNullOrWhiteSpace(s)) return "ダミー名前";
                return nameGetter();
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public PersonRefer(string id, Func<string> nameGetter, Sex sex )
            : base(id, "ダミー名前", sex)
        {
            this.nameGetter = nameGetter;
        }
    }

    /// <summary>
    /// 特定の場所に出現する人物を記述します。
    /// モジュールはこのクラスを使用しても使用しなくても可です。
    /// </summary>
    public abstract class AbstractPersonWithPlace : Person
    {
        /// <summary>
        /// オーバーライドして出現する場所を返します。
        /// </summary>
        public abstract string PlaceID { get; }
        /// <summary>
        /// オーバーライドして有効であれば動的に判定してTrueを返します。
        /// </summary>
        /// <returns>有効であればTrueです。</returns>
        public abstract bool IsAvailable();
        /// <summary>
        /// 話しかけた時に実行すべき内容をオーバーライドします。
        /// </summary>
        public abstract Task TalkAsync();/* DIABLE ASYNC WARN */
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="sex">性別です</param>
        public AbstractPersonWithPlace(string id, string name, Sex sex) : base(id, name, sex) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="color">表示色です。</param>
        public AbstractPersonWithPlace(string id, string name, Color color) : base(id, name, color) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="color">表示色です。</param>
        /// <param name="sex">性別です</param>
        public AbstractPersonWithPlace(string id, string name, Color color, Sex sex) : base(id, name, color, sex) { }
    }

    /// <summary>
    /// 特定の場所に出現する人物を記述します。
    /// モジュールはこのクラスを使用しても使用しなくても可です。
    /// </summary>
    public class PersonWithPlace : AbstractPersonWithPlace
    {
        private string placeID;
        private Func<Person, bool> isAvailable;
        private Func<Person,Task> talkAsync;/* DIABLE ASYNC WARN */
        /// <summary>
        /// 現在位置を返します。
        /// </summary>
        public override string PlaceID { get { return placeID; } }
        /// <summary>
        /// 使用可能ならTrueです。
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            return this.isAvailable(this);
        }
        /// <summary>
        /// メニューで会話を選択したときに実行されます。
        /// </summary>
        public override async Task TalkAsync()
        {
            await talkAsync(this);
        }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="sex">性別です</param>
        /// <param name="placeID">場所です。</param>
        /// <param name="isAvailable">出現する場合にTrueを返します</param>
        /// <param name="talk">メニュー選択時のアクションです</param>
        public PersonWithPlace(string id, string name, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person,Task> talk) : this(id, name, Color.Empty, sex, placeID, isAvailable, talk) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="color">表示色です。</param>
        /// <param name="sex">性別です</param>
        /// <param name="placeID">場所です。</param>
        /// <param name="isAvailable">出現する場合にTrueを返します</param>
        /// <param name="talkAsync">メニュー選択時のアクションです</param>
        public PersonWithPlace(string id, string name, Color color, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person,Task> talkAsync)/* DIABLE ASYNC WARN */
            : base(id, name, color, sex)
        {
            this.placeID = placeID;
            this.isAvailable = isAvailable;
            this.talkAsync = talkAsync;/* DIABLE ASYNC WARN */
        }
    }

    /// <summary>
    /// デフォルトで用意される人物です。
    /// </summary>
    public class DefaultPersons
    {
        /// <summary>
        /// 主人公の発声です。
        /// </summary>
        public static Person 主人公 = new PersonWatashi();
        /// <summary>
        /// 主人公の独白です。
        /// </summary>
        public static Person 独白 = new PersonDokuhaku();
        /// <summary>
        /// システムです。
        /// </summary>
        public static Person システム = new Person("{C7244E5A-9C96-4017-9528-3CA7CBAA5F86}", "SYSTEM", Sex.Nutral);
        /// <summary>
        /// 警告専用システムです。
        /// </summary>
        public static Person システムWarn = new Person("{31d5fe8a-2258-4e4d-82ee-70fd6b52b8b1}","SYSTEM", Color.Red, Sex.Nutral);

        /// <summary>
        /// 特別強調メッセージです。
        /// </summary>
        public static Person Super = new PersonSuper();

        static DefaultPersons()
        {
            // Person.AddAndCheckは使えない。
            // これらのオブジェクトにオーナーモジュールIDは存在しないから。
            Person.List.Add(主人公.Id, 主人公);
            Person.List.Add(独白.Id, 独白);
            Person.List.Add(システム.Id, システム);
        }
    }
}
