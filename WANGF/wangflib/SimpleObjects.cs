using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// 動的なフラグのコレクションを持つためのクラスです。静的モジュールでのみ使用します。
    /// </summary>
    public class SimpleDynamicObject : DynamicObject
    {
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// (コレクションの型を強制する場合に使用します。
        /// </summary>
        public void InitIfNotExist(string name, object val)
        {
            if (!Data.Keys.Contains(name)) Data[name] = val;
        }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Data.TryGetValue(binder.Name, out result)) return true;
            result = 0; // integer zero is default
            return true;
        }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Data[binder.Name] = value;
            return true;
        }
    }

    /// <summary>
    /// 内部使用専用です。直接利用すべきではありません。
    /// </summary>
    public class SimplePlace : Place
    {
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<string> IdGetter;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override string Id { get { return IdGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<string> NameGetter;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override string HumanReadableName { get { return NameGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<bool> ForceOverrideGetter = () => false;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool ForceOverride { get { return ForceOverrideGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<string> ParentGetter = () => null;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override string Parent { get { return ParentGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<bool> VisibleGetter = () => true;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool Visible { get { return VisibleGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<int> PositionXGetter = null;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<int> PositionYGetter = null;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override Position GetDistance()
        {
            if (PositionXGetter == null || PositionYGetter == null) return null;
            return new Position() { x = PositionXGetter(), y = PositionYGetter() };
        }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public string SeeSuperTalk;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
            if (SeeSuperTalk != null)
            {
                list.Add(new SimpleMenuItem("見る", () =>
                {
                    var talk = SuperTalk.CreateSuperTalkFromOwnerModuleId(this.OwnerModuleId);
                    talk.Play(SeeSuperTalk);
                    return true;
                }));
            }
            await Task.Delay(0);
            return true;
        }
    }

    /// <summary>
    /// 内部使用専用です。直接利用すべきではありません。
    /// </summary>
    public class SimplePerson : PersonWithPlace
    {
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public Func<bool> ForceOverrideGetter = () => false;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool ForceOverride { get { return ForceOverrideGetter(); } }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public string TalkSuperTalk;
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override async Task TalkAsync()
        {
            var talk = SuperTalk.CreateSuperTalkFromOwnerModuleId(this.OwnerModuleId);
            talk.Play(TalkSuperTalk);
            await Task.Delay(0);
        }
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        /// <param name="id">一意のIdです。</param>
        /// <param name="name">名前です</param>
        /// <param name="sex">性別です</param>
        /// <param name="placeID">場所です。</param>
        /// <param name="isAvailable">出現する場合にTrueを返します</param>
        /// <param name="talk">メニュー選択時のアクションです</param>
        public SimplePerson(string id, string name, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person, Task> talk) : base(id, name, sex, placeID, isAvailable, talk) { }
    }

    /// <summary>
    /// 内部使用専用です。直接利用すべきではありません。
    /// </summary>
    public class SimpleOverwriteCollection : Collection
    {
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public override bool ForceOverride { get { return true; } }
    }

    /// <summary>
    /// 内部使用専用です。直接利用すべきではありません。
    /// </summary>
    public class SimpleCollectionItem : CollectionItem
    {
        /// <summary>
        /// 内部使用専用です。直接利用すべきではありません。
        /// </summary>
        public SuperTalkSource SuperTalk;
    }
}
