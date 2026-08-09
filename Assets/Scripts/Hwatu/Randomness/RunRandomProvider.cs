using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.Randomness
{
    [DisallowMultipleComponent]
    public sealed class RunRandomProvider : MonoBehaviour
    {
        [SerializeField] private int runSeed;

        private readonly Dictionary<RandomStreamId, IRandomSource> streams =
            new Dictionary<RandomStreamId, IRandomSource>();

        public int RunSeed => runSeed;

        public void BeginRun(int seed)
        {
            runSeed = seed;
            streams.Clear();
        }

        public IRandomSource GetStream(RandomStreamId streamId)
        {
            if (!Enum.IsDefined(typeof(RandomStreamId), streamId))
            {
                throw new ArgumentOutOfRangeException(nameof(streamId));
            }

            IRandomSource randomSource;
            if (!streams.TryGetValue(streamId, out randomSource))
            {
                randomSource = new SeededRandomSource(
                    DeriveStreamSeed(runSeed, streamId));
                streams.Add(streamId, randomSource);
            }

            return randomSource;
        }

        private static int DeriveStreamSeed(int seed, RandomStreamId streamId)
        {
            return unchecked((seed * 397) ^ (int)streamId);
        }
    }
}
