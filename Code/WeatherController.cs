using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class handling weather and day night cycle
/// </summary>
public class WeatherController : MonoBehaviour
{


    public MeshRenderer groundRenderer;
    public GameObject fog;
    public GameObject sun;
    public Material snowMaterial;
    public Material rainMaterial;
    public GameObject rainEffect;
    public GameObject snowEffect;
    public GameObject fireFlyEffect;


    float timeElapsed = 0;
    float duration = 86400;

    float rotation;
    Color sunColor;
    Color32[] colors;

    Light sunlight;

    public static bool raining = false;
    public static bool snowing = false;




    public int testhour = 12, testminute = 0, testsecond = 0;
    public bool useFakeTime = false;


    void Start()
    {


        sunColor = sun.GetComponent<Light>().color;
        sunlight = sun.GetComponent<Light>();
        fog.SetActive(false);
        rainEffect.SetActive(false);
        snowEffect.SetActive(false);
        groundRenderer.gameObject.SetActive(false);



        colors = new Color32[10];
        colors[0] = new Color32(236, 122, 255, 255);//1
        colors[1] = new Color32(255, 131, 122, 255);//2
        colors[2] = new Color32(255, 112, 0, 255);//3
        colors[3] = new Color32(255, 210, 0, 255);//4   
        colors[4] = new Color32(255, 255, 209, 255);//5
        colors[5] = new Color32(239, 251, 181, 255);//4
        colors[6] = new Color32(255, 210, 0, 255);//4
        colors[7] = new Color32(255, 112, 0, 255);//3
        colors[8] = new Color32(255, 131, 122, 255);//2
        colors[9] = new Color32(236, 122, 255, 255);//1



    }


    // Update is called once per frame
    void Update()
    {


        if (Input.GetKeyDown("f1"))
        {
            fog.SetActive(false);
            groundRenderer.gameObject.SetActive(true);
            groundRenderer.material = rainMaterial;
            rainEffect.SetActive(true);
            snowEffect.SetActive(false);
            fireFlyEffect.SetActive(false);
            raining = true;
            snowing = false;
        }
        if (Input.GetKeyDown("f2"))
        {
            fog.SetActive(false);
            groundRenderer.gameObject.SetActive(true);
            groundRenderer.material = snowMaterial;
            snowEffect.SetActive(true);
            rainEffect.SetActive(false);
            fireFlyEffect.SetActive(false);
            raining = false;
            snowing = true;
        }
        if (Input.GetKeyDown("f3"))
        {
            groundRenderer.gameObject.SetActive(true);
            groundRenderer.material = snowMaterial;
            fog.SetActive(true);
            snowEffect.SetActive(false);
            rainEffect.SetActive(false);
            fireFlyEffect.SetActive(false);
        }
        if (Input.GetKeyDown("f4"))
        {
            groundRenderer.gameObject.SetActive(false);
            fog.SetActive(false);
            snowEffect.SetActive(false);
            rainEffect.SetActive(false);
            fireFlyEffect.SetActive(false);
            snowing = false;
            raining = false;
        }
        if (Input.GetKeyDown("f5"))
        {
            groundRenderer.gameObject.SetActive(false);
            fireFlyEffect.SetActive(true);
            fog.SetActive(false);
            snowEffect.SetActive(false);
            rainEffect.SetActive(false);
            snowing = false;
            raining = false;
        }




        if (timeElapsed < duration)
        {


            rotation = Mathf.Lerp(0f, 360f, timeElapsed / duration);



        }
        else
        {


            timeElapsed = 0;

        }


        //sunrise
        if (timeElapsed < 81000 && timeElapsed > 80000)
        {
            sunlight.intensity = 0.1f;
            sunlight.color = colors[0];
        }
        if (timeElapsed < 82000 && timeElapsed > 81000)
        {
            sunlight.intensity = 0.2f;
            sunlight.color = colors[0];
        }
        if (timeElapsed < 83000 && timeElapsed > 82000)
        {
            sunlight.intensity = 0.3f;
            sunlight.color = colors[1];
        }
        if (timeElapsed < 84000 && timeElapsed > 83000)
        {
            sunlight.intensity = 0.4f;
            sunlight.color = colors[1];
        }
        if (timeElapsed < 85000 && timeElapsed > 84000)
        {
            sunlight.intensity = 0.5f;
            sunlight.color = colors[2];
        }
        if (timeElapsed < 86000 && timeElapsed > 85000)
        {
            sunlight.intensity = 0.6f;
            sunlight.color = colors[2];
        }
        if (timeElapsed < 1000 && timeElapsed > 0)
        {
            sunlight.intensity = 0.7f;
            sunlight.color = colors[3];
        }
        if (timeElapsed < 2000 && timeElapsed > 1000)
        {
            sunlight.intensity = 0.8f;
            sunlight.color = colors[4];
        }
        if (timeElapsed < 3000 && timeElapsed > 2000)
        {
            sunlight.intensity = 0.9f;
            sunlight.color = colors[4];
        }
        if (timeElapsed < 4000 && timeElapsed > 3000)
        {
            sunlight.intensity = 1f;
            sunlight.color = colors[5];
        }

        //sunset
        if (timeElapsed < 36000 && timeElapsed > 35000)
        {
            sunlight.intensity = 1f;
            sunlight.color = colors[5];
        }
        if (timeElapsed < 37000 && timeElapsed > 36000)
        {
            sunlight.intensity = 0.9f;
            sunlight.color = colors[4];
        }
        if (timeElapsed < 38000 && timeElapsed > 37000)
        {
            sunlight.intensity = 0.8f;
            sunlight.color = colors[4];
        }
        if (timeElapsed < 39000 && timeElapsed > 38000)
        {
            sunlight.intensity = 0.7f;
            sunlight.color = colors[3];
        }
        if (timeElapsed < 40000 && timeElapsed > 39000)
        {
            sunlight.intensity = 0.6f;
            sunlight.color = colors[3];
        }
        if (timeElapsed < 41000 && timeElapsed > 40000)
        {
            sunlight.intensity = 0.5f;
            sunlight.color = colors[3];
        }
        if (timeElapsed < 42000 && timeElapsed > 41000)
        {
            sunlight.intensity = 0.4f;
            sunlight.color = colors[2];
        }
        if (timeElapsed < 43000 && timeElapsed > 42000)
        {
            sunlight.intensity = 0.3f;
            sunlight.color = colors[2];
        }
        if (timeElapsed < 44000 && timeElapsed > 43000)
        {
            sunlight.intensity = 0.2f;
            sunlight.color = colors[1];
        }
        if (timeElapsed < 45000 && timeElapsed > 44000)
        {
            sunlight.intensity = 0.1f;
            sunlight.color = colors[0];
        }









        //night
        if (timeElapsed > 45000 && timeElapsed < 80000)
        {
            sunlight.intensity = 0;

        }

        //day
        else if (timeElapsed > 4000 && timeElapsed < 35000)
        {
            sunlight.intensity = 1;

        }

        sun.transform.rotation = Quaternion.Euler(rotation, 0, 0);


        if (useFakeTime)
        {
            timeElapsed = SecondsPassedSince(testhour, testminute, testsecond);
        }
        else
        {
            timeElapsed = SecondsPassedSince(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
        }



    }

    public void LetItSnow()
    {
        fog.SetActive(false);
        groundRenderer.gameObject.SetActive(true);
        groundRenderer.material = snowMaterial;
        snowEffect.SetActive(true);
        rainEffect.SetActive(false);
        fireFlyEffect.SetActive(false);
        raining = false;
        snowing = true;
    }
    public void LetItRain()
    {
        fog.SetActive(false);
        groundRenderer.gameObject.SetActive(true);
        groundRenderer.material = rainMaterial;
        rainEffect.SetActive(true);
        snowEffect.SetActive(false);
        fireFlyEffect.SetActive(false);
        raining = true;
        snowing = false;
    }
    public void MakeItClear()
    {
        groundRenderer.gameObject.SetActive(false);
        fog.SetActive(false);
        snowEffect.SetActive(false);
        rainEffect.SetActive(false);
        fireFlyEffect.SetActive(false);
        snowing = false;
        raining = false;
    }
    public void MakeItFly()
    {
        groundRenderer.gameObject.SetActive(false);
        fireFlyEffect.SetActive(true);
        fog.SetActive(false);
        snowEffect.SetActive(false);
        rainEffect.SetActive(false);
        snowing = false;
        raining = false;
    }

    public void MakeItFoggy()
    {
        groundRenderer.gameObject.SetActive(true);
        groundRenderer.material = snowMaterial;
        fog.SetActive(true);
        snowEffect.SetActive(false);
        rainEffect.SetActive(false);
        fireFlyEffect.SetActive(false);

    }




    /// <summary>
    /// Function to calculate how many seconds have passed since 8 am (sunrise) to set the cycle for day night properly
    /// </summary>
    public float SecondsPassedSince(int hours, int minutes, int seconds)
    {


        if (hours >= 0 && hours < 8)
        {
            var differenceBeforeMidnight = 57600; //difference between 8am and 0am in secs
            var differenceAfterMidnight = Math.Abs(DateTime.Parse("0:00:00").Subtract(DateTime.Parse(hours + ":" + minutes + ":" + seconds)).TotalSeconds);
            var totalsec = differenceBeforeMidnight + differenceAfterMidnight;

            return (float)totalsec;
        }
        else
        {

            var differenceBeforeMidnight = Math.Abs(DateTime.Parse("8:00:00").Subtract(DateTime.Parse(hours + ":" + minutes + ":" + seconds)).TotalSeconds);

            return (float)differenceBeforeMidnight;
        }



    }




}
