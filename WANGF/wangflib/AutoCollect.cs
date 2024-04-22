using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ANGFLib
{
    internal class AutoCollectItem
    {
        internal FieldInfo fi;
        internal string id;
        internal string name;
        public AutoCollectItem(FieldInfo fi, string id, string name)
        {
            this.fi = fi;
            this.id = id;
            this.name = name;
        }
    }

    /// <summary>
    /// アプリケーションドメインに読み込まれている全てのモジュールを操作します。
    /// 任意のモジュールからは使用すべきではありません。
    /// </summary>
    public static class AutoCollect
    {
        private static List<AutoCollectItem> targetList;
        private static Dictionary<Assembly, List<AutoCollectItem>> targetListByAssembly;
        private static void createTargetList()
        {
            targetList = new List<AutoCollectItem>();
            targetListByAssembly = new Dictionary<Assembly, List<AutoCollectItem>>();
            foreach (var ex in State.LoadedModulesEx)
            {
                var mod = ex.GetType().Assembly;
                if (ex is CompatibleWrappingModuleEx)
                {
                    foreach (var item in ex.QueryObjects<ANGFLib.Module>()) mod = item.GetType().Assembly;
                }
                walkOneModule(mod);
            }
            // walk myself, "wangflib.dll"
            walkOneModule(Assembly.GetExecutingAssembly());
            //foreach (var n in AppDomain.CurrentDomain.GetAssemblies()) walkOneModule(n);
        }
        private static void walkOneModule(Assembly n)
        {
            if (n.FullName.Contains("Microsoft.")) return;
            var list = new List<AutoCollectItem>();
            foreach (var t in n.GetTypes())
            {
                var o1 = t.GetCustomAttributes(typeof(AutoFlagsAttribute), true).OfType<AutoFlagsAttribute>().FirstOrDefault();
                if (o1 == null) continue;
                foreach (var m in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    object[] o2 = m.GetCustomAttributes(true);
                    var a1 = o2.OfType<FlagNameAttribute>().FirstOrDefault();
                    var a2 = o2.OfType<FlagPrefixAttribute>().FirstOrDefault();
                    var a3 = o2.OfType<DynamicObjectFlagAttribute>().FirstOrDefault();
                    AutoCollectItem toadd;
                    if (a1 != null) toadd = new AutoCollectItem(m, o1.Id, a1.Name);
                    else if (a2 != null) toadd = new AutoCollectItem(m, o1.Id, a2.Name);
                    else if (a3 != null) toadd = new AutoCollectItem(m, o1.Id, a3.Name);
                    else continue;
                    list.Add(toadd);
                    targetList.Add(toadd);
                }
            }
            targetListByAssembly.Add(n, list);
        }

        /// <summary>
        /// アプリケーションドメインに読み込まれている全てのモジュールのAutoFlagsAttributeを持つ
        /// 全てのクラスに対して引数のアクションを呼び出します。
        /// </summary>
        /// <param name="action">実行すべきアクションを指定します</param>
        public static void WalkAll(Func<System.Reflection.FieldInfo, string, string, Dummy> action)
        {
            if (targetList == null) createTargetList();
            foreach (var t in targetList) action(t.fi, t.id, t.name);
        }

        /// <summary>
        /// 指定されたモジュールのAutoFlagsAttributeを持つ
        /// 全てのクラスに対して引数のアクションを呼び出します。
        /// </summary>
        /// <param name="action">実行すべきアクションを指定します</param>
        /// <param name="n">対象モジュール</param>
        public static void WalkOneModule(Func<System.Reflection.FieldInfo, string, string, Dummy> action, Assembly n)
        {
            if (targetList == null) createTargetList();
            foreach (Assembly item in targetListByAssembly.Keys)
            {
                if (item.FullName == n.FullName)
                {
                    foreach (var t in targetListByAssembly[item])
                    {
                        action(t.fi, t.id, t.name);
                    }
                    break;
                }
            }
        }
    }
}
