using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField]
    private Sprite TileImg;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private Vector2 SpriteSize;

    [SerializeField]
    private int ColNum = 0;

    [SerializeField]
    private int RowNum = 0;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private int bigBallStartNum;

    [SerializeField]
    private const int smallBallRandomNum = 3;

    [SerializeField]
    private float bigBallScale;

    [SerializeField]
    private float smallBallScale;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private Color[] colors;

    private GameObject[,] Tiles;

    private Vector2Int? selectedTilePos = new Vector2Int();

    private bool isBallMoved;

    int ballCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        isBallMoved = true;

        if (Tiles == null)
        {
            MakeTiles();
            MakeStartingBall();
            ballCount = smallBallRandomNum + bigBallStartNum;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var tilePos = GetTilePosByMousePos(Input.mousePosition);

            if (tilePos == null)
            {
                return;
            }

            var clickedTile = GetTileAt(tilePos.Value.x, tilePos.Value.y);
            var currentTileCmp = clickedTile.GetComponent<Tile>();
            if (currentTileCmp.state == Tile.State.HaveBigBall)
            {
                selectedTilePos = tilePos;
                
            }
            else
            {
                if (selectedTilePos != null)
                {
                        var numEmptyTileleft = (ColNum * RowNum) - ballCount;
                    // nếu vị trí để di chuyển có bóng nhỏ thì di chuyển bóng nhỏ sang vị trí random mới và làm lớn nó
                    if (clickedTile.transform.childCount > 0)
                    {
                        var ball = clickedTile.transform.GetChild(0);
                        if (currentTileCmp.state == Tile.State.HaveSmallBall && numEmptyTileleft > 0)
                        {
                            int x, y;
                            RandomPos(out x, out y);
                            var tile = GetTileAt(x, y);
                            ball.transform.position = tile.transform.position;
                            ball.transform.localScale = bigBallScale * Vector3.one;
                            ball.SetParent(tile.transform);
                            tile.GetComponent<Tile>().state = Tile.State.HaveBigBall;

                        }
                        
                    }
                  
                    // tạo bóng nhỏ random nếu thỏa mãn điều kiện
                    if (numEmptyTileleft >= 3)
                    {
                        IsBallMoved();
                        MakeRandomBalls(smallBallRandomNum, smallBallScale);
                        ballCount = ballCount + 3;

                    }
                    else if (numEmptyTileleft > 0)
                    {
                        IsBallMoved();
                        MakeRandomBalls(numEmptyTileleft, smallBallScale);
                        ballCount = ballCount + numEmptyTileleft;
                    }
                }
                selectedTilePos = null;
            }
        }
    }

    void MakeStartingBall()
    {
        MakeRandomBalls(bigBallStartNum, bigBallScale);
        MakeRandomBalls(smallBallRandomNum, smallBallScale);
    }

    void MakeRandomBalls(int numBalls, float scale)
    {
        for (int i = 0; i < numBalls; i++)
        {
            int x , y;
            RandomPos(out x, out y);
            var tile = GetTileAt(x, y);

            var ballPos = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z - 0.1f);
            var ball = Instantiate<GameObject>(ballPrefab);
            ball.transform.localScale = Vector3.one * scale;
            ball.transform.position = ballPos;
            ball.transform.SetParent(tile.transform);
            var meshRenderer = ball.GetComponent<MeshRenderer>();
            var colorIndex = Random.Range(0, 4);
            meshRenderer.material.color = colors[colorIndex];

            var state = scale < bigBallScale ? Tile.State.HaveSmallBall : Tile.State.HaveBigBall;
            tile.GetComponent<Tile>().state = state;
        }
    }

    private void RandomPos(out int x, out int y)
    {
        x = Random.Range(0, ColNum);
        y = Random.Range(0, RowNum);
        while (!IsTileEmpty(x, y))
        {
            x = Random.Range(0, ColNum);
            y = Random.Range(0, RowNum);
        }
    }


    private bool IsTileEmpty(int x, int y)
    {
        var tileCmp = GetTileAt(x, y).GetComponent<Tile>();
        return tileCmp.state == Tile.State.Empty;
    }

    public GameObject GetTileAt(int x, int y) 
    {
        if (Tiles == null)
        {
            MakeTiles();
        }
        return Tiles[x, y]; 
    }

    Vector2Int? GetTilePosByMousePos(Vector3 mousePos)
    {
        var origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var hit = Physics2D.Raycast(origin, Vector3.forward, 1 + Vector3.Distance(this.transform.position, camera.transform.position));

        if (hit)
        {
            var tile = hit.collider.gameObject;
            for (int x = 0; x < RowNum; x++)
            {
                for (int y = 0; y < ColNum; y++)
                {
                    if (Tiles[x, y] == tile)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
        }
        return null;
    }

    void MakeTiles()
    {
        Tiles = new GameObject[RowNum, ColNum];
        for (int y = 0; y < RowNum; y++)
        {
            for (int x = 0; x < ColNum; x++)
            {
                var tile = Instantiate<GameObject>(tilePrefab);

                tile.transform.SetParent(this.transform);
                tile.transform.position = this.transform.position;
                var NewPos = new Vector2(tile.transform.position.x + SpriteSize.x * x, tile.transform.position.y + SpriteSize.y * y);
                tile.transform.position = NewPos;

                Tiles[x, y] = tile;
            }
        }
    }

    //Thực hiện di chuyển cho quả bóng 
    private void IsBallMoved()
    {
        if (selectedTilePos == null)
        {
            return;
        }

        var tilePos = GetTilePosByMousePos(Input.mousePosition);

        var clickedTile = GetTileAt(tilePos.Value.x, tilePos.Value.y);
        var currentTileCmp = clickedTile.GetComponent<Tile>();

        var lastSelectedBallTile = GetTileAt(selectedTilePos.Value.x, selectedTilePos.Value.y);
        var selectedBall = lastSelectedBallTile.transform.GetChild(0);
        selectedBall.transform.position = clickedTile.transform.position;
        currentTileCmp.state = Tile.State.HaveBigBall;
        selectedBall.transform.SetParent(clickedTile.transform);

        var lastSelectedBallTileCmp = lastSelectedBallTile.GetComponent<Tile>();
        lastSelectedBallTileCmp.state = Tile.State.Empty;
 
    }

    //private void CheckSmallBall()
    //{
    //    for (int i = 0; i < RowNum; i++)
    //    {
    //        for (int j = 0; j < ColNum; j++)
    //        {
    //            var ball = transform.GetChild(0).GetChild(0);
                
    //            ball.transform.localScale = bigBallScale * Vector3.one;
                
    //        }
    //    }
    //}
}
