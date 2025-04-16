using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("InputManager");
                instance = go.AddComponent<InputManager>();
            }
            return instance;
        }
    }

    private PlayerInput playerInput;
    [SerializeField] private InputActionReference selectReference, cancelReference, holdReference, rotateReference, addReference, attackEnableReference;
    private InputAction selectAction, cancelAction, holdAction, rotateAction, addAction, attackEnableAction;

    private static InputSystemUIInputModule s_Module;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        s_Module = null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        playerInput = GetComponent<PlayerInput>();
        selectAction = playerInput.actions[selectReference.name];
        cancelAction = playerInput.actions[cancelReference.name];
        holdAction = playerInput.actions[holdReference.name];
        rotateAction = playerInput.actions[rotateReference.name];
        addAction = playerInput.actions[addReference.name];
        attackEnableAction = playerInput.actions[attackEnableReference.name];
    }

    private void OnEnable()
    {
        selectAction.performed += context => HandleInput(SelectPerformed, true);
        selectAction.canceled += context => HandleInput(SelectCanceled, true);
        cancelAction.performed += context => HandleInput(CancelPerformed, true);
        holdAction.started += context => HandleInput(HoldStarted, true);
        holdAction.performed += context => HandleInput(HoldPerformed, true);
        holdAction.canceled += context => HandleInput(HoldReleased, false);
        rotateAction.performed += context => HandleInput(RotateStarted, true);
        rotateAction.canceled += context => HandleInput(RotateReleased, false);
        addAction.performed += context => HandleInput(AddPerformed, true);
        addAction.canceled += context => HandleInput(AddCanceled, false);
        attackEnableAction.performed += context => HandleInput(AttackEnablePerformed, false);
    }

    private void OnDisable()
    {
        selectAction.performed -= context => HandleInput(SelectPerformed, true);
        selectAction.canceled -= context => HandleInput(SelectCanceled, true);
        cancelAction.performed -= context => HandleInput(CancelPerformed, true);
        holdAction.started -= context => HandleInput(HoldStarted, true);
        holdAction.performed -= context => HandleInput(HoldPerformed, true);
        holdAction.canceled -= context => HandleInput(HoldReleased, false);
        rotateAction.performed -= context => HandleInput(RotateStarted, true);
        rotateAction.canceled -= context => HandleInput(RotateReleased, false);
        addAction.performed -= context => HandleInput(AddPerformed, true);
        addAction.canceled -= context => HandleInput(AddCanceled, false);
        attackEnableAction.performed -= context => HandleInput(AttackEnablePerformed, false);
    }

    public event System.Action SelectPerformed;
    public event System.Action SelectCanceled;
    public event System.Action CancelPerformed;
    public event System.Action HoldStarted;
    public event System.Action HoldPerformed;
    public event System.Action HoldReleased;
    public event System.Action RotateStarted;
    public event System.Action RotateReleased;
    public event System.Action AddPerformed;
    public event System.Action AddCanceled;
    public event System.Action AttackEnablePerformed;

    private void HandleInput(System.Action action, bool checkUI)
    {
        if (!checkUI || !IsPointerOverGUIAction())
        {
            action?.Invoke();
        }
    }

    public static bool IsPointerOverGUIAction()
    {
        if (!EventSystem.current)
        {
            return false;
        }

        if (!s_Module)
        {
            s_Module = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
        }

        return s_Module.GetLastRaycastResult(Pointer.current.deviceId).isValid;
    }
}