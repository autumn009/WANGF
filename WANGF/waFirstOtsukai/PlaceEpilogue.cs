using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;


namespace FirstOtsukai
{
	public class HaoPlaceEpilogue : Place
	{
        public override bool IsStatusHide => true;
        public override string Id
        {
            get { return FirstOtsykaiConstants.EpilogueID; }
        }
        public override async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
			await Task.Delay(0);
            return false;
        }
        public override void OnEntering()
		{
			// エピローグは仮想の場なので、出入りは宣言しない
		}

		public override void OnLeaveing()
		{
			// エピローグは仮想の場なので、出入りは宣言しない
		}

		public override string HumanReadableName
		{
            get { return "(エピローグ)"; }
		}

		public static async Task ニューゲーム選択Async()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
			items.Add(new SimpleMenuItem("はい"));
			items.Add(new SimpleMenuItem("いいえ"));
			int selection = await UI.SimpleMenuWithoutSystemAsync("もう1度初めからやりますか?", items.ToArray());
			switch (selection)
			{
				case 1:
					State.Terminate();
					break;
				default:
					await State.WarpToAsync(FirstOtsykaiConstants.PrologueID);
					break;
			}
		}

		public override async Task OnMenuAsync()
		{
            await UI.Actions.ShowTotalReportAsync();
            await ニューゲーム選択Async();
		}
	}
}
