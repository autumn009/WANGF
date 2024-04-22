using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace ANGFLib
{
    /// <summary>
    /// 内部使用専用です。
    /// </summary>
    public class CompatibleWrappingModuleEx : ANGFLib.ModuleEx
    {
        private ANGFLib.Module module;
        /// <summary>
        /// 内部使用専用です。
        /// </summary>
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { module as T };
            return new T[0];
        }
        /// <summary>
        /// 内部使用専用です。
        /// </summary>
        public CompatibleWrappingModuleEx(ANGFLib.Module module)
        {
            this.module = module;
        }
    }
}
