using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using ANGF;
using ANGFLib;


namespace FirstOtsukai
{
    class HaoPlacePrologue : Place
    {
        public override bool IsStatusHide => true;
        public override string Id
        {
            get { return FirstOtsykaiConstants.PrologueID; }
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
            Flags.Now = HaoConstants.StartDateTime;
            Flags.生活サイクル起点時間 = 8;
            State.今日の起床時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day,
                Flags.生活サイクル起点時間, 0, 0);
            State.今日の就寝時刻 = State.今日の起床時刻.AddHours(16);
            State.SetScheduleVisible(FirstOtsykaiConstants.時間切れスケジュールID, true);
            State.SetScheduleVisible(FirstOtsykaiConstants.百円拾うスケジュールID, true);

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
            var q = new QuickTalk();
            q.AddTalker("m",HaoPersons.ママ);
            q.AddMacro("n", HaoFlags.名前);
            q.Play(@"
m $nちゃん。おしょうゆを買ってきて。
ボクはおかいものを頼まれた。
");
            return true;
        }

        private async Task getPlayerNameAsync()
        {
            for (; ; )
            {
                DefaultPersons.システム.Say("これより、名前を入力します。");

                HaoFlags.名前 = await UI.Actions.enterPlayerNameAsync("名前を入力してください(例:ヒデキ、ヒロミ等): ", "ヒデキ");
                
                SimpleMenuItem[] items = {
                    new SimpleMenuItem("はい" ),
                    new SimpleMenuItem("いいえ" )
                };
                int selection = await UI.SimpleMenuWithoutSystemAsync(string.Format("「名前」={0}ちゃん　でよろしいですか?", HaoFlags.名前), items);
                if (selection == 0) break;
            }
        }

		private async Task startAsync()
		{
            defaultItems();
            prologue();
			await State.WarpToAsync(FirstOtsykaiConstants.我が家ID);
            General.NotifyNewGame();
        }

        private async Task silentStartAsync()
        {
            defaultItems();
            await State.WarpToAsync(FirstOtsykaiConstants.我が家ID);
            General.NotifyNewGame();
        }

        private static void defaultItems()
        {
            State.GetItem(HaoItems.Tシャツ);
            State.GetItem(HaoItems.ズボン);
            State.GetItem(HaoItems.Itemジュース);
        }

        public override async Task OnMenuAsync()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
			items.Add(new SimpleMenuItem("プロローグ有り (初回プレイ時推奨)" ));
			items.Add(new SimpleMenuItem("プロローグ無し & 名前入力あり" ));
			for (; ; )
			{
				int selection = await UI.SimpleMenuWithCancelAsync("開始方法を選択してください。", items.ToArray());
				switch (selection)
				{
					case -1:
						await State.WarpToAsync(Places.PlaceNull.Id);
						return;
					case 0:
						await getPlayerNameAsync();
                        await startAsync();
						return;
					case 1:
                        await getPlayerNameAsync();
                        await silentStartAsync();
						return;
				}
			}
		}
    }
}
