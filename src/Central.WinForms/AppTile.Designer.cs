﻿namespace ZeroInstall.Central.WinForms
{
    partial class AppTile
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppTile));
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.buttonRun = new Common.Controls.SplitButton();
            this.contextMenuRun = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSelectVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSelectCommand = new System.Windows.Forms.ToolStripMenuItem();
            this.runMenuSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonConf = new Common.Controls.SplitButton();
            this.contextMenuManage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.linkLabelDetails = new System.Windows.Forms.LinkLabel();
            this.trackingProgressBar = new Common.Controls.TrackingProgressBar();
            this.iconDownloadWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.contextMenuRun.SuspendLayout();
            this.contextMenuManage.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxIcon
            // 
            resources.ApplyResources(this.pictureBoxIcon, "pictureBoxIcon");
            this.pictureBoxIcon.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.App;
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // labelSummary
            // 
            resources.ApplyResources(this.labelSummary, "labelSummary");
            this.labelSummary.Name = "labelSummary";
            // 
            // buttonRun
            // 
            resources.ApplyResources(this.buttonRun, "buttonRun");
            this.buttonRun.ContextMenuStrip = this.contextMenuRun;
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.ShowSplit = true;
            this.buttonRun.SplitMenuStrip = this.contextMenuRun;
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // contextMenuRun
            // 
            this.contextMenuRun.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSelectVersion,
            this.buttonSelectCommand,
            this.runMenuSeperator,
            this.buttonUpdate});
            this.contextMenuRun.Name = "contextMenuRun";
            resources.ApplyResources(this.contextMenuRun, "contextMenuRun");
            // 
            // buttonSelectVersion
            // 
            this.buttonSelectVersion.Name = "buttonSelectVersion";
            resources.ApplyResources(this.buttonSelectVersion, "buttonSelectVersion");
            this.buttonSelectVersion.Click += new System.EventHandler(this.buttonSelectVersion_Click);
            // 
            // buttonSelectCommand
            // 
            this.buttonSelectCommand.Name = "buttonSelectCommand";
            resources.ApplyResources(this.buttonSelectCommand, "buttonSelectCommand");
            this.buttonSelectCommand.Click += new System.EventHandler(this.buttonSelectCommmand_Click);
            // 
            // runMenuSeperator
            // 
            this.runMenuSeperator.Name = "runMenuSeperator";
            resources.ApplyResources(this.runMenuSeperator, "runMenuSeperator");
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Name = "buttonUpdate";
            resources.ApplyResources(this.buttonUpdate, "buttonUpdate");
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonConf
            // 
            resources.ApplyResources(this.buttonConf, "buttonConf");
            this.buttonConf.ContextMenuStrip = this.contextMenuManage;
            this.buttonConf.Name = "buttonConf";
            this.buttonConf.ShowSplit = true;
            this.buttonConf.SplitMenuStrip = this.contextMenuManage;
            this.buttonConf.UseVisualStyleBackColor = true;
            this.buttonConf.Click += new System.EventHandler(this.buttonConf_Click);
            // 
            // contextMenuManage
            // 
            this.contextMenuManage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonRemove});
            this.contextMenuManage.Name = "contextMenuManage";
            resources.ApplyResources(this.contextMenuManage, "contextMenuManage");
            // 
            // buttonRemove
            // 
            this.buttonRemove.Name = "buttonRemove";
            resources.ApplyResources(this.buttonRemove, "buttonRemove");
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // linkLabelDetails
            // 
            resources.ApplyResources(this.linkLabelDetails, "linkLabelDetails");
            this.linkLabelDetails.Name = "linkLabelDetails";
            this.linkLabelDetails.TabStop = true;
            this.linkLabelDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDetails_LinkClicked);
            // 
            // trackingProgressBar
            // 
            resources.ApplyResources(this.trackingProgressBar, "trackingProgressBar");
            this.trackingProgressBar.Name = "trackingProgressBar";
            // 
            // iconDownloadWorker
            // 
            this.iconDownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.iconDownloadWorker_DoWork);
            this.iconDownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.iconDownloadWorker_RunWorkerCompleted);
            // 
            // AppTile
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonConf);
            this.Controls.Add(this.linkLabelDetails);
            this.Controls.Add(this.trackingProgressBar);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonRun);
            this.MaximumSize = new System.Drawing.Size(4096, 60);
            this.MinimumSize = new System.Drawing.Size(220, 60);
            this.Name = "AppTile";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.contextMenuRun.ResumeLayout(false);
            this.contextMenuManage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelSummary;
        private Common.Controls.SplitButton buttonRun;
        private Common.Controls.SplitButton buttonConf;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuManage;
        private System.Windows.Forms.ToolStripMenuItem buttonRemove;
        private System.Windows.Forms.LinkLabel linkLabelDetails;
        private Common.Controls.TrackingProgressBar trackingProgressBar;
        private System.ComponentModel.BackgroundWorker iconDownloadWorker;
        private System.Windows.Forms.ContextMenuStrip contextMenuRun;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectVersion;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectCommand;
        private System.Windows.Forms.ToolStripSeparator runMenuSeperator;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdate;
    }
}
