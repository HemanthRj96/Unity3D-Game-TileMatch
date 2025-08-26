[System.Serializable]
public class IdentificationComponent : ISKUComponent
{
    public string skuID;
    public string displayName;

    public IdentificationComponent(string skuID, string displayName)
    {
        this.skuID = skuID;
        this.displayName = displayName;
    }
}