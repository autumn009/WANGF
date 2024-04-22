using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    public static class JournalInit
    {
        public static void Init(IJournalingWriter writer)
        {
            JournalingWriter.ReplaseJournalingWriter(writer);
            JournalingWriter.ReplaseJournalingFileExtention("anj");
            State.JournalingPlayer = async (filename)=>{
                JournalingDocumentPlayer.IsSuccess = true;  // 失敗したらこれをfalseで上書きしてバッチテストの結果とする
                JournalingDocumentPlayer.doc = new JournalingDocument();
                await JournalingDocumentPlayer.doc.JournalingDocumentInitAsync(filename);

                // UIを継承すべき機能は同じ機能を継承する・リアルタイムステータス系
                JournalingDocumentPlayer.JournalActions.progressStatusAsync = UI.Actions.progressStatusAsync;/* DIABLE ASYNC WARN */
                JournalingDocumentPlayer.JournalActions.NotifyStatusMessageAsync = UI.Actions.NotifyStatusMessageAsync;/* DIABLE ASYNC WARN */
                // この2つのアクションに限っては独自性が皆無なので継承させる
                JournalingDocumentPlayer.JournalActions.GetStars = UI.Actions.GetStars;
                JournalingDocumentPlayer.JournalActions.AddStarsAsync = UI.Actions.AddStarsAsync;/* DIABLE ASYNC WARN */

                // アクションセットを差し替え
                UI.Actions = JournalingDocumentPlayer.JournalActions;
            };
        }
    }
}
