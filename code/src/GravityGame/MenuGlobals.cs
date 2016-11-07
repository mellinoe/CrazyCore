using ImGuiNET;

namespace GravityGame
{
    public static class MenuGlobals
    {
        private static int s_menusOpen;

        public static void PushMenuOpened()
        {
            s_menusOpen++;
        }

        public static void PopMenuOpened()
        {
            s_menusOpen--;
        }

        public static int NumMenusOpen => s_menusOpen;

        public static Font MenuFont { get; set; }
    }
}
