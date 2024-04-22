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
                    DefaultPersons.独白.Say($"{p.HumanReadableName}は仲間になった。");
                    p.HPMP全回復();
                    HarPersons.初期装備追加と装備(p.Id);
                    Party.AddMember(p.Id);
                }
                else
                    DefaultPersons.独白.Say("これ以上仲間は増やせない。");
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
        public static Person マリア = new HarPerson(HarConstants.マリアID, "マリア", Sex.Female, HarConstants.街ID, HarFlags.MariaEquip);
        public static Person レイ= new HarPerson(HarConstants.レイID, "レイ", Sex.Female, HarConstants.街ID, HarFlags.ReiEquip);
        public static Person リリ = new HarPerson(HarConstants.リリID, "リリ", Sex.Female, HarConstants.街ID, HarFlags.RiriEquip);
        public static Person アドバイスおじさん = new PersonWithPlace("{87eaace9-6b14-43b9-b054-f6dd6be81181}", "アドバイスおじさん", Sex.Male, HarConstants.街ID, (p)=>true, adviceAsync);
        public static Person 経験値おじさん = new PersonWithPlace("{5f0b1070-613a-4652-b736-e336e0d28359}", "経験値おじさん", Sex.Male, HarConstants.街ID, (p) => true, exp);

        private static Task exp(Person obj)
        {
            foreach (var personId in Party.EnumPartyMembers())
            {
                var p = Person.List[personId];
                経験値おじさん.Say($"{p.HumanReadableName}の情報だ。");
                経験値おじさん.Say($"現在のレベルは{p.GetLevel()}だ。");
                経験値おじさん.Say($"現在の経験値は{p.GetEXP()}だ。");

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
                    経験値おじさん.Say("既にとても強い。");
                }
                else
                {
                    経験値おじさん.Say($"次のレベルに必要な経験値はあと{n - p.GetEXP()}だ。");
                }
            }
            return Task.CompletedTask;
        }

        private static Task adviceAsync(Person obj)/* DIABLE ASYNC WARN */
        {
            var q = new QuickTalk();
            q.AddTalker("a", アドバイスおじさん);
            q.Play("""
                a アドバイスしてやろう。
                a 魔王城にいるボススライムを倒すんだ。
                a 魔王城は冒険街道の先にあるぞ。
                a パーティーの最大人数は3人だ。味方はよく選べよ。
                a スライムを倒すと鉄の剣か鉄の鎧が手に入る。これを人数分手に入れて全員に装備するんだ。
                a いいか、装備を忘れるなよ。装備したい場所の名前をクリック/タップだ。
                a 戦闘中に魔法を使うとHP全員全回復の魔法が使えるぞ。使える魔法はそれだけだ。
                a 街でHPMPを回復する場合は宿屋を使え。MP回復する方法はこれしかないぞ。
                a モンスターは先頭のメンバーだけ攻撃する。
                a 先頭のメンバーのHPが0になるとゲームオーバーだ。
                a では幸運を祈る。
                """);
            return Task.CompletedTask;
        }

        public static void 初期装備追加と装備(string personId)
        {
            State.GetItem(HarItems.青銅の剣);
            State.GetItem(HarItems.青銅の鎧);
            Flags.Equip.TargetPersonId = personId;
            Flags.Equip[0] = HarItems.青銅の剣.Id;
            Flags.Equip[1] = HarItems.青銅の鎧.Id;
        }
    }
}
