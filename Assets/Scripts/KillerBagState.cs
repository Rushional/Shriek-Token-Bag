using System;
using System.Collections.Generic;
using System.Linq;

public enum Killer
{
    BigBadWolf,
    Hans,
    DrFright,
    Geppetto,
    RatchetLady,
    Baghead,
    HUNTER,
    Razorface,
    Tormentor,
    Krampus
}

public class KillerBagState
{
    private List<Killer> bagKillersList = new List<Killer>();
    private List<Killer> removedKillersList = new List<Killer>();
    private Killer? currentKiller;

    public void Reset()
    {
        bagKillersList = Enum.GetValues(typeof(Killer))
            .Cast<Killer>()
            .ToList();

        removedKillersList = new List<Killer>();
        currentKiller = null;
    }

    public bool CanDrawToken => currentKiller == null && bagKillersList.Count > 0;
    public bool HasCurrentKiller => currentKiller.HasValue;
    public bool HasRemovedKillers => removedKillersList.Count > 0;

    public void DrawRandomKiller()
    {
        if (!CanDrawToken)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, bagKillersList.Count);
        currentKiller = bagKillersList[randomIndex];
    }

    public void PutBackCurrentKiller()
    {
        currentKiller = null;
    }

    public void RemoveCurrentKiller()
    {
        removedKillersList.Add(currentKiller.Value);
        bagKillersList.Remove(currentKiller.Value);
        currentKiller = null;
    }

    public void RestoreRemovedKiller(Killer killer)
    {
        removedKillersList.Remove(killer);
        bagKillersList.Add(killer);
    }

    public string GetCurrentDisplayName()
    {
        return currentKiller.HasValue ? GetDisplayName(currentKiller.Value) : string.Empty;
    }

    public static string GetDisplayName(Killer killer)
    {
        switch (killer)
        {
            case Killer.BigBadWolf:
                return "Big Bad Wolf";
            case Killer.Hans:
                return "Hans";
            case Killer.DrFright:
                return "Dr. Fright";
            case Killer.Geppetto:
                return "Geppetto";
            case Killer.RatchetLady:
                return "Ratchet Lady";
            case Killer.Baghead:
                return "Baghead";
            case Killer.HUNTER:
                return "H.U.N.T.E.R.";
            case Killer.Razorface:
                return "Razorface";
            case Killer.Tormentor:
                return "Tormentor";
            case Killer.Krampus:
                return "Krampus";
            default:
                return killer.ToString();
        }
    }

    public List<Killer> GetRemovedKillersList()
    {
        return removedKillersList;
    }

    public List<Killer> GetBagKillersList()
    {
        return bagKillersList;
    }

    public Killer? GetCurrentKiller() 
    {
        return currentKiller;
    }
}
