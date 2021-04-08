﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Connect4Player {None, Black, White}

public class Connect4Board : MonoBehaviour
{
    [SerializeField]
    private GameObject blackDisc, whiteDisc;

    [SerializeField]
    private Connect4Player currentPlayer = Connect4Player.Black;

    [SerializeField]
    private float rotateDuration = 0.25f;

    [SerializeField]
    private float discShiftDuration = 0.25f;

    [SerializeField]
    private float discDropDuration = 0.5f;

    [SerializeField]
    private float aiMoveDelay = 0.25f;

    [SerializeField]
    private bool isBlackDiscAI, isWhiteDiscAI;

    private Connect4Player[,] boardMatrix = new Connect4Player[6, 7];
    private Connect4Player winnerPlayer = Connect4Player.None;

    private LineRenderer winLineRenderer;
    private Vector3 startLinePosition, endLinePosition;

    private InputManager inputManager;

    private SwipeDetection swipeDetection;

    private GameManager gameManager;

    private GameObject currentDisc;

    private int currentDiscRow;

    private int currentDiscCol;

    // Start is called before the first frame update
    void Start()
    {
        winLineRenderer = GetComponent<LineRenderer>();

        inputManager = InputManager.Instance;
        swipeDetection = SwipeDetection.Instance;
        gameManager = GameManager.Instance;

        ResetBoard();

        if (currentPlayer == Connect4Player.Black)
        {
            currentDisc = Instantiate<GameObject>(blackDisc, transform);
            transform.rotation = Quaternion.identity;

            if (isBlackDiscAI)
            {
                inputManager.enabled = false;

                List<int> AvailableMoves = GetAvailableMoves(boardMatrix);
                int targetColumn = AvailableMoves[Random.Range(0, AvailableMoves.Count)];
                StartCoroutine(RunAITurn(targetColumn));
            }
            else
            {
                inputManager.enabled = true;
            }
        }
        else if (currentPlayer == Connect4Player.White)
        {
            currentDisc = Instantiate<GameObject>(whiteDisc, transform);
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

            if (isWhiteDiscAI)
            {
                inputManager.enabled = false;

                List<int> AvailableMoves = GetAvailableMoves(boardMatrix);
                int targetColumn = AvailableMoves[Random.Range(0, AvailableMoves.Count)];
                StartCoroutine(RunAITurn(targetColumn));
            }
            else
            {
                inputManager.enabled = true;
            }
        }
        currentDiscRow = -1;
        currentDiscCol = GetColAt(currentDisc.transform.position.x, currentPlayer);
    }

    void ResetBoard()
    {
        inputManager.OnStartTouch -= ResetGame;

        swipeDetection.OnSwipeLeft += ShiftDiscToLeft;
        swipeDetection.OnSwipeRight += ShiftDiscToRight;
        swipeDetection.OnSwipeDown += DropDisc;

        gameManager.SetTieTextActive(false);

        winnerPlayer = 0;
        winLineRenderer.enabled = false;

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                boardMatrix[row, col] = Connect4Player.None;
            }
        }

        GameObject [] blackDiscs = GameObject.FindGameObjectsWithTag("Black Disc");
        foreach (GameObject disc in blackDiscs)
            Destroy(disc);

        GameObject [] whiteDiscs = GameObject.FindGameObjectsWithTag("White Disc");
        foreach (GameObject disc in whiteDiscs)
            Destroy(disc);
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

    public int GetRowAvailableAt(int col)
    {
        if (boardMatrix[0, col] != 0)
            return -1;

        int targetRow = 5;
        while (boardMatrix[targetRow, col] != 0 && targetRow > 0)
            targetRow--;
        
        return targetRow;
    }

    public Connect4Player[,] GetGameBoardMatrix()
    {
        return boardMatrix;
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

    public bool IsWinner(Connect4Player player, int row, int col, Connect4Player[,] board)
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
                if (board == boardMatrix)
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
                if (board == boardMatrix)
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
                    if (board == boardMatrix)
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
                    if (board == boardMatrix)
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

    public void SetWinner(Connect4Player player)
    {
        currentDisc = null;

        swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
        swipeDetection.OnSwipeRight -= ShiftDiscToRight;
        swipeDetection.OnSwipeDown -= DropDisc;

        inputManager.enabled = true;
        inputManager.OnStartTouch += ResetGame;

        winnerPlayer = player;
        winLineRenderer.enabled = true;
        winLineRenderer.startColor = player == Connect4Player.Black ? Color.white : Color.black;
        winLineRenderer.endColor = winLineRenderer.startColor;
        winLineRenderer.SetPosition(0, startLinePosition);
        winLineRenderer.SetPosition(1, endLinePosition);

        if (winnerPlayer == Connect4Player.Black)
            gameManager.SetBlackDiscScore(PlayerPrefs.GetInt("Black Disc Score") + 1);
        else if (winnerPlayer == Connect4Player.White)
            gameManager.SetWhiteDiscScore(PlayerPrefs.GetInt("White Disc Score") + 1);
    }

    public bool IsTie()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (boardMatrix[row, col] == Connect4Player.None)
                    return false;
            }
        }

        return true;
    }

    public Connect4Player GetWinner()
    {
        return winnerPlayer;
    }

    public IEnumerator RotateBoard()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0.0f, -180.0f, 0.0f);

        float t = 0;
        while (t <= rotateDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t / rotateDuration);

            yield return null;
        }

        transform.rotation = targetRotation;
        yield return StartCoroutine(ChangeTurn());
    }

    public IEnumerator ChangeTurn()
    {
        if (currentPlayer == Connect4Player.Black)
            currentPlayer = Connect4Player.White;
        else
            currentPlayer = Connect4Player.Black;

        yield return 0;

        List<int> AvailableMoves = GetAvailableMoves(boardMatrix);
        int targetColumn = AvailableMoves[Random.Range(0, AvailableMoves.Count)];

        if (currentPlayer == Connect4Player.Black)
        {
            currentDisc = Instantiate<GameObject>(blackDisc, transform);

            if (isBlackDiscAI)
            {
                inputManager.enabled = false;
                StartCoroutine(RunAITurn(targetColumn));
            }
            else
            {
                inputManager.enabled = true;
            }
        }
        else if (currentPlayer == Connect4Player.White)
        {
            currentDisc = Instantiate<GameObject>(whiteDisc, transform);

            if (isWhiteDiscAI)
            {
                inputManager.enabled = false;
                StartCoroutine(RunAITurn(6 - targetColumn));
            }
            else
            {
                inputManager.enabled = true;
            }
        }

        currentDiscRow = -1;
        currentDiscCol = GetColAt(currentDisc.transform.position.x, currentPlayer);
        swipeDetection.enabled = true;

        string moves = string.Empty;
        foreach (var move in AvailableMoves)
        {
            moves += move + " ";
        }
        Debug.Log(moves);
    }

    IEnumerator ShiftToColumn(int targetCol)
    {
        float targetX = GetXAt(targetCol, currentPlayer);

        if (Mathf.Abs(targetX) <= 15)
        {
            swipeDetection.enabled = false;

            float startX = GetXAt(currentDiscCol, currentPlayer);

            float t = 0;
            while (t <= discShiftDuration)
            {
                t += Time.deltaTime;
                float currentX = Mathf.Lerp(startX, targetX, t / discShiftDuration);
                currentDisc.transform.position = new Vector3(currentX, currentDisc.transform.position.y, currentDisc.transform.position.z);

                yield return null;
            }

            currentDisc.transform.position = new Vector3(targetX, currentDisc.transform.position.y, currentDisc.transform.position.z);
            currentDiscCol = targetCol;

            swipeDetection.enabled = true;
        }
        else 
            yield return 0;
    }

    IEnumerator DropDiscInBoard()
    {
        int targetRow = GetRowAvailableAt(currentDiscCol);
        if (targetRow >= 0)
        {
            swipeDetection.enabled = false;

            float startY = currentDisc.transform.position.y;
            float targetY = GetYAt(targetRow);

            float t = 0;
            while (t <= discDropDuration)
            {
                t += Time.deltaTime;
                float currentY = Mathf.Lerp(startY, targetY, t / discDropDuration);
                currentDisc.transform.position = new Vector3(currentDisc.transform.position.x, currentY, currentDisc.transform.position.z);

                yield return null;
            }

            currentDisc.transform.position = new Vector3(currentDisc.transform.position.x, targetY, currentDisc.transform.position.z);
            currentDiscRow = targetRow;
            PlaceDiscOn(currentPlayer, currentDiscRow, currentDiscCol, GetGameBoardMatrix());

            if (IsWinner(currentPlayer, currentDiscRow, currentDiscCol, GetGameBoardMatrix()))
            {
                SetWinner(currentPlayer);
                yield return 0;
            }
            else
            {
                if (IsTie())
                {
                    currentDisc = null;

                    swipeDetection.OnSwipeLeft -= ShiftDiscToLeft;
                    swipeDetection.OnSwipeRight -= ShiftDiscToRight;
                    swipeDetection.OnSwipeDown -= DropDisc;

                    inputManager.OnStartTouch += ResetGame;

                    gameManager.SetTieTextActive(true);

                    yield return 0;
                }
                else
                    yield return StartCoroutine(RotateBoard());
            }
        }
        else
            yield return 0;
    }

    private IEnumerator RunAITurn(int targetCol)
    {
        yield return new WaitForSeconds(aiMoveDelay);

        if (currentPlayer == Connect4Player.Black)
            yield return StartCoroutine(ShiftToColumn(targetCol));
        else if (currentPlayer == Connect4Player.White)
            yield return StartCoroutine(ShiftToColumn(6 - targetCol));

        yield return new WaitForSeconds(aiMoveDelay);
        yield return StartCoroutine("DropDisc");
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

    public void ResetGame(Vector2 position, float time)
    {
        ResetBoard();
        StartCoroutine(RotateBoard());
    }
}
