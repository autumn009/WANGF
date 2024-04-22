using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace waFirstRPG
{
	[AutoFlags("{65B3515D-AA32-448a-B565-82DDFB6C9B8E}")]
    public static class HarFlags
    {
        [FlagName("名前")]
        public static string 名前 = "";
        // 装備マップの現状を保持
        [FlagPrefix("MariaEquip")]
        public static FlagCollection<string> MariaEquip = new FlagCollection<string>();
        [FlagPrefix("ReiEquip")]
        public static FlagCollection<string> ReiEquip = new FlagCollection<string>();
        [FlagPrefix("RiriEquip")]
        public static FlagCollection<string> RiriEquip = new FlagCollection<string>();
    }
}
