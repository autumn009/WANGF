using ANGFLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using waSimpleRPGBase;
using static ANGFLib.SuperTalkCollections;

namespace waFirstRPG
{
    internal static class HarFight
    {
        /// <summary>
        /// 戦闘を行う
        /// </summary>
        /// <param name="monster"></param>
        /// <returns>trueなら勝利、falseなら敗北</returns>
        public static async Task<bool> FightToAsync(HarMonster monster)
        {
            var rand = new Random((int)Flags.Now.Ticks);
            monster.HPMP全回復();
            bool exit = false;
            DefaultPersons.Super.Say($"{monster.HumanReadableName}が現れた。");
            for (; ; )
            {
                if (SystemFile.IsDebugMode)
                {
                    DefaultPersons.システム.Say($"DEBUG: monster HP:{monster.GetHP()} MP:{monster.GetMP()}");
                }
                foreach (var personId in Party.EnumPartyMembers())
                {
                    State.GoTime(1);
                    var p = Person.List[personId];
                    await UI.SimpleMenuAsync($"{p.HumanReadableName}は、どうしますか?", new SimpleMenuItem[]{
                        new SimpleMenuItem("戦う", () =>
                        {
                            exit = attackSub(p,monster);
                            return true;
                        }),
                        new SimpleMenuItem("回復魔法", () =>
                        {
                            if( p.GetMP() <= 0 )
                            {
                                DefaultPersons.独白.Say("MPが足りません。");
                            }
                            else
                            {
                                DefaultPersons.独白.Say("全員のHPを回復します。");
                                RPGBaseUtil.全員HP全回復();
                                p.SetMP(p.GetMP()-1);
                            }
                            return true;
                        }),
                        new SimpleMenuItem("逃げる", () =>
                        {
                            DefaultPersons.独白.Say($"{p.HumanReadableName}は逃げ出した。");
                            DefaultPersons.独白.Say($"しかし逃げられない。");
                            return true;
                        }),
                    });
                    if (exit)
                    {
                        DefaultPersons.Super.Say($"{monster.HumanReadableName}を倒した。");
                        expGetter(monster);
                        switch (rand.Next(6))
                        {
                            case 0:
                                itemGetter(HarItems.鉄の剣);
                                break;
                            case 1:
                                itemGetter(HarItems.鉄の鎧);
                                break;
                        }
                        return true;
                    }
                }
                State.GoTime(1);
                var p1 = Person.List[Party.EnumPartyMembers().First()];
                exit = attackSub(monster, p1);
                if (exit)
                {
                    DefaultPersons.Super.Say($"{p1.HumanReadableName}は、{monster.HumanReadableName}に負けてしまった。");
                    DefaultPersons.Super.Say("戦闘敗北エンディング達成……");
                    State.SetCollection(Constants.EndingCollectionID, HarConstants.戦闘敗北エンディングID, null);
                    await State.WarpToAsync(HarConstants.EpilogueID);
                    return false;
                }
            }

            void expGetter(Person to)
            {
                int exp = 1;
                if (monster == HarMonsters.BossSlime) exp = 10;
                DefaultPersons.独白.Say($"経験値{exp}を手に入れた。");
                foreach (var personId in Party.EnumPartyMembers())
                {
                    var p = Person.List[personId];
                    var oldLevel = p.GetLevel();
                    p.SetEXP(p.GetEXP() + exp);
                    if(oldLevel < p.GetLevel() )
                    {
                        DefaultPersons.Super.Say($"{p.HumanReadableName}はレベルが上がった。レベル{p.GetLevel()}になった。");
                    }
                }
            }

            void itemGetter(Item item)
            {
                DefaultPersons.独白.Say($"{item.HumanReadableName}を手に入れました。");
                State.GetItem(item);
            }
        }

        /// <summary>
        /// 戦闘1回分
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>trueなら戦闘終了</returns>
        private static bool attackSub(Person from, Person to)
        {
            var old = Flags.Equip.TargetPersonId;
            try
            {
                DefaultPersons.独白.Say($"{from.HumanReadableName}は{to.HumanReadableName}に攻撃した。");
                // 武器の攻撃力の補正
                int attack = from.GetAttackValue();
                if (from is not HarMonster)
                {
                    Flags.Equip.TargetPersonId = from.Id;
                    if (!string.IsNullOrWhiteSpace(Flags.Equip[0]))
                    {
                        var item = Item.List[Flags.Equip[0]] as HarItem;
                        if (item != null) attack += item.Value;
                    }
                }
                // 防具の防御力の補正
                int defence = to.GetDefenceValue();
                if (to is not HarMonster)
                {
                    Flags.Equip.TargetPersonId = to.Id;
                    if (!string.IsNullOrWhiteSpace(Flags.Equip[1]))
                    {
                        var item = Item.List[Flags.Equip[1]] as HarItem;
                        if (item != null) defence += item.Value;
                    }
                }
                // ダメージ計算
                int damage = Math.Max(0, attack - defence / 2);
                if (damage <= 0)
                {
                    DefaultPersons.独白.Say($"{from.HumanReadableName}は、{to.HumanReadableName}にダメージを与えられない。");
                }
                else
                {
                    to.SetHP(Math.Max(0, to.GetHP() - damage));
                    DefaultPersons.独白.Say($"{to.HumanReadableName}は{damage}のダメージ。");
                }
                return to.GetHP() <= 0;
            }
            finally
            {
                Flags.Equip.TargetPersonId = old;
            }
        }
    }
}
