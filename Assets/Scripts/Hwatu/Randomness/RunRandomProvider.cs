using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.Randomness
{
    [DisallowMultipleComponent]
    public sealed class RunRandomProvider : MonoBehaviour
    {
        [SerializeField] private int runSeed;
        [SerializeField] private bool useRandomSeed;

        private readonly Dictionary<RandomStreamId, IRandomSource> streams =
            new Dictionary<RandomStreamId, IRandomSource>();
        private bool isSeedInitialized;

        public int RunSeed => runSeed;

        public void BeginRun(int seed)
        {
            runSeed = seed;
            streams.Clear();
            isSeedInitialized = true;
        }

        public IRandomSource GetStream(RandomStreamId streamId)
        {
            if (!Enum.IsDefined(typeof(RandomStreamId), streamId))
            {
                throw new ArgumentOutOfRangeException(nameof(streamId));
            }

            EnsureSeedInitialized();

            IRandomSource randomSource;
            if (!streams.TryGetValue(streamId, out randomSource))
            {
                randomSource = new SeededRandomSource(
                    DeriveStreamSeed(runSeed, streamId));
                streams.Add(streamId, randomSource);
            }

            return randomSource;
        }

        private void EnsureSeedInitialized()
        {
            if (isSeedInitialized)
            {
                return;
            }

            if (useRandomSeed)
            {
                runSeed = Guid.NewGuid().GetHashCode();
            }

            isSeedInitialized = true;
        }

        private static int DeriveStreamSeed(int seed, RandomStreamId streamId)
        {
            return unchecked((seed * 397) ^ (int)streamId);
        }
    }
}
