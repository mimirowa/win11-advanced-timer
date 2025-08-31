using Microsoft.UI.Xaml.Controls;

namespace AdvancedTimer.App;

public sealed partial class TimerHudView : UserControl
{
    public TimerHudView()
    {
        this.InitializeComponent();
    }

    public TextBlock CountdownText => CountdownTextBlock;
    public Button PauseButton => PauseButtonControl;
    public Button ResumeButton => ResumeButtonControl;
    public Button RestartButton => RestartButtonControl;
    public Button CancelButton => CancelButtonControl;
    public CheckBox TopMostToggle => TopMostToggleControl;
}
