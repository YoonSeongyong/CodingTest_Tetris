using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoGhost : MonoBehaviour
{

    public void SetItemToChild(int childIdx, int itemIndex)
    {
        transform.GetChild(childIdx).GetComponentInChildren<TileScript>().SetSprite(itemIndex);
    }
}
