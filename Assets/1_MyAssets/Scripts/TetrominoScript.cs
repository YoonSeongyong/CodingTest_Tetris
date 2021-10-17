using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoScript : MonoBehaviour
{
    public Sprite spriteItem;
    public BoardScript board;
    public TetrominoGhost ghost;

    private Vector3 prePos;
    private Quaternion preRot;
    private float lockDelay=0f;
    private bool timeTicking = false;
    // Start is called before the first frame update
    void Start()
    {
        prePos = transform.position;
        preRot = transform.rotation;

    }


    // Update is called once per frame
    void Update()
    {
        if((prePos == transform.position) && (preRot == transform.rotation))
        {
            timeTicking = true;
            lockDelay += Time.deltaTime;
            if (lockDelay >= 1f)
            {
                GetComponentInParent<BoardScript>().isMoving = false;
                lockDelay = 0;
            }
        }
        else
        {
            GetComponentInParent<BoardScript>().isMoving = true;
            lockDelay = 0;
        }
        prePos = transform.position;
        preRot = transform.rotation;
    }

    public void SetItemToChild(int itemIdx)
    {
        int rand = Random.Range(0, 4);

        transform.GetChild(rand).GetComponentInChildren<TileScript>().SetSprite(itemIdx);

        // Sprite 을 지정해도 플레이 화면상에선 null로 뜸
        //ghost.SetItemToChild(rand, itemIdx);

    }

}
