using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;
using waSimpleRPGBase;

namespace waFirstRPG
{
    [Exclude]   // このクラスは街道作成用なので、自動コレクトの対象外である
    public class HarRoadPlace : RoadPlace
    {
        public override async Task OnEnteringAsync()
        {
            await base.OnEnteringAsync();
            var random = new Random((int)Flags.Now.Ticks);
            var result = random.Next(3);    // エンカウント確率1/3
            if (result != 0) return;
            var tgt = HarMonsters.Slime;
            //DefaultPersons.Super.Say($"{tgt.HumanReadableName}が出現した!");
            await HarFight.FightToAsync(tgt);
        }
        public HarRoadPlace(string name, string id, string nextPlaceId, string prevPlaceId) : base(name, id, nextPlaceId, prevPlaceId)
        {
        }
    }

    public static class HarRoad
    {
        public static RoadPlace[] 冒険街道 = General.RoadPlaceGenerator("冒険街道", "{edffcf05-d2d4-4805-b077-7512858f7dc3}", HarConstants.街ID, HarConstants.魔王城ID, 3, (name, id, from, to) => new HarRoadPlace(name, id, from, to)).ToArray();
    }

    public class HarPlace街 : Place
    {
        public override string Id
        {
            get { return HarConstants.街ID; }
        }
        public override string HumanReadableName
        {
            get { return "街"; }
        }
        public override string[] GetLinkedPlaceIds()
        {
            return new string[] { HarRoad.冒険街道[0].Id };
        }
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("宿屋", async () =>
            {
                DefaultPersons.独白.Say("宿屋に泊まった。");
                RPGBaseUtil.全員HPMP全回復();
                DefaultPersons.独白.Say("全員のHPとMPが全回復した。");
                await State.GoNextDayMorningAsync();
                return true;
            }));
            return base.ConstructMenu(list);
        }
    }
    public class HarPlace魔王城 : Place
    {
        public override string Id
        {
            get { return HarConstants.魔王城ID; }
        }
        public override string HumanReadableName
        {
            get { return "魔王城"; }
        }
        public override string[] GetLinkedPlaceIds()
        {
            return new string[] { HarRoad.冒険街道[^1].Id };
        }
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("呼び鈴を鳴らす", async () =>
            {
                DefaultPersons.独白.Say("呼び鈴を鳴らした。");
                DefaultPersons.独白.Say("チリンチリン");
                var r = await HarFight.FightToAsync(HarMonsters.BossSlime);
                if (r == true)
                {
                    DefaultPersons.Super.Say("グッド・エンディング達成!");
                    State.SetCollection(Constants.EndingCollectionID, HarConstants.グッドエンディングID, null);
                    await State.WarpToAsync(HarConstants.EpilogueID);
                }
                return true;
            }));
            return base.ConstructMenu(list);
        }
    }
}
