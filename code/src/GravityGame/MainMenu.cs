using System;
using Engine;
using Engine.Behaviors;
using ImGuiNET;
using Engine.Assets;
using Engine.Graphics;
using System.Numerics;
using Engine.Audio;
using System.Runtime.InteropServices;
using Veldrid.Assets;
using System.Linq;
using System.IO;

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
        private AssetID[] _allScenes;

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
            if (fontPath != null && File.Exists(fontPath))
            {
                _font = ImGui.GetIO().FontAtlas.AddFontFromFileTTF(GetMainMenuFontPath(), 48);
                MenuGlobals.MenuFont = _font;
            }

            _gs.ImGuiRenderer.RecreateFontDeviceTexture(_gs.Context);

            _allScenes = _assetSystem.Database.GetAssetsOfType(typeof(SceneAsset));
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

            ImGui.SetNextWindowSize(io.DisplaySize * new Vector2(0.5f, 0.75f), SetCondition.Always);
            ImGui.SetNextWindowPosCenter(SetCondition.Always);
            ImGui.BeginWindow("", ref opened, 0.0f, WindowFlags.NoTitleBar | WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoMove);
            ImGui.SetWindowFontScale(1.0f);
            if (_font != null)
            {
                ImGui.PushFont(_font);
            }
            _menuFunc();
            if (_menuFunc != DrawMainPage)
            {
                if (ImGui.Button("Back"))
                {
                    _menuFunc = DrawMainPage;
                }
            }
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
        }

        private void DrawOptionsPage()
        {
        }

        private void DrawPlayPage()
        {
            foreach (var kvp in PlayerStageProgress.Instance.Stages)
            {
                if (DrawSceneOptionLabel(kvp.Key, kvp.Value))
                {
                    AssetID sceneID = _allScenes.FirstOrDefault(id => id.Value.Contains(kvp.Key));
                    if (sceneID.IsEmpty)
                    {
                        throw new InvalidOperationException("No scene was found with the name " + kvp.Key);
                    }

                    _sls.LoadScene(_assetSystem.Database.LoadAsset<SceneAsset>(sceneID, false));
                }
            }
        }

        private bool DrawSceneOptionLabel(string sceneName, StageCompletionInfo sci)
        {
            bool result = false;
            if (ImGui.BeginChildFrame((uint)sceneName.GetHashCode(), new Vector2(0, 175), WindowFlags.ShowBorders))
            {
                if (ImGui.BeginChildFrame(0, new Vector2(150, 0), WindowFlags.Default))
                {
                    if (ImGui.Button(sceneName, new Vector2(-1, -1)))
                    {
                        result = true;
                    }
                }
                ImGui.EndChildFrame();
                ImGui.SameLine();
                if (ImGui.BeginChildFrame(1, new Vector2(-1, -0), WindowFlags.Default))
                {
                    ImGui.Text($"Points: {sci.MaxPointsCollected} / {sci.MaxPointsPossible}");
                    ImGui.Text($"Any%%: {sci.FastestCompletionAny} seconds");
                    ImGui.Text($"100%%: {sci.FastestCompletionFull} seconds");
                }
                ImGui.EndChildFrame();
            }
            ImGui.EndChildFrame();

            return result;
        }
    }
}
