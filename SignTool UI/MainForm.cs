﻿using System;
using System.IO;
using System.Windows.Forms;

namespace SignToolUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonBrowseSignTool_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Executables|*.exe";
            openFileDialog.FileName = "SignTool.exe";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSignToolPath.Text = openFileDialog.FileName;
            }
        }
        
        private void buttonBrowseCertificate_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Certificates|*.pfx";
            openFileDialog.FileName = string.Empty;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxCertificatePath.Text = openFileDialog.FileName;
            }
        }

        private void checkBoxShowPwd_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = !checkBoxShowPwd.Checked;
        }

        private void checkedListBoxFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                int selectedIndex = checkedListBoxFiles.SelectedIndex;
                checkedListBoxFiles.Items.Remove(checkedListBoxFiles.SelectedItem);

                if (selectedIndex >= 0)
                {
                    checkedListBoxFiles.SelectedIndex = selectedIndex == 0 ? -1 : selectedIndex - 1;
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            checkedListBoxFiles.Items.Clear();
        }

        private void buttonAddFiles_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Assemblies|*.dll";
            openFileDialog.FileName = string.Empty;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    checkedListBoxFiles.Items.Add(fileName, true);
                }
            }
        }

        private void buttonAddDirectory_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.dll", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        checkedListBoxFiles.Items.Add(file);
                    }
                }
                catch {}
            }
        }

        private void checkedListBoxFiles_ControlChanged(object sender, ControlEventArgs e)
        {
            progressBarStatus.Maximum = checkedListBoxFiles.CheckedItems.Count;
        }

        private void splitButtonSign_Click(object sender, EventArgs e)
        {
            ToggleDisabledForm(true);
            textBoxOutput.Clear();
            panelProgress.Visible = true;
            progressBarStatus.Maximum = checkedListBoxFiles.CheckedItems.Count;
            progressBarStatus.Value = 0;

            Signer signer = new Signer(textBoxSignToolPath.Text, textBoxCertificatePath.Text, textBoxPassword.Text, textBoxTimestampURL.Text);
            signer.Verbose = menuItemSignVerbose.Checked;
            signer.Debug = menuItemSignDebug.Checked;

            signer.OnSignToolOutput += (string message) =>
            {
                if (string.IsNullOrEmpty(message)) return;
                
                if (textBoxOutput.InvokeRequired) // safe cross-thread call
                {
                    textBoxOutput.Invoke(new Action<string>(textBoxOutput.AppendText), message + Environment.NewLine);
                }
                else
                {
                    textBoxOutput.AppendText(message);
                }
            };

            foreach (var file in checkedListBoxFiles.CheckedItems)
            {
                string filename = Path.GetFileName(file.ToString());
                textBoxOutput.AppendText(string.Format("Signing file {0}...{1}", filename, Environment.NewLine));
                signer.Sign(file.ToString());
                progressBarStatus.PerformStep();
            }

            ToggleDisabledForm(false);
        }

        private void contextMenuItemSignOption_Click(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
        }

        private void ToggleDisabledForm(bool disable)
        {
            groupBoxDetails.Enabled = !disable;
            groupBoxFiles.Enabled = !disable;
            splitButtonSign.Enabled = !disable;
        }
    }
}