using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using ANGFLib;

using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text.Encodings.Web;
using System.Runtime.InteropServices;

namespace ANGFLib
{
    /// <summary>
    /// シンプルなメニューのアクションです。
    /// </summary>
    /// <returns>bool値を返しますが使い方は定義されていません。</returns>
    public delegate bool SimpleMenuAction();

    /// <summary>
    /// 標準的なコマンド名の文字列を提供します。
    /// </summary>
    public class CommonCommandNames
    {
        /// <summary>
        /// 「装備」の文字列を提供します。
        /// </summary>
        public const string 装備 = "装備";
        /// <summary>
        /// 「SYSTEM」の文字列を提供します。
        /// </summary>
        public const string SYSTEM = "SYSTEM";
    }

    /// <summary>
    /// メニューの色をどうするか
    /// Default: システムに任せる　(デフォルト)
    /// Normal: 通常色　(強調しない)
    /// Hot: 強調
    /// </summary>
    public enum SimpleMenuType
    {
        Default = 0,
        Normal = 1,
        Hot = 2,
    }

    /// <summary>
    /// シンプルなメニューの項目です。
    /// </summary>
    [Serializable]
    public class SimpleMenuItem : MarshalByRefObject
    {
        /// <summary>
        /// シンプルなメニューの名前です。
        /// </summary>
        public string Name = null;
        /// <summary>
        /// メニューが選択された場合のアクションを指定します。nullの場合は何もしません。
        /// </summary>
        public SimpleMenuAction SimpleMenuAction = null;
        /// メニューが選択された場合のアクションを指定します。nullの場合は何もしません。
        /// </summary>
        public Func<Task<bool>> SimpleMenuActionAsync = null;/* DIABLE ASYNC WARN */
        /// <summary>
        /// ボタンの下に出る説明文。オプション。説明文があるとリッチ形式になる。
        /// </summary>
        public string Explanation = null;
        /// <summary>
        /// ボタンのマウスオーバーで出る説明文。オプション。(タッチ操作だと出せないこと注意)
        /// </summary>
        public string MouseOverText = null;
        /// <summary>
        /// ユーザー定義のオブジェクトを指定します。使い道は決まっていません。
        /// </summary>
        public object UserParam = null;
        /// <summary>
        /// メニューのタイプを設定します。
        /// </summary>
        public SimpleMenuType MenuType = SimpleMenuType.Default;
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">人間可読の名前です。</param>
        /// <param name="simlpleMenuAction">選択時に実行される手順です。nullならありません。</param>
        public SimpleMenuItem(string name, SimpleMenuAction simlpleMenuAction)
        {
            Name = name;
            SimpleMenuAction = simlpleMenuAction;
        }
        public SimpleMenuItem(string name, Func<Task<bool>> simlpleMenuActionAsync)/* DIABLE ASYNC WARN */
        {
            Name = name;
            SimpleMenuActionAsync = simlpleMenuActionAsync;/* DIABLE ASYNC WARN */
        }
        /// <summary>
        /// コンストラクタです。
        /// 名前はuserParam.ToString()となります。
        /// </summary>
        /// <param name="userParam">ユーザー定義の任意のオブジェクトです。</param>
        public SimpleMenuItem(object userParam)
        {
            Name = userParam.ToString();
            UserParam = userParam;
        }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">人間可読の名前です。</param>
        /// <param name="simlpleMenuAction">選択時に実行される手順です。nullならありません。</param>
        /// <param name="userParam">ユーザー定義の任意のオブジェクトです。</param>
        public SimpleMenuItem(string name, SimpleMenuAction simlpleMenuAction, object userParam)
        {
            Name = name;
            SimpleMenuAction = simlpleMenuAction;
            UserParam = userParam;
        }
    }

    /// <summary>
    /// アイテムの価格を取得するデリゲート型です。
    /// </summary>
    /// <param name="item">対象とするアイテムです。</param>
    /// <param name="usedCount">アイテムの使用回数です。</param>
    /// <returns>価格です。</returns>
    public delegate int GetPriceInvoker(Item item, int usedCount);

    /// <summary>
    /// 引数を持たず、返却値のないデリゲート型です。
    /// </summary>
    public delegate void VoidMethodInvoker();

    /// <summary>
    /// 引数を持たず、返却値のないデリゲート型だけを引数に持つデリゲート型です。
    /// </summary>
    /// <param name="x">引数を持たず、返却値のないデリゲート型ですの値です。</param>
    public delegate void VoidMethodInvokerInvoker(VoidMethodInvoker x);

    /// <summary>
    /// UIで行うべきアクションのセットです
    /// </summary>
    public class UIActionSet
    {
        /// <summary>
        /// 画面上の装備情報を更新します。
        /// </summary>
        public Func<Dummy> ResetGameStatus = () => default;
        /// <summary>
        /// テキストの表示をクリアします。
        /// 主にロード/セーブ後に呼び出します。
        /// </summary>
        public Func<Dummy> ResetDisplay = () => default;
        /// <summary>
        /// 1行のメッセージを出力します。
        /// </summary>
        public Func<Person, string, Dummy> messageOutputMethod = (Person talker, string message) => default;
        /// <summary>
        /// シンプルなメニューを提供します。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, SimpleMenuItem[], SimpleMenuItem, Task<int>> simpleMenuMethodAsync = async (string prompt, SimpleMenuItem[] items, SimpleMenuItem systemMenu) => { await Task.Delay(0); return -1; };
        /// <summary>
        /// セーブするファイル名ユーザーから取得します。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, Task<string>> saveFileNameAsync = async (string prompt) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// ロードするファイル名をユーザーから取得します。一般のアプリからは呼び出しません。
        /// </summary>
        /// <param name="prompt">説明です</param>
        /// <returns>ファイルの名前です。名前の意味は実装依存です</returns>
        public async Task<string> loadFileNameAsync(string prompt)
        {
            return await loadFileNameWithExtentionAsync
(prompt, General.FileExtention);
        }
        /// <summary>
        /// ロードするファイル名をユーザーから取得します。拡張子を任意に指定できます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, string, Task<string>> loadFileNameWithExtentionAsync = async (string prompt, string extention) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// 自動セーブされるフォルダとデフォルトとしてロードするファイル名をユーザーから取得します。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, Task<string>> loadFileNameFromAutoSaveAsync = async (string prompt) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// 1つの文字列をユーザーより入力します。
        /// </summary>
        public Func<string, string, Task<string>> enterPlayerNameAsync = async (string prompt, string defaultValue) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// アイテム使用メニューを開いて実行します。
        /// </summary>
        public Func<Task<bool>> consumeItemMenuAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// 1つのアイテムを選択し、そのアイテムのIdを返します。
        /// </summary>
        public Func<Task<string>> selectOneItemAsync = async () => { await Task.Delay(0); return ""; };
        /// <summary>
        /// 装備画面を開きます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<Task<bool>> equipMenuAsync = async () =>
        {
            var r = await UI.Actions.equipMenuExAsync(Flags.Equip.ToEquipSet(), DefaultPersons.主人公.Id, null);
            if (r == null) return false;
            Flags.Equip.FromEquipSet(r);
            return true;
        };
        /// <summary>
        /// 装備画面を開きます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<EquipSet, string, Func<EquipSet, string>, Task<EquipSet>> equipMenuExAsync = async (equipSet, cutomValidator, personId) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 販売画面を開きます。
        /// </summary>
        public Func<GetPriceInvoker, Task<bool>> shopSellMenuAsync = async (GetPriceInvoker getPrice) => { await Task.Delay(0); return false; };
        /// <summary>
        /// 売却画面を開きます。
        /// </summary>
        public Func<Item[], GetPriceInvoker, Task<bool>> shopBuyMenuAsync = async (Item[] sellingItems, GetPriceInvoker getPrice) => { await Task.Delay(0); return false; };
        /// <summary>
        /// 睡眠の画面効果を発生させます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<Task<bool>> sleepFlashAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// 画面が白くなる画面効果を発生させます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<Task<bool>> WhiteFlashAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// ジャーナリングプレイバックから復帰します。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<Task> restoreActionSetAsync = () => Task.CompletedTask;
        /// <summary>
        /// ジャーナリングのプレイバック中に発生したアサーションを通知します。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, Task> tellAssertionFailedAsync = async (string message) => { await Task.Delay(0); };
        /// <summary>
        /// 進捗をメッセージで伝えます。一般のアプリからは呼び出しません。
        /// </summary>
        public Func<string, Task> progressStatusAsync = async (string message) => { await Task.Delay(0); };
        /// <summary>
        /// ジャーナリングのプレイバック中であるか判定します。
        /// </summary>
        public Func<bool> isJournalFilePlaying = delegate () { return false; };
        /// <summary>
        /// 砂時計カーソルの設定と解除を行います。
        /// </summary>
        public Func<bool, Task> 砂時計セット実行Async = async (bool on) => { await Task.Delay(0); };
        /// <summary>
        /// エクスポートするファイル名を取得します。
        /// </summary>
        public Func<string, string, string> ExportFileName実行 = (dummy1, dummy2) => null;
        /// <summary>
        /// インポートするファイル名を取得します。
        /// </summary>
        public Func<string, string, string> ImportFileName実行 = (dummy1, dummy2) => null;
        /// <summary>
        /// トータルレポートを表示します。
        /// </summary>
        public Func<Task> ShowTotalReportAsync = async () => { await Task.Delay(0); };
        /// <summary>
        /// トータルレポートをダウンロードします。
        /// </summary>
        public Func<Task> DownloadTotalReportAsync = async () => { await Task.Delay(0); };
        /// <summary>
        /// コレクションのリストを起動します。
        /// </summary>
        public Func<string, bool, bool, Task<CollectionItem>> InvokeCollectionListAsync = async (id, a, b) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 生活サイクルを変更します
        /// </summary>
        public Func<Task<bool>> ChangeCycleAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// 任意の処理を、フォームのメインスレッドで実行します。
        /// </summary>
        public Func<Func<Task>, Task> CallAnyMethodAsync = async (x) => { await x(); };
        /// <summary>
        /// 任意の処理を、デスクトップの場合のみ実行します。
        /// </summary>
        public VoidMethodInvokerInvoker CallOnlyDesktop = (x) => { x(); };
        /// <summary>
        /// 任意の処理を、WebPlayerの場合のみ実行します。
        /// </summary>
        public VoidMethodInvokerInvoker CallOnlyWeb = (x) => { /* no action here */ };
        /// <summary>
        /// メインのフォームを返すメソッドです。
        /// </summary>
        public Func<object> GetMainForm = () => { return null; };
        /// <summary>
        /// スキップ中ならTrueを返すメソッドです。
        /// </summary>
        public Func<bool> IsSkipping = () => { return false; };
        /// <summary>
        /// 各種ファイルを読み込みます
        /// </summary>
        public Func<string, string, Task<byte[]>> LoadFileAsync = async (Category, Name) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 各種ファイルを永続的に保管します
        /// </summary>
        public Func<string, string, byte[], Task<string>> SaveFileAsync = async (Category, Name, Body) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 自動セーブを実行する
        /// </summary>
        public Func<Task<bool>> AutoSaveFileAsync = async () => { await Task.Delay(0); return true; };
        /// <summary>
        /// シンプルなリードオンリーのリストUI
        /// </summary>
        public Func<string, Tuple<string, string>[], Task> SimpleListAsync = async (title, items) => { await Task.Delay(0); };
        /// <summary>
        /// DesktopならTrue
        /// </summary>
        public Func<bool> IsDesktop = () => true;
        /// <summary>
        /// WebPlayerの場合のみスプラッシュを表示する
        /// </summary>
        public Func<string, Task> WebSplashAsync = async (htmlFragment) => { await Task.Delay(0); };
        /// <summary>
        /// WebPlayerの場合のみスプラッシュを表示する
        /// </summary>
        public Func<string, Dummy> SetPictureUrl = (url) => default;
        /// <summary>
        /// DesktopPlayerの場合のみスプラッシュを表示する
        /// 引数はビットマップ(JPEG)へのストリームである
        /// </summary>
        public Func<Stream, Task> DesktopSplashAsync = async (bitmapStream) => { await Task.Delay(0); };
        /// <summary>
        /// HTMLフォームを表示して結果を名前と値のペアのコレクションとして受け取る
        /// HTMLフォームにform要素を含んではならない
        /// </summary>
        public Func<string, string, Task<Dictionary<string, string>>> HtmlFormAsync = async (title, htmlForm) =>
        {
            await Task.Delay(0);
            return new Dictionary<string, string>();
        };
        /// <summary>
        /// アップグレードチェックを行う
        /// </summary>
        public Func<string, string[], bool> UpgradeCheck = (msg, checkSams) => false;
        /// <summary>
        /// Webアクセスのアクセストークンを取得する
        /// </summary>
        public Func<string> GetAccessToken = () => null;
        /// <summary>
        /// リアルタイム通知メッセージの表示
        /// </summary>
        public Func<string, Task> NotifyStatusMessageAsync = async (msg) => { await Task.Delay(0); };
        /// <summary>
        /// 所持スター数を得る
        /// </summary>
        public Func<int> GetStars = () => 0;
        /// <summary>
        /// スターを消費する
        /// </summary>
        public Func<int, Task> AddStarsAsync = async (delta) => { await Task.Delay(0); };
        /// <summary>
        /// 日付の入力
        /// </summary>
        public Func<DateTime, DateTime, DateTime, Task<DateTime?>> DoEnterDateAsync = async (inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 日付時刻の入力
        /// </summary>
        public Func<DateTime, DateTime, DateTime, Task<DateTime?>> DoEnterDateTimeAsync = async (inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 日付時刻の入力オプション付き
        /// </summary>
        public Func<DateTime, DateTime, DateTime, string[], Task<Tuple<DateTime?,string>>> DoEnterDateTimeWithOptionsAsync = async (inital, min, max, options ) => { await Task.Delay(0); return null; };
        /// <summary>
        /// 整数の入力
        /// </summary>
        public Func<string, int, int, int, Task<int?>> DoEnterNumberAsync = async (prompt, inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// ネット送信
        /// </summary>
        public Func<string, string, Task<string>> NetSendAsync = async (id, base64) => { await Task.Delay(0); return null; };
        /// <summary>
        /// Uri取得
        /// </summary>
        public Func<string> GetUri = () => "unknown";
        /// <summary>
        /// カスタムなRazorコンポーネントを開く
        /// 引数のTypeはコンポーネントの型。
        /// 戻り値のobjectは定義が無い。自由に使って良い
        /// </summary>
        public Func<Type, Task<object>> OpenCustomRazorComponentAsync = (type) => Task.FromResult<object>(null);
        /// <summary>
        /// カスタムなRazorコンポーネントを閉じる
        /// このアクションはカスタムコンポーネント自身が終了時に呼び出すべき
        /// 戻り値のオブジェクトはOpenCustomRazorComponentの戻り値となる
        /// </summary>
        public Action<object> CloseCustomRazorComponent = (obj) => { };
        public Action Reboot = () => { };
    }

    /// <summary>
    /// ユーザーインターフェースを代表するクラスです。
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// 現在のアクションセットです。
        /// </summary>
        static public UIActionSet Actions;

        // 現在読み込まれているファイルのフルパス
        // 読み込まれていなければnull
        private static string currentFullPath;

        /// <summary>
        /// 読み出し専用のcurrentFullPathのラッパ
        /// </summary>
        public static string CurrentFullPath
        {
            get { return currentFullPath; }
        }

        /// <summary>
        /// メッセージ出力のヘルパ
        /// </summary>
        /// <param name="talker"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        static public void M(Person talker, string format, params Object[] arg)
        {
            if (arg.Length == 0)
            {
                Actions.messageOutputMethod(talker, format);
            }
            else
            {
                Actions.messageOutputMethod(talker, string.Format(format, arg));
            }
        }

        private static async Task<string> loadSaveAsync(string name, string filename, Func<string, string, Task<string>> proc, bool isAuto = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                DefaultPersons.システム.Say(name + "は中止されました。");
                return null; ;
            }
            string newFileName = null;
            string result = null;
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                await UI.Actions.砂時計セット実行Async(true);
                try
                {
                    newFileName = await proc(isAuto ? "AUTO" : "SAVE", filename);
                    if (string.IsNullOrWhiteSpace(newFileName))
                    {
                        result = "ロード/セーブの処理はできませんでした。";
                        newFileName = null;
                    }
                }
                catch (Exception e)
                {
                    result = e.ToString();
                }
                finally
                {
                    await UI.Actions.砂時計セット実行Async(false);
                }
            });
            if (result != null)
            {
                UI.M(DefaultPersons.システム, result);
                return null;
            }
            DefaultPersons.システム.Say(name + "は完了しました。");
            return newFileName;
        }

        /// <summary>
        /// UI込みでロードを行います。
        /// </summary>
        /// <returns>ロードに成功したらTrueを返します。</returns>
        static public async Task<bool> LoadAsync()
        {
            string filename = await Actions.loadFileNameAsync("データの読み込みを行います。");
            string result = await loadSaveAsync("読み込み", filename, State.LoadAsync);
            if (result == null) return false;
            currentFullPath = result; // データを読み込んだから上書きさせてはならない
            Actions.ResetGameStatus();
            Actions.ResetDisplay();
            DefaultPersons.システム.Say("{0}を読み込みました。", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        /// <summary>
        /// UI込みで自動セーブフォルダをデフォルトとしてロードを行います。
        /// </summary>
        /// <returns>ロードに成功したらTrueを返します。</returns>
        static public async Task<bool> LoadFromAutoSaveAsync()
        {
            string filename = await Actions.loadFileNameFromAutoSaveAsync("データの読み込みを行います。");
            string result = await loadSaveAsync("読み込み", filename, State.LoadAsync, true);
            if (result == null) return false;
            currentFullPath = null; // 自動セーブのデータを読み込んだから上書きさせてはならない
            Actions.ResetGameStatus();
            Actions.ResetDisplay();
            DefaultPersons.システム.Say("{0}を読み込みました。", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        /// <summary>
        /// 既に読み込まれているファイルを上書きします。
        /// </summary>
        /// <returns>セーブに成功したらTrueを返します。</returns>
        static public async Task<bool> SaveAsync()
        {
            if (currentFullPath == null) return await SaveAsAsync();
            // Skip.binをファイルに書く
            await ANGFLib.MessageSkipper.SaveAsync();
            string reault = await loadSaveAsync("保存", currentFullPath, State.SaveAsync);
            DefaultPersons.システム.Say("{0}を保存しました。", Path.GetFileNameWithoutExtension(reault));
            return true;
        }

        /// <summary>
        /// 新しいファイル名を入力してファイルをセーブします
        /// </summary>
        /// <returns>セーブに成功したらTrueを返します。</returns>
        static async public Task<bool> SaveAsAsync()
        {
            string filename = await Actions.saveFileNameAsync("データの保存を行います。");
            // Skip.binをファイルに書く
            await ANGFLib.MessageSkipper.SaveAsync();
            string result = await loadSaveAsync("保存", filename, State.SaveAsync);
            if (result == null) return false;
            currentFullPath = result;
            DefaultPersons.システム.Say("{0}を保存しました。", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        static private bool done()
        {
            State.Terminate();
            return true;
        }

        static private async Task<bool> confirmCommonAsync(string label)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem("はい", done ),
                new SimpleMenuItem("いいえ", nop ),
            };
            int selection = await UI.SimpleMenuWithoutSystemAsync($"本当に{label}しますか?", items);
            return selection == 0;
        }

        /// <summary>
        /// 終了の意思確認を行い、はいであればState.Terminate()メソッドを呼び出します。
        /// </summary>
        /// <returns>「はい」ならTrueを返します。</returns>
        static public async Task<bool> DoneConfirmAsync() => await confirmCommonAsync("終了");

        /// <summary>
        /// リブートの意思確認を行い、はいであればState.Terminate()メソッドを呼び出します。
        /// </summary>
        /// <returns>「はい」ならTrueを返します。</returns>
        static public async Task<bool> RebootConfirmAsync() => await confirmCommonAsync("リブート");

        static private async Task<bool> itemAsync()
        {
            return await UI.Actions.consumeItemMenuAsync();
        }

        static private async Task<bool> equipAsync()
        {
            var targetList = Party.EnumPartyMembers().Select(c => Person.List[c]).OfType<IPartyMember>().Where(c=>c.GetDirectEquipEnabled()).OfType<Person>().ToArray();
            Person p = DefaultPersons.主人公;
            if ( targetList.Length > 1 )
            {
                int index = await UI.SimpleMenuWithCancelAsync("装備を変更したい相手", targetList.Select(c => new SimpleMenuItem(c.HumanReadableName, null, null)).ToArray());
                p = targetList[index];
            }
            Func<EquipSet, string> customValidator = null;  // TBW must be implement
            Flags.Equip.TargetPersonId = p.Id;
            var eq = await UI.Actions.equipMenuExAsync(Flags.Equip.ToEquipSet(), p.Id, customValidator);
            if( eq == null ) return false;
            Flags.Equip.FromEquipSet(eq);
            return true;
        }

        static private bool nop()
        {
            // 何もしない
            return true;
        }

        static private async Task<bool> goHomeAndSleepAsync()
        {
            if (!await UI.YesNoMenuAsync("確認", "本当に1日を終える", "いや、まだ終わらない")) return false;
            DefaultPersons.システム.Say("1日を終わりにします。");
            await State.GoNextDayMorningAsync();
            return true;
        }

        static private async Task<bool> restMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            // 現在時刻より未来かつ就寝予定時刻までの時間をリストアップする
            int from = Flags.Now.Hour;
            int to = State.今日の就寝時刻.Hour;
            if (General.GetDateOnly(Flags.Now) != General.GetDateOnly(State.今日の就寝時刻))
            {
                to += 24;
            }
            for (int h0 = from + 1; h0 <= to; h0++)
            {
                int h = h0; // キャプチャー、オン!
                string label = string.Format("休憩{0}{1}時まで", h >= 24 ? "翌日" : "", h % 24);
                items.Add(new SimpleMenuItem(label, () => General.RestUntilAsync(h % 24, h >= 24)));
            }
            await UI.SimpleMenuWithCancelAsync("[休憩メニュー]", items.ToArray());
            return true;
        }

        static private async Task<bool> addEquipSetAsync(Person p)
        {
            DefaultPersons.システム.Say($"{p.HumanReadableName}が現在身に付けている装備セットを簡単に再現できるように保存します。");
            if (await UI.YesNoMenuAsync("保存しますか?", "保存する", "やめる"))
            {
                var old = Flags.Equip.TargetPersonId;
                try
                {
                    Flags.Equip.TargetPersonId = p.Id;
                    var allItemNames = Enumerable.Range(0, SimpleName<EquipType>.List.Count).Select(c => Items.GetItemByNumber(Flags.Equip[c])).Where(c => !c.IsItemNull).Select(c => c.HumanReadableName);
                    string s = allItemNames.FirstOrDefault() ?? "全裸";
                    if (allItemNames.Count() > 1) s += " etc";
                    string name = s;
                    if (State.IsEquipSetName(name))
                    {
                        for (int i = 1; ; i++)
                        {
                            name = $"{s} ({i})";
                            if (!State.IsEquipSetName(name)) break;
                        }
                    }
                    State.SetEquipSet(name, General.装備品情報のコピー());
                    DefaultPersons.システム.Say($"{name}として保存しました。これ以後はボタン1つでこの衣装を再現できます。名前は後から変更できます。");
                }
                finally
                {
                    Flags.Equip.TargetPersonId = old;
                }
            }
            return true;
        }

        static private async Task<bool> renameRemoveCommonEquipSetAsync(Func<string,Task> doit)
        {
            await UI.SimpleMenuWithCancelAsync("対象の装備セットを選択して下さい。", State.GetEquipSets().Where(c=>!State.DisabledEquipSetNames().Contains(c)).Select(c => new SimpleMenuItem(c, async () =>
            {
                await doit(c);
                return true;
            })).ToArray());
            return true;
        }

        static private async Task<bool> renameEquipSetAsync()
        {
            return await renameRemoveCommonEquipSetAsync(async (name) =>
            {
                string newName = await UI.Actions.enterPlayerNameAsync("新しい名前", name);
                if (string.IsNullOrWhiteSpace(newName))
                {
                    DefaultPersons.システム.Say($"空白の名前は使用できません。");
                    return;
                }
                if (State.IsEquipSetName(newName))
                {
                    DefaultPersons.システム.Say($"{newName}は既に使用済みの名前です。");
                    return;
                }
                State.RenameEquipSetName(name, newName);
                DefaultPersons.システム.Say($"{name}の名前を{newName}に変更しました。");
            });
        }

        static private async Task<bool> removeEquipSetAsync()
        {
            return await renameRemoveCommonEquipSetAsync(async (name) => {
                if (await UI.YesNoMenuAsync("本当に削除しますか?","はい","いいえ"))
                {
                    State.RemoveEquipSet(name);
                    DefaultPersons.システム.Say($"{name}を削除しました。");
                }
            });
        }

        static private async Task<bool> changeCycleAsync()
        {
            return await UI.Actions.ChangeCycleAsync();
        }
        static private bool SystemInfo()
        {
            DefaultPersons.システム.Say("現在の所持金: {0}", Flags.所持金);
            DefaultPersons.システム.Say("現在の所持スター: {0}", StarManager.GetStars());
#if MYBLAZORAPP
            DefaultPersons.システム.Say($"現在のプラットフォーム: {State.PlatformName ?? "Unknown"}");
            var host = General.IsInAzure() ? "Azure/ネット送信可能" : "Other/ネット送信不可";
            DefaultPersons.システム.Say($"現在のホスト: { host }");
            var blazor = General.IsBlazorWebAssembly() ? "WebAssembly" : "Server";
            DefaultPersons.システム.Say($"現在のBlazor: {blazor}");
#if DEBUG
            DefaultPersons.システム.Say($"現在のRuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
#endif
#else
            DefaultPersons.システム.Say("現在のプラットフォーム: {0}", State.IsInWebPlayer ? "Web" : "Desktop");
            var misc = Util.ReadRegistryForMisc();
            var f = misc.UseCloudStorage;
            if (State.IsInWebPlayer) f = true;
            DefaultPersons.システム.Say("現在のストレージ: {0}", f ? "クラウド" : "ローカル");
#endif
            return true;
        }

        static private async Task<bool> systemMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            if ((State.MenuStopMaps & MenuStopControls.Item) == 0)
            {
                items.Add(new SimpleMenuItem("持ち物", itemAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Equip) == 0)
            {
                items.Add(new SimpleMenuItem(CommonCommandNames.装備, equipAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Rest) == 0)
            {
                items.Add(new SimpleMenuItem("休憩1時間", General.Rest60Async));
                items.Add(new SimpleMenuItem("休憩15分", General.Rest15Async));
                items.Add(new SimpleMenuItem("休憩 指定時まで", restMenuAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.GoHome) == 0)
            {
                items.Add(new SimpleMenuItem("1日を終える", goHomeAndSleepAsync));
            }
            var oneManParty = Party.EnumPartyMembers().Count() == 1;
            foreach (var item in Party.EnumPartyMembers())
            {
                var p = Person.List[item];
                var label = "装備セット追加";
                if (!oneManParty) label += $" ({p.HumanReadableName})";
                items.Add(new SimpleMenuItem(label, async ()=> { await addEquipSetAsync(p); return true; }));
            }
            items.Add(new SimpleMenuItem("装備セット名前変更", renameEquipSetAsync));
            items.Add(new SimpleMenuItem("装備セット削除", removeEquipSetAsync));
            if ((State.MenuStopMaps & MenuStopControls.ChangeCycle) == 0)
            {
                items.Add(new SimpleMenuItem("生活サイクル変更", changeCycleAsync));
            }

            foreach (var m in State.loadedModules) m.ConstructSystemMenu(items, State.CurrentPlace);

            if ((State.MenuStopMaps & MenuStopControls.Load) == 0)
            {
                items.Add(new SimpleMenuItem("ロード", LoadAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.AutoLoad) == 0)
            {
                items.Add(new SimpleMenuItem("自動セーブ フォルダからロード", LoadFromAutoSaveAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Save) == 0 && currentFullPath != null)
            {
                items.Add(new SimpleMenuItem(Path.GetFileNameWithoutExtension(currentFullPath) + "へ上書きセーブ", SaveAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Save) == 0)
            {
                items.Add(new SimpleMenuItem("名前を付けてセーブ", SaveAsAsync));
            }
            foreach (var item in SimpleName<Collection>.List.Values)
            {
                var id0 = item.Id;  // Capture on
                items.Add(new SimpleMenuItem(item.HumanReadableName+"・一覧", async () =>
                {
                    _ = await UI.Actions.InvokeCollectionListAsync(id0, false, false);
                    return false;
                }));
            }
            items.Add(new SimpleMenuItem("システム詳細情報", SystemInfo));
            if (SystemFile.IsDebugMode)
            {
                items.Add(new SimpleMenuItem("デバッグ用無制限時間移動", async () =>
                {
                    if (!UI.Actions.isJournalFilePlaying())
                    {
                        var minDate = DateTime.MinValue;
                        var maxDate = new DateTime(9998, 12, 31);
                        var date = await UI.Actions.DoEnterDateAsync(Flags.Now, minDate, maxDate);
                        if (date == null) return false;
                        await General.TimeWarpAsync(date.Value);
                    }
                    return true;
                }));
                items.Add(new SimpleMenuItem("テスト専用ショップ", async () =>
                {
                    var list = Item.List.Values.Where(c => !c.IsItemNull);
                    await UI.Actions.shopBuyMenuAsync(list.ToArray(), (a, b) => 100);
                    return true;
                }));
                items.Add(new SimpleMenuItem("所持金百万円アップ", () =>
                {
                    Flags.所持金 += 1000000;
                    DefaultPersons.システム.Say("所持金を増やしました。");
                    return true;
                }));
                items.Add(new SimpleMenuItem("所持スター百個アップ", () =>
                {
                    StarManager.AddStar(100);
                    DefaultPersons.システム.Say("所持スターを増やしました。");
                    return true;
                }));
                items.Add(new SimpleMenuItem("ジャーナリング再生", async () =>
                {
                    await General.StartJournalingPlaybackAsync();
                    return true;
                }
                ));
            }
            items.Add(new SimpleMenuItem("ゲーム・レポートのダウンロード", async () =>
            {
                await UI.Actions.DownloadTotalReportAsync();
                return true;
            }));
            var extraSubMenus = ANGFLib.MenuItem.List.Values.Where(c => c.MenuType == MyMenuType.Info).ToArray();
            foreach (var subMenu in extraSubMenus)
            {
                items.Add(new SimpleMenuItem(subMenu.HumanReadableName, async () =>
                {
                    //if (subMenu.Method != null) subMenu.Method(UI.Actions.GetMainForm());
                    if (subMenu.MethodAsync != null) await subMenu.MethodAsync(UI.Actions.GetMainForm());
                    return true;
                }));
            }
            //if (extraSubMenus.Length > 0)
            //{
            //items.Add(new SimpleMenuItem("追加メニューへ", async () => { await extraSubMenu(extraSubMenus); return true; }));
            //}
            //foreach (var item in ANGFLib.MenuItem.List.Values.Where(c => c.MenuType == MyMenuType.Top))
            //{
            //items.Add(new SimpleMenuItem(item.HumanReadableName, async () =>
            //{
            //if (item.Method != null) item.Method(null);
            //if (item.MethodAsync != null) await item.MethodAsync(null);
            //return true;
            //}));
            //}
#if false
            items.Add(new SimpleMenuItem("Person整合性チェック", () =>
            {
                foreach (var item in Person.AllPersonIds)
                {
                    System.Diagnostics.Debug.Assert(Person.List.Keys.Contains(item));
                }
                return false;
            }));
#endif
            items.Add(new SimpleMenuItem("リブート", async () =>
            {
                if (!await UI.YesNoMenuAsync("セーブしていないデータは全て失われます。ゲームをリブートしますか?", "はい", "いいえ")) return true;
                if (!await UI.YesNoMenuAsync("本当にリブートするのですね?", "はい", "いいえ")) return true;
                UI.Actions.Reboot();
                return true;
            })
            { MenuType = SimpleMenuType.Hot });
            if (!General.IsBlazorWebAssembly())
            {
                items.Add(new SimpleMenuItem("終了", async () =>
                {
                    if (!await UI.YesNoMenuAsync("セーブしていないデータは全て失われます。ゲームを終了しますか?", "はい", "いいえ")) return true;
                    if (!await UI.YesNoMenuAsync("本当に終了するのですね?", "はい", "いいえ")) return true;
                    Environment.Exit(0);
                    return true;
                })
                { MenuType = SimpleMenuType.Hot });
            }

            await UI.simpleMenuWithCancelForSystemMenuAsync("[システム メニュー]", items.ToArray());
            return true;
        }

        //private static async Task extraSubMenu(MenuItem[] extraSubMenus)
        //{
        //List<SimpleMenuItem> items = new List<SimpleMenuItem>();
        //    foreach (var item in extraSubMenus)
        //    {
        //items.Add(new SimpleMenuItem(item.HumanReadableName, async () => {
        //            if (item.Method != null) item.Method(null);
        //if (item.MethodAsync != null) await item.MethodAsync(null);
        //  return true; }));
        //}
        //    if (SystemFile.IsDebugMode) {
        //          items.Add(new SimpleMenuItem("メッセージ既読クリア", () => {
        //        MessageSkipper.Clear();
        //          return true;
        //    }));
        //}
        //await UI.SimpleMenuWithCancelForSystemMenu("[追加 システム メニュー]", items.ToArray());
        //  }

        private static SimpleMenuItem systemMenuItem = new SimpleMenuItem(CommonCommandNames.SYSTEM, systemMenuAsync);
        /// <summary>
        /// シンプルなメニューです。システムメニューが付きます。選ばれた場合、項目に実行すべき内容があれば実行します。
        /// </summary>
        /// <param name="prompt">プロンプト文字列です。</param>
        /// <param name="items">メニュー一覧です</param>
        /// <returns>選択された添え字です。</returns>
        static public async Task<int> SimpleMenuAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(true);
            return await Actions.simpleMenuMethodAsync(prompt, items, systemMenuItem);
        }

        /// <summary>
        /// シンプルなメニューです。キャンセルが付きます。システムメニューは付きません。選ばれた場合、項目に実行すべき内容があれば実行します。
        /// </summary>
        /// <param name="prompt">プロンプト文字列です。</param>
        /// <param name="items">メニュー一覧です</param>
        /// <returns>選択された添え字です。キャンセルされた場合は-1を返します。</returns>
        static public async Task<int> SimpleMenuWithCancelAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, new SimpleMenuItem("キャンセル"));
        }
        static private async Task<int> simpleMenuWithCancelForSystemMenuAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(true);
            return await Actions.simpleMenuMethodAsync(prompt, items, new SimpleMenuItem("キャンセル"));
        }
        /// <summary>
        /// シンプルなメニューです。キャンセルや、システムメニューは付きません。選ばれた場合、項目に実行すべき内容があれば実行します。
        /// </summary>
        /// <param name="prompt">プロンプト文字列です。</param>
        /// <param name="items">メニュー一覧です</param>
        /// <returns>選択された添え字です。</returns>
        static public async Task<int> SimpleMenuWithoutSystemAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null);
        }

        /// <summary>
        /// 二択のメニューを出します
        /// </summary>
        /// <param name="prompt">プロンプト文字列です。</param>
        /// <param name="yesMessage">肯定の選択肢文字列です。</param>
        /// <param name="noMessage">否定の選択肢文字列です。</param>
        /// <returns>肯定の選択肢が選ばれたらTrueを返します。</returns>
        static public async Task<bool> YesNoMenuAsync(string prompt, string yesMessage, string noMessage)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem(yesMessage ),
                new SimpleMenuItem(noMessage ),
            };
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null) == 0;
        }

        /// <summary>
        /// 三択のメニューを出します
        /// </summary>
        /// <param name="prompt">プロンプト文字列です。</param>
        /// <param name="message1">選択肢1文字列です。</param>
        /// <param name="message2">選択肢2文字列です。</param>
        /// <param name="message3">選択肢3文字列です。</param>
        /// <returns>0,1,2で選ばれた項目を示します。</returns>
        static public async Task<int> 三択Async(string prompt, string message1, string message2, string message3)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem(message1 ),
                new SimpleMenuItem(message2 ),
                new SimpleMenuItem(message3 ),
            };
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null);
        }

        public static Func<bool,Dummy> EnableEquipButtonsBody;
        public static void EnableEquipButtons(bool b)
        {
            if (EnableEquipButtonsBody != null) EnableEquipButtonsBody(b);
        }

    }
}
