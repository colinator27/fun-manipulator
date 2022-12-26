using System;
namespace FunManipulator;

public static class DustParticles
{
    private static List<DustImage>? _images = null;
    public static List<DustImage> Images { get => LoadImages(); }

    private static List<DustImage> LoadImages()
    {
        if (_images != null)
            return _images;

        _images = new List<DustImage>();
        using (BinaryReader br = new(new FileStream("dust_data.bin", FileMode.Open)))
        {
            int count = br.ReadUInt16();
            _images.EnsureCapacity(count);
            for (int i = 0; i < count; i++)
            {
                DustImage image = new();
                image.Load(br);
                _images.Add(image);
            }
        }
        return _images;
    }
}

public struct DustParticle
{
    public float X;
    public float Y;
    public int XScale;

    public DustParticle(float x, float y, int xscale)
    {
        X = x;
        Y = y;
        XScale = xscale;
    }
}

public sealed class DustFrame
{
    public List<DustParticle> Particles { get; init; } = new();
}

public sealed class DustImage
{
    public List<DustFrame> Frames { get; init; } = new();

    public void Load(BinaryReader br)
    {
        int frameCount = br.ReadByte();
        Frames.EnsureCapacity(frameCount);
        for (int i = 0; i < frameCount; i++)
        {
            DustFrame frame = new();
            Frames.Add(frame);

            int particleCount = br.ReadByte();
            frame.Particles.EnsureCapacity(frameCount);
            for (int j = 0; j < particleCount; j++)
            {
                int x = br.ReadByte();
                int y = br.ReadByte();
                int xscale = br.ReadByte();
                frame.Particles.Add(new(x, y, xscale));
            }
        }
    }
}

public static partial class Search
{
    public sealed class ElementExtLastDustFrame : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.ExtLastDustFrame;
        public int GetSize() => _size;

        public DustParticle[] DataParticles { get; init; }
        public DustParticle[] RecordedParticles { get; init; }
        public ThreadLocal<DustParticle[]> WorkingParticles { get; init; }

        private int _size { get; init; }

        public ElementExtLastDustFrame(List<DustParticle> dataParticles, List<DustParticle> recordedParticles)
        {
            DataParticles = dataParticles.ToArray();
            RecordedParticles = recordedParticles.ToArray();
            WorkingParticles = new(() => (DustParticle[])DataParticles.Clone());
            _size = DataParticles.Length * 2;
        }

        public bool Check(uint[] rng, ref int index)
        {
            if (index + _size > rng.Length)
                return false;
            int currIndex = index;

            ReadOnlySpan<DustParticle> dataParticles = DataParticles;
            ReadOnlySpan<DustParticle> recordedParticles = RecordedParticles;
            Span<DustParticle> workingParticles = WorkingParticles.Value!;
            for (int i = 0; i < dataParticles.Length; i++)
            {
                float yRNG = RNG.ValueToSingle(rng[currIndex++], 0.5f) + 0.2f;
                float xRNG = RNG.ValueToSingle(rng[currIndex++], 4f) - 2f;
                workingParticles[i].X = (dataParticles[i].X * 2) + (xRNG * 11);
                workingParticles[i].Y = (dataParticles[i].Y * 2) - (yRNG * 66);
            }

            const float maxDistanceSquared = 3f * 3f;
            for (int i = 0; i < recordedParticles.Length; i++)
            {
                var particle = recordedParticles[i];
                bool found = false;
                for (int j = 0; j < workingParticles.Length; j++)
                {
                    var workParticle = workingParticles[j];
                    float dx = particle.X - workParticle.X;
                    float dy = particle.Y - workParticle.Y;
                    float distSquared = (dx * dx) + (dy * dy);
                    if (distSquared <= maxDistanceSquared)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }

            index = currIndex;
            return true;
        }
    }
}