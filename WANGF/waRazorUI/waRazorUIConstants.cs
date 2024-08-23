using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waRazorUI
{
    internal static class waRazorUIConstants
    {
        internal const string ShortName = "WANGF";
        internal const string LongName = "Web Autumn's Novel Game Framework";
        internal const string Version = "0.35 Beta";
        internal static string BuildDateTimeString
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var attr = Attribute.GetCustomAttribute(assembly, typeof(BuildDateTimeAttribute)) as BuildDateTimeAttribute;
                return attr?.Date;
            }
        }
        internal static bool IsTesting
        {
            get
            {
                return Version.ToLower().Contains("beta") || Version.ToLower().Contains("alpha");
            }
        }
    }
}
