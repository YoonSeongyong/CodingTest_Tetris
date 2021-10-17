using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour
{
    public GameObject tilePefab;
    public GameObject borderTilePrefab;
    public GameObject boardTilePrefab;
    public GameObject itemSlot;
    public GameObject panelGameOver, panelPressStart;


    public Sprite[] tetrominoSprite;

    public Transform tetromino, boardParent, grid, tetromino_ghost;     // 컨트롤할 테트로미노의 부모 오브젝트, 게임 보드 오브젝트
    public Transform tetrominoHold, tetrominoNext;                      // 홀드 위치용 부모 오브젝트

    public GameObject[,] boardGrid;                                     // 보드판 배열 {0 : 없음, 1 : 있음}

    public Text textScore;

    public AudioClip[] clips;
    //public GameObject[,] boardGrid;                                   // 보드판 배열 {0 : 없음, 1 : 있음}


    public int boardWidth, boardHeight;                                 // 보드 크기
    public float fallingTerm;
    public bool isMoving = false;

    private int hWidth, hHeight, curIndex, holdIndex, nextIndex;        // 보드 좌,우, 높이 계산용 변수
    private int ownedItem, score;                                              // 얻은 아이템

    private int rotate = 0;                                             // 0 : 회전x , 1 : 시계방향, -1 : 반시계방향
    private Vector3 moveDir = Vector3.zero;

    private float deltaTime = 0f, resetTime = 0f;
    private bool isHold = false, isFirstHold = true;


    private Queue<int> tetrominoQueue = new Queue<int>();
    private Queue<Transform> holdQueue = new Queue<Transform>();
    private Sprite bombImage;

    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        rotate = 0;
        moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.R))
        {
            resetTime += Time.deltaTime;
            if (resetTime >= 1.5f)
            {
                GameFlowManager.Instance.gameFlow = Flow.PAUSE;
                resetTime = 0;
                ResetGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            StartGame();

        ///////////////////////

        if (GameFlowManager.Instance.gameFlow == Flow.PAUSE) return;

        ///////////////////////


        //TEST
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    GetItem(0);
        //}
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            itemSlot.GetComponentInChildren<Item>().Activate(ownedItem);
            ownedItem = -1;
        }
        //

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDir = Vector3.left;
            isMoving = true;
            AudioManager.Instance.PlayClip(clips[2]);
            //tetromino.transform.position += Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDir = -Vector3.left;
            isMoving = true;
            AudioManager.Instance.PlayClip(clips[2]);
            //tetromino.transform.position -= Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDir = Vector3.down;
            AudioManager.Instance.PlayClip(clips[2]);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            rotate = 1;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            rotate = -1;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            // 홀드
            HoldTetromino();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = false;
            AudioManager.Instance.PlayClip(clips[0]);
            while (ControlTetromino(Vector3.down, 0))
            {
            }
        }



        if (moveDir != Vector3.zero || rotate != 0)
        {
            ControlTetromino(moveDir, rotate);
        }

        FallTetromino();
        ControlGhost();

    }

    private void ResetGame()
    {
        Initialize();
    }

    private void StartGame()
    {
        panelPressStart.SetActive(false);
        GameFlowManager.Instance.gameFlow = Flow.PLAY;
    }

    public void GetItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0:                                                             // 한줄폭탄
                ownedItem = itemIndex;
                itemSlot.GetComponentInChildren<Item>().GetItem(itemIndex);
                //itemSlot.GetComponentInChildren<ItemShaker>().Activate();
                break;
        }

    }

    void Initialize()
    {
        hWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        hHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        score = 0;

        boardGrid = new GameObject[boardHeight + 2, boardWidth];
        //Debug.Log(boardGrid[0, 0]);
        isMoving = false;

        panelPressStart.SetActive(true);
        panelGameOver.SetActive(false);

        for (int i = 0; i < tetromino.childCount; i++)
        {
            Destroy(tetromino.GetChild(i).gameObject);
            Destroy(tetromino_ghost.GetChild(i).gameObject);
        }
        for(int i=0;i< grid.childCount;i++)
        {
            Destroy(grid.GetChild(i).gameObject);
        }

        ownedItem = -1;
        itemSlot.GetComponentInChildren<Item>().Initialize();

        CreateBoard();
        Create7Bag();

        CreateTetrominoGhost(tetromino_ghost, tetrominoQueue.Peek(), 0.4f);
        CreateTetromino(tetromino, tetrominoQueue.Dequeue(), 1.0f);
        ShowNext();
        HoldInit();
    }

    void HoldInit()
    {
        for (int i = 0; i < 7; i++)
        {
            //CreateTetromino(tetrominoHold.GetChild(i),i);
            tetrominoHold.GetChild(i).gameObject.SetActive(false);
        }
    }

    void FallTetromino()
    {
        Vector3 prePos = tetromino.position;
        deltaTime += Time.deltaTime;
        if (deltaTime >= fallingTerm)
        {
            ControlTetromino(Vector3.down, 0);

            deltaTime = 0;
        }
    }

    void ItemRoulette()
    {
        int rand = Random.Range(0, 10);


        switch (rand)
        {
            case 3:
                tetromino.GetComponent<TetrominoScript>().SetItemToChild(0);
                break;
            default:
                //Debug.Log("Fail to Get Item");
                break;
        }
    }

    bool ControlTetromino(Vector3 _moveDir, int _rotate)
    {
        Vector3 prePos = tetromino.position;
        Quaternion preRot = tetromino.rotation;

        tetromino.position += _moveDir;
        tetromino.rotation *= Quaternion.Euler(0, 0, 90 * _rotate);

        // 이동 제한
        if (!MoveCheck(tetromino))
        {
            tetromino.position = prePos;
            tetromino.rotation = preRot;

            //Debug.Log(stopTime); 
            //stopTime += Time.deltaTime;

            if (!isMoving && (_moveDir.y == -1 && _moveDir.x == 0 && _rotate == 0))
            {
                EndTurn();
                //Debug.Log("블럭 추가"); 

                if (!MoveCheck(tetromino))
                {
                    // 게임 오버
                    panelGameOver.SetActive(true);
                    textScore.text = score.ToString();
                    GameOver();
                }
            }
            return false;
        }
        return true;
    }

    void ControlGhost()
    {
        var prePos = tetromino_ghost.position = tetromino.position;
        var preRot = tetromino_ghost.rotation = tetromino.rotation;

        while (true)
        {
            tetromino_ghost.position += Vector3.down;

            if (!MoveCheck(tetromino_ghost))
            {
                tetromino_ghost.position = prePos;
                tetromino_ghost.rotation = preRot;
                break;
            }
            else
            {
                prePos = tetromino_ghost.position;
                preRot = tetromino_ghost.rotation;
            }
        }

        //if (!MoveCheck(tetromino_ghost))
        //{
        //    tetromino_ghost.position = prePos;
        //    tetromino_ghost.rotation = preRot;
        //    return false;
        //}
    }

    private void GameOver()
    {
        GameFlowManager.Instance.gameFlow = Flow.PAUSE;
        AudioManager.Instance.PlayClip(clips[4]);
    }

    private void ShowNext()
    {
        nextIndex = tetrominoQueue.Peek();
        for (int i = 0; i < tetrominoNext.childCount; i++)
        {
            tetrominoNext.GetChild(i).gameObject.SetActive(false);
        }
        tetrominoNext.GetChild(nextIndex).gameObject.SetActive(true);
    }

    void HoldTetromino()
    {
        /*
         *      시작할 때 : 홀드 부모 객체 위치에 7개의 테트로미노를 생성, 비활성화 시켜둠
         *      첫 홀드일 경우 : 현재 테트로미노 인덱스를 홀드 인덱스로 변경, 7bag 큐에서 dequeue한 후 테트로미노 생성
         *      그 이후 홀드일 경우 : 현재 테트로미노 인덱스와 홀드 인덱스 Swap, 테트로미노 자식 객체들을 삭제, 7bag 큐에서 dequeue한 후 테트로미노 생성
         */
        if (isHold) return;

        isHold = !isHold;

        if (isFirstHold)
        {
            isFirstHold = false;
            holdIndex = curIndex;
            //CreateTetromino(tetrominoQueue.Dequeue());
            tetrominoHold.GetChild(holdIndex).gameObject.SetActive(true);

            for (int i = 0; i < tetromino.childCount; i++)
            {
                Destroy(tetromino.GetChild(i).gameObject);
            }
            tetromino.DetachChildren();
            //curIndex = ;\

            DeleteGhost();
            CreateTetrominoGhost(tetromino_ghost, tetrominoQueue.Peek(), 0.4f);
            CreateTetromino(tetromino, tetrominoQueue.Dequeue(), 1.0f);
            if (tetrominoQueue.Count == 0)
                Create7Bag();
            ItemRoulette();
            ShowNext();


        }
        else
        {
            for (int i = 0; i < tetromino.childCount; i++)
            {
                Destroy(tetromino.GetChild(i).gameObject);
            }

            tetrominoHold.GetChild(holdIndex).gameObject.SetActive(false);

            int temp = holdIndex;
            holdIndex = curIndex;
            curIndex = temp;


            tetrominoHold.GetChild(holdIndex).gameObject.SetActive(true);

            DeleteGhost();
            CreateTetrominoGhost(tetromino_ghost, curIndex, 0.4f);   // 고스트
            CreateTetromino(tetromino, curIndex, 1.0f);   // 컨트롤용
        }
    }

    public void EndTurn()
    {
        AddBoard();
        CheckBoardColumn();
        //curIndex = tetrominoQueue.Peek();

        DeleteGhost();
        CreateTetrominoGhost(tetromino_ghost, tetrominoQueue.Peek(), 0.4f);
        CreateTetromino(tetromino, tetrominoQueue.Dequeue(), 1);
        ItemRoulette();
        //Debug.Log("큐 : " + tetrominoQueue.Count);
        if (tetrominoQueue.Count == 0)
            Create7Bag();
        ShowNext();
    }

    void DeleteGhost()
    {
        for (int i = 0; i < tetromino_ghost.childCount; i++)
        {
            Destroy(tetromino_ghost.GetChild(i).gameObject);
        }
    }

    void AddBoard()
    {
        // 테트로미노 노드를 나눠서 그리드에 적용

        while (tetromino.childCount > 0)
        {
            var temp = tetromino.GetChild(0);
            int tX = Mathf.RoundToInt(temp.position.x + hWidth);
            int tY = Mathf.RoundToInt(temp.position.y + hHeight - 1);

            //tetromino.GetChild(0).GetComponent<TileScript>().SetGridPos(tX, tY);
            //Debug.Log("tX : " + tX)
            boardGrid[tY, tX] = temp.gameObject;
            temp.parent = grid;
        }
        isHold = false;
    }

    // 전체 보드 체크
    void CheckBoardColumn()
    {
        bool isPlay = false;
        for (int y = boardHeight - 1; y >= 0; y--)
        {
            if (CheckColumn(y))
            {
                DeleteColumn(y);
                DownRow(y);
                ScoreUP();
                if (!isPlay)
                {
                    AudioManager.Instance.PlayClip(clips[3]);
                    isPlay = true;
                }
            }
        }
    }

    public void ScoreUP()
    {
        score++;
    }

    // 가로줄 체크
    bool CheckColumn(int _y)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            if (boardGrid[_y, x] == null)
                return false;
        }
        return true;
    }

    // 가로 한 줄 삭제
    void DeleteColumn(int _y)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            Destroy(boardGrid[_y, x].gameObject);
            boardGrid[_y, x] = null;
        }
    }

    // 윗 줄을 아래 줄로 내리는 함수
    void DownRow(int _y)
    {
        for (int y = _y; y < boardHeight - 1; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {

                if (boardGrid[y + 1, x] == null)
                    continue;

                boardGrid[y, x] = boardGrid[y + 1, x];
                boardGrid[y + 1, x].transform.position += Vector3.down;
                boardGrid[y + 1, x] = null;
                //boardGrid[y, x] = temp;
            }
        }
    }

    public void CreateTile(GameObject prefab, Transform parent, Vector2 position, int layer)
    {
        var temp = Instantiate(prefab, position, Quaternion.identity);
        temp.transform.parent = parent;
        temp.transform.localPosition = position;
        temp.GetComponent<SpriteRenderer>().sortingOrder = layer;
    }

    public void CreateTile(GameObject prefab, Transform parent, Vector2 position, Sprite sprite, float trans)
    {
        var temp = Instantiate(prefab, position, Quaternion.identity);
        temp.transform.parent = parent;
        temp.transform.localPosition = position;
        temp.GetComponent<SpriteRenderer>().sprite = sprite;
        temp.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, trans);
    }

    bool MoveCheck(Transform _tetro)
    {
        for (int i = 0; i < _tetro.childCount; i++)
        {
            var temp = _tetro.GetChild(i);

            if (temp.position.x <= -hWidth - 1 || temp.position.x >= hWidth)
            {
                //Debug.Log("좌우 무브 체크");
                return false;
            }
            //else
            //{
            //    Debug.Log(temp.po/*s*/ition.x + ",     " + (-hWidth-1));

            //}

            int tX = Mathf.RoundToInt(temp.position.x + hWidth);
            int tY = Mathf.RoundToInt(temp.position.y + hHeight - 1);
            if ((temp.position.y <= -hHeight || boardGrid[tY, tX] != null))
            {
                //Debug.Log("상하 무브 체크");
                //isMoving = false;
                return false;
            }
        }
        return true;
    }




    private void Create7Bag()
    {
        bool[] randQ = new bool[7];

        for (int i = 0; i < 7; i++)
        {
            int n = Random.Range(0, 7);
            if (!randQ[n])
            {
                //Debug.Log(randQ[n]);
                tetrominoQueue.Enqueue(n);
                randQ[n] = true;
            }
            else
            {
                i -= 1;
                continue;
            }
        }
    }

    public void CreateTetromino(Transform _parent, int rand, float trans)
    {
        //int rand = Random.Range(0, 7);
        tetromino.position = new Vector2(0, hHeight - 1);
        tetromino.rotation = Quaternion.identity;
        curIndex = rand;
        switch (rand)
        {
            case 0:                 // I mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-2, 0), tetrominoSprite[rand], trans);
                break;

            case 1:                 // O Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 1), tetrominoSprite[rand], trans);
                break;

            case 2:                 // Z Mino
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, -1), tetrominoSprite[rand], trans);

                break;

            case 3:                 // S Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, -1), tetrominoSprite[rand], trans);

                break;

            case 4:                 // J Mino
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 1), tetrominoSprite[rand], trans);

                break;

            case 5:                 // L Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 1), tetrominoSprite[rand], trans);

                break;

            case 6:                 // T Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                break;
        }

    }

    public void CreateTetrominoGhost(Transform _parent, int rand, float trans)
    {
        //int rand = Random.Range(0, 7);
        tetromino.position = new Vector2(0, hHeight - 1);
        tetromino.rotation = Quaternion.identity;
        switch (rand)
        {
            case 0:                 // I mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-2, 0), tetrominoSprite[rand], trans);
                break;

            case 1:                 // O Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 1), tetrominoSprite[rand], trans);
                break;

            case 2:                 // Z Mino
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, -1), tetrominoSprite[rand], trans);

                break;

            case 3:                 // S Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, -1), tetrominoSprite[rand], trans);

                break;

            case 4:                 // J Mino
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 1), tetrominoSprite[rand], trans);

                break;

            case 5:                 // L Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(1, 1), tetrominoSprite[rand], trans);

                break;

            case 6:                 // T Mino
                CreateTile(tilePefab, _parent, new Vector2(1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(-1, 0), tetrominoSprite[rand], trans);
                CreateTile(tilePefab, _parent, new Vector2(0, -1), tetrominoSprite[rand], trans);
                break;
        }

    }

    public void CreateBoard()
    {
        int index = 0;


        for (int x = -hWidth; x < hWidth; x++)
        {
            for (int y = -hHeight; y < hHeight; y++)
            {
                CreateTile(boardTilePrefab, boardParent, new Vector2(x, y),0);
            }
        }

        // 좌,우 테두리
        for (int y = -hHeight; y < hHeight; y++)
        {
            CreateTile(borderTilePrefab, boardParent, new Vector2(-hWidth - 1, y),1);
            CreateTile(borderTilePrefab, boardParent, new Vector2(hWidth, y),1);

            index++;
        }


        //// 아래 테두리
        for (int x = -hWidth - 1; x <= hWidth; x++)
        {
            CreateTile(borderTilePrefab, boardParent, new Vector2(x, -hHeight),1);
        }

    }



}
