using SFML.Audio;

namespace FunManipulator
{
    public class Beeps
    {
        public static SoundBuffer? Buffer { get; private set; }
        public static Sound? Sound { get; private set; }
        public static double Start { get; private set; }

        public static void BuildData()
        {
            SoundBuffer sound = new(Config.Instance.BeepFilename);
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

        public static void Cleanup()
        {
            Sound?.Dispose();
            Buffer?.Dispose();
        }
    }
}
