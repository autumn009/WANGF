using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace ANGFLib
{
    public static class JournalingDocumentPlayer
    {
        public static UIActionSet JournalActions = new UIActionSet();
        public static JournalingDocument doc;
        private static LinkedList<string> lastMessages = new LinkedList<string>();

        // プレイバックの成功失敗を示す・バッチテストの結果用
        public static bool IsSuccess { get; set; }

        public static async Task myTellAssertionFailedAsync(JournalingAssertionException ex)
        {
            JournalingDocumentPlayer.IsSuccess = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*** ジャーナリングファイル再生中のアサーションに失敗しました ***");
            sb.AppendLine();
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
            sb.AppendLine("Last Message Lines:");
            foreach (string s in lastMessages)
            {
                sb.AppendLine(s);
            }
            await UI.Actions.tellAssertionFailedAsync(sb.ToString());
        }
        private static async Task myTellAssertionFailedAsync(Exception ex)
        {
            JournalingDocumentPlayer.IsSuccess = false;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*** ジャーナリングファイル再生中に一般例外が発生しました ***");
            sb.AppendLine();
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
            sb.AppendLine("Last Message Lines:");
            foreach (string s in lastMessages)
            {
                sb.AppendLine(s);
            }
            await UI.Actions.tellAssertionFailedAsync(sb.ToString());
        }

        private delegate Task<Item> getTargetDelegateAsync();/* DIABLE ASYNC WARN */
        static JournalingDocumentPlayer()
        {
            JournalActions.ResetGameStatus = delegate()
            {
                // ジャーナリングプレイバック中は更新する意味はない
                // 逆に速度低下の問題を起こす
                //ScenarioThread.ResetGameStatus();
                return default;
            };
            JournalActions.messageOutputMethod = (Person talker, string message)=>
            {
                // メッセージは最後の10個だけ記録しておく
                // (アサートがフェイルしたときに何をしていたのか分かりやすいように)
                lastMessages.AddLast(talker.MyName + ": " + message);
                if (lastMessages.Count > 10)
                {
                    lastMessages.RemoveFirst();
                }
                return default;
            };
            JournalActions.simpleMenuMethodAsync = async (string prompt, SimpleMenuItem[] items, SimpleMenuItem systemMenu)=>
            {
                // SimpleMenuのキャンセルは明示的にジャーナリングされるので、
                // 意図したSimpleMenuが次に来ないのは全てアサートのフェイルと見なして
                // 良い
                try
                {
                    await doc.ProcessingAssertsAsync();
                    if (doc.IsEndOfRecords)
                    {
                        await UI.Actions.restoreActionSetAsync();
                        await SystemFile.SaveIfDirtyAsync();
                        return -2;	// 穏健に通常モードに戻る
                    }
                    JournalingNode node = await doc.GetNextRecordAsync(JournalingNodeType.SimpleMenu);
                    if (systemMenu != null && systemMenu.Name == node.ArgumentString)
                    {
                        if (systemMenu.SimpleMenuAction != null) systemMenu.SimpleMenuAction();
                        if (systemMenu.SimpleMenuActionAsync != null) await systemMenu.SimpleMenuActionAsync();
                        return -1;
                    }
                    else
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
#if USE_BLAZOR_CULTURE_FIXER
                            // Blazorに限って日本語カルチャで円記号を全角にしてしまう問題への対策
                            if (items[i].Name.Replace('￥', Constants.YenSign) == node.ArgumentString.Replace('￥', Constants.YenSign))
#else
                            if (items[i].Name == node.ArgumentString)
#endif
                            {
                                if (items[i].SimpleMenuAction != null)
                                {
                                    items[i].SimpleMenuAction();
                                }
                                if (items[i].SimpleMenuActionAsync != null)/* DIABLE ASYNC WARN */
                                {
                                    await items[i].SimpleMenuActionAsync();
                                }
                                return i;
                            }
                        }
                    }
                    var systemMessage = "";
                    if (systemMenu != null) systemMessage = $"システムメニューは【{systemMenu.Name}】です。";
                    var menus = string.Join(",", items.Select(c => c.Name));
                    throw await JournalingAssertionException.CreateAsync($"メニューの選択肢に『{ node.ArgumentString}』はありません。メニューの選択肢は【{menus}】です。{systemMessage}", node);
                }
                catch (JournalingAssertionException ex)
                {
                    //UI.Actions.restoreActionSet();
                    //await SystemFile.SaveIfDirtyAsync();
                    await myTellAssertionFailedAsync(ex);
                    return -2;	// 通常モードへのスイッチをリクエスト
                }
                catch (Exception ex)
                {
                    //UI.Actions.restoreActionSet();
                    //await SystemFile.SaveIfDirtyAsync();
                    await myTellAssertionFailedAsync(ex);
                    return -2;	// 通常モードへのスイッチをリクエスト
                }
            };

            JournalActions.enterPlayerNameAsync = async (string prompt, string defaultValue)=>
            {
                // キャンセルできない操作なので、見つからないのは全てアサートのフェイル
                // と見なして良い
                try
                {
                    JournalingNode node = await doc.GetNextRecordAsync(JournalingNodeType.NameEntry);
                    return node.ArgumentString;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return "アサート失敗";	// 本質的に意味のない文字列を仮に返す
                }
            };
            JournalActions.ChangeCycleAsync = async () =>
            {
                // キャンセルできない操作なので、見つからないのは全てアサートのフェイル
                // と見なして良い
                try
                {
                    JournalingNode node = await doc.GetNextRecordIfExistAsync(JournalingNodeType.ChangeCycleRelative);
                    if (node == null) return false;	// 意図したノードがなければキャンセルされたと見なす

                    int 旧起床時刻 = State.今日の起床時刻.Hour;
                    DateTime 新今日の就寝時刻 = State.今日の起床時刻.AddHours(16).AddHours(node.ArgumentInt);
                    if (新今日の就寝時刻 <= Flags.Now)
                    {
                        throw await JournalingAssertionException.CreateAsync("就寝時刻を現在時刻よりも手前に変えようとしました。", node);
                    }
                    // 以下の + 24はマイナス値を回避するためのゲタとしてはかせている
                    // マイナス値の%はマイナスの結果を出して意図した値にならない
                    int 新生活サイクル起点時間 = (Flags.生活サイクル起点時間 + node.ArgumentInt + 24) % 24;
                    Flags.生活サイクル起点時間 = 新生活サイクル起点時間;
                    State.今日の就寝時刻 = 新今日の就寝時刻;

                    // 超過による就寝は無いが、
                    // 時刻ジャストによる就寝はあり得るかもしれない
                    if (Flags.Now >= State.今日の就寝時刻)
                    {
                        DefaultPersons.システム.Say("就寝時刻を過ぎているので、すぐに就寝します。");
                        await State.GoNextDayMorningAsync();
                        return true;
                    }
                    return true;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return false;	// 本質的に意味のない値を仮に返す
                }
            };
            JournalActions.consumeItemMenuAsync = async()=>
            {
                try
                {
                    JournalingNode node = await doc.GetNextRecordIfExistAsync(JournalingNodeType.ConsumeItem);
                    if (node == null) return false;	// 意図したノードがなければキャンセルされたと見なす

                    Item selectedItem = JournalingDocument.GetItemFromName(node.ArgumentString);

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
                        return true;
                    }
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return false;
                }
            };
            JournalActions.selectOneItemAsync = async ()=>
            {
                try
                {
                    JournalingNode node = await doc.GetNextRecordIfExistAsync(JournalingNodeType.SelectOneItem);
                    if (node == null) return "";	// 意図したノードがなければキャンセルされたと見なす

                    Item selectedItem = JournalingDocument.GetItemFromName(node.ArgumentString);

                    return selectedItem.Id;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return "";
                }
            };
            JournalActions.shopSellMenuAsync = async (getPrice) =>
            {
                try
                {
                    JournalingNode node = await doc.GetNextRecordIfExistAsync(JournalingNodeType.Sell);
                    if (node == null) return false;	// 意図したノードがなければキャンセルされたと見なす

                    Item selectedItem = JournalingDocument.GetItemFromName(node.ArgumentString);
                    if (node.ArgumentInt > State.GetItemCount(selectedItem))
                    {
                        throw await JournalingAssertionException.CreateAsync(State.GetItemCount(selectedItem).ToString() +
                            "個しか持っていないアイテム" +
                            selectedItem.HumanReadableName +
                            "を、" +
                            node.ArgumentInt.ToString() +
                            "個売ろうとしました。", node);
                    }

                    int 売却予定価格 = getPrice(selectedItem, State.GetUsedCount(selectedItem))
                        * node.ArgumentInt;
                    Flags.所持金 += 売却予定価格;
                    State.SetItemCount(selectedItem, State.GetItemCount(selectedItem) -
                        node.ArgumentInt);
                    General.SoldNotifyAll(selectedItem, State.GetUsedCount(selectedItem));
                    State.SetUsedCount(selectedItem, 0);    // 手元にないので使用回数は無意味

                    return true;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return false;
                }
            };
            JournalActions.shopBuyMenuAsync = async (sellingItems, getPrice) =>
            {
                try
                {
                    JournalingNode node = await doc.GetNextRecordIfExistAsync(JournalingNodeType.Buy);
                    if (node == null) return false;	// 意図したノードがなければキャンセルされたと見なす

                    Item selectedItem = JournalingDocument.GetItemFromName(node.ArgumentString);
                    if (node.ArgumentInt + State.GetItemCount(selectedItem) > selectedItem.Max)
                    {
                        throw await JournalingAssertionException.CreateAsync(selectedItem.Max.ToString() +
                            "個しか持てないアイテム" +
                            selectedItem.HumanReadableName +
                            "を、" +
                            State.GetItemCount(selectedItem).ToString() +
                            "個持っているのに" +
                            node.ArgumentInt.ToString() +
                            "個買おうとしました。", node);
                    }

                    int 購入予定価格 = getPrice(selectedItem, State.GetUsedCount(selectedItem)) * node.ArgumentInt;

                    if (Flags.所持金 - 購入予定価格 < 0)
                    {
                        throw await JournalingAssertionException.CreateAsync(selectedItem.HumanReadableName +
                            "を、買うには" +
                            購入予定価格.ToString() +
                            "円必要ですが、" +
                            Flags.所持金.ToString() +
                            "円しかありません。", node);
                    }

                    Flags.所持金 -= 購入予定価格;
                    State.SetItemCount(selectedItem, State.GetItemCount(selectedItem) + node.ArgumentInt);
                    State.SetUsedCount(selectedItem, 0);    // 新品未使用!
                    return true;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return false;
                }
            };
            JournalActions.equipMenuExAsync = async (srcEquipSet, personId, customValidator) =>
            {
                try
                {
                    Flags.Equip.TargetPersonId = personId;
                    var set = srcEquipSet.Duplicate();
                    for (; ; )
                    {
                        if (doc.IsEndOfRecords) break;
                        JournalingNode node = await doc.PeekNextRecordAsync();

                        getTargetDelegateAsync getTargetAsync = async delegate () /* DIABLE ASYNC WARN */
                        {
                            // 指定がないときは脱がす=ItemNullにする
                            Item target = Items.ItemNull;
                            if (node.ArgumentString.Length > 0)
                            {
                                target = JournalingDocument.GetItemFromName(node.ArgumentString);
                                if (State.GetItemCount(target) == 0)
                                {
                                    throw await JournalingAssertionException.CreateAsync("所有していないアイテム、" +
                                        target.HumanReadableName +
                                        "を装備しようとしました。", node);
                                }
                            }
                            return target;
                        };

                        switch (node.CommandType)
                        {
                            case JournalingNodeType.Equip:
                                if (string.IsNullOrWhiteSpace(node.ArgumentString2))
                                {
                                    // 引数のセットを書き換える
                                    set.AllItems[node.ArgumentInt] = (await getTargetAsync());
                                }
                                else
                                {
                                    // 指定IDを直接書き換える
#if true
                                    System.Diagnostics.Debug.Assert(Flags.Equip.TargetPersonId == node.ArgumentString2);
                                    set.AllItems[node.ArgumentInt] = (await getTargetAsync());
#else
                                    var old = Flags.Equip.TargetPersonId;
                                    try
                                    {
                                        Flags.Equip.TargetPersonId = node.ArgumentString2;
                                        Flags.Equip[node.ArgumentInt] = (await getTargetAsync()).Id;
                                    }
                                    finally
                                    {
                                        Flags.Equip.TargetPersonId = old;
                                    }
#endif
                                }
                                break;
                            default:
                                return set;
                        }
                        await doc.IncrementNextRecordAsync();
                    }
                    if (customValidator != null)
                    {
                        if (customValidator(set) != null) return null;
                    }
                    return set;
                }
                catch (JournalingAssertionException ex)
                {
                    await myTellAssertionFailedAsync(ex);
                    return null;
                }
            };
            JournalActions.restoreActionSetAsync = async delegate () /* DIABLE ASYNC WARN */
            {
                await General.CallToRestoreActionSetAsync();
                //ScenarioThread.RestoreActionSet();
            };
            JournalActions.tellAssertionFailedAsync = async (string message)=>
            {
                //await UI.Actions.restoreActionSetAsync();
                await General.CallToRestoreActionSetAsync();
                await SystemFile.SaveIfDirtyAsync();
                await General.CallToTellAssertionFailedAsync(message);
                //ScenarioThread.TellAssertionFailed(message);
            };
            JournalActions.progressStatusAsync = async (string message)=>
            {
                await General.CallToProgressStatusAsync(message);
                //ScenarioThread.ProgressStatus(message);
            };
            JournalActions.isJournalFilePlaying = delegate()
            {
                return true;
            };
            // 本当にAzureでくてもテストの都合でAzureだと主張する
            JournalActions.GetUri = () => "https://wangf.azurewebsites.net/";
        }

    }
}
