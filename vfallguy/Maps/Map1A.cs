using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using vfallguy.Helpers;
using vfallguy.Hooks;
using vfallguy.Maps;

namespace vfallguy;

public class Map1A : Map
{
    private List<AOESequence> _mech1Rotating = new();
    private List<AOESequence> _mech1Pillars = new();
    private List<AOESequence> _mech2Pendulums = new();
    private List<AOESequence> _mech3Pillars = new();

    private static PathfindMap BuildBaseMap()
    {
        var map = new PathfindMap(0.5f, 0.25f, new(0, 0, 223), 14, 101, 60);

        // Define specific map blocks here
        // Example: columns, central columns, lane columns, prisms, trapezium, corners

        return map;
    }

    private static void BlockPrism(PathfindMap m, float cx, float cy, float s)
    {
        var center = new Vector2(cx, cy);
        var vs = new Vector2(s);
        m.BlockPixelsInside(center - vs, center + vs, v => Math.Abs(v.X - cx) + Math.Abs(v.Y - cy) <= s, 0, 60, 100, 0);
    }

    private static void BlockTrapezium(PathfindMap m, float dx1, float y1, float dx2, float y2)
    {
        float coeff = (dx2 - dx1) / (y2 - y1);
        float cons = dx1 - y1 * coeff;
        m.BlockPixelsInside(new(-dx2, y2), new(dx2, y1), v => Math.Abs(v.X) < cons + coeff * v.Y, 0, 60, 100, 0);
    }

    private static void BlockCorners(PathfindMap m, float x1, float y1, float x2, float y2, float x3, float y3)
    {
        var a = new Vector2(x1, y1);
        var b = new Vector2(x2, y2);
        var c = new Vector2(x3, y3);
        var ab = b - a;
        var bc = c - b;
        var n1 = new Vector2(ab.Y, -ab.X);
        var n2 = new Vector2(bc.Y, -bc.X);
        m.BlockPixelsInside(new(-x1, y3), new(x1, y1), v => Vector2.Dot(n1, new(Math.Abs(v.X) - x1, v.Y - y1)) < 0 && Vector2.Dot(n2, new(Math.Abs(v.X) - x2, v.Y - y2)) < 0, 0, 60, 100, 0);
    }

    private static List<(float z, float y)> _heightProfile = new()
    {
        // Define your height profile similar to Map3
        (135.6f, 36.2f),
        (139.0f, 35.5f),
        (139.1f, 34.5f),
        (143.0f, 34.3f),
        (143.1f, 33.6f),
        (147.0f, 33.4f),
        (147.1f, 32.8f),
        (150.9f, 32.5f),
        (151.0f, 31.9f),
        (180.7f, 28.8f),
        (218.9f, 15.1f),
        (229.7f, 14.5f),
        (236.8f, 13.5f),
        (262.9f, 6.0f),
        (273.2f, 6.0f),
        (286.9f, 3.2f),
    };

    public Map1A(GameEvents events) : base(events, BuildBaseMap(), _heightProfile, 123)
    {
        for (int i = 0; i < 5; ++i)
        {
            _mech1Rotating.Add(CreateRotatingSequence(new Vector3(-10, 10, -202 - 10 * i)));
            _mech1Rotating.Add(CreateRotatingSequence(new Vector3(10, 10, -202 - 10 * i)));
            if ((i & 1) == 0)
                _mech1Rotating.Add(CreateRotatingSequence(new Vector3(0, 10, -202 - 10 * i)));
        }

        _mech1Pillars.Add(CreatePillarSequence(2.1f, 10, -248.5f, 20.58f, -17.6f, 2.98f));
        _mech1Pillars.Add(CreatePillarSequence(2.1f, 10, -251.5f, 19.71f, -3, 16.71f));

        _mech2Pendulums.Add(CreatePendulumSequence(-281, false));
        _mech2Pendulums.Add(CreatePendulumSequence(-288, true));
        _mech2Pendulums.Add(CreatePendulumSequence(-301, false));
        _mech2Pendulums.Add(CreatePendulumSequence(-308, true));
        _mech2Pendulums.Add(CreatePendulumSequence(-321, false));
        _mech2Pendulums.Add(CreatePendulumSequence(-328, true));

        _mech3Pillars.Add(CreatePillarSequence(1.6f, 13.67f, -347.8f, 3.5f, -6.88f, -3.77f, 0, 3.74f, 6.86f));
        _mech3Pillars.Add(CreatePillarSequence(1.6f, 15.97f, -355.8f, 3.5f, -6.88f, -3.50f, 0, 3.50f, 6.85f));
        _mech3Pillars.Add(CreatePillarSequence(1.6f, 18.15f, -363.8f, 3.5f, -6.88f, -3.77f, 0, 3.74f, 6.86f));
    }

    protected override void OnActionEffect(uint actionId, Vector3 casterPos)
    {
        switch (actionId)
        {
            case 34773:
                UpdateSequence(casterPos, 0, _mech1Rotating);
                break;
            case 34774:
                UpdateSequence(casterPos, 0, _mech1Pillars.Concat(_mech3Pillars));
                break;
            case 34775:
                UpdateSequence(casterPos, 0, _mech2Pendulums);
                break;
        }
    }

    private AOESequence CreateRotatingSequence(Vector3 pos)
    {
        float[] delays = new float[] { 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 3.3f };
        return CreateSequence(AOEShape.Circle, 5, 0, delays.Select(d => (pos, 0.0f, d)));
    }

    private AOESequence CreatePillarSequence(float delay, float y, float z, float length, params float[] xs)
    {
        List<(Vector3, float, float)> l = new();
        for (int i = 0; i < xs.Length - 1; ++i)
            l.Add((new Vector3(xs[i], y, z), MathF.PI * 0.5f, delay));
        for (int i = 0; i < xs.Length - 1; ++i)
            l.Add((new Vector3(xs[xs.Length - 1 - i], y, z), -MathF.PI * 0.5f, delay));
        return CreateSequence(AOEShape.Rect, length, 1.5f, l);
    }

    private AOESequence CreatePendulumSequence(float z, bool left)
    {
        List<(Vector3, float, float)> l = new()
        {
            (new Vector3(left ? -5 : 5, 10, z), 0, 0.2f),
            (new Vector3(left ? -2 : 2, 10, z), 0, 0.1f),
            (new Vector3(left ? 1 : -1, 10, z), 0, 1.8f),
            (new Vector3(left ? 1 : -1, 10, z), 0, 0.2f),
            (new Vector3(left ? -2 : 2, 10, z), 0, 0.1f),
            (new Vector3(left ? -5 : 5, 10, z), 0, 1.8f)
        };
        return CreateSequence(AOEShape.Square, 1.5f, 0, l);
    }
}
