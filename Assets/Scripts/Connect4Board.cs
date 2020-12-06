using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Connect4Board : MonoBehaviour
{
    public Connect4Disc blackDisc, whiteDisc;

    public int currentPlayer = 1;

    public float rotateDuration = 0.25f;

    private int[,] boardMatrix = new int[6, 7];

    private int winnerPlayer = 0;

    private LineRenderer winLineRenderer;
    private Vector3 startLinePosition, endLinePosition;

    // Start is called before the first frame update
    void Start()
    {
        winLineRenderer = GetComponent<LineRenderer>();

        ResetBoard();

        if (currentPlayer == 1)
        {
            Instantiate<Connect4Disc>(blackDisc, transform);
            transform.rotation = Quaternion.identity;
        }
        else if (currentPlayer == -1)
        {
            Instantiate<Connect4Disc>(whiteDisc, transform);
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }
    }

    void ResetBoard()
    {
        winnerPlayer = 0;
        winLineRenderer.enabled = false;

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                boardMatrix[row, col] = 0;
            }
        }

        GameObject [] blackDiscs = GameObject.FindGameObjectsWithTag("Black Disc");
        foreach (GameObject disc in blackDiscs)
            Destroy(disc);

        GameObject [] whiteDiscs = GameObject.FindGameObjectsWithTag("White Disc");
        foreach (GameObject disc in whiteDiscs)
            Destroy(disc);
    }

    public float GetXAt(int col, int player)
    {
        if (player == 1)
            return (5 * col) - 15;
        
        return (5 * (6 - col)) - 15;
    }

    public float GetYAt(int row)
    {
        return 10 - (5 * row);
    }

    public int GetColAt(float x, int player)
    {
        if (player == 1)
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

    public int[,] GetGameBoardMatrix()
    {
        return boardMatrix;
    }

    public void PlaceDiscOn(int player, int row, int col, int[,] board)
    {
        board[row, col] = player;
    }

    public bool IsWinner(int player, int row, int col, int [,] board)
    {
        bool horizontalCheck = IsWinnerAtRow(player, row, board);
        bool verticalCheck = IsWinnerAtCol(player, col, board);
        bool diagonalCheck = IsWinnerDiagonally(player, board);

        return horizontalCheck || verticalCheck || diagonalCheck;
    }

    private bool IsWinnerAtRow(int player, int row, int[,] board)
    {
        for (int col = 0; col <= 3; col++)
        {
            int count = board[row, col] + board[row, col + 1] + board[row, col + 2] + board[row, col + 3];
            if (count == player * 4)
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

    private bool IsWinnerAtCol(int player, int col, int[,] board)
    {
        for (int row = 0; row <= 2; row++)
        {
            int count = board[row, col] + board[row + 1, col] + board[row + 2, col] + board[row + 3, col];
            if (count == player * 4)
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

    private bool IsWinnerDiagonally(int player, int [,] board)
    {
        for (int col = 3; col < 7; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                int count = board[row, col] + board[row + 1, col - 1] + board[row + 2, col - 2] + board[row + 3, col - 3];
                if (count == player * 4)
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
                int count = board[row, col] + board[row - 1, col - 1] + board[row - 2, col - 2] + board[row - 3, col - 3];
                if (count == player * 4)
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

    public void SetWinner(int player)
    {
        winnerPlayer = player;

        winLineRenderer.enabled = true;
        winLineRenderer.startColor = player == 1 ? Color.white : Color.black;
        winLineRenderer.endColor = winLineRenderer.startColor;
        winLineRenderer.SetPosition(0, startLinePosition);
        winLineRenderer.SetPosition(1, endLinePosition);
    }

    public int GetWinner()
    {
        return winnerPlayer;
    }

    public IEnumerator Rotate()
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
        currentPlayer *= -1;

        yield return 0;

        if (currentPlayer == 1)
            Instantiate<Connect4Disc>(blackDisc, transform);
        else if (currentPlayer == -1)
            Instantiate<Connect4Disc>(whiteDisc, transform);
    }

    public void ResetGame()
    {
        ResetBoard();
        StartCoroutine(Rotate());
    }

    void Update()
    {
        if (winnerPlayer != 0)
        {
            if (Input.GetMouseButtonDown(0))
                ResetGame();
        }
    }
}
