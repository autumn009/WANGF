using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using ANGFLib;
using System.Threading.Tasks;

namespace FirstOtsukai
{
	public class HaoPersons
	{
        public static Person �x�� = new PersonWithPlace(FirstOtsykaiConstants.�x��ID, "�x��", Sex.Male, FirstOtsykaiConstants.�����΂�ID, (x) => Flags.Now.Hour >= 8 && Flags.Now.Hour <= 18, async (x) =>
		{
			x.Say("�{���̖ڋʂ͂Ȃ����Ă��Ȃ��̂ł���܂��B");
            await Task.CompletedTask;
		});
        public static Person �}�} = new PersonWithPlace(FirstOtsykaiConstants.�}�}ID, "�}�}", Sex.Female, FirstOtsykaiConstants.�䂪��ID, (x) => true, async (x) =>
		{
            if (State.GetItemCount(HaoItems.Item���傤��) > 0)
            {
                HaoPersons.�}�}.Say("����A�����傤�䂠�肪�Ƃ��B");
                DefaultPersons.�Ɣ�.Say("�����!�@����������!");
                SystemFile.SetFlagString(FirstOtsykaiConstants.Hao��l����SystemFlagID, HaoFlags.���O);
                await SystemFile.SaveIfDirtyAsync();
                DefaultPersons.�V�X�e��.Say("�͂��߂Ă̂��g���̃N���A���T���A�����b�N����܂����B");
                DefaultPersons.�V�X�e��.Say("���̃Q�[���Œǉ��@�\���g�p�ł���ꍇ������܂��B");
                State.SetCollection(Constants.EndingCollectionID, FirstOtsykaiConstants.�O�b�h�G���f�B���OID, null);
                await State.WarpToAsync(FirstOtsykaiConstants.EpilogueID);
            }
            else
            {
                x.Say("���˂���������A�����傤��𔃂��Ă��āB");
                x.Say("����������B�����Ď蔲���ł͂Ȃ��́B");
            }
		});
        public static Person ���[�v��������1 = new PersonWithPlace(FirstOtsykaiConstants.���[�v��������ID1, "���[�v��������", Sex.Male, FirstOtsykaiConstants.�����΂�ID, (x) => true, async (x) =>
        {
            HaoPersons.���[�v��������1.Say("�����̂���ِ��E�ɍs���Ă݂����͂Ȃ�����?");
            if (await UI.YesNoMenuAsync("�ِ��E��", "�s��", "��߂Ƃ�"))
            {
                await State.WarpToAsync(HaoConstants.���̐��EID, FirstOtsykaiConstants.�XID);
            }
        });
        public static Person ���[�v��������2 = new PersonWithPlace(FirstOtsykaiConstants.���[�v��������ID2, "���[�v��������", Sex.Male, FirstOtsykaiConstants.�XID, (x) => true, async (x) =>
        {
            HaoPersons.���[�v��������2.Say("���̐��E�ɖ߂肽���͂Ȃ�����?");
            if (await UI.YesNoMenuAsync("���̐��E��", "�߂�", "��߂Ƃ�"))
            {
                await State.WarpToAsync(null, FirstOtsykaiConstants.�����΂�ID);
            }
        });
        public static Person ���� = new PersonWithPlace(FirstOtsykaiConstants.����ID, "����", Sex.Male, FirstOtsykaiConstants.������ID, (x) => true, (x) =>
        {
            HaoPersons.����.Say("�ڂ��͖����B���������͂��Ȃ���B");
            return Task.CompletedTask;
        });
	}
}
