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
        string Filename { get; set; }
        string WavFilename { get; set; }
        NAudio.Wave.DirectSoundOut SoundOutput { get; set; }

        private WaveOut Output { get; set; }

        public Form1()
        {
            InitializeComponent();

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
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
                Filename = openFileDialog1.FileName;

                MessagingToolkit.QRCode.Codec.QRCodeDecoder decoder = new MessagingToolkit.QRCode.Codec.QRCodeDecoder();
                MessagingToolkit.QRCode.Codec.Data.QRCodeBitmapImage image = new MessagingToolkit.QRCode.Codec.Data.QRCodeBitmapImage(new Bitmap(Filename));

                try
                {
                    byte[] data = (byte[])(Array)decoder.DecodeBytes(image);
                    pictureBox1.Image = new Bitmap(Filename);

                    WavFilename = Filename;
                    WavFilename = Path.ChangeExtension(WavFilename, "wav");

                    const int numchannels = 1;
                    const int samplerate = 8000;
                    WaveFormat format = new WaveFormat(samplerate, sizeof(byte) * 8, numchannels);
                    using (WaveFileWriter writer = new WaveFileWriter(WavFilename, format))
                    {
                        writer.Write(data, 0, data.Length);
                    }

                    button1.Enabled = true;
                    button2.Enabled = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed decoding!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Output != null)
            {
                button3_Click(sender, e);
            }

            if (Output == null)
            {
                WaveFileReader reader = new WaveFileReader(WavFilename);
                LoopStream loop = new LoopStream(reader);
                loop.EnableLooping = false;
                Output = new WaveOut();
                Output.Init(loop);
                Output.Play();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Output != null)
            {
                button3_Click(sender, e);
            }

            if (Output == null)
            {
                WaveFileReader reader = new WaveFileReader(WavFilename);
                LoopStream loop = new LoopStream(reader);
                Output = new WaveOut();
                Output.Init(loop);
                Output.Play();

                button3.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Output.Stop();
            Output.Dispose();
            Output = null;

            button3.Enabled = false;
        }
    }

    /// <summary>
    /// Stream for looping playback
    /// </summary>
    public class LoopStream : WaveStream
    {
        WaveStream sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length
        {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRequired = (int)Math.Min(count - totalBytesRead, Length - Position);
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, bytesRequired);
                if (bytesRead == 0 || sourceStream.Position > sourceStream.Length)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
