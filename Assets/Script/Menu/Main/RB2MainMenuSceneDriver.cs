using System;
using UnityEngine;

namespace YARG.Menu.Main
{
    /// <summary>
    /// Binding point between YARG's menu selection and the authored Rock Band 2
    /// main-menu scene imported into Unity.
    ///
    /// The source RB2 main.milo contains one camera-shot/animation family per
    /// top-level selection. Imported Unity clips should keep these state keys:
    /// quickplay, tour, training, options, store, extras.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RB2MainMenuSceneDriver : MonoBehaviour
    {
        [Serializable]
        public sealed class AuthoredState
        {
            public RB2MainMenuPresenter.AuthoredSelection Selection;
            public string Key;

            [Header("Original RB2 object names")]
            public string CameraShot;
            public string GriffinAnimation;
            public string MotdAnimation;
            public string TigerEyebrowAnimation;
        }

        [SerializeField]
        private Animator _sceneAnimator;

        [SerializeField]
        private AuthoredState[] _states =
        {
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Quickplay,
                Key = "quickplay",
                CameraShot = "camshot_quickplay.shot",
                GriffinAnimation = "Griffin_quickplay.anim",
                MotdAnimation = "motd_quickplay.anim",
                TigerEyebrowAnimation = "tiger_eyebrows_quickplay.anim",
            },
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Tour,
                Key = "tour",
                CameraShot = "camshot_tour.shot",
                GriffinAnimation = "Griffin_tour.anim",
                MotdAnimation = "motd_tour.anim",
                TigerEyebrowAnimation = "tiger_eyebrows_tour.anim",
            },
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Training,
                Key = "training",
                CameraShot = "camshot_training.shot",
                GriffinAnimation = "Griffin_training.anim",
                MotdAnimation = "motd_training.anim",
                TigerEyebrowAnimation = "tiger_eyebrows_training.anim",
            },
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Options,
                Key = "options",
                CameraShot = "camshot_options.shot",
                GriffinAnimation = "Griffin_options.anim",
                MotdAnimation = "motd_options.anim",
                TigerEyebrowAnimation = "tiger_eyebrows_options.anim",
            },
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Store,
                Key = "store",
                CameraShot = "camshot_store.shot",
                GriffinAnimation = "Griffin_store.anim",
                MotdAnimation = "motd_store.anim",
                TigerEyebrowAnimation = null,
            },
            new()
            {
                Selection = RB2MainMenuPresenter.AuthoredSelection.Extras,
                Key = "extras",
                CameraShot = "camshot_extras.shot",
                GriffinAnimation = "Griffin_extras.anim",
                MotdAnimation = "motd_extras.anim",
                TigerEyebrowAnimation = "tiger_eyebrows_extras.anim",
            },
        };

        private RB2MainMenuPresenter _presenter;

        private void Awake()
        {
            _presenter = GetComponentInParent<RB2MainMenuPresenter>();
        }

        private void OnEnable()
        {
            if (_presenter == null)
            {
                _presenter = GetComponentInParent<RB2MainMenuPresenter>();
            }

            if (_presenter == null)
            {
                return;
            }

            _presenter.AuthoredSelectionChanged += OnAuthoredSelectionChanged;
            OnAuthoredSelectionChanged(_presenter.CurrentSelection);
        }

        private void OnDisable()
        {
            if (_presenter != null)
            {
                _presenter.AuthoredSelectionChanged -= OnAuthoredSelectionChanged;
            }
        }

        private void OnAuthoredSelectionChanged(RB2MainMenuPresenter.SelectionState selection)
        {
            if (_sceneAnimator == null)
            {
                return;
            }

            var state = FindState(selection.Selection);
            if (state == null || string.IsNullOrEmpty(state.Key))
            {
                return;
            }

            // Imported RB2 clips/controllers use the authored selection key as the
            // Unity state name. A short fixed cross-fade keeps selection changes
            // responsive while the exact MILO keyframe timing is still being ported.
            _sceneAnimator.CrossFadeInFixedTime(state.Key, 0.08f, 0);
        }

        public AuthoredState FindState(RB2MainMenuPresenter.AuthoredSelection selection)
        {
            foreach (var state in _states)
            {
                if (state.Selection == selection)
                {
                    return state;
                }
            }

            return null;
        }
    }
}
