using System;
using System.Collections.Generic;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    public enum CardCollectionMode
    {
        Browse,
        Selection
    }

    [DisallowMultipleComponent]
    public sealed class CardCollectionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text collectionTitle;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button selectionCancelButton;
        [SerializeField] private RectTransform monthGrid;
        [SerializeField] private MonthCardSlotView monthCardSlotTemplate;
        [SerializeField] private CardCatalogData cardCatalog;
        [SerializeField] private Vector2 artworkSize = new Vector2(60f, 108f);

        private readonly MonthCardSlotView[] monthSlots =
            new MonthCardSlotView[
                CardDefinition.MaxMonth - CardDefinition.MinMonth + 1];
        private readonly List<CardPresentation> generatedCards =
            new List<CardPresentation>();
        private readonly HashSet<CardInstance> selectableCards =
            new HashSet<CardInstance>();
        private bool isInitialized;
        private CardCollectionMode currentMode;

        public event Action<CardInstance> CardSelected;
        public event Action SelectionCancelled;

        private sealed class CardPresentation
        {
            public CardInstance Card { get; }
            public CardData Data { get; }
            public Image ArtworkImage { get; private set; }

            public CardPresentation(CardInstance card, CardData data)
            {
                Card = card;
                Data = data;
            }

            public void BindArtworkImage(Image artworkImage)
            {
                ArtworkImage = artworkImage;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
            backdropButton.onClick.AddListener(HandleBackdropClicked);
            selectionCancelButton.onClick.AddListener(HandleSelectionCancelled);
        }

        private void OnDestroy()
        {
            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(HandleBackdropClicked);
            }

            if (selectionCancelButton != null)
            {
                selectionCancelButton.onClick.RemoveListener(HandleSelectionCancelled);
            }
        }

        public void Show(
            string title,
            IReadOnlyList<CardInstance> cards,
            CardCollectionMode mode)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Collection title cannot be empty.",
                    nameof(title));
            }

            currentMode = mode;
            selectableCards.Clear();
            if (mode == CardCollectionMode.Selection)
            {
                AddSelectableCards(cards);
            }

            ShowCore(title, cards);
        }

        public void ShowSelection(
            string title,
            IReadOnlyList<CardInstance> cards,
            IReadOnlyList<CardInstance> selectable)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Collection title cannot be empty.",
                    nameof(title));
            }

            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            if (selectable == null)
            {
                throw new ArgumentNullException(nameof(selectable));
            }

            currentMode = CardCollectionMode.Selection;
            selectableCards.Clear();
            AddSelectableCards(selectable);
            ShowCore(title, cards);
        }

        private void ShowCore(string title, IReadOnlyList<CardInstance> cards)
        {
            collectionTitle.text = title;
            selectionCancelButton.gameObject.SetActive(
                currentMode == CardCollectionMode.Selection);
            backdropButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
            Refresh(cards);
        }

        public void Refresh(IReadOnlyList<CardInstance> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            EnsureInitialized();
            List<CardPresentation>[] cardsByMonth = ResolveCardsByMonth(cards);
            ClearCards();

            for (int month = CardDefinition.MinMonth;
                month <= CardDefinition.MaxMonth;
                month++)
            {
                List<CardPresentation> monthCards = cardsByMonth[month];
                monthCards.Sort(CompareCards);

                foreach (CardPresentation presentation in monthCards)
                {
                    CreateCardArtwork(
                        presentation,
                        monthSlots[month - CardDefinition.MinMonth].CardRoot);
                    generatedCards.Add(presentation);
                }
            }
        }

        public void Clear()
        {
            EnsureInitialized();
            ClearCards();
        }

        public void Hide()
        {
            Clear();
            selectableCards.Clear();
            selectionCancelButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
            backdropButton.gameObject.SetActive(false);
        }

        private void HandleBackdropClicked()
        {
            if (currentMode == CardCollectionMode.Browse)
            {
                Hide();
            }
        }

        private void HandleSelectionCancelled()
        {
            if (currentMode == CardCollectionMode.Selection)
            {
                SelectionCancelled?.Invoke();
            }
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            ValidateReferences();
            monthCardSlotTemplate.gameObject.SetActive(false);

            for (int month = CardDefinition.MinMonth;
                month <= CardDefinition.MaxMonth;
                month++)
            {
                MonthCardSlotView monthSlot = Instantiate(
                    monthCardSlotTemplate,
                    monthGrid);
                monthSlot.name = $"MonthCardSlot_{month:00}";
                monthSlot.SetMonth(month);
                monthSlot.gameObject.SetActive(true);
                monthSlots[month - CardDefinition.MinMonth] = monthSlot;
            }

            isInitialized = true;
        }

        private List<CardPresentation>[] ResolveCardsByMonth(
            IReadOnlyList<CardInstance> cards)
        {
            var cardsByMonth =
                new List<CardPresentation>[CardDefinition.MaxMonth + 1];
            for (int month = CardDefinition.MinMonth;
                month <= CardDefinition.MaxMonth;
                month++)
            {
                cardsByMonth[month] = new List<CardPresentation>();
            }

            foreach (CardInstance card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException(
                        "Card collection cannot contain a null card.",
                        nameof(cards));
                }

                int month = card.Definition.Month;
                if (month < CardDefinition.MinMonth
                    || month > CardDefinition.MaxMonth)
                {
                    throw new InvalidOperationException(
                        $"Card '{card.Definition.Id}' has an invalid month.");
                }

                CardData cardData = cardCatalog.GetById(card.Definition.Id);
                cardsByMonth[month].Add(new CardPresentation(card, cardData));
            }

            return cardsByMonth;
        }

        private void ClearCards()
        {
            foreach (CardPresentation presentation in generatedCards)
            {
                if (presentation.ArtworkImage == null)
                {
                    continue;
                }

                presentation.ArtworkImage.gameObject.SetActive(false);
                Destroy(presentation.ArtworkImage.gameObject);
            }

            generatedCards.Clear();
        }

        private void CreateCardArtwork(
            CardPresentation presentation,
            RectTransform parent)
        {
            var artworkObject = new GameObject(
                $"CardArtwork_{presentation.Data.CardId}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform artworkRect =
                artworkObject.GetComponent<RectTransform>();
            artworkRect.SetParent(parent, worldPositionStays: false);

            artworkRect.sizeDelta = artworkSize;

            Image artworkImage = artworkObject.GetComponent<Image>();
            artworkImage.sprite = presentation.Data.Artwork;
            artworkImage.preserveAspect = true;
            artworkImage.raycastTarget = false;

            if (currentMode == CardCollectionMode.Selection)
            {
                bool isSelectable = selectableCards.Contains(presentation.Card);
                artworkImage.color = isSelectable
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0.35f);

                if (isSelectable)
                {
                    artworkImage.raycastTarget = true;
                    Button artworkButton = artworkObject.AddComponent<Button>();
                    artworkButton.targetGraphic = artworkImage;
                    artworkButton.onClick.AddListener(
                        () => CardSelected?.Invoke(presentation.Card));
                }
            }

            presentation.BindArtworkImage(artworkImage);
        }

        private void AddSelectableCards(IReadOnlyList<CardInstance> cards)
        {
            foreach (CardInstance card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException(
                        "Selectable cards cannot contain a null card.",
                        nameof(cards));
                }

                selectableCards.Add(card);
            }
        }

        private static int CompareCards(CardPresentation first, CardPresentation second)
        {
            int typeComparison = first.Data.CardType.CompareTo(second.Data.CardType);
            return typeComparison != 0
                ? typeComparison
                : string.Compare(
                    first.Data.CardId,
                    second.Data.CardId,
                    StringComparison.Ordinal);
        }

        private void Reset()
        {
            collectionTitle = transform.Find("CollectionTitle")
                ?.GetComponent<TMP_Text>();
            selectionCancelButton = transform.Find("SelectionCancelButton")
                ?.GetComponent<Button>();
            GridLayoutGroup gridLayout =
                GetComponentInChildren<GridLayoutGroup>(includeInactive: true);
            monthGrid = gridLayout == null
                ? null
                : gridLayout.transform as RectTransform;
            monthCardSlotTemplate =
                GetComponentInChildren<MonthCardSlotView>(includeInactive: true);
        }

        private void ValidateReferences()
        {
            if (collectionTitle == null)
            {
                throw new InvalidOperationException(
                    "Collection title is not assigned.");
            }

            if (backdropButton == null)
            {
                throw new InvalidOperationException(
                    "Collection backdrop button is not assigned.");
            }

            if (selectionCancelButton == null)
            {
                throw new InvalidOperationException(
                    "Selection cancel button is not assigned.");
            }

            if (!selectionCancelButton.transform.IsChildOf(transform))
            {
                throw new InvalidOperationException(
                    "Selection cancel button must be inside the collection panel.");
            }

            if (backdropButton.transform == transform
                || backdropButton.transform.IsChildOf(transform))
            {
                throw new InvalidOperationException(
                    "Collection backdrop must be outside the collection panel.");
            }

            if (monthGrid == null)
            {
                throw new InvalidOperationException("Month grid is not assigned.");
            }

            if (monthCardSlotTemplate == null)
            {
                throw new InvalidOperationException(
                    "Month card slot template is not assigned.");
            }

            if (monthCardSlotTemplate.transform.parent != monthGrid)
            {
                throw new InvalidOperationException(
                    "Month card slot template must be a direct child of the month grid.");
            }

            if (cardCatalog == null)
            {
                throw new InvalidOperationException("Card catalog is not assigned.");
            }

            if (artworkSize.x <= 0f || artworkSize.y <= 0f)
            {
                throw new InvalidOperationException(
                    "Artwork size must have positive width and height.");
            }
        }
    }
}
