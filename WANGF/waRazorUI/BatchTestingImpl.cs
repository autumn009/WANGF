using ANGFLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wangflib;

namespace waRazorUI
{
    internal class BatchTestingImpl : IBatchTestingForFramework, IBatchTestingForEachTitle
    {
        private const string NameHeader = "BatchTesting{8cf0fb25-d78d-4f3e-86ef-4de0c6437afd}";
        private const string EnableKey = NameHeader + "Enable";
        private const string CountKey = NameHeader + "Count";
        private const string MaxKey = NameHeader + "Max";
        private const string StartKey = NameHeader + "Start";

        public async Task<DateTime> GetStartDateTimeAsync()
        {
            var s = await wangfUtil.LocalStorage.GetItemAsync(StartKey);
            if (DateTime.TryParseExact(s, Constants.DateTimeFormat,null, System.Globalization.DateTimeStyles.None, out var date)) return date;
            return DateTime.MinValue;
        }
        public async Task SetStartDateTimeAsync(DateTime dateTime)
        {
            await wangfUtil.LocalStorage.SetItemAsync(StartKey, dateTime.ToString(Constants.DateTimeFormat));
        }

        private static string ResultMessageKey(int index)
        {
            return NameHeader + index;
        }
        async Task<IEnumerable<string>> IBatchTestingForFramework.EnumResultsAsync()
        {
            var list = new List<string>();
            var startDateTime = await wangflib.BatchTest.BatchTestingForFramework.GetStartDateTimeAsync();
            if (startDateTime != DateTime.MinValue)
            {
                var start = startDateTime.ToString("yyyy/MM/dd HH:mm:ss");
                list.Add($"開始時刻: {start}");
                var maxs = await wangfUtil.LocalStorage.GetItemAsync(MaxKey);
                int.TryParse(maxs, out var max);
                for (int i = 0; i < max; i++)
                {
                    var s = await wangfUtil.LocalStorage.GetItemAsync(ResultMessageKey(i));
                    if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                }
                //await ((IBatchTestingForEachTitle)this).SetCountAsync(0);

                var elapsedTime = (DateTime.Now - startDateTime).ToString("hh\\:mm\\:ss");
                list.Add($"経過時間: {elapsedTime}");
            }
            return list;
        }

        async Task IBatchTestingForFramework.ClearAsync()
        {
            var maxs = await wangfUtil.LocalStorage.GetItemAsync(MaxKey);
            int.TryParse(maxs, out var max);
            for (int i = 0; i < max; i++)
            {
                await wangfUtil.LocalStorage.RemoveItemAsync(ResultMessageKey(i));
            }
            await wangfUtil.LocalStorage.SetItemAsync(CountKey, "0");
            await wangfUtil.LocalStorage.SetItemAsync(MaxKey, "0");
            await wangfUtil.LocalStorage.SetItemAsync(EnableKey, "");
            await SetStartDateTimeAsync(DateTime.MinValue); // 開始時刻をクリア
        }

        async Task IBatchTestingForFramework.StartBatchTestingAsync(int max)
        {
            await SetStartDateTimeAsync(DateTime.Now);
            for (int i = 0; i < max; i++)
            {
                await wangfUtil.LocalStorage.RemoveItemAsync(ResultMessageKey(i));
            }
            int count = 0;
            var ar = (await Modules.EnumEmbeddedModulesAsync()).ToArray();
            for (int i = 0; i < max; i++)
            {
                if( ar[i].autotest == MetaModuleAutoTest.Yes )
                {
                    count = i;
                    break;
                }
            }
            await wangfUtil.LocalStorage.SetItemAsync(CountKey, count.ToString());
            await wangfUtil.LocalStorage.SetItemAsync(MaxKey, max.ToString());
            await wangfUtil.LocalStorage.SetItemAsync(EnableKey, "1");
            HtmlGenerator.TriggerReload();
            await Task.Delay(-1);   // 無限待機
        }
        async Task IBatchTestingForFramework.TerminateBatchTestingAsync()
        {
            await wangfUtil.LocalStorage.SetItemAsync(EnableKey, "");
        }

        /// <summary>
        /// バッチテスト中か?
        /// </summary>
        async Task<bool> IBatchTestingForEachTitle.IsBatchTestingAsync()
        {
            return !string.IsNullOrWhiteSpace(await wangfUtil.LocalStorage.GetItemAsync(EnableKey));
        }

        async Task<int> IBatchTestingForEachTitle.GetCountAsync()
        {
            int.TryParse(await wangfUtil.LocalStorage.GetItemAsync(CountKey), out int result);
            return result;
        }
        async Task<int> IBatchTestingForEachTitle.GetMaxAsync()
        {
            int.TryParse(await wangfUtil.LocalStorage.GetItemAsync(MaxKey), out int result);
            return result;
        }

        async Task IBatchTestingForEachTitle.SetCountAsync(int n)
        {
            await wangfUtil.LocalStorage.SetItemAsync(CountKey, n.ToString());
        }
        /// <summary>
        /// バッチテストを終了する。次があれば次のテストが始まり、無ければ結果を表示してそのまま終わる
        /// </summary>
        async Task IBatchTestingForEachTitle.EndBatchTestingAsync(bool successed, string name, string result)
        {
            if (!await ((IBatchTestingForEachTitle)this).IsBatchTestingAsync()) return; // if not batch testing then no operation
            var count = await ((IBatchTestingForEachTitle)this).GetCountAsync();
            var msg = (successed ? "Succeeded" : "Failed") + $": {name}: {result}";
            await wangfUtil.LocalStorage.SetItemAsync(ResultMessageKey(count), msg);
            var max = await((IBatchTestingForEachTitle)this).GetMaxAsync();
            for (; ; )
            {
                count++;
                if (count >= max)
                {
                    await wangfUtil.LocalStorage.SetItemAsync(EnableKey, "");
                    break;
                }
                else
                {
                    if ((await Modules.EnumEmbeddedModulesAsync()).ElementAt(count).autotest != MetaModuleAutoTest.Yes) continue;
                    await wangfUtil.LocalStorage.SetItemAsync(CountKey, count.ToString());
                    break;
                }
            }
            HtmlGenerator.TriggerReload();
            await Task.Delay(-1);   // 無限待機
        }

        async Task<bool> IBatchTestingForEachTitle.IsStartBatchTestingAsync()
        {
            return await ((IBatchTestingForEachTitle)this).IsBatchTestingAsync();
        }
    }
}
