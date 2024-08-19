using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StartMeFirst
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void setdir()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
        }

        private void update()
        {
            buttonSetupRuntime.Enabled = File.Exists("SetupRuntime.exe");
            buttonLinkModules.Enabled = File.Exists("modules.txt");
            buttonGame.Enabled = File.Exists("BlazorMaui001.exe");
        }

        void createShortCut(string lnkPath, string fullPath)
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
                        lnk = shell.CreateShortcut(lnkPath);
                        if (lnk != null)
                        {
                            lnk.TargetPath = fullPath;
                            lnk.Save();
                            System.Diagnostics.Debug.WriteLine($"created {lnkPath} to {fullPath}");
                        }
                    }
                }
            }
            finally
            {
                if (lnk != null) Marshal.ReleaseComObject(lnk);
                if (shell != null) Marshal.ReleaseComObject(shell);
            }
        }

        private void buttonSetupRuntime_Click(object sender, EventArgs e)
        {
            Process.Start("BlazorMaui001.exe");
        }

        private void buttonLinkModules_Click(object sender, EventArgs e)
        {
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dstPath = Path.Combine(programDataPath, "autumn", "modules");
            var lines = File.ReadAllLines("modules.txt");
            foreach (var item in lines)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var src = Path.Combine(Directory.GetCurrentDirectory(), item);
                var dst = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), item);
                createShortCut(dst, src);
            }
            MessageBox.Show(this, "リンク作成終了。ゲームを起動できます。");
        }

        private void buttonGame_Click(object sender, EventArgs e)
        {
            Process.Start("BlazorMaui001.exe");
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            setdir();
            update();
        }
    }
}
