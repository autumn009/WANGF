using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Web;
using Microsoft.Win32;
using System.ComponentModel.Design.Serialization;

namespace ANGFLib
{
    /// <summary>
    /// シナリオ管理オブジェクト
    /// </summary>
    public static class Scenarios
    {
        private static MyXmlDoc[] modules;
        public static void SetModules(MyXmlDoc[] modules) => Scenarios.modules = modules;
        private static MyXmlDoc seekXmlDoc(string id)
        {
            // Modulesは先だって読み込まれて構築済みであるはずだ
            var query = from n in modules where n.id == id select n;
            return query.FirstOrDefault();
        }

        private static void seekDependentRecuresive(MyXmlDoc doc, List<ReferModuleInfoEx> toAdd)
        {
            if (doc == null) return;

            // 既に登録済みなら再度登録はしない
            var found = toAdd.FirstOrDefault((c) => c.Id == doc.id);
            if (found != null) return;

            foreach (var n in doc.require)
            {
                System.Diagnostics.Debug.Assert(n.Id.Length > 0);
                var xmlDoc = seekXmlDoc(n.Id);
                if (xmlDoc == null)
                {
                    General.ReportError(string.Format("{0} モジュール({1})が見つかりません。", n.Name, n.Id));
                }
                seekDependentRecuresive(xmlDoc, toAdd);
            }
            //if (!string.IsNullOrWhiteSpace(doc.ModuleDllFilePath))
            //{
            // リスト追加は常に参照先よりも後になる。置き換え可能にするためである
            toAdd.Add(new ReferModuleInfoEx() { Id = doc.id, FullPath = doc.ModuleDllFilePath, Name = doc.name, MinVersion = doc.MinVersion, AngfRuntimeXml = doc });
            //}
        }

        private static Version fillVersionRecurve(string id, ReferModuleInfo r, Version current)
        {
            var doc = seekXmlDoc(r.Id);
            if (doc == null) return current;
            if (doc.shareWorld == id && doc.shareWorldMinVersion != null)
            {
                if (doc.shareWorldMinVersion >= current) current = doc.shareWorldMinVersion;
            }
            foreach (var n in doc.require)
            {
                if (n.Id == id && n.MinVersion != null)
                {
                    if (n.MinVersion >= current) current = n.MinVersion;
                }
            }
            return current;
        }

        private static MyXmlDoc[] shareWorldRefered(List<ReferModuleInfoEx> assembliesToRead)
        {
            List<MyXmlDoc> list = new List<MyXmlDoc>();
            // まだリストに入っておらず、既存モジュールをshareworldとして参照しているか
            foreach (var n in modules)
            {
                // 既にロード確定済みのモジュールは判定外 (厳密な判定はあとでもやるが、ここで刈り込んでおかないと無限ループする
                if (assembliesToRead.FirstOrDefault((c) => c.Id == n.id) != null) continue;
                // そのモジュールが参照しているshareworld対象モジュール一覧
                var query = from c in assembliesToRead where c.Id == n.shareWorld && n.shareWorld.Trim().Length > 0 select c;
                if (query.Count() > 0) list.Add(n);
            }
            return list.ToArray();
        }

        private static List<Assembly> loadedAssebliesToKeep = new List<Assembly>();
        /// <summary>
        /// シナリオを読み込む (依存関係のあるすべてのアセンブリを読む)
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>開始場所のId。nullか空文字列なら無い</returns>
        public static async System.Threading.Tasks.Task<string> LoadAsync(MyXmlDoc doc)
        {
            // 必要なトラップ
            System.Diagnostics.Trace.Assert(loadedAssebliesToKeep.Count == 0, "ANGF.Scenarios.Loadメソッドの呼び出しは1回限りです。");

            // リスト作成
            List<ReferModuleInfoEx> assembliesToRead = new List<ReferModuleInfoEx>();
            // seek システム拡張 (システム拡張のモジュールは他のモジュールに先立って読み込まれねばならない)
            assembliesToRead.AddRange(modules.Where(c => c.shareWorld == "ANGFSYSTEM").Select(c => new ReferModuleInfoEx() { Id = c.id, FullPath = c.ModuleDllFilePath, Name = c.name, MinVersion = c.MinVersion, AngfRuntimeXml = c }));

            // seek targets
            seekDependentRecuresive(doc, assembliesToRead);

            // シェアワールドを探してリストに追加
            // これは順番として常に後になることが保証されねばならない
            for (; ; )
            {
                // 既に読み込み対象になったモジュールのシェアワールド一覧を得る
                MyXmlDoc[] ar = shareWorldRefered(assembliesToRead);
                // リストが空になったら必要なモジュールは全てリストされたことになる
                if (ar.Length == 0) break;
                // 結果をシェアワールドとして読み込み対象とする
                foreach (var m in ar) seekDependentRecuresive(m, assembliesToRead);
            }

            List<ModuleEx> mods = new List<ModuleEx>();
            string startPlaceId = null;

            // 必要バージョンを埋める
            foreach (var n in assembliesToRead)
            {
                var current = new Version();
                foreach (var m in assembliesToRead)
                {
                    current = fillVersionRecurve(n.Id, m, current);
                }
                n.MinVersion = current;
                //System.Diagnostics.Debug.WriteLine("module:" + n.Name + " MinVersion=" + n.MinVersion);
            }

            /*
            // 1プロセスの途中で再ロードというシチュエーションがあるなら、クリアが必要
            Places.Clear();
            Persons.Clear();
            Endings.Clear();
            Items.Clear();
            Equips.Clear();
            */
            // 読み込む
            loadedAssebliesToKeep.Clear();
            foreach (var n in assembliesToRead)
            {
                bool notReuireAssembly;
                //System.Diagnostics.Debug.WriteLine("1");
                Assembly m = assemblyLoadFromSub(n, out notReuireAssembly);
                if (m == null && !notReuireAssembly)
                {
                    General.ReportError("モジュール" + n.Name + "が見つかりません。");
                }
                // システムモジュールは除外する
                if (m == null && n.Id != "{93029964-704B-4d38-BE1D-EDB6182602E8}")
                {
#if MYBLAZORAPP
#else
                    if (!State.IsInWebPlayer)
                    {
                        System.Diagnostics.Debug.WriteLine("4");
                        // モジュールに対応するセンブリは存在しないが代理モジュールを今ここで構築する
                        // ただし、Webプレイヤーの時は機能させない
                        var sb = new StringBuilder();
                        string source = null;
                        m = DynamicBuildAssembly.CreateAssembly(n, sb, out source);
                        if (m == null)
                        {
                            General.ReportDynamicBuildError(n, sb, source);
                            continue;
                        }
                    }
#endif
                }
                //System.Diagnostics.Debug.WriteLine("2");
                if (m != null)
                {
                    // バージョンチェック
                    // 動的に生成されたモジュールにバージョンは無いのでチェックをパスする
                    if (!string.IsNullOrWhiteSpace(m.Location))
                    {
                        System.Diagnostics.FileVersionInfo ver =
                            System.Diagnostics.FileVersionInfo.GetVersionInfo(m.Location);
                        if (n.MinVersion > Version.Parse(ver.FileVersion))
                        {
                            General.ReportError("モジュール" + n.Name + "は少なくともFileVersion:" + n.MinVersion.ToString() + "が要求されましたが、読み込まれたのはFileVersion:" + ver.ToString() + "でした。バージョンアップが必要です。");
                        }
                    }

                    loadedAssebliesToKeep.Add(m);
#if !DEBUG
                    //try
                    //{
#endif
                    var q = from module in m.GetModules()
                            from type in module.GetTypes()
                            where type.IsSubclassOf(typeof(ModuleEx)) && !type.IsAbstract
                            select type;
                    if (q.Count() > 0)
                    {
                        foreach (var foundType in q)
                        {
                            ModuleEx thismodex = (ModuleEx)Activator.CreateInstance(foundType);
                            mods.Add(thismodex);
                            thismodex.SetXmlModuleData(n);
                            var thismods = thismodex.GetModules();
                            foreach (var thismod in thismods)
                            {
                                // 以下はシステムモジュールは実行されず、この機能は使えない
                                thismod.SetXmlModuleData(n);
                                thismod.SetAngfRuntimeXml(n.AngfRuntimeXml);
                            }
                            var id = await LoadObjectAsync(thismodex);
                            if (id != null) startPlaceId = id;
                        }
                    }
                    else
                    {
                        var q2 = from module in m.GetModules()
                                 from type in module.GetTypes()
                                 where type.IsSubclassOf(typeof(ANGFLib.Module)) && !type.IsAbstract
                                 select type;
                        foreach (var foundType in q2)
                        {
                            ANGFLib.Module thismod = (ANGFLib.Module)Activator.CreateInstance(foundType);
                            var thismodex = new CompatibleWrappingModuleEx(thismod);
                            mods.Add(thismodex);
                            // 以下はシステムモジュールは実行されず、この機能は使えない
                            thismod.SetXmlModuleData(n);
                            thismod.SetAngfRuntimeXml(n.AngfRuntimeXml);
                            var id = await LoadObjectAsync(thismodex);
                            if (id != null) startPlaceId = id;
                        }
                    }
#if !DEBUG
                    //}
                    //catch (Exception e)
                    //{
                    //System.Windows.Forms.MessageBox.Show(UI.Actions.GetMainForm(),
                    //    "モジュール" + n.Name + "で例外が発生しました。このモジュールは読み込まれません。\r\n" + e.ToString());
                    //}
#endif
                }
                //System.Diagnostics.Debug.WriteLine("3");
            }
            State.LoadedModulesEx = mods.ToArray();
            State.loadedModules = State.LoadedModulesEx.SelectMany(c => c.GetModules()).ToArray();

            Collections.ValidateCollections();

            //System.Diagnostics.Debug.WriteLine("9");

            // アイテム名バリデーション
            foreach (var n in SimpleName<Item>.List.Values) System.Diagnostics.Trace.Assert(!n.HumanReadableName.Contains(' '), n.HumanReadableName + " has a space character");

            return startPlaceId;
        }

        private static string assemblyLoadFileName(string fullpath)
        {
            // GACには対応していない
            // モジュールはGACに入らない
            // そもそもFile.ExistsがGACに反応しない
            if (File.Exists(fullpath)) return fullpath;
            // テストプロジェクト時のパスを判定
#if UPGRADE
            const string alter = @"bin\release upgrade";
#elif TRIAL
            const string alter = @"bin\release trial";
#elif DEBUG
            const string alter = @"bin\debug";
#else
            const string alter = @"bin\release";
#endif
            string alterPath = Path.Combine(Path.Combine(Path.GetDirectoryName(fullpath), alter), Path.GetFileName(fullpath));
            if (File.Exists(alterPath)) return alterPath;
            return null;
        }
        /// <summary>
        /// 指定モジュールのバージョン情報を取得します
        /// </summary>
        /// <param name="refModInfo"></param>
        /// <returns></returns>
        public static Version AssemblyGetVersion(ReferModuleInfo refModInfo)
        {
            var module = Scenarios.modules.First(c => c.id == refModInfo.Id);
            if (refModInfo.FullPath != null && (refModInfo.FullPath.ToLower().EndsWith(".dll") || refModInfo.FullPath.ToLower().EndsWith(".tmp")))
            {
                return module.versionInfo;
            }
            else
            {
                XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
                if (module.moduleEx != null)
                {
                    if (module.moduleEx.Element(ns + "Version") != null) return Version.Parse(module.moduleEx.Element(ns + "Version").Value);
                }
            }
            return null;
        }
        private static Assembly assemblyLoadFromSub(ReferModuleInfo refModInfo, out bool notRequired)
        {
            notRequired = false;
            //var targetModule = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((c) => c.GetName().Name == Path.GetFileNameWithoutExtension(refModInfo.FullPath));
            //if (targetModule != null) return targetModule;

            // バージョン一覧を満たす
            foreach (var n in Scenarios.modules.Where((c) => c.id == refModInfo.Id)) n.versionInfo = AssemblyGetVersion(refModInfo);

            // 候補一覧を得る
            var q = from n in Scenarios.modules
                    where n.id == refModInfo.Id
                    select n;

            // 配列を得てソートする
            var ar = q.ToArray();
            if (ar.Length == 0) return null;
            Array.Sort(ar, (x, y) =>
            {
                if (x.versionInfo != null && y.versionInfo == null) return 1;
                if (x.versionInfo == null && y.versionInfo != null) return -1;
                if (x.versionInfo == null && y.versionInfo == null) return 0;
                System.Diagnostics.Debug.Assert(x.versionInfo != null);
                System.Diagnostics.Debug.Assert(y.versionInfo != null);
                if (x.versionInfo.Major != y.versionInfo.Major) return x.versionInfo.Major - y.versionInfo.Major;
                if (x.versionInfo.Minor != y.versionInfo.Minor) return x.versionInfo.Minor - y.versionInfo.Minor;
                if (x.versionInfo.Build != y.versionInfo.Build) return x.versionInfo.Build - y.versionInfo.Build;
                return x.versionInfo.Revision - y.versionInfo.Revision;
            });
            // ロードすべきファイルを得る (最後の1つがそれ)
            var moduleToLoad = ar.Last().ModuleDllFilePath;
            //System.Diagnostics.Debug.WriteLine("["+ ar.Last().ModuleDllFilePath + "]");

            if (moduleToLoad == null
                || !(moduleToLoad.ToLower().EndsWith(".dll") || moduleToLoad.ToLower().EndsWith(".tmp")))
            {
                // It's dynamic assembly
                notRequired = true;
                return null;
            }
#if true
            System.Diagnostics.Debug.Assert(!(moduleToLoad.StartsWith("http:") || moduleToLoad.StartsWith("https:")));
#else
            if (moduleToLoad.StartsWith("http:") || moduleToLoad.StartsWith("https:"))
            {
                var client = new WebClient();
                var stream = client.OpenRead(moduleToLoad);
                var bb = new BinaryArrayBuilder(stream);
                using (var reader = new BinaryReader(stream))
                {
                    for (; ; )
                    {
                        var ar2 = reader.ReadBytes(1024);
                        if (ar2 == null || ar2.Length == 0) break;
                        bb.AppendArray(ar2);
                    }
                }
                //var tempPath = Path.GetTempPath();
                //var tempFileName = Path.Combine(tempPath, moduleToLoad.Substring(moduleToLoad.LastIndexOf('/') + 1));
                var tempFileName = Path.GetTempFileName();
                File.WriteAllBytes(tempFileName, bb.ToArray());
                refModInfo.FullPath = tempFileName;
                return Assembly.LoadFrom(tempFileName);
            }
            else
            {
#endif
            // もしリソースにあればリソースから読み込む
            //System.Diagnostics.Debug.WriteLine("[A]");
            try
            {
                byte[] bytes = null;
                //using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(moduleToLoad))
                //System.Diagnostics.Debug.WriteLine("[AA]");
                var a = Assembly.GetEntryAssembly();
                //System.Diagnostics.Debug.WriteLine("[AB]"+ moduleToLoad);
                foreach (var resourceName in a.GetManifestResourceNames())
                {
                    if( resourceName.ToLower().EndsWith(".dll"))
                    {
                        //System.Diagnostics.Debug.WriteLine("[Found:]" + resourceName);
                        var s = a.GetManifestResourceStream(resourceName);
                        //System.Diagnostics.Debug.WriteLine("[AC]");
                        using (var stream = s)
                        {
                            //System.Diagnostics.Debug.WriteLine("[B]");
                            bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, ar.Length);
                        }
                        //System.Diagnostics.Debug.WriteLine("[C]");
                        return Assembly.Load(bytes);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // do nothing, continue next
            }
            //System.Diagnostics.Debug.WriteLine("[D]");
            string filename = assemblyLoadFileName(moduleToLoad);
            //System.Diagnostics.Debug.WriteLine("[" + (filename == null).ToString() + "]");
            if (filename == null) return null;
            return Assembly.LoadFrom(filename);
#if false
        }
#endif
        }

        /// <summary>
        /// オブへジェクトをロードする
        /// </summary>
        /// <param name="thismodex"></param>
        /// <param name="startPlaceId"></param>
        public static async System.Threading.Tasks.Task<string> LoadObjectAsync(ModuleEx thismodex)
        {
            string startPlaceId = null;
            foreach (var item in thismodex.GetModules())
            {
                await loadObjectSubAsync(item);
                var r = item.GetStartPlace();
                if (r != null) startPlaceId = r;
            }
            SimpleName<World>.AddAndCheckAll(thismodex.GetModules().First().Id, thismodex.GetWorlds());
            return startPlaceId;
        }
        private static async System.Threading.Tasks.Task loadObjectSubAsync(ANGFLib.Module thismod)
        {
            SimpleName<Place>.AddAndCheckAll(thismod.Id, thismod.GetPlaces());
            SimpleName<MiniStatus>.AddAndCheckAll(thismod.Id, thismod.GetStatuses());
            SimpleName<Person>.AddAndCheckAll(thismod.Id, thismod.GetPersons());
            SimpleName<Item>.AddAndCheckAll(thismod.Id, thismod.GetItems());
            SimpleName<Schedule>.AddAndCheckAll(thismod.Id, thismod.GetSchedules());
            await Collections.AddAndMergeAndCheckAllAsync(thismod.Id, thismod.GetCollections());
            SimpleName<EquipType>.AddAndCheckAll(thismod.Id, thismod.GetEquipTypes());
            //if (SimpleName<EquipType>.List.Count > 7) throw new ApplicationException("装備は7つまでです。");
            SimpleName<Shop>.AddAndCheckAll(thismod.Id, thismod.GetShops());
            SimpleName<ShopAndItemReleation>.AddAndCheckAll(thismod.Id, thismod.GetShopAndItemReleations());
            SimpleName<MenuItem>.AddAndCheckAll(thismod.Id, thismod.GetExtendMenu());

            if (thismod.FutureEquipSimulation != null) FutureEquipSimulations.AddItem(thismod.FutureEquipSimulation);
            if (thismod.FileExtention != null) General.FileExtention = thismod.FileExtention;
            if (thismod.HowToMove != null) Moves.HowToMove = thismod.HowToMove;
            if (thismod.HowToSubMove != null) Moves.HowToSubMove = thismod.HowToSubMove;
            if (thismod.MapLengthCoocker != null) Coockers.MapLengthCoocker = thismod.MapLengthCoocker;
            if (thismod.PriceCoocker != null) Coockers.PriceCoocker = thismod.PriceCoocker;
        }

#if !MYBLAZORAPP
        private static MyXmlDoc loadMyXmlDoc(string filename)
        {
            var result = new MyXmlDoc();
            XDocument doc = null;
            // 指定されたファイルが存在する場合は、それを読もうとする
            // 拡張子が".dll"ならリソース。それ以外ならXMLの生ファイル
            if (Path.GetExtension(filename).ToLower() != ".dll")
            {
                doc = XDocument.Load(new StreamReader(filename), LoadOptions.SetLineInfo);
            }
            else
            {
                var assem = Assembly.ReflectionOnlyLoadFrom(filename);
                foreach (var n in assem.GetManifestResourceNames())
                {
                    if (n.ToLower().EndsWith(".angfruntime.xml"))
                    {
                        doc = XDocument.Load(assem.GetManifestResourceStream(n));
                        break;
                    }
                }
            }
            if (doc == null)
            {
                UI.Actions.tellAssertionFailed("モジュール" + filename + "にはリソースAngfgRunTime.xmlが含まれていません。");
                //DefaultPersons.システム.Say();
                return null;
            }
            if (!result.LoadFromXDocument(doc, filename)) return null;
            result.xmlFilePath = filename;
            return result;
        }
#endif

#if MYBLAZORAPP
        [Obsolete]
        private static List<MyXmlDoc> seekModulesSub(object form, List<MyXmlDoc> list, object parentRegistry, string keyName, bool includeErrorModule) => null;
#else
        private static List<MyXmlDoc> seekModulesSub(object form,
            List<MyXmlDoc> list,
            RegistryKey parentRegistry,
            string keyName, bool includeErrorModule)
        {
            var o = parentRegistry.OpenSubKey(keyName);
            if (o == null) return list;
            try
            {
                foreach (string n in o.GetValueNames())
                {
                    // TBW 例外ハンドルの復帰
                    //try
                    //{
                    var mod = loadMyXmlDoc((string)o.GetValue(n));
                    if (mod != null) list.Add(mod);
                    else if (includeErrorModule)
                    {
                        list.Add(MyXmlDoc.CreateErrorObject((string)o.GetValue(n)));
                    }
                    //}
                    //catch (Exception e)
                    //{
                    //    System.Windows.Forms.MessageBox.Show(form, e.ToString(), form.Text);
                    //}
                }
                foreach (var n in o.GetSubKeyNames())
                {
                    seekModulesSub(form, list, o, n, includeErrorModule);
                }
            }
            finally
            {
                o.Close();
            }
            return list;
        }
#endif

        /// <summary>
        /// レジストリからモジュールを探索する
        /// </summary>
        /// <param name="form"></param>
        /// <param name="includeErrorModule"></param>
        /// <returns></returns>
#if MYBLAZORAPP
        [Obsolete]
        public static List<MyXmlDoc> SeekModulesFromRegistryOnly(object form, bool includeErrorModule = false) => null;
#else
        public static List<MyXmlDoc> SeekModulesFromRegistryOnly(object form, bool includeErrorModule = false)
        {
            return seekModulesSub(form, new List<MyXmlDoc>(), Registry.CurrentUser, ANGFLib.Constants.RegistoryRoot, includeErrorModule);
        }
#endif
    }
}
