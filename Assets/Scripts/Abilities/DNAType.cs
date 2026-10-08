using UnityEngine;

// Типы DNA-способностей блоба.
public enum DNAType
{
    None = 0,
    HighJump = 1, // (Y) Yellow + Triangle
    Swim = 2,     // (B) Blue + Circle
    Fire = 3,     // (R) Red + Triangle
    Armor = 4     // (G) Green + Square
}

public static class DNATypeInfo
{
    public static string DisplayName(DNAType type)
    {
        switch (type)
        {
            case DNAType.HighJump: return "HIGH JUMP";
            case DNAType.Swim: return "SWIM";
            case DNAType.Fire: return "FIRE ATTACK";
            case DNAType.Armor: return "ARMOR";
            default: return "";
        }
    }

    public static Color GetColor(DNAType type)
    {
        switch (type)
        {
            case DNAType.HighJump: return new Color(1f, 0.85f, 0.2f);
            case DNAType.Swim: return new Color(0.25f, 0.65f, 1f);
            case DNAType.Fire: return new Color(1f, 0.35f, 0.15f);
            case DNAType.Armor: return new Color(0.35f, 1f, 0.3f);
            default: return Color.white;
        }
    }
}
