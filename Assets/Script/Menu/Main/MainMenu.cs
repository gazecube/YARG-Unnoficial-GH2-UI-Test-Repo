using TMPro;
using UnityEngine;
using YARG.Core.Input;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Navigation;
using YARG.Menu.Persistent;
using YARG.Menu.Settings;
using YARG.Settings;

namespace YARG.Menu.Main
{
    public class MainMenu : MonoBehaviour
    {
        private static bool _antiPiracyDialogShown;

        private RB2MainMenuPresenter _rb2Presenter;

        [SerializeField]
        private TextMeshProUGUI _versionText;

        private void Start()
        {
            _versionText.text = GlobalVariables.Instance.CurrentVersion;

            // RB2 UI experiment: keep YARG's existing menu actions/backend, but hand the
            // presentation of the main-menu selection to an RB2-style presenter.
            // The presenter is added at runtime so the stock prefab remains usable while
            // we progressively port the authored RB2 scene and animation data.
            if (!TryGetComponent(out _rb2Presenter))
            {
                _rb2Presenter = gameObject.AddComponent<RB2MainMenuPresenter>();
            }

            // Show the anti-piracy dialog if it hasn't been shown already
            // Also only show it once per game launch
            if (!_antiPiracyDialogShown && SettingsManager.Settings.ShowAntiPiracyDialog)
            {
                DialogManager.Instance.ShowOneTimeMessage(
                    "Menu.Dialog.AntiPiracy",
                    () =>
                    {
                        SettingsManager.Settings.ShowAntiPiracyDialog = false;
                        SettingsManager.SaveSettings();
                    });

                _antiPiracyDialogShown = true;
            }

            if (SettingsMenu.ConsumeOpenOnNextMenuLoad())
            {
                SettingsMenu.Instance.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            // Set navigation scheme
            Navigator.Instance.PushScheme(new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown,
                new NavigationScheme.Entry(MenuAction.Select, "Menu.Main.GoToCurrentlyPlaying", CurrentlyPlaying)
            }, true));
        }

        private void OnDisable()
        {
            Navigator.Instance?.PopScheme();
        }

        public void CurrentlyPlaying()
        {
            MusicLibraryMenu.RequestGoToCurrentlyPlaying(MusicPlayer.NowPlaying);
            QuickPlay();
        }

        public void QuickPlay()
        {
            var menu = MenuManager.Instance.PushMenu(MenuManager.Menu.MusicLibrary, false);

            MusicLibraryMenu.LibraryMode = MusicLibraryMode.QuickPlay;

            menu.gameObject.SetActive(true);
        }

        public void Practice()
        {
            var menu = MenuManager.Instance.PushMenu(MenuManager.Menu.MusicLibrary, false);

            MusicLibraryMenu.LibraryMode = MusicLibraryMode.Practice;

            menu.gameObject.SetActive(true);
        }

        public void Profiles()
        {
            MenuManager.Instance.PushMenu(MenuManager.Menu.ProfileList);
        }

        public void Replays()
        {
            MenuManager.Instance.PushMenu(MenuManager.Menu.History);
        }

        public void Credits()
        {
            MenuManager.Instance.PushMenu(MenuManager.Menu.Credits);
        }

        public void Settings()
        {
            SettingsMenu.Instance.gameObject.SetActive(true);
        }

        public void Exit()
        {
#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;

#else
			Application.Quit();

#endif
        }

        public void OpenDiscord()
        {
            Application.OpenURL("https://discord.gg/sqpu4R552r");
        }

        public void OpenTwitter()
        {
            Application.OpenURL("https://twitter.com/YARGGame");
        }

        public void OpenGithub()
        {
            Application.OpenURL("https://github.com/YARC-Official/YARG");
        }
    }
}
