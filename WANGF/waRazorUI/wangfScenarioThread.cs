using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ANGFLib;
using wangflib;

namespace waRazorUI
{
    [Obsolete]
    public class FormMain : Form { }
    [Obsolete]
    public class Form { }

    public class ScenarioThread
    {
        private const bool 既読スキップ = true;
        private static object formMain = null;
        private static object formEquiop = null;
        public static object GetMainForm() => formMain;
        public static object GetEquipForm() => formEquiop;
        private static UIActionSet humanActions = new UIActionSet();

        static ScenarioThread()
        {
            General.CallToProgressStatusAsync = ProgressStatusAsync;/* DIABLE ASYNC WARN */
            General.CallToRestoreActionSetAsync = RestoreActionSetAsync;/* DIABLE ASYNC WARN */
            General.CallToTellAssertionFailedAsync = TellAssertionFailedAsync;/* DIABLE ASYNC WARN */
        }

        private static async Task goNextIfBatchTestingAsync()
        {
            var name = string.Join(',', State.loadedModules.Reverse().Select (c=>c.GetXmlModuleData().Name));
            var result = string.Join('/', HtmlGenerator.messages.Select(c=>c.Message));
            await BatchTest.BatchTestingForEachTitle.EndBatchTestingAsync(JournalingDocumentPlayer.IsSuccess, name, result);
        }

        public static async Task RestoreActionSetAsync()
        {
            UI.Actions = humanActions;
            SystemFile.EndPlayback();
            DateTime endDateTime = DateTime.Now;
            ANGFLib.SystemFile.IsDebugMode = true;
            // ここで本来ならSystemFileを保存すべきだが
            // できないので、メインループ先頭のSaveIfDirtyに任せる
            DefaultPersons.システム.Say(ANGFLib.Constants.スキップ例外Prefix
            + string.Format("{0}", endDateTime - Util.JounaligStartDateTime));
            await goNextIfBatchTestingAsync();
        }
        public static async Task TellAssertionFailedAsync(string message)
        {
            await humanActions.tellAssertionFailedAsync(message);
        }
        public static async Task ProgressStatusAsync(string message)
        {
            await humanActions.progressStatusAsync(message);
        }
        public static void ResetGameStatus()
        {
            humanActions.ResetGameStatus();
        }

        /// <summary>
        /// 自動セーブを行うディレクトリを返す
        /// </summary>
        private static string automaticSaveDirectory
        {
            get => "autosave";
        }

#if false
        private static bool checkSub(string checksum, byte[] image)
        {
            var sum = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] bs = sum.ComputeHash(image);

            for (int i = 0; i < bs.Length; i++)
            {
                uint val = Convert.ToUInt32(checksum.Substring(i * 2, 2), 16);
                if (bs[i] != val) return false;
            }
            return true;
        }
#endif

        /// <summary>
        /// システムファイルのファイル名です。
        /// </summary>
        public static string SystemFileFileName
        {
            get => "systemFile.bin";
        }
        // ちょっとトリック多いので注意
        public static string SkipFileFileName
        {
            get => "skip.bin";
        }

        private static async Task<bool> useCloudStorageAsync()
        {
            bool result = false;
#if false
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                result = await wangfUtil.GetUseCloudStorageAsync();
            });
#else
            await Task.Delay(0);
#endif
            return result;
        }

        private static async Task<string> fileSelectCommonAsync(object form, Func<object, string[]> getter, string caption)
        {
            string[] ar = getter(form);
            if (ar == null) return null;
            var list = ar.Select(c => new SimpleMenuItem(c)).ToArray();
            var index = await UI.SimpleMenuWithCancelAsync(caption, list);
            if (index < 0) return null;
            return ar[index];
        }

        //private static void callAsync(Func<Task> actiton)
        //{
        //System.Diagnostics.Debug.WriteLine("callAsync called");
        //Microsoft.AspNet.Identity.AsyncHelper.RunSync(actiton);
        //}

        private static int parseAsIntSub(Dictionary<string, string> dic, string key)
        {
            string s;
            dic.TryGetValue(key, out s);
            if (s == null) return -1;
            int n;
            int.TryParse(s, out n);
            return n;
        }


        public static void InitUIActions()
        {
            humanActions.ResetGameStatus = delegate ()
            {
                // EMPTY method
                return default;
            };
            humanActions.ResetDisplay = delegate ()
            {
                // EMPTY method
                return default;
            };
            humanActions.messageOutputMethod = (Person talker, string message)=>
            {
                System.Diagnostics.Debug.Assert(message != null);
                System.Diagnostics.Debug.Assert(talker != null);
                waRazorUI.HtmlGenerator.ShowMessage(talker, message);
                return default;
            };
            humanActions.simpleMenuMethodAsync = async (string prompt, SimpleMenuItem[] commonItems, SimpleMenuItem systemMenu) =>
            {
                // clear real-time messages
                HtmlGenerator.RealtimeStatusMessage = "";
                // do menu
                int resultCode = -1;
                var items = commonItems;
                if (systemMenu != null) items = commonItems.Append(systemMenu).ToArray();

                await waRazorUI.HtmlGenerator.CreateMenuItemsAsync(prompt, items, (n) =>
                {
                    UI.Actions.SetPictureUrl(null);
                    resultCode = n;
                    if (systemMenu != null && n == items.Length - 1) resultCode = -1;   // if System Menu
                    return default;
                });
                // clear progress messages
                HtmlGenerator.ProgressMessage = "";

                // ジャーナリングファイルのプレイバックを開始するためのトリガ
                if (resultCode == -2)
                {
                    // この時点で、既にActionsはプレイバック用に置き換わっているはず
                    // なので、それを使わせる
                    return await UI.Actions.simpleMenuMethodAsync(prompt, items, systemMenu);
                }

                JournalingWriter.Write("M", resultCode == -1 ? systemMenu.Name : items[resultCode].Name);

                if (resultCode == -1)
                {
                    if (systemMenu != null && systemMenu.SimpleMenuAction != null) systemMenu.SimpleMenuAction();
                    if (systemMenu != null && systemMenu.SimpleMenuActionAsync != null) await systemMenu.SimpleMenuActionAsync();
                }
                else
                {
                    if (items[resultCode].SimpleMenuAction != null) items[resultCode].SimpleMenuAction();
                    if (items[resultCode].SimpleMenuActionAsync != null) await items[resultCode].SimpleMenuActionAsync();
                }
                return resultCode;
            };

            humanActions.saveFileNameAsync = async (string prompt) =>
            {
                if (await useCloudStorageAsync())
                {
                    // TBW
                    System.Diagnostics.Trace.Fail("Cloud Storage not implemented");
                    return null;
                }
                else
                {
                    var items = await wangfUtil.CreateFileListAsync(FileLayout.CreateSaveFileName, true);
                    var r = await UI.SimpleMenuWithCancelAsync(prompt, items);
                    if (r < 0) return null;
                    return FileLayout.CreateSaveFileName((int)items[r].UserParam);
                }
            };
            humanActions.loadFileNameWithExtentionAsync = async (string prompt, string ext) =>
            {
                if (await useCloudStorageAsync())
                {
                    // TBW
                    System.Diagnostics.Trace.Fail("Cloud Storage not implemented");
                    return null;
                }
                else
                {
                    var items = await wangfUtil.CreateFileListAsync(FileLayout.CreateSaveFileName, false);
                    var r = await UI.SimpleMenuWithCancelAsync(prompt, items);
                    if (r < 0) return null;
                    return FileLayout.CreateSaveFileName((int)items[r].UserParam);
                }
            };
            humanActions.loadFileNameFromAutoSaveAsync = async (string prompt) =>
            {
                if (await useCloudStorageAsync())
                {
                    // TBW
                    System.Diagnostics.Trace.Fail("Cloud Storage not implemented");
                    return null;
                }
                else
                {
                    var items = await wangfUtil.CreateFileListAsync(FileLayout.CreateAutoSaveFileName, false);
                    var r = await UI.SimpleMenuWithCancelAsync(prompt, items);
                    if (r < 0) return null;
                    return FileLayout.CreateAutoSaveFileName((int)items[r].UserParam);
                }
            };
            humanActions.enterPlayerNameAsync = async (string prompt, string defaultValue) =>
            {
                string result = await waRazorUI.HtmlGenerator.EnterPlayerNameAsync(prompt, defaultValue);
                JournalingWriter.Write("N", result);
                return result;
            };
            humanActions.consumeItemMenuAsync = async () =>
            {
                Item selectedItem = await waRazorUI.HtmlGenerator.ConsumeItemMenuAsync();
                if (selectedItem == null) return false;

                JournalingWriter.Write("C", selectedItem.HumanReadableName);

                if (selectedItem.IsConsumeItem)
                {
                    bool isConsumed = await selectedItem.消費Async();
                    if (isConsumed)
                    {
                        State.SetItemCount(selectedItem, State.GetItemCount(selectedItem) - 1);
                    }
                    return isConsumed;
                }
                else
                {
                    DefaultPersons.システム.Say("{0}を使いました。", selectedItem.HumanReadableName);
                    DefaultPersons.システム.Say("しかし何も起こりませんでした。");
                    return true;
                }
            };

            humanActions.selectOneItemAsync = async () =>
            {
                Item selectedItem = await waRazorUI.HtmlGenerator.ConsumeItemMenuAsync();
                if (selectedItem == null) return "";

                JournalingWriter.Write("AI", selectedItem.HumanReadableName);

                return selectedItem.Id;
            };

            humanActions.shopSellMenuAsync = async (GetPriceInvoker getPrice) =>
            {
                Tuple<Item, int> selectedItem = await waRazorUI.HtmlGenerator.SellItemMenuAsync(getPrice);
                if (selectedItem == null) return false;
                JournalingWriter.Write("SS", HtmlGenerator.GeneralItemCount, selectedItem.Item1.HumanReadableName);
                General.SoldNotifyAll(selectedItem.Item1, selectedItem.Item2);
                return true;
            };

            humanActions.shopBuyMenuAsync = async (Item[] sellingItems, GetPriceInvoker getPrice) =>
            {
                Item selectedItem = await waRazorUI.HtmlGenerator.BuyItemMenuAsync(sellingItems, getPrice);
                if (selectedItem == null) return false;
                JournalingWriter.Write("SB", HtmlGenerator.GeneralItemCount, selectedItem.HumanReadableName);
                return true;
            };
            // 任意の装備セットの更新UI
            humanActions.equipMenuExAsync = async(equipSet, personId, customValidator) =>
            {
                // メッセージがあればそれを先に見せる
                if (HtmlGenerator.messages.Count() > 0)
                {
                    await humanActions.simpleMenuMethodAsync("装備開始ボタンを押してください", new SimpleMenuItem[] { new SimpleMenuItem("装備開始") }, null);
                }
                HtmlGenerator.CurrentEquipPersonGiudId = personId;
                int defaultIndex = 0;
                foreach (var item in State.LoadedModulesEx)
                {
                    var r = item.QueryObjects<DefaultEquipIndexGetter>();
                    if( r.Length > 0 )
                    {
                        defaultIndex = r[0].GetDefaultEquipIndex();
                        break;
                    }
                }

                await HtmlGenerator.OpenEquipAsync(equipSet, defaultIndex);
                try
                {
                    HtmlGenerator.source = new System.Threading.CancellationTokenSource();
                    await Task.Delay(-1, HtmlGenerator.source.Token);
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // nothing to do
                }
                if (customValidator != null)
                {
                    var r = customValidator(HtmlGenerator.candUIEquip);
                    if (r != null)
                    {
                        DefaultPersons.システム.Say(r);
                        return null;
                    }
                }
                return HtmlGenerator.candUIEquip;
            };
            humanActions.sleepFlashAsync = async () =>
            {
                //await waRazorUI.HtmlGenerator.FlashScreen(0, 0, 0);
                HtmlGenerator.messages.Add(new MessageBlock("zzzzzzzzzzz", System.Drawing.Color.FromArgb(255, 255, 255), System.Drawing.Color.FromArgb(0, 0, 0)));
                await Task.Delay(0);
                return true;
            };
            humanActions.WhiteFlashAsync = async () =>
            {
                //await waRazorUI.HtmlGenerator.FlashScreen(255, 255, 255);
                HtmlGenerator.messages.Add(new MessageBlock("!!!!!!!!!!!!!!", System.Drawing.Color.FromArgb(0, 0, 0), System.Drawing.Color.FromArgb(255, 255, 255)));
                await Task.Delay(0);
                return true;
            };
            humanActions.tellAssertionFailedAsync = async (string message) =>
            {
                waRazorUI.HtmlGenerator.ShowMessage(DefaultPersons.システム, message);
                await Task.Delay(0);
                //await wangfUtil.MessageBox(message);
            };
            humanActions.progressStatusAsync = async (string message) =>
            {
                await waRazorUI.HtmlGenerator.SetProgressMessageAsync(message);
            };
            humanActions.砂時計セット実行Async = async (bool on) =>
            {
                await UI.Actions.CallAnyMethodAsync(async () =>
                {
                    if (on)
                        await JsWrapper.SetWaitCursorAsync();
                    else
                        await JsWrapper.ResetWaitCursorAsync();
                });
            };
            humanActions.ExportFileName実行 = (filename, title) =>
            {
                System.Diagnostics.Trace.Fail("ExportFileName実行 not implemented");
                return string.Empty;
            };
            humanActions.ImportFileName実行 = (filename, title) =>
            {
                System.Diagnostics.Trace.Fail("ImportFileName実行 not implemented");
                return string.Empty;
            };
            humanActions.ShowTotalReportAsync = async () =>
            {
                await ShowReportAsync(General.GetTotalReport());
            };
            humanActions.DownloadTotalReportAsync = async () =>
            {
                var all = General.GetTotalReport();
                var result = Encoding.UTF8.GetBytes(all);
                await JsWrapper.DownloadFileFromStreamAsync("GameReport" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", result);
            };
            humanActions.InvokeCollectionListAsync = async (id, unlocked, okEnable) =>
            {
                var bld = new CollectionListBuilder(id, unlocked, okEnable);
                var list = new List<SimpleMenuItem>();
                // create module list
                var moduleIds = new List<string>();
                bld.Build((collection, rootVisible, subCollection, visibleState) =>
                {
                    if (!moduleIds.Contains(collection.OwnerModuleId)) moduleIds.Add(collection.OwnerModuleId);
                    return default;
                });

                // output each modules
                // 後から登録されたモジュールを先に見せた方が自然だからReverse
                foreach (var moduleId in moduleIds.ToArray().Reverse())
                {
                    string name = moduleId;
                    var mod = State.loadedModules.FirstOrDefault(c => c.Id == moduleId);
                    if (mod != null) name = mod.GetAngfRuntimeXml().name;
                    DefaultPersons.独白.Say($"MODULE [[{name}]]");
                    bld.Build((collection, rootVisible, subCollection, visibleState) =>
                    {
                        if (collection.OwnerModuleId != moduleId) return default;
                        string name;
                        object para;
                        if (subCollection == null)
                        {
                            name = collection.Name;
                            para = collection;
                        }
                        else
                        {
                            name = collection.Name + "/" + subCollection.Name;
                            para = subCollection;
                        }
                        var mod = State.loadedModules.FirstOrDefault(c => c.Id == collection.OwnerModuleId);
                        if (mod != null) name += " [" + mod.GetAngfRuntimeXml().name + "]";
                        var own = "　";
                        if (visibleState == State.CollectionState.Own) own = "●";
                        if (visibleState == State.CollectionState.Own && okEnable)
                        {
                            var item = new SimpleMenuItem("[" + own + "] " + name, null, para);
                            list.Add(item);
                        }
                        else if (visibleState == State.CollectionState.None)
                        {
                            var rootName = "???";
                            if (rootVisible) rootName = collection.Name;

                            if (SystemFile.IsDebugMode)
                            {
                                if (subCollection == null)
                                    DefaultPersons.独白.Say($"[　] {rootName} ({collection.Name})");
                                else
                                    DefaultPersons.独白.Say($"[　] {rootName}/??? ({collection.Name}/{subCollection.Name})");
                            }
                            else
                            {
                                if (subCollection == null)
                                    DefaultPersons.独白.Say($"[　] {rootName}");
                                else
                                    DefaultPersons.独白.Say($"[　] {rootName}/???");
                            }
                        }
                        else
                        {
                            DefaultPersons.独白.Say("[" + own + "] " + name);
                        }
                        return default;
                    });
                }
                var r = await UI.SimpleMenuWithCancelAsync("", list.ToArray());
                if (r < 0) return null;
                return list[r].UserParam as CollectionItem;
            };
            humanActions.ChangeCycleAsync = async () =>
            {
                return await ChangeCycleUI.ChangeCycleAsync();
            };
            humanActions.CallAnyMethodAsync = async (x) =>
            {
                dynamic dform = ScenarioThread.GetMainForm();
                await dform.CallAnyMethodAsync(x);
            };
            humanActions.GetMainForm = () => formMain;
            humanActions.IsSkipping = () =>
            {
                // We don't have skipping function in Blazor Version
                return false;   // dummy
            };
            humanActions.LoadFileAsync = async (Category, Name) =>
            {
                if (await useCloudStorageAsync())
                {
                    // TBW
                    System.Diagnostics.Trace.Fail("Cloud Storage not implemented");
                    return new byte[0];
                    //return WebApiWrapper.LoadFile(formMain, Category, Name);
                }
                else
                {
                    try
                    {
                        if (Category == "SystemFile")
                        {
                            //return File.ReadAllBytes(ScenarioThread.SystemFileFileName);
                            return await loadCommonAsync(ScenarioThread.SystemFileFileName);
                        }
                        else if (Category == "skip")
                        {
                            //return File.ReadAllBytes(ScenarioThread.SkipFileFileName);
                            return await loadCommonAsync(ScenarioThread.SkipFileFileName);
                        }
                        else
                        {
                            //return File.ReadAllBytes(Name);
                            return await loadCommonAsync(Name);
                        }
                    }
                    catch (Exception)
                    {
                        return new byte[0];
                    }
                }
                //return new byte[0]; // dummy
            };
            humanActions.SaveFileAsync = async (Category, Name, Body) =>
            {
                if (await useCloudStorageAsync())
                {
                    // TBW
                    System.Diagnostics.Trace.Fail("Cloud Storage not implemented");
                    return null;
                    //return WebApiWrapper.SaveFile(formMain, Category, Name, Body);
                }
                else
                {
                    try
                    {
                        if (Category == "SystemFile")
                        {
                            await saveCommonAsync(Body, ScenarioThread.SystemFileFileName);
                        }
                        else if (Category == "skip")
                        {
                            await saveCommonAsync(Body, ScenarioThread.SkipFileFileName);
                        }
                        else if (Category == "AUTO")
                        {
                            for (int i = 8; i >= 0; i--)
                            {
                                var src = FileLayout.CreateAutoSaveFileName(i);
                                var srcd = FileLayout.CreateDescFileName(src);
                                var dst = FileLayout.CreateAutoSaveFileName(i + 1);
                                var dstd = FileLayout.CreateDescFileName(dst);
                                var dat = await wangfUtil.LocalStorage.GetItemAsync(src);
                                var datd = await wangfUtil.LocalStorage.GetItemAsync(srcd);
                                if (dat != null && datd != null)
                                {
                                    await wangfUtil.LocalStorage.SetItemAsync(dst, dat);
                                    await wangfUtil.LocalStorage.SetItemAsync(dstd, datd);
                                }
                            }
                            await saveCommonAsync(Body, FileLayout.CreateAutoSaveFileName(0), true, true);
                        }
                        else
                        {
                            await saveCommonAsync(Body, Name, false, true);
                        }
                        return Name;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            };
            humanActions.AutoSaveFileAsync = async () =>
            {
                string filename;
                //if (useCloudStorage())
                //    filename = General.GenerateSuggestedFileNameWithoutExt(true);
                //else
                //filename = System.IO.Path.Combine(automaticSaveDirectory, General.GenerateSuggestedFileName(true));

                filename = "DUMMY"; // dummy file name not used

                string result = null;
                await UI.Actions.CallAnyMethodAsync(async () =>
                {
                    try
                    {
                        await State.SaveAsync("AUTO", filename);
                    }
                    catch (Exception e)
                    {
                        result = e.ToString();
                    }
                });
                if (result != null)
                {
                    UI.M(DefaultPersons.システム, result);
                    return false;
                }
                DefaultPersons.システム.Say("自動セーブを完了しました。");
                return true;
            };
            humanActions.SimpleListAsync = async (title, items) =>
            {
                HtmlGenerator.IsPopupEnabled = true;
                HtmlGenerator.SimpleListTitle = title;
                HtmlGenerator.SimpleList = items;
                waRazorUI.HtmlGenerator.TriggerUIUpdate();
                await Task.Delay(0);
            };
            humanActions.WebSplashAsync = async (htmlFragment) =>
            {
                HtmlGenerator.splashMarkup = htmlFragment;
                try
                {
                    waRazorUI.HtmlGenerator.TriggerUIUpdate();
                    await UI.SimpleMenuWithoutSystemAsync("", new SimpleMenuItem[] { new SimpleMenuItem("次へ") });
                }
                finally
                {
                    HtmlGenerator.splashMarkup = null;
                }
            };
            humanActions.SetPictureUrl = (url) =>
            {
                HtmlGenerator.PictureUrl = url;
                return default;
            };
            humanActions.HtmlFormAsync = async (title, htmlForm) =>
            {
                var rdic = new Dictionary<string, string>();
                HtmlGenerator.myMarkup = htmlForm;
                waRazorUI.HtmlGenerator.earlyDoneAsync = async (index) =>
                {
                    if (index == 0)
                    {
                        rdic.Add("OK", "true");
                        await JsWrapper.CollectResultAsync(rdic);
                    }
                };
                try
                {
                    waRazorUI.HtmlGenerator.TriggerUIUpdate();
                    JournalingWriter.IsTemporaryStopped = true;
                    try
                    {
                        await UI.SimpleMenuWithoutSystemAsync("", new SimpleMenuItem[] { new SimpleMenuItem("OK") });
                    }
                    finally
                    {
                        JournalingWriter.IsTemporaryStopped = false;
                    }
                }
                finally
                {
                    HtmlGenerator.myMarkup = null;
                    waRazorUI.HtmlGenerator.earlyDoneAsync = null;/* DIABLE ASYNC WARN */
                }
                return rdic;
            };
            humanActions.UpgradeCheck = (msg, checkSams) =>
            {
                System.Diagnostics.Trace.Fail("humanActions.UpgradeCheck not implemented");
                return false;
            };
            humanActions.GetAccessToken = () =>
            {
                // TBW
                System.Diagnostics.Trace.Fail("humanActions.GetAccessToken not implemented");
                string r = null;
                //AutoResetEvent localBlockEvent = new AutoResetEvent(false);
                //formMain.Invoke((MethodInvoker)delegate ()
                //{
                //formMain.GetAccessToken((accessToken) =>
                //{
                //r = accessToken;
                //localBlockEvent.Set();
                //});
                //});
                //localBlockEvent.WaitOne();
                return r;
            };
            humanActions.NotifyStatusMessageAsync = async (msg) =>
            {
                HtmlGenerator.RealtimeStatusMessage = msg;
                HtmlGenerator.TriggerUIUpdate();
                await Task.Delay(1);
            };
            humanActions.DoEnterDateAsync = async (initialValue, minValue, maxValue) =>
            {
                var date = await HtmlGenerator.OpenDateTime(initialValue, -1, -1);
                HtmlGenerator.TriggerUIUpdate();
                if (date == DateTime.MinValue) return null;
                if (date >= minValue && date <= maxValue) return date;
                DefaultPersons.システム.Say("可能な範囲は{0:yyyy年MM月dd日}～{1:yyyy年MM月dd日}です。再入力をお願いします。", minValue, maxValue);
                return null;
            };
            humanActions.DoEnterDateTimeAsync = async (initialValue, minValue, maxValue) =>
            {
                return (await humanActions.DoEnterDateTimeWithOptionsAsync(initialValue, minValue, maxValue, null)).Item1;
            };
            humanActions.DoEnterDateTimeWithOptionsAsync = async (initialValue, minValue, maxValue, options) =>
            {
                var date = await HtmlGenerator.OpenDateTime(initialValue, initialValue.Hour, initialValue.Minute, options);
                HtmlGenerator.TriggerUIUpdate();
                if (date == DateTime.MinValue) return null;
                if (date >= minValue && date <= maxValue) return new Tuple<DateTime?, string>(date, options[HtmlGenerator.DateTimeOption]);
                DefaultPersons.システム.Say("可能な範囲は{0:yyyy年MM月dd日}～{1:yyyy年MM月dd日}です。再入力をお願いします。", minValue, maxValue);
                return null;
            };
            humanActions.NetSendAsync = async (id, base64) =>
            {
                var p = new netSendPara(id, DateTime.Now.ToString("yyyyMMdd"), base64);
                var response = await HtmlGenerator.Http.PostAsJsonAsync<netSendPara>("https://wangfproxy.azurewebsites.net/Home", p);
                return await response.Content.ReadAsStringAsync();
            };
            humanActions.IsDesktop = () => false;
            humanActions.GetUri = () => HtmlGenerator.GetUri();
            humanActions.OpenCustomRazorComponentAsync = async (type) =>
            {
                return await HtmlGenerator.OpenCustomRazorComponentAsync(type);
            };
            humanActions.CloseCustomRazorComponent = (obj) =>
            {
                HtmlGenerator.CloseCustomRazorComponent(obj);
            };
            humanActions.Reboot = () =>
            {
                HtmlGenerator.TriggerReload();
                Task.Delay(-1);
            };
            UI.Actions = humanActions;
        }

        private record netSendPara(string id, string code, string base64);

        private static async Task saveCommonAsync(byte[] Body, string filename, bool isAuto = false, bool requireDesc = false)
        {
            var base64body = Convert.ToBase64String(Body);
            await wangfUtil.LocalStorage.SetItemAsync(filename, base64body);
            if (requireDesc)
            {
                var desc = General.GenerateSuggestedFileNameWithoutExt(isAuto);
                //var descar = System.Text.Encoding.UTF8.GetBytes(desc);
                //var descBody = Convert.ToBase64String(descar);
                await wangfUtil.LocalStorage.SetItemAsync(FileLayout.CreateDescFileName(filename), desc);
            }
        }
        private static async Task<byte[]> loadCommonAsync(string filename)
        {
            var base64body = await wangfUtil.LocalStorage.GetItemAsync(filename);
            if (base64body == null) return new byte[0];
            try
            {
                return Convert.FromBase64String(base64body);
            }
            catch (FormatException ex)
            {
                await UI.Actions.tellAssertionFailedAsync(ex.ToString());
            }
            return new byte[0];
        }

        /// <summary>
        /// シナリオ実行のためのメインスレッドメソッド
        /// このメソッドは、どこで強制中断されても問題を残さないようになっていなければならない
        /// データのセーブなど、中断されると問題が起きる処理はUIスレッド側でInvokeさせる必要がある
        /// </summary>
        private static async Task ScenarioThreadProcAsync()
        {
            //System.Diagnostics.Debug.WriteLine("ScenarioThreadProc called");
            try
            {
                await General.CallAllModuleMethodAsync(async (m) => { await m.OnInitAsync(); });
                // UI.Actionsが有効になって以後にのみ呼び出されるべき
                await General.CallAllModuleMethodAsync((Func<Module, Task>)(async (m) => { await m.OnStartAsync(); }));

#if true
                //Console.WriteLine("PlaceFrontMenu.MainLoop();");
                //System.Diagnostics.Debug.WriteLine("PlaceFrontMenu.MainLoop();");
                // entering main loop
                // Mainループを抜けたら必ずリロードが発生する
                await MainLoop.DoItAsync();
                HtmlGenerator.TriggerReload();
#else
                var startPosId = Flags.CurrentPlaceId;
                var startWorldId = Flags.CurrentWorldId;
                // Blazor WebAssemblyではWebブラウザを閉じるという方法でこのループを抜け出す
                // Blazor Serverでは抜ける方法はない。Blazor Server版は正規にリリースすべきではない
                for (; ; )
                {
                    await MainLoop.DoItAsync();
                    Flags.CurrentPlaceId = startPosId;
                    Flags.CurrentWorldId = startWorldId;
                }
#endif
                ////System.Diagnostics.Debug.WriteLine("PlaceFrontMenu.MainLoop(); done");
                // メインウィンドウも終わらせる
                //formMain.Invoke((MethodInvoker)delegate ()
                //{
                //formMain.Close();
                //});

                // WANGFでは待たない
                //new AutoResetEvent(false).WaitOne();	// 待っている途中でフォームスレッドからアボートされる

                // WANGF専用メッセージ
                //DefaultPersons.システム.Say("再プレイする場合はリロードして下さい。");
#if DEBUG
                //DefaultPersons.システム.Say("Blazor Serverで動作している場合はサーバ側プロセスの再起動も必要です。");
#endif
            }
            catch (ThreadAbortException)
            {
                // nop (レポートさせる例外ではない)
            }
            //#if !DEBUG
            catch (MessageOnlyException e)
            {
                await HtmlGenerator.ClearAllAsync();
                HtmlGenerator.ExceptionInfo = e.Message;
                HtmlGenerator.TriggerUIUpdate();
            }
            catch (Exception e)
            {
                await HtmlGenerator.ClearAllAsync();
                //if (System.Diagnostics.Debugger.IsAttached) throw;
                HtmlGenerator.ExceptionInfo = e.ToString();
                HtmlGenerator.TriggerUIUpdate();
                // for desktop version
                //formMain.Invoke((MethodInvoker)delegate()
                //{
                //formMain.ExceptionProxy(e);
                //});
            }
//#endif
        }

        public static async Task ShowReportAsync(string message)
        {
            waRazorUI.HtmlGenerator.TotalReport = message;
            try
            {
                JournalingWriter.IsTemporaryStopped = true;
                try
                {
                    await UI.SimpleMenuWithoutSystemAsync("", new SimpleMenuItem[] { new SimpleMenuItem("次へ") });
                }
                finally
                {
                    JournalingWriter.IsTemporaryStopped = false;
                }
            }
            finally
            {
                waRazorUI.HtmlGenerator.TotalReport = null;
            }
        }

        //private static Task schenarioTask;
        //private static bool abortRequest = false;
        public static void SetMainForm(object formMain)
        {
            ScenarioThread.formMain = formMain;
        }
        public static void SetEquiopForm(object formEquiop)
        {
            ScenarioThread.formEquiop = formEquiop;
        }
        public static void RunScenarioMain()
        {
            //schenarioTask = Microsoft.AspNet.Identity.AsyncHelper._myTaskFactory.StartNew(ScenarioThreadProc);
            _ = ScenarioThreadProcAsync();/* DIABLE ASYNC WARN */
        }

        public static void AbortScenarioMain()
        {
            // nothing to do here
            //if (schenarioTask != null)
            //{
            //thread.Abort();
            //thread.Join();
            //}
        }
    }

}
