using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    public interface IPartyMember
    {
        Person GetPerson();
        IEnumerable<string> GetEquippedItemIds();
        string GetRawEquip(int index);
        void SetRawEquip(int index, string id);
        bool GetDirectEquipEnabled() => true;
    }

    public static class Party
    {
        // trueにすると常にパーティー先頭がデフォルト主人公という機能を停止する
        public static bool DisableDefault主人公 { get; set; } = false;
        private static void update(IEnumerable<string> newEnun)
        {
            var q = newEnun;
            if (!DisableDefault主人公) q = q.Where(c => c != DefaultPersons.主人公.Id);
            var newArray = q.ToArray();
            Flags.PartyIDs.Clear();
            for (int i = 0; i < newArray.Count(); i++)
            {
                Flags.PartyIDs[i.ToString()] = newArray.ElementAt(i);
            }
        }
        public static void ClearWithoutWatashi()
        {
            Flags.PartyIDs.Clear();
        }
        public static string AddMember(string id)
        {
            if (Contains(id)) return $"既にメンバーです。";
            update(EnumPartyMembers().Append(id));
            return null;
        }
        public static string RemoveMember(string id)
        {
            if (!DisableDefault主人公)
            {
                if (id == DefaultPersons.主人公.Id)
                {
                    return "主人公は削除できません。";
                }
            }
            if (Contains(id))
            {
                update(EnumPartyMembers().Where(c => c != id));
                return null;
            }
            return "見つかりません。";
        }
        public static bool Contains(string id)
        {
            return EnumPartyMembers().Contains(id);
        }
        public static IEnumerable<string> EnumPartyMembers()
        {
            if (!DisableDefault主人公) yield return DefaultPersons.主人公.Id;
            foreach (var item in Flags.PartyIDs.Keys.OrderBy(c => int.Parse(c)))
            {
                yield return Flags.PartyIDs[item];
            }
        }
    }
}
