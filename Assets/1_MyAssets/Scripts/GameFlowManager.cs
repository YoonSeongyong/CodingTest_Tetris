using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Flow { PLAY = 0, PAUSE }

public class GameFlowManager : MonoBehaviour
{

    private static GameFlowManager _instance;
    public static GameFlowManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(GameFlowManager)) as GameFlowManager;
                if (!_instance)
                {
                    Debug.Log("No Singleton obj");
                }
            }

            return _instance;
        }
    }

    public Flow gameFlow;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    private void Start()
    {
        gameFlow = Flow.PAUSE;
    }

}
