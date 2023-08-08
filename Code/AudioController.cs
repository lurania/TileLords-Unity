using DataModel.Common;
using DataModel.Common.GameModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioController : MonoBehaviour
{

    public AudioSource musicSource;
    public AudioSource soundSource;
    public AudioSource weatherSource;
    List<AudioClip> music;
    List<AudioClip> sounds;
    public LoadingScreen loadingScreen;
    public MapHandler cubeSpawner;
    public bool playMusic = false;

    TileType oldType = TileType.Grassland;
    TileType newType = TileType.Desert;
    MiniTile miniTile;
    bool playingRain = false;

    void Start()
    {
        music = new List<AudioClip>();
        sounds = new List<AudioClip>();
        musicSource.Play();

    }
    /// <summary>
    /// Function which plays the proper ambient sounds depending on the location and other influences (like rainsound in rain)
    /// </summary>
    public IEnumerator AmbientSoundPlayer()
    {

        while (true)
        {
            if (WeatherController.raining && !playingRain)
            {
                playingRain = true;
                weatherSource.clip = sounds[6];
                weatherSource.Play();
            }
            if (!WeatherController.raining && playingRain)
            {
                playingRain = false;
                weatherSource.Stop();
            }
            if (cubeSpawner.plusCodeMiniTileDict.TryGetValue(PlayerU.currentPlusCodeLocation, out MiniTileUnity miniU))
            {
                miniTile = miniU.getMiniTile();
                oldType = newType;
                newType = miniTile.ParentType;
  
                if (newType != oldType)
                {
                   

                    if (miniTile.ParentType == TileType.Desert)
                    {
                        
                        soundSource.clip = sounds[7];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Jungle)
                    {
                        soundSource.clip = sounds[Random.Range(0, 5)];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Grassland)
                    {
                        soundSource.clip = sounds[Random.Range(0, 1)];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Mountains)
                    {
                        soundSource.clip = sounds[8];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Savanna)
                    {
                        soundSource.clip = sounds[Random.Range(0, 5)];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Snow)
                    {
                        soundSource.clip = sounds[Random.Range(0, 2)];
                        soundSource.Play();
                    }
                    else if (miniTile.ParentType == TileType.Swamp)
                    {
                        soundSource.clip = sounds[Random.Range(0, 5)];
                        soundSource.Play();
                    }

                }
            }
            yield return new WaitForSeconds(5);
        }
    }


    public IEnumerator LoadMusic()
    {
        Addressables.LoadAssetsAsync<AudioClip>("Music", v => { }).Completed += OnLoadDone;
        Addressables.LoadAssetsAsync<AudioClip>("Sounds", v => { }).Completed += OnLoadDoneSounds;
        yield return null;

    }

    private void OnLoadDone(AsyncOperationHandle<IList<AudioClip>> obj)
    {
        Debug.Log(obj.Status);
        foreach (var item in obj.Result)
        {
            Debug.Log("adding" + item.name);
            music.Add(item);
        }
        loadingScreen.AudioControllerToggelReady();
        //StartCoroutine(KeepMusicPlaying());

    }

    private void OnLoadDoneSounds(AsyncOperationHandle<IList<AudioClip>> obj)
    {
        Debug.Log(obj.Status);
        foreach (var item in obj.Result)
        {
            Debug.Log("adding" + item.name);
            sounds.Add(item);
        }
        StartCoroutine(AmbientSoundPlayer());
        if (playMusic)
        {
            StartCoroutine(KeepMusicPlaying());
        }

    }


    IEnumerator KeepMusicPlaying()
    {

        while (true)
        {
            musicSource.clip = music[Random.Range(0, music.Count)];
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }
    }




}
