using System;

namespace Hwatu.Hands
{
    [Flags]
    public enum HandTag
    {
        None = 0,
        Named = 1 << 0,
        Pair = 1 << 1,
        Bright = 1 << 2
    }
}
