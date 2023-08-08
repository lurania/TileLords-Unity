using UnityEngine;
using System.Collections;

using DataModel.Common;
using System.Reactive.Subjects;
using System;
using System.Reactive.Linq;



public class GPSUnity : MonoBehaviour
{

    public Subject<GPS> gps = new Subject<GPS>();

    private void Start()
    {
        StartCoroutine(StartGPS());
    }

    IEnumerator StartGPS()
    {
        

        // First, check if user has location service enabled
        yield return new WaitForSeconds(3);
        if (!Input.location.isEnabledByUser)
        {
            print("LOCATION NOT ENABLED");
            yield break;
        }
        print("LOCATION ENABLED");
        // Start service before querying location
        Input.location.Start(1, 0.1f);
        Input.compass.enabled = true;


        //wait for service to go from stopped to started
        while (Input.location.status == LocationServiceStatus.Stopped)
        {
            print("Status: " + Input.location.status + "        waiting....");
            yield return new WaitForSeconds(1);
        }

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }




        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            StartCoroutine(GPSQuery());
        }

        
    }

    IEnumerator GPSQuery()
    {
        while (true)
        {

            // Access granted and location value could be retrieved
            gps.OnNext(new GPS(Input.location.lastData.latitude, Input.location.lastData.longitude));
           
            yield return new WaitForSeconds(5);
        }





    }


}