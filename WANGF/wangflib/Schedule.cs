using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// スケジュールを返します
    /// </summary>
    public abstract class Schedule : SimpleName<Schedule>
    {
        /// <summary>
        /// 有効なイベントはTrueを返します
        /// </summary>
        public abstract bool IsActive{get;}
        /// <summary>
        /// UIで表示しない隠しイベントはTrueを返します
        /// </summary>
        public virtual bool IsHidden { get { return false; } }
        /// <summary>
        /// イベントの開始時間です
        /// </summary>
        public abstract DateTime StartTime{get;}
        /// <summary>
        /// イベントの長さです
        /// </summary>
        public abstract TimeSpan Length {get;}
        /// <summary>
        /// イベントの説明文です
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// 条件を満たした場合に呼ばれます。必ず、State.GoTimeで指定後の時間に進めるか、イベントを無効にする必要があります
        /// </summary>
        public abstract Task HitProcedureAsync();/* DIABLE ASYNC WARN */
        /// <summary>
        /// 条件を満たしていない場合に呼ばれます。終了時刻を過ぎていたときに呼び出されます
        /// </summary>
        public abstract Task NoHitProcedureAsync();/* DIABLE ASYNC WARN */
        // IsAutoResetは、自動的にProcedureが走ることはなく、
        // 明示的に事後にResetを呼ぶ必要のあるイベントのときfalse
        // falseでも、Suppokashiは走る
        public virtual bool IsAutoReset => true;
        /// <summary>
        /// HitProcedurematahaまたはNoHitProcedure実行後に呼び出されます
        /// サイリクリックなスケジュールでは次のスケジュールを入れることができます
        /// オーバーライドして何も処理しなければイベントは終了してそのままです。
        /// </summary>
        public virtual void OnProcedureDone()
        {
            // NOP
        }
    }

    /// <summary>
    /// スケジュールをチェックします。一般アプリは使用すべきではありません。
    /// </summary>
    public class ScheduleCheck
    {
        // 2つの時間範囲に交差する範囲があるかを判定します。
        // 時間を飛ばす際に、飛ばす範囲とスケジュール範囲が交差するか否かを調べられます。
        private static bool TimeRegionMatch(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 <= end2 && start2 <= end1;
        }

        // スケジュールを二重にチェックしないためのフラグ
        private static bool scheduleDoLocker = false;

        /// <summary>
        /// 一般アプリから使用すべきではありません。
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static async Task<bool> EventCheckAsync(DateTime startTime, DateTime endTime)
        {
            if (scheduleDoLocker == false)
            {
                foreach (Schedule item in SimpleName<Schedule>.List.Values)
                {
                    if (State.IsScheduleVisible(item.Id))
                    {
                        if (TimeRegionMatch(startTime, endTime, item.StartTime, item.StartTime + item.Length))
                        {
                            if (item.IsActive)
                            {
                                if (item.IsAutoReset)
                                {
                                    scheduleDoLocker = true;
                                    // 実行すべきイベントとの完全なる一致を確認した
                                    State.SetScheduleVisible(item.Id, false);  // 消費されるスケジュールはもう見えなくなる。2重実行防止
                                    DateTime schEndTime = item.StartTime + item.Length;
                                    State.GoTime((int)(schEndTime - Flags.Now).TotalMinutes);
                                    await item.HitProcedureAsync();
                                    scheduleDoLocker = false;
                                    item.OnProcedureDone();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 一般アプリから使用すべきではありません。
        /// </summary>
        public static async Task SuppokashiCheckAndDoAsync()
        {
            foreach (Schedule item in SimpleName<Schedule>.List.Values)
            {
                if (State.IsScheduleVisible(item.Id))
                {
                    // 終了時刻を超過しているか?
                    if (Flags.Now >= (item.StartTime + item.Length))
                    {
                        State.SetScheduleVisible(item.Id, false);  // 消費されるスケジュールはもう見えなくなる。2重実行防止
                        await item.NoHitProcedureAsync();
                        item.OnProcedureDone();
                    }
                }
            }
        }
    }
}
