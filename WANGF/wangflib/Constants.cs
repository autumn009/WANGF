#define TestVersion

using System;
using System.Collections.Generic;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 各種の定数を提供します。
    /// </summary>
	public class Constants
    {
        /// <summary>
        /// 自分自身の名前です
        /// </summary>
		public const string MyName = "WANGF";

        /// <summary>
        /// フォーム上のタイトル表示名です。
        /// </summary>
		public const string FormName = "WANGF";	// フォームの表示名

        /// <summary>
        /// 公式サイトのURLです。
        /// </summary>
		public const string 公式サイトURL = "http://www.piedey.co.jp/";

        /// <summary>
        /// セーブするファイルのヘッダーの文字列です。
        /// </summary>
		public static readonly byte[] FileMagicHeader = { (byte)'A', (byte)'N', (byte)'G', (byte)'F' };
        /// <summary>
        /// システムファイルのヘッダーの文字列です。
        /// </summary>
		public static readonly byte[] SystemFileMagicHeader = { (byte)'A', (byte)'S', (byte)'Y', (byte)'1' };

        /// <summary>
        /// レジストリのHKCUのルートです
        /// </summary>
        public static readonly string RegistoryRoot = @"SOFTWARE\WANGFD\PATH";

        /// <summary>
        /// レジストリのHKCUのその他用ルートです
        /// </summary>
        public static readonly string RegistoryRootMisc = @"SOFTWARE\WANGFD\MISC";

        /// <summary>
        /// 自分自身の名前を返します。
        /// </summary>
		public static string MyMainName
        {
            get { return MyName; }
        }

        /// <summary>
        /// 共通に使用される日付時刻のフォーマットです。
        /// </summary>
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        /// <summary>
        /// 共通に使用される日付のフォーマットです。
        /// </summary>
        public const string DateFormat = "yyyyMMdd";

        /// <summary>
        /// 出力に使用する日付時刻フォーマットです。秒は含みません。
        /// </summary>
        public const string DateTimeHumanReadbleFormat = "yyyy年MM月dd日HH時mm分";

        /// <summary>
        /// 出力に使用する時刻フォーマットです。秒は含みません。
        /// </summary>
        public const string TimeHumanReadbleFormat = "HH時mm分";

        /// <summary>
        /// 例外的に常にSkipする文字列の先頭です。
        /// </summary>
		public const string スキップ例外Prefix = "***プレイバック所要時間 ";

        /// <summary>
        /// ホワイトスペース扱いする文字です
        /// </summary>
        public static readonly char[] WhiteSpaces = { ' ', '\t', '　' };
        /// <summary>
        /// WebSeriveのSystemFileへ記録する際のIDです
        /// </summary>
        public const string WebServiceUrlId = "{379d4139-3f7b-499e-b602-a4250146d923}";
        /// <summary>
        /// デフォルトのWebServiceのUrlです
        /// </summary>
        public const string DefaultWebServiceUrlId = "http://autumn.cloudapp.net/ANGF/FileWS.asmx";
        /// <summary>
        /// システムのPlaceIDです。
        /// </summary>
        public const string SystemPlaceID = "{C76B5966-3CE5-41eb-BD21-BBAE1A689EDF}";
        /// <summary>
        /// エンディングをコレクションするIDです。
        /// </summary>
        public const string EndingCollectionID = "{C3507068-210F-42cf-AAF3-852D80663C7C}";
        /// <summary>
        /// デフォルトのWorldのIDです。WorldのIDがnullの場合に想定されるWorldのIDです。
        /// </summary>
        public const string DefaultWordId = "{34c595b9-c11b-4e0f-a74c-e3080424e956}";
        /// <summary>
        /// Webプレイヤーで強制するロケール
        /// </summary>
        public const string LocalInWeb = "ja-JP";
        /// <summary>
        /// 円記号
        /// </summary>
        public const char YenSign = '\x00a5'; // ¥(x00a5) not \(x005c)
        /// <summary>
        /// WebプレイヤーでテンポラフォルダにDLLを書き込む際に付加するヘッダー
        /// </summary>
        public const string DllIdentityPrefix = "Angf_DllIdentityPrefix__";
        /// <summary>
        /// ローカルなStar数を示すSystemFile上のフラグ値です
        /// </summary>
        public const string LocalStarsId = "LocalStars";
        /// <summary>
        /// Webプレイヤーにユーザー登録したときに付与されるStar数を示す数値です
        /// </summary>
#if STAR_TEST_VERSION
        public const int WebInitialStars = 100;
#else
        public const int WebInitialStars = 0;
#endif
        /// <summary>
        /// Web版にのみ無料で1日あたり付与される就寝回数。Flags.EnabledSleepCountの規定値となる
        /// </summary>
        public const int FreeSleepCount = 3;
        /// <summary>
        /// ジャーナリング機能で書き込むストレージ名
        /// </summary>
        public const string OnlyANJName = "only.anj";
        /// <summary>
        /// 直接ジャーナリングのヘッダー文字列
        /// </summary>
        public const string JournalingDirectHeader = @"\\\\DIRECT\\\\";
        /// <summary>
        /// リンク移動で消費する分数のデフォルト値
        /// </summary>
        public const int DefaultLinkMoveMin = 10;
    }
}
