using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Unitwee_DialogNode : ScriptableObject
{
    public string DisplayName;
    public string Contents;
    public List<Unitwee_DialogNode> Options = new List<Unitwee_DialogNode>();
    public List<Unitwee_Extension> extensions;
    [HideInInspector] public string[] stringOpts; //for finding Options
    [HideInInspector] public string filename;
    
}

/// <summary>
/// Need every node to be able to store additional data like VA lines or portraits? Add to this struct!
/// </summary>
[System.Serializable]
public struct Unitwee_Extension
{
    public AudioClip clip;
}