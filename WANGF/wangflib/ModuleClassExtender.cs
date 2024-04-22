using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 内部利用専用です。
    /// </summary>
    public class ReferModuleInfoEx : ReferModuleInfo
    {
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public MyXmlDoc AngfRuntimeXml;
    }
    /// <summary>
    /// 内部利用専用です。
    /// </summary>
    public static class ModuleClassExtender
    {
        private static Dictionary<Module, ReferModuleInfo> dic = new Dictionary<Module, ReferModuleInfo>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static ReferModuleInfo GetXmlModuleData(this Module mod)
        {
            return dic[mod];
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static void SetXmlModuleData(this Module mod, ReferModuleInfo newData)
        {
            dic[mod] = newData;
        }
        private static Dictionary<ModuleEx, ReferModuleInfo> dicex = new Dictionary<ModuleEx, ReferModuleInfo>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static int GetXmlModuleDataCount()
        {
            return dicex.Count();
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static ReferModuleInfo GetXmlModuleData(this ModuleEx modex)
        {
            return dicex[modex];
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static void SetXmlModuleData(this ModuleEx modex, ReferModuleInfo newData)
        {
            dicex[modex] = newData;
        }
        private static Dictionary<ModuleEx, ANGFLib.Module[]> modules = new Dictionary<ModuleEx, Module[]>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static ANGFLib.Module[] GetModules(this ModuleEx modex)
        {
            // QueryObjectsを直接呼んではならない。必ずここを経由する
            if (!modules.ContainsKey(modex)) modules.Add(modex, modex.QueryObjects<ANGFLib.Module>());
            return modules[modex];
        }
        private static Dictionary<ModuleEx, World[]> worlds = new Dictionary<ModuleEx, World[]>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static World[] GetWorlds(this ModuleEx modex)
        {
            // QueryObjectsを直接呼んではならない。必ずここを経由する
            if (!worlds.ContainsKey(modex)) worlds.Add(modex, modex.QueryObjects<World>());
            return worlds[modex];
        }
    }
    /// <summary>
    /// 内部利用専用です。
    /// </summary>
    public static class ModuleClassExtenderEx
    {
        private static Dictionary<Module, MyXmlDoc> dic = new Dictionary<Module, MyXmlDoc>();
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static MyXmlDoc GetAngfRuntimeXml(this Module mod)
        {
            MyXmlDoc doc;
            dic.TryGetValue(mod, out doc);
            return doc;
        }
        /// <summary>
        /// 内部利用専用です。
        /// </summary>
        public static void SetAngfRuntimeXml(this Module mod, MyXmlDoc newData)
        {
            dic[mod] = newData;
        }
    }
}
