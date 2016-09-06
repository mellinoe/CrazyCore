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
        private unsafe Font* _font;

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
            _font = ImGuiNative.ImFontAtlas_AddFontFromFileTTF(ImGui.GetIO().GetNativePointer()->FontAtlas, GetMainMenuFontPath(), 48, IntPtr.Zero, null);
            _gs.ImGuiRenderer.RecreateFontDeviceTexture(_gs.Context);
        }

        private string GetMainMenuFontPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return @"C:\Windows\Fonts\segoeui.ttf";
            }

            throw new PlatformNotSupportedException();
        }

        public unsafe override void Update(float deltaSeconds)
        {
            bool opened = true;
            var io = ImGui.GetIO();

            ImGui.SetNextWindowPosCenter(SetCondition.Always);
            ImGui.SetNextWindowSize(io.DisplaySize * .5f, SetCondition.Always);
            ImGui.BeginWindow("", ref opened, 0.0f, WindowFlags.NoTitleBar | WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoMove);
            ImGui.SetWindowFontScale(1.0f);
            ImGuiNative.igPushFont(_font);
            _menuFunc();
            ImGuiNative.igPopFont();
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
