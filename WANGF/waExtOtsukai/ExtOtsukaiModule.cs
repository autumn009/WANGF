using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using ANGFLib;

using System.Threading.Tasks;

namespace ExtOtsukai
{
    public static class ExtOtsykaiConstants
    {
        public const string ModuleID = "{C6C3184F-0284-4a61-9F98-EC34F2B1D83E}";
        public const string グッドエンディングID = "{A539AF5D-0E82-4058-86F6-C7125FB33B3D}";
        public const string ショップID = "{D397C7F6-6233-4bc8-BA0C-7C39F2D35DAA}";
        public const string ぼくんちID = "{4F613A48-517F-4ace-9BA9-44A8EB06FA0F}";
        public const string こうばんID = "{ABD7E508-8F28-4e1d-BD9C-C3A448C53E02}";
        public const string PrologueID = "{8FD63EB6-ED2F-44f0-9EFC-BD7F9849FEE1}";
        public const string EpilogueID = "{F78E4CBE-324B-47cf-ACEC-2C5BADFAA374}";
        public const string TシャツId = "{9D03FA3B-63B3-483f-8261-49785484E6B1}";
        public const string ズボンId = "{9717BF50-6948-424e-852F-E68A83BC4AE0}";
        public const string 警官ID = "{3486DF79-67B0-434b-A724-3EF33ACCA6F8}";
        public const string ママID = "{3627A164-A805-4b88-BF8A-FBC5AC097D0C}";
        public const string Ext主人公名SystemFlagID = "{169f1b37-af00-44f6-9823-335c4d178c30}";

        public static bool [] 上下服 = { true, true };
    }

    public static class Oha
    {
        public static Item Itemセクシー服 = new ItemTemplate()
            {
                AvailableEquipMap = ExtOtsykaiConstants.上下服,
                SameTimeEquipMap = ExtOtsykaiConstants.上下服,
                Max = 1,
                Id = "{8C2B15EE-0317-4332-89E0-FE1F869B0CFC}",
                Name = "セクシー服",
                BaseDescription = "セクシーな服で装いもバッチリだ。目指せ玉の輿!",
                Price = 5000,
            };
    }
    class HaoPlacePrologue : Place
    {
        public override string Id
        {
            get { return ExtOtsykaiConstants.PrologueID; }
        }
        public override bool ForceOverride
        {
            get
            {
                return true;
            }
        }
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            return false;
        }
        public override void OnEntering()
        {
            // プロローグは仮想の場なので、出入りは宣言しない
            State.Clear();
            Flags.所持金 = 20000;
            Flags.Now = new DateTime(2010,1,1,8,0,0);
            Flags.生活サイクル起点時間 = 8;
            State.今日の起床時刻 = new DateTime(Flags.Now.Year, Flags.Now.Month, Flags.Now.Day,
                Flags.生活サイクル起点時間, 0, 0);
            State.今日の就寝時刻 = State.今日の起床時刻.AddHours(16);

            // 装備セットの情報更新要求
            UI.Actions.ResetGameStatus();
        }

        public override void OnLeaveing()
        {
            // プロローグは仮想の場なので、出入りは宣言しない
        }

        public override string HumanReadableName
        {
            get { return "(プロローグ2)"; }
        }
        public override bool IsStatusHide
        {
            get { return true; }
        }

        public static string GetName()
        {
            string name = "";
            foreach (var n in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in n.GetTypes())
                {
                    if (t.Name == "HaoFlags")
                    {
                        FieldInfo mi = t.GetField("名前");
                        if (mi == null) continue;
                        name = (string)mi.GetValue(null);
                        break;
                    }
                }
            }
            return name;
        }

        public bool prologue()
        {
            var q = new QuickTalk();
            string name = GetName();
            q.AddMacro("n", name);
            q.Play(@"
m $nちゃん。いい加減、結婚しなさい。
アタシは尻を叩かれた。
");
            return true;
        }

        private async Task getPlayerNameAsync()
        {
            string name;
            for (; ; )
            {
                DefaultPersons.システム.Say("これより、名前を入力します。");

                name = await UI.Actions.enterPlayerNameAsync("名前を入力してください(例:ジュン、アッコ等): ", "ジュン");

                SimpleMenuItem[] items = {
                    new SimpleMenuItem("はい" ),
                    new SimpleMenuItem("いいえ" )
                };
                int selection = await UI.SimpleMenuWithoutSystemAsync(string.Format("「名前」={0}ちゃん　でよろしいですか?", name), items);
                if (selection == 0) break;
            }
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                foreach (var n in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in n.GetTypes())
                    {
                        if (t.Name == "HaoFlags")
                        {
                            FieldInfo mi = t.GetField("名前");
                            if (mi == null) continue;
                            mi.SetValue(null, name);
                            return;
                        }
                    }
                }
                await Task.Delay(0);
            });
        }

        private async Task startAsync()
        {
            State.GetItem(Items.GetItemByNumber(ExtOtsykaiConstants.TシャツId));
            State.GetItem(Items.GetItemByNumber(ExtOtsykaiConstants.ズボンId));
            prologue();
            await State.WarpToAsync(ExtOtsykaiConstants.ぼくんちID);
            General.NotifyNewGame();
        }

        private async Task silentStartAsync()
        {
            State.GetItem(Items.GetItemByNumber(ExtOtsykaiConstants.TシャツId));
            State.GetItem(Items.GetItemByNumber(ExtOtsykaiConstants.ズボンId));
            await State.WarpToAsync(ExtOtsykaiConstants.ぼくんちID);
            General.NotifyNewGame();
        }

        public override async Task OnMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            items.Add(new SimpleMenuItem("プロローグ有り (初回プレイ時推奨)"));
            items.Add(new SimpleMenuItem("プロローグ無し & 名前入力あり"));
            for (; ; )
            {
                int selection = await UI.SimpleMenuWithCancelAsync("開始方法を選択してください。", items.ToArray());
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
    public class PersonWithPlaceExt : PersonWithPlace
    {
        public override bool ForceOverride
        {
            get
            {
                return true;
            }
        }
        public PersonWithPlaceExt(string id, string name, Sex sex, string placeID, Func<Person, bool> isAvailable, Func<Person,Task> talk) : base(id, name, Color.Empty, sex, placeID, isAvailable, talk) { }
    }

    public class ExtTalkers
    {
        public static Person 警官 = new PersonWithPlaceExt(ExtOtsykaiConstants.警官ID, "警官", Sex.Male, ExtOtsykaiConstants.こうばんID, (x) => Flags.Now.Hour >= 8 && Flags.Now.Hour <= 18, async (x) =>
        {
            if( Flags.Equip[0] == Oha.Itemセクシー服.Id )
            {
                x.Say("けけけ、結婚してください!");
                DefaultPersons.主人公.Say("はい。喜んで。");
                DefaultPersons.独白.Say("こうして結婚しました。めでたしめでたし。");
                State.SetCollection(Constants.EndingCollectionID, ExtOtsykaiConstants.グッドエンディングID, null);
                SystemFile.SetFlagString(ExtOtsykaiConstants.Ext主人公名SystemFlagID, HaoPlacePrologue.GetName());
                await SystemFile.SaveIfDirtyAsync();
                DefaultPersons.システム.Say("お姉ちゃんのお使いのクリア特典がアンロックされました。");
                DefaultPersons.システム.Say("他のゲームで追加機能が使用できる場合があります。");
                await State.WarpToAsync(ExtOtsykaiConstants.EpilogueID);
            }
            else
            {
                x.Say("本官の目玉はつながっていないのであります。");
            }
        });
        public static Person ママ = new PersonWithPlaceExt(ExtOtsykaiConstants.ママID, "ママ", Sex.Female, ExtOtsykaiConstants.ぼくんちID, (x) => true, (x) =>
        {
            x.Say("いい年だから、結婚しなさい。");
            return Task.CompletedTask;
        });
    }

    public class OverwriteCollection : Collection
    {
        public override bool ForceOverride
        {
            get
            {
                return true;
            }
        }
    }

    public class ExtOtsukaiModule : ANGFLib.Module
    {
        public override string Id
        {
            get
            {
                return ExtOtsykaiConstants.ModuleID;
            }
        }
        public override ANGFLib.Place[] GetPlaces()
        {
            return Util.CollectTypedObjects<Place>(Assembly.GetExecutingAssembly());
        }
        public override Collection[] GetCollections()
        {
            return new OverwriteCollection[]
			{
                new OverwriteCollection()
                {
                    RawId = Constants.EndingCollectionID,
                    Name = "エンディング",
                    Collections = new CollectionItem[]
                    {
                        new CollectionItem(){Name="お姉ちゃんのおつかい成功",Id=ExtOtsykaiConstants.グッドエンディングID}
                    }
                }
            };
        }
        public override Person[] GetPersons()
        {
            return new Person[]
			{
                ExtTalkers.ママ,
                ExtTalkers.警官,
            };
        }
        public override Item[] GetItems()
        {
            return new Item[]
			{
                Oha.Itemセクシー服,
            };

        }
        public override ShopAndItemReleation[] GetShopAndItemReleations()
        {
            return new ShopAndItemReleation[]
            {
                new ShopAndItemReleation(ExtOtsykaiConstants.ショップID,Oha.Itemセクシー服.Id ),
            };
        }
        public override string FileExtention
        {
            get
            {
                return "oha";
            }
        }
        public override string GetQuickTalkPerson(string name)
        {
            return name == "m" ? ExtTalkers.ママ.Id : null;
        }
    }
    public class ExtOtsukaiModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new ExtOtsukaiModule() as T };
            return new T[0];
        }
    }
}
