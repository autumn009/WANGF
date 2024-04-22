using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ANGFLib;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Emit;

namespace waSimpleRPGBase
{
    public enum EStatus
    {
        Dead = 1,     // 死亡
        Syncode = 2,  // 失神
        Poison = 4,   // 毒
        Sleep = 8,    // 睡眠
        Stone = 16,   // 石化
        // 残りの番号は拡張可能である
    }

    public class FightingParameterCalculator
    {
        protected virtual long[] GetGrowingTable() => RPGBaseLevelTable.PersonGrowingTable;
        protected virtual int[] GetHPTable() => RPGBaseLevelTable.HPTable;
        protected virtual int[] GetMPTable() => RPGBaseLevelTable.MPTable;
        protected virtual int[] GetAttackTable() => RPGBaseLevelTable.AttackTable;
        protected virtual int[] GetDefenceTable() => RPGBaseLevelTable.DefenceTable;

        public virtual int GetLevel(Person p)
        {
            var table = GetGrowingTable();
            var exp = p.GetEXP();
            var level = RPGBaseLevelTable.MaxLevel;
            for (int i = 1; i < table.Length; i++)
            {
                if (table[i] <= exp) level = i;
            }
            return level;
        }
        public virtual int GetMaxHP(int level)
        {
            return GetHPTable()[level];
        }
        public virtual int GetMaxHP(Person p)
        {
            var level = GetLevel(p);
            return GetMaxHP(level);
        }
        public virtual int GetMaxMP(int level)
        {
            return GetMPTable()[level];
        }
        public virtual int GetMaxMP(Person p)
        {
            var level = GetLevel(p);
            return GetMaxMP(level);
        }

        public virtual int GetAttackValue(Person p)
        {
            var level = GetLevel(p);
            return GetAttackValue(level);
        }
        public virtual int GetAttackValue(int level)
        {
            return GetAttackTable()[level];
        }

        public virtual int GetDefenceValue(Person p)
        {
            var level = GetLevel(p);
            return GetDefenceValue(level);
        }
        public virtual int GetDefenceValue(int level)
        {
            return GetDefenceTable()[level];
        }
    }

    public static class RPGBasePerson
    {
        private static readonly FlagCollection<int> hpc = new FlagCollection<int>();
        private static readonly FlagCollection<int> mpc = new FlagCollection<int>();
        private static readonly FlagCollection<long> expc = new FlagCollection<long>();
        private static readonly FlagCollection<int> statusc = new FlagCollection<int>();

        public static int GetHP(this Person p) => p.IsRequirePersistence ? RPGBaseFlags.HP群[p.Id] : hpc[p.Id];
        public static void SetHP(this Person p, int hp)
        {
            if (p.IsRequirePersistence)
                RPGBaseFlags.HP群[p.Id] = hp;
            else
                hpc[p.Id] = hp;
        }
        public static int GetMP(this Person p) => p.IsRequirePersistence ? RPGBaseFlags.MP群[p.Id] : mpc[p.Id];
        public static void SetMP(this Person p, int mp)
        {
            if (p.IsRequirePersistence)
                RPGBaseFlags.MP群[p.Id] = mp;
            else
                mpc[p.Id] = mp;
        }
        public static long GetEXP(this Person p)
        {
            if (p.IsRequirePersistence)
            {
                var s = RPGBaseFlags.EXP群[p.Id];
                if (string.IsNullOrWhiteSpace(s)) return 0L;
                return long.Parse(s);
            }
            else
                return expc[p.Id];
        }
        public static void SetEXP(this Person p, long exp)
        {
            if (p.IsRequirePersistence)
                RPGBaseFlags.EXP群[p.Id] = exp.ToString();
            else
                expc[p.Id] = exp;
        }
        public static EStatus GetStatus(this Person p) => (EStatus)(p.IsRequirePersistence ? RPGBaseFlags.Status群[p.Id] : statusc[p.Id]);
        public static void SetStatus(this Person p, EStatus status)
        {
            if (p.IsRequirePersistence)
                RPGBaseFlags.Status群[p.Id] = (int)status;
            else
                statusc[p.Id] = (int)status;
        }
        public static void ClearAllTempInfo()
        {
            hpc.Clear();
            mpc.Clear();
            expc.Clear();
            statusc.Clear();
        }
        private static FightingParameterCalculator? calc = null;
        private static FightingParameterCalculator getCalc()
        {
            if (calc == null)
            {
                foreach (var moduleEx in State.LoadedModulesEx)
                {
                    var ar = moduleEx.QueryObjects<FightingParameterCalculator>();
                    if (ar.Length > 0)
                    {
                        calc = ar[0];
                        return calc;
                    }
                }
                calc = new FightingParameterCalculator();
            }
            return calc;
        }
        public static int GetMaxHP(this Person p)
        {
            return getCalc().GetMaxHP(p);
        }
        public static int GetMaxMP(this Person p)
        {
            return getCalc().GetMaxMP(p);
        }
        public static int GetLevel(this Person p)
        {
            return getCalc().GetLevel(p);
        }
        public static int GetAttackValue(this Person p)
        {
            return getCalc().GetAttackValue(p);
        }
        public static int GetDefenceValue(this Person p)
        {
            return getCalc().GetDefenceValue(p);
        }
    }
}
