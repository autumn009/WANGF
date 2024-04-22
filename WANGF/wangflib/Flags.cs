using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ANGFLib
{
    /// <summary>
    /// 基本的なフラグを提供します。
    /// </summary>
    [AutoFlags("{668EDC40-C621-44a1-9042-100F79D6DFDA}")]
    public static class Flags
    {
        // 日付時刻は外部からの直接アクセスを拒絶する
        // 名前による間接アクセスは通る
        // Now経由で読み書きさせる
        // 初期状態でnowと個別の値は同期していないが
        // 使用前に必ず初期化されるという前提でよしとする
        [FlagName("年")]
        private static int 年;
        [FlagName("月")]
        private static int 月;
        [FlagName("日")]
        private static int 日;
        [FlagName("時")]
        private static int 時;
        [FlagName("分")]
        private static int 分;

        /// <summary>
        /// ゲーム内現在の日付時刻
        /// </summary>
        public static DateTime Now
        {
			get
			{
                try
				{
                    if (年 == 0 || 月 == 0 || 日 == 0) return DateTime.MinValue;
					return new DateTime(年, 月, 日, 時, 分, 0);
				}
				catch (Exception)
				{
					return DateTime.MinValue;
				}
			}
            set
            {
                年 = value.Year;
                月 = value.Month;
                日 = value.Day;
                時 = value.Hour;
                分 = value.Minute;
            }
        }

        /// <summary>
        /// ゲーム内現在の日付(時刻情報無し)
        /// </summary>
        public static DateTime Today
        {
            get
            {
                try
                {
                    return new DateTime(年, 月, 日);
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// 所持金です。
        /// </summary>
        [FlagName("所持金")]
        private static int _所持金;
        public static int 所持金
        {
            get { return _所持金; }
            set
            {
                const int 所持金上限 = 1000000000;
                // 負数もチェックするのは、オーバーフローがラップアラウンドする可能性があるため
                // 本当は怖いけどね
                if (value < 0 || value > 所持金上限)
                    _所持金 = 所持金上限;
                else
                    _所持金 = value;
            }
        }
        /// <summary>
        /// 生活サイクルの起点時間です。一般的には起床時刻です。
        /// </summary>
        [FlagName("生活サイクル起点時間")]
        public static int 生活サイクル起点時間;
        /// <summary>
        /// 起床した回数です。必ずしも現在日付と開始日付の差とは一致しません
        /// </summary>
		[FlagName("起床回数")]
		public static int 起床回数;
        /// <summary>
        /// 現在位置のIdを指定します。
        /// </summary>
        [FlagName("CurrentPlaceId")]
        public static string CurrentPlaceId;

        [FlagName("currentWorldId")]
        private static string currentWorldId;
        /// <summary>
        /// 現在位置のワールドを指定します。
        /// </summary>
        public static string CurrentWorldId
        {
            get { return string.IsNullOrWhiteSpace(currentWorldId) ? Constants.DefaultWordId : currentWorldId; }
            set { currentWorldId = value; }
        }
        /// <summary>
        /// 現在所有するスター数です (スターの扱いはシナリオ依存です)
        /// </summary>
		[FlagName("NumberOfStars")]
        public static int NumberOfStars;

        // 装備マップの現状を保持
        [FlagPrefix("Equip")]
        public static FlagCollection<string> equip = new FlagCollection<string>();
        /// <summary>
        /// 生の装備情報を読出します。
        /// </summary>
        /// <param name="index">装備部位です。</param>
        /// <returns>アイテムのIdです。</returns>
        public static string GetRawEquip(string personId, int index)
        {
            var p = Person.List[personId] as IPartyMember;
            System.Diagnostics.Debug.Assert(p != null);
            return p.GetRawEquip(index);
        }
        /// <summary>
        /// 生の装備情報を設定します。同時装備の情報は無視します
        /// </summary>
        /// <param name="index">装備部位です。</param>
        /// <param name="id">アイテムのIdです。</param>
        public static void SetRawEquip(string personId, int index, string id)
        {
            var p = Person.List[personId] as IPartyMember;
            System.Diagnostics.Debug.Assert(p != null);
            p.SetRawEquip(index, id);
            if (State.EquipChangeNotify != null) State.EquipChangeNotify();
        }
        /// <summary>
        /// 同時装備を意識した装備情報のラッパです。先頭の1名専用
        /// </summary>
        public static EquipWrapper Equip = new EquipWrapper();

        // パーティーメンバー情報 (個々のメンバーのPerson IDのコレクション
        [FlagPrefix("PartyIDs")]
        public static FlagCollection<string> PartyIDs = new FlagCollection<string>();

        /// <summary>
        /// そのIdのアイテムを何個所有しているかのコレクションを提供します。
        /// </summary>
        [FlagPrefix("item")]
        public static readonly FlagCollection<int> アイテム所有数フラグ群 = new FlagCollection<int>();
        /// <summary>
        /// そのIdのアイテムを何回使ったのかのコレクションを提供します。
        /// </summary>
        [FlagPrefix("used")]
        public static readonly FlagCollection<int> アイテム使用回数フラグ群 = new FlagCollection<int>();

        /// <summary>
        /// コレクションに関する情報のコレクションを提供します。
        /// </summary>
        [FlagPrefix("Col")]
        public static FlagCollection<int> Collections = new FlagCollection<int>();

        /// <summary>
        /// スケジュールの可視性に関する情報のコレクションを提供します。
        /// </summary>
        [FlagPrefix("Sch")]
        public static FlagCollection<bool> ScheduleVisilbles = new FlagCollection<bool>();

        /// <summary>
        /// 装備メニューは拡張されているか?
        /// </summary>
        [FlagName("IsExpandEquip")]
        public static bool IsExpandEquip;
    }

    /// <summary>
    /// 自動的に探索されてロード、セーブされるフラグのクラスに付加します。
    /// </summary>
	public class AutoFlagsAttribute : Attribute
	{
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
		public readonly string Id;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public AutoFlagsAttribute(string id)
		{
			this.Id = id;
		}
	}

    /// <summary>
    /// フラグであるフィールドに付加します。
    /// </summary>
	public class FlagNameAttribute : Attribute
	{
        /// <summary>
        /// ファイルに記録されるフラグの名前です。
        /// </summary>
		public readonly string Name;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">ファイルに記録されるフラグの名前です。</param>
		public FlagNameAttribute(string name)
		{
			Name = name;
		}
	}

    /// <summary>
    /// フラグのコレクションであるフィールドに付加します。
    /// </summary>
    public class FlagPrefixAttribute : Attribute
	{
        /// <summary>
        /// セーブされるファイルに記録されるプレフィクスの文字列です。
        /// </summary>
		public readonly string Name;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">セーブされるファイルに記録されるプレフィクスの文字列です。</param>
		public FlagPrefixAttribute(string name)
		{
			Name = name;
		}
	}

    /// <summary>
    /// 動的フラグのコレクションであるフィールドに付加します。
    /// DynamicObjectを継承した型のオブジェクトにのみ指定できます。
    /// </summary>
    public class DynamicObjectFlagAttribute : Attribute
    {
        /// <summary>
        /// セーブされるファイルに記録されるプレフィクスの文字列です。
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">セーブされるファイルに記録されるプレフィクスの文字列です。</param>
        public DynamicObjectFlagAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 任意の型のコレクションです。
    /// </summary>
    /// <typeparam name="T">使用する型を指定します。bool, int, stringの3種類のみ有効です。</typeparam>
    public class FlagCollection<T>
    {
        private Dictionary<string, T> dic = new Dictionary<string, T>();
        /// <summary>
        /// フラグの内容を読み書きします。
        /// </summary>
        /// <param name="index">キーです</param>
        /// <returns>値です</returns>
        public T this[string index]
        {
            get
            {
                T val;
                if (dic.TryGetValue(index, out val)) return val;
                return default(T);
            }
            set { dic[index] = value; }
        }
        /// <summary>
        /// フラグの内容を読み出せるか挑戦します。
        /// </summary>
        /// <param name="key">キーです</param>
        /// <param name="value">値す。</param>
        /// <returns>読み出しに成功した場合はTrueを返します。</returns>
        public bool TryGetValue(string key, out T value)
        {
            return dic.TryGetValue(key, out value);
        }
        /// <summary>
        /// 内容をクリアします。
        /// </summary>
        public void Clear()
        {
            dic.Clear();
        }
        /// <summary>
        /// 指定したキーを持つ項目を削除します
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return dic.Remove(key);
        }
        /// <summary>
        /// 値のコレクションを返します。
        /// </summary>
        public Dictionary<string, T>.ValueCollection Values { get { return dic.Values; } }
        /// <summary>
        /// キーのコレクションを返します。
        /// </summary>
		public Dictionary<string, T>.KeyCollection Keys { get { return dic.Keys; } }
	}
}

