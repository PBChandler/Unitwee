using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Unitwee_DialogNode : ScriptableObject
{
    public string DisplayName;
    public string Contents;
    public List<LiveOption> Options = new List<LiveOption>();
    public List<Unitwee_Extension> extensions;
    public string[] fileOptions;
    public int id;
  public List<int> idOptions = new List<int>();
    [HideInInspector] public string filename;
   public string devName;
    public Unitwee_DialogSequence owner;
}
[System.Serializable]
public struct LiveOption
{
    public string key;
    public Unitwee_DialogNode value;
}
/// <summary>
/// Need every node to be able to store additional data like VA lines or portraits? Add to this struct!
/// </summary>
[System.Serializable]
public struct Unitwee_Extension
{
    public AudioClip clip;
}