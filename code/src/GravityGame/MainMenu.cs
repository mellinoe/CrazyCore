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
        private string[] _displayModeOptions =
        {
            "Normal",
            "Exclusive Fullscreen",
            "Borderless Fullscreen"
        };
        private string[] _graphicsBackEndOptions =
        {
            "Default",
            "Direct3D11",
            "OpenGL"
        };

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
            LoadFont();

            _gs.ImGuiRenderer.RecreateFontDeviceTexture(_gs.Context);

            _allScenes = _assetSystem.Database.GetAssetsOfType(typeof(SceneAsset));
        }

        private unsafe void LoadFont()
        {
            if (MenuGlobals.MenuFont == null)
            {
                using (var stream = _assetSystem.Database.OpenAssetStream("Fonts/Itim-Regular.ttf"))
                {
                    byte[] fontBytes = new byte[stream.Length];
                    using (var copyTarget = new MemoryStream(fontBytes))
                    {
                        stream.CopyTo(copyTarget);
                        fixed (byte* bytePtr = fontBytes)
                        {
                            _font = ImGui.GetIO().FontAtlas.AddFontFromMemoryTTF(new IntPtr(bytePtr), (int)stream.Length, 48);
                            MenuGlobals.MenuFont = _font;
                        }
                    }
                }
            }
            else
            {
                _font = MenuGlobals.MenuFont;
            }
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
            if (ImGui.Button("Controls", new Vector2(250, 60)))
            {
                _menuFunc = DrawControlsPage;
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
            float renderQuality = GravityGamePreferences.Instance.RenderQuality;
            if (ImGui.DragFloat("Render Quality", ref renderQuality, 0.3f, 1.0f, 0.05f))
            {
                renderQuality = MathUtil.Clamp(renderQuality, 0.3f, 1.0f);
                GravityGamePreferences.Instance.RenderQuality = renderQuality;
                _gs.RenderQuality = renderQuality;
            }
            if (ImGui.IsLastItemHovered())
            {
                ImGui.SetTooltip("Changes the game's render quality by modifying the engine's internal resolution.");
            }

            int currentIndex = (int)GravityGamePreferences.Instance.WindowStatePreference;
            if (ImGui.Combo("Display Mode", ref currentIndex, _displayModeOptions))
            {
                var newState = (InitialWindowStatePreference)currentIndex;
                if (GravityGamePreferences.Instance.WindowStatePreference != newState)
                {
                    GravityGamePreferences.Instance.WindowStatePreference = newState;
                    _gs.Context.Window.WindowState = GraphicsPreferencesUtil.MapPreferencesState(newState);
                }

            }

            currentIndex = (int)GravityGamePreferences.Instance.BackEndPreference;
            if (ImGui.Combo("Graphics Backend", ref currentIndex, _graphicsBackEndOptions))
            {
                var newState = (GraphicsBackEndPreference)currentIndex;
                if (GravityGamePreferences.Instance.BackEndPreference != newState)
                {
                    GravityGamePreferences.Instance.BackEndPreference = newState;
                }
            }
            if (ImGui.IsLastItemHovered())
            {
                ImGui.SetTooltip("Requires a restart to take effect.");
            }
        }

        private void DrawControlsPage()
        {
            ImGui.Columns(2, "ControlsColumns", true);

            ImGui.Text("Forward"); ImGui.NextColumn(); ImGui.Text("W"); ImGui.NextColumn();
            ImGui.Text("Left"); ImGui.NextColumn(); ImGui.Text("A"); ImGui.NextColumn();
            ImGui.Text("Right"); ImGui.NextColumn(); ImGui.Text("S"); ImGui.NextColumn();
            ImGui.Text("Backward"); ImGui.NextColumn(); ImGui.Text("D"); ImGui.NextColumn();
            ImGui.Text("Move Camera"); ImGui.NextColumn(); ImGui.Text("Mouse"); ImGui.NextColumn();
            ImGui.Text("Jump (Powerup)"); ImGui.NextColumn(); ImGui.Text("Space"); ImGui.NextColumn();
            ImGui.Text("Boost (Powerup)"); ImGui.NextColumn(); ImGui.Text("Shift"); ImGui.NextColumn();
            ImGui.Text("Red Magnet (Powerup)"); ImGui.NextColumn(); ImGui.Text("Left Mouse"); ImGui.NextColumn();
            ImGui.Text("Blue Magnet (Powerup)"); ImGui.NextColumn(); ImGui.Text("Right Mouse"); ImGui.NextColumn();
            ImGui.Text("Open Menu"); ImGui.NextColumn(); ImGui.Text("Escape"); ImGui.NextColumn();
            ImGui.Text("Restart Level"); ImGui.NextColumn(); ImGui.Text("R (With Menu Open)"); ImGui.NextColumn();
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
