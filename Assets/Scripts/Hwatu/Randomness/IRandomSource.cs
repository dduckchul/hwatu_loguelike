namespace Hwatu.Randomness
{
    public interface IRandomSource
    {
        int Next(int minInclusive, int maxExclusive);
    }
}
