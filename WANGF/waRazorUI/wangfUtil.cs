using ANGFLib;
//using Blazor.Extensions.Storage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Drawing;
using System.Text;

namespace waRazorUI
{
    public record FlagEditorInfo(int index, string name, Type type, Func<string,Dummy> setValue, Func<string> getValue);

    public class MessageBlock
    {
        public readonly string Message;
        //public readonly Person Talker;
        public readonly string TalkerName;
        public readonly bool IsTalkerDokuhaku;
        public readonly bool IsSuperMessage;
        public readonly int xOffsetMessage;
        public readonly Color TextColor;
        public readonly Color BackColor;
        private Color createHightlightColor(Color color)
        {
            int[] c = { color.R, color.G, color.B };
            int min = int.MaxValue;
            foreach (int n in c)
            {
                if (n < min) min = n;
            }
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == min) c[i] = 0;
            }
            return Color.FromArgb(c[0], c[1], c[2]);
        }
        public MessageBlock(Person talker, string message, Color backColor)
        {
            Message = message;
            TalkerName = talker.MyName;
            IsTalkerDokuhaku = talker is PersonDokuhaku;
            IsSuperMessage = talker is PersonSuper;
            TextColor = talker.MessageColor;
            BackColor = backColor;
        }
        public MessageBlock(string message, Color foreColor, Color backColor)   // 特殊効果専用
        {
            Message = message;
            TalkerName = DefaultPersons.独白.MyName;
            IsTalkerDokuhaku = true;
            TextColor = foreColor;
            BackColor = backColor;
        }
        public MessageBlock(Color backColor)   // 画面クリア用の空白の塊を作る
        {
            Message = "";
            TalkerName = DefaultPersons.独白.MyName;
            TextColor = DefaultPersons.独白.MessageColor;
            BackColor = backColor;
        }
    }

    public interface ILocalStorage
    {
        Task<string> GetItemAsync(string key);
        Task SetItemAsync(string key, string value);
        Task RemoveItemAsync(string key);
        Task ClearAllAsync();
        Task<string[]> EnumItemKeysAsync();
    }
    public class FileSystemStorage : ILocalStorage
    {
        private static string root
        {
            get
            {
                //var s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WANGF", "DATA");
                var s = Path.Combine(General.GetDataRootDirectory(), "Data");
                Directory.CreateDirectory(s);
                return s;
            }
        }
        public static string GetDataRoot() => root;

        public async Task<string> GetItemAsync(string key)
        {
            try
            {
                return await File.ReadAllTextAsync(Path.Combine(root, key));
            }
            catch (Exception)
            {
                return default;
            }
        }
        public async Task SetItemAsync(string key, string value)
        {
            await File.WriteAllTextAsync(Path.Combine(root, key), value);
        }
        public async Task RemoveItemAsync(string key)
        {
            File.Delete(Path.Combine(root, key));
            await Task.Delay(0);
        }
        public async Task ClearAllAsync()
        {
            Directory.Delete(root, true);
            await Task.Delay(0);
        }
        public async Task<string[]> EnumItemKeysAsync()
        {
            await Task.Delay(0);
            return Directory.GetFiles(root).Select(c => Path.GetFileName(c)).ToArray();
        }
    }

    public class LocalStorage: ILocalStorage
    {
        public async Task<string> GetItemAsync(string key)
        {
            try
            {
                var s = await JsWrapper.localStorageGetItemWrapperAsync(key);
                if (s == null) return default;
                // for compatibility
                if (s.StartsWith("\"")) s = s.Substring(1);
                if (s.EndsWith("\"")) s = s.Substring(0, s.Length - 1);
                return s;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task SetItemAsync(string key, string value)
        {
            await JsWrapper.localStorageSetItemWrapperAsync(key, value);
        }
        public async Task RemoveItemAsync(string key)
        {
            await JsWrapper.localStorageRemoveItemWrapperAsync(key);
        }
        public async Task ClearAllAsync()
        {
            await JsWrapper.localStorageClearAllWrapperAsync();
        }
        public async Task<string[]> EnumItemKeysAsync()
        {
            return await JsWrapper.localStorageEnumKeysWrapperAsync();
        }
    }

    public class wangfUtil
    {
        //public static DateTime JounaligStartDateTime;

        public static ILocalStorage LocalStorage => General.IsBlazorWebAssembly() ? new LocalStorage() : new FileSystemStorage();
        private const string UseCloudStorage = "tUseCloudStorage";

        // WANGFの場合、メインスレッドに意味は無い
        internal static void ReportErrorFromMainThread(string message)
        {
            General.ReportError(message);
        }

        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        public static string[] GetEmbeddedResourceNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

#if false
        public static IEnumerable<MyXmlDoc> EnumBuildinModules()
        {
            foreach (var resourceName in GetEmbeddedResourceNames())
            {
                //System.Diagnostics.Debug.WriteLine("Found:"+resourceName);
                if (resourceName.ToLower().EndsWith(".dll"))
                {
                    var pair = General.LoadManifestOnly(GetEmbeddedResourceStream(resourceName));
                    //System.Diagnostics.Debug.WriteLine((pair == null).ToString());
                    var doc = pair.Item1;
                    var ver = pair.Item2;
                    var result = new MyXmlDoc();
                    if (result.LoadFromXDocument(doc, resourceName))
                    {
                        result.xmlFilePath = resourceName;
                        result.versionInfo = ver;
                        yield return result;
                    }
                }
            }
        }
#endif

        public static async Task<SimpleMenuItem[]> CreateFileListAsync(Func<int,string> filenamegetter, bool isIncludeEmpty)
        {
            var list = new List<SimpleMenuItem>();
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var filename = filenamegetter(i);
                    var body = await wangfUtil.LocalStorage.GetItemAsync(filename);
                    var desc = await wangfUtil.LocalStorage.GetItemAsync(FileLayout.CreateDescFileName(filename));
                    if (body == null || desc == null)
                    {
                        if (!isIncludeEmpty) continue;
                        desc = "(new)";
                    }
                    list.Add(new SimpleMenuItem($"File{i}: {desc}", null, i));
                }
            });
            return list.ToArray();
        }

        public static async Task MessageBoxAsync(string message)
        {
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                await JsWrapper.AlertAsync(message);
            });
        }

        public static string FileNameEncoder(string src)
        {
            const string badCharacters = """\/:*?"<>|%""";
            var sb = new StringBuilder();
            foreach (var item in src)
            {
                if (badCharacters.Contains(item))
                    sb.Append($"%{((ushort)item):X4}");
                else
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }
        public static string FileNameDecoder(string src)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '%')
                {
                    sb.Append((char)Convert.ToInt32(src.Substring(i + 1, 4), 16));
                    i += 4;
                }
                else
                    sb.Append(src[i]);
            }
            return sb.ToString();
        }
    }
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateTimeAttribute : Attribute
    {
        public string Date { get; set; }
        public BuildDateTimeAttribute(string date)
        {
            Date = date;
        }
    }
}
