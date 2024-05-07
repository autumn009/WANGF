using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ANGFLib;
using Microsoft.Win32;

namespace waRazorUI
{
    public class SystemGameStartupInfos : GameStartupInfos
    {
        public override IEnumerable<GameStartupInfo> EnumEmbeddedModules()
        {
            return new StaticGameStartupInfo[]
            {
                new StaticGameStartupInfo("{660c7829-76d3-4862-8c00-1b83d96978c5}","システム管理", new Type[]{ typeof(SystemMyModuleEx) },  false,"データファイルの管理を行います。保存したデータはこの機能で必ずバックアップを取って下さい。他のマシンで続きを遊びたいときもこれを使います。", MetaModuleAutoTest.No)
            };
        }
    }

    public static class Modules
    {
        public static async Task<IEnumerable<GameStartupInfo>> EnumEmbeddedModulesAsync()
        {
            var seq = GetOnlyEmbeddedModules() ?? Enumerable.Empty<GameStartupInfo>();
            if (!General.IsBlazorWebAssembly())
            {
                seq = seq.Concat(await FileModules.EnumFileModulesAsync());
            }
            if (seq.Count() == 0)
            {
                if (General.IsBlazorWebAssembly())
                {
                    DefaultPersons.システムWarn.Say("モジュールが見つかりません。これが意図しない動作の場合、OnlyEmbeddedModules.dllが適切なモジュールを含んでいない確認してください。OnlyGameStartupInfos.EnumEmbeddedModulesメソッドが適切なモジュール一覧を返す必要があります。");
                }
                else
                {
                    DefaultPersons.システムWarn.Say("モジュールが見つかりません。これが意図しない動作の場合、各モジュール(DLL)へのショートカットがC:\\ProgramData\\autumn\\WANGF\\modulesに存在するか確認してください。ハードリンク、シンボリックリンクではなくショートカットが必要です。");
                }
            }
            return seq.Concat(new SystemGameStartupInfos().EnumEmbeddedModules());

            IEnumerable<GameStartupInfo> GetOnlyEmbeddedModules()
            {
                IEnumerable<GameStartupInfo> seq = null;
                foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assem.GetTypes())
                    {
                        if (!type.IsAbstract && type.IsSubclassOf(typeof(GameStartupInfos)))
                        {
                            var n = (GameStartupInfos)Activator.CreateInstance(type);
                            if (seq == null) seq = Enumerable.Empty<GameStartupInfo>();
                            seq = seq.Concat(n.EnumEmbeddedModules());
                        }
                    }
                }
                return seq;
            }
        }
    }
    public static class FileModules
    {
        // 全てのモジュールのファイルを得る。ショートカットは自動解釈される
        private static IEnumerable<string> enumFiles()
        {
            var path = Path.Combine(General.GetCommonRootDirectory(), "modules");
            if (Directory.Exists(path))
            {
                foreach (var item0 in Directory.EnumerateFiles(path, "*.*"))
                {
                    var item = item0;
                    for (; ; )
                    {
                        if (item.ToLower().EndsWith(".lnk"))
                        {
                            var next = FileLayout.LnkFileToTargetPath(item);
                            if (next == null) break;
                            item = next;
                        }
                        else break;
                    }
                    if (item.EndsWith(".xml") || item.EndsWith(".dll")) yield return item;
                }
            }
        }
        private static async Task<MyXmlDoc> loadMyXmlDocAsync(string filename)
        {
            return await Util.LoadMyXmlDocAsync(filename);
        }
        private static async Task<MyXmlDoc[]> GetMyDocsAsync()
        {
            var list = new List<MyXmlDoc>();
            foreach (var item in enumFiles())
            {
                var doc = await loadMyXmlDocAsync(item);
                if (doc != null) list.Add(doc);
            }
            return list.ToArray();
        }
        public static async Task<IEnumerable<GameStartupInfo>> EnumFileModulesAsync()
        {
            var m = await GetMyDocsAsync();
            Scenarios.SetModules(m);
            return m.Where(c=>c.startupModule).Select(c => new FileGameStartupInfo(c.id, c.name, c, c.is18k, c.description, c.AutoTestEnabled ? MetaModuleAutoTest.Yes : MetaModuleAutoTest.No, c.TitlePicture));
        }
    }
}
