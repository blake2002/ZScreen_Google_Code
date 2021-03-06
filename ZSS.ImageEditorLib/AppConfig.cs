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

#region Source code: Greenshot (GPL)

/*
    This file originated from the Greenshot project (GPL). It may or may not have been modified.
    Please do not contact Greenshot about errors with this code. Instead contact the creators of this program.
    URL: http://greenshot.sourceforge.net/
    Code (CVS): http://greenshot.cvs.sourceforge.net/viewvc/greenshot/
*/

#endregion Source code: Greenshot (GPL)

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Greenshot.Drawing;

namespace Greenshot.Configuration
{
    public enum ScreenshotDestinations
    {
        Editor = 1,
        FileDefault = 2,
        FileWithDialog = 4,
        Clipboard = 8,
        Printer = 16
    }

    /// <summary>
    /// AppConfig is used for loading and saving the configuration. All public fields
    /// in this class are serialized with the BinaryFormatter and then saved to the
    /// config file. After loading the values from file, SetDefaults iterates over
    /// all public fields an sets fields set to null to the default value.
    /// </summary>
    [Serializable]
    public class AppConfig
    {
        public static string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Greenshot\config.dat");
        private static AppConfig instance = null;

        // the configuration part - all public vars are stored in the config file
        // don't use "null" and "0" as default value!

        #region Output config

        public ScreenshotDestinations Output_Destinations = ScreenshotDestinations.Editor;

        public string Output_File_Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string Output_File_FilenamePattern = "screenshot_%YYYY%-%MM%-%DD%_%hh%-%mm%-%ss%";
        public string Output_File_Format = ImageFormat.Png.ToString();
        public bool? Output_File_CopyPathToClipboard = false;
        public int Output_File_JpegQuality = 80;
        public bool? Output_File_PromptJpegQuality = false;
        public int Output_File_IncrementingNumber = 1;

        public string Output_FileAs_Fullpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dummy.png");

        public bool? Output_Print_PromptOptions = true;
        public bool? Output_Print_AllowRotate = true;
        public bool? Output_Print_AllowEnlarge = true;
        public bool? Output_Print_AllowShrink = true;
        public bool? Output_Print_Center = true;

        #endregion Output config

        #region Editor config

        public Size? Editor_WindowSize = new Size(800, 550);
        public Color Editor_ForeColor = Color.FromArgb(255, 255, 0, 0);
        public Color Editor_BackColor = Color.Transparent;
        public Color Editor_GradientColor = Color.Transparent;
        public string Editor_GradientType = "None";
        public int Editor_Thickness = 1;
        public Color[] Editor_RecentColors = new Color[12];
        public Font Editor_Font = null;
        public ArrowHeads Editor_ArrowHeads = ArrowHeads.End;

        #endregion Editor config

        /// <summary>
        /// a private constructor because this is a singleton
        /// </summary>
        private AppConfig()
        {
        }

        /// <summary>
        /// get an instance of AppConfig
        /// </summary>
        /// <returns></returns>
        public static AppConfig GetInstance()
        {
            if (instance == null)
            {
                instance = Load();
            }
            return instance;
        }

        /// <summary>
        /// loads the configuration from the config file
        /// </summary>
        /// <returns>an instance of AppConfig with all values set from the config file</returns>
        private static AppConfig Load()
        {
            AppConfig conf;
            CheckConfigFile();
            Stream s = null;
            try
            {
                s = File.Open(ConfigPath, FileMode.Open);
                BinaryFormatter b = new BinaryFormatter();
                conf = (AppConfig)b.Deserialize(s);
                s.Close();
                conf.SetDefaults();
                return conf;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (s != null)
                {
                    s.Close();
                }
                AppConfig config = new AppConfig();
                config.Save();
                return config;
            }
        }

        /// <summary>
        /// Checks for the existence of a configuration file.
        /// First in greenshot's Applicationdata folder (where it is stored since 0.6),
        /// then (if it cannot be found there) in greenshot's program directory (where older
        /// versions might have stored it).
        /// If the latter is the case, the file is moved to the new location, so that a user does not lose
        /// their configuration after upgrading.
        /// If there is no file in both locations, a virgin config file is created.
        /// </summary>
        private static void CheckConfigFile()
        {
            if (!File.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath.Substring(0, ConfigPath.LastIndexOf(@"\")));
                new AppConfig().Save();
            }
        }

        /// <summary>
        /// saves the configuration values to the config file
        /// </summary>
        public void Save()
        {
            try
            {
                Stream s = File.Open(ConfigPath, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, this);
                s.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Image Editor: " + ex.Message);
            }
        }

        /// <summary>
        /// when new fields are added to this class, they are instanced
        /// with null by default. this method iterates over all public
        /// fields and uses reflection to set them to the proper default value.
        /// </summary>
        public void SetDefaults()
        {
            Type type = this.GetType();
            FieldInfo[] fieldInfos = type.GetFields();
            foreach (FieldInfo fi in fieldInfos)
            {
                object o = fi.GetValue(this);
                int i;
                if (o == null || (int.TryParse(o.ToString(), out i) && i == 0))
                {
                    // found field with value null. setting to default.
                    AppConfig tmpConf = new AppConfig();
                    Type tmpType = tmpConf.GetType();
                    FieldInfo defaultField = tmpType.GetField(fi.Name);
                    fi.SetValue(this, defaultField.GetValue(tmpConf));
                }
            }
        }
    }
}