using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace FirstOtsukai
{
    public class HaoPlaceおみせ : HaoPlace
    {
        public override string Id
        {
            get { return FirstOtsykaiConstants.おみせID; }
        }
        public override string HumanReadableName
        {
            get { return "おみせ"; }
        }
        public override Position GetDistance()
        {
            return new Position() { x = 200, y = 0 };
        }
        private async Task<bool> buyAsync()
        {
            await General.StandardBuyAsync(FirstOtsykaiConstants.ショップID,(i,o)=>i.Price);
            return true;
        }
        private async Task<bool> sellAsync()
        {
            await General.StandardSellAsync(FirstOtsykaiConstants.ショップID, (i, o) => i.Price/2);
            return true;
        }
        private async Task<bool> fukubukiAsync()
        {
            var list = new FukubikiRecord[] {
                new FukubikiRecord() { RankName="特賞", TagetItem=HaoItems.つなぎ, CustomName="使用回数100回のつなぎ", TimesForOne = 10000, AfterProcAsync = (item)=> { State.AddUsedCount(item,100); return Task.CompletedTask; } },
                new FukubikiRecord() { RankName="一等", TagetItem=HaoItems.つなぎ, TimesForOne = 100 },
                new FukubikiRecord() { RankName="二等", TagetItem=HaoItems.Itemしょうゆ, TimesForOne = 10 }
            };
            await Fukubiki.DoFukubikiAsync(1, list, HaoItems.Itemジュース);
            return true;
        }
        private async Task<bool> fukubuki2Async()
        {
            var list = new FukubikiRecord[] {
                new FukubikiRecord() { RankName="特賞", TagetItem=HaoItems.つなぎ, CustomName="使用回数100回のつなぎ", TimesForOne = 2, AfterProcAsync = (item)=> { State.AddUsedCount(item,100);return Task.CompletedTask; } },
            };
            await Fukubiki.DoFukubikiAsync(1, list, HaoItems.Itemジュース);
            return true;
        }
        private bool getStar()
        {
            if (StarManager.GetStars() < 1)
            {
                DefaultPersons.システム.Say("Starを持っていないので1つあげます。");
                StarManager.AddStar(1);
            }
            else
            {
                DefaultPersons.システム.Say("既にStarを持っているのであげません。");
            }
            return true;
        }

        public override async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("買い物", buyAsync));
            list.Add(new SimpleMenuItem("売り物", sellAsync));
            list.Add(new SimpleMenuItem("福引き", fukubukiAsync));
            if (ANGFLib.SystemFile.IsDebugMode) list.Add(new SimpleMenuItem("2回に1回は100回使用つなぎがもらえるテスト用福引き", fukubuki2Async));
            list.Add(new SimpleMenuItem("スターをもらう", getStar));
            await Task.Delay(0);
            return true;
        }
    }
}
