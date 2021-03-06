﻿using System;
using System.IO;
using System.Windows.Forms;
using UploadersLib;
using ZScreenLib.Properties;

namespace ZScreenLib
{
    public partial class DestSelector : UserControl
    {
        #region ConfigGUI

        private bool HasValidFileUploader()
        {
            foreach (ToolStripMenuItem tsmi in tsddbDestFile.DropDownItems)
            {
                FileDestination ut = (FileDestination)tsmi.Tag;
                switch (ut)
                {
                    case FileDestination.CustomUploader:
                        tsmi.Enabled = Engine.ConfigUploaders.CustomUploadersList.Count > 0;
                        break;
                    case FileDestination.Dropbox:
                        tsmi.Enabled = Engine.ConfigUploaders.DropboxOAuthInfo != null;
                        break;
                    case FileDestination.FTP:
                        tsmi.Enabled = Engine.ConfigUploaders.FTPAccountList2.Count > 0;
                        break;
                    case FileDestination.Minus:
                        tsmi.Enabled = Engine.ConfigUploaders.MinusConfig.Tokens.Count > 0;
                        break;
                    case FileDestination.RapidShare:
                    case FileDestination.SendSpace:
                        break;
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestFile.DropDownItems)
            {
                if (!tsmi.Enabled)
                {
                    tsmi.Checked = false; // if not enabled then we don't need it checked either issue 604
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestFile.DropDownItems)
            {
                if (tsmi.Enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasValidImageUploader(bool bHasValidFileUploader)
        {
            foreach (ToolStripMenuItem tsmi in tsddbDestImage.DropDownItems)
            {
                ImageDestination ut = (ImageDestination)tsmi.Tag;
                switch (ut)
                {
                    case ImageDestination.FileUploader:
                        tsmi.Enabled = bHasValidFileUploader;
                        break;
                    case ImageDestination.Flickr:
                        tsmi.Enabled = !string.IsNullOrEmpty(Engine.ConfigUploaders.FlickrAuthInfo.Token);
                        break;
                    case ImageDestination.ImageShack:
                        tsmi.Enabled = Engine.ConfigUploaders.ImageShackAccountType == AccountType.Anonymous ||
                            Engine.ConfigUploaders.ImageShackAccountType == AccountType.User && !string.IsNullOrEmpty(Engine.ConfigUploaders.ImageShackRegistrationCode);
                        break;
                    case ImageDestination.Imgur:
                        tsmi.Enabled = Engine.ConfigUploaders.ImgurOAuthInfo != null;
                        break;
                    /*case ImageDestination.MediaWiki:
                        tsmi.Enabled = Engine.ConfigUploaders.MediaWikiAccountList.Count > 0;
                        break;*/
                    case ImageDestination.Photobucket:
                        tsmi.Enabled = Engine.ConfigUploaders.PhotobucketOAuthInfo != null;
                        break;
                    case ImageDestination.TinyPic:
                        tsmi.Enabled = Engine.ConfigUploaders.TinyPicAccountType == AccountType.Anonymous ||
                            Engine.ConfigUploaders.TinyPicAccountType == AccountType.User &&
                         !string.IsNullOrEmpty(Engine.ConfigUploaders.TinyPicRegistrationCode);
                        break;
                    case ImageDestination.Twitpic:
                        tsmi.Enabled = !string.IsNullOrEmpty(Engine.ConfigUploaders.TwitPicPassword);
                        break;
                    case ImageDestination.Twitsnaps:
                        tsmi.Enabled = Engine.ConfigUploaders.TwitterOAuthInfoList.Count > 0;
                        break;
                    case ImageDestination.UploadScreenshot:
                        break;
                    case ImageDestination.yFrog:
                        tsmi.Enabled = !string.IsNullOrEmpty(Engine.ConfigUploaders.YFrogPassword);
                        break;
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestImage.DropDownItems)
            {
                if (!tsmi.Enabled)
                {
                    tsmi.Checked = false; // if not enabled then we don't need it checked either issue 604
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestImage.DropDownItems)
            {
                if (tsmi.Enabled && tsmi.Checked)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasValidTextUploader(bool bHasValidFileUploader)
        {
            foreach (ToolStripMenuItem tsmi in tsddbDestText.DropDownItems)
            {
                TextDestination ut = (TextDestination)tsmi.Tag;
                switch (ut)
                {
                    case TextDestination.FileUploader:
                        tsmi.Enabled = bHasValidFileUploader;
                        if (!tsmi.Enabled)
                        {
                            tsmi.Checked = false;
                        }
                        break;
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestText.DropDownItems)
            {
                if (!tsmi.Enabled)
                {
                    tsmi.Checked = false; // if not enabled then we don't need it checked either issue 604
                }
            }

            foreach (ToolStripMenuItem tsmi in tsddbDestText.DropDownItems)
            {
                if (tsmi.Enabled && tsmi.Checked)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ReconfigOutputsUI()
        {
            // Outputs > Files
            bool bHasValidFileUploader = HasValidFileUploader();

            // Outputs > Text
            bool bHasValidTextUploader = HasValidTextUploader(bHasValidFileUploader);

            // Outputs > Images
            bool bHasValidImageUploader = HasValidImageUploader(bHasValidFileUploader);

            // Outputs
            foreach (ToolStripMenuItem tsmi in tsddbOutputs.DropDownItems)
            {
                OutputEnum ut = (OutputEnum)tsmi.Tag;
                switch (ut)
                {
                    case OutputEnum.RemoteHost:
                        if (!bHasValidTextUploader && !bHasValidImageUploader)
                        {
                            tsmi.Checked = false;
                        }
                        break;
                    case OutputEnum.SharedFolder:
                        tsmi.Enabled = Engine.ConfigUploaders.LocalhostAccountList.Count > 0;
                        if (!tsmi.Enabled)
                        {
                            tsmi.Checked = false;
                        }
                        break;
                }
            }

            return false;
        }

        #endregion ConfigGUI

        public DestSelector()
        {
            InitializeComponent();
        }

        private void DestSelector_Load(object sender, System.EventArgs e)
        {
            foreach (ToolStripItem tsi in tsDest.Items)
            {
                if (tsi is ToolStripDropDownButton)
                {
                    ToolStripDropDownButton tsddb = tsi as ToolStripDropDownButton;
                    tsddb.MouseHover += new System.EventHandler(tsddb_MouseHover);
                    if (!Engine.ConfigApp.SupportMultipleDestinations)
                    {
                        tsddb.DropDownItemClicked += new ToolStripItemClickedEventHandler(tsddb_DropDownItemClickedRestrictToOneItem);
                    }
                }
            }
        }

        public void DropDownMenusClose()
        {
            foreach (ToolStripItem tsi in tsDest.Items)
            {
                if (tsi is ToolStripDropDownButton)
                {
                    ToolStripDropDownButton tsddb = tsi as ToolStripDropDownButton;
                    tsddb.DropDown.Close();
                }
            }
        }

        public void EnableDisableDestControls(ToolStripItemClickedEventArgs e = null)
        {
            ToolStripMenuItem tsmiOClipboard = GetOutputTsmi(tsddbOutputs, OutputEnum.Clipboard);
            ToolStripMenuItem tsmiOLocalDisk = GetOutputTsmi(tsddbOutputs, OutputEnum.LocalDisk);
            ToolStripMenuItem tsmiORemoteHost = GetOutputTsmi(tsddbOutputs, OutputEnum.RemoteHost);
            ToolStripMenuItem tsmiOSharedFolder = GetOutputTsmi(tsddbOutputs, OutputEnum.SharedFolder);

            ToolStripMenuItem tsmiCCData = GetClipboardContentTsmi(tsddbClipboardContent, ClipboardContentEnum.Data);
            ToolStripMenuItem tsmiCCLocal = GetClipboardContentTsmi(tsddbClipboardContent, ClipboardContentEnum.Local);
            ToolStripMenuItem tsmiCCRemote = GetClipboardContentTsmi(tsddbClipboardContent, ClipboardContentEnum.Remote);
            ToolStripMenuItem tsmiCCOCR = GetClipboardContentTsmi(tsddbClipboardContent, ClipboardContentEnum.OCR);

            string mspView = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), @"Microsoft Shared\MODI\12.0\MSPVIEW.EXE");
            tsmiCCOCR.Enabled = File.Exists(mspView);

            tsmiCCLocal.Enabled = tsmiOLocalDisk.Checked;
            if (!tsmiCCLocal.Enabled)
            {
                // if data is not stored in Local Disk then nothing file path related can be stored in Clipboard
                tsmiCCLocal.Checked = false;
            }

            tsmiCCRemote.Enabled = tsmiORemoteHost.Checked || tsmiOSharedFolder.Checked;
            if (!tsmiCCRemote.Enabled)
            {
                // if data is not stored in Remote Host then nothing URL related can be stored in Clipboard
                tsmiCCRemote.Checked = false;
            }

            tsddbDestImage.Enabled = tsmiORemoteHost.Checked && tsmiCCRemote.Enabled;
            tsddbDestFile.Enabled = tsmiORemoteHost.Checked && tsmiCCRemote.Enabled;
            tsddbDestText.Enabled = tsmiORemoteHost.Checked && tsmiCCRemote.Enabled;
            tsddbLinkFormat.Enabled = tsmiOClipboard.Checked && !tsmiCCData.Checked;
            tsddbDestLink.Enabled = tsmiORemoteHost.Checked && tsmiCCRemote.Enabled;

            tsddbClipboardContent.Enabled = tsmiOClipboard.Checked;

            DestSelectorHelper.UpdateToolStripDest(tsddbClipboardContent);
        }

        public ToolStripMenuItem GetClipboardContentTsmi(ToolStripDropDownButton tsddb, ClipboardContentEnum et)
        {
            foreach (ToolStripMenuItem tsmi in tsddb.DropDownItems)
            {
                if ((ClipboardContentEnum)tsmi.Tag == et)
                {
                    return tsmi;
                }
            }
            return new ToolStripMenuItem();
        }

        public ToolStripMenuItem GetLinkFormatTsmi(ToolStripDropDownButton tsddb, LinkFormatEnum lf)
        {
            foreach (ToolStripMenuItem tsmi in tsddb.DropDownItems)
            {
                if ((LinkFormatEnum)tsmi.Tag == lf)
                {
                    return tsmi;
                }
            }
            return new ToolStripMenuItem();
        }

        public ToolStripMenuItem GetOutputTsmi(ToolStripDropDownButton tsddb, OutputEnum et)
        {
            foreach (ToolStripMenuItem tsmi in tsddb.DropDownItems)
            {
                if ((OutputEnum)tsmi.Tag == et)
                {
                    return tsmi;
                }
            }
            return new ToolStripMenuItem();
        }

        private void RestrictToOneCheck(ToolStripDropDownButton tsddb, ToolStripItemClickedEventArgs e)
        {
            for (int i = 0; i < tsddb.DropDownItems.Count; i++)
            {
                ToolStripMenuItem tsmi = (ToolStripMenuItem)tsddb.DropDownItems[i];
                tsmi.Checked = tsmi == e.ClickedItem && !((ToolStripMenuItem)e.ClickedItem).Checked;
            }
        }

        public void tsbDestConfig_Click(object sender, System.EventArgs e)
        {
            UploadersConfigForm form = new UploadersConfigForm(Engine.ConfigUploaders, Engine.ConfigUI.ApiKeysConfig);
            form.Icon = Resources.zss_main;
            if (form.ShowDialog() == DialogResult.OK)
            {
                ReconfigOutputsUI();
            }
        }

        private void tsddb_DropDownItemClickedRestrictToOneItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripDropDownButton tsddb = sender as ToolStripDropDownButton;
            RestrictToOneCheck(tsddb, e);
        }

        private void tsddb_MouseHover(object sender, System.EventArgs e)
        {
            ToolStripDropDownButton tsddb = sender as ToolStripDropDownButton;

            foreach (ToolStripItem tsi in tsDest.Items)
            {
                if (tsi is ToolStripDropDownButton)
                {
                    ToolStripDropDownButton tsddb2 = tsi as ToolStripDropDownButton;
                    if (tsddb.Text != tsddb2.Text)
                    {
                        tsddb.DropDown.Close();
                    }
                }
            }

            tsddb.ShowDropDown();
            tsddb.DropDown.AutoClose = false;
        }

        private void tsddbClipboardContent_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            RestrictToOneCheck(tsddbClipboardContent, e);
            EnableDisableDestControls();
        }

        private void tsddbLinkFormat_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            RestrictToOneCheck(tsddbLinkFormat, e);
        }

        private void tsddDestLinks_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            RestrictToOneCheck(tsddbDestLink, e);
        }
    }
}