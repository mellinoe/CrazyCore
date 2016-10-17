using System;
using Engine;
using Engine.Behaviors;
using ImGuiNET;
using Engine.Assets;
using Engine.Graphics;
using System.Numerics;
using Engine.Audio;
using System.Runtime.InteropServices;

namespace GravityGame
{
    public class MainMenu : Behavior
    {
        private Action _menuFunc;
        private AssetSystem _assetSystem;
        private SceneLoaderSystem _sls;
        private GraphicsSystem _gs;
        private AudioSourceComponent _audioSource;
        private Font _font;

        public MainMenu()
        {
            _menuFunc = DrawMainPage;
        }

        protected unsafe override void Start(SystemRegistry registry)
        {
            _assetSystem = registry.GetSystem<AssetSystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
            _audioSource = GameObject.GetComponent<AudioSourceComponent>();
            string fontPath = GetMainMenuFontPath();
            if (fontPath != null)
            {
                _font = ImGui.GetIO().FontAtlas.AddFontFromFileTTF(GetMainMenuFontPath(), 48);
            }

            _gs.ImGuiRenderer.RecreateFontDeviceTexture(_gs.Context);
        }

        private string GetMainMenuFontPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return @"C:\Windows\Fonts\segoeui.ttf";
            }

            return null;
        }

        public unsafe override void Update(float deltaSeconds)
        {
            bool opened = true;
            var io = ImGui.GetIO();

            ImGui.SetNextWindowSize(io.DisplaySize * .5f, SetCondition.Always);
            ImGui.SetNextWindowPosCenter(SetCondition.Always);
            ImGui.BeginWindow("", ref opened, 0.0f, WindowFlags.NoTitleBar | WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoMove);
            ImGui.SetWindowFontScale(1.0f);
            if (_font != null)
            {
                ImGui.PushFont(_font);
            }
            _menuFunc();
            if (_font != null)
            {
                ImGui.PopFont();
            }
            ImGui.SetWindowFontScale(1.0f);
            ImGui.EndWindow();
        }

        private void DrawMainPage()
        {
            if (ImGui.Button("Play", new Vector2(250, 60)))
            {
                _menuFunc = DrawPlayPage;
            }
            if (ImGui.Button("Options", new Vector2(250, 60)))
            {
                _menuFunc = DrawOptionsPage;
            }
            if (ImGui.Button("Credits", new Vector2(250, 60)))
            {
                _menuFunc = DrawCreditsPage;
            }
            if (ImGui.Button("Exit", new Vector2(250, 60)))
            {
                _gs.Context.Window.Close();
            }
        }

        private void DrawCreditsPage()
        {
            ImGui.Text("Created by Eric Mellino");
            ImGui.Text("Built using .NET Core.");
            if (ImGui.Button("Back"))
            {
                _menuFunc = DrawMainPage;
            }
        }

        private void DrawOptionsPage()
        {
            if (ImGui.Button("Back"))
            {
                _menuFunc = DrawMainPage;
            }
        }

        private void DrawPlayPage()
        {
            foreach (var scene in _assetSystem.Database.GetAssetsOfType(typeof(SceneAsset)))
            {
                if (ImGui.Button(scene))
                {
                    _sls.LoadScene(_assetSystem.Database.LoadAsset<SceneAsset>(scene));
                }
            }
            if (ImGui.Button("Back"))
            {
                _menuFunc = DrawMainPage;
            }
        }
    }
}
