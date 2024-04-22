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
            // �v�����[�O�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
            State.Clear();
            Flags.������ = 10000;
            Flags.Now = HaoConstants.StartDateTime;
            Flags.�����T�C�N���N�_���� = 8;
            State.�����̋N������ = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day,
                Flags.�����T�C�N���N�_����, 0, 0);
            State.�����̏A�Q���� = State.�����̋N������.AddHours(16);
            State.SetScheduleVisible(FirstOtsykaiConstants.���Ԑ؂�X�P�W���[��ID, true);
            State.SetScheduleVisible(FirstOtsykaiConstants.�S�~�E���X�P�W���[��ID, true);

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
            var q = new QuickTalk();
            q.AddTalker("m",HaoPersons.�}�});
            q.AddMacro("n", HaoFlags.���O);
            q.Play(@"
m $n�����B�����傤��𔃂��Ă��āB
�{�N�͂��������̂𗊂܂ꂽ�B
");
            return true;
        }

        private async Task getPlayerNameAsync()
        {
            for (; ; )
            {
                DefaultPersons.�V�X�e��.Say("������A���O����͂��܂��B");

                HaoFlags.���O = await UI.Actions.enterPlayerNameAsync("���O����͂��Ă�������(��:�q�f�L�A�q���~��): ", "�q�f�L");
                
                SimpleMenuItem[] items = {
                    new SimpleMenuItem("�͂�" ),
                    new SimpleMenuItem("������" )
                };
                int selection = await UI.SimpleMenuWithoutSystemAsync(string.Format("�u���O�v={0}�����@�ł�낵���ł���?", HaoFlags.���O), items);
                if (selection == 0) break;
            }
        }

		private async Task startAsync()
		{
            defaultItems();
            prologue();
			await State.WarpToAsync(FirstOtsykaiConstants.�䂪��ID);
            General.NotifyNewGame();
        }

        private async Task silentStartAsync()
        {
            defaultItems();
            await State.WarpToAsync(FirstOtsykaiConstants.�䂪��ID);
            General.NotifyNewGame();
        }

        private static void defaultItems()
        {
            State.GetItem(HaoItems.T�V���c);
            State.GetItem(HaoItems.�Y�{��);
            State.GetItem(HaoItems.Item�W���[�X);
        }

        public override async Task OnMenuAsync()
		{
			List<SimpleMenuItem> items = new List<SimpleMenuItem>();
			items.Add(new SimpleMenuItem("�v�����[�O�L�� (����v���C������)" ));
			items.Add(new SimpleMenuItem("�v�����[�O���� & ���O���͂���" ));
			for (; ; )
			{
				int selection = await UI.SimpleMenuWithCancelAsync("�J�n���@��I�����Ă��������B", items.ToArray());
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
