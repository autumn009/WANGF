using ANGFLib;
using waFirstRPG;
using System.Reflection;
using waSimpleRPGBase;

namespace waFirstRPG
{
    public class HarModule : ANGFLib.Module
    {
        public override string Id => HarConstants.ModuleID;
        public override Person[] GetPersons()
        {
            return new Person[]
            {
                HarPersons.マリア,
                HarPersons.レイ,
                HarPersons.リリ,
                HarPersons.アドバイスおじさん,
                HarPersons.経験値おじさん,
            };
        }
        public override ANGFLib.Place[] GetPlaces()
        {
            return Util.CollectTypedObjects<Place>(Assembly.GetExecutingAssembly()).Concat(General.GetAllRoads(typeof(HarRoad))).ToArray();
        }
        public override async Task<string> GetDefaultPlaceAsync() { await Task.Delay(0); return HarConstants.街ID; }
        public override string GetStartPlace() { return HarConstants.FrontMenuID; }
        public override EquipType[] GetEquipTypes()
        {
            return new EquipType[]
            {
                new EquipType(){ Name="武器", StatusName="Weapon",Priority=100 },
                new EquipType(){ Name="鎧", StatusName="Armor", Priority=200 },
            };
        }
        public override string FileExtention
        {
            get
            {
                return "har";
            }
        }
        public override MiniStatus[] GetStatuses()
        {
            return new MiniStatus[]
            {
                new FixedMiniStatus( ()=>HarFlags.名前,"{b3dbf285-1c08-41ba-b6dc-31be58ddd284}",100 ),
                new FixedMiniStatus( ()=>State.CurrentPlace.HumanReadableName,"{ad4be81b-df68-4416-953f-6251c6643f25}",200 ),
                new FixedMiniStatus( ()=>Coockers.PriceCoocker(Flags.所持金),"{9135b8ad-d273-42fb-a780-7efafe803797}",300 ),
            };
        }
        public override GetMyPersonNameInvoker GetMyPersonName
        {
            get { return myGetMyPersonName; }
        }

        public string myGetMyPersonName()
        {
            return HarFlags.名前;
        }

        public override Item[] GetItems()
        {
            return new Item[]
            {
                HarItems.青銅の剣,
                HarItems.青銅の鎧,
                HarItems.鉄の剣,
                HarItems.鉄の鎧,
            };
        }
        public override bool ConstructMenu(List<SimpleMenuItem> list, Place place)
        {

            var r = base.ConstructMenu(list, place);
            if (SystemFile.IsDebugMode)
            {
                list.Add(new SimpleMenuItem("強制戦闘スライム", async () =>
                {
                    await HarFight.FightToAsync(HarMonsters.Slime);
                    return true;
                }));
                list.Add(new SimpleMenuItem("強制戦闘ボススライム", async () =>
                {
                    await HarFight.FightToAsync(HarMonsters.BossSlime);
                    return true;
                }));
                list.Add(new SimpleMenuItem("強制鉄装備", () =>
                {
                    強制装備(HarItems.鉄の剣, HarItems.鉄の鎧);
                    return true;
                }));
                list.Add(new SimpleMenuItem("強制青銅装備", () =>
                {
                    強制装備(HarItems.青銅の剣, HarItems.青銅の鎧);
                    return true;
                }));
                list.Add(new SimpleMenuItem("レベル99強制", () =>
                {
                    foreach (var item in Party.EnumPartyMembers())
                    {
                        var p = Person.List[item];
                        p.SetEXP(271040785023476832L);
                    }
                    return true;
                }));
            }
            return r;

            void 強制装備(Item weapon, Item armor)
            {
                var old = Flags.Equip.TargetPersonId;
                try
                {
                    // 所持数が3個に足りないときは付け加える　(パーティーが1人2人でも矛盾は起きない)
                    while (State.GetItemCount(weapon) < 3) State.GetItem(weapon);
                    while (State.GetItemCount(armor) < 3) State.GetItem(armor);
                    // 全員に強制装備
                    foreach (var personId in Party.EnumPartyMembers())
                    {
                        Flags.Equip.TargetPersonId = personId;
                        Flags.Equip[0] = weapon.Id;
                        Flags.Equip[1] = armor.Id;
                    }
                }
                finally
                {
                    Flags.Equip.TargetPersonId = old;
                }
            }
        }
        public override Collection[] GetCollections()
        {
            return new Collection[]
            {
                new Collection()
                {
                    RawId = Constants.EndingCollectionID,
                    Name = "エンディング",
                    Collections = new CollectionItem[]
                    {
                        new CollectionItem(){Name="討伐成功",Id=HarConstants.グッドエンディングID},
                        new CollectionItem(){Name="戦闘敗北",Id=HarConstants.戦闘敗北エンディングID},
                    }
                }
            };
        }
    }
    public class MyFightingParameterCalculator : FightingParameterCalculator
    {
        public override int GetMaxHP(Person p)
        {
            if (p is HarMonster mon) return mon.MaxHP;
            return base.GetMaxHP(p);
        }
        public override int GetAttackValue(Person p)
        {
            if (p is HarMonster mon) return mon.AttackValue;
            return base.GetAttackValue(p);
        }
        public override int GetDefenceValue(Person p)
        {
            if (p is HarMonster mon) return mon.DefenceValue;
            return base.GetDefenceValue(p);
        }
    }

    public class HarModuleEx : ModuleEx
    {
#pragma warning disable CS8601 
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new HarModule() as T };
            if (typeof(T) == typeof(RPGbaseModeProvider)) return new T[] { new RPGbaseModeProvider() as T };
            if (typeof(T) == typeof(FightingParameterCalculator)) return new T[] { new MyFightingParameterCalculator() as T };
            return new T[0];
        }
#pragma warning restore CS8601
    }
}