using UnityEngine;
using System.Collections;

public class SwipeDetection : Singleton<SwipeDetection>
{
    #region Direction Events
    public delegate void SwipeLeft();
    public event SwipeLeft OnSwipeLeft;

    public delegate void SwipeRight();
    public event SwipeRight OnSwipeRight;

    public delegate void SwipeDown();
    public event SwipeDown OnSwipeDown;
    #endregion

    [SerializeField]
    private float minimumDistance = 0.2f;
     [SerializeField]
    private float maximumTime = 1f;
    [SerializeField, Range(0f, 1f)]
    private float directionThreshold = 0.9f;

    [SerializeField]
    private float trailPositionZ = -10f;

#pragma warning disable 0649
    [SerializeField]
    private GameObject trail; 
#pragma warning restore 0649

    private InputManager inputManager;

    private Vector2 startPosition;
    private float startTime;

    private Vector2 endPosition;
    private float endTime;

    private Coroutine trailCoroutine;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;

        trail.SetActive(true);
        trail.transform.position = position;
        trailCoroutine = StartCoroutine("Trail");
    }

    private IEnumerator Trail()
    {
        while (true)
        {
            trail.transform.position = new Vector3(inputManager.PrimaryPosition().x, inputManager.PrimaryPosition().y, trailPositionZ);
            yield return null;
        }
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        trail.SetActive(false);
        StopCoroutine(trailCoroutine);

        endPosition = position;
        endTime = time;

        DetectSwipe();
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.left, direction) > directionThreshold && OnSwipeLeft != null)
        {
            OnSwipeLeft();
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold && OnSwipeRight != null)
        {
            OnSwipeRight();
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold && OnSwipeDown != null)
        {
            OnSwipeDown();
        }
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition, endPosition) >= minimumDistance && 
            (endTime - startTime) <= maximumTime)
        {
            Debug.DrawLine(startPosition, endPosition, Color.red, 5f);

            Vector3 direction = endPosition - startPosition;
            Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
            SwipeDirection(direction2D);
        }
    }

    public void SetTrailColor(Color color)
    {
        TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
        trailRenderer.startColor = color;
        trailRenderer.endColor = color;
    }
}
