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
            // �v�����[�O�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
            State.Clear();
            Flags.������ = 10000;
            Flags.Now = HarConstants.StartDateTime;
            Flags.�����T�C�N���N�_���� = 8;
            State.�����̋N������ = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day,
                Flags.�����T�C�N���N�_����, 0, 0);
            State.�����̏A�Q���� = State.�����̋N������.AddHours(16);
            HarFlags.���O = "�}�C�P��";
            HarPersons.���������ǉ��Ƒ���(DefaultPersons.��l��.Id);
#if false
            State.GetItem(HarItems.���̌�);
            State.GetItem(HarItems.���̌�);
            State.GetItem(HarItems.���̊Z);
            State.GetItem(HarItems.���̊Z);
            State.GetItem(HarItems.�S�̌�);
            State.GetItem(HarItems.�S�̊Z);
#endif
            RPGBaseUtil.�S��HPMP�S��();

            // �����Z�b�g�̏��X�V�v��
            UI.Actions.ResetGameStatus();
        }

        public override void OnLeaveing()
        {
            // �v�����[�O�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
        }

        public override string HumanReadableName
        {
            get { return "(�v�����[�O)"; }
        }

        public bool prologue()
        {
            HarPersons.�}���A.Say("���̓}�������D���ȃ}���A�B�b���������璇�ԂɂȂ���B");
            HarPersons.���C.Say("���͗�V���������C�B�b����������͂ɂȂ�܂��B");
            HarPersons.����.Say("���͂������ȃ����B�b�������ĂˁB");
            return true;
        }

		private async Task startAsync()
		{
            defaultItems();
            prologue();
			await State.WarpToAsync(HarConstants.�XID);
            General.NotifyNewGame();
        }

        private static void defaultItems()
        {
            //State.GetItem(HaoItems.T�V���c);
            //State.GetItem(HaoItems.�Y�{��);
            //State.GetItem(HaoItems.Item�W���[�X);
        }

        public override async Task OnMenuAsync()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
			items.Add(new SimpleMenuItem("�Q�[���J�n" ));
			for (; ; )
			{
				int selection = await UI.SimpleMenuWithCancelAsync("�J�n���@��I�����Ă��������B", items.ToArray());
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
