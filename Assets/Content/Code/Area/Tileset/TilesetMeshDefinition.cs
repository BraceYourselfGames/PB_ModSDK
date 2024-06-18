using UnityEngine;
using System.Text;

[CreateAssetMenu (fileName = "TilesetMeshDefintion", menuName = "Tileset/Mesh Definition")]
public class TilesetMeshDefinition : ScriptableObject
{
    public string tileset = "set1";
    public string prefix = "center";
    public bool use = false;
    public bool spawnFlipped = false;
    public bool helperGeometry = false;
    public GameObject prefab;

    public enum SubBlockRequirement
    {
        Irrelevant,
        Empty,
        Full
    }

    [Space (8f)]
    public SubBlockRequirement state0TopXPosZPos = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state1TopXNegZPos = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state2TopXNegZNeg = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state3TopXPosZNeg = SubBlockRequirement.Irrelevant;

    [Space (8f)]
    public SubBlockRequirement state4BottomXPosZPos = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state5BottomXNegZPos = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state6BottomXNegZNeg = SubBlockRequirement.Irrelevant;
    public SubBlockRequirement state7BottomXPosZNeg = SubBlockRequirement.Irrelevant;

    public bool skipStepAfterPlacement = false;
    public int subtypeID = 0;



    public bool[] GetMatchesWithConfiguration (bool[] configuration)
    {
        int length = spawnFlipped ? 8 : 4;
        bool[] matches = new bool[length];
        for (int i = 0; i < length; ++i)
        {
            int rotation = i % 4;
            bool flip = i > 3;
            matches[i] = CheckConfigMatchWithRequirements (configuration, GetRequirementsTransformed (rotation, flip));
        }
        return matches;
    }

    public bool CheckConfigMatchWithRequirements (bool[] configuration, SubBlockRequirement[] requirements)
    {
        bool match = true;
        for (int i = 0; i < 8; ++i)
        {
            if (!IsSubblockMatching (requirements[i], configuration[i])) match = false;
        }
        return match;
    }

    public SubBlockRequirement[] GetRequirementsTransformed (int rotation, bool flip) // 0, 1, 2, 3
    {
        SubBlockRequirement[] requirements = new SubBlockRequirement[8];

        if (!flip)
        {
            requirements[rotation] = state0TopXPosZPos;                    // 0, 1, 2, 3
            requirements[(1 + rotation) % 4] = state1TopXNegZPos;          // 1, 2, 3, 0
            requirements[(2 + rotation) % 4] = state2TopXNegZNeg;          // 2, 3, 0, 1
            requirements[(3 + rotation) % 4] = state3TopXPosZNeg;          // 3, 0, 1, 2
            requirements[4 + rotation] = state4BottomXPosZPos;             // 4, 5, 6, 7
            requirements[4 + ((1 + rotation) % 4)] = state5BottomXNegZPos; // 5, 6, 7, 4
            requirements[4 + ((2 + rotation) % 4)] = state6BottomXNegZNeg; // 6, 7, 4, 5
            requirements[4 + ((3 + rotation) % 4)] = state7BottomXPosZNeg; // 7, 4, 5, 6
        }
        else
        {
            requirements[rotation] = state1TopXNegZPos;                    // 0, 1, 2, 3
            requirements[(1 + rotation) % 4] = state0TopXPosZPos;          // 1, 2, 3, 0
            requirements[(2 + rotation) % 4] = state3TopXPosZNeg;          // 2, 3, 0, 1
            requirements[(3 + rotation) % 4] = state2TopXNegZNeg;          // 3, 0, 1, 2
            requirements[4 + rotation] = state5BottomXNegZPos;             // 4, 5, 6, 7
            requirements[4 + ((1 + rotation) % 4)] = state4BottomXPosZPos; // 5, 6, 7, 4
            requirements[4 + ((2 + rotation) % 4)] = state7BottomXPosZNeg; // 6, 7, 4, 5
            requirements[4 + ((3 + rotation) % 4)] = state6BottomXNegZNeg; // 7, 4, 5, 6
        }

        return requirements;
    }

    private bool IsSubblockMatching (SubBlockRequirement state, bool configuration)
    {
        if (state == SubBlockRequirement.Irrelevant || state == SubBlockRequirement.Empty && configuration == false || state == SubBlockRequirement.Full && configuration == true) return true;
        else return false;
    }




    // Utility

    public string RequirementsToString (SubBlockRequirement[] requirements)
    {
        StringBuilder sb = new StringBuilder ();
        string textFull = "1";
        string textEmpty = "0";
        string textIrrelevant = "x";
        for (int i = 0; i < requirements.Length; ++i)
        {
            sb.Append (requirements[i] == SubBlockRequirement.Irrelevant ? textIrrelevant : (requirements[i] == SubBlockRequirement.Empty ? textEmpty : textFull));
        }
        return sb.ToString ();
    }

    public string RequirementsToFilename (SubBlockRequirement[] requirements)
    {
        StringBuilder sb = new StringBuilder ();
        string textFull = "1";
        string textEmpty = "0";
        string textIrrelevant = "x";

        sb.Append (tileset);
        sb.Append ("_");

        sb.Append (prefix);
        sb.Append ("_(");

        int sumUpper = GetIntFromRequirement (requirements[0]) + GetIntFromRequirement (requirements[1]) + GetIntFromRequirement (requirements[2]) + GetIntFromRequirement (requirements[3]);
        if (sumUpper == 1) sb.Append ("upperwall_out_to_");
        else if (sumUpper == 2)
        {
            if
            (
                ((requirements[0] == requirements[1]) && requirements[0] == SubBlockRequirement.Full) ||
                ((requirements[1] == requirements[2]) && requirements[1] == SubBlockRequirement.Full) ||
                ((requirements[2] == requirements[3]) && requirements[2] == SubBlockRequirement.Full) ||
                ((requirements[3] == requirements[0]) && requirements[3] == SubBlockRequirement.Full)
            ) sb.Append ("upperwall_str_to_");
            else if 
            (
                requirements[0] != SubBlockRequirement.Irrelevant &&
                requirements[1] != SubBlockRequirement.Irrelevant &&
                requirements[2] != SubBlockRequirement.Irrelevant &&
                requirements[3] != SubBlockRequirement.Irrelevant
            ) sb.Append ("upperwall_diag_to_");
            else sb.Append ("upperwall_inw_to_");
        }
        else if (sumUpper == 3)
        {
            if 
            (
                requirements[0] != SubBlockRequirement.Empty &&
                requirements[1] != SubBlockRequirement.Empty &&
                requirements[2] != SubBlockRequirement.Empty &&
                requirements[3] != SubBlockRequirement.Empty
            ) sb.Append ("ceiling_to_");
            else sb.Append ("upperwall_inw_to_");
        }
        else if (sumUpper == 4) sb.Append ("ceiling_to_");
        else sb.Append ("floor_to_");

        int sumLower = GetIntFromRequirement (requirements[4]) + GetIntFromRequirement (requirements[5]) + GetIntFromRequirement (requirements[6]) + GetIntFromRequirement (requirements[7]);
        if (sumLower == 1) sb.Append ("lowerwall_out)_");
        else if (sumLower == 2)
        {
            if
            (
                ((requirements[4] == requirements[5]) && requirements[4] == SubBlockRequirement.Full) ||
                ((requirements[5] == requirements[6]) && requirements[5] == SubBlockRequirement.Full) ||
                ((requirements[6] == requirements[7]) && requirements[6] == SubBlockRequirement.Full) ||
                ((requirements[7] == requirements[4]) && requirements[7] == SubBlockRequirement.Full)
            ) sb.Append ("lowerwall_str)_");
            else if
            (
                requirements[4] != SubBlockRequirement.Irrelevant &&
                requirements[5] != SubBlockRequirement.Irrelevant &&
                requirements[6] != SubBlockRequirement.Irrelevant &&
                requirements[7] != SubBlockRequirement.Irrelevant
            ) sb.Append ("lowerwall_diag)_");
            else sb.Append ("lowerwall_inw)_");
        }
        else if (sumLower == 3)
        {
            if
            (
                requirements[4] != SubBlockRequirement.Empty &&
                requirements[5] != SubBlockRequirement.Empty &&
                requirements[6] != SubBlockRequirement.Empty &&
                requirements[7] != SubBlockRequirement.Empty
            ) sb.Append ("floor)_");
            else sb.Append ("lowerwall)_");
        }
        else if (sumLower == 4) sb.Append ("floor)_");
        else sb.Append ("ceiling)_");

        sb.Append (spawnFlipped ? "flip_" : "keep_");

        for (int i = 0; i < requirements.Length; ++i)
        {
            sb.Append (requirements[i] == SubBlockRequirement.Irrelevant ? textIrrelevant : (requirements[i] == SubBlockRequirement.Empty ? textEmpty : textFull));
            if (i == 3) sb.Append ("_");
        }
        return sb.ToString ();
    }

    private int GetIntFromRequirement (SubBlockRequirement requirement)
    {
        return requirement == SubBlockRequirement.Full ? 1 : 0;
    }
}
