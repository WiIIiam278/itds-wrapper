using ITDSWrapper.Haptics;

namespace ITDSWrapper.ViewModels.Controls;

public class VirtualMultiButtonViewModel(string? label, VirtualButtonViewModel[] buttons, double width, double height, IHapticsBackend? haptics) : ViewModelBase
{
    public string? Label { get; set; } = label;
    public VirtualButtonViewModel[] Buttons { get; set; } = buttons;

    public double Width { get; set; } = width;
    public double Height { get; set; } = height;

    public IHapticsBackend? Haptics { get; set; } = haptics;
}