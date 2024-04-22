using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using ANGFLib;
using System.Threading.Tasks;
using waFirstOtsukaiRazor;

namespace FirstOtsukai
{
    public static class FirstOtsykaiConstants
    {
        public const string ModuleID = "{C3A9BACE-AC46-462b-A552-5E7D4FE96319}";
        public const string FrontMenuID = "{C2C415D4-1B29-4bb3-B65E-B66490AD78F1}";
        public const string PrologueID = "{8FD63EB6-ED2F-44f0-9EFC-BD7F9849FEE1}";
        public const string EpilogueID = "{F78E4CBE-324B-47cf-ACEC-2C5BADFAA374}";
        public const string 我が家ID = "{4F613A48-517F-4ace-9BA9-44A8EB06FA0F}";
        public const string こうばんID = "{ABD7E508-8F28-4e1d-BD9C-C3A448C53E02}";
        public const string おみせID = "{6C04D463-F7DA-400e-9FC5-A4DCE2625797}";
        public const string グッドエンディングID = "{8D6645C0-4F5B-4a5d-805F-AF7A40576518}";
        public const string 時間切れエンディングID = "{CBB6DDA0-C98F-4036-A011-1A9CB8EB4F5C}";
        public const string ショップID = "{D397C7F6-6233-4bc8-BA0C-7C39F2D35DAA}";
        public const string 警官ID = "{3486DF79-67B0-434b-A724-3EF33ACCA6F8}";
        public const string ママID = "{3627A164-A805-4b88-BF8A-FBC5AC097D0C}";
        public const string 時間切れスケジュールID = "{F9745D2D-09A1-4ee2-82FA-5A49F2D62EF1}";
        public const string 百円拾うスケジュールID = "{B09A5C58-0C96-437f-BFF1-31A021E28587}";
        public const string 街ID = "{044caa22-10ce-4435-b10d-2738d47d5b72}";
        public const string 魔王城ID = "{bcefae12-ab14-406b-9114-d5421e9eceb5}";
        public const string ワープおじさんID1 = "{cee9e9c7-2280-4708-a281-9452ebd079dd}";
        public const string ワープおじさんID2 = "{9f49be05-7ea4-46fc-9c40-f35009635c48}";
        public const string 魔王ID = "{caa82a03-0890-42d8-a512-ab3b51963647}";
        public const string Hao主人公名SystemFlagID = "{f4aa091c-3b14-495f-b514-2222583dc80a}";
    }

    public class HaoModule : ANGFLib.Module
    {
        public override string Id
        {
            get
            {
                return FirstOtsykaiConstants.ModuleID;
            }
        }
        public override Person[] GetPersons()
        {
            return new Person[]
			{
                HaoPersons.ママ,
                HaoPersons.警官,
                HaoPersons.ワープおじさん1,
                HaoPersons.ワープおじさん2,
                HaoPersons.魔王,
            };
        }
        public override ANGFLib.Place[] GetPlaces()
        {
            return Util.CollectTypedObjects<Place>(Assembly.GetExecutingAssembly());
        }
        public override async Task<string> GetDefaultPlaceAsync() { await Task.Delay(0); return FirstOtsykaiConstants.我が家ID; }
        public override string GetStartPlace() { return FirstOtsykaiConstants.FrontMenuID; }

        private async Task testCallAnyMethodSubAsync()
        {
            //dynamic d = UI.Actions.GetMainForm();
            //await d.testMethod();
            System.Diagnostics.Debug.WriteLine("testCallAnyMethodSubAsync called");/* DIABLE ASYNC WARN */
            await Task.Delay(0);
        }

        public override MenuItem[] GetExtendMenu()
        {
            if (ANGFLib.SystemFile.IsDebugMode)
            {
                return new MenuItem[] {
                new MenuItem(){ Label="追加例1", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                    DefaultPersons.システム.Say("追加例1です。");
                    await Task.Delay(0);
                }},
                new MenuItem(){ Label="追加例2", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                    DefaultPersons.システム.Say("追加例2です。");
                    await Task.Delay(0);
                }},
                new MenuItem(){ Label="選択付き追加例", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                    DefaultPersons.システム.Say("選択付き追加例です。");
                    var r = await UI.YesNoMenuAsync("選択して下さい","はい","いいえ");
                    DefaultPersons.システム.Say($"選択は{r}です。");
                }},
                new MenuItem(){ Label="リッチメニューの例", MenuType= MyMenuType.Info, MethodAsync= async (mainForm)=>{
                    var list = new SimpleMenuItem[]
                    {
                        new SimpleMenuItem("その1"){Explanation="説明テキスト1" },
                        new SimpleMenuItem("その2"){Explanation="説明テキスト2" },
                        new SimpleMenuItem("その3"){Explanation="説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3" },
                    };
                    int index = await UI.SimpleMenuWithCancelAsync("選択してください",list);
                    if( index < 0 ) return;
                    DefaultPersons.システム.Say(list[index].Explanation);
                }},
                new MenuItem(){ Label="tellAssert例", MenuType= MyMenuType.Top, MethodAsync=async (mainForm)=>{
                    await UI.Actions.tellAssertionFailedAsync("アサートメッセージ");
                }},
                new MenuItem(){ Label="CallAnyMethodAsync例", MenuType= MyMenuType.Top, MethodAsync=async (mainForm)=>{
                    await UI.Actions.CallAnyMethodAsync(testCallAnyMethodSubAsync);
                }},
                new MenuItem(){ Label="selectItem利用例", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                    DefaultPersons.システム.Say("選択したアイテムのIDを出力します。");
                    var itemId = await UI.Actions.selectOneItemAsync();
                    if (itemId == null) itemId = "(null)";
                    DefaultPersons.システム.Say(itemId);
                  }},
                new MenuItem(){ Label="htmlForm利用例", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                    System.Reflection.Assembly myAssembly =
    System.Reflection.Assembly.GetExecutingAssembly();
                    var s1 = General.LoadEmbededResourceAsText(myAssembly, "FirstOtsukai.testPage001.html");
                    var s2 = General.LoadEmbededResourceAsText(myAssembly, "FirstOtsukai.testPage001.js");

                    var dic = await UI.Actions.HtmlFormAsync("テストフォーム",s1 + "\r\n<script>\r\n" + s2 + "\r\n</script>");
                    foreach (var item in dic.Keys.Where(c=>c.StartsWith("htmlform_")))
                    {
                        DefaultPersons.システム.Say("{0}={1}", item, dic[item]);
                    }
                  }},
                    new MenuItem(){ Label="カスタムコンポーネント例", MenuType= MyMenuType.Info, MethodAsync=async (mainForm)=>{
                        var s = await UI.Actions.OpenCustomRazorComponentAsync(typeof(Component1));
                        DefaultPersons.システム.Say($"selected result is {s}");
                    }},

//#if DEBUG
                new MenuItem(){ Label="追加例3 (例外発生)", MenuType= MyMenuType.Info, MethodAsync=async(mainForm)=>{
                    new int[0].First();
                    await Task.Delay(0);
                }},
                new MenuItem(){ Label="追加例4 (例外発生)", MenuType= MyMenuType.Top, MethodAsync=async (mainForm)=>{
                    new int[0].First();
                    await Task.Delay(0);
                }},
//#endif
    			};
            }
            else
            {
                return new MenuItem[0];
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
                        new CollectionItem(){Name="おつかい成功",Id=FirstOtsykaiConstants.グッドエンディングID},
                        new CollectionItem(){Name="時間切れ",Id=FirstOtsykaiConstants.時間切れエンディングID},
                    }
                }
            };
        }
        public override Item[] GetItems()
        {
            return new Item[]
			{
                HaoItems.Itemしょうゆ,
                HaoItems.Tシャツ,
                HaoItems.ズボン,
                HaoItems.Itemジュース,
                HaoItems.つなぎ,
            };
        }
        public override Schedule[] GetSchedules()
        {
            return new Schedule[]
			{
                new HaoScheduleStopper(),
                new HaoSchedule100Yes(),
            };
        }
        public override EquipType[] GetEquipTypes()
        {
            return new EquipType[]
            {
                new EquipType(){ Name="上服", StatusName="Top", Priority=100 },
                new EquipType(){ Name="下服", StatusName="Bottom",Priority=200 },
            };
        }
        public override Shop[] GetShops()
        {
            return new Shop[]
            {
                new Shop(FirstOtsykaiConstants.ショップID, "ショップ1" ),
            };
        }
        public override ShopAndItemReleation[] GetShopAndItemReleations()
        {
            return new ShopAndItemReleation[]
            {
                new ShopAndItemReleation(FirstOtsykaiConstants.ショップID,HaoItems.Itemジュース.Id ),
                new ShopAndItemReleation(FirstOtsykaiConstants.ショップID,HaoItems.Itemしょうゆ.Id ),
                new ShopAndItemReleation(FirstOtsykaiConstants.ショップID,HaoItems.つなぎ.Id ),
            };
        }

        public override string GetQuickTalkMacro(string name)
        {
            return name == "n" ? HaoFlags.名前 : null;
        }
        public override FutureEquipSimulationInvoker FutureEquipSimulation
        {
            get
            {
                return myFutureEquipSimulation;
            }
        }
        private bool myFutureEquipSimulation(EquipSet set, out string msg)
        {
            if (set.AllItems[0].IsItemNull || set.AllItems[1].IsItemNull)
            {
                msg = "装備NG";
            }
            else
            {
                msg = "装備OK";
            }
            return true;
        }
        public override string FileExtention
        {
            get
            {
                return "hao";
            }
        }
        public override MiniStatus[] GetStatuses()
        {
            return new MiniStatus[]
            {
                new FixedMiniStatus( ()=>HaoFlags.名前,"{61A0F2D2-853A-4b61-97C8-EF084E5C6FF5}",100 ),
                new FixedMiniStatus( ()=>State.CurrentPlace.HumanReadableName,"{F146AFC5-9850-428b-826C-8202F7A1C81F}",200 ),
                new FixedMiniStatus( ()=>Coockers.PriceCoocker(Flags.所持金),"{454EF8A9-0BD1-42b6-86F3-628745B5EE8E}",300 ),
            };
        }

        public override GetDateColorInvoker GetDateColor
        {
            get
            {
                return MyGetDateColor;
            }
        }

        public override IsDateTimeInvoker IsRedDay
        {
            get
            {
                return MyIsRedDay;
            }
        }
        public override IsDateTimeInvoker IsBlueDay
        {
            get
            {
                return MyIsBlueDay;
            }
        }

        public virtual bool MyIsRedDay(DateTime dt)
        {
            return dt.DayOfWeek == DayOfWeek.Sunday;
        }

        public virtual bool MyIsBlueDay(DateTime dt)
        {
            return dt.DayOfWeek == DayOfWeek.Saturday;
        }

        public virtual System.Drawing.Color MyGetDateColor(DateTime dt, bool red, bool blue)
        {
            if (red) return System.Drawing.Color.FromArgb(255, 192, 192);
            else if (blue) return System.Drawing.Color.FromArgb(192, 192, 255);
            else return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        public override GetMyPersonNameInvoker GetMyPersonName
        {
            get { return myGetMyPersonName; }
        }

        public string myGetMyPersonName()
        {
            return HaoFlags.名前;
        }

        public override GetBackColorInvoker GetBackColor
        {
            get
            {
                return myGetBackColor;
            }
        }

        private System.Drawing.Color myGetBackColor()
        {
            if (Flags.Now.Hour < 6)
            {
                return System.Drawing.Color.FromArgb(0, 0, 0);
            }
            else if (Flags.Now.Hour < 12)
            {
                return System.Drawing.Color.FromArgb(0, 0, 192);
            }
            else if (Flags.Now.Hour < 17)
            {
                return System.Drawing.Color.FromArgb(96, 96, 192);
            }
            else if (Flags.Now.Hour < 19)
            {
                return System.Drawing.Color.FromArgb(128, 64, 64);
            }
            else if (Flags.Now.Hour < 23)
            {
                return System.Drawing.Color.FromArgb(0, 0, 128);
            }
            else
            {
                return System.Drawing.Color.FromArgb(0, 0, 0);
            }
        }
        public override MenuStopControls StopMenus()
        {
            return MenuStopControls.None;
        }
        public override void ConstructSystemMenu(List<SimpleMenuItem> list, Place place)
        {
            list.Add(new SimpleMenuItem("何もしない。", () =>
            {
                DefaultPersons.独白.Say("はぁ。めんどうくさいなあ。");
                return true;
            }));
            list.Add(new SimpleMenuItem("リッチメニューの例", async () =>
            {
                var list = new SimpleMenuItem[]
                {
                        new SimpleMenuItem("その1"){Explanation="説明テキスト1" },
                        new SimpleMenuItem("その2"){Explanation="説明テキスト2" },
                        new SimpleMenuItem("その3"){Explanation="説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3説明テキスト3" },
                };
                int index = await UI.SimpleMenuWithCancelAsync("選択してください", list);
                if (index < 0) return false;
                DefaultPersons.システム.Say(list[index].Explanation);
                return true;
            }));
            return;
        }
        public override void WriteReport(System.IO.TextWriter writer, bool forDebug)
        {
            writer.WriteLine("● はじめてお使いレポート");
            writer.WriteLine("名前: {0}", HaoFlags.名前 ?? "(なし)");
            writer.WriteLine();
        }
    }
    public class HaoSoldNotify : SoldNotify
    {
        public override void NotifyItemWasSold(string itemId, int count)
        {
            var item = Item.List[itemId];
            DefaultPersons.システム.Say("アイテム{0}を売ったという通知を受け取りました。", item.HumanReadableName);
        }
    }

    public class HaoModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new HaoModule() as T };
            if (typeof(T) == typeof(World)) return new T[] { new AnotherWorld() as T, new ThisWorld() as T };
            if (typeof(T) == typeof(ANGFLib.SoldNotify)) return new T[] { new HaoSoldNotify() as T };
            return new T[0];
        }
    }
}
