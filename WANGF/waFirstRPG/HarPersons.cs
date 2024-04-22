using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using ANGFLib;
using System.Linq.Expressions;
using waFirstRPG;
using waSimpleRPGBase;

namespace waFirstRPG
{
    public class HarPerson : PersonWithPlace, IPartyMember
    {
        private readonly FlagCollection<string> rawEquip;

        public HarPerson(string id, string name, Sex sex, string placeID, FlagCollection<string> rawEquip) : base(id, name, sex, placeID, (p) => !Party.Contains(p.Id), async (p) =>
        {
            if (p != null)
            {
                if (Party.EnumPartyMembers().Count() < 3)
                {
                    DefaultPersons.�Ɣ�.Say($"{p.HumanReadableName}�͒��ԂɂȂ����B");
                    p.HPMP�S��();
                    HarPersons.���������ǉ��Ƒ���(p.Id);
                    Party.AddMember(p.Id);
                }
                else
                    DefaultPersons.�Ɣ�.Say("����ȏ㒇�Ԃ͑��₹�Ȃ��B");
            }
            await Task.Delay(0);
        })
        {
            this.rawEquip = rawEquip;
        }

        public IEnumerable<string> GetEquippedItemIds()
        {
            for (int i = 0; i < SimpleName<EquipType>.List.Count; i++)
            {
                yield return rawEquip[i.ToString()];
            }
        }
        public Person GetPerson() => this;

        public string GetRawEquip(int index)
        {
            return rawEquip[index.ToString()];
        }

        public void SetRawEquip(int index, string id)
        {
            rawEquip[index.ToString()] = id;
        }

        public override bool IsRequirePersistence => true;
    }


    public class HarPersons
    {
        public static Person �}���A = new HarPerson(HarConstants.�}���AID, "�}���A", Sex.Female, HarConstants.�XID, HarFlags.MariaEquip);
        public static Person ���C= new HarPerson(HarConstants.���CID, "���C", Sex.Female, HarConstants.�XID, HarFlags.ReiEquip);
        public static Person ���� = new HarPerson(HarConstants.����ID, "����", Sex.Female, HarConstants.�XID, HarFlags.RiriEquip);
        public static Person �A�h�o�C�X�������� = new PersonWithPlace("{87eaace9-6b14-43b9-b054-f6dd6be81181}", "�A�h�o�C�X��������", Sex.Male, HarConstants.�XID, (p)=>true, adviceAsync);
        public static Person �o���l�������� = new PersonWithPlace("{5f0b1070-613a-4652-b736-e336e0d28359}", "�o���l��������", Sex.Male, HarConstants.�XID, (p) => true, exp);

        private static Task exp(Person obj)
        {
            foreach (var personId in Party.EnumPartyMembers())
            {
                var p = Person.List[personId];
                �o���l��������.Say($"{p.HumanReadableName}�̏�񂾁B");
                �o���l��������.Say($"���݂̃��x����{p.GetLevel()}���B");
                �o���l��������.Say($"���݂̌o���l��{p.GetEXP()}���B");

                var table = RPGBaseLevelTable.PersonGrowingTable;
                long n = -1;
                for (int i = 1; i < table.Length; i++)
                {
                    if (p.GetEXP() < table[i])
                    {
                        n = table[i];
                        break;
                    }
                }
                if (n < 0)
                {
                    �o���l��������.Say("���ɂƂĂ������B");
                }
                else
                {
                    �o���l��������.Say($"���̃��x���ɕK�v�Ȍo���l�͂���{n - p.GetEXP()}���B");
                }
            }
            return Task.CompletedTask;
        }

        private static Task adviceAsync(Person obj)/* DIABLE ASYNC WARN */
        {
            var q = new QuickTalk();
            q.AddTalker("a", �A�h�o�C�X��������);
            q.Play("""
                a �A�h�o�C�X���Ă�낤�B
                a ������ɂ���{�X�X���C����|���񂾁B
                a ������͖`���X���̐�ɂ��邼�B
                a �p�[�e�B�[�̍ő�l����3�l���B�����͂悭�I�ׂ�B
                a �X���C����|���ƓS�̌����S�̊Z����ɓ���B�����l������ɓ���đS���ɑ�������񂾁B
                a �������A������Y���Ȃ�B�����������ꏊ�̖��O���N���b�N/�^�b�v���B
                a �퓬���ɖ��@���g����HP�S���S�񕜂̖��@���g���邼�B�g���閂�@�͂��ꂾ�����B
                a �X��HPMP���񕜂���ꍇ�͏h�����g���BMP�񕜂�����@�͂��ꂵ���Ȃ����B
                a �����X�^�[�͐擪�̃����o�[�����U������B
                a �擪�̃����o�[��HP��0�ɂȂ�ƃQ�[���I�[�o�[���B
                a �ł͍K�^���F��B
                """);
            return Task.CompletedTask;
        }

        public static void ���������ǉ��Ƒ���(string personId)
        {
            State.GetItem(HarItems.���̌�);
            State.GetItem(HarItems.���̊Z);
            Flags.Equip.TargetPersonId = personId;
            Flags.Equip[0] = HarItems.���̌�.Id;
            Flags.Equip[1] = HarItems.���̊Z.Id;
        }
    }
}
