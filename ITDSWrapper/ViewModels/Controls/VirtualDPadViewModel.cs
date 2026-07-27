using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Input;
using ITDSWrapper.Haptics;
using ITDSWrapper.Input;

namespace ITDSWrapper.ViewModels.Controls;

public class VirtualDPadViewModel(double width, double height) : ViewModelBase
{
    private const double BoundaryBuffer = Math.PI / 180.0 * 5.0;
    private const double FullCircle = 2 * Math.PI;
    private const double SectorRotation = Math.PI / 180.0 * 205.0;
    
    private readonly Dictionary<int, AvaloniaPointerState> _pointerStates = [];
    private int _lastPointerId = -1;
    private Vec2 _currentVector = new(0, 0, 75, 75);
    private double _currentSectorMinAngle;
    private double _currentSectorMaxAngle;
    
    public VirtualButtonViewModel? Up { get; set; }
    public VirtualButtonViewModel? Right { get; set; }
    public VirtualButtonViewModel? Down { get; set; }
    public VirtualButtonViewModel? Left { get; set; }

    public double Width { get; set; } = width;
    public double Height { get; set; } = height;
    
    private double CenterX => Width / 2;
    private double CenterY => Height / 2;
    
    private double DeadZoneTopLeftX => CenterX - 0.2 * Width;
    private double DeadZoneTopLeftY => CenterY - 0.2 * Height;
    private double DeadZoneBottomRightX => CenterX + 0.2 * Width;
    private double DeadZoneBottomRightY => CenterY + 0.2 * Height;

    public void Update(int id, PointerPoint point)
    {
        if (!_pointerStates.TryGetValue(id, out AvaloniaPointerState? state))
        {
            _pointerStates.Add(id, new(point.Position.X, point.Position.Y));
            _lastPointerId = id;
        }
        else
        {
            state.Update(point);
        }

        if (_lastPointerId < 0)
            return;

        if (_pointerStates[_lastPointerId].X >= DeadZoneTopLeftX &&
            _pointerStates[_lastPointerId].Y >= DeadZoneTopLeftY &&
            _pointerStates[_lastPointerId].X <= DeadZoneBottomRightX &&
            _pointerStates[_lastPointerId].Y <= DeadZoneBottomRightY)
            return;

        Vec2 newVector = new(_pointerStates[_lastPointerId].X, _pointerStates[_lastPointerId].Y, CenterX, CenterY);
        double angle = ((newVector.AngleFromTopLeft - SectorRotation) % FullCircle + FullCircle) % FullCircle;
        if (Math.Abs(_currentSectorMinAngle - _currentSectorMaxAngle) > double.Epsilon &&
            AngleWithin(angle, _currentSectorMinAngle - BoundaryBuffer, _currentSectorMaxAngle + BoundaryBuffer))
            return;

        _currentVector = newVector;
        Console.WriteLine($"({_currentVector.X}, {_currentVector.Y}) Angle: {RadToDeg(angle)}");
        if (angle >= DegToRad(0) && angle <= DegToRad(45))
        {
            _currentSectorMinAngle = DegToRad(0);
            _currentSectorMaxAngle = DegToRad(45);
            Up?.AssociatedInput?.Press(Up);
            Down?.AssociatedInput?.Release(Down);
            Right?.AssociatedInput?.Release(Right);
            Left?.AssociatedInput?.Press(Left);
        }
        else if (angle >= DegToRad(45) && angle <= DegToRad(90))
        {
            _currentSectorMinAngle = DegToRad(45);
            _currentSectorMaxAngle = DegToRad(90);
            Up?.AssociatedInput?.Press(Up);
            Down?.AssociatedInput?.Release(Down);
            Right?.AssociatedInput?.Release(Right);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (angle >= DegToRad(90) && angle <= DegToRad(135))
        {
            _currentSectorMinAngle = DegToRad(90);
            _currentSectorMaxAngle = DegToRad(135);
            Up?.AssociatedInput?.Press(Up);
            Down?.AssociatedInput?.Release(Down);
            Right?.AssociatedInput?.Press(Right);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (angle >= DegToRad(135) && angle <= DegToRad(180))
        {
            _currentSectorMinAngle = DegToRad(135);
            _currentSectorMaxAngle = DegToRad(180);
            Up?.AssociatedInput?.Release(Up);
            Down?.AssociatedInput?.Release(Down);
            Right?.AssociatedInput?.Press(Right);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (angle >= DegToRad(180) && angle <= DegToRad(225))
        {
            _currentSectorMinAngle = DegToRad(180);
            _currentSectorMaxAngle = DegToRad(225);
            Up?.AssociatedInput?.Release(Up);
            Down?.AssociatedInput?.Press(Down);
            Right?.AssociatedInput?.Press(Right);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (angle >= DegToRad(225) && angle <= DegToRad(270))
        {
            _currentSectorMinAngle = DegToRad(225);
            _currentSectorMaxAngle = DegToRad(270);
            Up?.AssociatedInput?.Release(Up);
            Down?.AssociatedInput?.Press(Down);
            Right?.AssociatedInput?.Release(Right);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (angle >= DegToRad(270) && angle <= DegToRad(315))
        {
            _currentSectorMinAngle = DegToRad(270);
            _currentSectorMaxAngle = DegToRad(315);
            Up?.AssociatedInput?.Release(Up);
            Down?.AssociatedInput?.Press(Down);
            Right?.AssociatedInput?.Release(Right);
            Left?.AssociatedInput?.Press(Left);
        }
        else if (angle >= DegToRad(315) && angle < DegToRad(360))
        {
            _currentSectorMinAngle = DegToRad(315);
            _currentSectorMaxAngle = DegToRad(360);
            Up?.AssociatedInput?.Release(Up);
            Down?.AssociatedInput?.Release(Down);
            Right?.AssociatedInput?.Release(Right);
            Left?.AssociatedInput?.Press(Left);
        }
        else
        {
            return;
        }
            
        Up?.Haptics?.Fire(true);
    }

    public void Release(int id)
    {
        _pointerStates.Remove(id);
        if (_pointerStates.Count == 0)
        {
            _lastPointerId = -1;
            _currentVector = new(75.0, 75.0, CenterX, CenterY);
            _currentSectorMinAngle = 0.0;
            _currentSectorMaxAngle = 0.0;
            Up?.AssociatedInput?.Release(Up);
            Right?.AssociatedInput?.Release(Right);
            Down?.AssociatedInput?.Release(Down);
            Left?.AssociatedInput?.Release(Left);
        }
        else if (id == _lastPointerId)
        {
            _lastPointerId = _pointerStates.Keys.Last();
        }
    }

    private static bool AngleWithin(double angle, double min, double max)
    {
        double span = ((max - min) % FullCircle + FullCircle) % FullCircle;
        double offset = ((angle - min) % FullCircle + FullCircle) % FullCircle;
        return offset <= span;
    }

    private double DegToRad(double angle) => Math.PI / 180 * angle;
    private double RadToDeg(double angle) => 180 / Math.PI * angle;
}

public record Vec2(double X, double Y, double CenterX, double CenterY)
{
    public double AngleFromTopLeft => Math.Atan2(Y - CenterY, X - CenterX);
}