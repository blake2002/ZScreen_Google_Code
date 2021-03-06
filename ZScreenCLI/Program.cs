﻿using System;
using System.Collections.Generic;
using System.IO;
using HelpersLib;
using NDesk.Options;
using UploadersLib;
using UploadersLib.HelperClasses;
using ZScreenLib;

namespace ZScreenCLI
{
    public static class Program
    {
        private static string ApplicationName = "zscreencli.exe";

        private static bool bVerbose = false;
        private static bool bShowHelp = false;
        private static bool bFileUpload = false;
        private static bool bClipboardUpload = false;
        private static bool bCropShot = false;
        private static bool bSelectedWindow = false;
        private static bool bScreen = false;

        private static List<int> listOutputTypes = new List<int>();
        private static int clipboardContent = -1;
        private static List<int> listImageHosts = new List<int>();
        private static List<int> listTextHosts = new List<int>();
        private static List<int> listFileHosts = new List<int>();
        private static List<string> listPaths = new List<string>();

        [STAThread]
        private static void Main(string[] args)
        {
            ProcessArgs(args);
        }

        #region CLI

        private static void ProcessArgs(string[] args)
        {
            var p = new OptionSet()
            {
                { "h|help", "Show this message and exit",
                   v => bShowHelp = v != null },
                { "v|verbose", "Debug output",
                    v => bVerbose = v != null },
                { "o|outputs=", "Outputs. This must be an integer.",
                    (int v) => listOutputTypes.Add(v) },
                { "i|hi=", "Image uploader type. This must be an integer.",
                    (int v) => listImageHosts.Add(v) },
                { "t|ht=", "Text uploader type. This must be an integer.",
                    (int v) => listTextHosts.Add(v) },
                { "f|hf=", "File uploader type. This must be an integer.",
                    (int v) => listFileHosts.Add(v) },
                { "cc=", "Clipboard content. This must be an integer.",
                    (int v) => clipboardContent = v },
                { "s|ws", "Capture selected window.",
                    v => bSelectedWindow = v != null },
                { "r|wc", "Capture rectangular region.",
                    v => bCropShot = v != null },
                { "d|wf", "Capture entire screen.",
                    v => bScreen = v != null },
                { "c|uc", "Upload clipboard content.",
                    v => bClipboardUpload = v != null },
                { "u|uf=", "File path to upload.",
                    v =>
                    {
                        if (File.Exists(v)) listPaths.Add (v);
                        else if (Directory.Exists(v)) listPaths.Add(v);
                        if (listPaths.Count>0) bFileUpload = true;
                    }
                },
            };

            if (args.Length == 0)
            {
                ShowHelp(p);
            }

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write(string.Format("{0}: ", ApplicationName));
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'ZScreenCLI.exe -help' for more information.");
                return;
            }

            if (bVerbose) Console.WriteLine(string.Format("Loading {0}", Engine.ConfigApp.WorkflowConfigPath));
            Engine.ConfigUploaders = UploadersConfig.Load(Engine.ConfigApp.UploadersConfigPath);
            Engine.ConfigWorkflow = Workflow.Read(Engine.ConfigApp.WorkflowConfigPath);

            if (bShowHelp)
            {
                ShowHelp(p);
                return;
            }

            if (bFileUpload && listPaths.Count > 0)
            {
                FileUpload();
            }

            if (bClipboardUpload)
            {
                ClipboardUpload();
            }

            if (bCropShot)
            {
                CaptureRectRegion(WorkerTask.JobLevel2.CaptureRectRegion);
            }

            if (bSelectedWindow)
            {
                CaptureRectRegion(WorkerTask.JobLevel2.CaptureSelectedWindow);
            }

            if (bScreen)
            {
                CaptureScreen();
            }
        }

        private static WorkerTask DefaultWorkerTask()
        {
            WorkerTask tempTask = new WorkerTask(new Workflow("CLI"));

            foreach (int o in listOutputTypes)
            {
                tempTask.WorkflowConfig.DestConfig.Outputs.Add((OutputEnum)o);
            }
            if (tempTask.WorkflowConfig.DestConfig.Outputs.Count == 0)
            {
                tempTask.WorkflowConfig.DestConfig.Outputs.Add(OutputEnum.Clipboard);
                tempTask.WorkflowConfig.DestConfig.Outputs.Add(OutputEnum.LocalDisk);
            }

            if (clipboardContent > 0)
            {
                tempTask.WorkflowConfig.DestConfig.TaskClipboardContent.Add((ClipboardContentEnum)clipboardContent);
            }
            else
            {
                tempTask.WorkflowConfig.DestConfig.TaskClipboardContent.Add(ClipboardContentEnum.Data);
            }

            foreach (int ut in listImageHosts)
            {
                if (bVerbose) Console.WriteLine(string.Format("Added {0}", ((ImageDestination)ut).GetDescription()));
                tempTask.WorkflowConfig.DestConfig.ImageUploaders.Add((ImageDestination)ut);
            }
            foreach (int ut in listFileHosts)
            {
                if (bVerbose) Console.WriteLine(string.Format("Added {0}", ((FileDestination)ut).GetDescription()));
                tempTask.WorkflowConfig.DestConfig.FileUploaders.Add((FileDestination)ut);
            }
            return tempTask;
        }

        private static void CaptureScreen()
        {
            WorkerTask esTask = DefaultWorkerTask();
            Console.WriteLine();
            Console.WriteLine("Capturing entire screen in 3 seconds.");
            Console.WriteLine("If you would like to minimize this window, then do it now.");
            Console.WriteLine();
            System.Threading.Thread.Sleep(3000);
            esTask.StartWork(WorkerTask.JobLevel2.CaptureEntireScreen);
            esTask.PublishData();
            PostPublishTask(esTask);
        }

        private static void CaptureRectRegion(WorkerTask.JobLevel2 job2)
        {
            WorkerTask csTask = DefaultWorkerTask();
            csTask.StartWork(job2);
            if (csTask.IsNotCanceled())
            {
                csTask.PublishData();
                Console.WriteLine(csTask.ToErrorString());
                PostPublishTask(csTask);
            }
        }

        private static void ClipboardUpload()
        {
            WorkerTask cbTask = DefaultWorkerTask();
            cbTask.LoadClipboardContent();
            cbTask.PublishData();

            PostPublishTask(cbTask);
        }

        private static void FileUpload()
        {
            List<string> listFiles = new List<string>();
            foreach (string fdp in listPaths)
            {
                if (File.Exists(fdp))
                {
                    listFiles.Add(fdp);
                }
                else if (Directory.Exists(fdp))
                {
                    listFiles.AddRange(Directory.GetFiles(fdp, "*.*", SearchOption.AllDirectories));
                }
            }
            foreach (string fp in listFiles)
            {
                WorkerTask fuTask = DefaultWorkerTask();
                fuTask.StartWork(WorkerTask.JobLevel2.UploadFromClipboard);
                fuTask.UpdateLocalFilePath(fp);
                fuTask.PublishData();

                PostPublishTask(fuTask);
            }
        }

        private static void PostPublishTask(WorkerTask task)
        {
            if (task.UploadResults.Count > 0)
            {
                foreach (UploadResult ur in task.UploadResults)
                {
                    Console.WriteLine(ur.URL);
                }
                UploadManager.ShowUploadResults(task, true);
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: zscreen.exe [OPTIONS]+ message");
            Console.WriteLine("Upload screenshots, text or files.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.Write(p.WriteOptionDescriptions().ToString());
            Console.WriteLine();
            Console.WriteLine("Outputs:\n");
            foreach (OutputEnum ut in Enum.GetValues(typeof(OutputEnum)))
            {
                Console.WriteLine(string.Format("{0}: {1}", (int)ut, ut.GetDescription()));
            }
            Console.WriteLine();
            Console.WriteLine("Image hosts:\n");
            foreach (ImageDestination ut in Enum.GetValues(typeof(ImageDestination)))
            {
                Console.WriteLine(string.Format("{0}: {1}", (int)ut, ut.GetDescription()));
            }
            Console.WriteLine();
            Console.WriteLine("Text hosts:\n");
            foreach (TextDestination ut in Enum.GetValues(typeof(TextDestination)))
            {
                Console.WriteLine(string.Format("{0}: {1}", (int)ut, ut.GetDescription()));
            }
            Console.WriteLine();
            Console.WriteLine("File hosts:\n");
            foreach (FileDestination ut in Enum.GetValues(typeof(FileDestination)))
            {
                Console.WriteLine(string.Format("{0}: {1}", (int)ut, ut.GetDescription()));
            }
        }

        #endregion CLI
    }
}