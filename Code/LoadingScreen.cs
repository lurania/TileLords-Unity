using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    

    public GameObject worldUICanvas;
    public GameObject loadingScreenCanvas;

    public MapHandler cubeSpawner;
    public AudioController audioController;
    bool cubeSpawnerReady = false;
    bool audioControllerReady = false;
    public bool skipLoading = false;



    public void Start()
    {
        if (skipLoading)
        {
            StartLoading();
        }

        loadingScreenCanvas.SetActive(false);  
  
    }
    public void StartLoading()
    {
        if (!skipLoading)
        {
            loadingScreenCanvas.SetActive(true);
            System.Diagnostics.Debug.WriteLine("Loading");
            worldUICanvas.SetActive(false);
            cubeSpawner.LoadAndSubscribe();
            StartCoroutine(audioController.LoadMusic());
            StartCoroutine(CheckStatus());
        }
        else
        {
            Debug.Log("Skipped Loading");
            cubeSpawner.LoadAndSubscribe();
            StopLoading();
            
        }


    }

    public void StopLoading()
    {

        Debug.Log("Loading done!");
        worldUICanvas.SetActive(true);
        loadingScreenCanvas.SetActive(false);
       

    }

    public void CubeSpawnerToggleReady()
    {
        cubeSpawnerReady = true;
        Debug.Log("CubeSpawner ready!");
    }
    public void AudioControllerToggelReady()
    {
        audioControllerReady = true;
        Debug.Log("Audio ready!");
    }


    IEnumerator CheckStatus()
    {

        while (!(audioControllerReady && cubeSpawnerReady))
        {
            yield return new WaitForSeconds(1);
        }

        StopLoading();

    }

 
}
