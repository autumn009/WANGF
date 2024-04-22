using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using ANGF;
using ANGFLib;
using waSimpleRPGBase;

namespace waFirstRPG
{
    class HaoPlacePrologue : Place
    {
        public override bool IsStatusHide => true;
        public override string Id
        {
            get { return HarConstants.PrologueID; }
        }
        public override async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
            await Task.Delay(0);
            return false;
        }
        public override void OnEntering()
        {
            // プロローグは仮想の場なので、出入りは宣言しない
            State.Clear();
            Flags.所持金 = 10000;
            Flags.Now = HarConstants.StartDateTime;
            Flags.生活サイクル起点時間 = 8;
            State.今日の起床時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day,
                Flags.生活サイクル起点時間, 0, 0);
            State.今日の就寝時刻 = State.今日の起床時刻.AddHours(16);
            HarFlags.名前 = "マイケル";
            HarPersons.初期装備追加と装備(DefaultPersons.主人公.Id);
#if false
            State.GetItem(HarItems.青銅の剣);
            State.GetItem(HarItems.青銅の剣);
            State.GetItem(HarItems.青銅の鎧);
            State.GetItem(HarItems.青銅の鎧);
            State.GetItem(HarItems.鉄の剣);
            State.GetItem(HarItems.鉄の鎧);
#endif
            RPGBaseUtil.全員HPMP全回復();

            // 装備セットの情報更新要求
            UI.Actions.ResetGameStatus();
        }

        public override void OnLeaveing()
        {
            // プロローグは仮想の場なので、出入りは宣言しない
        }

        public override string HumanReadableName
        {
            get { return "(プロローグ)"; }
        }

        public bool prologue()
        {
            HarPersons.マリア.Say("私はマリモが好きなマリア。話しかけたら仲間になるわよ。");
            HarPersons.レイ.Say("私は礼儀正しいレイ。話しかけたら力になります。");
            HarPersons.リリ.Say("私はお洒落なリリ。話しかけてね。");
            return true;
        }

		private async Task startAsync()
		{
            defaultItems();
            prologue();
			await State.WarpToAsync(HarConstants.街ID);
            General.NotifyNewGame();
        }

        private static void defaultItems()
        {
            //State.GetItem(HaoItems.Tシャツ);
            //State.GetItem(HaoItems.ズボン);
            //State.GetItem(HaoItems.Itemジュース);
        }

        public override async Task OnMenuAsync()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
			items.Add(new SimpleMenuItem("ゲーム開始" ));
			for (; ; )
			{
				int selection = await UI.SimpleMenuWithCancelAsync("開始方法を選択してください。", items.ToArray());
				switch (selection)
				{
					case -1:
                        await State.WarpToAsync(Places.PlaceNull.Id);
						return;
					case 0:
						await startAsync();
						return;
				}
			}
		}
    }
}
