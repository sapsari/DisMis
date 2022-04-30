using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State
{
    public enum StateType
    {
        Clueless,
        Disinformed,
        Misinformed,
        Wellinformed,
    }

    public StateType Type;
    public bool IsActive;
    public int Score;
}
