using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Connect4Board : MonoBehaviour
{
    public Connect4Disc blackDisc, whiteDisc;

    public int currentPlayer = 1;

    public float rotateDuration = 0.25f;

    private int[,] boardMatrix = new int[6, 7];

    private Connect4Disc currentDisc;

    // Start is called before the first frame update
    void Start()
    {
        ResetBoard();

        if (currentPlayer == 1)
        {
            currentDisc = Instantiate<Connect4Disc>(blackDisc, transform);
            transform.rotation = Quaternion.identity;
        }
        else if (currentPlayer == -1)
        {
            currentDisc = Instantiate<Connect4Disc>(whiteDisc, transform);
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }
    }

    void ResetBoard()
    {
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

    public void PlaceDiscOn(int player, int row, int col)
    {
        boardMatrix[row, col] = player;
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
            currentDisc = Instantiate<Connect4Disc>(blackDisc, transform);
        else if (currentPlayer == -1)
            currentDisc = Instantiate<Connect4Disc>(whiteDisc, transform);
    }
}
