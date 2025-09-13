using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class BallThrow
{
    public TypeOfBowler bowlerType;
    public Side bowlerSide;
    public BallType ballType;
    public BallLine ballLine;
    public BallLength ballLength;

    override public string ToString()
    {
        return $"Type of Bowler: {bowlerType}\nBowler Side: {bowlerSide}\nBall Type: {ballType}\nBall Line: {ballLine} \nBall Length: {ballLength}";
    }
}


public enum BallType
{
    Straight,
    OutSwinger,
    InSwinger,
    SlowerBall,
    OffCutter,
    LegCutter,
    Doosra,
    ArmBall,
    TopSpinner,
    Googly,
    Slider,
    Flipper,
    LegSpin,
    OffSpin
}
public enum TypeOfBowler
{
    Fast,
    OffSpin,
    LegSpin,
    Medium
}

public enum BallLength
{
    FullLength,
    GoodLength,
    Short,
    Yorker
}

public enum BallLine
{
    WayOutsideOff,
    OutsideOff,
    OffStump,
    MiddleStump,
    LegStump,
    DowntheLeg,
    WayDownLeg,
}
public enum BattingTiming
{
    Poor = 4,
    Average = 3,
    Good = 2,
    VeryGood = 1,
    Perfect = 0,
}
public enum Side
{
    LeftArm,
    RightArm
}