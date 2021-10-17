using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(AudioManager)) as AudioManager;
                if (!_instance)
                {
                    Debug.Log("No Singleton obj");
                }
            }
            return _instance;
        }
    }
    public AudioClip[] clips;

    private AudioSource audioSource;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip _clip)
    {
        audioSource.PlayOneShot(_clip);
    }

}


