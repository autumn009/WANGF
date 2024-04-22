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
			// �G�s���[�O�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
		}

		public override void OnLeaveing()
		{
			// �G�s���[�O�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
		}

		public override string HumanReadableName
		{
            get { return "(�G�s���[�O)"; }
		}

		public static async Task �j���[�Q�[���I��Async()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            items.Add(new SimpleMenuItem("���[�h", async () =>
            {
                await UI.LoadAsync();
                return true;
            }));
            items.Add(new SimpleMenuItem("�����Z�[�u �t�H���_���烍�[�h", async () =>
            {
                await UI.LoadFromAutoSaveAsync();
                return true;
            }));
            items.Add(new SimpleMenuItem("�j���[�Q�[��", async () => { await State.WarpToAsync(HarConstants.PrologueID); return true; }));
			items.Add(new SimpleMenuItem("�I��", () => { State.Terminate(); return true; }));
			await UI.SimpleMenuWithoutSystemAsync("��蒼���܂���?", items.ToArray());
		}

		public override async Task OnMenuAsync()
		{
            await UI.Actions.ShowTotalReportAsync();
            await �j���[�Q�[���I��Async();
		}
	}
}
