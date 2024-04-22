using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.IO;

namespace ANGFLib
{
    public class JournalingInputDescripter
    {
        public string ModuleId { get; private set; }
        public string FileName { get; private set; }

        public ModuleEx ModuleEx
        {
            get
            {
                return State.LoadedModulesEx.FirstOrDefault(c => c.GetXmlModuleData().Id == ModuleId);
            }
        }

        public Assembly TargetAssembly
        {
            get
            {
                return ModuleEx?.GetType().Assembly;
            }
        }

        public Stream CreateStream()
        {
            if (FileName.StartsWith(Constants.JournalingDirectHeader))
            {
                var s = FileName.Substring(Constants.JournalingDirectHeader.Length);
                var bytes = Encoding.UTF8.GetBytes(s);
                return new MemoryStream(bytes);
            }
            else
            {
                if (ModuleId == null)
                {
                    return new FileStream(FileName, FileMode.Open);
                }
                else
                {
                    var name = TargetAssembly?.GetName().Name;
                    if (name == null) return null;
                    var t = TargetAssembly;
                    var s = t?.GetManifestResourceStream(name + ".TestFiles." + FileName);
                    return s;
                }
            }
        }

        public JournalingInputDescripter(string ModuleId, string FileName)
        {
            this.ModuleId = ModuleId;
            this.FileName = FileName;
        }
        public JournalingInputDescripter(Assembly MyTaregtAssembly, string FileName)
        {
            var found = State.LoadedModulesEx.FirstOrDefault(c =>c.GetType().Assembly.FullName == MyTaregtAssembly.FullName );
            if (found != null) this.ModuleId = found.GetXmlModuleData().Id;
            this.FileName = FileName;
        }

        public static JournalingInputDescripter CreateFromFilename(string fileName)
        {
            return new JournalingInputDescripter((string)null, fileName);
        }
    }
}
