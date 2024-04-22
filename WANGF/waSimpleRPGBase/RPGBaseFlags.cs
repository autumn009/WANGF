using ANGFLib;

namespace waSimpleRPGBase
{
    [AutoFlags("{ae51ce74-f273-4c13-a24f-0e3e49302209}")]
    public static class RPGBaseFlags
    {
        [FlagPrefix("HP")]
        public static readonly FlagCollection<int> HP群 = new FlagCollection<int>();
        [FlagPrefix("MP")]
        public static readonly FlagCollection<int> MP群 = new FlagCollection<int>();
        [FlagPrefix("Status")]
        public static readonly FlagCollection<int> Status群 = new FlagCollection<int>();
        [FlagPrefix("ExpLongStr")]
        public static readonly FlagCollection<string> EXP群 = new FlagCollection<string>();
    }
}
