using System;
using Engine.Behaviors;
using Engine;
using ImGuiNET;
using Engine.Graphics;
using Veldrid.Assets;
using Engine.Assets;
using System.Numerics;

namespace CrazyCore
{
    public class PauseMenu : Behavior
    {
        private bool _visible = false;
        private InputSystem _input;
        private GraphicsSystem _gs;
        private SceneLoaderSystem _sls;
        private AssetSystem _as;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
            _as = registry.GetSystem<AssetSystem>();
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.Escape) && (MenuGlobals.NumMenusOpen == 0 || _visible))
            {
                SetVisible(!_visible);
            }

            if (_visible)
            {
                if (MenuGlobals.MenuFont != null)
                {
                    ImGui.PushFont(MenuGlobals.MenuFont);
                }

                ImGui.SetNextWindowPosCenter(SetCondition.Always);
                var io = ImGui.GetIO();
                ImGui.SetNextWindowSize(new Vector2(io.DisplaySize.X * .5f, io.DisplaySize.Y * .4f), SetCondition.Always);
                if (ImGui.BeginWindow("Pause Menu", WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoTitleBar | WindowFlags.NoMove))
                {
                    if (ImGui.Button("Restart (R)"))
                    {
                        Restart();
                    }
                    if (ImGui.Button("Exit To Main Menu"))
                    {
                        GoToMainMenu();
                    }
                    if (ImGui.Button("Quit"))
                    {
                        Quit();
                    }
                }
                ImGui.EndWindow();

                if (_input.GetKeyDown(Veldrid.Platform.Key.R))
                {
                    Restart();
                }

                if (MenuGlobals.MenuFont != null)
                {
                    ImGui.PopFont();
                }
            }
        }

        private void Restart()
        {
            AssetID sceneID = "Scenes/" + _sls.LoadedScene.Name + ".scene";
            SceneAsset scene = _as.Database.LoadAsset<SceneAsset>(sceneID, cache: false);
            CinematicCamera.SkipCinematicCamera = true;
            _sls.LoadScene(scene);
        }

        private void GoToMainMenu()
        {
            AssetID sceneID = "Scenes/MainMenu.scene";
            SceneAsset scene = _as.Database.LoadAsset<SceneAsset>(sceneID, cache: false);
            _sls.LoadScene(scene);
        }

        private void Quit()
        {
            _gs.Context.Window.Close();
        }

        private void SetVisible(bool value)
        {
            if (_visible != value)
            {
                _visible = value;
                if (_visible)
                {
                    MenuGlobals.PushMenuOpened();
                }
                else
                {
                    MenuGlobals.PopMenuOpened();
                }
            }
        }

        protected override void PostDisabled()
        {
            if (_visible)
            {
                MenuGlobals.PopMenuOpened();
            }
        }

        protected override void PostEnabled()
        {
            if (_visible)
            {
                MenuGlobals.PushMenuOpened();
            }
        }
    }
}
