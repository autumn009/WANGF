using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    public static class StarManager
    {
        /// <summary>
        /// 現在の所有スター数
        /// </summary>
        /// <returns></returns>
        public static int GetStars()
        {
            return Flags.NumberOfStars;
        }
        /// <summary>
        /// スターを消費してゲーム内の何かを手に入れる
        /// 同時に消費されたとき、スター数がマイナスになることは許容される
        /// </summary>
        /// <param name="stars">消費するstar数</param>
        /// <param name="proc">何かを獲得する手順。エラーが起きたら理由文字列を返す。成功ならnull</param>
        /// <returns>実行できない理由。成功したらnull</returns>
        public static async Task<string> ExchangeFromStarToProcAsync(int stars, Func<Task<string>> proc)
        {
            if (stars > 0)
            {
                var currentStars = GetStars();
                if (currentStars < stars)
                {
                    return string.Format("スターが{0}個要求されましたが、{1}個しかありません。{2}個不足です。スターはページ下部のスター販売店のボタンから追加購入できます。", stars, currentStars, stars - currentStars);
                }
            }
            var r = await proc();
            if (r != null) return r;
            if (stars > 0)
            {
                AddStar(-stars);
            }
            return null;
        }
        public static async Task<bool> ExchangeFromStarToProcExAsync(int stars, Func<Task<string>> proc, bool confirmation = true)
        {
            if (confirmation)
            {
                DefaultPersons.システム.Say("このメニューではスター{0}個を消費します。", stars);
                DefaultPersons.システム.Say("スターは消費するとセーブデータをロードしても元に戻りません。");
                if (!await UI.YesNoMenuAsync("スターの消費に同意します?", "はい", "いいえ")) return false;
            }
            else
            {
                // この経路は意味不明である。たぶん使用されることはない。
                if (!await UI.YesNoMenuAsync("実行しますか?", "はい", "いいえ")) return false;
            }
            var r = await ExchangeFromStarToProcAsync(stars, proc);
            if (!string.IsNullOrEmpty(r))
            {
                DefaultPersons.システム.Say(r);
            }
            return r == null;
        }
        public static void AddStar(int starsToAdd)
        {
            Flags.NumberOfStars += starsToAdd;
        }
    }
}
