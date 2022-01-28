using SFML.Audio;

namespace FunManipulator
{
    public class Beeps
    {
        public static SoundBuffer Buffer;
        public static Sound Sound;
        public static double Start;

        public static void BuildData()
        {
            SoundBuffer sound = new SoundBuffer(Config.Instance.BeepFilename);
            short[] buffer = new short[(int)Math.Ceiling(sound.SampleRate * sound.ChannelCount * Config.Instance.BeepCount * Config.Instance.BeepInterval)];
            Parallel.For(0, Config.Instance.BeepCount, i =>
            {
                Array.Copy(sound.Samples, 0, buffer, 
                           (int)Math.Ceiling(sound.SampleRate * sound.ChannelCount * i * Config.Instance.BeepInterval), sound.Samples.Length);
            });
            Buffer = new SoundBuffer(buffer, sound.ChannelCount, sound.SampleRate);
            Sound = new Sound(Buffer);
            Start = (Config.Instance.BeepCount - 1) * Config.Instance.BeepInterval;
            sound.Dispose();
        }
    }
}
