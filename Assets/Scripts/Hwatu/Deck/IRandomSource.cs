namespace Hwatu.Deck
{
    public interface IRandomSource
    {
        int Next(int minInclusive, int maxExclusive);
    }
}
