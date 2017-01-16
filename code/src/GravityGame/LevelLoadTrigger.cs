using System;
using Engine;
using Engine.Physics;
using Veldrid.Assets;
using Engine.Assets;
using Engine.Audio;
using ImGuiNET;
using System.Numerics;

namespace GravityGame
{
    public class LevelLoadTrigger : Component
    {
        private Collider _collider;
        protected SceneLoaderSystem _sls;
        protected AssetSystem _assetSystem;
        private AudioSystem _audioSystem;

        public AssetRef<SceneAsset> LoadedScene { get; set; }
        public AssetRef<WaveFile> AudioClip { get; set; }
        public float Volume { get; set; } = 1.0f;
        public bool DisableCinematicCameraEffect { get; set; } = false;

        public event Action<GameObject> LevelLoadTriggered;

        protected override void Attached(SystemRegistry registry)
        {
            _assetSystem = registry.GetSystem<AssetSystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
            _audioSystem = registry.GetSystem<AudioSystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        protected override void OnDisabled()
        {
            if (_collider == null)
            {
                throw new InvalidOperationException("Collider Component was missing on  " + GameObject.Name);
            }
            _collider.TriggerEntered -= OnTriggerEntered;
        }

        protected override void OnEnabled()
        {
            _collider = GameObject.GetComponent<Collider>();
            if (_collider == null)
            {
                throw new InvalidOperationException("Collider Component was missing on  " + GameObject.Name);
            }

            _collider.TriggerEntered += OnTriggerEntered;
        }

        private void OnTriggerEntered(Collider other)
        {
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                if (AudioClip != null)
                {
                    _audioSystem.PlaySound(_assetSystem.Database.LoadAsset(AudioClip), Volume);
                }

                LevelLoadTriggered?.Invoke(other.GameObject);
                LoadLevel();
            }
        }

        protected virtual void LoadLevel()
        {
            if (DisableCinematicCameraEffect)
            {
                CinematicCamera.SkipCinematicCamera = true;
            }

            SceneAsset scene = _assetSystem.Database.LoadAsset(LoadedScene, cache: false);
            _sls.LoadScene(scene);
        }
    }

    public class LevelLoadTriggerWithMenu : LevelLoadTrigger
    {
        private TimeControlSystem _timeSystem;
        private InputSystem _input;

        protected override void Attached(SystemRegistry registry)
        {
            base.Attached(registry);
            _timeSystem = registry.GetSystem<TimeControlSystem>();
            _input = registry.GetSystem<InputSystem>();
        }

        protected override void LoadLevel()
        {
            GameObject.AddComponent(new DelegateMenu(DrawMenu, null));
            _timeSystem.TimeScale = 0f;
            Enabled = false;
        }

        private bool DrawMenu()
        {
            var displaySize = ImGui.GetIO().DisplaySize;
            if (MenuGlobals.MenuFont != null)
            {
                ImGui.PushFont(MenuGlobals.MenuFont);
            }
            ImGui.SetNextWindowSize(displaySize * new Vector2(0.7f, 1f) - new Vector2(0, 20), SetCondition.Always);
            ImGui.SetNextWindowPosCenter(SetCondition.Always);
            if (ImGui.BeginWindow(string.Empty, WindowFlags.NoTitleBar | WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoMove))
            {
                if (ImGui.Button("Next Level (Space)"))
                {
                    LoadNextLevel();
                }
                if (ImGui.Button("Retry (R)"))
                {
                    RetryCurrentLevel();
                }
                if (ImGui.Button("Exit To Main Menu"))
                {
                    AssetID sceneID = "Scenes/MainMenu.scene";
                    SceneAsset scene = _assetSystem.Database.LoadAsset<SceneAsset>(sceneID, cache: false);
                    _sls.LoadScene(scene);
                }
            }
            ImGui.EndWindow();
            if (MenuGlobals.MenuFont != null)
            {
                ImGui.PopFont();
            }

            if (_input.GetKeyDown(Veldrid.Platform.Key.Space))
            {
                LoadNextLevel();
            }
            else if (_input.GetKeyDown(Veldrid.Platform.Key.R))
            {
                RetryCurrentLevel();
            }

            return false;
        }

        private void RetryCurrentLevel()
        {
            AssetID sceneID = "Scenes/" + _sls.LoadedScene.Name + ".scene";
            SceneAsset scene = _assetSystem.Database.LoadAsset<SceneAsset>(sceneID, cache: false);
            _sls.LoadScene(scene);
        }

        private void LoadNextLevel()
        {
            SceneAsset scene = _assetSystem.Database.LoadAsset(LoadedScene, cache: false);
            _sls.LoadScene(scene);
        }
    }
}
