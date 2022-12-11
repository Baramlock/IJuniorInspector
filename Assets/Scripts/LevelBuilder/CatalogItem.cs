using System.Collections.Generic;
using UnityEngine;

public class CatalogItem
{
    public readonly string Name;
    public readonly List<GameObject> Item;

    public CatalogItem(string name, List<GameObject> items)
    {
        Name = name;
        Item = items;
    }
}