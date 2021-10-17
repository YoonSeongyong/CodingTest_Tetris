using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour
{
    public BoardScript board;
    public Sprite[] icon;
    public AudioClip clip;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize()
    {
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
    }
    public void GetItem(int itemIdx)
    {
        image.color = new Color(1, 1, 1, 1);
        image.sprite = icon[itemIdx];
    }


    public void Activate(int itemIdx)
    {
        switch (itemIdx)
        {
            case 0:
                DeleteLine();
                board.ScoreUP();
                AudioManager.Instance.PlayClip(clip);
                for (int y = 0; y <= board.boardHeight - 1; y++)
                {
                    DownRow(y);
                }
                break;

            default:
                Debug.Log("HAVE NO ITEM");
                break;
        }
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
    }

    private void DeleteLine()
    {
        for (int x = 0; x < board.boardWidth; x++)
        {
            if (board.boardGrid[0, x] == null)
                continue;

            Destroy(board.boardGrid[0, x].gameObject);
            board.boardGrid[0, x] = null;
        }
    }

    private void DownRow(int _y)
    {
        //for (int y = _y; y < board.boardHeight - 1; y++)
        //{
        for (int x = 0; x < board.boardWidth; x++)
        {
            if (board.boardGrid[_y + 1, x] == null)
                continue;


            board.boardGrid[_y, x] = board.boardGrid[_y + 1, x];
            board.boardGrid[_y + 1, x].transform.position += Vector3.down;
            board.boardGrid[_y + 1, x] = null;

        }
        //}
    }

}
