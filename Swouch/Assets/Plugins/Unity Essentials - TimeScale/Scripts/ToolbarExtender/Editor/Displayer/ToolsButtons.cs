using UnityEditor;

namespace unityEssentials.timeScale.toolbarExtent
{
    [InitializeOnLoad]
    public class ToolsButton
    {
        private static TimeScaleSlider _timeScaleSlider = new TimeScaleSlider();

        static ToolsButton()
        {
            _timeScaleSlider.Init();
            ToolbarExtender.LeftToolbarGUI.Add(_timeScaleSlider.DisplaySliderLeft);
            ToolbarExtender.RightToolbarGUI.Add(_timeScaleSlider.DisplaySliderRight);
        }
    }
}