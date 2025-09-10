using ITDSWrapper.Haptics;
using ITDSWrapper.Input;

namespace ITDSWrapper.ViewModels.Controls;

public class VirtualButtonViewModel(string? label, VirtualButtonInput? associatedInput, double width, double height, IHapticsBackend haptics) : ViewModelBase
{
    public string? Label { get; set; } = label;
    public VirtualButtonInput? AssociatedInput { get; set; } = associatedInput;

    public double Width { get; set; } = width;
    public double Height { get; set; } = height;

    public IHapticsBackend Haptics { get; set; } = haptics;
}