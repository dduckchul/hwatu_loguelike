using System;
using Hwatu.Randomness;
using UnityEngine;

namespace Hwatu.Deck
{
    [DisallowMultipleComponent]
    public sealed class BattleDeckController : MonoBehaviour
    {
        public BattleDeck Deck { get; private set; }
        public bool IsInitialized => Deck != null;

        public void Initialize(PlayerDeck playerDeck, IRandomSource randomSource)
        {
            if (playerDeck == null)
            {
                throw new ArgumentNullException(nameof(playerDeck));
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            Deck = playerDeck.CreateBattleDeck(randomSource);
        }
    }
}
