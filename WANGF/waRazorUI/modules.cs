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
            var s1 = GetOnlyEmbeddedModules() ?? Enumerable.Empty<GameStartupInfo>();
            var s2 = Enumerable.Empty<GameStartupInfo>();
            if (!General.IsBlazorWebAssembly()) s2 = await FileModules.EnumFileModulesAsync();
            var s3 = new SystemGameStartupInfos();
            return s1.Concat(s2).Concat(s3.EnumEmbeddedModules());

            IEnumerable<GameStartupInfo> GetOnlyEmbeddedModules()
            {
                foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assem.GetTypes())
                    {
                        if (type != null && type.Name != null)
                        {
                            if (type.Name.Contains("OnlyGameStartupInfos"))
                            {
                                var n = (GameStartupInfos)Activator.CreateInstance(type);
                                return n.EnumEmbeddedModules();
                            }
                        }
                    }
                }
                return null;
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
            return m.Where(c=>c.startupModule).Select(c => new FileGameStartupInfo(c.id, c.name, c, c.is18k, c.description, c.AutoTestEnabled ? MetaModuleAutoTest.Yes : MetaModuleAutoTest.No));
        }
    }
}
