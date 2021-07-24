using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;
}

public class Define
{
    public enum Weapons
    {
        Empty,
        Sword,
        Bow,
        Staff, 
        Spear, 
    }

    public enum FacingDirection
    {
        UP = 270,
        DOWN = 90,
        LEFT = 180,
        RIGHT = 0,

    }

    public enum ControllerState
    {
        Idle,
        Move,
        Skill,
        Death,
    }
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
}
