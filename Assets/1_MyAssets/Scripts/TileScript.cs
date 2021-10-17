using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    /*  테트리스 타일에 들어가야 할 것
        1. 색상
    */
    private SpriteRenderer spriteRenderer;

    public Sprite[] spriteItem;

    public bool isItem = false;
    private int itemIndex;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(int index)
    {
        itemIndex = index;
        spriteRenderer.sprite = spriteItem[itemIndex];
        isItem = true;
    }

    //public void ItemCheck()
    //{
    //    if(isItem)
    //        GetComponentInParent<BoardScript>().GetItem(itemIndex);
    //}

    private void OnDestroy()
    {
        if (isItem)
            GetComponentInParent<BoardScript>().GetItem(itemIndex);
    }

}

