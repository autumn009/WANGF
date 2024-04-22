using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace waSimpleRPGBase
{
    public class RPGbaseModeProvider
    {
        public virtual bool IsRPGMode() => true;
    }

    public static class RPGBaseMode
    {
        public static bool IsRPGMode()
        {
            foreach (var moduleEx in State.LoadedModulesEx)
            {
                var ar = moduleEx.QueryObjects<RPGbaseModeProvider>();
                if (ar.Length > 0)
                {
                    return ar[0].IsRPGMode();
                }
            }
            return false;
        }
    }
}
