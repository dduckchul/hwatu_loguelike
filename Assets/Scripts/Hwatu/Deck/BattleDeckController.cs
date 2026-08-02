using System;
using UnityEngine;

namespace Hwatu.Deck
{
    [DisallowMultipleComponent]
    public sealed class BattleDeckController : MonoBehaviour
    {
        public BattleDeck Deck { get; private set; }
        public bool IsInitialized => Deck != null;

        public void Initialize(PlayerDeck playerDeck, int shuffleSeed)
        {
            if (playerDeck == null)
            {
                throw new ArgumentNullException(nameof(playerDeck));
            }

            Deck = playerDeck.CreateBattleDeck(new SeededRandomSource(shuffleSeed));
        }
    }
}
