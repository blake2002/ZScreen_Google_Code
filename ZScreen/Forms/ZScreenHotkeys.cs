﻿using System;
using System.Windows.Forms;
using HelpersLib;
using ZScreenLib;

namespace ZScreenGUI
{
    public partial class ZScreen : HotkeyForm
    {
        public static int mHKSelectedRow = -1;

        private void UpdateHotkeys(bool resetKeys = false)
        {
            foreach (HotkeyTask hk in Enum.GetValues(typeof(HotkeyTask)))
            {
                UpdateHotkey(hk, resetKeys);
            }
        }

        private void UpdateHotkey(HotkeyTask hotkeyEnum, bool resetKeys = false)
        {
            object userHotKey;

            if (resetKeys)
            {
                userHotKey = Engine.conf.GetFieldValue("DefaultHotkey" + hotkeyEnum.ToString());
            }
            else
            {
                userHotKey = Engine.conf.GetFieldValue("Hotkey" + hotkeyEnum.ToString());
            }

            if (userHotKey != null && userHotKey is Keys)
            {
                Keys hotkey = (Keys)userHotKey;

                HotkeyInfo oldHotkeyInfo = GetHotkeyInfoFromTag(hotkeyEnum);
                UnregisterHotkey(oldHotkeyInfo);

                HotkeyInfo newHotkeyInfo = null;

                switch (hotkeyEnum)
                {
                    case HotkeyTask.ActiveWindow:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureActiveWindow);
                        break;
                    case HotkeyTask.CropShot:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureRectRegion);
                        break;
                    case HotkeyTask.EntireScreen:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureEntireScreen);
                        break;
                    case HotkeyTask.ClipboardUpload:
                        newHotkeyInfo = RegisterHotkey(hotkey, UploadUsingClipboardOrGoogleTranslate);
                        break;
                    case HotkeyTask.SelectedWindow:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureSelectedWindow);
                        break;
                    case HotkeyTask.LastCropShot:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureRectRegionLast);
                        break;
                    case HotkeyTask.AutoCapture:
                        newHotkeyInfo = RegisterHotkey(hotkey, ShowAutoCapture);
                        break;
                    case HotkeyTask.DropWindow:
                        newHotkeyInfo = RegisterHotkey(hotkey, ShowDropWindow);
                        break;
                    case HotkeyTask.FreehandCropShot:
                        newHotkeyInfo = RegisterHotkey(hotkey, CaptureFreeHandRegion);
                        break;
                    case HotkeyTask.LanguageTranslator:
                        newHotkeyInfo = RegisterHotkey(hotkey, StartWorkerTranslator);
                        break;
                    case HotkeyTask.ScreenColorPicker:
                        newHotkeyInfo = RegisterHotkey(hotkey, ScreenColorPicker);
                        break;
                    case HotkeyTask.TwitterClient:
                        newHotkeyInfo = RegisterHotkey(hotkey, Adapter.ShowTwitterClient);
                        break;
                }

                if (newHotkeyInfo != null)
                {
                    newHotkeyInfo.Tag = hotkeyEnum;
                }
            }
        }

        public void UpdateHotkeysDGV()
        {
            UpdateHotkeysDGV(false);
        }

        private void UpdateHotkeysDGV(bool resetKeys)
        {
            dgvHotkeys.Rows.Clear();

            foreach (HotkeyTask hk in Enum.GetValues(typeof(HotkeyTask)))
            {
                AddHotkey(hk, resetKeys);
            }

            dgvHotkeys.Refresh();
        }

        private void AddHotkey(HotkeyTask hotkeyEnum, bool resetKeys)
        {
            object userHotKey = Engine.conf.GetFieldValue("DefaultHotkey" + hotkeyEnum.ToString());

            if (!resetKeys)
            {
                userHotKey = Engine.conf.GetFieldValue("Hotkey" + hotkeyEnum.ToString());
            }

            if (userHotKey != null && userHotKey.GetType() == typeof(Keys))
            {
                dgvHotkeys.Rows.Add(hotkeyEnum.GetDescription(), ((Keys)userHotKey).ToSpecialString(), ((Keys)userHotKey).ToSpecialString());
                dgvHotkeys.Rows[dgvHotkeys.Rows.Count - 1].Tag = hotkeyEnum;
            }
        }

        /// <summary>
        /// Sets the Hotkey of the Active Cell of the DataGridView
        /// </summary>
        /// <param name="key"></param>
        public void SetHotkey(Keys key)
        {
            SetHotkey(mHKSelectedRow, key);
        }

        public void SetHotkey(int row, Keys key)
        {
            dgvHotkeys.Rows[row].Cells[1].Value = key.ToSpecialString();
            lblHotkeyStatus.Text = dgvHotkeys.Rows[row].Cells[0].Value + " Hotkey set to: " + key.ToSpecialString() + ". Press enter when done setting all desired Hotkeys.";
            HotkeyTask hk = (HotkeyTask)dgvHotkeys.Rows[row].Tag;
            SaveHotkey(hk, key);
            UpdateHotkey(hk);
        }

        public static bool SaveHotkey(HotkeyTask hk, Keys key)
        {
            return Engine.conf.SetFieldValue("Hotkey" + hk.ToString(), key);
        }

        public void ResetHotkeys()
        {
            int index = 0;
            foreach (HotkeyTask hk in Enum.GetValues(typeof(HotkeyTask)))
            {
                object dfltHotkey = Engine.conf.GetFieldValue("DefaultHotkey" + hk.ToString().Replace(" ", string.Empty));
                SetHotkey(index++, (Keys)dfltHotkey);
            }
            UpdateHotkeysDGV(true);
        }

        #region Hotkeys DataGridView Events

        private void dgvHotkeys_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks
            if (e.RowIndex < 0 || e.ColumnIndex != dgvHotkeys.Columns[1].Index)
            {
                return;
            }

            mSetHotkeys = true;
            mHKSelectedRow = e.RowIndex;

            lblHotkeyStatus.Text = "Press the keys you would like to use... Press enter when done setting all desired Hotkeys.";

            dgvHotkeys.Rows[e.RowIndex].Cells[1].Value = GetSelectedHotkeySpecialString() + " <Set Keys>";
        }

        private void dgvHotkeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (mSetHotkeys)
            {
                if (e.KeyValue == (int)Keys.Up || e.KeyValue == (int)Keys.Down || e.KeyValue == (int)Keys.Left || e.KeyValue == (int)Keys.Right)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        private void dgvHotkeys_Leave(object sender, EventArgs e)
        {
            QuitSettingHotkeys();
        }

        private void dgvHotkeys_MouseLeave(object sender, EventArgs e)
        {
            QuitSettingHotkeys();
        }

        #endregion Hotkeys DataGridView Events

        #region Hotkey Helpers

        public void CheckHotkeys(object sender, KeyEventArgs e)
        {
            if (mSetHotkeys)
            {
                if (e.KeyData == Keys.Enter)
                {
                    QuitSettingHotkeys();
                }
                else if (e.KeyData == Keys.Escape)
                {
                    SetHotkey(Keys.None);
                }
                else
                {
                    SetHotkey(e.KeyData);
                }
            }
        }

        public string GetSelectedHotkeyName()
        {
            return dgvHotkeys.Rows[mHKSelectedRow].Tag.ToString();
        }

        public string GetSelectedHotkeySpecialString()
        {
            object obj = Engine.conf.GetFieldValue("Hotkey" + GetSelectedHotkeyName().Replace(" ", string.Empty));
            if (obj != null && obj.GetType() == typeof(Keys))
            {
                return ((Keys)obj).ToSpecialString();
            }

            return "Error getting hotkey";
        }

        public void QuitSettingHotkeys()
        {
            if (mSetHotkeys)
            {
                mSetHotkeys = false;

                if (mHKSelectedRow > -1)
                {
                    this.lblHotkeyStatus.Text = GetSelectedHotkeyName() + " Hotkey Updated.";
                    //reset hotkey text from <set keys> back to
                    this.dgvHotkeys.Rows[mHKSelectedRow].Cells[1].Value = GetSelectedHotkeySpecialString();
                }
                mHKSelectedRow = -1;
            }
        }

        #endregion Hotkey Helpers
    }
}