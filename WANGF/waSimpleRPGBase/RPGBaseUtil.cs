using ANGFLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ANGFLib.SuperTalkCollections;

namespace waSimpleRPGBase
{
    public static class RPGBaseUtil
    {
        public static void HPMP全回復(this Person p)
        {
            p.SetHP(p.GetMaxHP());
            p.SetMP(p.GetMaxMP());
        }
        public static void HP全回復(this Person p)
        {
            p.SetHP(p.GetMaxHP());
        }
        public static void MP全回復(this Person p)
        {
            p.SetMP(p.GetMaxMP());
        }
        public static void 全員HPMP全回復()
        {
            foreach (var personId in Party.EnumPartyMembers())
            {
                var p = Person.List[personId];
                p.HPMP全回復();
            }
        }
        public static void 全員HP全回復()
        {
            foreach (var personId in Party.EnumPartyMembers())
            {
                var p = Person.List[personId];
                p.HP全回復();
            }
        }
    }
}
