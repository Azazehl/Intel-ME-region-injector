﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MEInject
{
    public partial class Form1 : Form
    {
        private static readonly byte[] Mask = { 0x24, 0x46, 0x50, 0x54 };
        private byte[] BIOSfile;
        private byte[] MEfile;
        private int BIOS_ME_offset;
        private static byte _diff = 0x10;
        public Form1()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
            Log("Ready!");
        }



        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Multiselect = false };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            BIOSfile = File.ReadAllBytes(ofd.FileName);
            var offset = Find(BIOSfile, Mask);
            //MessageBox.Show(offset1.ToString("X") + "<-mask1 mask2->" + (offset2 - 0x10).ToString("X"));
            if (offset == -1)
            {
                Log("Can't find ME region in file");
                return;
            }
            offset -= _diff;
            if (offset == 0)
            {
                Log("Please, open BIOS, not ME");
                return;
            }
            BIOS_ME_offset = offset;
            MEoffsetLabel.Text = "ME offset in BIOS: 0x" + BIOS_ME_offset.ToString("X8");
            Log("BIOS read successful!");
            BIOStextbox.Text = "First 0x10 bytes ME in BIOS:\n";
            for (int i = 0; i < 32; i++)
            {
                if (i % 8 == 0) BIOStextbox.Text += "\n";
                if (i % 4 == 0) BIOStextbox.Text += " ";

                BIOStextbox.Text += BIOSfile[i + BIOS_ME_offset].ToString("X2") + " ";
            }
        }

        static int Find(IList<byte> array, IList<byte> mask)
        {
            try
            {
                for (var i = 0; i < array.Count; i++)
                {
                    if (array[i] != mask[0]) continue;
                    for (var j = 1; j < mask.Count; j++)
                    {
                        if (i + j >= array.Count) return -1;
                        if (array[i + j] == mask[j] & j == mask.Count - 1) return i;
                        if (array[i + j] == mask[j]) continue;
                        i += j;
                        break;
                    }
                }
                return -1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(@"C:\Users\Nick\Google Диск\BIOS\Wistron");
            //var files = Directory.GetFiles(@"C:\Windows\System32");

            foreach (var file in files)
            {
                try
                {
                    Log(file + " - " + (Find(File.ReadAllBytes(file), Mask) - _diff).ToString("X"));
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }

            }


        }

        private void OpenMEButton_Click(object sender, EventArgs e)
        {
            MEtextbox.Text = "First 0x10 bytes:\n";
            var ofd = new OpenFileDialog { Multiselect = false };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            MEfile = File.ReadAllBytes(ofd.FileName);
            if (Find(MEfile, Mask) - _diff != 0)
            {
                Log("It's not valid ME file");
                return;
            }
            for (int i = 0; i < 32; i++)
            {
                if (i % 8 == 0) MEtextbox.Text += "\n";
                if (i % 4 == 0) MEtextbox.Text += " ";
                MEtextbox.Text += MEfile[i].ToString("X2") + " ";
            }
            Log("ME read successful!");
        }

        void Log(string message)
        {
            DebugTextBox.Text += DateTime.Now + " - " + message + "\n";
        }
    }
}