using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.IO;
using System.Web;
using ANGFLib;
using System.Threading.Tasks;

namespace waSimpleRPGBase
{
    public class RPGNamesStatus : MiniStatus
    {
        public override string Id
        {
            get { return "{63c23802-b377-4828-ba2f-ac6c6d89519d}"; }
        }
        public override string HumanReadableName
        {
            get { return "RPG名前ステータス"; }
        }
        public override int Priority
        {
            get
            {
                return 2000;
            }
        }
        public override bool IsVisible()
        {
            return RPGBaseMode.IsRPGMode();
        }
        public override string Text()
        {
            return string.Join(' ', Party.EnumPartyMembers().Select(c =>
            {
                var p = Person.List[c];
                return $"{p.HumanReadableName} Lv{p.GetLevel()} HP{p.GetHP()}/{p.GetMaxHP()} MP{p.GetMP()}/{p.GetMaxMP()}";
            }));
        }
    }

    public class RpgBaseModule : ANGFLib.Module
	{
		public override string Id
		{
			get
			{
				return "{448adc93-6ab8-43c6-bc68-3e8fcb973a27}";
			}
		}
        public override void ConstructSystemMenu(List<SimpleMenuItem> list, Place place)
        {
            if (SystemFile.IsDebugMode)
            {
                list.Add(new SimpleMenuItem("成長テーブル", () =>
                {
                    var calc = new FightingParameterCalculator();
                    for (int i = 1; i < RPGBaseLevelTable.PersonGrowingTable.Length; i++)
                    {
                        DefaultPersons.独白.Say($"Lv{i}: {RPGBaseLevelTable.PersonGrowingTable[i]} HP: {calc.GetMaxHP(i)} MP: {calc.GetMaxMP(i)} ATK: {calc.GetAttackValue(i)} DF:{calc.GetDefenceValue(i)}");
                    }
                    return true;
                }
                ));
            }
            base.ConstructSystemMenu(list, place);
        }
        public override MiniStatus[] GetStatuses()
        {
            return new MiniStatus[] {
                new RPGNamesStatus(),
            };
        }
    }
    public class RpgBaseModuleEx : ModuleEx
    {
#pragma warning disable CS8601 
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new RpgBaseModule() as T };
            return new T[0];
        }
#pragma warning restore CS8601
    }
}
