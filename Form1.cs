using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MessagingToolkit.QRCode;
using System.IO;
using NAudio.Wave;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string WavFilename { get; set; }
        QRSound Sound { get; set; }

        public Form1()
        {
            InitializeComponent();

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;

            saveAudioToolStripMenuItem.Enabled = false;

            Sound = new QRSound();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            // Show the dialog and get result.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog1.Filter = "png files (*.png)|*.png";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                try
                {
                    Sound.OpenImage(openFileDialog1.FileName);
                    pictureBox1.Image = new Bitmap(openFileDialog1.FileName);

                    WavFilename = openFileDialog1.FileName;
                    WavFilename = Path.ChangeExtension(WavFilename, "wav");

                    button1.Enabled = true;
                    button2.Enabled = true;
                    saveAudioToolStripMenuItem.Enabled = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed loading QR image!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Sound.IsPlaying())
            {
                Sound.Stop();
            }

            Sound.PlaySound(false);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Sound.IsPlaying())
            {
                Sound.Stop();
            }

            Sound.PlaySound(true);
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Sound.IsPlaying())
            {
                Sound.Stop();
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = false;
            }
        }

        private void saveAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Sound.FileLoaded())
            {
                // Show the dialog and get result.
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(WavFilename);
                saveFileDialog1.FileName = Path.GetFileName(WavFilename);
                saveFileDialog1.Filter = "wav files (*.wav)|*.wav";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;

                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    WavFilename = saveFileDialog1.FileName;
                    Sound.SaveAudio(WavFilename);
                }
            }
        }
    }
}
