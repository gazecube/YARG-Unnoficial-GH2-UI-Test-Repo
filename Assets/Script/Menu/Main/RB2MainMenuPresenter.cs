using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YARG.Menu.Navigation;

namespace YARG.Menu.Main
{
    /// <summary>
    /// First-stage Rock Band 2 main-menu presentation layer.
    ///
    /// The original RB2 shell keeps a persistent MainPanel and changes authored
    /// presentation state when focus moves between buttons. This class mirrors that
    /// separation in YARG: MainMenu still owns the actions, while this component owns
    /// selection presentation.
    ///
    /// No original Harmonix assets are embedded here. The animation keys intentionally
    /// match the names used by RB2's authored main.milo scene so a local asset importer
    /// can bind them later without changing menu/backend code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RB2MainMenuPresenter : MonoBehaviour
    {
        public enum AuthoredSelection
        {
            Quickplay,
            Tour,
            Training,
            Options,
            Store,
            Extras,
        }

        public readonly struct SelectionState
        {
            public readonly AuthoredSelection Selection;
            public readonly string AuthoredKey;

            public SelectionState(AuthoredSelection selection, string authoredKey)
            {
                Selection = selection;
                AuthoredKey = authoredKey;
            }
        }

        private const float SELECTED_SCALE = 1.10f;
        private const float UNSELECTED_SCALE = 1.0f;
        private const float SELECTED_ALPHA = 1.0f;
        private const float UNSELECTED_ALPHA = 0.68f;
        private const float ANIMATION_SPEED = 12.0f;

        private readonly List<EntryPresentation> _entries = new();

        private NavigationGroup _navigationGroup;
        private SelectionState _selectionState;
        private MainMenu _mainMenu;

        /// <summary>
        /// Fired whenever the YARG selection maps to a new RB2 authored presentation key.
        /// Future scene/camera import code can subscribe to this instead of knowing about
        /// YARG's concrete menu objects.
        /// </summary>
        public event Action<SelectionState> AuthoredSelectionChanged;

        public SelectionState CurrentSelection => _selectionState;

        private sealed class EntryPresentation
        {
            public readonly NavigatableBehaviour Behaviour;
            public readonly Transform Transform;
            public readonly CanvasGroup CanvasGroup;
            public readonly AuthoredSelection AuthoredSelection;
            public readonly string AuthoredKey;

            public EntryPresentation(NavigatableBehaviour behaviour, CanvasGroup canvasGroup,
                AuthoredSelection authoredSelection, string authoredKey)
            {
                Behaviour = behaviour;
                Transform = behaviour.transform;
                CanvasGroup = canvasGroup;
                AuthoredSelection = authoredSelection;
                AuthoredKey = authoredKey;
            }
        }

        private void Awake()
        {
            _mainMenu = GetComponent<MainMenu>();
            ConfigureRb2TopLevelEntries();
            BuildEntryCache();
        }

        private void OnEnable()
        {
            if (_navigationGroup == null)
            {
                BuildEntryCache();
            }

            if (_navigationGroup != null)
            {
                _navigationGroup.SelectionChanged += OnSelectionChanged;

                if (_navigationGroup.SelectedBehaviour != null)
                {
                    OnSelectionChanged(_navigationGroup.SelectedBehaviour, SelectionOrigin.Programmatically);
                }
            }
        }

        private void OnDisable()
        {
            if (_navigationGroup != null)
            {
                _navigationGroup.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void Update()
        {
            foreach (var entry in _entries)
            {
                bool selected = entry.Behaviour.Selected;
                float targetScale = selected ? SELECTED_SCALE : UNSELECTED_SCALE;
                float targetAlpha = selected ? SELECTED_ALPHA : UNSELECTED_ALPHA;

                entry.Transform.localScale = Vector3.Lerp(entry.Transform.localScale,
                    Vector3.one * targetScale, Time.unscaledDeltaTime * ANIMATION_SPEED);

                entry.CanvasGroup.alpha = Mathf.Lerp(entry.CanvasGroup.alpha,
                    targetAlpha, Time.unscaledDeltaTime * ANIMATION_SPEED);
            }
        }

        private void ConfigureRb2TopLevelEntries()
        {
            var menuOptions = transform.Find("Menu Options");
            if (menuOptions == null || _mainMenu == null)
            {
                return;
            }

            // Reuse the existing YARG menu-entry prefab instances so navigation,
            // pointer support and layout continue to work. We only replace their
            // labels/actions/order with RB2's original top-level shell.
            ConfigureEntry(menuOptions, "Quickplay", "Quickplay", 0, _mainMenu.QuickPlay);
            ConfigureEntry(menuOptions, "Profiles", "Tour", 1, null);
            ConfigureEntry(menuOptions, "Practice", "Training", 2, _mainMenu.Practice);
            ConfigureEntry(menuOptions, "Settings", "Options", 3, _mainMenu.Settings);
            ConfigureEntry(menuOptions, "Replays", "Community", 4, null);
            ConfigureEntry(menuOptions, "Credits", "Music Store", 5, null);

            var exit = menuOptions.Find("Exit");
            if (exit != null)
            {
                exit.gameObject.SetActive(false);
            }
        }

        private static void ConfigureEntry(Transform parent, string existingName, string rb2Name,
            int siblingIndex, UnityEngine.Events.UnityAction action)
        {
            var entry = parent.Find(existingName);
            if (entry == null)
            {
                return;
            }

            entry.name = rb2Name;
            entry.SetSiblingIndex(siblingIndex);

            var text = entry.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = rb2Name.ToUpperInvariant();
            }

            var button = entry.GetComponent<NavigatableButton>();
            if (button != null)
            {
                button.RemoveOnClickListeners();
                if (action != null)
                {
                    button.SetOnClickEvent(action);
                }
            }
        }

        private void BuildEntryCache()
        {
            _entries.Clear();

            var menuOptions = transform.Find("Menu Options");
            if (menuOptions == null)
            {
                return;
            }

            _navigationGroup = menuOptions.GetComponent<NavigationGroup>();
            if (_navigationGroup == null)
            {
                return;
            }

            foreach (Transform child in menuOptions)
            {
                var behaviour = child.GetComponent<NavigatableBehaviour>();
                if (behaviour == null)
                {
                    continue;
                }

                var canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
                }

                MapYargEntryToRb2(child.name, out var authoredSelection, out var authoredKey);
                _entries.Add(new EntryPresentation(behaviour, canvasGroup, authoredSelection, authoredKey));
            }
        }

        private void OnSelectionChanged(NavigatableBehaviour selected, SelectionOrigin selectionOrigin)
        {
            if (selected == null)
            {
                return;
            }

            foreach (var entry in _entries)
            {
                if (entry.Behaviour != selected)
                {
                    continue;
                }

                _selectionState = new SelectionState(entry.AuthoredSelection, entry.AuthoredKey);
                AuthoredSelectionChanged?.Invoke(_selectionState);
                return;
            }
        }

        private static void MapYargEntryToRb2(string entryName,
            out AuthoredSelection selection, out string authoredKey)
        {
            // RB2's main.milo has dedicated presentation states named:
            // quickplay, tour, training, options, store and extras.
            //
            // YARG does not yet have equivalents for every RB2 top-level destination,
            // so unsupported destinations intentionally map to the closest "extras"
            // presentation while keeping their original YARG action intact.
            switch (entryName)
            {
                case "Quickplay":
                    selection = AuthoredSelection.Quickplay;
                    authoredKey = "quickplay";
                    break;

                case "Tour":
                    selection = AuthoredSelection.Tour;
                    authoredKey = "tour";
                    break;

                case "Training":
                    selection = AuthoredSelection.Training;
                    authoredKey = "training";
                    break;

                case "Options":
                    selection = AuthoredSelection.Options;
                    authoredKey = "options";
                    break;

                case "Music Store":
                    selection = AuthoredSelection.Store;
                    authoredKey = "store";
                    break;

                case "Community":
                default:
                    selection = AuthoredSelection.Extras;
                    authoredKey = "extras";
                    break;
            }
        }
    }
}
