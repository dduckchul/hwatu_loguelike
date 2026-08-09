using System;
using System.Collections.Generic;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardUpgradeChoiceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button ribbonButton;
        [SerializeField] private Image ribbonImage;
        [SerializeField] private Button animalButton;
        [SerializeField] private Image animalImage;
        [SerializeField] private Button backButton;

        private CardData ribbonCandidate;
        private CardData animalCandidate;

        public event Action<CardData> CandidateSelected;
        public event Action BackRequested;

        private void Awake()
        {
            ValidateReferences();
            ribbonButton.onClick.AddListener(SelectRibbon);
            animalButton.onClick.AddListener(SelectAnimal);
            backButton.onClick.AddListener(RequestBack);
        }

        private void OnDestroy()
        {
            if (ribbonButton != null)
            {
                ribbonButton.onClick.RemoveListener(SelectRibbon);
            }

            if (animalButton != null)
            {
                animalButton.onClick.RemoveListener(SelectAnimal);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(RequestBack);
            }
        }

        public void Show(
            CardInstance sourceCard,
            IReadOnlyList<CardData> candidates)
        {
            if (sourceCard == null)
            {
                throw new ArgumentNullException(nameof(sourceCard));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            ValidateReferences();
            ribbonCandidate = null;
            animalCandidate = null;

            foreach (CardData candidate in candidates)
            {
                if (candidate == null)
                {
                    throw new ArgumentException(
                        "Upgrade candidates cannot contain null.",
                        nameof(candidates));
                }

                if (candidate.Month != sourceCard.Definition.Month)
                {
                    throw new InvalidOperationException(
                        "An upgrade candidate must have the same month as the source card.");
                }

                switch (candidate.CardType)
                {
                    case CardType.Ribbon:
                        if (ribbonCandidate != null)
                        {
                            throw new InvalidOperationException(
                                "Only one Ribbon upgrade candidate can be displayed.");
                        }

                        ribbonCandidate = candidate;
                        break;

                    case CardType.Animal:
                        if (animalCandidate != null)
                        {
                            throw new InvalidOperationException(
                                "Only one Animal upgrade candidate can be displayed.");
                        }

                        animalCandidate = candidate;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Only Ribbon and Animal cards can be upgrade candidates.");
                }
            }

            if (ribbonCandidate == null && animalCandidate == null)
            {
                throw new InvalidOperationException(
                    "At least one upgrade candidate is required.");
            }

            titleText.text = $"{sourceCard.Definition.Month}월 강화 선택";
            BindCandidate(ribbonButton, ribbonImage, ribbonCandidate);
            BindCandidate(animalButton, animalImage, animalCandidate);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            ribbonCandidate = null;
            animalCandidate = null;
            gameObject.SetActive(false);
        }

        private void SelectRibbon()
        {
            if (ribbonCandidate != null)
            {
                CandidateSelected?.Invoke(ribbonCandidate);
            }
        }

        private void SelectAnimal()
        {
            if (animalCandidate != null)
            {
                CandidateSelected?.Invoke(animalCandidate);
            }
        }

        private void RequestBack()
        {
            BackRequested?.Invoke();
        }

        private static void BindCandidate(
            Button button,
            Image image,
            CardData candidate)
        {
            bool isAvailable = candidate != null;
            button.gameObject.SetActive(isAvailable);
            if (isAvailable)
            {
                image.sprite = candidate.Artwork;
                image.preserveAspect = true;
            }
        }

        private void Reset()
        {
            titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
            ribbonButton = transform.Find("RibbonButton")?.GetComponent<Button>();
            ribbonImage = ribbonButton == null
                ? null
                : ribbonButton.GetComponent<Image>();
            animalButton = transform.Find("AnimalButton")?.GetComponent<Button>();
            animalImage = animalButton == null
                ? null
                : animalButton.GetComponent<Image>();
            backButton = transform.Find("BackButton")?.GetComponent<Button>();
        }

        private void ValidateReferences()
        {
            if (titleText == null
                || ribbonButton == null
                || ribbonImage == null
                || animalButton == null
                || animalImage == null
                || backButton == null)
            {
                throw new InvalidOperationException(
                    "Card upgrade choice view references are not fully assigned.");
            }
        }
    }
}
