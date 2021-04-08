using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    #region Events
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;

    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;
    #endregion

    private TouchControls touchControls;

    private Camera mainCamera;

    void Awake()
    {
        touchControls = new TouchControls();
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        touchControls.Enable();
    }

    void OnDisable()
    {
        touchControls.Disable();
    }

    void Start()
    {
        touchControls.Touch.PrimaryContact.started += ctx => StartTouchPrimary(ctx);
        touchControls.Touch.PrimaryContact.canceled += ctx => EndTouchPrimary(ctx);
    }

    void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnStartTouch != null)
        {
            Vector2 touchPosition = touchControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            if (touchPosition == Vector2.zero)
            {
                StartCoroutine(ReadStartTouchAgain(context));
                return;
            }

            OnStartTouch(Utils.ScreenToWorld(mainCamera, touchPosition), (float)context.startTime);
        }
    }

    private IEnumerator ReadStartTouchAgain(InputAction.CallbackContext context)
    {
        yield return new WaitForEndOfFrame();

        Vector2 touchPosition = touchControls.Touch.PrimaryPosition.ReadValue<Vector2>();
        OnStartTouch(Utils.ScreenToWorld(mainCamera, touchPosition), (float)context.startTime);
    }

    void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndTouch != null)
        {
            OnEndTouch(Utils.ScreenToWorld(mainCamera, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>()), (float)context.time);
        }
    }

    public Vector2 PrimaryPosition()
    {
        return Utils.ScreenToWorld(mainCamera, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>());
    }
}
