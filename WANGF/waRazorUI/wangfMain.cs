using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ANGFLib;
using wangflib;
//using FirstOtsukai;

namespace waRazorUI
{
    public class wangfMain
    {
        //public static string GetMainAreaHtmlString()
        //{
        //    return "<p>This is created by C# code</p>";
        //}
        //static wangfMain()
        //{
        //initMain();
        //}

        public static async Task GetBatchTestingAsync()
        {
            HtmlGenerator.EnumBatchTestingResults = await BatchTest.BatchTestingForFramework.EnumResultsAsync();
            HtmlGenerator.IsBatchTestingResultEnable = HtmlGenerator.EnumBatchTestingResults.Count() > 0;
        }

        public static async Task InitializeAsync()
        {
#if CLEAR_STATIC_INFOS_BEFORE_START
            // 現在あるSimpleName<T>の全てのインスタンスをクリアする
            // リロードが完全に行われずに再実行された場合の保険
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assem.GetTypes())
                {
                    if (type.BaseType != null && type.BaseType.Name.Contains("SimpleName"))
                    {
                        //System.Diagnostics.Debug.WriteLine($"found type {type.FullName}");
                        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy))
                        {
                            if (field.Name == "List")
                            {
                                //System.Diagnostics.Debug.WriteLine($"found field {field.Name}");
                                //System.Diagnostics.Debug.WriteLine($"found FieldType {field.FieldType.Name}");
                                if (field.FieldType.Name.Contains("Dictionary`2"))
                                {
                                    //System.Diagnostics.Debug.WriteLine($"clear {type.FullName}");
                                    dynamic v = field.GetValue(null);
                                    v.Clear();
                                }
                            }
                        }
                    }
                }
            }
#endif

            // Batch Testingの初期化
            var impl = new BatchTestingImpl();
            BatchTest.BatchTestingForEachTitle = impl;
            BatchTest.BatchTestingForFramework = impl;

            // Batch Testingの結果取得
            await GetBatchTestingAsync();

            ScenarioThread.InitUIActions();
            State.PlatformName = "Blazor";
            General.ReportError = (message) =>
            {
                //throw new ApplicationException(message);
                DefaultPersons.システム.Say(message);
                return new Dummy();
            };
            JournalInit.Init(General.IsBlazorWebAssembly() ? new MyJournalingStringBuilderWriter() : new MyJournalingFileWriter());
            try
            {
                await SystemFile.LoadAsync();
                await MessageSkipper.ReloadAsync();
            }
            catch (ApplicationException e)
            {
                wangfUtil.ReportErrorFromMainThread(e.Message);
            }
            State.SetEquipSet(State.装備なし, new EquipSet());
            // 選択に先立って全てのモジュールのXMLファイルを読み込んで一覧を作る
            //Scenarios.Modules = Scenarios.SeekModulesFromRegistryOnly(null).ToArray();
            //System.Diagnostics.Debug.WriteLine("call wangfUtil.EnumBuildinModules");
            //Scenarios.Modules = wangfUtil.EnumBuildinModules().ToArray();
            //System.Diagnostics.Debug.WriteLine("done wangfUtil.EnumBuildinModules");
        }

        public static RawMyXmlDoc[] StartModules()
        {
            //return Scenarios.SeekModulesFromRegistryOnly(null).Where(c => c.startupModule).OrderByDescending(c => SystemFile.GetTimeStamp(c.id)).ToArray();
            return Scenarios.Modules.Where(c => c.startupModule).OrderByDescending(c => SystemFile.GetTimeStamp(c.id)).ToArray();
        }

        public static async Task ExtensibleStartupAsync()
        {
            await Task.Delay(0);
            HtmlGenerator.CreateMainMenu();
            ScenarioThread.RunScenarioMain();
        }
        public static async Task StartupAsync(Type[] moduleExs)
        {
            //Console.WriteLine("1:"+moduleExs.Length.ToString());
            List<ModuleEx> mods = new List<ModuleEx>();
            string startPlaceId = "";
            Module last = null;
            foreach (var moduleEx in moduleExs)
            {
                ModuleEx thismodex = (ModuleEx)Activator.CreateInstance(moduleEx);
                mods.Add(thismodex);
                var xgetter = thismodex.QueryObjects<XDocument>();
                var myxdoc = new MyXmlDoc();
                if (xgetter.Length == 0)
                {
                    var name = thismodex.GetType().Assembly.ManifestModule.Name;
                    var resname = System.IO.Path.GetFileNameWithoutExtension(name) + ".AngfRunTime.xml";
                    var stream = thismodex.GetType().Assembly.GetManifestResourceStream(resname);
                    myxdoc.LoadFromXDocument(XDocument.Load(stream), name);
                }
                else
                    myxdoc.LoadFromXDocument(xgetter[0], "NO FILE NAME");
                var r = new ReferModuleInfo()
                {
                    Id = myxdoc.id,
                    FullPath = null,
                    Name = myxdoc.name,
                    MinVersion = myxdoc.MinVersion,
                };
                thismodex.SetXmlModuleData(r);
                thismodex.GetModules().First().SetXmlModuleData(r);
                var thismods = thismodex.GetModules();
                foreach (var thismod in thismods)
                {
                    // 以下はシステムモジュールは実行されず、この機能は使えない
                    thismod.SetXmlModuleData(r);
                    thismod.SetAngfRuntimeXml(myxdoc);
                    last = thismod;
                }
                var hints = thismodex.QueryObjects<GameHintInfo>();
                foreach (var hint in hints) HtmlGenerator.GameHintUrl = hint.GameHintUrl;

                var s = await Scenarios.LoadObjectAsync(thismodex);
                if (s != null) startPlaceId = s;
            }
            State.LoadedModulesEx = mods.ToArray();
            State.loadedModules = State.LoadedModulesEx.SelectMany(c => c.GetModules()).ToArray();

            SystemFile.SetLastPlayDateTime(last.Id);
            await SystemFile.SaveIfDirtyAsync();
            //System.Diagnostics.Debug.WriteLine("1");
            //Flags.CurrentPlaceId = last.GetStartPlace();
            Flags.CurrentPlaceId = startPlaceId;
            Flags.CurrentWorldId = Constants.DefaultWordId;
            HtmlGenerator.CreateMainMenu();
            //System.Diagnostics.Debug.WriteLine("2");
            //Console.WriteLine("999");
            ScenarioThread.RunScenarioMain();
        }

        public string UserId
        {
            get { return State.UserId; }
            set { State.UserId = value; }
        }

        public void Dispose()
        {
            //ANGFLib.MessageSkipper.Save();
        }

        public string GetAboutInfoHtmlFragment(bool session18k)
        {
            var sb = new StringBuilder();
            foreach (var n in State.LoadedModulesEx)
            {
                foreach (var mod in n.GetModules())
                {
                    var version = ANGFLib.Scenarios.AssemblyGetVersion(mod.GetXmlModuleData()).ToString();
                    if (!mod.GetAngfRuntimeXml().is18k || session18k)
                    {
                        sb.Append("<h3>");
                        sb.Append(WebUtility.HtmlEncode(mod.GetAngfRuntimeXml().name));
                        sb.Append(" Version ");
                        sb.Append(WebUtility.HtmlEncode(version));
                        sb.AppendLine("</h3>");
                        foreach (var item in n.QueryObjects<AngfVersionInfo>())
                        {
                            item.GetHtmlFragment(sb);
                        }
                        foreach (var item in mod.GetOfficialSiteUrl())
                        {
                            sb.Append("<a data-role='button' href='");
                            sb.Append(item.Uri);
                            sb.Append("'>");
                            sb.Append(WebUtility.HtmlEncode(item.Name));
                            sb.AppendLine("</a>");
                        }
                    }
                }
            }
            return sb.ToString();
        }

    }
}
