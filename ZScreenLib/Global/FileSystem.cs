﻿#region License Information (GPL v2)

/*
    ZScreen - A program that allows you to upload screenshots in one keystroke.
    Copyright (C) 2008-2011 ZScreen Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v2)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HelpersLib;
using HelpersLib.GraphicsHelper;
using UploadersLib;

namespace ZScreenLib
{
    public static class FileSystem
    {
        /// <summary>
        /// Returns a list of file paths from a collection of files and directories
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static List<string> GetExplorerFileList(string[] paths)
        {
            StringCollection sc = new StringCollection();
            foreach (string p in paths)
            {
                sc.Add(p);
            }
            return GetExplorerFileList(sc);
        }

        public static List<string> GetExplorerFileList(StringCollection paths)
        {
            List<string> files = new List<string>();
            foreach (string fp in paths)
            {
                if (File.Exists(fp))
                {
                    if (Path.GetTempPath().StartsWith(Path.GetDirectoryName(fp)))
                    {
                        string temp = Path.Combine(Engine.ImagesDir, Path.GetFileName(fp));
                        File.Copy(fp, temp);
                        files.Add(temp);
                    }
                    else
                    {
                        files.Add(fp);
                    }
                }
                else if (Directory.Exists(fp))
                {
                    string[] dirFiles = Directory.GetFiles(fp, "*.*", SearchOption.AllDirectories);
                    foreach (string f in dirFiles)
                    {
                        files.Add(f);
                    }
                }
            }
            return files;
        }

        public static FileInfo WriteImage(string fp, Stream stream)
        {
            string destDir = Path.GetDirectoryName(fp);
            FileInfo fi = new FileInfo(FileSystem.GetUniqueFilePath(Engine.ConfigWorkflow, destDir, Path.GetFileName(fp)));

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            DialogResult result = DialogResult.OK;

            if (Engine.ConfigWorkflow.ConfigFileNaming.OverwriteFiles)
                result = DialogResult.OK;
            else if (File.Exists(fp))
            {
                if (MessageBox.Show(string.Format("{0} already exists. \nDo you want to overwrite the existing file?", fp),
                    Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    result = DialogResult.OK;
                }
                else
                {
                    result = DialogResult.Cancel;
                }
            }

            if (result == DialogResult.OK)
            {
                using (stream)
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(fp);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        stream.WriteToFile(fp);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.WriteException(ex, "Error while saving image");
                    }
                }
            }

            return fi;
        }

        public static string GetTextFromFile(string filePath)
        {
            string s = string.Empty;
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    s = sr.ReadToEnd();
                }
            }
            return s;
        }

        public static string GetImagesDir()
        {
            return Directory.Exists(Engine.ImagesDir) ? Engine.ImagesDir : Engine.RootImagesDir;
        }

        public static string GetTempFilePath(string fileName)
        {
            string dir = Engine.CacheDir;
            if (string.IsNullOrEmpty(dir))
                dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, fileName);
        }

        public static bool WriteText(string fp, string myText)
        {
            bool succ = false;
            try
            {
                File.WriteAllText(fp, myText);
                succ = true;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex, "WriteText");
            }
            return succ;
        }

        public static bool ExportText(string name, string filePath)
        {
            bool succ = true;
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine(GetText(name));
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex, "Error while exporting text");
                succ = false;
            }

            return succ;
        }

        public static string GetText(string name)
        {
            string text = "";
            try
            {
                System.Reflection.Assembly oAsm = System.Reflection.Assembly.GetExecutingAssembly();

                string fn = "";
                foreach (string n in oAsm.GetManifestResourceNames())
                {
                    if (n.Contains(name))
                    {
                        fn = n;
                        break;
                    }
                }
                Stream oStrm = oAsm.GetManifestResourceStream(fn);
                StreamReader oRdr = new StreamReader(oStrm);
                text = oRdr.ReadToEnd();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex, "Error while getting text from resource");
            }

            return text;
        }

        public static Image ImageFromFile(string fp)
        {
            return HelpersLib.GraphicsHelper.Core.GetImageSafely(fp);
        }

        /// <summary>
        /// Function to check if file is a valid Text file by checking its extension
        /// </summary>
        /// <param name="fp"></param>
        /// <returns></returns>
        public static bool IsValidTextFile(string fp)
        {
            if (!string.IsNullOrEmpty(fp) && File.Exists(fp))
            {
                foreach (string s in Engine.zTextFileTypes)
                {
                    if (Path.GetExtension(fp).ToLower().EndsWith(s)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Function to check if file is a valid Text file by checking its extension
        /// </summary>
        /// <param name="fp"></param>
        /// <returns></returns>
        public static bool IsValidWebpageFile(string fp)
        {
            if (!string.IsNullOrEmpty(fp) && File.Exists(fp))
            {
                foreach (string s in Engine.zWebpageFileTypes)
                {
                    if (Path.GetExtension(fp).ToLower().EndsWith(s)) return true;
                }
            }

            return false;
        }

        public static string GetUniqueFileName(Workflow workflow, string fileNameOrPath)
        {
            if (!workflow.ConfigFileNaming.OverwriteFiles)
            {
                string fn = Path.GetFileNameWithoutExtension(fileNameOrPath);
                string ext = Path.GetExtension(fileNameOrPath);

                if (fn.Length > workflow.ConfigFileNaming.MaxNameLength - ext.Length)
                {
                    string nfn = fn.Substring(0, workflow.ConfigFileNaming.MaxNameLength - ext.Length);
                    fileNameOrPath = nfn + ext;
                }

                string fp, fileExt, pattern = @"(^.+\()(\d+)(\)\.\w+$)";
                int num = 1;
                GroupCollection groups = Regex.Match(fileNameOrPath, pattern).Groups;
                if (string.IsNullOrEmpty(groups[2].Value))
                {
                    fp = fileNameOrPath.Substring(0, fileNameOrPath.LastIndexOf('.')) + "(";
                    fileExt = ")" + fileNameOrPath.Remove(0, fileNameOrPath.LastIndexOf('.'));
                }
                else
                {
                    fp = groups[1].Value;
                    fileExt = groups[3].Value;
                    num = Convert.ToInt32(groups[2].Value);
                }
                while (File.Exists(fileNameOrPath))
                {
                    fileNameOrPath = fp + ++num + fileExt;
                }
            }

            return fileNameOrPath;
        }

        /// <summary>
        /// If file exist then adding number end of file name.
        /// Example: directory/fileName(2).exe
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueFilePath(Workflow workflow, string dir, string fileName)
        {
            return Path.Combine(Engine.IsPortable ?
                Path.Combine(Application.StartupPath, dir) : dir,
                GetUniqueFileName(workflow, fileName));
        }

        public static string GetFileSize(long bytes)
        {
            if (bytes >= 1073741824)
            {
                return String.Format("{0:##.##} GiB", (decimal)bytes / 1073741824);
            }
            if (bytes >= 1048576)
            {
                return String.Format("{0:##.##} MiB", (decimal)bytes / 1048576);
            }
            if (bytes >= 1024)
            {
                return String.Format("{0:##.##} KiB", (decimal)bytes / 1024);
            }
            if (bytes > 0 & bytes < 1024)
            {
                return String.Format("{0:##.##} Bytes", bytes);
            }
            return "0 Bytes";
        }

        public static void BackupSettings()
        {
            string dirbackup = Path.Combine(Engine.DocsAppFolder, "Archive");

            if (Engine.ConfigUploaders.FTPAccountList2.Count > 0)
            {
                string fpftp = Path.Combine(dirbackup, string.Format("{0}-{1}-ftp-accounts.{2}", Application.ProductName, DateTime.Now.ToString("yyyyMM"), Engine.EXT_FTP_ACCOUNTS));
                if (!File.Exists(fpftp))
                {
                    FTPAccountManager fam = new FTPAccountManager(Engine.ConfigUploaders.FTPAccountList2);
                    fam.Save(fpftp);
                }
            }

            string fpWorkflowConfig = Path.Combine(dirbackup, string.Format("{0}-{1}-{2}", Application.ProductName, DateTime.Now.ToString("yyyyMM"), Engine.WorkflowConfigFileName));
            if (!File.Exists(fpWorkflowConfig))
            {
                Engine.ConfigWorkflow.Write(fpWorkflowConfig);
            }

            string fpoutputsconfig = Path.Combine(dirbackup, string.Format("{0}-{1}-{2}", Application.ProductName, DateTime.Now.ToString("yyyyMM"), Engine.UploadersConfigFileName));
            if (!File.Exists(fpoutputsconfig))
            {
                Engine.ConfigUploaders.Save(fpoutputsconfig);
            }

            if (Engine.ConfigUI != null)
            {
                string fp = Path.Combine(dirbackup, string.Format("{0}-{1}-Settings.xml", Application.ProductName, DateTime.Now.ToString("yyyyMM")));
                if (!File.Exists(fp))
                {
                    Engine.ConfigUI.Write(fp);
                }
            }
        }

        /// <summary>
        /// Function to move a directory with overwriting existing files
        /// </summary>
        /// <param name="dirOld"></param>
        /// <param name="dirNew"></param>
        public static void MoveDirectory(string dirOld, string dirNew)
        {
            if (Directory.Exists(dirOld) && dirOld != dirNew)
            {
                if (!Directory.Exists(dirNew)) Directory.CreateDirectory(dirNew);

                if (MessageBox.Show("Would you like to move old Root folder content to the new location?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(dirOld, dirNew, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Function to validate a URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsValidLink(string url)
        {
            return !string.IsNullOrEmpty(url) && !url.StartsWith("file://") && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static bool ManageImageFolders(string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                string[] images = Directory.GetFiles(path);

                List<string> imagesList = new List<string>();

                foreach (string image in images)
                {
                    foreach (string s in Engine.zImageFileTypes)
                    {
                        if (Path.HasExtension(image) && Path.GetExtension(image.ToLower()) == "." + s)
                        {
                            imagesList.Add(image);
                            break;
                        }
                    }
                }

                DebugHelper.WriteLine(string.Format("Found {0} images to move to sub-folders", imagesList.Count));

                if (imagesList.Count > 0)
                {
                    if (MessageBox.Show(string.Format("{0} files found in {1}\nPlease wait until all the files are moved...",
                        imagesList.Count, path), Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                    {
                        return false;
                    }

                    DateTime time;
                    string newFolderPath;
                    string movePath;

                    foreach (string image in imagesList)
                    {
                        if (File.Exists(image))
                        {
                            time = File.GetCreationTime(image);
                            newFolderPath = new NameParser(NameParserType.SaveFolder) { CustomDate = time }.Convert(Engine.ConfigWorkflow.SaveFolderPattern);
                            newFolderPath = Path.Combine(path, newFolderPath);

                            if (!Directory.Exists(newFolderPath))
                            {
                                Directory.CreateDirectory(newFolderPath);
                            }

                            movePath = FileSystem.GetUniqueFileName(Engine.ConfigWorkflow, Path.Combine(newFolderPath, Path.GetFileName(image)));
                            File.Move(image, movePath);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public static string GetBrowserFriendlyUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = Regex.Replace(url, " ", "%20");
            }
            return url;
        }
    }
}