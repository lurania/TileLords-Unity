using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
///A class to hold all the data about a quest
/// </summary>
public class QuestU : MonoBehaviour
{
    

    private string questText;
    private string statusText;
    private string distance;
    private int questTrackerID;
    private byte[] questID;
    private ContentType questLevel;
    private PlusCode trackedLocation;
    private PlusCode handInLocation;
    private bool isTracked = false;
    public TMP_Text assignedTextBox;
    int requiredAmount;
    int currentAmount;
    double completionPercent;
    ResourceType typeToPickUp;

    public bool marked = false;
    public GameObject marker;

    public ResourceType TypeToPickUp { get => typeToPickUp; set => typeToPickUp = value; }
    public int RequiredAmount { get => requiredAmount; set => requiredAmount = value; }

    public int CurrentAmount { get => currentAmount; set => currentAmount = value; }
    public double CompletionPercent { get => completionPercent; set => completionPercent = value; }
    public string QuestText { get => questText; set => questText = value; }
    public string StatusText { get => statusText; set => statusText = value; }
    public string Distance { get => distance; set => distance = value; }
    public int QuestTrackerID { get => questTrackerID; set => questTrackerID = value; }
    public byte[] QuestID { get => questID; set => questID = value; }
    public PlusCode TrackedLocation { get => trackedLocation; set => trackedLocation = value; }

    public PlusCode HandInLocation { get => handInLocation; set => handInLocation = value; }
    public bool IsTracked { get => isTracked; set => isTracked = value; }

    public ContentType QuestLevel { get => questLevel; set => questLevel = value; }
}
