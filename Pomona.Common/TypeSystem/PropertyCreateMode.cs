namespace Pomona.Common.TypeSystem
{
    public enum PropertyCreateMode
    {
        Excluded, // Default for all generated properties.
        Optional, // Default for all publicly writable properties, 
        Required, // Default for properties that got a matching argument in shortest constructor
    }
}