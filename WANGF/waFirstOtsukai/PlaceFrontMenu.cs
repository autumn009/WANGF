using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;


namespace FirstOtsukai
{
	public class HaoPlaceFrontMenu : ANGFLib.Place
	{
        public override bool IsStatusHide => true;
        public override string Id
        {
            get { return FirstOtsykaiConstants.FrontMenuID; }
        }
        public override async Task<bool> ConstructMenuAsync(List<SimpleMenuItem> list)
        {
			await Task.Delay(0);
			return false;
        }
		public override void OnEntering()
		{
			// フロントメニューは仮想の場なので、出入りは宣言しない
		}

		public override void OnLeaveing()
		{
			// フロントメニューは仮想の場なので、出入りは宣言しない
		}

		public override string HumanReadableName
		{
			get { return "(フロントメニュー)"; }
		}

		public override async Task OnMenuAsync()
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
			items.Add(new SimpleMenuItem("最初から", async () =>
				{
					await State.WarpToAsync(FirstOtsykaiConstants.PrologueID);
					return true;
				}));
            items.Add(new SimpleMenuItem("終了", async () =>
				{
                    // 終了の意志を確認。確認時は既にcurrentPalceがPlaceNullになっている
                    await UI.DoneConfirmAsync();
					return true;
				}));
            await UI.SimpleMenuWithoutSystemAsync("開始方法または機能を選択", items.ToArray());
		}
	}
}
