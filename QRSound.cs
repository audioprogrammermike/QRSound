using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;
using System.Drawing;
using NAudio.Wave;
using System.IO;



namespace QRSound
{
    class QRSoundConverter
    {
        string WavFilename { get; set; }
        byte[] Data { get; set; }
        WaveOut Output { get; set; }

        public void OpenImage(string filepath)
        {
            QRCodeDecoder decoder = new QRCodeDecoder();
            QRCodeBitmapImage image = new QRCodeBitmapImage(new Bitmap(filepath));
            Data = (byte[])(Array)decoder.DecodeBytes(image);
        }

        public void OpenAudio(string filepath)
        {
            WaveFileReader reader = new WaveFileReader(filepath);

            WaveFormatConversionStream rawstream = new WaveFormatConversionStream(new WaveFormat(8000, sizeof(byte) * 8, 1), reader);
            Data = new byte[rawstream.Length];
            rawstream.Read(Data, 0, (int)rawstream.Length);
        }

        public Image GetQRImage()
        {
            QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
            encoder.QRCodeVersion = 0; //http://platform.twit88.com/news/60

            byte[] towrite = Data;
            Array.Resize(ref towrite, 1024); //Not sure how the max is defined in the library (1kB is sufficient)

            string asastring = System.Text.Encoding.UTF8.GetString(towrite);
            Image qrimage = encoder.Encode(asastring);
            return qrimage;
        }

        public void SaveAudio(string filepath)
        {
            if(FileLoaded())
            {
                const int numchannels = 1;
                const int samplerate = 8000;
                WaveFormat format = new WaveFormat(samplerate, sizeof(byte) * 8, numchannels);
                using (WaveFileWriter writer = new WaveFileWriter(filepath, format))
                {
                    writer.Write(Data, 0, Data.Length);
                }
            }
            else
            {
                throw new System.InvalidOperationException("No QR file loaded");
            }
        }

        public bool FileLoaded() { return Data != null; }

        public bool IsPlaying() { return Output != null; }

        public void PlaySound(bool loop)
        {
            if (!FileLoaded()) { throw new System.InvalidOperationException("No QR file loaded"); }
            if (IsPlaying()) { throw new System.InvalidOperationException("Already playing"); }

            MemoryStream memstream = new MemoryStream();
            memstream.Write(Data, 0, Data.Length);

            RawSourceWaveStream rawstream = new RawSourceWaveStream(memstream, new WaveFormat(8000, sizeof(byte) * 8, 1));
            rawstream.Position = 0;

            LoopStream loopstream = new LoopStream(rawstream);
            loopstream.EnableLooping = loop;
            Output = new WaveOut();
            Output.Init(loopstream);
            Output.Play();
        }

        public void Stop()
        {
            if (!IsPlaying()) { throw new System.InvalidOperationException("No audio active"); }

            Output.Stop();
            Output.Dispose();
            Output = null;
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
