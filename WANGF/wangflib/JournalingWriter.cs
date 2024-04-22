using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 独自のジャーナリングWriterを実装できます
    /// </summary>
    public interface IJournalingWriter
    {
        /// <summary>
        /// ジャーナリング機能は有効か?
        /// </summary>
        /// <returns>有効ならTrue</returns>
        bool IsAvailable();
        /// <summary>
        /// 指定ファイルに対してジャーナリングを開始する
        /// </summary>
        /// <param name="filename">出力ファイル名</param>
        void Create(string filename);
        /// <summary>
        /// 1つのコマンドを出力する
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="arguments">任意の数の引数 (スペース区切りで出力される)</param>
        void Write(string commandName, params object[] arguments);
        /// <summary>
        /// ジャーナリングを終了する
        /// </summary>
        void Close();
    }
    /// <summary>
    /// 何もしないジャーナリングWriter
    /// </summary>
    public class DummyJournalingWriter : IJournalingWriter
    {
        /// <summary>
        /// 無効を示す
        /// </summary>
        /// <returns>無効(false)を返します</returns>
        public bool IsAvailable() { return false; }
        /// <summary>
        /// 無効です
        /// </summary>
        /// <param name="filename">ダミー</param>
        public void Create(string filename) { }
        /// <summary>
        /// 無効です
        /// </summary>
        /// <param name="commandName">ダミー</param>
        /// <param name="arguments">ダミー</param>
        public void Write(string commandName, params object[] arguments) { }
        /// <summary>
        /// 無効です
        /// </summary>
        public void Close() { }
    }
    /// <summary>
    /// JournalingWriterの入り口です
    /// </summary>
    public static class JournalingWriter
    {
        private static IJournalingWriter journalingWriter = new DummyJournalingWriter();
        /// <summary>
        /// ジャーナリングファイルの拡張子
        /// </summary>
        public static string journalingFileExtention {get; private set;}
        static JournalingWriter()
        {
            journalingFileExtention = "dummyExtention";
        }

        /// <summary>
        /// ジャーナリングの書き込み機能オブジェクトを置き換える
        /// </summary>
        /// <param name="jWriter">置き換えるべきオブジェクト</param>
        public static void ReplaseJournalingWriter(IJournalingWriter jWriter)
        {
            journalingWriter = jWriter;
        }
        /// <summary>
        /// ジャーナリングファイルの拡張子をセットする
        /// </summary>
        /// <param name="ext"></param>
        public static void ReplaseJournalingFileExtention(string ext)
        {
            journalingFileExtention = ext;
        }
        /// <summary>
        /// ジャーナリング機能は有効か?
        /// </summary>
        /// <returns>有効ならTrue</returns>
        public static bool IsAvailable() { return journalingWriter.IsAvailable(); }
        /// <summary>
        /// 一時的に出力を止める
        /// </summary>
        /// <returns>止めるならTrue。初期値はFalse</returns>
        public static bool IsTemporaryStopped { get; set; }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="filename">一般アプリから使うべきではありません。</param>
        public static void Create(string filename)
        {
            journalingWriter.Create(filename);
        }
        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        public static void Write(string commandName)
        {
            if (!IsTemporaryStopped) journalingWriter.Write(commandName);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="argument">一般アプリから使うべきではありません。</param>
        public static void Write(string commandName, string argument)
        {
            if (!IsTemporaryStopped) journalingWriter.Write(commandName,argument);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="argument">一般アプリから使うべきではありません。</param>
        public static void Write(string commandName, int argument)
        {
            if (!IsTemporaryStopped) journalingWriter.Write(commandName, argument);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="argument1">一般アプリから使うべきではありません。</param>
        /// <param name="argument2">一般アプリから使うべきではありません。</param>
        public static void Write(string commandName, int argument1, string argument2)
        {
            if (!IsTemporaryStopped) journalingWriter.Write(commandName, argument1, argument2);
        }

        /// <summary>
        /// モジュール独自のジャーナリング情報を書き込みます。
        /// </summary>
        /// <param name="argument1">対象となるモジュールを指定します</param>
        /// <param name="arguments">モジュール定義のジャーナリングされる情報です。空白文字を含んではいけません。</param>
        public static void WriteEx(Module argument1, params string[] arguments)
        {
            string[] ar1 = { argument1.Id };
            if (!IsTemporaryStopped) journalingWriter.Write("EX", ar1.Concat(arguments).ToArray());
        }

        /// <summary>
        /// モジュール独自のジャーナリング情報を書き込みます。
        /// </summary>
        /// <param name="argument1">対象となるモジュールを指定します</param>
        /// <param name="arguments">モジュール定義のジャーナリングされる情報です。空白文字を含んではいけません。</param>
        public static void WriteEx2(Module argument1, string[] arguments)
        {
            WriteEx(argument1, arguments);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="commandName">一般アプリから使うべきではありません。</param>
        /// <param name="index">一般アプリから使うべきではありません。</param>
        /// <param name="item">一般アプリから使うべきではありません。</param>
        public static void WriteEquip(string commandName, int index, Item item)
        {
            if (!IsTemporaryStopped)
            {
                var firstArg = index.ToString() + "_" + Flags.Equip.TargetPersonId;
                if (item.IsItemNull)
                    journalingWriter.Write(commandName, firstArg);
                else
                    journalingWriter.Write(commandName, firstArg, item.HumanReadableName);
            }
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        /// <param name="comment">一般アプリから使うべきではありません。</param>
        public static void WriteComment(string comment)
        {
            if (!IsTemporaryStopped) journalingWriter.Write("*", comment);
        }

        /// <summary>
        /// 一般アプリから使うべきではありません。
        /// </summary>
        public static void Close()
        {
            journalingWriter.Close();
        }
    }
}
