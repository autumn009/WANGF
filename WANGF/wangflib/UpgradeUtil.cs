using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// ファイルのアップグレードチェックを手助けします。
    /// 使用しても使用しなくても構いません。
    /// </summary>
    public static class UpgradeUtil
    {
        /// <summary>
        /// 正規版のファイルを持っているかを調べます.
        /// WebPlayerでは機能しません
        /// </summary>
        /// <param name="dummy">ダミーです</param>
        /// <param name="msg">事前に表示する説明ウィンドウです</param>
        /// <param name="checkSams">SHA1形式のチェックサムを16進数文字列形式で複数渡します。どれか1つに一致するファイルが指定されれば認証はパスします。</param>
        /// <returns>チェックにパスするとtrueになります</returns>
        public static bool UpgradeCheck(object dummy, string msg, string[] checkSams)
        {
            return UI.Actions.UpgradeCheck(msg, checkSams);
        }
    }
}
