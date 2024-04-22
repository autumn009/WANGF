using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace waRazorUI
{
    internal record AdInfo(string Description, string Uri, string AuthorName, string AuthorUri);

    internal static class AdBlock
    {
        private static AdInfo[] db;

        private static AdInfo[] init(bool is18k)
        {
            var list = new List<AdInfo>();
            using var reader = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("waRazorUI.adblock.tsv"));
            for (; ; )
            {
                var line = reader.ReadLine();
                if (line == null) break;
                var s = line.Split('\t');
                var my18k = s[4] != "0";
                if (my18k == is18k) list.Add(new AdInfo(s[0], s[1], s[2], s[3]));
            }
            // random shuffle
            for (int i = 0; i < list.Count; i++)
            {
                var tgt = Random.Shared.Next(list.Count);
                var t = list[i];
                list[i] = list[tgt];
                list[tgt] = t;
            }
            return list.ToArray();
        }
        private static int adCounter = 0;
        private static DateTime adLastDateTime = DateTime.MinValue;
        public static bool DisableIncrement { get; set; } = false;
        internal static AdInfo GetNextAd(bool is18k)
        {
            if (db == null) db = init(is18k);
            var r = db[adCounter];
            if (!DisableIncrement)
            {
                if(adLastDateTime.AddSeconds(30) < DateTime.Now)
                {
                    adCounter = (adCounter + 1) % db.Length;
                    adLastDateTime = DateTime.Now;
                }
            }
            return r;
        }
    }
}
