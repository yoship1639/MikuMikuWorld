using NAudio.Wave;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using AL = OpenTK.Audio.OpenAL.AL;

namespace MikuMikuWorld.Assets
{
    public class Sound : IAsset
    {
        public bool Loaded { get; private set; }
        public string Name { get; set; }

        public string Filepath { get; set; }
        public Stream Stream { get; set; }
        public string Format { get; set; }

        public byte[] Buffer { get; set; }
        public int Channels { get; set; }
        public int Bits { get; set; }
        public int SampleRate { get; set; }

        public int Source { get; private set; }
        public int ALBuffer { get; private set; }

        public Sound() { }
        public Sound(string filepath)
        {
            Filepath = filepath;
        }
        public Sound(byte[] wavBuffer)
        {
            Stream = new MemoryStream(wavBuffer);
        }
        public Sound(Stream stream, string format)
        {
            Stream = stream;
            Format = format;
        }
        public Sound(byte[] buffer, int channels, int bits, int rate)
        {
            Buffer = buffer;
            Channels = channels;
            Bits = bits;
            SampleRate = rate;
        }

        public void Play(float volume = 1.0f, bool loop = false)
        {
            if (Source > 0)
            {
                AL.Source(Source, ALSourcef.Gain, volume);
                AL.Source(Source, ALSourceb.Looping, loop);
                AL.SourcePlay(Source);
            }
        }
        public void Play(Vector3 pos, float volume = 1.0f, bool loop = false)
        {
            if (Source > 0)
            {
                AL.Source(Source, ALSource3f.Position, ref pos);
                AL.Source(Source, ALSourcef.Gain, volume);
                AL.Source(Source, ALSourceb.Looping, loop);
                AL.SourcePlay(Source);
            }
        }
        public void Stop()
        {
            if (Source > 0) AL.SourceStop(Source);
        }

        public Result Load()
        {
            if (Loaded) Unload();

            if (Buffer == null)
            {
                if (Filepath != null && Filepath.Contains(".wav"))
                {
                    var buf = File.ReadAllBytes(Filepath);
                    using (var ms = new MemoryStream(buf))
                    {
                        int ch;
                        int bit;
                        int r;

                        try
                        {
                            Buffer = LoadWave(ms, out ch, out bit, out r);
                            Channels = ch;
                            Bits = bit;
                            SampleRate = r;
                        }
                        catch { }
                    }
                }
                else if (Filepath != null && Filepath.Contains(".mp3"))
                {
                    var buf = File.ReadAllBytes(Filepath);
                    using (var ms = new MemoryStream(buf))
                    {
                        int ch;
                        int bit;
                        int r;

                        try
                        {
                            Buffer = LoadWave(ms, out ch, out bit, out r);
                            Channels = ch;
                            Bits = bit;
                            SampleRate = r;
                        }
                        catch { }
                    }
                }
                else if (Stream != null)
                {
                    int ch = 0;
                    int bit = 0;
                    int r = 0;

                    try
                    {
                        if (Format == "WAV") Buffer = LoadWave(Stream, out ch, out bit, out r);
                        else if (Format == "MP3") Buffer = LoadMp3(Stream, out ch, out bit, out r);
                        Channels = ch;
                        Bits = bit;
                        SampleRate = r;
                    }
                    catch { }
                }
            }
            if (Buffer == null) return Result.Failed;

            Source = AL.GenSource();
            ALBuffer = AL.GenBuffer();
            var format = ALFormat.Stereo16;
            if (Channels == 2 && Bits == 8) format = ALFormat.Stereo8;
            else if (Channels == 1 && Bits == 16) format = ALFormat.Mono16;
            else if (Channels == 1 && Bits == 8) format = ALFormat.Mono8;
            AL.BufferData(ALBuffer, format, Buffer, Buffer.Length, SampleRate);
            AL.BindBufferToSource(Source, ALBuffer);

            Loaded = true;
            return Result.Success;
        }

        public Result Unload()
        {
            if (!Loaded) return Result.Success;

            AL.DeleteSource(Source);
            AL.DeleteBuffer(ALBuffer);

            Loaded = false;
            return Result.Success;
        }

        private static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = null; 
                while (true)
                {
                    data_signature = new string(reader.ReadChars(4));
                    if (data_signature == "data") break;
                    
                    var size = reader.ReadInt32();
                    reader.ReadBytes(size);
                }

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes(data_chunk_size);
            }
        }

        private static byte[] LoadMp3(Stream stream, out int channels, out int bits, out int rate)
        {
            using (var mp3 = new Mp3FileReader(stream))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    return LoadWave(pcm, out channels, out bits, out rate);
                }
            }
        }

        public Sound Clone()
        {
            var sound = new Sound(Buffer, Channels, Bits, SampleRate);
            sound.Load();
            return sound;
        }
    }
}
