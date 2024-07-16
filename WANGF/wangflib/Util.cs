using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace ANGFLib
{
    // ActionをFunc<Dummy>に置換するためのダミー定義
    public struct Dummy
    {
    }

    public class RegistryMiscInfo
    {
        public bool UseCloudStorage;
    }

    /// <summary>
    /// この属性が付加されたクラスは、CollectTypedObjectsメソッドの収集から除外されます
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExcludeAttribute : Attribute { }

    /// <summary>
    /// ヒントのURLを提供する
    /// </summary>
    public abstract class GameHintInfo
    {
        public abstract string GameHintUrl { get; }
    }

    /// <summary>
    /// オプションの便利なサービスを提供する
    /// 使っても使わなくても良い
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 指定されたアセンブリのリストから指定された型をリストしてそのインスタンスを作成して列挙する
        /// abstractクラスと、ExcludeAttribute付きの型は除外される
        /// </summary>
        /// <typeparam name="TargetType">インスタンスを欲しい型</typeparam>
        /// <param name="enableDetector">その型を列挙に含めるか判定するカスタムデリゲート</param>
        /// <param name="targetAssebmbles">対象とするアセンブリ一覧</param>
        /// <returns>該当する型のインスタンスの列挙</returns>
        public static IEnumerable<TargetType> CollectTypedObjects<TargetType>(Func<TargetType, bool> enableDetector, params Assembly[] targetAssebmbles) where TargetType : class
        {
            foreach (var targetAssebmbly in targetAssebmbles)
            {
                foreach (var t in targetAssebmbly.GetTypes())
                {
                    if (t.IsAbstract) continue;
                    if (t.IsSubclassOf(typeof(TargetType)))
                    {
                        if (t.GetCustomAttributes(typeof(ExcludeAttribute), false).Length == 0)
                        {
                            var tgt = (TargetType)Activator.CreateInstance(t);
                            var paramValue = tgt as TargetType;
                            if (paramValue != null)
                            {
                                if (enableDetector(paramValue)) yield return tgt;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 引数で指定されたアセンブリの中の指定された型のインスタンスの配列を返す
        /// abstractクラスと、ExcludeAttribute付きの型は除外される
        /// </summary>
        /// <typeparam name="TargetType">対象とする型</typeparam>
        /// <param name="targetAssebmbly">対象アセンブリ(現在のアセンブリはAssembly.GetExecutingAssembly()で取る)</param>
        /// <returns></returns>
        public static TargetType[] CollectTypedObjects<TargetType>(Assembly targetAssebmbly) where TargetType : class
        {
            return CollectTypedObjects<TargetType>((dummy) => true, targetAssebmbly).ToArray();
        }
        /// <summary>
        /// 全てのアセンブリからGameStartupInfosから継承したクラスのインスタンスからGameStartupInfoを列挙する
        /// </summary>
        /// <returns>GameStartupInfo型インスタンスの列挙</returns>
        public static IEnumerable<GameStartupInfo> GetOnlyEmbeddedModules()
        {
            var all = CollectTypedObjects<GameStartupInfos>((type) => !type.IsIgnoreFromTopMenu, AppDomain.CurrentDomain.GetAssemblies());
            var seq = Enumerable.Empty<GameStartupInfo>();
            foreach (var item in all) seq = seq.Concat(item.EnumEmbeddedModules());
            return seq;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Color CalcCenterColor(Color c1, Color c2)
        {
            int r = c1.R + (c2.R - c1.R) / 2;
            int g = c1.G + (c2.G - c1.G) / 2;
            int b = c1.B + (c2.B - c1.B) / 2;
            return Color.FromArgb(r, g, b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="rate"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Color CalcBetweenColor(Color c1, Color c2, int rate, int count)
        {
            int r = c1.R + (c2.R - c1.R) * rate / count;
            int g = c1.G + (c2.G - c1.G) * rate / count;
            int b = c1.B + (c2.B - c1.B) * rate / count;
            return Color.FromArgb(r, g, b);
        }

        public static DateTime JounaligStartDateTime;

        public static void InitCulture()
        {
            var cult = System.Globalization.CultureInfo.CreateSpecificCulture("ja-JP");
            System.Threading.Thread.CurrentThread.CurrentCulture = cult;
            //Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture);
            //System.Globalization.CultureInfo.CurrentCulture = cult;
            //System.Globalization.CultureInfo.CurrentUICulture = cult;
        }

        // from https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        public static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        /// <summary>
        /// ランキング用データファイルの生成
        /// </summary>
        /// <returns></returns>
        public static string SaveDefinitionFilesForCollectionRanking(Module module)
        {
            using var writer = new StringWriter();
            var collections = module.GetCollections();
            if (collections.Length == 0) return null;
            writer.WriteLine(module.GetAngfRuntimeXml().id);
            writer.WriteLine(module.GetAngfRuntimeXml().name);
            foreach (var collection in collections)
            {
                writer.WriteLine("@" + collection.Id);
                writer.WriteLine(collection.HumanReadableName);
                foreach (var col2 in collection.Collections)
                {
                    writer.WriteLine(col2.Id);
                    writer.WriteLine(col2.Name);
                    var subs = col2.GetSubItems();
                    if (subs != null)
                        foreach (var col3 in subs)
                        {
                            writer.WriteLine(col3.Id);
                            writer.WriteLine(col3.Name + " (" + col2.Name + ")");
                        }
                }
            }
            return writer.ToString();
        }

#if USE_API_HASH
        private static byte[] rawCalcHash(string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            //var algorithm = new MD5CryptoServiceProvider();
            var algorithm = MD5.Create();
            byte[] bs = algorithm.ComputeHash(data);
            algorithm.Clear();
            return bs;
        }
#endif
        public static int MyCalcHash(string s)
        {
#if USE_API_HASH
            var ar = rawCalcHash(s);
            uint v = 0;
            for (int i = 0; i < ar.Length; i += 4)
            {
                v ^= (uint)(ar[i] << 24) | (uint)(ar[i + 1] << 16) | (uint)(ar[i + 2] << 8) | (uint)(ar[i + 3]);
            }
            return (int)v;
#else
            return GetDeterministicHashCode(s);
#endif
        }
        public static byte[] ReadStreamToByteArray(Stream stream)
        {
            using var stream2 = new MemoryStream();
            for (; ; )
            {
                byte[] buf = new byte[256];
                var len = stream.Read(buf, 0, buf.Length);
                if (len == 0) break;
                stream2.Write(buf, 0, len);
            }
            return stream2.ToArray();
        }
        public static string ReplaceNumbersZenkaku2Hankaku(string src)
        {
            return src.Replace('０', '0')
                .Replace('１', '1')
                .Replace('２', '2')
                .Replace('３', '3')
                .Replace('４', '4')
                .Replace('５', '5')
                .Replace('６', '6')
                .Replace('７', '7')
                .Replace('８', '8')
                .Replace('９', '9');
        }

        public static bool TryParseMyBool(string src, out bool value)
        {
            src = src.Trim().ToLower();
            if (src == "1" || src == "true")
            {
                value = true;
                return true;
            }
            value = false;
            if (src == "0" || src == "false") return true;
            return false;
        }

        public static async Task<MyXmlDoc> LoadMyXmlDocAsync(string filename)
        {
            var result = new MyXmlDoc();
            XDocument doc = null;
            byte[] titlePicture = null;
            // 指定されたファイルが存在する場合は、それを読もうとする
            // 拡張子が".dll"ならリソース。それ以外ならXMLの生ファイル
            if (Path.GetExtension(filename).ToLower() != ".dll")
            {
                doc = XDocument.Load(new StreamReader(filename), LoadOptions.SetLineInfo);
            }
            else
            {
                try
                {
                    var assem = MyAssembly.MyReflectionOnlyLoadFrom(filename);
                    doc = seekAngfRuntimeXmlFile(assem);
                    titlePicture = SeekTitlePicture(assem);
                }
                catch (Exception ex)
                {
                    await UI.Actions.tellAssertionFailedAsync($"{filename} was {ex.ToString()}");
                }
            }
            if (doc == null)
            {
                await UI.Actions.tellAssertionFailedAsync("モジュール" + filename + "にはリソースAngfgRunTime.xmlが含まれていません。");
                //DefaultPersons.システム.Say();
                return null;
            }
            if (!result.LoadFromXDocument(doc, filename)) return null;
            result.xmlFilePath = filename;
            result.TitlePicture = titlePicture;
            return result;
        }
        public static MyXmlDoc LoadMyXmlDoc(Assembly assem)
        {
            var doc = seekAngfRuntimeXmlFile(assem);
            var result = new MyXmlDoc();
            if (!result.LoadFromXDocument(doc, string.Empty)) return null;
            result.xmlFilePath = string.Empty;
            return result;
        }
        private static XDocument seekAngfRuntimeXmlFile(Assembly assem)
        {
            foreach (var n in assem.GetManifestResourceNames())
            {
                if (n.ToLower().EndsWith(".angfruntime.xml"))
                {
                    return XDocument.Load(assem.GetManifestResourceStream(n));
                }
            }
            return null;    // not found
        }
        public static byte[] SeekTitlePicture(Assembly assem)
        {
            foreach (var n in assem.GetManifestResourceNames())
            {
                //System.Diagnostics.Debug.WriteLine(n);
                if (n.ToLower().EndsWith(".wangftitle.jpg"))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        assem.GetManifestResourceStream(n).CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            return null;    // not found
        }

        internal static async Task DummyMethodAsync()
        {
            await Task.Delay(0);
        }
    }

    /// <summary>
    /// バイナリ配列を作成する
    /// </summary>
    public class BinaryArrayBuilder
    {
        private Stream stream;
        private List<byte> list = new List<byte>();
        /// <summary>
        /// 配列を追加する
        /// </summary>
        /// <param name="arToAdd"></param>
        public void AppendArray(byte[] arToAdd)
        {
            list.AddRange(arToAdd);
        }
        /// <summary>
        /// 配列に変換する
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return list.ToArray();
        }
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="stream">ダミー</param>
        public BinaryArrayBuilder(Stream stream)
        {
            this.stream = stream;
        }

    }

    [Serializable]
    public class MessageOnlyException : Exception
    {
        public MessageOnlyException() { }
        public MessageOnlyException(string message) : base(message) { }
        public MessageOnlyException(string message, Exception inner) : base(message, inner) { }
        protected MessageOnlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class MyAssembly
    {
        public static Assembly MyReflectionOnlyLoadFrom(string filePath)
        {
            var mlc = new MetadataLoadContext(new CoreMetadataAssemblyResolver());
            return mlc.LoadFromAssemblyPath(filePath);
        }
        /// <summary>
        /// Resolver for Core Assembly
        /// came from https://github.com/ahsonkhan/azure-sdk-tools/blob/5c28afb51b996c97c67f88563b12f35407357cf1/src/dotnet/Mgmt.CI.BuildTools/CI/CI.Common/Mgmt.CI.Common/Services/ReflectionService.cs#L186 and modefied
        /// </summary>
        class CoreMetadataAssemblyResolver : MetadataAssemblyResolver
        {
            private Assembly _coreAssembly;
            public CoreMetadataAssemblyResolver() { }
            public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
            {
                string name = assemblyName.Name;
                if (name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("System.Reflection.Metadata", StringComparison.OrdinalIgnoreCase) ||
                    // For interop attributes such as DllImport and Guid:
                    name.Equals("System.Runtime.InteropServices", StringComparison.OrdinalIgnoreCase))
                {
                    if (_coreAssembly == null) _coreAssembly = context.LoadFromStream(CreateStreamForCoreAssembly());
                    return _coreAssembly;
                }
                return null;
            }

            private Stream CreateStreamForCoreAssembly()
            {
                // We need a core assembly in IL form. Since this version of this code is for Jitted platforms, the System.Private.Corelib
                // of the underlying runtime will do just fine.
                string assumedLocationOfCoreLibrary = typeof(object).Assembly.Location;
                if (assumedLocationOfCoreLibrary == null || assumedLocationOfCoreLibrary == string.Empty)
                {
                    throw new Exception("Could not find a core assembly");
                }
                return File.OpenRead(GetPathToCoreAssembly());
            }
            private string GetPathToCoreAssembly()
            {
                return typeof(object).Assembly.Location;
            }
        }
    }

    public class RealtimeKeyScan
    {
        // DLLをインポートする必要がある
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;

        public static short myGetKeyState(short virtualKeyCode )
        {
            // if we are in Blazor, then we can't use GetKeyState. Simply ignore and return 0.
            if (General.IsBlazorWebAssembly()) return 0;
            return GetKeyState(virtualKeyCode);
        }
        public static bool IsShiftKeyPress()
        {
            // GetKeyStateは最上位ビットが1か否かでキー投下の有無を取得できる
            return myGetKeyState(VK_SHIFT) < 0;
        }
        public static bool IsCtrlKeyPress()
        {
            // GetKeyStateは最上位ビットが1か否かでキー投下の有無を取得できる
            return myGetKeyState(VK_CONTROL) < 0;
        }
        public static bool IsShiftAndCtrlKeyPress()
        {
            return IsShiftKeyPress() && IsCtrlKeyPress();
        }
    }
}
