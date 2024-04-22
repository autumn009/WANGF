using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waSimpleRPGBase
{
    public static class RPGBaseLevelTable
    {
        public const int MaxLevel = 99;
        public static long[] PersonGrowingTable = new long[MaxLevel + 1];
        public static int[] HPTable = new int[MaxLevel + 1];
        public static int[] MPTable = new int[MaxLevel + 1];
        public static int[] AttackTable = new int[MaxLevel + 1];
        public static int[] DefenceTable = new int[MaxLevel + 1];
        static RPGBaseLevelTable()
        {
            int hp = 10;
            int hpdiff = 10;
            int mp = 1;
            int mpdiff = 10;
            for (int i = 1; i <= MaxLevel; i++)
            {
                PersonGrowingTable[i] = (long)Math.Pow(1.5, i);
                HPTable[i] = hp;
                MPTable[i] = mp;
                hp += hpdiff;
                if (i < 50) hpdiff = (int)(hpdiff * 1.1);
                mp += Math.Max(3,mpdiff/10);
                if (i < 50) mpdiff = (int)(mpdiff * 1.1);
                AttackTable[i] = i * 11;
                DefenceTable[i] = i * 13;
            }
            // レベル1の必要経験値は0でなければならないので強制する
            // これが0より大きいと経験値0のキャラはレベル1にすらなれないことになる。
            PersonGrowingTable[1] = 0;
        }
    }
}
