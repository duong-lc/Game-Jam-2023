using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LaneColorInstance", menuName = "LaneColorInstance", order = 0)]
public class LaneColorChangeDictionary : SerializedScriptableObject
{
    public Dictionary<float, LaneColorInstance> TimeToLaneColorInst;
}

[Serializable]
public class LaneColorInstance
{
    public float time;
    public NoteColorEnum upperLaneColor;
    public NoteColorEnum lowerLaneColor;
}
