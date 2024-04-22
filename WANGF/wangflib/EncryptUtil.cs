#define USE_ALTER_ENCLYPT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ANGFLib
{
#if USE_ALTER_ENCLYPT
    /// <summary>
    /// 自作　代替暗号化ヘルパ
    /// </summary>
    public class EncryptUtil
	{
		private const byte magic1 = 0x83;
		private const byte magic2 = 0xed;
		private static byte sum(byte[] array)
		{
			byte r = 0;
			foreach (var item in array) r += item;
			return r;
		}

		/// <summary>
		/// 文字列を暗号化する
		/// </summary>
		/// <param name="str">暗号化する文字列</param>
		/// <param name="key">パスワード</param>
		/// <returns>暗号化されたバイナリ</returns>
		public static byte[] EncryptString(string str, string key)
		{
			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			byte mask = sum(keyBytes);
			mask |= magic1;
			mask &= magic2;
			byte[] src = Encoding.UTF8.GetBytes(str);
			for (int i = 0; i < src.Length; i++)
			{
				src[i] += (byte)i;
				src[i] += keyBytes[i % keyBytes.Length];
				src[i] ^= mask;
			}
			return src;
		}
		/// <summary>
		/// 暗号化された文字列を復号化する
		/// </summary>
		/// <param name="bytesIn">暗号化されたバイナリ</param>
		/// <param name="key">パスワード</param>
		/// <returns>復号化された文字列</returns>
		public static string DecryptString(byte[] src, string key)
		{
			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			byte mask = sum(keyBytes);
			mask |= magic1;
			mask &= magic2;
			for (int i = 0; i < src.Length; i++)
			{
				src[i] ^= mask;
				src[i] -= keyBytes[i % keyBytes.Length];
				src[i] -= (byte)i;
			}
			return Encoding.UTF8.GetString(src);
		}
#if true
		static EncryptUtil()
		{
			const string testKey = "TheKey";
			string[] testSrc =
			{
				"Hello", "世界の真実", "ソイレントグリーン", "0123456789"
			};
			foreach (var item in testSrc)
			{
				var b = EncryptString(item, testKey);
				var s = DecryptString(b, testKey);
				Debug.Assert(item == s, "EncryptUtil class self test failed");
			}
		}
#endif
#else
		/// <summary>
		/// 暗号化ヘルパ
		/// 下記URLからもらってきたサンプルソースを修正して使用
		/// http://dobon.net/vb/dotnet/string/encryptstring.html
		/// </summary>
		class EncryptUtil
	{
		/// <summary>
		/// 文字列を暗号化する
		/// </summary>
		/// <param name="str">暗号化する文字列</param>
		/// <param name="key">パスワード</param>
		/// <returns>暗号化されたバイナリ</returns>
		public static byte[] EncryptString(string str, string key)
		{
			//文字列をバイト型配列にする
			byte[] bytesIn = System.Text.Encoding.UTF8.GetBytes(str);

			//DESCryptoServiceProviderオブジェクトの作成
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();

			//共有キーと初期化ベクタを決定
			//パスワードをバイト配列にする
			byte[] bytesKey = System.Text.Encoding.UTF8.GetBytes(key);
			//共有キーと初期化ベクタを設定
			des.Key = resizeBytesArray(bytesKey, des.Key.Length);
			des.IV = resizeBytesArray(bytesKey, des.IV.Length);

			//暗号化されたデータを書き出すためのMemoryStream
			using (MemoryStream msOut = new MemoryStream())
			{
				//DES暗号化オブジェクトの作成
				ICryptoTransform desdecrypt = des.CreateEncryptor();
				//書き込むためのCryptoStreamの作成
				using (CryptoStream cryptStreem = new CryptoStream(msOut,
					desdecrypt, CryptoStreamMode.Write))
				{
					//書き込む
					cryptStreem.Write(bytesIn, 0, bytesIn.Length);
					cryptStreem.FlushFinalBlock();
					//暗号化されたデータを取得
					return msOut.ToArray();
				}
			}
		}

		/// <summary>
		/// 暗号化された文字列を復号化する
		/// </summary>
		/// <param name="bytesIn">暗号化されたバイナリ</param>
		/// <param name="key">パスワード</param>
		/// <returns>復号化された文字列</returns>
		public static string DecryptString(byte[] bytesIn, string key)
		{
			//DESCryptoServiceProviderオブジェクトの作成
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();

			//共有キーと初期化ベクタを決定
			//パスワードをバイト配列にする
			byte[] bytesKey = System.Text.Encoding.UTF8.GetBytes(key);
			//共有キーと初期化ベクタを設定
			des.Key = resizeBytesArray(bytesKey, des.Key.Length);
			des.IV = resizeBytesArray(bytesKey, des.IV.Length);

			//暗号化されたデータを読み込むためのMemoryStream
			using (MemoryStream msIn = new MemoryStream(bytesIn))
			{
				//DES復号化オブジェクトの作成
				ICryptoTransform desdecrypt = des.CreateDecryptor();
				//読み込むためのCryptoStreamの作成
				using (CryptoStream cryptStreem = new CryptoStream(msIn,
					desdecrypt,	CryptoStreamMode.Read))
				{
					//復号化されたデータを取得するためのStreamReader
					using (StreamReader srOut =	new StreamReader(cryptStreem,
						System.Text.Encoding.UTF8))
					{
						//復号化されたデータを取得する
						return srOut.ReadToEnd();
					}
				}
			}
		}

		/// <summary>
		/// 共有キー用に、バイト配列のサイズを変更する
		/// </summary>
		/// <param name="bytes">サイズを変更するバイト配列</param>
		/// <param name="newSize">バイト配列の新しい大きさ</param>
		/// <returns>サイズが変更されたバイト配列</returns>
		private static byte[] resizeBytesArray(byte[] bytes, int newSize)
		{
			byte[] newBytes = new byte[newSize];
			if (bytes.Length <= newSize)
			{
				for (int i = 0; i < bytes.Length; i++)
					newBytes[i] = bytes[i];
			}
			else
			{
				int pos = 0;
				for (int i = 0; i < bytes.Length; i++)
				{
					newBytes[pos++] ^= bytes[i];
					if (pos >= newBytes.Length)
						pos = 0;
				}
			}
			return newBytes;
		}
	}
#endif
	}
}
