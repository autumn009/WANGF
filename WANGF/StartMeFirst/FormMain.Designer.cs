namespace StartMeFirst
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonSetupRuntime = new System.Windows.Forms.Button();
            this.buttonLinkModules = new System.Windows.Forms.Button();
            this.buttonGame = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonSetupRuntime
            // 
            this.buttonSetupRuntime.Location = new System.Drawing.Point(12, 33);
            this.buttonSetupRuntime.Name = "buttonSetupRuntime";
            this.buttonSetupRuntime.Size = new System.Drawing.Size(311, 23);
            this.buttonSetupRuntime.TabIndex = 0;
            this.buttonSetupRuntime.Text = "システム側ランタイム(.NET/WebView)のセットアップ(&R)";
            this.buttonSetupRuntime.UseVisualStyleBackColor = true;
            this.buttonSetupRuntime.Click += new System.EventHandler(this.buttonSetupRuntime_Click);
            // 
            // buttonLinkModules
            // 
            this.buttonLinkModules.Location = new System.Drawing.Point(12, 62);
            this.buttonLinkModules.Name = "buttonLinkModules";
            this.buttonLinkModules.Size = new System.Drawing.Size(311, 23);
            this.buttonLinkModules.TabIndex = 1;
            this.buttonLinkModules.Text = "ゲーム・モジュールへのリンクのセットアップ(&L)";
            this.buttonLinkModules.UseVisualStyleBackColor = true;
            this.buttonLinkModules.Click += new System.EventHandler(this.buttonLinkModules_Click);
            // 
            // buttonGame
            // 
            this.buttonGame.Location = new System.Drawing.Point(12, 91);
            this.buttonGame.Name = "buttonGame";
            this.buttonGame.Size = new System.Drawing.Size(311, 23);
            this.buttonGame.TabIndex = 2;
            this.buttonGame.Text = "ゲーム起動(&G)";
            this.buttonGame.UseVisualStyleBackColor = true;
            this.buttonGame.Click += new System.EventHandler(this.buttonGame_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonExit.Location = new System.Drawing.Point(12, 120);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(311, 23);
            this.buttonExit.TabIndex = 3;
            this.buttonExit.Text = "終了(&X)";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "2つのセットアップを行った後でゲームを起動してください";
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonGame;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonExit;
            this.ClientSize = new System.Drawing.Size(337, 169);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonGame);
            this.Controls.Add(this.buttonLinkModules);
            this.Controls.Add(this.buttonSetupRuntime);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "WANGF Start Me First";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSetupRuntime;
        private System.Windows.Forms.Button buttonLinkModules;
        private System.Windows.Forms.Button buttonGame;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Label label1;
    }
}

