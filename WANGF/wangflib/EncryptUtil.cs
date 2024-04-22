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
    /// ����@��ֈÍ����w���p
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
		/// ��������Í�������
		/// </summary>
		/// <param name="str">�Í������镶����</param>
		/// <param name="key">�p�X���[�h</param>
		/// <returns>�Í������ꂽ�o�C�i��</returns>
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
		/// �Í������ꂽ������𕜍�������
		/// </summary>
		/// <param name="bytesIn">�Í������ꂽ�o�C�i��</param>
		/// <param name="key">�p�X���[�h</param>
		/// <returns>���������ꂽ������</returns>
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
				"Hello", "���E�̐^��", "�\�C�����g�O���[��", "0123456789"
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
		/// �Í����w���p
		/// ���LURL���������Ă����T���v���\�[�X���C�����Ďg�p
		/// http://dobon.net/vb/dotnet/string/encryptstring.html
		/// </summary>
		class EncryptUtil
	{
		/// <summary>
		/// ��������Í�������
		/// </summary>
		/// <param name="str">�Í������镶����</param>
		/// <param name="key">�p�X���[�h</param>
		/// <returns>�Í������ꂽ�o�C�i��</returns>
		public static byte[] EncryptString(string str, string key)
		{
			//��������o�C�g�^�z��ɂ���
			byte[] bytesIn = System.Text.Encoding.UTF8.GetBytes(str);

			//DESCryptoServiceProvider�I�u�W�F�N�g�̍쐬
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();

			//���L�L�[�Ə������x�N�^������
			//�p�X���[�h���o�C�g�z��ɂ���
			byte[] bytesKey = System.Text.Encoding.UTF8.GetBytes(key);
			//���L�L�[�Ə������x�N�^��ݒ�
			des.Key = resizeBytesArray(bytesKey, des.Key.Length);
			des.IV = resizeBytesArray(bytesKey, des.IV.Length);

			//�Í������ꂽ�f�[�^�������o�����߂�MemoryStream
			using (MemoryStream msOut = new MemoryStream())
			{
				//DES�Í����I�u�W�F�N�g�̍쐬
				ICryptoTransform desdecrypt = des.CreateEncryptor();
				//�������ނ��߂�CryptoStream�̍쐬
				using (CryptoStream cryptStreem = new CryptoStream(msOut,
					desdecrypt, CryptoStreamMode.Write))
				{
					//��������
					cryptStreem.Write(bytesIn, 0, bytesIn.Length);
					cryptStreem.FlushFinalBlock();
					//�Í������ꂽ�f�[�^���擾
					return msOut.ToArray();
				}
			}
		}

		/// <summary>
		/// �Í������ꂽ������𕜍�������
		/// </summary>
		/// <param name="bytesIn">�Í������ꂽ�o�C�i��</param>
		/// <param name="key">�p�X���[�h</param>
		/// <returns>���������ꂽ������</returns>
		public static string DecryptString(byte[] bytesIn, string key)
		{
			//DESCryptoServiceProvider�I�u�W�F�N�g�̍쐬
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();

			//���L�L�[�Ə������x�N�^������
			//�p�X���[�h���o�C�g�z��ɂ���
			byte[] bytesKey = System.Text.Encoding.UTF8.GetBytes(key);
			//���L�L�[�Ə������x�N�^��ݒ�
			des.Key = resizeBytesArray(bytesKey, des.Key.Length);
			des.IV = resizeBytesArray(bytesKey, des.IV.Length);

			//�Í������ꂽ�f�[�^��ǂݍ��ނ��߂�MemoryStream
			using (MemoryStream msIn = new MemoryStream(bytesIn))
			{
				//DES�������I�u�W�F�N�g�̍쐬
				ICryptoTransform desdecrypt = des.CreateDecryptor();
				//�ǂݍ��ނ��߂�CryptoStream�̍쐬
				using (CryptoStream cryptStreem = new CryptoStream(msIn,
					desdecrypt,	CryptoStreamMode.Read))
				{
					//���������ꂽ�f�[�^���擾���邽�߂�StreamReader
					using (StreamReader srOut =	new StreamReader(cryptStreem,
						System.Text.Encoding.UTF8))
					{
						//���������ꂽ�f�[�^���擾����
						return srOut.ReadToEnd();
					}
				}
			}
		}

		/// <summary>
		/// ���L�L�[�p�ɁA�o�C�g�z��̃T�C�Y��ύX����
		/// </summary>
		/// <param name="bytes">�T�C�Y��ύX����o�C�g�z��</param>
		/// <param name="newSize">�o�C�g�z��̐V�����傫��</param>
		/// <returns>�T�C�Y���ύX���ꂽ�o�C�g�z��</returns>
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
