using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// プレイバックして欲しいファイルをキューします
    /// </summary>
    public static class JournalPlaybackQueue
    {
        private static Queue<JournalingInputDescripter> queue = new Queue<JournalingInputDescripter>();
        private static object obj = new object();
        /// <summary>
        /// ファイル名をキューに追加します
        /// 追加されたファイルは自動的にシステムが拾い上げてプレイバックされます。
        /// </summary>
        /// <param name="filename"></param>
        public static void Enqueue(JournalingInputDescripter filename)
        {
            queue.Enqueue(filename);
        }
        /// <summary>
        /// 一般アプリからは使うべきではありません
        /// </summary>
        /// <returns>キューに格納されていたファイル名です</returns>
        public static JournalingInputDescripter Dequeue()
        {
            lock (obj)
            {
                if (queue.Count == 0) return null;
                return queue.Dequeue();
            }
        }
        /// <summary>
        /// キューに入っているファイル名の数です
        /// </summary>
        /// <returns>ファイル名の数</returns>
        public static int Count()
        {
            return queue.Count;
        }
        /// <summary>
        /// キューをクリアします。キュー関連でエラーが起きて停止する場合は呼び出されるべきです
        /// </summary>
        public static void Clear()
        {
            queue.Clear();
        }
    }
}
