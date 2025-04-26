using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Unity.IO;
/// <summary>
/// This script contains the main backing logic and functions behind the Unitwee System.
/// Started by PBChandler on 4.25.25.
/// </summary>
public class Unitwee
{
    /// <summary>
    /// For use in the Editor.
    /// </summary>
    [MenuItem("Unitwee/Convert Twee File")]
    static void ConvertTweeFile()
    {
        //Open a window, allow user to select a twee file.
        string path = EditorUtility.OpenFilePanel("Select a Twee File", "", "twee");

        if (!string.IsNullOrEmpty(path))
        {
            string fullTwee = File.ReadAllText(path);
            string[] fileLines = StringToLineArray(fullTwee);

            DialogNode activeDialogNode = new DialogNode();
            List<DialogNode> dialogNodes = new List<DialogNode>();
            string buildingContents = "";
            List<string> buildingOptions = new List<string>();
            List<int> buildingOptionsIDs = new List<int>();
            string buildingName = "";
            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = fileLines[i];

                if (line.Contains("::"))
                {
                    if (!line.Contains('{'))
                    {
                        Reset();
                        continue;
                    }

                    // TODO: Notify the user that they can't use special characters in file name, there's not really a way to avoid this.
                    buildingName = line.Substring(line.IndexOf("::") + 2, line.IndexOf('{') - 2).Trim();
                    if (buildingName != "")
                    {
                        Reset();
                        activeDialogNode.idOptions = new List<int>();
                        activeDialogNode.Name = buildingName;
                        if(line.Contains("|"))
                        {
                            activeDialogNode.DisplayName = line.Substring(0, line.IndexOf('|'));
                        }
                        else
                        {
                            activeDialogNode.DisplayName = activeDialogNode.Name;
                        }
                        for(int j = i+1; j < fileLines.Length; j++)
                        {
                            if (fileLines[j].Contains("::"))
                            {
                                j = fileLines.Length;
                            }
                            else if(fileLines[j].Contains("[["))
                            {
                                buildingOptions.Add(fileLines[j]);
                                buildingOptionsIDs.Add(j);
                            }
                            else
                            {
                                buildingContents += fileLines[j];
                            }
                        }
                        activeDialogNode.Contents = buildingContents;
                        activeDialogNode.Options = buildingOptions.ToArray();
                        dialogNodes.Add(activeDialogNode);
                        activeDialogNode.idOptions = buildingOptionsIDs;
                        activeDialogNode.id = i;
                        Reset();
                    }
                }
            }
            foreach (DialogNode wow in dialogNodes)
            {
                string consolidatedOptions = "";
                for (int i = 0; i < wow.Options.Length; i++)
                {
                    consolidatedOptions += wow.Options[i] + "\n";
                }
                Debug.Log(wow.Name + " + " + wow.Contents + consolidatedOptions);
            }

            Unitwee_DialogSequence sequence = ScriptableObject.CreateInstance<Unitwee_DialogSequence>();
            for(int i = 0; i < dialogNodes.Count; i++)
            {
                Unitwee_DialogNode NewNode = StructNodeToClassNode(dialogNodes[i]);

                string finalPath = ("Assets/Resources/Unitwee/TranscribedDialog/" + "Unitwee" + i + ".asset").Trim();
                AssetDatabase.CreateAsset(NewNode, finalPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //"cooldict" is probably an unprofessional filename, but any alternatives I wanted to come up with felt more confusing than just giving this one a proper noun.
            Dictionary<string, Unitwee_DialogNode> coolDict = new Dictionary<string, Unitwee_DialogNode>();
            for (int i = 0; i < dialogNodes.Count; i++)
            {
                Unitwee_DialogNode a = Resources.Load<Unitwee_DialogNode>("Unitwee/TranscribedDialog/Unitwee" + i);
                coolDict.Add("Unitwee" + i, a);
                a.filename = "Unitwee" + i;
                sequence.nodes.Add(a);
            }
           
            AssetDatabase.CreateAsset(sequence, "Assets/Resources/Unitwee/TranscribedDialog/" + Path.GetFileName(path) + "_"+ Path.GetFileName(path) + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach(Unitwee_DialogNode parent in sequence.nodes)
            {
                parent.owner = sequence;
                for (int i = 0; i < sequence.nodes.Count; i++)
                {
                    sequence.nodes[i].id = i;
                }

                for(int j = 0; j < parent.fileOptions.Length; j++)
                {
                    for (int i = 0; i < sequence.nodes.Count; i++)
                    {
                        string h = parent.fileOptions[j];
                        if (h.Contains(sequence.nodes[i].devName+"]]"))
                        {
                            LiveOption o = new LiveOption();
                            if (h.Contains('|'))
                            {
                                h = h.Replace("[[", "");
                                o.key = h.Substring(0, h.IndexOf('|'));
                            }
                               
                            else
                                o.key = coolDict["Unitwee" + i].DisplayName;
                            o.value = coolDict["Unitwee" + i];
                            parent.Options.Add(o);
                        }
                    }
                }
                
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            /// <summary> Resets activeDialogNode and it's friends. </summary>
            void Reset()
            {
                activeDialogNode = new DialogNode();
                buildingContents = "";
                buildingOptions.Clear();
            }

            Unitwee_DialogNode StructNodeToClassNode(DialogNode import)
            {
                Unitwee_DialogNode export = ScriptableObject.CreateInstance<Unitwee_DialogNode>();
                export.name = import.Name;
                export.devName = import.Name;
                if (import.DisplayName.Contains('|'))
                    export.DisplayName = import.DisplayName.Substring(0, import.DisplayName.IndexOf('|') - 1);
                export.DisplayName = import.DisplayName;
                export.fileOptions = import.Options;
                export.Contents = import.Contents;
                export.id = import.id;
                return export;
            }
        }
    }

   

   
    /// <summary>
    /// A structure containing the basic data gained from a text file that is basically a Twine node.
    /// </summary>
    [System.Serializable]
    public struct DialogNode
    {
        public string Name;
        public string Contents;
        public string[] Options;
        public string DisplayName;
        public int id;
        public List<int> idOptions;
    }
    /// <summary>
    /// Converts a string into a line by line array based on it's use of '\n'
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    static string[] StringToLineArray(string original)
    {
        List<string> retval = new List<string>();
        string currentLine = "";

        int CharLength = original.Length;
        for (int i = 0; i < CharLength; i++)
        {
            if (original[i] == '\n')
            {
                //discards completely blank lines.
                if (!string.IsNullOrEmpty(currentLine) && !string.IsNullOrWhiteSpace(currentLine))
                    retval.Add(currentLine);
                currentLine = "";
            }
            else
                currentLine += original[i];
        }
        return retval.ToArray();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
