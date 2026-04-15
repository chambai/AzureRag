using MudBlazor;

namespace Ui.Themes;

public static class ChatAiTheme
{
    public static MudTheme Theme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#4F8CFF",
            Secondary = "#7C4DFF",

            Background = "#121212",
            Surface = "#1E1E1E",

            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#1E1E1E",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0B0B0"
        }
    };
}
