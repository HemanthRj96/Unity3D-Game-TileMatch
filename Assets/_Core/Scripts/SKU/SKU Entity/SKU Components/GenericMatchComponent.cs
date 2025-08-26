using UnityEngine;

[System.Serializable]
public class GenericMatchComponent : ISKUComponent
{
    public int value;
    public Color color;

    public GenericMatchComponent(int value, Color color)
    {
        this.value = value;
        this.color = color;
    }
}