﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;

namespace ZSS.Global
{
    public static class RegistryMgr
    {
        public static bool CheckStartWithWindows()
        {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if (regkey != null && (string)regkey.GetValue(Application.ProductName,
                "null", RegistryValueOptions.None) != "null")
            {
                Registry.CurrentUser.Flush();
                return true;
            }
            Registry.CurrentUser.Flush();
            return false;
        }

        public static void SetStartWithWindows(bool startWin)
        {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (regkey != null)
            {
                if (startWin)
                {

                    regkey.SetValue(Application.ProductName, Application.ExecutablePath, RegistryValueKind.String);
                }
                else
                {
                    regkey.DeleteValue(Application.ProductName, false);
                }

                Registry.CurrentUser.Flush();
            }
        }

        public static void FindImageEditors()
        {
            //Adobe Photoshop - HKEY_CLASSES_ROOT\Applications\Photoshop.exe\shell\open\command
            SoftwareCheck(@"Applications\Photoshop.exe\shell\open\command", "Adobe Photoshop");
            //Irfan View - HKEY_CLASSES_ROOT\Applications\i_view32.exe\shell\open\command - HKEY_LOCAL_MACHINE\SOFTWARE\Classes\IrfanView\shell\open\command
            SoftwareCheck(@"Applications\i_view32.exe\shell\open\command", "Irfan View");
            //Paint.NET - HKEY_CLASSES_ROOT\Paint.NET.1\shell\open\command
            SoftwareCheck(@"Paint.NET.1\shell\open\command", "Paint.NET");
        }

        public static bool SoftwareCheck(string regPath, string sName)
        {
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(regPath, false);
            if (regKey != null) //If registry notFound
            {
                string stringReg = regKey.GetValue("").ToString();
                if (!string.IsNullOrEmpty(stringReg)) //If registry value not empty
                {
                    string filePath = stringReg.Substring(1, stringReg.LastIndexOf("%") - 4);
                    if (File.Exists(filePath)) //If notFound path exist
                    {
                        if (!Program.conf.SoftwareExist(sName)) //If not added to Software list before
                        {
                            Program.conf.ImageSoftwareList.Add(new Software(sName, filePath, false));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

    }
}