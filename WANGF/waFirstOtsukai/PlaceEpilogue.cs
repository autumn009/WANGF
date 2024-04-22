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
			items.Add(new SimpleMenuItem("�͂�"));
			items.Add(new SimpleMenuItem("������"));
			int selection = await UI.SimpleMenuWithoutSystemAsync("����1�x���߂�����܂���?", items.ToArray());
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
            await �j���[�Q�[���I��Async();
		}
	}
}
