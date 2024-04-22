using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    public class FukubikiRecord
    {
        public int TimesForOne; // N回に1回出る
        public string RankName; // 特賞や、一等賞など。
        public string CustomName;   // nullならTagetItemの名前を使う
        public Item TagetItem;
        public Func<Item,Task> AfterProcAsync;/* DIABLE ASYNC WARN */  // 当たった後で渡す前に加工する。nullなら加工しない
    }

    public static class Fukubiki
    {
        private static Random perfectRandom = new Random();

        private static async Task<bool> gettingItemSubAsync(Item item)
        {
            // 既に最大数持っていたら、これ以上は持てない
            if (State.GetItemCount(item) >= item.Max)
            {
                DefaultPersons.システム.Say("{0}は、既に最大数まで持っています。当てた景品と交換しますか?", item.HumanReadableName);
                var r = await UI.YesNoMenuAsync("当てた景品と", "交換する", "景品は辞退する");
                if (!r) return false;
            }
            State.GetItem(item);
            return true;
        }

        public static async Task DoFukubikiAsync(int starsForOnePlay, IEnumerable<FukubikiRecord> records, Item hazureItem)
        {
            DefaultPersons.システム.Say("福引きは完全にランダムで同じ条件でやり直しても同じ結果になるとは限りません。");
            DefaultPersons.システム.Say("また消費されたスターはデータをセーブせずに、ロードし直しても復活しません。");
            for (;;)
            {
                SimpleMenuItem[] menus = new SimpleMenuItem[]
                {
                    new SimpleMenuItem("福引きを引く"),
                    new SimpleMenuItem("景品一覧を確認する"),
                    new SimpleMenuItem("福引きを終える"),
                };
                var index = await UI.SimpleMenuWithCancelAsync("福引き", menus);
                if (index < 0 || index == 2) return;
                else if (index == 0)
                {
                    var yes = string.Format("{0}個のスターを払う", starsForOnePlay);
                    var r = await UI.YesNoMenuAsync("スターを払って福引きを行いますか?", yes, "やめる");
                    if (!r) continue;
                    await StarManager.ExchangeFromStarToProcExAsync(starsForOnePlay, async () =>
                    {
                        DefaultPersons.システム.Say("レッツ福引き。");
                        DefaultPersons.独白.Say("がらがらがらがらが……");
                        DefaultPersons.独白.Say("何が出たかな。");
                        foreach (var item in records.OrderBy(c => c.TimesForOne))
                        {
                            if (1.0 / item.TimesForOne > perfectRandom.NextDouble())
                            {
                                DefaultPersons.システム.Say("おめでとうございます。{0}の{1}が当たりました。", item.RankName, item.CustomName ?? item.TagetItem.HumanReadableName);
                                var item2 = item.TagetItem;
                                if (await gettingItemSubAsync(item2))
                                {
                                    if (item.AfterProcAsync != null) await item.AfterProcAsync(item2);
                                    return null;
                                }
                                break;
                            }
                        }
                        DefaultPersons.システム.Say("残念でした。参加賞の{0}です。", hazureItem.HumanReadableName);
                        await gettingItemSubAsync(hazureItem);
                        return null;
                    });
                }
                else if (index == 1)
                {
                    foreach (var item in records)
                    {
                        DefaultPersons.システム.Say("{0} {1}", item.RankName, item.CustomName ?? item.TagetItem.HumanReadableName);
                    }
                    DefaultPersons.システム.Say("外れた場合、または当たったアイテムが所有限界数に達していた場合、残念賞は{0}です。", hazureItem.HumanReadableName);
                }
            }
        }
    }
}
