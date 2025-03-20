using System;
[Serializable]
public class BreedResponse
{
    public BreedData data;
}
[Serializable]
public class DataObject // Новый класс
{
    public string id;
    public string type;
}
[Serializable]
public class BreedData
{
    public string id;
    public string type;
    public BreedAttributes attributes;
    public BreedRelationships relationships;
    public BreedLinks links;
}
[Serializable]
public class BreedAttributes
{
    public string name;
    public string description;
    public LifeSpan life;
    public Weight male_weight;
    public Weight female_weight;
    public bool hypoallergenic;
}
[Serializable]
public class LifeSpan
{
    public int max;
    public int min;
}
[Serializable]
public class Weight
{
    public int max;
    public int min;
}
[Serializable]
public class BreedRelationships
{
    public GroupData group;
}
[Serializable]
public class GroupData
{
    public DataObject group;
}
[Serializable]
public class Group
{
    public string id;
    public string type;
}
[Serializable]
public class BreedLinks
{
    public string self;
}