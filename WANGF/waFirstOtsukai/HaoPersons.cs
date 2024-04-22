using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using ANGFLib;
using System.Threading.Tasks;

namespace FirstOtsukai
{
	public class HaoPersons
	{
        public static Person 警官 = new PersonWithPlace(FirstOtsykaiConstants.警官ID, "警官", Sex.Male, FirstOtsykaiConstants.こうばんID, (x) => Flags.Now.Hour >= 8 && Flags.Now.Hour <= 18, async (x) =>
		{
			x.Say("本官の目玉はつながっていないのであります。");
            await Task.CompletedTask;
		});
        public static Person ママ = new PersonWithPlace(FirstOtsykaiConstants.ママID, "ママ", Sex.Female, FirstOtsykaiConstants.我が家ID, (x) => true, async (x) =>
		{
            if (State.GetItemCount(HaoItems.Itemしょうゆ) > 0)
            {
                HaoPersons.ママ.Say("あら、おしょうゆありがとう。");
                DefaultPersons.独白.Say("やった!　成功したぞ!");
                SystemFile.SetFlagString(FirstOtsykaiConstants.Hao主人公名SystemFlagID, HaoFlags.名前);
                await SystemFile.SaveIfDirtyAsync();
                DefaultPersons.システム.Say("はじめてのお使いのクリア特典がアンロックされました。");
                DefaultPersons.システム.Say("他のゲームで追加機能が使用できる場合があります。");
                State.SetCollection(Constants.EndingCollectionID, FirstOtsykaiConstants.グッドエンディングID, null);
                await State.WarpToAsync(FirstOtsykaiConstants.EpilogueID);
            }
            else
            {
                x.Say("おねがいだから、おしょうゆを買ってきて。");
                x.Say("これも教育よ。けして手抜きではないの。");
            }
		});
        public static Person ワープおじさん1 = new PersonWithPlace(FirstOtsykaiConstants.ワープおじさんID1, "ワープおじさん", Sex.Male, FirstOtsykaiConstants.こうばんID, (x) => true, async (x) =>
        {
            HaoPersons.ワープおじさん1.Say("魔王のいる異世界に行ってみたくはないかね?");
            if (await UI.YesNoMenuAsync("異世界に", "行く", "やめとく"))
            {
                await State.WarpToAsync(HaoConstants.あの世界ID, FirstOtsykaiConstants.街ID);
            }
        });
        public static Person ワープおじさん2 = new PersonWithPlace(FirstOtsykaiConstants.ワープおじさんID2, "ワープおじさん", Sex.Male, FirstOtsykaiConstants.街ID, (x) => true, async (x) =>
        {
            HaoPersons.ワープおじさん2.Say("元の世界に戻りたくはないかね?");
            if (await UI.YesNoMenuAsync("元の世界に", "戻る", "やめとく"))
            {
                await State.WarpToAsync(null, FirstOtsykaiConstants.こうばんID);
            }
        });
        public static Person 魔王 = new PersonWithPlace(FirstOtsykaiConstants.魔王ID, "魔王", Sex.Male, FirstOtsykaiConstants.魔王城ID, (x) => true, (x) =>
        {
            HaoPersons.魔王.Say("ぼくは魔王。何も悪さはしないよ。");
            return Task.CompletedTask;
        });
	}
}
