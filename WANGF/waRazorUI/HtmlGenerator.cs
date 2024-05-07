using ANGFLib;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using wangflib;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using System.Diagnostics;
using System.Reflection.Emit;

namespace waRazorUI
{
    public enum EEquipMode
    {
        Normal = 0,
        EquipSelectList = 1,
        GeneralItemList = 2
    }

    public class HtmlGenerator
    {
        public static bool useCustomComponent = false;
        public static Type TypeOfCustomComponent = null;
        public static object ResultOfCustomComponent = null;
        public static bool nextButtonEnabled = false;
        public static string[] listLabels = null;
        public static SimpleMenuType[] SimpleMenuTypes = null;
        private static string[] actualListLabels = null;
        public static string[] listExplanations = null;
        public static string[] listMouseOverTexts = null;
        public static string menuPrompt;
        public static Func<int, Task> menuDoneAsync;/* DIABLE ASYNC WARN */
        public static string debugMessage = "";
        public static string RealtimeStatusMessage = "";
        public static string myMarkup = "";    // WANGF.Client.wangfMain.GetMainAreaHtmlString();
        public static string splashMarkup = "";
        public static System.Threading.CancellationTokenSource source = null;
        public static List<MessageBlock> messages = new List<MessageBlock>();
        public static string lineInputPrompt;
        public static string lineInputTextValue;
        public static string lineInputDefaultValue;
        public static string lineInputErrorMessage;
        public static Item SelectedItem;
        public static MenuItem[] listMenus = null;
        public static bool IsJounalResultEnable;
        public static bool IsBatchTestingResultEnable;
        public static IEnumerable<string> EnumBatchTestingResults;
        public static string GiftCodeTextValue;
        public static bool IsPopupEnabled;
        public static string SimpleListTitle;
        public static Tuple<string, string>[] SimpleList;
        public static string ExceptionInfo;
        public static bool? Is18K { get; set; } = null;
        public static FlagEditorInfo[] FlagEditorList;
        public static FlagEditorInfo SelectedFlagItem;
        public static string FlagInputTextValue;
        public static string GameHintUrl;
        public static bool UploadEnabled = false;
        public static bool UploadJournalingEnabled => SystemFile.IsDebugMode;
        public static string UploadDescription => UploadEnabled ? "全ファイルZIP" : "再生ジャーナリングファイル";
        public static bool IsGameExplainEnabled = false;
        public static string JournalingDirectSourceTextValue;
        public static string PictureUrl;
        public static System.Net.Http.HttpClient Http;
        public static DateTime EditingDateTime;
        public static DateTime ResultDateTime;
        public static int Hour;
        public static int Minutes;
        public static string[] DateTimeOptions;
        public static int DateTimeOption;
        public static int duplicateIndexInCreateMenuItemsAsync = -1;

        public static async Task PopupCloseAsync()
        {
            HtmlGenerator.IsPopupEnabled = false;
            SimpleList = null;
            SimpleListTitle = null;
            EditingDateTime = DateTime.MinValue;
            TriggerUIUpdate();
            await Task.Delay(0);
        }

        private static string _generalItemId;
        public static string GeneralItemId
        {
            get { return _generalItemId; }
            set
            {
                _generalItemId = value;
                GeneralItemSelectionChanged();
            }
        }
        public static string GeneralItemExplanation;


        private static string[] getDefaultValue() => new string[1] { "*" };

        private static async Task setBackgroundColorAsync(Func<System.Drawing.Color, System.Drawing.Color> coocker = null)
        {
            System.Drawing.Color color = ANGFLib.Util.CalcCenterColor(ANGFLib.Util.CalcCenterColor(getBackgroundColor(ANGFLib.Flags.Now), System.Drawing.Color.White), System.Drawing.Color.White);
            if (coocker != null) color = coocker(color);
            await JsWrapper.setBackColorWrapperAsync(ToCssColor(color));
        }

        public static async Task ClearAllAsync()
        {
            myMarkup = "";
            foreach (var item in messages) MessageSkipper.SetMessage既読(item.Message);
            messages.Clear();
            listLabels = new string[0];
            nextButtonEnabled = false;
            lineInputTextValue = null;
            lineInputDefaultValue = null;
            SelectedItem = null;
            await setBackgroundColorAsync();
            EquipMode = EEquipMode.Normal;
            TriggerUIUpdate();
        }

        static string ToCssColor(System.Drawing.Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private static System.Drawing.Color createHightlightColor(System.Drawing.Color color)
        {
            int[] c = { color.R, color.G, color.B };
            int min = int.MaxValue;
            foreach (int n in c)
            {
                if (n < min) min = n;
            }
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == min) c[i] = 0;
            }
            return System.Drawing.Color.FromArgb(c[0], c[1], c[2]);
        }
        public static string getNameStyle(MessageBlock message)
        {
            System.Drawing.Color nameColor = ANGFLib.Util.CalcCenterColor(message.TextColor, System.Drawing.Color.Black);
            return "color:" + ToCssColor(nameColor);
        }
        public static string getMessageStyle(MessageBlock message)
        {
            string bold = "";
            if (message.IsSuperMessage)
            {
                bold = "font-size:200%;";
            }
            else if (!ANGFLib.MessageSkipper.IsMessage既読(message.Message))
            {
                bold = "font-size:150%;";
            }
            string back = "";
            if (message.BackColor != System.Drawing.Color.Transparent)
            {
                back = ";background-color:" + ToCssColor(message.BackColor);
                bold = "font-size:200%;";
            }
            return bold + "font-weight:bold; color:" + ToCssColor(message.TextColor) + back;
        }

        public static bool isNotReady
        {
            get
            {
                return ANGFLib.State.CurrentPlace.IsStatusHide;
            }
        }

        public static string TotalReport { get; internal set; }

        private static System.Drawing.Color getBackgroundColor(DateTime dt)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 0, 0);
            if (isNotReady)
            {
                // 開始前は黒くしておく
                return color;
            }
            //Console.WriteLine("C:" + ANGFLib.State.loadedModules.Length);
            foreach (var m in ANGFLib.State.loadedModules)
            {
                //Console.WriteLine(m.GetBackColor());
                if (m.GetBackColor != null)
                {
                    return m.GetBackColor();
                }
            }
            return color;
        }

        public static Func<int, Task> earlyDoneAsync = null;/* DIABLE ASYNC WARN */

        static void RawSimpleMenu(string[] labels, SimpleMenuType[] types, string[] explanations, string[] mouseOverTexts, Func<int, bool, Task> done)
        {
            actualListLabels = labels;
            if (labels.Length < 1)
                listLabels = getDefaultValue();
            else
                listLabels = labels;
            SimpleMenuTypes = types;
            if (explanations == null)
                listExplanations = new string[listLabels.Length];
            else
                listExplanations = explanations;

            if (mouseOverTexts == null)
                listMouseOverTexts = new string[listLabels.Length];
            else
                listMouseOverTexts = mouseOverTexts;

            menuDoneAsync = async (index) =>
            {
                if (earlyDoneAsync != null) await earlyDoneAsync(index);
                await ClearAllAsync();
                await done(index, true);
            };
            TriggerUIUpdate();
        }

        public static void TriggerUIUpdate()
        {
            dynamic dform = ScenarioThread.GetMainForm();
            dform?.Update();
            dynamic dformEquip = ScenarioThread.GetEquipForm();
            dformEquip?.Update();
        }

        public static void TriggerReload()
        {
            if (General.IsBlazorWebAssembly())
            {
                dynamic dform = ScenarioThread.GetMainForm();
                dform?.Reload();
            }
            else
            {
                Process.Start(Environment.ProcessPath);
                Environment.Exit(0);
            }
        }

        static void nextClicked()
        {
            nextButtonEnabled = false;
            source.Cancel();
        }

        private static string getTitleUri(GameStartupInfo[] games)
        {
            var ar = games.Where(c => c.TitlePicture != null).Select(c => c.TitlePicture).ToArray();
            if (ar.Length == 0) return null;
            return "data:image/jpeg;base64," + Convert.ToBase64String(ar[Random.Shared.Next(ar.Length)]);
        }

        public static async Task ShowGameSelectAsync(GameStartupInfo[] games)
        {
            UI.Actions.SetPictureUrl(getTitleUri(games)??"_content/waRazorUI/WANGF001.jpg");
            string[] baseNames = new string[0];
            if (SystemFile.IsDebugMode) baseNames = new string[] { "Batch Test" };
            SimpleMenuType[] baseTypes = new SimpleMenuType[0];
            if (SystemFile.IsDebugMode) baseTypes = new SimpleMenuType[] { SimpleMenuType.Normal };
            HtmlGenerator.IsGameExplainEnabled = true;
            Func<int, bool, Task> doit = async (index, touchRequest) =>
            {
                if (SystemFile.IsDebugMode)
                {
                    if (index == 0)
                    {
                        //if (!General.IsBlazorWebAssembly())
                        //{
                        //DefaultPersons.システムWarn.Say("このプラットフォームでバッチテストはサポートされていません。");
                        //return;
                        //}
                        var count = (await Modules.EnumEmbeddedModulesAsync()).Count();
                        // start batch testing
                        await BatchTest.BatchTestingForFramework.StartBatchTestingAsync(count);
                        return;
                    }
                    index--;
                }
                UI.Actions.SetPictureUrl(null);
                HtmlGenerator.IsGameExplainEnabled = false;
                if (index < 0) return;  // do nothing
                General.GameTitle = games[index].name;  // set game title as selected
                HtmlGenerator.Is18K = games[index].Is18K;
                StaticGameStartupInfo.myStartupAsync = wangfMain.StartupAsync;
                FileGameStartupInfo.myStartupAsync = wangfMain.ExtensibleStartupAsync;
                if (touchRequest) games[index].Touch();   // タイムスタンプ更新
                await games[index].StartGameAsync();
            };
            // Shift+Ctrlが押されているときはバッチテストを開始しない
            if (!RealtimeKeyScan.IsShiftAndCtrlKeyPress() && await BatchTest.BatchTestingForEachTitle.IsBatchTestingAsync())
            {
                //if (!General.IsBlazorWebAssembly())
                //{
                // バッチテストが許可されていないプラットフォームで走っていたらここでテストを強制停止させる
                //await BatchTest.BatchTestingForFramework.ClearAsync();
                //return;
                //}
                var count = await BatchTest.BatchTestingForEachTitle.GetCountAsync();
                if (SystemFile.IsDebugMode) count++;
                await doit(count, false);
            }
            else
            {
                games = games.OrderByDescending(c => c.GetTouchedDateTime()).ToArray();
                var ar = baseNames.Concat(games.Select(c => c.name + (c.Is18K ? " (18禁)" : ""))).ToArray();
                var types = baseTypes.Concat(games.Select(c => c.Is18K ? SimpleMenuType.Hot : SimpleMenuType.Normal)).ToArray();
                var baseDescriptions = new string[0];
                if (SystemFile.IsDebugMode) baseDescriptions = new string[] { "" };
                RawSimpleMenu(ar, types, null, baseDescriptions.Concat(games.Select(c => c.description)).ToArray(), doit);
            }
        }
        static void debugButton()
        {
            myMarkup = "<p style='color:#0000ff'>replaced</p>";

            //RunAsync text
#if true
            Task.Run(async () =>
            {
                await Console.Out.WriteLineAsync("A1");
            }).Wait();
#else
Microsoft.AspNet.Identity.AsyncHelper.RunSync(async () =>
{
await Console.Out.WriteLineAsync("A1");
});
#endif
            Console.WriteLine("A2");
        }

        private static async Task startJournalingPlaybackAsync()
        {
            if (JournalPlaybackQueue.Count() == 0) return;

            HtmlGenerator.PictureUrl = null;    // hide splash
            JournalingInputDescripter filename = JournalPlaybackQueue.Dequeue();
            if (filename == null) return;

            // いよいよプレイバックを始めるざんす
            try
            {
                // コンパイル実行
                //this.Text = "Compiling Journal File...";
                Util.JounaligStartDateTime = DateTime.Now;
                SystemFile.StartPlayback();
                await State.JournalingPlayer(filename);
                DefaultPersons.システム.Say($"Journaling Playback All Done. Time:{DateTime.Now - Util.JounaligStartDateTime}");
            }
            catch (JournalingDocumentException ex)
            {
                //System.Diagnostics.Trace.WriteLine(ex.ToString());
                waRazorUI.HtmlGenerator.ShowMessage(DefaultPersons.システム, $"Journaling Playback Encounted JournalingDocumentException... Time:{DateTime.Now - Util.JounaligStartDateTime}");
                waRazorUI.HtmlGenerator.ShowMessage(DefaultPersons.システム, ex.ToString());
                return;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Trace.WriteLine(ex.ToString());
                waRazorUI.HtmlGenerator.ShowMessage(DefaultPersons.システム, $"Journaling Playback Encounted General Exception... Time:{DateTime.Now - Util.JounaligStartDateTime}");
                waRazorUI.HtmlGenerator.ShowMessage(DefaultPersons.システム, ex.ToString());
                return;
            }
        }

        public static async Task CreateMenuItemsAsync(string prompt, SimpleMenuItem[] labels, Func<int, Dummy> onselected)
        {
            if (JournalPlaybackQueue.Count() > 0)
            {
                await startJournalingPlaybackAsync();
                // フォームに退場とシンプルメニューの再実行を要請
                // 再実行時には差し変わったアクションセットが実行される
                onselected(-2);
                return;
            }

            //Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture);
            Util.InitCulture();
            await setBackgroundColorAsync();
            menuPrompt = prompt;
            duplicateIndexInCreateMenuItemsAsync = -3; // special code for same command again 
            for (; ; )
            {
                try
                {
                    source = new System.Threading.CancellationTokenSource();
                    RawSimpleMenu(labels.Select(c => c.Name).ToArray(),
                        labels.Select(c => c.MenuType).ToArray(),
                        labels.Select(c => c.Explanation).ToArray(),
                        labels.Select(c => c.MouseOverText).ToArray(),
                        (index, touchRequest) =>
                        {
                            duplicateIndexInCreateMenuItemsAsync = index;
                            source.Cancel();
                            return Task.CompletedTask;
                        });
                    //StateHasChanged();
                    await Task.Delay(-1, source.Token);
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // nothing to do
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // nothing to do
                }
                if (duplicateIndexInCreateMenuItemsAsync != -3) break;
            }
            //catch (System.Exception e)
            //{
            //    Console.WriteLine("e2:"+e.ToString());
            //}
            //Console.WriteLine("a");
            onselected(duplicateIndexInCreateMenuItemsAsync);
        }
        private static void addMesssage(ANGFLib.Person talker, string message, System.Drawing.Color backColor)
        {
            messages.Add(new MessageBlock(talker, message, backColor));
            //if (messages.Count > 50) messages.RemoveAt(0);
        }
        public static void ShowMessage(ANGFLib.Person talker, string message)
        {
            try
            {
                addMesssage(talker, message, System.Drawing.Color.Transparent);
                //StateHasChanged();

                //if (messageSkip && ANGFLib.MessageSkipper.IsMessage既読(message)) return;
                // ジャーナリングの経過時間表示は常に待たない
                if (message.StartsWith(ANGFLib.Constants.スキップ例外Prefix)) return;

                //ANGFLib.MessageSkipper.SetMessage既読(message);
                //source = new System.Threading.CancellationTokenSource();
                //nextButtonEnabled = true;
                //await Task.Delay(-1, source.Token);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // nothing to do
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                // nothing to do
            }
        }

        private static NavigationManager uriHelper;

        public static string GetUri()
        {
            return uriHelper.Uri;
        }

        public static void OnInitializedSub(object form, IJSRuntime jsRuntime, NavigationManager urihelper)
        {
            ScenarioThread.SetMainForm(form);
            JsWrapper.JsRuntime = jsRuntime;
            Http = new HttpClient();
            uriHelper = urihelper;
            //Console.WriteLine($"{System.Threading.Thread.CurrentThread.CurrentCulture} {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
            //CultureInfo.CurrentCulture = new CultureInfo("ja-JP");
            //Console.WriteLine($"{System.Threading.Thread.CurrentThread.CurrentCulture} {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            //Console.WriteLine(Thread.CurrentThread.CurrentCulture.Name);
        }

        public static void OnInitializedSub2(object form)
        {
            ScenarioThread.SetEquiopForm(form);
        }

        public static async Task<string> EnterPlayerNameAsync(string prompt, string defaultValue)
        {
            lineInputPrompt = prompt;
            lineInputTextValue = defaultValue;
            lineInputDefaultValue = defaultValue;
            await JsWrapper.JsRuntime.InvokeAsync<string>("setLineInputTextValue", lineInputDefaultValue);

            TriggerUIUpdate();

            string result = null;
            try
            {
                source = new System.Threading.CancellationTokenSource();
                //StateHasChanged();
                await Task.Delay(-1, source.Token);
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                // nothing to do
            }
            finally
            {
                result = lineInputTextValue;
                await ClearAllAsync();
            }
            return result;
        }



        private static string createStatusString(string str, string alter)
        {
            Util.InitCulture();
            if (isNotReady) return alter;
            return str;
        }

        private static string createStatusString(Func<string> str, Func<string> alter)
        {
            Util.InitCulture();
            if (isNotReady) return alter();
            return str();
        }

        internal static async Task FlashScreenAsync(byte r, byte g, byte b)
        {
#if ENABLE_FLASHING
            AdBlock.DisableIncrement = true;
            try
            {
                for (int i = byte.MaxValue - 1; i >= 0; i -= 16) await fAsync((byte)i);
                for (int i = 0; i < byte.MaxValue; i += 16) await fAsync((byte)i);
            }
            finally
            {
                AdBlock.DisableIncrement = false;
            }
            async Task fAsync(byte rate)
            {
                var col = System.Drawing.Color.FromArgb(r, g, b);
                await setBackgroundColor((src) =>
                {
                    float rate1 = (float)rate / 255;
                    int dr = src.R - r;
                    int dg = src.B - b;
                    int db = src.G - g;
                    byte r0 = (byte)(r + dr * rate1);
                    byte g0 = (byte)(g + dg * rate1);
                    byte b0 = (byte)(b + db * rate1);
                    return System.Drawing.Color.FromArgb(r0, g0, b0);
                });
                TriggerUIUpdate();
                await Task.Delay(100);
            };
#else
            await Task.Delay(0);
#endif
        }

        internal static string ProgressMessage = "";

        internal static async Task SetProgressMessageAsync(string message)
        {
            ProgressMessage = message;
            waRazorUI.HtmlGenerator.TriggerUIUpdate();
            await Task.Delay(1);
        }

        public static string statusBackStyle()
        {
            return "color:White;background-color:" + ToCssColor(getBackgroundColor(ANGFLib.Flags.Now));
        }
        public static string dateTime()
        {
            const string stringForNotReady = "----年--月--日(-)--時--分" + "★※※※";
            foreach (var modex in State.LoadedModulesEx)
            {
                var ar = modex.QueryObjects<CustomDateTimeGetter>();
                foreach (var item in ar)
                {
                    return createStatusString(() => ANGFLib.Flags.Now.ToString(item.CustomDateTime()), () => stringForNotReady);
                }
            }
            return createStatusString(() => ANGFLib.Flags.Now.ToString("yyyy年MM月dd日(ddd)HH時mm分") + " ★" + ANGFLib.StarManager.GetStars().ToString(), () => stringForNotReady);
        }
        public static string kishoTime()
        {
            return createStatusString(ANGFLib.State.今日の起床時刻.ToString("HH時mm分起床"),
        "--時--分起床");
        }
        public static string sleepTime()
        {
            return createStatusString(ANGFLib.State.今日の就寝時刻.ToString("HH時mm分就寝予定"),
        "--時--分就寝予定");
        }
        public static string dateTimeStyle()
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            if (ANGFLib.State.loadedModules != null)
            {
                bool isRed = false, isBlue = false;
                foreach (var n in ANGFLib.State.loadedModules)
                {
                    if (n.IsRedDay != null && n.IsRedDay(ANGFLib.Flags.Now))
                    {
                        isRed = true;
                        break;
                    }
                }
                foreach (var n in ANGFLib.State.loadedModules)
                {
                    if (n.IsBlueDay != null && n.IsBlueDay(ANGFLib.Flags.Now))
                    {
                        isBlue = true;
                        break;
                    }
                }
                foreach (var n in ANGFLib.State.loadedModules)
                {
                    if (n.GetDateColor != null)
                    {
                        color = n.GetDateColor(ANGFLib.Flags.Now, isRed, isBlue);
                        break;
                    }
                }
            }
            return "color:" + ToCssColor(color);
        }

        internal static async Task<Tuple<Item, int>> SellItemMenuAsync(GetPriceInvoker getPrice)
        {
            Item selectedItem = await generalItemListAsync($"アイテム売却: {State.CurrentPlace.HumanReadableName}", enumSellItems, createSellItemLabel, true);
            if (selectedItem == null) return null;

            if (State.IsEquipedItem(selectedItem))
            {
                DefaultPersons.システム.Say("装備中のアイテムは売れません。");
                return null;
            }

            if (selectedItem.Price == 0)
            {
                DefaultPersons.システム.Say("売却できないアイテムです。");
                return null;
            }

            if (HtmlGenerator.GeneralItemCount > State.GetItemCount(selectedItem))
            {
                DefaultPersons.システム.Say($"{HtmlGenerator.GeneralItemCount}個売ろうとしましたが、{State.GetItemCount(selectedItem)}個しか持っていません。");
                return null;
            }

            int 売却予定価格 = getPrice(selectedItem, State.GetUsedCount(selectedItem)) * HtmlGenerator.GeneralItemCount;
            string confirmMessage = string.Format("{0}を{1}個、計{2}で売却します。よろしいですか?",
                selectedItem.HumanReadableName,
                HtmlGenerator.GeneralItemCount,
                Coockers.PriceCoocker(売却予定価格));

            var result = await UI.YesNoMenuAsync(confirmMessage, "はい", "いいえ");
            if (!result) return null;

            Flags.所持金 += 売却予定価格;
            State.SetItemCount(selectedItem, State.GetItemCount(selectedItem) - HtmlGenerator.GeneralItemCount);
            var UsedCount = State.GetUsedCount(selectedItem);
            State.SetUsedCount(selectedItem, 0);    // 手元にないので使用回数は無意味
            return new Tuple<Item, int>(selectedItem, UsedCount);

            IEnumerable<Item> enumSellItems()
            {
                return Item.List.Values.Where(item => State.GetItemCount(item) > 0 &&
                        // 値段が0のアイテムは売れないアイテム
                        getPrice(item, State.GetUsedCount(item)) > 0);
            }

            string createSellItemLabel(Item item)
            {
                string s = string.Format("{0,9} {1} (所持 {2}/{3} あと{4}購入可)",
                    Coockers.PriceCoocker(getPrice(item, 0)), item.HumanReadableName,
                    State.GetItemCount(item), item.Max, item.Max - State.GetItemCount(item));
                return s;
            }
        }


        internal static async Task<Item> BuyItemMenuAsync(Item[] sellingItems, GetPriceInvoker getPrice)
        {
            Item selectedItem = await generalItemListAsync($"アイテム購入: {State.CurrentPlace.HumanReadableName}", () => sellingItems, createBuyItemLabel, true);
            if (selectedItem == null) return null;

            if (selectedItem.Max < State.GetItemCount(selectedItem) + HtmlGenerator.GeneralItemCount)
            {
                DefaultPersons.システム.Say(selectedItem.HumanReadableName + "は"
                    + selectedItem.Max + "個までしか持てないため、購入できません。");
                return null;
            }

            if (HtmlGenerator.GeneralItemCount == 0)
            {
                DefaultPersons.システム.Say("個数は1以上を指定してください。");
                return null;
            }

            int 購入予定価格 = getPrice(selectedItem, 0) * HtmlGenerator.GeneralItemCount;
            string confirmMessage = string.Format("{0}を{1}個、計{2}で購入します。よろしいですか?",
                selectedItem.HumanReadableName,
                HtmlGenerator.GeneralItemCount,
                Coockers.PriceCoocker(購入予定価格));

            JournalingWriter.IsTemporaryStopped = true;
            try
            {
                var result = await UI.YesNoMenuAsync(confirmMessage, "はい", "いいえ");
                if (!result) return null;
            }
            finally
            {
                JournalingWriter.IsTemporaryStopped = false;
            }

            if (Flags.所持金 - 購入予定価格 < 0)
            {
                DefaultPersons.システム.Say("お金が足りません。");
                return null;
            }

            Flags.所持金 -= 購入予定価格;
            State.SetItemCount(selectedItem, State.GetItemCount(selectedItem) + HtmlGenerator.GeneralItemCount);
            State.SetUsedCount(selectedItem, 0);    // 新品未使用!
            return selectedItem;

            bool isBuyable(Item item)
            {
                var price = getPrice(item, 0);
                return Flags.所持金 - price >= 0;
            }

            string createBuyItemLabel(Item item)
            {
                string price = Coockers.PriceCoocker(getPrice(item, 0));
                string name = item.HumanReadableName;
                int taking = State.GetItemCount(item);
                int maxItems = item.Max;
                int availableToBuy = item.Max - State.GetItemCount(item);
                string supply = "";
                if (!isBuyable(item)) supply += "購入不可:所持金不足";
                else if (availableToBuy == 0) supply += $"購入不可:所持可能最大数{maxItems}まで所持";
                else if (taking > 0) supply += $"既に{taking}個所持しています。あと{availableToBuy}個所持可";
                else supply += $"あと{availableToBuy}個所持可";
                if (supply.Length > 0)
                    return $"{price,9} {name} [{supply}]";
                else
                    return $"{price,9} {name}";
            }
        }

        internal static async Task<Item> ConsumeItemMenuAsync()
        {
            return await generalItemListAsync("アイテム選択", HtmlGenerator.EnumConsumeItems, ANGFLib.General.CreateItemConsumeLabelText, false);
        }

        public static string createMiniStatusCss(ANGFLib.MiniStatus sta)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("width:20em;");
            sb.Append("visibility:");
            sb.Append(sta.IsVisible() ? "visible" : "hidden");
            sb.Append(";color:");
            sb.Append(isNotReady ? "#FFFFFF" : ToCssColor(sta.ForeColor()));
            return sb.ToString();
        }
        public static string createMiniStatusText(ANGFLib.MiniStatus sta)
        {
            return isNotReady ? "---" : sta.Text();
        }
        private static async Task closeEquipAsync()
        {
            var eq = EquipMode;
            EquipMode = eq - 1;
            if (EquipMode == EEquipMode.Normal) EnableEquipButton();
            await Task.Delay(0);
        }

        public static string GeneralItemHeader;
        public static Func<IEnumerable<Item>> EnumGeneralItems;
        public static Func<Item, string> CreateGeneralLabel;
        public static int GeneralItemCount;
        public static int GeneralItemCountMax;
        public static bool EnableGeneralItemCount;

        private static async Task openGeneralItemListAsync(string header, Func<IEnumerable<Item>> enumItems, Func<Item, string> createLabel, bool enableGeneralItemCount)
        {
            DisableEquipButton();
            GeneralItemExplanation = "";
            GeneralItemHeader = header;
            EnumGeneralItems = enumItems;
            CreateGeneralLabel = createLabel;
            GeneralItemCount = 1;
            GeneralItemCountMax = 99;   // 本当は動的に更新することが望ましいが暫定実装
            EnableGeneralItemCount = enableGeneralItemCount;
            EquipMode = EEquipMode.GeneralItemList;
            await Task.Delay(0);
        }
        private static async Task closeGeneralItemListAsync()
        {
            EquipMode = EEquipMode.Normal;
            await Task.Delay(0);
        }
        private static async Task<Item> generalItemListAsync(string header, Func<IEnumerable<Item>> enumItems, Func<Item, string> createLabel, bool enableGeneralItemCount)
        {
            await openGeneralItemListAsync(header, enumItems, createLabel, enableGeneralItemCount);
            TriggerUIUpdate();

            Item selectedItem = null;
            try
            {
                source = new System.Threading.CancellationTokenSource();
                //StateHasChanged();
                await Task.Delay(-1, source.Token);
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                // nothing to do
            }
            finally
            {
                selectedItem = SelectedItem;
                await ClearAllAsync();
            }
            return selectedItem;
        }

        private static Item getCurrentItem()
        {
            var item = (string.IsNullOrWhiteSpace(eqItemId) || eqItemId == "null") ? Items.ItemNull : Items.GetItemByNumber(eqItemId);
            return item;
        }

        private static Item getConsumeCurrentItem()
        {
            var item = GeneralItemId == "null" ? Items.ItemNull : Items.GetItemByNumber(GeneralItemId);
            return item;
        }

        public static void GeneralItemSelectionChanged()
        {
            Item item = getConsumeCurrentItem();
            GeneralItemExplanation = item.FullDescription;
        }
        public static async Task GeneralItemOKAsync()
        {
            Item item = getConsumeCurrentItem();
            if (item.IsItemNull)
                SelectedItem = null;
            else
                SelectedItem = item;
            await closeGeneralItemListAsync();
            EnableEquipButton();
            source.Cancel();
        }
        public static async Task GeneralItemCancelAsync()
        {
            await closeGeneralItemListAsync();
            source.Cancel();
        }
        public static IEnumerable<Item> EnumConsumeItems()
        {
            return SimpleName<Item>.List.Values.Where((n) => State.GetItemCount(n) > 0);
        }
        public static void EnableEquipButton()
        {
            IsEquipButtonDisabled = null;
        }
        public static void DisableEquipButton()
        {
            IsEquipButtonDisabled = "disabled";
        }
        public static Dummy myEnableEquipButtonsBody(bool b)
        {
            if (b) EnableEquipButton(); else DisableEquipButton();
            TriggerUIUpdate();
            return default;
        }

        public static async Task mainMenuDoneAsync(int index)
        {
            //if (listMenus[index].Method != null) listMenus[index].Method(UI.Actions.GetMainForm());
            if (listMenus[index].MethodAsync != null) await listMenus[index].MethodAsync(UI.Actions.GetMainForm());
        }

        public static async Task GiftCodeOKClickedAsync()
        {
            bool accepted = false;
            if (GiftCodeTextValue == null) return;
            if (GiftCodeTextValue.Trim() == "debug=1")
            {
                await setDebugAsync(true);
            }
            if (GiftCodeTextValue.Trim() == "debug=0")
            {
                await setDebugAsync(false);
            }
            // ask each module
            foreach (var ex in State.LoadedModulesEx)
            {
                foreach (var item in ex.QueryObjects<GiftCodeProcessor>())
                {
                    accepted |= await item.ProcessAsync(GiftCodeTextValue.Trim());
                }
            }
            // clear text if accepted
            if (accepted)
            {
                GiftCodeTextValue = "";
                TriggerUIUpdate();  // この行はテキストボックスを更新するために必要
            }
            else
            {
                GiftCodeTextValue = "Bad Code: " + GiftCodeTextValue;
                TriggerUIUpdate();  // この行はテキストボックスを更新するために必要
            }

            async Task setDebugAsync(bool mode)
            {
                accepted = true;
                if ( SystemFile.IsDebugMode == mode )
                {
                    await JsWrapper.AlertAsync("既にそのモードです。");
                    return;
                }
                await JsWrapper.AlertAsync("アプリを再起動します");
                SystemFile.IsDebugMode = mode;
                await SystemFile.SaveAsync();
                TriggerReload();
            }
        }

        public static void CreateMainMenu()
        {
            General.StartJournalingPlaybackAsync = async () =>
            {
                JournalPlaybackQueue.Enqueue(new JournalingInputDescripter(ANGFLib.General.TestingAssembly ?? Assembly.GetExecutingAssembly(), Constants.JournalingDirectHeader + HtmlGenerator.JournalingDirectSourceTextValue));
                await startJournalingPlaybackAsync();
            };
            var list = new List<MenuItem>();
            list.Add(new MenuItem()
            {
                Label = "スケジュール確認",
                MethodAsync = async (obj) =>
                {
                    var allList = Schedule.List.Values.Where(c => State.IsScheduleVisible(c.Id)).OrderBy(c => c.StartTime).Select(c => new Tuple<string, string>(c.StartTime.ToString(Constants.DateTimeHumanReadbleFormat), $"{c.HumanReadableName} {c.Description} ({c.StartTime.ToString(Constants.DateTimeHumanReadbleFormat)}-{(c.StartTime + c.Length).ToString(Constants.TimeHumanReadbleFormat)})")).ToArray();
                    await UI.Actions.SimpleListAsync("スケジュール確認", allList);
                },
                IsMenuEnabled = () => Schedule.List.Values.Where(c => State.IsScheduleVisible(c.Id)).Count() > 0
            });
            if (SystemFile.IsDebugMode)
            {
                list.Add(new MenuItem()
                {
                    Label = "ジャーナリングフォルダを開く",
                    MethodAsync = async (obj) =>
                    {
                        System.Diagnostics.Process.Start("explorer.exe", General.GetDataRootDirectory());
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => !General.IsBlazorWebAssembly()
                });
                list.Add(new MenuItem()
                {
                    Label = "ジャーナリング終了",
                    MethodAsync = async (obj) =>
                    {
                        JournalingWriter.Close();
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => JournalingWriter.IsAvailable()
                });
                list.Add(new MenuItem()
                {
                    Label = "ジャーナリング開始",
                    MethodAsync = async (obj) =>
                    {
                        JournalingWriter.Create(Constants.OnlyANJName);
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => !JournalingWriter.IsAvailable()
                });
                list.Add(new MenuItem()
                {
                    Label = "ジャーナリング結果表示",
                    MethodAsync = async (obj) =>
                    {
                        IsJounalResultEnable = true;
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => General.JounalResults != null && General.JounalResults.Length > 0 && IsJounalResultEnable == false
                });
                list.Add(new MenuItem()
                {
                    Label = "ジャーナリング結果非表示",
                    MethodAsync = async (obj) =>
                    {
                        IsJounalResultEnable = false;
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => IsJounalResultEnable
                });
                list.Add(new MenuItem()
                {
                    Label = "テスト用フラグエディタ",
                    MethodAsync = async (obj) =>
                    {
                        HtmlGenerator.IsPopupEnabled = true;
                        SelectedFlagItem = null;
                        var list = new List<FlagEditorInfo>();
                        AutoCollect.WalkAll((field, modid, name) =>
                        {
                            var n = field.FieldType;
                            if (n == typeof(string))
                            {
                                list.Add(new FlagEditorInfo(
                                    list.Count,
                                    $"{field.Name}={field.GetValue(null)} ({modid})",
                                    typeof(string),
                                    (x) => { field.SetValue(null, x); return default; },
                                    () => (string)field.GetValue(null)
                                    ));
                            }
                            else if (n == typeof(int))
                            {
                                list.Add(new FlagEditorInfo(
                                    list.Count,
                                    $"{field.Name}={field.GetValue(null)} ({modid})",
                                    typeof(int),
                                    (x) =>
                                    {
                                        int.TryParse(x, out int y);
                                        field.SetValue(null, y);
                                        return default;
                                    },
                                    () => field.GetValue(null).ToString()
                                    ));
                            }
                            else if (n == typeof(bool))
                            {
                                list.Add(new FlagEditorInfo(
                                    list.Count,
                                    $"{field.Name}={field.GetValue(null)} ({modid})",
                                    typeof(bool),
                                    (x) =>
                                    {
                                        bool.TryParse(x, out bool y);
                                        field.SetValue(null, y);
                                        return default;
                                    },
                                    () => field.GetValue(null).ToString()
                                    ));
                            }
                            else if (n == typeof(FlagCollection<string>))
                            {
                                var coll = ((FlagCollection<string>)field.GetValue(null));
                                foreach (var key in coll.Keys)
                                {
                                    list.Add(new FlagEditorInfo(
                                        list.Count,
                                        $"{field.Name}[{key}]={coll[key]} ({modid})",
                                    typeof(string),
                                    (x) => { coll[key] = x; return default; },
                                    () => coll[key]
                                    ));
                                }
                            }
                            else if (n == typeof(FlagCollection<int>))
                            {
                                var coll = ((FlagCollection<int>)field.GetValue(null));
                                foreach (var key in coll.Keys)
                                {
                                    list.Add(new FlagEditorInfo(
                                        list.Count,
                                        $"{field.Name}[{key}]={coll[key]} ({modid})",
                                    typeof(int),
                                    (x) =>
                                    {
                                        int.TryParse(x, out int y);
                                        coll[key] = y;
                                        return default;
                                    },
                                    () => coll[key].ToString()
                                    ));
                                }
                            }
                            else if (n == typeof(FlagCollection<bool>))
                            {
                                var coll = ((FlagCollection<bool>)field.GetValue(null));
                                foreach (var key in coll.Keys)
                                {
                                    list.Add(new FlagEditorInfo(
                                        list.Count,
                                        $"{field.Name}[{key}]={coll[key]} ({modid})",
                                    typeof(bool),
                                    (x) =>
                                    {
                                        bool.TryParse(x, out bool y);
                                        coll[key] = y;
                                        return default;
                                    },
                                    () => coll[key].ToString()
                                    ));
                                }
                            }
                            return default;
                        });
                        FlagEditorList = list.ToArray();
                        TriggerUIUpdate();
                        await Task.Delay(0);
                    },
                    IsMenuEnabled = () => true
                });
                list.Add(new MenuItem()
                {
                    Label = "コレクションランキング用ファイル生成",
                    MethodAsync = async (obj) =>
                    {
                        byte[] result;
                        using (var ms = new MemoryStream())
                        {
                            // メモリストリーム上にZipArchiveを作成する
                            using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                            {
                                foreach (var mod in State.loadedModules)
                                {
                                    string id = mod.Id;
                                    string str = Util.SaveDefinitionFilesForCollectionRanking(mod);
                                    DefaultPersons.独白.Say($"archiving: {mod.Id}");
                                    var data = Encoding.UTF8.GetBytes(str);
                                    var entry = zipArchive.CreateEntry(id + ".txt");
                                    using (var es = entry.Open())
                                    {
                                        // エントリにバイナリを書き込む
                                        es.Write(data, 0, data.Length);
                                    }
                                }
                            }
                            result = ms.ToArray();
                        }
                        DefaultPersons.独白.Say($"size: {result.Length}");

                        await JsWrapper.DownloadFileFromStreamAsync("wangfRankingArchive" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip", result);
                    },
                    IsMenuEnabled = () => true
                });

            }
            list.Add(new MenuItem()
            {
                Label = "共通フォルダを開く",
                MethodAsync = async (obj) =>
                {
                    System.Diagnostics.Process.Start("EXPLORER.EXE", $"/e,/root,\"{ANGFLib.General.GetCommonRootDirectory()}\"");
                    await Task.Delay(0);
                },
                IsMenuEnabled = () => !General.IsBlazorWebAssembly()
            });
            list.Add(new MenuItem()
            {
                Label = "個人フォルダを開く",
                MethodAsync = async (obj) =>
                {
                    System.Diagnostics.Process.Start("EXPLORER.EXE", $"/e,/root,\"{ANGFLib.General.GetDataRootDirectory()}\"");
                    await Task.Delay(0);
                },
                IsMenuEnabled = () => !General.IsBlazorWebAssembly()
            });
            listMenus = list.Concat(SimpleName<MenuItem>.List.Values).Where(c => c.MenuType == MyMenuType.Top).ToArray();
        }
        internal static bool IsAdEnabled()
        {
            return Is18K != null;
        }
        internal static AdInfo GetNextAd()
        {
            if (Is18K == null) return null;
            return AdBlock.GetNextAd(Is18K.Value);
        }

        public static async Task FlagInputSETClickedAsync()
        {
            SelectedFlagItem.setValue(FlagInputTextValue);
            SelectedFlagItem = null;
            IsPopupEnabled = false;
            await Task.Delay(0);
        }

        internal static async Task FlagEditAsync(int index)
        {
            TriggerUIUpdate();
            await Task.Delay(100);
            HtmlGenerator.IsPopupEnabled = true;
            SelectedFlagItem = FlagEditorList[index];
            FlagInputTextValue = SelectedFlagItem.getValue();
        }

        public static async Task<DateTime> OpenDateTime(DateTime initialValue, int initialHour, int initialMinutes, string[] options = null)
        {
            EditingDateTime = initialValue;
            ResultDateTime = DateTime.MinValue;
            Hour = initialHour;
            Minutes = initialMinutes;
            DateTimeOptions = options;
            DateTimeOption = 0;
            IsPopupEnabled = true;
            TriggerUIUpdate();
            //wait for completion
            source = new System.Threading.CancellationTokenSource();
            try
            {
                await Task.Delay(-1, source.Token);
            }
            catch (TaskCanceledException)
            {
                // ignore it
            }
            // create result
            if (initialHour >= 0)
            {
                Hour = Math.Max(Math.Min(Hour, 23), 0);
                Minutes = Math.Max(Math.Min(Minutes, 59), 0);
                return new DateTime(ResultDateTime.Year, ResultDateTime.Month, ResultDateTime.Day, Hour, Minutes, 0);
            }
            return ResultDateTime.Date;
        }

        public static async Task DateTimeSETClickedAsync()
        {
            ResultDateTime = EditingDateTime;
            EditingDateTime = DateTime.MinValue;
            DateTimeOptions = null;
            IsPopupEnabled = false;
            source.Cancel();    // continue to run scenario thread
            await Task.Delay(0);
        }
        public static async Task DateTimeCanceldAsync()
        {
            ResultDateTime = DateTime.MinValue;
            EditingDateTime = DateTime.MinValue;
            DateTimeOptions = null;
            IsPopupEnabled = false;
            source.Cancel();    // continue to run scenario thread
            await Task.Delay(0);
        }

        //***********************
        // EQUIPMENT UI FEATURES
        //***********************

        // 画面モードの指定
        public static EEquipMode EquipMode = EEquipMode.Normal;
        // 選択対象者のPerson IDを入れる。
        // あくまで選択を補助するガイド情報でありその人の装備情報に直接データを入れるのはナシ
        public static string CurrentEquipPersonGiudId;
        public static int CurrentEquipIndex;    // 選択部位
        public static string IsEquipButtonDisabled;
        public static bool IsEquipButtonEnabledForDirectAPI;
        public static string eqItemExplanation; // アイテム説明文
        public static string eqItemId { get; private set; } // リスト選択アイテム
        public static EquipSet candUIEquip = new EquipSet(); // 一時装備アイテムプール
        public static EquipSet orgUIEquip = new EquipSet(); // 初期状態の装備アイテムプール
        public static bool IsDirectEquipInvoked = false;

        public static void eqItemSelectionChanged(ChangeEventArgs e)
        {
            eqItemId = (string)(e.Value ?? "");
            Item item = getCurrentItem();
            ItemArray.SetOperation(item, CurrentEquipIndex, (index) => candUIEquip.AllItems[index], (index, item) =>
            {
                candUIEquip.AllItems[index] = item;
                return new Dummy();
            });
            eqItemExplanation = item.FullDescription;
        }

        private static async Task openEquipAsync(bool isDirectEquipInvoked)
        {
            await ClearAllAsync();
            DisableEquipButton();
            IsDirectEquipInvoked = isDirectEquipInvoked;
            eqItemId = Items.ItemNull.Id;
            eqItemExplanation = "";
            var eq = EquipMode;
            EquipMode = eq + 1;
            TriggerUIUpdate();
            await Task.Delay(0);
        }

        // run in thenario thread
        public static async Task OpenEquipAsync(EquipSet old, int initialIndex)
        {
            CurrentEquipIndex = initialIndex;
            candUIEquip = old.Duplicate();
            orgUIEquip = old.Duplicate();
            await openEquipAsync(false);
        }

        private static async Task<bool> eqItemOKSubForDirectAsync()
        {
            Item item = getCurrentItem();

            if (!item.IsItemNull)
            {
                var ar = EquipType.List.Values.OrderBy(c => c.Priority).ToArray();
                for (int i = 0; i < ar.Length; i++)
                {
                    // 対象部位の場合はチェックしない
                    if (i == CurrentEquipIndex) continue;

                    // 同時に装備できる箇所もチェックから外す
                    // 同じアイテムは同じ場所にしか装備できないのでこれで十分である
                    if (item.SameTimeEquipMap.Length > i && item.SameTimeEquipMap[i]) continue;

                    // そのアイテムが別部位に装備されている
                    if (item.Id == candUIEquip.AllItems[i].Id)
                    {
                        await wangfUtil.MessageBoxAsync(item.HumanReadableName + "は既に" + ar[i].Name + "に装備済み!");
                        return false;
                    }
                }
            }

            string reason = item.CanEquip(CurrentEquipIndex);
            if (reason != null)
            {
                await wangfUtil.MessageBoxAsync(reason);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CurrentEquipPersonGiudId))
            {
                string reason2 = item.CanEquipEx(CurrentEquipIndex, CurrentEquipPersonGiudId, candUIEquip);
                if (reason2 != null)
                {
                    await wangfUtil.MessageBoxAsync(reason2);
                    return false;
                }
            }

            //Flags.Equip.TargetPersonId = CurrentEquipPersonId;
            // 複数対処のアイテムの処理も配慮する
            ItemArray.SetOperation(item, CurrentEquipIndex, (index) => candUIEquip.AllItems[index], (index, item) =>
            {
                candUIEquip.AllItems[index] = item;
                return new Dummy();
            });

            var validator = General.EquipCustomValudation(CurrentEquipPersonGiudId);
            if (validator != null)
            {
                string s = validator(candUIEquip);
                if (s != null)
                {
                    DefaultPersons.システム.Say(s);
                    return false;
                }
            }

            return true;
        }

        public static async Task setEquipSetAsync(string personId, string equipSetName)
        {
            var old = Flags.Equip.TargetPersonId;
            try
            {
                candUIEquip = General.装備品情報のコピー();
                orgUIEquip = General.装備品情報のコピー();
                HtmlGenerator.messages.Clear();
                var items = State.LoadEquipSetIfItemExist(personId, equipSetName);
                if (items.Length > 0) DefaultPersons.システム.Say($"{string.Join(", ", items.Select(c => c.HumanReadableName))}が足りません。");
                candUIEquip = General.装備品情報のコピー();
                // assumes candUIEquip has a correct items
                journalEquipmentDiff(candUIEquip, orgUIEquip);
                HtmlGenerator.TriggerUIUpdate();
                await Task.Delay(0);
                return;
            }
            finally
            {
                Flags.Equip.TargetPersonId = old;
            }
        }

        public static async Task eqItemOKAsync()
        {
            if (IsDirectEquipInvoked)
            {
                var old = Flags.Equip.TargetPersonId;
                try
                {
                    Flags.Equip.TargetPersonId = CurrentEquipPersonGiudId;
                    bool b = await eqItemOKSubForDirectAsync();
                    if (b)
                    {
                        // ダイレクトUIからキックされている場合は
                        // その人物の装備を更新しなければならない
                        for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
                        {
                            Flags.Equip[i] = candUIEquip.AllItems[i].Id;
                        }
                        // assumes candUIEquip has a correct items
                        journalEquipmentDiff(candUIEquip, orgUIEquip);
                    }
                    else
                    {
                        candUIEquip = null;
                    }
                }
                finally
                {
                    Flags.Equip.TargetPersonId = old;
                    await closeEquipAsync();
                    source.Cancel();
                }
            }
            else
            {
                // APIから起動された場合はcandUIEquipだけ書き換えて返す
                // 具体的な装備情報に反映させてはならない。
                bool b = await eqItemOKSubForDirectAsync();
                if (b)
                {
                    // assumes candUIEquip has a correct items
                    journalEquipmentDiff(candUIEquip, orgUIEquip);
                }
                else
                {
                    candUIEquip = null;
                }
                await closeEquipAsync();
                source.Cancel();
            }
        }

        public static async Task eqItemCancelAsync()
        {
            await closeEquipAsync();
            candUIEquip = null;
            source.Cancel();
        }

        public static async Task EpuipDigestListAreaDoneAsync(string personId, int equipIndex)
        {
            CurrentEquipPersonGiudId = personId;
            CurrentEquipIndex = equipIndex;
            var equipTypes = EquipType.List.Values.OrderBy(c => c.Priority).ToArray();
            var memberIds = Party.EnumPartyMembers();
            candUIEquip = new EquipSet();
            orgUIEquip = new EquipSet();
            var old = Flags.Equip.TargetPersonId;
            try
            {
                Flags.Equip.TargetPersonId = personId;
                for (int i = 0; i < equipTypes.Length; i++)
                {
                    //Person member = Person.List[CurrentEquipPersonGiudId];
                    //var partyMember = ((ANGFLib.IPartyMember)member).GetEquippedItemIds().ElementAt(i);
                    var item = ANGFLib.Items.GetItemByNumber(Flags.Equip[i]);
                    candUIEquip.AllItems[i] = item;
                    orgUIEquip.AllItems[i] = item;
                }
            }
            finally
            {
                Flags.Equip.TargetPersonId = old;
            }
            await openEquipAsync(true);
        }
        //public static async Task EpuipFullListAreaDoneAsync(int equipIndex)
        //{
        //CurrentEquipIndex = equipIndex;
        //await openEquip();
        //}
        //public static async Task EpuipFullListAreaCloseAsync()
        //{
        //await closeEquip();
        //}

        public static string CreateEquipStatusString(Item item)
        {
            if (isNotReady) return "----";
            if (item.IsItemNull) return "";
            return item.HumanReadableName;
        }

        private static void journalEquipmentDiff(EquipSet newset, EquipSet oldset)
        {
            // 変更差分があるか調べる
            bool diff = false;
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                string id1 = oldset.AllItems[i].Id ?? "";
                string id2 = newset.AllItems[i].Id ?? "";
                if (id1 == id2) continue;
                diff = true;
                break;
            }
            if (!diff) return;

            JournalingWriter.Write("M", CommonCommandNames.SYSTEM);
            JournalingWriter.Write("M", CommonCommandNames.装備);
            // 変更差分をジャーナリングする
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                string id1 = oldset.AllItems[i].Id ?? "";
                string id2 = newset.AllItems[i].Id ?? "";
                if (id1 != id2) JournalingWriter.WriteEquip("EQ", i, Items.GetItemByNumber(Flags.Equip[i]));
            }
        }

        public static void EquipTypeSelectionChanged(ChangeEventArgs e)
        {
            int found = -1;
            var list = EquipType.List.Values.OrderBy(c => c.Priority).ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].HumanReadableName == e.Value.ToString())
                {
                    found = i;
                    break;
                }
            }
            if (found < 0)
            {
                Console.WriteLine($"selected {e.Value} not found");
                return;
            }
            CurrentEquipIndex = found;
            eqItemId = Items.ItemNull.Id;
            eqItemExplanation = "";
            HtmlGenerator.TriggerUIUpdate();
        }

        public static void FixCurrentEquipIndex()
        {
            bool notNeedToFix = false;
            var list = ANGFLib.EquipType.List.Values.OrderBy(c => c.Priority).ToArray();
            for (int i = 0; i < list.Count(); i++)
            {
                if (ANGFLib.Flags.IsExpandEquip || !list[i].IsVisibleIfExpanded)
                {
                    if (i == HtmlGenerator.CurrentEquipIndex)
                    {
                        notNeedToFix = true;
                        break;
                    }
                }
            }
            if (notNeedToFix) return;
            bool firstItem = true;
            for (int i = 0; i < list.Count(); i++)
            {
                if (ANGFLib.Flags.IsExpandEquip || !list[i].IsVisibleIfExpanded)
                {
                    if (firstItem)
                    {
                        firstItem = false;
                        // 項目の一部が隠されているときに現在位置を自動補正する
                        HtmlGenerator.CurrentEquipIndex = i;
                        break;
                    }
                }
            }
        }

        public static void SetEqItemId(string id)
        {
            eqItemId = id;
        }

        private RenderFragment CreateComponent() => builder =>
        {
            builder.OpenComponent(0, HtmlGenerator.TypeOfCustomComponent);
            builder.CloseComponent();
        };
        internal static async Task<object> OpenCustomRazorComponentAsync(Type type)
        {
            source = new System.Threading.CancellationTokenSource();
            TypeOfCustomComponent = type;
            useCustomComponent = true;
            TriggerUIUpdate();
            try
            {
                await Task.Delay(-1, source.Token);
            }
            catch (TaskCanceledException)
            {
                // empty
            }
            var r = HtmlGenerator.ResultOfCustomComponent;
            HtmlGenerator.ResultOfCustomComponent = null;
            return r;
        }
        internal static void CloseCustomRazorComponent(object obj)
        {
            useCustomComponent = false;
            TypeOfCustomComponent = null;
            ResultOfCustomComponent = obj;
            TriggerUIUpdate();
            source.Cancel();
        }

        internal static string ExceptionWarnText
        {
            get
            {
                if (General.IsBlazorWebAssembly()) return "ページをリロード";
                return "アプリを閉じてから起動";
            }
        }

        static HtmlGenerator()
        {
            ANGFLib.UI.EnableEquipButtonsBody = myEnableEquipButtonsBody;
        }
    }
}
