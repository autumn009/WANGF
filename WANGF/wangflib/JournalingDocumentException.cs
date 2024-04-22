using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// ジャーナリング機能のコンパイルエラーの通知用です。一般のモジュールは使用すべきではありません。
    /// </summary>
    public class JournalingDocumentException : ApplicationException
    {
        //public JournalingDocumentException() { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="message">メッセージの内容です。</param>
        public JournalingDocumentException(string message) : base(message) { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="message">メッセージの内容です。</param>
        /// <param name="inner">内部的な例外です。</param>
        public JournalingDocumentException(string message, Exception inner)
            : base(message, inner)
        {
            JournalPlaybackQueue.Clear();
        }
        //protected JournalingDocumentException(
        //  System.Runtime.Serialization.SerializationInfo info,
        //  System.Runtime.Serialization.StreamingContext context)
        //	: base(info, context) { }
        public JournalingDocumentException(string message, JournalingNode node)
            : base($"{message} in {node.SourceFileName.FileName}:{node.LineNumber}")
        {
        }
        public JournalingDocumentException(string message, string fileName, int lineNumber)
            : base($"{message} in {fileName}:{lineNumber}")
        {
        }
    }
}
