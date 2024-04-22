using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace FirstOtsukai
{
    public class HaoScheduleStopper : Schedule
    {
        public override bool IsActive
        {
            get { return true; }
        }

        public override bool IsHidden
        {
            get { return false; }
        }

        public override DateTime StartTime
        {
            get { return new DateTime(HaoConstants.StartDateTime.Year, HaoConstants.StartDateTime.Month, HaoConstants.StartDateTime.Day, 18, 0, 0); }
        }

        public override TimeSpan Length
        {
            // ガードタイムを取って長めに
            get { return new TimeSpan(24,0,0); }
        }

        public override string Description
        {
            get { return "失敗期限"; }
        }

        public override async Task HitProcedureAsync()
        {
            DefaultPersons.独白.Say("時間切れになってしまった。");
            DefaultPersons.独白.Say("お使いは失敗だ。次こそはがんばろう。");
            State.SetCollection(Constants.EndingCollectionID, FirstOtsykaiConstants.時間切れエンディングID, null);
            State.今日の就寝時刻 = DateTime.MinValue;  // 自動帰還判定禁止
            await State.WarpToAsync(FirstOtsykaiConstants.EpilogueID);
            await Task.Delay(0);
        }

        public override async Task NoHitProcedureAsync()
        {
            await Task.Delay(0);
            throw new ApplicationException("ここに来ることはないはずである");
        }

        public override string HumanReadableName
        {
            get { return "時間切れ"; }
        }

        public override string Id
        {
            get { return FirstOtsykaiConstants.時間切れスケジュールID; }
        }
    }

    public class HaoSchedule100Yes : Schedule
    {
        public override bool IsActive
        {
            get { return Flags.CurrentPlaceId == FirstOtsykaiConstants.こうばんID; }
        }

        public override bool IsHidden
        {
            get { return false; }
        }

        public override DateTime StartTime
        {
            get { return HaoConstants.StartDateTime; }
        }

        public override TimeSpan Length
        {
            get { return new TimeSpan(1, 0, 0); }
        }

        public override string Description
        {
            get { return "百円拾う"; }
        }

        public override async Task HitProcedureAsync()
        {
            DefaultPersons.独白.Say("百円が落ちていたので拾った。");
            DefaultPersons.独白.Say("そのまま交番に届けてほめられた。");
            State.GoTime(10);
            await Task.Delay(0);
        }

        public override async Task NoHitProcedureAsync()
        {
            DefaultPersons.独白.Say("百円は他の誰かに拾われてしまったらしい。");
            await Task.Delay(0);
        }

        public override string HumanReadableName
        {
            get { return "百円拾う"; }
        }

        public override string Id
        {
            get { return FirstOtsykaiConstants.百円拾うスケジュールID; }
        }
    }
}
