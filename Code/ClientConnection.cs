
using DataModel.Client;
using DataModel.Common;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Main class handling the connection to the server
/// </summary>
public class ClientConnection : MonoBehaviour
{
    
    public ClientInstanceManager ClientInstance;

    //classes that depends on clientInstance
    public ContentUILogic contentUILogic;
    public QuestTracker questTracker;
    public Inventory inventory;

    public static byte[] ClientId { get; set; }
    public GPSUnity gps;
    public string ServerIP;
    public LoginScreen loginScreen;

    List<System.IDisposable> disposables = new List<System.IDisposable>();

    public static TMP_Text debugText;
    private void Awake()
    {
        Application.targetFrameRate = 60;
      
    }

    public void SetIPAndConnect(string ip)
    {
        Debug.Log(ip);
        Debug.Log(ServerIP.Equals(ip));
        ServerIP = ip;
        ClientInstance = new ClientInstanceManager(ServerIP);
        ClientInstance.StartClient();
 
        var DEBUG = ClientInstance.InboundTraffic.Where(v => v is InventoryContentMessage).Select(v => v as InventoryContentMessage).ObserveOnMainThread().Subscribe();
        disposables.Add(DEBUG); 
        contentUILogic.Initialise();
        questTracker.Initialise();
        inventory.Initialise();
        Debug.Log("Setting ip and connecting...");

    }

  
    private void OnDestroy()
    {
        disposables.ForEach(v => v.Dispose());
        ClientInstance.ShutDown();
    }




}
