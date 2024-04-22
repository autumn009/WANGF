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
			// �t�����g���j���[�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
		}

		public override void OnLeaveing()
		{
			// �t�����g���j���[�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
		}

		public override string HumanReadableName
		{
			get { return "(�t�����g���j���[)"; }
		}

		public override async Task OnMenuAsync()
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
			items.Add(new SimpleMenuItem("�ŏ�����", async () =>
				{
					await State.WarpToAsync(FirstOtsykaiConstants.PrologueID);
					return true;
				}));
            items.Add(new SimpleMenuItem("�I��", async () =>
				{
                    // �I���̈ӎu���m�F�B�m�F���͊���currentPalce��PlaceNull�ɂȂ��Ă���
                    await UI.DoneConfirmAsync();
					return true;
				}));
            await UI.SimpleMenuWithoutSystemAsync("�J�n���@�܂��͋@�\��I��", items.ToArray());
		}
	}
}
