using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connect4Disc : MonoBehaviour
{
    public int color = 1;

    public float shiftDuration = 0.25f;
    public float dropDuration = 0.5f;
    public bool isPlayer = true;

    private Vector3 touchDownPosition, touchUpPosition;

    private bool isMoving = false;

    private Connect4Board gameBoard;
    private int currentRow, currentCol;

    // Start is called before the first frame update
    void Start()
    {
        gameBoard = FindObjectOfType<Connect4Board>();
        currentRow = -1;
        currentCol = gameBoard.GetColAt(transform.position.x, color);
    }

    IEnumerator ShiftToColumn(int targetCol)
    {
        float targetX = gameBoard.GetXAt(targetCol, color);

        if (Mathf.Abs(targetX) <= 15)
        {
            isMoving = true;
            float startX = gameBoard.GetXAt(currentCol, color);

            float t = 0;
            while (t <= shiftDuration)
            {
                t += Time.deltaTime;
                float currentX = Mathf.Lerp(startX, targetX, t / shiftDuration);
                transform.position = new Vector3(currentX, transform.position.y, transform.position.z);

                yield return null;
            }

            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
            currentCol = targetCol;

            isMoving = false;
        }
        else 
            yield return 0;
    }

    IEnumerator DropInBoard()
    {
        int targetRow = gameBoard.GetRowAvailableAt(currentCol);
        if (targetRow >= 0)
        {
            isMoving = true;

            float startY = transform.position.y;
            float targetY = gameBoard.GetYAt(targetRow);

            float t = 0;
            while (t <= dropDuration)
            {
                t += Time.deltaTime;
                float currentY = Mathf.Lerp(startY, targetY, t / dropDuration);
                transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

                yield return null;
            }

            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            currentRow = targetRow;
            gameBoard.PlaceDiscOn(color, currentRow, currentCol, gameBoard.GetGameBoardMatrix());

            isMoving = false;

            if (gameBoard.IsWinner(color, currentRow, currentCol, gameBoard.GetGameBoardMatrix()))
            {
                gameBoard.SetWinner(color);
                yield return 0;
            }
            else
                yield return StartCoroutine(gameBoard.Rotate());
        }
        else
            yield return 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRow == -1)
        {
            if (isPlayer && !isMoving)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchDownPosition = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    touchUpPosition = Input.mousePosition;
                    
                    Vector3 swipeDirection = (touchUpPosition - touchDownPosition).normalized;
                    
                    if (swipeDirection.y > -0.5f)
                    {
                        if (Mathf.Abs(swipeDirection.x) >= 0.5)
                        {
                            if (swipeDirection.x > 0)
                                StartCoroutine(ShiftToColumn(currentCol + color));
                            else if (swipeDirection.x < 0)
                                StartCoroutine(ShiftToColumn(currentCol - color));
                        }
                    }
                    else
                        StartCoroutine(DropInBoard());
                }
            }
        }
    }
}