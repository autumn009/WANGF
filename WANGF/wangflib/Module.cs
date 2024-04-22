using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ANGFLib
{
    /// <summary>
    /// 内部利用専用です。
    /// </summary>
    [Serializable]
    public class ReferModuleInfo : MarshalByRefObject
    {
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public string Id;
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public string FullPath;
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public string Name; // (名前が不詳の場合idがそのまま名前。名前はエラーメッセージ用)
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        private Version v;
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public Version MinVersion
        {
            get { return v; }
            set { v = value; }
        }
    }

    /// <summary>
    /// 取りやめるべきメニューの種類を指定します
    /// </summary>
    public enum MenuStopControls
    {
        /// <summary>
        /// 何もありません。
        /// </summary>
        None = 0,
        /// <summary>
        /// メニューからアイテムを使用しません。
        /// </summary>
        Item = 1,
        /// <summary>
        /// メニューから装備を使用しません。
        /// </summary>
        Equip = 2,
        /// <summary>
        /// メニューから休憩を使用しません。
        /// </summary>
        Rest = 4,
        /// <summary>
        /// メニューから家に帰りません。
        /// </summary>
        GoHome = 8,
        /// <summary>
        /// メニューから生活サイクルを変更しません
        /// </summary>
        ChangeCycle = 16,
        /// <summary>
        /// メニューからロードしません。
        /// </summary>
        Load = 32,
        /// <summary>
        /// メニューから、自動セーブフォルダからロードしません。
        /// </summary>
        AutoLoad = 64,
        /// <summary>
        /// メニューからセーブしません。
        /// </summary>
        Save = 128,
        /// <summary>
        /// Systemメニューを使いません。そこから選択されるすべてのメニューも使用しません。
        /// </summary>
        System = 256,
        /// <summary>
        /// すべての機能を指定したことを意味します。
        /// </summary>
        All = 0x1ff,
    };

    /// <summary>
    /// XMLの名前空間を提供します。
    /// </summary>
    public static class XmlNamespacesConstants
    {
        /// <summary>
        /// 標準的な名前空間です。この名前空間はシステム側で予約されているので、一般アプリは別の名前空間を使用すべきです。
        /// </summary>
        public const string StdXmlNamespace = "http://angf.autumn.org/std001";
    }

    /// <summary>
    /// シンプルな名前を管理します。
    /// </summary>
    public static class SimpleNameLookUp
    {
        /// <summary>
        /// Idと名前の関係をアプリケーションドメイン内で一意に管理します。
        /// このフィールドは一般アプリからは参照すべきではありません。
        /// </summary>
        public static Dictionary<string, string> IdToName = new Dictionary<string, string>();
    }
    /// <summary>
    /// シンプルな名前です。これを継承することで、場所や人物などのクラスが作られます。
    /// </summary>
    /// <typeparam name="T">それが扱うクラス名です。SimpleName%lt;T&gt;でなければなりません。</typeparam>
    [Serializable]
    public abstract class SimpleName<T> where T : SimpleName<T>
    {
        /// <summary>
        /// 人間か読める名前です。
        /// </summary>
        public abstract string HumanReadableName { get; }
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public abstract string Id { get; }
        /// <summary>
        /// そのIdのオブジェとが既に存在する場合に差し替えるならTrue。そうでないならFalseを指定します。常に適切な重複チェックが行われます。
        /// </summary>
        public virtual bool ForceOverride { get { return false; } }
        /// <summary>
        /// 所属するモジュールのIdです。
        /// </summary>
        public string OwnerModuleId;
        /// <summary>
        /// そのクラスのオブジェクトの一覧とIdとの対応関係を提供します。一般アプリからは変更すべきではありません。
        /// </summary>
        public static Dictionary<string, T> List = new Dictionary<string, T>();
        /// <summary>
        /// 追加とIdの重複チェックを行います。
        /// </summary>
        /// <param name="ownerModuleId">オーナーとなるモジュールのIdです。</param>
        /// <param name="item">登録すべきオブジェクトです。</param>
        public static void AddAndCheck(string ownerModuleId, T item)
        {
            if (string.IsNullOrWhiteSpace(item.Id)) throw new ApplicationException(item.HumanReadableName + "のIdはnullか空白のみにはできません。");
            if (string.IsNullOrWhiteSpace(item.HumanReadableName)) throw new ApplicationException(item.Id + "のHumanReadableNameはnullか空白のみにはできません。");
            item.OwnerModuleId = ownerModuleId;

#if false
            System.Diagnostics.Debug.Assert(!item.Id.Contains("DUMMY"));
#endif

            if (item.ForceOverride)
            {
                if (!List.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は存在しないので、オーバーライドできません。");
                List[item.Id] = (T)item;
                if (!SimpleNameLookUp.IdToName.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は全体で存在しないので、オーバーライドできません。");
                SimpleNameLookUp.IdToName[item.Id] = item.HumanReadableName;
            }
            else
            {
                if (List.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は重複しています。");
                List.Add(item.Id, (T)item);
                if (SimpleNameLookUp.IdToName.Keys.Contains(item.Id)) throw new ApplicationException("ID " + item.Id + "は全体で重複しています。");
                SimpleNameLookUp.IdToName.Add(item.Id, item.HumanReadableName);
            }
        }
        /// <summary>
        /// AddAndCheckメソッドをコレクションの全てに対して呼び出します。
        /// </summary>
        /// <param name="ownerModuleId">オーナーとなるモジュールのIdです。</param>
        /// <param name="items">対象とするコレクションです</param>
        public static void AddAndCheckAll(string ownerModuleId, T[] items)
        {
            foreach (var item in items) AddAndCheck(ownerModuleId, item);
        }
    }
    /// <summary>
    /// 位置を示します。単位系はアプリケーション定義です。
    /// </summary>
    public class Position
    {
        /// <summary>
        /// 座標のxを示します。
        /// </summary>
        public int x;
        /// <summary>
        /// 座標のyを示します。
        /// </summary>
        public int y;
        /// <summary>
        /// 2点間の距離を計算します。
        /// </summary>
        /// <param name="p">もう1つの座標を示します。</param>
        /// <returns>距離です。</returns>
        public double GetDistanceFrom(Position p) { return Math.Sqrt((p.x - x) * (p.x - x) + (p.y - y) * (p.y - y)); }
        /// <summary>
        /// 2点間の距離を計算します。
        /// </summary>
        /// <param name="p">もう1つの座標を示します。</param>
        /// <returns>距離です。</returns>
        public double GetDistanceFromSquared(Position p) { return (p.x - x) * (p.x - x) + (p.y - y) * (p.y - y); }
    }
    /// <summary>
    /// 場所を示します。
    /// </summary>
    public abstract class Place : SimpleName<Place>
    {
        /// <summary>
        /// 場所に入った場合に処理を行います。通常は、{0}に来ました。とシステムが通知します。
        /// </summary>
        public virtual async Task OnEnteringAsync()
        {
            OnEntering();
            await Task.Delay(0);
        }
        /// <summary>
        /// 場所を出た場合に処理を行います。通常は、{0}を去りました。とシステムが通知します。
        /// </summary>
        public virtual async Task OnLeaveingAsync()
        {
            OnLeaveing();
            await Task.Delay(0);
        }
        /// <summary>
        /// 場所に入った場合に処理を行います。通常は、{0}に来ました。とシステムが通知します。
        /// </summary>
        public virtual void OnEntering()
        {
            DefaultPersons.システム.Say("{0}に来ました。", HumanReadableName);
        }
        /// <summary>
        /// 場所を出た場合に処理を行います。通常は、{0}を去りました。とシステムが通知します。
        /// </summary>
        public virtual void OnLeaveing()
        {
            DefaultPersons.システム.Say("{0}を去りました。", HumanReadableName);
        }
        /// <summary>
        /// ConstructMenuメソッドでFalseを返した場合に呼び出されます。
        /// </summary>
        public virtual Task OnMenuAsync()/* DIABLE ASYNC WARN */
        {
            throw new ApplicationException(this.GetType().Name + "はOnMenuメニューを持ちません。");
        }
        /// <summary>
        /// 必要なメニューを追加します。
        /// </summary>
        /// <param name="list">追加すべきメニューのコレクションです。</param>
        /// <returns>Falseを返すとlist引数の値はキャンセルされてOnMenuを呼びます</returns>
        public virtual bool ConstructMenu(List<SimpleMenuItem> list)
        {
            return true;
        }
        /// <summary>
        /// 必要なメニューを追加します。
        /// </summary>
        /// <param name="list">追加すべきメニューのコレクションです。</param>
        /// <returns>Falseを返すとlist引数の値はキャンセルされてOnMenuを呼びます</returns>
        public virtual async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
            await Task.Delay(0);
            return true;
        }
        /// <summary>
        /// HumanReadableNameの値を返します。
        /// </summary>
        /// <returns>HumanReadableNameの値</returns>
        public override string ToString()
        {
            return HumanReadableName;
        }
        /// <summary>
        /// PlaceのIdまたはnullを返す (そのPlaceの一部扱いとなる)
        /// 移動メニューに出したくない時はnullを返す
        /// 移動メニューは動的に処理される
        /// </summary>
        public virtual string Parent
        {
            get { return null; }
        }
        /// <summary>
        /// ステータスを見せない場所はオーバーライドしてtrueを返す
        /// モジュールのうち1つでも見せないとしたら見せない
        /// </summary>
        public virtual bool IsStatusHide
        {
            get { return false; }
        }
        /// <summary>
        /// 異動先やマップで見える場所はオーバーライドしてTrueを返します。ただし、距離を持たないあるいは距離を持った祖先を持たない場合は見えません。
        /// </summary>
        public virtual bool Visible
        {
            get { return true; }
        }
        private Place getParent()
        {
            if (this.Parent == null) return null;
            Place p = this;
            for (; ; )
            {
                if (p.Parent == null) break;
                p = Place.List[p.Parent];
            }
            return p;
        }
        /// <summary>
        /// 距離(位置座票)を持っている場合はTrueを返します。
        /// </summary>
        public bool HasDistance
        {
            get
            {
                return GetDistance() != null;
            }
        }
        /// <summary>
        /// オーバーライドして位置座票を返します。
        /// </summary>
        /// <returns>位置座票。持っていない場合はnull</returns>
        public virtual Position GetDistance()
        {
            return null;
        }
        /// <summary>
        /// トップレベルの親が位置を持っていたらTrueになります。
        /// </summary>
        public bool HasParentDistance
        {
            get
            {
                Place p = getParent();
                if (p == null) return HasDistance;
                return p.HasParentDistance;
            }
        }
        /// <summary>
        /// トップレベルの親が持っている座標を返します。
        /// </summary>
        /// <returns></returns>
        public Position GetParentDistance()
        {
            Place p = getParent();
            if (p == null) return GetDistance();
            return p.GetDistance();
        }
        /// <summary>
        /// トップレベルの親がvisibeならtrueを返します。
        /// 祖先が無いなら自分自身がVisibleであるか否かを返します。
        /// </summary>
        /// <returns>トップレベルの親がvisibeならtrue</returns>
        public bool ParentVisible
        {
            get
            {
                if (Parent == null) return Visible;
                Place p = getParent();
                if (p == null) return false;
                return p.Visible;
            }
        }
        /// <summary>
        /// 移動できない場合に理由を返します。
        /// </summary>
        /// <param name="dst">予定された移動先です。</param>
        /// <returns>移動できない理由を説明する文字列です。移動できる場合はnullを返します。</returns>
        public virtual string FatalLeaveConfim(Place dst)
        {
            return null;
        }
        // 確認文があるときは文字列。無いときはnullを返す
        // 移動できないことはないが、問題がある場合
        /// <summary>
        /// 移動にあたって確認文を返します。
        /// </summary>
        /// <param name="dst">予定された移動先です。</param>
        /// <returns>確認する場合は文字列。確認を必要としない場合はnullです</returns>
        public virtual string LeaveConfim(Place dst)
        {
            //return "これから移動しようとしています。";
            return null;
        }
        /// <summary>
        /// トップレベルの親のIdです。
        /// </summary>
        public string ParentTopID
        {
            get
            {
                Place top = getParent();
                if (top != null) return top.Id;
                return Id;
            }
        }

        public virtual string [] GetLinkedPlaceIds()
        {
            return new string[0];
        }

        /// <summary>
        /// その場所が属するWorldのIDを示します。オーバーライドしない場合、デフォルトのワールドに属すると見なされます。
        /// </summary>
        public virtual string World { get { return Constants.DefaultWordId; } }

        /// <summary>
        /// 特定のWorldに属する全てのモジュールを返す
        /// </summary>
        /// <param name="worldId">WorldのID</param>
        /// <returns>指定Worldに属するPlace一覧</returns>
        public static Place[] GetAllPlacesInWorld(string worldId)
        {
            return SimpleName<Place>.List.Values.Where(c => c.World == worldId).ToArray();
        }
    }

    /// <summary>
    /// 画面上に表示するステータス情報です
    /// </summary>
    [Serializable]
    public abstract class MiniStatus : SimpleName<MiniStatus>
    {
        /// <summary>
        /// 見える場合はTrueを返します。
        /// </summary>
        /// <returns>Trueですが、拡張する場合はオーバーライドします</returns>
        public virtual bool IsVisible() { return true; }
        /// <summary>
        /// オーバーライドして表示する文字列を返します。
        /// </summary>
        /// <returns>表示される文字列です。</returns>
        public virtual string Text() { return ""; }
        /// <summary>
        /// 文字を表示する色を返します。
        /// </summary>
        /// <returns>白ですが、変更する場合はオーバーライドします。</returns>
        public virtual System.Drawing.Color ForeColor() { return System.Drawing.Color.White; }
        /// <summary>
        /// 表示順番を決める優先順位の数値を返します。数値が小さいものが先になります。オーバーライドしない場合0になります。
        /// </summary>
        public virtual int Priority { get { return 0; } }
    }
    /// <summary>
    /// 一般アプリからは使うべきではありません。
    /// </summary>
    /// <returns>一般アプリからは使うべきではありません。</returns>
    public delegate string GetVisibleTextInvoker();
    /// <summary>
    /// 常に固定された文字列を返すメソッドを使うMiniStatusです
    /// </summary>
    public class FixedMiniStatus : MiniStatus
    {
        private string id;
        private int priority;
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public override string Id
        {
            get { return id; }
        }
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return id; }
        }
        /// <summary>
        /// 優先順位です。より小さい値が前に来ます。
        /// </summary>
        public override int Priority
        {
            get
            {
                return priority;
            }
        }
        /// <summary>
        /// 表示される文字列を返します。
        /// </summary>
        /// <returns>表示される文字列です。</returns>
        public override string Text() { return getVisibleText(); }
        private GetVisibleTextInvoker getVisibleText;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="getVisibleText">表示する文字列を返します。</param>
        /// <param name="id">一意のIdです。</param>
        /// <param name="priority">優先順位です。値が小さい方が先になります。</param>
        public FixedMiniStatus(GetVisibleTextInvoker getVisibleText, string id, int priority)
        {
            this.getVisibleText = getVisibleText;
            this.priority = priority;
            this.id = id;
        }
    }
    /// <summary>
    /// 各装備部位の詳細です。
    /// </summary>
    [Serializable]
    public class EquipType : SimpleName<EquipType>
    {
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public override string Id
        {
            get { return Name; }
        }
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return Name; }
        }
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        public string Name;
        /// <summary>
        /// ステータス画面上に表示される名前です。
        /// </summary>
        public string StatusName;
        /// <summary>
        /// 装備メニューでアイテム名の前に表示される短い名前です。
        /// nullのままだと、HumanReadableNameの値が使われます。
        /// </summary>
        public string ShortName;
        /// <summary>
        /// その部位の詳細説明です。nullだと説明省略です。
        /// </summary>  
        public string Description;
        /// <summary>
        /// 優先順位です。値が小さい方が先に表示されます。
        /// </summary>
        public int Priority;
        /// <summary>
        /// 拡張表示の場合のみ表示される装備品
        /// false(デフォルト)だと常時表示
        /// </summary>
        public bool IsVisibleIfExpanded;
        /// <summary>
        /// ラムダ式で表示するかを判定する
        /// nullは常時表示を意味する
        /// </summary>
        public Func<bool> IsVisibleByProc;
    }
    /// <summary>
    /// カスタマイズされるメニューの項目です。
    /// </summary>
    public class MenuItem : SimpleName<MenuItem>
    {
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public override string Id
        {
            get { return Label; }
        }
        /// <summary>
        /// 人間が読める名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return Label; }
        }
        /// <summary>
        /// メニュー上の名前です。
        /// </summary>
        public string Label;
        /// <summary>
        /// メニューが選ばれたときに実行されます。
        /// </summary>
        // 戻り値がvoidだと同期と非同期を間違えても警告されないので、使用を停止した
        // MethodAsyncを使うべき
        //public MyMethodInvoker Method;
        /// <summary>
        /// メニューが選ばれたときに実行されます。
        /// </summary>
        public Func<object, Task> MethodAsync;/* DIABLE ASYNC WARN */
        /// <summary>
        /// メニューの種類です。トップレベルか情報メニューの下かを指定します。
        /// </summary>
        public MyMenuType MenuType;
        /// <summary>
        /// メニューが禁止ならtrueになります。
        /// </summary>
        public Func<bool> IsMenuEnabled = () => true;
    }
    /// <summary>
    /// メニューの種類です。トップレベルか情報メニューの下かを指定します。
    /// </summary>
    public enum MyMenuType
    {
        /// <summary>
        /// トップレベル
        /// </summary>
        Top,
        /// <summary>
        /// 情報メニュー(SYSTEMメニュー)
        /// </summary>
        Info,
    }
    /// <summary>
    /// 一般アプリからは使用すべきではありません。
    /// </summary>
    /// <param name="mainForm">一般アプリからは使用すべきではありません。</param>
    public delegate void MyMethodInvoker(object mainForm);
    /// <summary>
    /// 日付時刻が指定の条件を満たすかを判定します。
    /// </summary>
    /// <param name="dt">日付時刻です</param>
    /// <returns>条件を満たしていればTrueです。</returns>
    public delegate bool IsDateTimeInvoker(DateTime dt);
    /// <summary>
    /// ある日付の色を取得します。
    /// </summary>
    /// <param name="dt">日付時刻です。</param>
    /// <param name="isRedDay">赤い日ならTrueです。</param>
    /// <param name="isBlueDay">青い日ならTrueです。</param>
    /// <returns>当該する色を返します。</returns>
    public delegate System.Drawing.Color GetDateColorInvoker(DateTime dt, bool isRedDay, bool isBlueDay);
    /// <summary>
    /// 背景色を取得します。
    /// </summary>
    /// <returns>当該する色を返します。</returns>
    public delegate System.Drawing.Color GetBackColorInvoker();
    /// <summary>
    /// 主人公の名前を取得します。フルネームではなく短めの名前を返します。
    /// </summary>
    /// <returns>主人公の名前です。</returns>
    public delegate string GetMyPersonNameInvoker();
    /// <summary>
    /// 長さをアプリ表記文字列に置き換えます。
    /// </summary>
    /// <param name="length">長さです。単位は応用ソフト依存です。</param>
    /// <returns>文字列です</returns>
    public delegate string MapLengthCoockerInvoker(int length);
    /// <summary>
    /// 価格をアプリ表記文字列に置き換えます。
    /// </summary>
    /// <param name="price">価格です。単位は応用ソフト依存です。</param>
    /// <returns>文字列です</returns>
    public delegate string PriceCoockerInvoker(int price);

    /// <summary>
    /// サイト情報を記述します。ヘルプメニュー構築用です
    /// </summary>
    public abstract class SiteInfo
    {
        /// <summary>
        /// サイトの名前を人間可能の文字列で返します
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// サイトのURIを返します
        /// </summary>
        public abstract string Uri { get; }
    }
    /// <summary>
    /// モジュールの基底クラスです。
    /// </summary>
    public abstract class Module
    {
        // 識別用IDを提供する。1クラスに1種類で良い。1つの.NETモジュール内に複数のクラスがありうるので、あえてANGFモジュール自身とは別の番号を提供する
        /// <summary>
        /// 一意の識別子を提供します。
        /// </summary>
        public virtual string Id { get { throw new ApplicationException("Idが実装されていません。"); } }
        /// <summary>
        /// オーバーライドして場所一覧を提供します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Place[] GetPlaces() { return new Place[0]; }
        /// <summary>
        /// 就寝時に自動で帰る場所のIDを指定します。動的に判定されるので、いちいちユーザーにしていさせる構成もありです。
        /// </summary>
        /// <returns>場所のIdです。nullなら関知しません (移動しない)</returns>
        public virtual async Task<string> GetDefaultPlaceAsync() { await Task.Delay(0); return null; }
        /// <summary>
        /// ゲームを開始する場所を示します。
        /// </summary>
        /// <returns>場所です。nullか空文字列なら従属するモジュールであり開始する固有の場所はない</returns>
        public virtual string GetStartPlace() { return null; }
        /// <summary>
        /// 人一覧を提供します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Person[] GetPersons() { return new Person[0]; }
        /// <summary>
        /// アイテム一覧を提供します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Item[] GetItems() { return new Item[0]; }
        /// <summary>
        /// スケジュールされた手順一覧を提供します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Schedule[] GetSchedules() { return new Schedule[0]; }
        /// <summary>
        /// コレクション一覧を提供します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Collection[] GetCollections() { return new Collection[0]; }
        /// <summary>
        /// 画面上のステータスを追加します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual MiniStatus[] GetStatuses() { return new MiniStatus[0]; }
        /// <summary>
        /// 装備部位の一覧です。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual EquipType[] GetEquipTypes()
        {
            return new EquipType[0]
                /*{
                    new EquipType() {  Name="上服", StatusName="Upper", Priority=100, },
                    new EquipType() {  Name="下服", StatusName="Lower", Priority=200,},
                    new EquipType() {  Name="アクセサリ", StatusName="Access.", Priority=300,},
                }*/
               ;
        }
        /// <summary>
        /// 装備の未来シミュレーションを行います。不可ならfalseを返します。msgはパネルに出るメッセージです。
        /// </summary>
        public virtual FutureEquipSimulationInvoker FutureEquipSimulation { get { return null; } }
        /// <summary>
        /// ファイルの拡張子があれば提供します。無ければそのまま(nullを返す)
        /// </summary>
        public virtual string FileExtention { get { return null; } }
        /// <summary>
        /// ショップ一覧を返します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual Shop[] GetShops() { return new Shop[0]; }
        /// <summary>
        /// ショップとアイテムの関係一覧を返します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual ShopAndItemReleation[] GetShopAndItemReleations() { return new ShopAndItemReleation[0]; }
        /// <summary>
        /// 移動時間を解決します。解決しない場合はそのまま(nullを返す)
        /// </summary>
        public virtual HowToMoveInvoker HowToMove { get { return null; } }
        /// <summary>
        /// サブ移動時間を解決します。解決しない場合はそのまま(nullを返す)
        /// </summary>
        public virtual HowToMoveInvoker HowToSubMove { get { return null; } }
        /// <summary>
        /// 「私」用の名前を返します。(あれば)
        /// </summary>
        public virtual GetMyPersonNameInvoker GetMyPersonName { get { return null; } }
        /// <summary>
        /// QuickTalkのPersonを解決します (解決しない場合nullを返す)
        /// </summary>
        /// <param name="name">短縮名です</param>
        /// <returns>PersonのIdです。</returns>
        public virtual string GetQuickTalkPerson(string name) { return null; }
        /// <summary>
        /// QuickTalkのマクロを解決します (解決しない場合nullを返す)
        /// </summary>
        /// <param name="name">マクロ名</param>
        /// <returns>置換すべき文字列</returns>
        public virtual string GetQuickTalkMacro(string name) { return null; }
        /// <summary>
        /// 必要なメニューを追加します。falseを返すとキャンセルされてOnMenuを呼びます
        /// </summary>
        /// <param name="list">追加すべきメニュー</param>
        /// <param name="place">場所</param>
        /// <returns>FalseならキャンセルしてOnMenuを呼ぶ</returns>
        public virtual bool ConstructMenu(List<SimpleMenuItem> list, Place place) { return true; }
        /// <summary>
        /// 必要なSYSTEMメニューを追加します。
        /// </summary>
        /// <param name="list">メニューです。</param>
        /// <param name="place">場所です。</param>
        public virtual void ConstructSystemMenu(List<SimpleMenuItem> list, Place place) { return; }
        /// <summary>
        /// 見せないメニューのビットマップを返します。結果は全モジュールのORを取ります。
        /// </summary>
        /// <returns>見せないメニューのビットマップ</returns>
        public virtual MenuStopControls StopMenus() { return MenuStopControls.None; }
        /// <summary>
        /// 拡張するメニューを返します。
        /// </summary>
        /// <returns>空の一覧です。</returns>
        public virtual MenuItem[] GetExtendMenu() { return new MenuItem[0]; }
        /// <summary>
        /// 各種レポート用です。
        /// </summary>
        /// <param name="writer">書き込み先です。</param>
        /// <param name="forDebug">Trueならデバッグ環境です。</param>
        public virtual void WriteReport(System.IO.TextWriter writer, bool forDebug) { return; }

        /// <summary>
        /// 日付に対応するモード。日曜祭日用・祭日を追加する場合はこれをオーバーライドする
        /// </summary>
        public virtual IsDateTimeInvoker IsRedDay { get { return null; } }
        /// <summary>
        /// 日付に対応するモード。土曜日を追加する場合はこれをオーバーライドする
        /// </summary>
        public virtual IsDateTimeInvoker IsBlueDay { get { return null; } }
        /// <summary>
        /// 日付に対応するカラーを返します
        /// </summary>
        public virtual GetDateColorInvoker GetDateColor { get { return null; } }
        /// <summary>
        /// 背景色を得ます
        /// </summary>
        public virtual GetBackColorInvoker GetBackColor { get { return null; } }
        /// <summary>
        /// 長さを表示用に加工します。
        /// </summary>
        public virtual MapLengthCoockerInvoker MapLengthCoocker { get { return null; } }
        /// <summary>
        /// 金額を表示用に加工します。
        /// </summary>
        public virtual PriceCoockerInvoker PriceCoocker { get { return null; } }
        /// <summary>
        /// 装備時のカスタムバリデータを取得します
        /// nullを返すと常に成功です。
        /// </summary>
        public virtual Func<EquipSet, string> GetEquipCustomValidator(string personId) { return null; }
        /// <summary>
        /// 実行前に呼ばれます
        /// </summary>
        /// <returns>trueを返します。</returns>
        public virtual async Task<bool> OnInitAsync() { await Task.Delay(0);  return true; }
        /// <summary>
        /// 実行前に呼ばれます
        /// </summary>
        /// <returns>trueを返します。</returns>
        public virtual bool OnStart() { return true; }
        /// <summary>
        /// 実行前に呼ばれます
        /// </summary>
        /// <returns>trueを返します。</returns>
        public virtual async Task<bool> OnStartAsync() { await Task.Delay(0); return true; }
        /// <summary>
        /// 実行後に呼ばれます、
        /// </summary>
        /// <returns>trueを返します。</returns>
        public virtual bool OnEnd() { return true; }
        /// <summary>
        /// 新しいゲームを開始する場合に呼ばれます。
        /// </summary>
        /// <returns>trueを返します。</returns>
        public virtual bool OnNewGame() { return true; }
        /// <summary>
        /// ロードの開始時に呼ばれます。
        /// </summary>
        /// <param name="src">ドキュメントの入ったDOMです</param>
        /// <returns>trueを返します。</returns>
        public virtual bool OnLoadStart(XmlDocument src) { return true; }
        /// <summary>
        /// ロードの終了時に呼ばれます。
        /// </summary>
        /// <param name="src">ドキュメントの入ったDOMです</param>
        /// <returns>trueを返します。</returns>
        public virtual bool OnLoadEnd(XmlDocument src) { return true; }
        /// <summary>
        /// セーブの開始時に呼ばれます。
        /// </summary>
        /// <param name="writer">書き込み先です</param>
        /// <returns>trueを返します。</returns>
        public virtual bool OnSaveStart(XmlWriter writer) { return true; }
        /// <summary>
        /// セーブの終了に呼ばれます。
        /// </summary>
        /// <param name="writer">書き込み先です</param>
        /// <returns>trueを返します。</returns>
        public virtual bool OnSaveEnd(XmlWriter writer) { return true; }
        /// <summary>
        /// 一般コマンド入力待ちの前に呼ばれます
        /// AbortRequest = false;でメインループ先頭に即座に戻ります
        /// </summary>
        public virtual Task OnBeforeCommandAsync() => Task.Delay(0);
        /// <summary>
        /// 一般コマンド入力待ちの後に呼ばれます
        /// AbortRequest = false;でメインループ先頭に即座に戻ります
        /// </summary>
        public virtual Task OnAfterCommandAsync() => Task.Delay(0);
        /// <summary>
        /// GoTimeメソッド実行時に呼ばれます。
        /// </summary>
        /// <param name="from">開始時刻です</param>
        /// <param name="to">終了時刻です</param>
        public virtual void OnGoTime(DateTime from, DateTime to) { }
        /// <summary>
        /// GoTimeメソッド実行の後で呼ばれます。
        /// </summary>
        /// <param name="from">開始時刻です</param>
        /// <param name="to">終了時刻です</param>
        public virtual void OnAfterGoTime(DateTime from, DateTime to) { }
        /// <summary>
        /// 就寝前に呼ばれます。
        /// </summary>
        /// <returns>falseを呼び出すと処理を中断します。エンディングに行けます。</returns>
        public virtual async Task<bool> OnBeforeSleepAsync() { await Task.Delay(0); return true; }
        /// <summary>
        /// 就寝中に呼ばれます。
        /// </summary>
        /// <returns>falseを呼び出すと処理を中断します。エンディングに行けます。</returns>
        public virtual async Task<bool> OnSleepingAsync() { await Task.Delay(0); return true; }
        /// <summary>
        /// 就寝後に呼ばれます。
        /// </summary>
        /// <returns>falseを呼び出すと処理を中断します。エンディングに行けます。</returns>
        public virtual async Task<bool> OnAfterSleepAsync() { await Task.Delay(0); return true; }
        /// <summary>
        /// 1日の開始時に呼ばれます。全てのOnAfterSleepよりも後に呼ばれることが保証されます。
        /// </summary>
        /// <returns>falseを呼び出すと処理を中断します。エンディングに行けます。</returns>
        public virtual async Task<bool> OnStartTodayAsync() { await Task.Delay(0); return true; }
        /// <summary>
        /// 移動のあとで呼ばれます(WarpToAsync/GotoAsync)
        /// </summary>
        /// <returns>falseを返すと処理を中断します。</returns>
        public virtual async Task<bool> OnAfterMoveAsync(string fromId, string toId) { await Task.Delay(0); return true; }
        /// <summary>
        /// ジャーナリング再生時、モジュール独自のデータに遭遇したときに呼ばれます
        /// </summary>
        /// <returns>詳細はモジュール定義です。</returns>
        public virtual Task<bool> OnExtraJurnalPlaybackAsync(string[] paramArray)/* DIABLE ASYNC WARN */
        {
            throw new ApplicationException("OnExtraJurnalPlaybackはこのモジュールではサポートされていません。"); 
        }
        /// <summary>
        /// オフィシャルサイトのURLを返します。HELPメニュー作成時に動的に呼ばれるので、動的に構築できます
        /// </summary>
        /// <returns>サイト情報の配列</returns>
        public virtual SiteInfo[] GetOfficialSiteUrl() { return new SiteInfo[0]; }
    }
}
