using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public enum Connect4Player {None = 0, Black = 1, White = 2}

public class Connect4Board : SingletonPersistent<Connect4Board>
{
#pragma warning disable 0649
    [Header("Board Settings")]
    [SerializeField]
    private float rotateDuration = 0.25f;

    [Header("Disc Settings")]
    [SerializeField]
    private Connect4Player currentPlayer = Connect4Player.Black;
    
    [SerializeField]
    private float discShiftDuration = 0.25f;

    [SerializeField]
    private float discDropDuration = 0.5f;

    [Header("AI Settings")]
    [SerializeField]
    private float aiMoveDelay = 0.25f;

    [SerializeField]
    private float aiAutoResetDelay = 2.0f;

    [SerializeField]
    [Range(0, 6)]
    private int aiBlackMinimaxDepth = 0;

    [SerializeField]
    [Range(0, 6)]
    private int aiWhiteMinimaxDepth = 0;


    [SerializeField]
    private bool isBlackDiscAI, isWhiteDiscAI;
#pragma warning restore 0649

    private Connect4Player[,] currentBoardMatrix = new Connect4Player[6, 7];
    private Connect4Player winnerPlayer = Connect4Player.None;
    private LineRenderer winLineRenderer;
    private Vector3 startLinePosition, endLinePosition;

    private InputManager inputManager;

    private SwipeDetection swipeDetection;

    private ScoreManager scoreManager;

    private ObjectPooler blackDiscObjectPool;

    private ObjectPooler whiteDiscObjectPool;

    private GameManager gameManager;

    private GameObject currentDisc;

    private int currentDiscRow;

    private int currentDiscCol;

    private RaiseEventOptions raiseEventOptions;

    private SendOptions sendOptions;

    private const byte SPAWN_DISC = 0;

    private const byte MOVE_DISC = 1;

    private const byte DROP_DISC = 2;

    private const byte ROTATE_BOARD = 3;

    private const byte CHANGE_TURN = 4;

    private const byte WON_GAME = 5;

    private const byte TIE_GAME = 6;

    [HideInInspector]
    public Connect4Player WinnerPlayer{
        get{return winnerPlayer;}
    }

    [HideInInspector]
    public bool IsBlackDiscAI{
        get{return isBlackDiscAI;}
    }

    [HideInInspector]
    public bool IsWhiteDiscAI{
        get{return isWhiteDiscAI;}
    }


    public override void Awake()
    {
        base.Awake();

        winLineRenderer = GetComponent<LineRenderer>();
        raiseEventOptions = new RaiseEventOptions{Receivers=ReceiverGroup.Others};
        sendOptions = SendOptions.SendReliable;

        blackDiscObjectPool = GetComponents<ObjectPooler>()[0];
        whiteDiscObjectPool = GetComponents<ObjectPooler>()[1];
    }

    public override void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        currentPlayer = Connect4Player.Black;
        transform.rotation = Quaternion.identity;

        if (scene.name == "Gameplay")
        {
            inputManager = InputManager.Instance;
            swipeDetection = SwipeDetection.Instance;
            scoreManager = ScoreManager.Instance;
            gameManager = GameManager.Instance;

            if (PhotonNetwork.IsConnected)
            {
                isBlackDiscAI = false;
                isWhiteDiscAI = false;

                PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;

                Connect4Player localPlayer = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties["Disc Color"];
                if (localPlayer == Connect4Player.Black)
                    swipeDetection.SetTrailColor(Color.black);
                else if (localPlayer == Connect4Player.White)
                    swipeDetection.SetTrailColor(Color.white);

                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", null}});
                inputManager.enabled = localPlayer == currentPlayer;
            }
            else
            {
                if (scoreManager != null)
                {
                    scoreManager.SetBlackDiscScore(0);
                    scoreManager.SetWhiteDiscScore(0);
                }

                if (!(isBlackDiscAI && isWhiteDiscAI))
                {
                    aiBlackMinimaxDepth = 0;
                    aiWhiteMinimaxDepth = 0;
                }
            }
        }   
        else if (scene.name == "StartMenu")
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;

            inputManager = null;
            swipeDetection = null;
            scoreManager = null;
            gameManager = null;

            isBlackDiscAI = true;
            isWhiteDiscAI = true;
        }
        
        ResetBoard();
        SpawnNextDisc();
    }
    public void ResetBoard()
    {
        StopAllCoroutines();

        if (swipeDetection != null)
        {
            swipeDetection.OnSwipeLeft += ShiftDiscToLeft;
            swipeDetection.OnSwipeRight += ShiftDiscToRight;
            swipeDetection.OnSwipeDown += DropDisc;
        }

        if (scoreManager != null)
            scoreManager.SetTieTextActive(false);

        winnerPlayer = 0;
        winLineRenderer.enabled = false;

        GameObject [] blackDiscs = GameObject.FindGameObjectsWithTag("Black Disc");
        foreach (GameObject disc in blackDiscs)
            blackDiscObjectPool.ReturnPooledObject(disc);

        GameObject [] whiteDiscs = GameObject.FindGameObjectsWithTag("White Disc");
        foreach (GameObject disc in whiteDiscs)
            whiteDiscObjectPool.ReturnPooledObject(disc);
        
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                currentBoardMatrix[row, col] = Connect4Player.None;
            }
        }

        if (isBlackDiscAI && isWhiteDiscAI)
        {
            aiBlackMinimaxDepth = Random.Range(1, 7);
            aiWhiteMinimaxDepth = Random.Range(1, 7);
        }
    }
    
    void SpawnNextDisc()
    {
        if (!(PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay"))
        {
            if (currentPlayer == Connect4Player.Black)
            {
                currentDisc = blackDiscObjectPool.GetPooledObject();

                if (isBlackDiscAI)
                {
                    if (inputManager != null)
                        inputManager.enabled = false;

                    int targetColumn;
                    if (aiBlackMinimaxDepth <= 0)
                    {
                        List<int> availableMoves = GetAvailableMoves(currentBoardMatrix);
                        targetColumn = availableMoves[Random.Range(0, availableMoves.Count)];
                    }
                    else
                        targetColumn = (int)MiniMax(aiBlackMinimaxDepth, -Mathf.Infinity, Mathf.Infinity, currentPlayer, currentBoardMatrix).x;
                    
                    StartCoroutine(RunAITurn(targetColumn));
                }
                else
                {
                    if (inputManager != null)
                        inputManager.enabled = true;

                    if (swipeDetection != null)
                        swipeDetection.SetTrailColor(Color.black);
                }
            }
            else if (currentPlayer == Connect4Player.White)
            {
                currentDisc = whiteDiscObjectPool.GetPooledObject();

                if (isWhiteDiscAI)
                {
                    if (inputManager != null)
                        inputManager.enabled = false;
                    
                    int targetColumn;
                    if (aiWhiteMinimaxDepth <= 0) 
                    {
                        List<int> availableMoves = GetAvailableMoves(currentBoardMatrix);
                        targetColumn = availableMoves[Random.Range(0, availableMoves.Count)];
                    }
                    else
                        targetColumn = (int)MiniMax(aiWhiteMinimaxDepth, -Mathf.Infinity, Mathf.Infinity, currentPlayer, currentBoardMatrix).x;

                    StartCoroutine(RunAITurn(targetColumn));
                }
                else
                {
                    if (inputManager != null)
                        inputManager.enabled = true;

                    if (swipeDetection != null)
                        swipeDetection.SetTrailColor(Color.white);
                }
            }

            currentDiscRow = -1;
            currentDiscCol = GetColAt(currentDisc.transform.position.x, currentPlayer);
        }
        else
        {
            if ((Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties["Disc Color"] == currentPlayer)
            {
                if (currentPlayer == Connect4Player.Black)
                    currentDisc = blackDiscObjectPool.GetPooledObject();
                else if (currentPlayer == Connect4Player.White)
                    currentDisc = whiteDiscObjectPool.GetPooledObject();

                currentDiscRow = -1;
                currentDiscCol = GetColAt(currentDisc.transform.position.x, currentPlayer);

                object[] data = new object[]{(int)currentPlayer};
                PhotonNetwork.RaiseEvent(SPAWN_DISC, data, raiseEventOptions, sendOptions);
            }
        }
    }

    public float GetXAt(int col, Connect4Player player)
    {
        if (player == Connect4Player.Black)
            return (5 * col) - 15;
        
        return (5 * (6 - col)) - 15;
    }

    public float GetYAt(int row)
    {
        return 10 - (5 * row);
    }

    public int GetColAt(float x, Connect4Player player)
    {
        if (player == Connect4Player.Black)
            return ((int)x + 15) / 5;
            
        return 6 - ((int)x + 15) / 5;
    }

    public int GetRowAt(float y)
    {
        return ((int)y - 10) / -5;
    }

    public int GetRowAvailableAt(int col, Connect4Player[,] board)
    {
        if (board[0, col] != 0)
            return -1;

        int targetRow = 5;
        while (board[targetRow, col] != 0 && targetRow > 0)
            targetRow--;
        
        return targetRow;
    }

    public List<int> GetAvailableMoves(Connect4Player[,] board)
    {
        List<int> Columns = new List<int>();

        for (int i = 0; i <= 6; i++)
        {
            if (board[0, i] == Connect4Player.None)
                Columns.Add(i);
        }

        return Columns;
    }

    public void PlaceDiscOn(Connect4Player player, int row, int col, Connect4Player[,] board)
    {
        board[row, col] = player;
    }

    public bool IsWinnerAt(Connect4Player player, int row, int col, Connect4Player[,] board)
    {
        if (player == Connect4Player.None)
            return false;

        bool horizontalCheck = IsWinnerAtRow(player, row, board);
        bool verticalCheck = IsWinnerAtCol(player, col, board);
        bool diagonalCheck = IsWinnerDiagonally(player, board);

        return horizontalCheck || verticalCheck || diagonalCheck;
    }

    private bool IsWinnerAtRow(Connect4Player player, int row, Connect4Player[,] board)
    {
        if (player == Connect4Player.None)
            return false;

        for (int col = 0; col <= 3; col++)
        {
            if (board[row, col] == player && board[row, col + 1] == player &&
                board[row, col + 2] == player && board[row, col + 3] == player)
            {
                if (board == currentBoardMatrix)
                {
                    startLinePosition = new Vector3(GetXAt(col, player), GetYAt(row), -1.0f);
                    endLinePosition = new Vector3(GetXAt(col + 3, player), GetYAt(row), -1.0f);
                }

                return true;
            }
        }

        return false;
    }

    private bool IsWinnerAtCol(Connect4Player player, int col, Connect4Player[,] board)
    {
        if (player == Connect4Player.None)
            return false;

        for (int row = 0; row <= 2; row++)
        {
            if (board[row, col] == player && board[row + 1, col] == player &&
                board[row + 2, col] == player && board[row + 3, col] == player)
            {
                if (board == currentBoardMatrix)
                {
                    startLinePosition = new Vector3(GetXAt(col, player), GetYAt(row), -1.0f);
                    endLinePosition = new Vector3(GetXAt(col, player), GetYAt(row + 3), -1.0f);
                }

                return true;
            }
        }

        return false;
    }

    private bool IsWinnerDiagonally(Connect4Player player, Connect4Player[,] board)
    {
        if (player == Connect4Player.None)
            return false;
        
        for (int col = 3; col < 7; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (board[row, col] == player && board[row + 1, col - 1] == player &&
                    board[row + 2, col - 2] == player && board[row + 3, col - 3] == player)
                {
                    if (board == currentBoardMatrix)
                    {
                        startLinePosition = new Vector3(GetXAt(col, player), GetYAt(row), -1.0f);
                        endLinePosition = new Vector3(GetXAt(col - 3, player), GetYAt(row + 3), -1.0f);
                    }

                    return true;
                }
            }

            for (int row = 3; row < 6; row++)
            {
                if (board[row, col] == player && board[row - 1, col - 1] == player &&
                    board[row - 2, col - 2] == player && board[row - 3, col - 3] == player)
                {
                    if (board == currentBoardMatrix)
                    {
                        startLinePosition = new Vector3(GetXAt(col, player), GetYAt(row), -1.0f);
                        endLinePosition = new Vector3(GetXAt(col - 3, player), GetYAt(row - 3), -1.0f);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private int CountDiscsInARow(int n, Connect4Player player, Connect4Player[,] board)
    {
        Debug.Assert(n == 2 || n == 3 || n == 4);

        int count = 0;
        
        //Check entire board horizontally
        for (int row = 0; row <= 5; row++)
        {
            for (int col = 0; col <= 3; col++)
            {
                if (n == 4)
                {
                    if (board[row, col] == player && board[row, col + 1] == player && 
                        board[row, col + 2] == player && board[row, col + 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 3)
                {
                    if (board[row, col] == player && board[row, col + 1] == player && 
                        board[row, col + 2] == player && board[row, col + 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row, col + 1] == player && 
                        board[row, col + 2] == player && board[row, col + 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 2)
                {
                    if (board[row, col] == player && board[row, col + 1] == player && 
                        board[row, col + 2] == Connect4Player.None && board[row, col + 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row, col + 1] == player && 
                        board[row, col + 2] == player && board[row, col + 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row, col + 1] == Connect4Player.None && 
                        board[row, col + 2] == player && board[row, col + 3] == player)
                    {
                        count++;
                    }
                }
            }
        }

        //Check entire board vertically
        for (int col = 0; col <= 6; col++)
        {
            for (int row = 0; row <= 2; row++)
            {
                if (n == 4)
                {
                    if (board[row, col] == player && board[row + 1, col] == player && 
                        board[row + 2, col] == player && board[row + 3, col] == player)
                    {
                        count++;
                    }
                }
                else if (n == 3)
                {
                    if (board[row, col] == player && board[row + 1, col] == player && 
                        board[row + 2, col] == player && board[row + 3, col] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row + 1, col] == player && 
                        board[row + 2, col] == player && board[row + 3, col] == player)
                    {
                        count++;
                    }
                }
                else if (n == 2)
                {
                    if (board[row, col] == player && board[row + 1, col] == player && 
                        board[row + 2, col] == Connect4Player.None && board[row + 3, col] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row + 1, col] == player && 
                        board[row + 2, col] == player && board[row + 3, col] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == player && board[row + 1, col] == player && 
                        board[row + 2, col] == Connect4Player.None && board[row + 3, col] == Connect4Player.None)
                    {
                        count++;
                    }
                }
            }
        }

        //Check entire board both sides diagonally
        for (int col = 3; col < 7; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (n == 4)
                {
                    if (board[row, col] == player && board[row + 1, col - 1] == player && 
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 3)
                {
                    if (board[row, col] == player && board[row + 1, col - 1] == player && 
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row + 1, col - 1] == player && 
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 2)
                {
                    if (board[row, col] == player && board[row + 1, col - 1] == player && 
                        board[row + 2, col - 2] == Connect4Player.None && board[row + 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row + 1, col - 1] == player && 
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row + 1, col - 1] == Connect4Player.None && 
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == player)
                    {
                        count++;
                    }
                }
            }

            for (int row = 3; row < 6; row++)
            {
                if (n == 4)
                {
                    if (board[row, col] == player && board[row - 1, col - 1] == player && 
                        board[row - 2, col - 2] == player && board[row - 3, col - 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 3)
                {
                    if (board[row, col] == player && board[row - 1, col - 1] == player && 
                        board[row - 2, col - 2] == player && board[row - 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row - 1, col - 1] == player && 
                        board[row - 2, col - 2] == player && board[row - 3, col - 3] == player)
                    {
                        count++;
                    }
                }
                else if (n == 2)
                {
                    if (board[row, col] == player && board[row - 1, col - 1] == player && 
                        board[row - 2, col - 2] == Connect4Player.None && board[row - 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row - 1, col - 1] == player && 
                        board[row - 2, col - 2] == player && board[row - 3, col - 3] == Connect4Player.None)
                    {
                        count++;
                    }
                    
                    if (board[row, col] == Connect4Player.None && board[row - 1, col - 1] == Connect4Player.None && 
                        board[row - 2, col - 2] == player && board[row - 3, col - 3] == player)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    public bool IsTie(Connect4Player [,] board)
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (board[row, col] == Connect4Player.None)
                    return false;
            }
        }

        return true;
    }

    public Connect4Player GetWinner()
    {
        return winnerPlayer;
    }

    public void SetWinner(Connect4Player player)
    {
        currentDisc = null;

        if (swipeDetection != null)
        {
            swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
            swipeDetection.OnSwipeRight -= ShiftDiscToRight;
            swipeDetection.OnSwipeDown -= DropDisc;
        }

        if (inputManager != null)
            inputManager.enabled = false;

        if (gameManager != null && !(isBlackDiscAI && isWhiteDiscAI))
        {
            gameManager.SetResetButtonVisible(true);
        }

        winnerPlayer = player;
        winLineRenderer.enabled = true;
        winLineRenderer.startColor = player == Connect4Player.Black ? Color.white : Color.black;
        winLineRenderer.endColor = winLineRenderer.startColor;
        winLineRenderer.SetPosition(0, startLinePosition);
        winLineRenderer.SetPosition(1, endLinePosition);

        if (scoreManager != null)
        {
            if (winnerPlayer == Connect4Player.Black)
            {
                scoreManager.SetBlackDiscScore(PlayerPrefs.GetInt("Black Disc Score") + 1);

                if (isWhiteDiscAI)
                {
                    if (PlayerPrefs.GetInt("Black Disc Score") % 2 == 1 && aiWhiteMinimaxDepth < 6)
                    {
                        aiWhiteMinimaxDepth++;
                    }
                }
            }
            else if (winnerPlayer == Connect4Player.White)
            {
                scoreManager.SetWhiteDiscScore(PlayerPrefs.GetInt("White Disc Score") + 1);

                if (isBlackDiscAI)
                {
                    if (PlayerPrefs.GetInt("White Disc Score") % 2 == 1 && aiBlackMinimaxDepth < 6)
                    {
                        aiBlackMinimaxDepth++;
                    }
                }
            }
        }

        if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
        {
            int winnerScore = player == Connect4Player.Black ? PlayerPrefs.GetInt("Black Disc Score") : PlayerPrefs.GetInt("White Disc Score");

            object[] data = new object[]{(int)player, startLinePosition, endLinePosition, winnerScore};
            PhotonNetwork.RaiseEvent(WON_GAME, data, raiseEventOptions, sendOptions);
        }
    }

    private float EvaluateBoard(Connect4Player player, Connect4Player [,] board)
    {
        Connect4Player opponent = player == Connect4Player.Black ? Connect4Player.White : Connect4Player.Black;
        
        int playerFourInARows = CountDiscsInARow(4, player, board);
        int playerThreeInARows = CountDiscsInARow(3, player, board);
        int playerTwoInARows = CountDiscsInARow(2, player, board);
        
        int opponentFourInARows = CountDiscsInARow(4, opponent, board);
        int opponentThreeInARows = CountDiscsInARow(3, opponent, board);
        int opponentTwoInARows = CountDiscsInARow(2, opponent, board);

        int randomCol = Random.Range(0, 7);

        int playerCountInRandomCol = 0;
        int opponentCountInRandomCol = 0;

        for (int row = 0; row <= 5; row++)
        {
            if (board[row, randomCol] == player)
                playerCountInRandomCol++;
            else if (board[row, randomCol] == opponent)
                opponentCountInRandomCol++;
        }

        float playerScore = playerFourInARows * 100000 + playerThreeInARows * 100 + playerTwoInARows * 10 + playerCountInRandomCol * 5;
        float opponentScore = opponentFourInARows * 100000 + opponentThreeInARows * 100 + opponentTwoInARows * 10 + opponentCountInRandomCol * 5;

        if (currentPlayer == Connect4Player.Black && aiBlackMinimaxDepth % 2 == 1 || 
            currentPlayer == Connect4Player.White && aiWhiteMinimaxDepth % 2 == 1)
            return opponentScore - playerScore;
        else
            return playerScore - opponentScore;
    }

    private Vector2 MiniMax(int depth, float alpha, float beta, Connect4Player player, Connect4Player[,] board)
    {
        Connect4Player opponent = player == Connect4Player.Black ? Connect4Player.White : Connect4Player.Black;

       if (depth == 0 || IsTie(board))
            return new Vector2(-1, EvaluateBoard(player, board));

        int targetColumn = -1;

        if (player == currentPlayer)
        {
            float value = -Mathf.Infinity;

            foreach (int col in GetAvailableMoves(board))
            {
                int row = GetRowAvailableAt(col, board);

                Connect4Player [,] tempBoard = (Connect4Player[,])board.Clone();
                PlaceDiscOn(player, row, col, tempBoard);

                float newValue = MiniMax(depth - 1, alpha, beta, opponent, tempBoard).y;
                if (newValue > value)
                {
                    value = newValue;
                    targetColumn = col;
                }

                alpha = Mathf.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }

            return new Vector3(targetColumn, value);
        }
        else
        {
            float value = Mathf.Infinity;

            foreach (int col in GetAvailableMoves(board))
            {
                int row = GetRowAvailableAt(col, board);

                Connect4Player [,] tempBoard = (Connect4Player[,])board.Clone();
                PlaceDiscOn(player, row, col, tempBoard);

                float newValue = MiniMax(depth - 1, alpha, beta, opponent, tempBoard).y;
                if (newValue < value)
                {
                    value = newValue;
                    targetColumn = col;
                }

                beta = Mathf.Min(beta, value);
                if (alpha >= beta)
                    break;
            }

            return new Vector3(targetColumn, value);
        }
    }
    
    IEnumerator ShiftToColumn(int targetCol)
    {
        float targetX = GetXAt(targetCol, currentPlayer);

        if (Mathf.Abs(targetX) <= 15)
        {
            if (swipeDetection != null)
                swipeDetection.enabled = false;

            object [] data = new object[2];

            float startX = GetXAt(currentDiscCol, currentPlayer);

            float t = 0;
            while (t <= discShiftDuration)
            {
                t += Time.deltaTime;
                float currentX = Mathf.Lerp(startX, targetX, t / discShiftDuration);
                currentDisc.transform.position = new Vector3(currentX, currentDisc.transform.position.y, currentDisc.transform.position.z);

                if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
                {
                    data[0] = currentDisc.transform.position;
                    data[1] = null;
                    PhotonNetwork.RaiseEvent(MOVE_DISC, data, raiseEventOptions, sendOptions);
                }

                yield return null;
            }

            currentDisc.transform.position = new Vector3(targetX, currentDisc.transform.position.y, currentDisc.transform.position.z);
            currentDiscCol = targetCol;

            if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
            {
                data[0] = currentDisc.transform.position;
                data[1] = currentDiscCol;
                PhotonNetwork.RaiseEvent(MOVE_DISC, data, raiseEventOptions, sendOptions);
            }

            if (swipeDetection != null)
                swipeDetection.enabled = true;
        }
        else 
            yield return 0;
    }

    IEnumerator DropDiscInBoard()
    {
        int targetRow = GetRowAvailableAt(currentDiscCol, currentBoardMatrix);
        if (targetRow >= 0)
        {
            if (swipeDetection != null)
                swipeDetection.enabled = false;

            object[] data = new object[4];

            float startY = currentDisc.transform.position.y;
            float targetY = GetYAt(targetRow);

            float t = 0;
            while (t <= discDropDuration)
            {
                t += Time.deltaTime;
                float currentY = Mathf.Lerp(startY, targetY, t / discDropDuration);
                currentDisc.transform.position = new Vector3(currentDisc.transform.position.x, currentY, currentDisc.transform.position.z);

                if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
                {
                    data[0] = currentDisc.transform.position;
                    data[1] = null;
                    data[2] = null;
                    data[3] = null;

                    PhotonNetwork.RaiseEvent(DROP_DISC, data, raiseEventOptions, sendOptions);
                }

                yield return null;
            }

            currentDisc.transform.position = new Vector3(currentDisc.transform.position.x, targetY, currentDisc.transform.position.z);
            currentDiscRow = targetRow;
            PlaceDiscOn(currentPlayer, currentDiscRow, currentDiscCol, currentBoardMatrix);

            if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
            {
                data[0] = currentDisc.transform.position;
                data[1] = (int)currentPlayer;
                data[2] = currentDiscRow;
                data[3] = currentDiscCol;

                PhotonNetwork.RaiseEvent(DROP_DISC, data, raiseEventOptions, sendOptions);
            }

            if (IsWinnerAt(currentPlayer, currentDiscRow, currentDiscCol, currentBoardMatrix))
            {
                SetWinner(currentPlayer);

                if (isBlackDiscAI && isWhiteDiscAI)
                    yield return StartCoroutine("AutoReset");
                else
                    yield return 0;
            }
            else
            {
                if (IsTie(currentBoardMatrix))
                {
                    currentDisc = null;

                    if (swipeDetection != null)
                    {
                        swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
                        swipeDetection.OnSwipeRight -= ShiftDiscToRight;
                        swipeDetection.OnSwipeDown -= DropDisc;
                    }

                    if (scoreManager != null)
                        scoreManager.SetTieTextActive(true);

                    if (inputManager != null)
                        inputManager.enabled = false;
                    
                    if (isBlackDiscAI && isWhiteDiscAI)
                        yield return StartCoroutine("AutoReset");
                    else
                        yield return 0;

                    if (gameManager != null)
                        gameManager.SetResetButtonVisible(true);

                    if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
                        PhotonNetwork.RaiseEvent(TIE_GAME, null, raiseEventOptions, sendOptions);
                }
                else
                    yield return StartCoroutine(RotateBoard());
            }
        }
        else
            yield return 0;
    }

    public IEnumerator RotateBoard()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0.0f, -180.0f, 0.0f);

        object[] data = new object[1];

        float t = 0;
        while (t <= rotateDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t / rotateDuration);

            if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
            {
                data[0] = transform.eulerAngles;
                PhotonNetwork.RaiseEvent(ROTATE_BOARD, data, raiseEventOptions, sendOptions);
            }

            yield return null;
        }

        transform.rotation = targetRotation;

        if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
        {
            data[0] = transform.eulerAngles;
            PhotonNetwork.RaiseEvent(ROTATE_BOARD, data, raiseEventOptions, sendOptions);
        }

        yield return StartCoroutine(ChangeTurn());
    }

    public IEnumerator ChangeTurn()
    {
        if (transform.eulerAngles.y <= 0.0f)
            currentPlayer = Connect4Player.Black;
        else
            currentPlayer = Connect4Player.White;
        
        yield return new WaitForEndOfFrame();

        if (swipeDetection != null)
            swipeDetection.enabled = true;
        
        if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Gameplay")
        {
            inputManager.enabled = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties["Disc Color"] == currentPlayer;

            object[] data = new object[]{(int)currentPlayer, !((bool)inputManager.enabled)};
            PhotonNetwork.RaiseEvent(CHANGE_TURN, data, raiseEventOptions, sendOptions);
        }
        else
        {
            SpawnNextDisc();
        }
    }

    private IEnumerator RunAITurn(int targetCol)
    {
        yield return new WaitForSeconds(aiMoveDelay);

        yield return StartCoroutine(ShiftToColumn(targetCol));

        yield return new WaitForSeconds(aiMoveDelay);

        yield return StartCoroutine("DropDisc");
    }

    private IEnumerator AutoReset()
    {
        yield return new WaitForSeconds(aiAutoResetDelay);
        GameManager.Instance.ResetGame();
    }

    void OnEventReceived(EventData obj)
    {
        if (obj.Code == SPAWN_DISC)
        {
            object[] data = (object[])obj.CustomData;
            
            currentPlayer = (Connect4Player)data[0];
            if (currentPlayer == Connect4Player.Black)
                currentDisc = blackDiscObjectPool.GetPooledObject();
            else if (currentPlayer == Connect4Player.White)
                currentDisc = whiteDiscObjectPool.GetPooledObject();

            currentDiscRow = -1;
            currentDiscCol = GetColAt(currentDisc.transform.position.x, currentPlayer);
        }
        else if (obj.Code == MOVE_DISC)
        {
            object[] data = (object[])obj.CustomData;

            currentDisc.transform.position = (Vector3)data[0];
            if (data[1] != null)
                currentDiscCol = (int)data[1];
        }
        else if (obj.Code == DROP_DISC)
        {
            object[] data = (object[])obj.CustomData;

            currentDisc.transform.position = (Vector3)data[0];
            if (data[1] != null && data[2] != null && data[3] != null)
            {
                currentPlayer = (Connect4Player)data[1];
                currentDiscRow = (int)data[2];
                currentDiscCol = (int)data[3];

                PlaceDiscOn(currentPlayer, currentDiscRow, currentDiscCol, currentBoardMatrix);
            }
        }
        else if (obj.Code == ROTATE_BOARD)
        {
            object[] data = (object[])obj.CustomData;

            transform.eulerAngles = (Vector3)data[0];
        }
        else if (obj.Code == CHANGE_TURN)
        {
            object[] data = (object[])obj.CustomData;
            
            currentPlayer = (Connect4Player)data[0];
            
            inputManager.enabled = (bool)data[1];

            SpawnNextDisc();

            if (swipeDetection != null)
                swipeDetection.enabled = true;
        }
        else if (obj.Code == WON_GAME)
        {
            currentDisc = null;

            if (swipeDetection != null)
            {
                swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
                swipeDetection.OnSwipeRight -= ShiftDiscToRight;
                swipeDetection.OnSwipeDown -= DropDisc;
            }

            if (inputManager != null)
                inputManager.enabled = false;

            if (gameManager != null)
                gameManager.SetResetButtonVisible(true);

            object[] data = (object[])obj.CustomData;

            winnerPlayer = (Connect4Player)data[0];
            winLineRenderer.enabled = true;
            winLineRenderer.startColor = winnerPlayer == Connect4Player.Black ? Color.white : Color.black;
            winLineRenderer.endColor = winLineRenderer.startColor;
            winLineRenderer.SetPosition(0, (Vector3)data[1]);
            winLineRenderer.SetPosition(1, (Vector3)data[2]);

            if (scoreManager != null)
            {
                if (winnerPlayer == Connect4Player.Black)
                    scoreManager.SetBlackDiscScore((int)data[3]);
                else if (winnerPlayer == Connect4Player.White)
                    scoreManager.SetWhiteDiscScore((int)data[3]);
            }
        }
        else if (obj.Code == TIE_GAME)
        {
            currentDisc = null;

            if (swipeDetection != null)
            {
                swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
                swipeDetection.OnSwipeRight -= ShiftDiscToRight;
                swipeDetection.OnSwipeDown -= DropDisc;
            }

            if (inputManager != null)
                inputManager.enabled = false;

            if (scoreManager != null)
                scoreManager.SetTieTextActive(true);

            if (gameManager != null)
                gameManager.SetResetButtonVisible(true);
        }
    }

    void ShiftDiscToLeft()
    {
        if (currentPlayer == Connect4Player.Black)
        {
            StartCoroutine(ShiftToColumn(currentDiscCol - 1));
        }
        else if (currentPlayer == Connect4Player.White)
        {
            StartCoroutine(ShiftToColumn(currentDiscCol + 1));
        }
    }

    void ShiftDiscToRight()
    {
        if (currentPlayer == Connect4Player.Black)
        {
            StartCoroutine(ShiftToColumn(currentDiscCol + 1));
        }
        else if (currentPlayer == Connect4Player.White)
        {
            StartCoroutine(ShiftToColumn(currentDiscCol - 1));
        }
    }

    void DropDisc()
    {
        StartCoroutine("DropDiscInBoard");
    }
    public void SetBlackDiscAI(bool AI)
    {
        isBlackDiscAI = AI;
    }

    public void SetWhiteDiscAI(bool AI)
    {
        isWhiteDiscAI = AI;
    }
}
