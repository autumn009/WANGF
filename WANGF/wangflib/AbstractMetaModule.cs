using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ANGFLib
{
    public enum MetaModuleAutoTest { Yes, No }
    public abstract record GameStartupInfo(string Id, string name, bool Is18K, string description, MetaModuleAutoTest autotest)
    {
        private string touchKey => $"ae690639-c2a0-4156-ae3c-cf067b2dd2f4_WANGF_Touch_{Id}";
        public DateTime GetTouchedDateTime()
        {
            var s = SystemFile.GetFlagString(touchKey);
            DateTime.TryParseExact(s, Constants.DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out var dt);
            return dt;
        }
        public void Touch()
        {
            SystemFile.SetFlagString(touchKey, DateTime.Now.ToString(Constants.DateTimeFormat));
        }
        public abstract Task StartGameAsync();
    }

    public abstract class GameStartupInfos
    {
        public abstract IEnumerable<GameStartupInfo> EnumEmbeddedModules();
    }

    public record StaticGameStartupInfo : GameStartupInfo
    {
        public static Func<Type[], Task> myStartupAsync;
        private Type[] modules { get; init; }
        public StaticGameStartupInfo(string id, string name, Type[] modules, bool Is18K, string description, MetaModuleAutoTest autotest) : base(id, name, Is18K, description, autotest)
        {
            this.modules = modules;
        }
        public override async Task StartGameAsync()
        {
            await myStartupAsync(modules);
        }
        public static StaticGameStartupInfo Create(Type[] modules)
        {
            foreach (var item in modules.Reverse())
            {
                var doc = Util.LoadMyXmlDoc(item.Assembly);
                if (doc != null)
                {
                    return new StaticGameStartupInfo(doc.id, doc.name, modules, doc.is18k, doc.description, doc.AutoTestEnabled? MetaModuleAutoTest.Yes: MetaModuleAutoTest.No);
                }
            }
            return null;
        }
    }

    public record FileGameStartupInfo : GameStartupInfo
    {
        public static Func<Task> myStartupAsync;
        private MyXmlDoc myDoc { get; init; }
        public FileGameStartupInfo(string id, string name, MyXmlDoc myDoc, bool Is18K, string description, MetaModuleAutoTest autotest) : base(id, name, Is18K, description, autotest)
        {
            this.myDoc = myDoc;
        }
        public override async Task StartGameAsync()
        {
            await startupSubAsync(myDoc);
        }
        private async Task startupSubAsync(MyXmlDoc f)
        {
            string startposId = await Scenarios.LoadAsync(f);
            Flags.CurrentPlaceId = startposId;
            Flags.CurrentWorldId = Constants.DefaultWordId;
            await myStartupAsync();
        }
    }
}
