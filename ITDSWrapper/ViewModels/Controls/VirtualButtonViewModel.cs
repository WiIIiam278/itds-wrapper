using ITDSWrapper.Input;

namespace ITDSWrapper.ViewModels.Controls;

public class VirtualButtonViewModel(string? label, GameInput? associatedInput, double width, double height) : ViewModelBase
{
    public string? Label { get; set; } = label;
    public GameInput? AssociatedInput { get; set; } = associatedInput;

    public double Width { get; set; } = width;
    public double Height { get; set; } = height;
}