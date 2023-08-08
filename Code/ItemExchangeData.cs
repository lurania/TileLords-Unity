using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModel.Common.Messages;
using UnityEngine.UI;
using DataModel.Common.GameModel;

public class ItemExchangeData : MonoBehaviour
{
    public int amount { get; set; }
    public ResourceType type { get; set; }

    public Image image { get; set; }
}
