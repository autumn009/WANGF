using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// ギフトコードを処理するクラス (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class GiftCodeProcessor
    {
        /// <summary>
        /// ギフトコードを処理する
        /// </summary>
        /// <param name="giftCode"></param>
        /// <returns>処理できたらtrue</returns>
        public abstract Task<bool> ProcessAsync(string giftCode);/* DIABLE ASYNC WARN */
    }

    /// <summary>
    /// カスタムGoNextDayMorningAsyncを処理するクラス
    /// (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class CustomGoNextDayMorningAsyncProcessor/* DIABLE ASYNC WARN */
    {
        /// <summary>
        /// カスタムGoNextDayMorningAsyncを処理する
        /// </summary>
        /// <returns>非同期処理の終了通知</returns>
        public abstract Task GoNextDayMorningAsync();/* DIABLE ASYNC WARN */
    }

    /// <summary>
    /// カスタムGoNextDayMorningAsyncを処理するクラス
    /// (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class CustomGoTimeProcessor
    {
        /// <summary>
        /// カスタムGoNextDayMorningAsyncを処理する
        /// </summary>
        /// <returns>非同期処理の終了通知</returns>
        public abstract void GoTime(int minutes, bool hasOtherEyes, bool enableEvent);
    }

    /// <summary>
    /// カスタムGoNextDayMorningAsyncを処理するクラス
    /// (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class CustomMainLoop
    {
        /// <summary>
        /// カスタムメインループを処理する
        /// </summary>
        /// <returns>非同期処理の終了通知</returns>
        public abstract Task MainLoopAsync();/* DIABLE ASYNC WARN */
    }

    /// <summary>
    /// カスタムGoNextDayMorningAsyncを処理するクラス
    /// (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class DefaultEquipIndexGetter
    {
        /// <summary>
        /// 装備部位UIで指定するデフォルト部位
        /// </summary>
        /// <returns>インデックス値</returns>
        public abstract int GetDefaultEquipIndex();/* DIABLE ASYNC WARN */
    }

    /// <summary>
    /// カスタム日付時刻を処理するクラス
    /// (QueryObjectsで返すためのクラス)
    /// </summary>
    public abstract class CustomDateTimeGetter
    {
        public abstract string CustomDateTime();
    }

    /// <summary>
    /// バージョン3以降で使用されるモジュール定義取得クラスです
    /// </summary>
    public abstract class ModuleEx
    {
        /// <summary>
        /// 型引数で指定された型のオブジェクトが生成可能であれば、それを返す
        /// 少なくともModule型の問い合わせには応答するようにモジュールを作成しなければならない
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>生成可能ならそのオブジェクト群。不可能ならサイズゼロの配列</returns>
        public abstract T[] QueryObjects<T>() where T : class;
    }
}
