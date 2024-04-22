using ANGFLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace wangflib
{
    public static class MainLoop
    {
        // OnBeforeCommandAsyncで最初からのやり直しをリクエスト
        public static bool AbortRequest = false;
        public static async Task DoItAsync()
        {
            foreach (var item in State.LoadedModulesEx)
            {
                var ar = item.QueryObjects<CustomMainLoop>();
                if (ar.Length > 0)
                {
                    // if found custom GoTime, call it and return
                    await ar[0].MainLoopAsync();
                    return;
                }
            }

            //await State.WarpToAsync(new PlaceFrontMenu());
            for (; ; )
            {
                // Save System File if it's dirty.
                await SystemFile.SaveIfDirtyAsync();

                bool result = await ScheduleCheck.EventCheckAsync(Flags.Now, Flags.Now);
                if (result) continue;

                await ScheduleCheck.SuppokashiCheckAndDoAsync();

                foreach (ANGFLib.Module n in State.loadedModules)
                {
                    AbortRequest = false;
                    await n.OnBeforeCommandAsync();
                    if (AbortRequest) break;
                }
                if (AbortRequest) continue;

                if (Flags.Now != DateTime.MinValue && State.今日の就寝時刻 != DateTime.MinValue && Flags.Now >= State.今日の就寝時刻)
                {
                    DefaultPersons.システム.Say("既に今日の就寝時刻を過ぎています。すぐに帰宅、就寝します。");
                    await State.GoNextDayMorningAsync();
                    continue;
                }

                //System.Diagnostics.Debug.WriteLine("State.CurrentPlace.Id=[" + State.CurrentPlace.Id+"]");

                List<SimpleMenuItem> list = new List<SimpleMenuItem>();
                var list2 = General.FindPersonsWithPlace(State.CurrentPlace.Id);
                if (list2.Length > 0)
                {
                    list.Add(new SimpleMenuItem("話す", General.TalkAsync));
                }
                bool goodflag = await State.CurrentPlace.ConstructMenuAsync(list);
                goodflag &= State.CurrentPlace.ConstructMenu(list);
                foreach (ANGFLib.Module n in State.loadedModules)
                {
                    if (!goodflag) break;
                    goodflag &= n.ConstructMenu(list, State.CurrentPlace);
                }
                if (!goodflag)
                {
                    //Console.WriteLine(State.CurrentPlace.HumanReadableName);
                    await State.CurrentPlace.OnMenuAsync();
                }
                else
                {
                    if (General.CanMove(General.candMoveMenu, Moves.HowToMove))
                    {
                        list.Add(new SimpleMenuItem("移動", () => General.MoveMenuAsync(General.candMoveMenu, Moves.HowToMove, false)));
                    }
                    if (General.CanMove(General.candSubMoveMenu, Moves.HowToSubMove, true))
                    {
                        list.Add(new SimpleMenuItem("サブ移動", () => General.MoveMenuAsync(General.candSubMoveMenu, Moves.HowToSubMove, true)));
                    }

                    if ((State.MenuStopMaps & MenuStopControls.System) == 0)
                    {
                        await UI.SimpleMenuAsync("どうしようか", list.ToArray());
                    }
                    else
                    {
                        await UI.SimpleMenuWithoutSystemAsync("どうしようか", list.ToArray());
                    }

                    foreach (ANGFLib.Module n in State.loadedModules)
                    {
                        AbortRequest = false;
                        await n.OnAfterCommandAsync();
                        if (AbortRequest) break;
                    }
                    if (AbortRequest) continue;
                }
                if (State.CurrentPlace.Id == "") break;
            }
        }
    }
}
