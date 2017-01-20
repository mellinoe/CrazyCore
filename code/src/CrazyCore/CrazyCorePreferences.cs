using Engine;
using Engine.Graphics;
using System.IO;
using System;

namespace CrazyCore
{
    public class CrazyCorePreferences : PersistentStorage<CrazyCorePreferences, CrazyCorePreferences.StorageInfo>, GraphicsPreferencesProvider
    {
        private float _renderQuality = 1.0f;
        public float RenderQuality
        {
            get
            {
                return MathUtil.Clamp(_renderQuality, .3f, 1f);
            }
            set
            {
                _renderQuality = MathUtil.Clamp(value, 0.3f, 1f);
                Save();
            }
        }

        private InitialWindowStatePreference _windowStatePreference = InitialWindowStatePreference.ExclusiveFullScreen;
        public InitialWindowStatePreference WindowStatePreference
        {
            get { return _windowStatePreference; }
            set
            {
                _windowStatePreference = value;
                Save();
            }
        }

        private GraphicsBackEndPreference _backEndPreference;
        public GraphicsBackEndPreference BackEndPreference
        {
            get
            {
                return _backEndPreference;
            }
            set
            {
                _backEndPreference = value;
                Save();
            }
        }

        public static void GameInit(Game game)
        {
        }

        public class StorageInfo : PersistentStorageInfo
        {
            public string StoragePath => Path.Combine("CrazyCore", "Preferences");
        }
    }
}
