using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UnityFunc
{
    public static IObservable<GPS> GPSCreated(GPSUnity source)
    {
        return source.gps;
    }

    





}
