using ANGFLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace waRazorUI
{
    static class FileLayout
    {
        internal static string CreateSaveFileName(int n) => General.FileExtention + "_File" + n;
        internal static string CreateAutoSaveFileName(int n) => General.FileExtention + "_Auto" + n;
        internal static string CreateDescFileName(string filename) => filename + "_desc";
        public static string LnkFileToTargetPath(string fullPath)
        {
            dynamic shell = null;   // IWshRuntimeLibrary.WshShell
            dynamic lnk = null;     // IWshRuntimeLibrary.IWshShortcut
            try
            {
#pragma warning disable CA1416 // プラットフォームの互換性を検証
                // available in Windows only
                var type = Type.GetTypeFromProgID("WScript.Shell");
#pragma warning restore CA1416 // プラットフォームの互換性を検証
                if (type != null)
                {
                    shell = Activator.CreateInstance(type);
                    if (shell != null)
                    {
                        lnk = shell.CreateShortcut(fullPath);
                        if (lnk != null)
                        {
                            if (string.IsNullOrEmpty(lnk.TargetPath)) return null; // lnk file not found
                            return lnk.TargetPath;
                        }
                    }
                }
                return null;
            }
            finally
            {
                if (lnk != null) Marshal.ReleaseComObject(lnk);
                if (shell != null) Marshal.ReleaseComObject(shell);
            }
        }
    }
}
