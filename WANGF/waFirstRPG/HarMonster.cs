using ANGFLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waFirstRPG
{
    internal class HarMonster : Person
    {
        public int AttackValue { get; init; }
        public int DefenceValue { get; init; }
        public int MaxHP { get; init; }
        public HarMonster(string id, string name) : base(id, name, Sex.Nutral)
        {
        }
    }
    internal class HarMonsters
    {
        public static HarMonster Slime = new HarMonster("{1b51e907-6769-4120-b745-9ea1432951c5}", "スライム") { AttackValue = 15, DefenceValue = 40, MaxHP = 10 };
        public static HarMonster BossSlime = new HarMonster("{beedf89d-d909-4935-bf31-a9091e93abbe}", "ボススライム") { AttackValue = 80, DefenceValue = 50, MaxHP = 600 };
    }
}
