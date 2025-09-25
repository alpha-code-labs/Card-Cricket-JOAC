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
    public PitchCondition pitchCondition;
    public BallThrow() { }
    public BallThrow(TypeOfBowler _ballerType, Side _side, BallType _ballType, BallLine _ballLine, BallLength _ballLength, PitchCondition _pitchCondition)
    {
        bowlerType = _ballerType;
        bowlerSide = _side;
        ballType = _ballType;
        ballLine = _ballLine;
        ballLength = _ballLength;
        pitchCondition = _pitchCondition;
    }
    override public string ToString()
    {
        return $"Type of Bowler: {bowlerType}\nBowler Side: {bowlerSide}\nBall Type: {ballType}\nBall Line: {ballLine} \nBall Length: {ballLength}";
    }
}

public enum BallType
{
    ArmBall,
    Doosra,
    Flipper,
    Googly,
    InSwinger,
    LegSpin,
    LegCutter,
    OffSpinner,
    OffCutter,
    OutSwinger,
    Slider,
    SlowerBall,
    Straight,
    TopSpinner
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
    // WayDownLeg,
    WayDowntheLeg
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