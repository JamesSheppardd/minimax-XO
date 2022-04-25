using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [SerializeField] public int size;
    [SerializeField] public float spriteDim;
    [SerializeField] public int[,] boardPositions = new int[,]{{ 0,0,0 }, { 0,0,0 }, { 0,0,0 }};
    [SerializeField] public Sprite squareSprite;
    [SerializeField] private List<Color32> posColours = new List<Color32>();
    [SerializeField] private List<Color32> stateColours = new List<Color32>();
    [SerializeField] private List<string> statePhrases = new List<string>();
    [SerializeField] public Text moveText, stateText;
    [SerializeField] public List<Sprite> playerSprites = new List<Sprite>();
    int searches = 0;

    string currentMove = "X";    // 1 = bot, 2 = player
    #region Board Functions
    void CreateBoard(int size, int[,] startPositions){
        GameObject parent = new GameObject("Board");
        parent.transform.eulerAngles = new Vector3(-90, 0, 0);
        int count = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject square = new GameObject($"row:{i}, col:{j}", typeof(SpriteRenderer), typeof(BoxCollider2D));
                square.transform.parent = parent.transform;
                SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                BoxCollider2D col = square.GetComponent<BoxCollider2D>();
                sr.sprite = squareSprite;
                col.size = Vector2.one;
                // Set position & size
                square.transform.position = new Vector2(j*spriteDim,i*spriteDim);
                square.transform.localScale = Vector3.one * spriteDim;
                // Set colour
                if(count % 2 == 0){
                    sr.color = posColours[0];
                } else{
                    sr.color = posColours[1];
                }
                count++;
            }
        }

    }

    

    void MakeMove(Vector2 position){
        int row = (int)position.y / 2;
        int column = (int)position.x / 2;

        if (boardPositions[row, column] == 0)
        {
            if (currentMove == "X")
            {
                // X moves
                // place X at position % dimension
                GameObject x = new GameObject($"X: {position}", typeof(SpriteRenderer));
                x.tag = "Pos";
                SpriteRenderer sr = x.GetComponent<SpriteRenderer>();
                sr.sprite = playerSprites[0];
                x.transform.position = new Vector2(position.x, position.y);
                sr.sortingOrder = 1;

                boardPositions[row,column] = 1;

                currentMove = "O";
            }
            else
            {
                // O moves
                GameObject o = new GameObject($"O: {position}", typeof(SpriteRenderer));
                o.tag = "Pos";
                SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
                sr.sprite = playerSprites[1];
                o.transform.position = new Vector2(position.x, position.y);
                sr.sortingOrder = 1;

                boardPositions[row,column] = 2;

                currentMove = "X";
            }

            DisplayBoardInConsole();
            //CheckForWin(boardPositions);
        }
        else
        {
            print($"Point {row},{column} already played");
        }
    }

    void DisplayBoardInConsole()
    {
        print($"|{boardPositions[2, 0]}|{boardPositions[2, 1]}|{boardPositions[2,2]}|\n|{boardPositions[1, 0]}|{boardPositions[1,1]}|{boardPositions[1,2]}|\n|{boardPositions[0,0]}|{boardPositions[0, 1]}|{boardPositions[0, 2]}|");
    }

    int CheckForWin(int[,] board){
        // returns 0 if no win, 1 if X won, 2 if O won, or -1 if draw
        
        // check if all rows are same value for each column, if not then...
        for(int i = 0; i < 3; i++)
        {
            if(board[i,0] != 0 && (board[i,0] == board[i, 1] && board[i,0] == board[i,2]))
            {
                return board[i, 0];
            }
        }
        // ... check if all columns are the same for each row...
        for (int j = 0; j < 3; j++)
        {
            if (board[0, j] != 0 && (board[0, j] == board[1, j] && board[0, j] == board[2, j]))
            {
                return board[0, j];
            }
        }
        // ... check diagonals - [0,0], [1,1], [2,2] or [0,2], [1,1] [2,0]
        if(board[1,1] != 0 && ((board[1,1] == board[0,0] && board[1,1] == board[2, 2]) || (board[1,1] == board[0,2] && board[1,1] == board[2,0])))
        {
            //Debug.Log("<color=green>Winner diagonally</color>");
            return board[1, 1];
        }


        
        return 0;
    }

    bool CheckForDraw(int[,] board)
    {
        // clean, fast way to check if all values are same
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if(board[i,j] == 0)
                {
                    // not all moves played
                    return false;
                }
            }
        }
        // if got here, draw
        return true;
    }

    bool CheckForGameEnd(int[,] board){
        if(CheckForDraw(board) || CheckForWin(board) != 0){
            return true;
        }
        else{
            return false;
        }
    }

    void SetStateText(int state)
    {
        // state: 0 = draw, 1 = loss, 2 = win
        if(state != -1){
            stateText.text = $"{statePhrases[state]}";
            stateText.color = stateColours[state];
        }
        else{
            stateText.text = "";
        }
    }

    void PlayerMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Vector2 pos = hit.transform.position;
                MakeMove(pos);
            }
        }
    }

    void BotMove()
    {
        float maxEval = -1000;
        int[] bestMove = new int[2];     // not a move, will be overwritten

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if(boardPositions[i,j] == 0)
                {

                    boardPositions[i,j] = 1;                            // -----|
                                                                        //     |
                    float score = Minimax(boardPositions, 8, false);    //    |--- I just fkin missed these 2 lines out and it fucked me up for literally 2 days - kms
                                                                        //   |
                    boardPositions[i,j] = 0;                            // -|

                    if(score > maxEval)
                    {
                        //print($"Best position at [{i},{j}], with score of {score}.");
                        maxEval = score;
                        bestMove[0] = i;
                        bestMove[1] = j;
                    }
                }
            }
        }
        string col = "red";
        if(searches == 255168){
            col = "green";
        }
        print($"CPU looked at <color={col}>{searches}</color> moves, with a max evaluation at {maxEval}");
        searches = 0;
        Vector2 pos = new Vector2(bestMove[1] * 2, bestMove[0] * 2);
        MakeMove(pos);
    }
    #endregion

    private void Update() {
        // end of game
        if(CheckForGameEnd(boardPositions)){
            int win = CheckForWin(boardPositions);
            if(win != 0)
            {
                SetStateText(win);
            }
            else if(win == 0 && CheckForDraw(boardPositions)){
                SetStateText(0);
            }
            GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(true);
            currentMove = "";
        }

        if(currentMove == "X")
        {
            BotMove();
        }
        else if (currentMove == "O")
        {
            PlayerMove();
        }


        

        moveText.text = $"Move: {currentMove}";
    }

    private void Start() {
        CreateBoard(size, boardPositions);
    }

    public void ResetGame(){
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                boardPositions[i,j] = 0;
            }
        }

        GameObject[] go = GameObject.FindGameObjectsWithTag("Pos");
        foreach (var i in go)
        {
            Destroy(i);
        }
        GameObject.FindGameObjectWithTag("End").SetActive(false);
        currentMove = "X";

    }

    #region Minimax
    float Minimax(int[,] board, int depth, bool maximisingPlayer)
    {
        int bot = 1;
        int player = 2;

        bool winner = CheckForGameEnd(board);
        if(winner){
            searches++;
            if(CheckForWin(board) == 1)
            {
                return 10 - depth;          // bot wins
            }
            else if(CheckForWin(board) == 2)
            {
                return -10 + depth;  // player wins
            }
            else if(CheckForDraw(board))
            {
                return 0;                       // draw
            }
        }


        if (maximisingPlayer)
        {
            // set maxEval to low value
            float maxEval = -1000f;
            // loop through each possible 'child' position, playing out the game
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = bot;
                        float eval = Minimax(board, depth-1, false);
                        board[i, j] = 0;
                        maxEval = Mathf.Max(eval, maxEval);
                    }
                    
                }
            }
            return maxEval;
        }

        else
        {
            // set minEval to high value
            float minEval = 1000f;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if(board[i,j] == 0)
                    {
                        board[i, j] = player;
                        float eval = Minimax(board, depth, true);
                        board[i, j] = 0;
                        minEval = Mathf.Min(eval, minEval);
                        
                    }
                }
            }
            return minEval;
        }
    }
    #endregion
}
