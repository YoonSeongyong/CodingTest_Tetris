//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ItemShaker : Item
//{
//    public override void Activate()
//    {
//        DeleteLine();
//        for (int y = 0; y <= board.boardHeight - 1; y++)
//        {
//            DownRow(y);
//        }
//    }

//    private void DeleteLine()
//    {
//        for (int x = 0; x < board.boardWidth; x++)
//        {
//            if (board.boardGrid[0, x] == null)
//                continue;

//            Destroy(board.boardGrid[0, x].gameObject);
//            board.boardGrid[0, x] = null;
//        }
//    }

//    private void DownRow(int _y)
//    {
//        //for (int y = _y; y < board.boardHeight - 1; y++)
//        //{
//        for (int x = 0; x < board.boardWidth; x++)
//        {
//            if (board.boardGrid[_y + 1, x] == null)
//                continue;


//            board.boardGrid[_y, x] = board.boardGrid[_y + 1, x];
//            board.boardGrid[_y + 1, x].transform.position += Vector3.down;
//            board.boardGrid[_y + 1, x] = null;

//        }
//        //}
//    }


//}
