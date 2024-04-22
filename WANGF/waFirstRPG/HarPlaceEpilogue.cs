using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;


namespace waFirstRPG
{
	public class HaoPlaceEpilogue : Place
	{
        public override bool IsStatusHide => true;
        public override string Id
        {
            get { return HarConstants.EpilogueID; }
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
            items.Add(new SimpleMenuItem("ロード", async () =>
            {
                await UI.LoadAsync();
                return true;
            }));
            items.Add(new SimpleMenuItem("自動セーブ フォルダからロード", async () =>
            {
                await UI.LoadFromAutoSaveAsync();
                return true;
            }));
            items.Add(new SimpleMenuItem("ニューゲーム", async () => { await State.WarpToAsync(HarConstants.PrologueID); return true; }));
			items.Add(new SimpleMenuItem("終了", () => { State.Terminate(); return true; }));
			await UI.SimpleMenuWithoutSystemAsync("やり直しますか?", items.ToArray());
		}

		public override async Task OnMenuAsync()
		{
            await UI.Actions.ShowTotalReportAsync();
            await ニューゲーム選択Async();
		}
	}
}
