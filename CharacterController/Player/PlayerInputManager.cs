using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private Transform m_TPCameraTransform;
    [Header("References")] 
    [SerializeField] private CinemachineInputProvider m_CameraInputProvider;

    #region MovementInput

    public bool EnterGlideInput => FindPlayerInputAction("Glide").WasPressedThisFrame();


    public bool ExitGlideInput => FindPlayerInputAction("Glide").WasReleasedThisFrame();
    public bool JumpInput => FindPlayerInputAction("Jump").WasPerformedThisFrame();
    public bool RollInput => FindPlayerInputAction("Roll").WasPressedThisFrame();
    public Vector3 Horizontal => GetWorldToCameraVector(FindPlayerInputAction("Horizontal").ReadValue<Vector2>());
    public Vector3 CameraInput => FindPlayerInputAction("Camera").ReadValue<Vector2>();
    #endregion

    #region Create
    public bool CreateLeftInput => FindPlayerInputAction("CreateLeft").WasPressedThisFrame();
    public bool CreateRightInput => FindPlayerInputAction("CreateRight").WasPressedThisFrame();
    public bool Ability => FindPlayerInputAction("Ability").WasPressedThisFrame();
    #endregion

    #region Throw
    public bool ThrowInputDown => FindPlayerInputAction("Throw").WasPressedThisFrame();
    public bool ThrowInputUp => FindPlayerInputAction("Throw").WasReleasedThisFrame();
    public bool CancelThrowInput => FindPlayerInputAction("CancelThrow").WasPressedThisFrame();
    public bool PickUpInput => FindPlayerInputAction("PickUp").WasPressedThisFrame();
    public bool PutDownInput => FindPlayerInputAction("PutDown").WasPressedThisFrame();
    #endregion
    
    #region Attack
    public bool AttackInput => FindPlayerInputAction("Attack").WasPressedThisFrame();

    #endregion

    public bool EnterInteractInput => FindPlayerInputAction("Interact").WasPressedThisFrame();
    public bool ExitInteractInput => FindPlayerInputAction("Interact").WasReleasedThisFrame();
    
    private void Awake()
    {
        m_TPCameraTransform = Camera.main.transform;
    }
    private static InputAction FindPlayerInputAction(string actionName)
    {
        return GameManagerStateMachine.Instance.Context.StaticData.Player.Input
            .FindActionMap("Player").FindAction(actionName);
    }

    private void Start()
    {
        GameManagerStateMachine.Instance.Context.StaticData.Player.Input.FindActionMap("Player").Enable();
    }

    // public void LockCameraInput(bool locked)
    // {
    //     if (m_CameraInputProvider == null)
    //     {
    //         m_CameraInputProvider = VirtualCameraManager.GetInstance().GetCVC(CustomVirtualCamera.VcType.Standard)
    //             .GetComponent<CinemachineInputProvider>();
    //     }
    //     m_CameraInputProvider.enabled = !locked;
    // }
    
    private Vector3 GetWorldToCameraVector(Vector2 input)
    {
        Vector3 l_Forward = m_TPCameraTransform.forward;
        l_Forward.y = 0;
        l_Forward.Normalize();
        Vector3 l_Right = m_TPCameraTransform.right;
        l_Right.y = 0;
        l_Right.Normalize();
        Vector2 l_Input = FindPlayerInputAction("Horizontal").ReadValue<Vector2>();
        return l_Forward * l_Input.y + l_Right * l_Input.x;
    }
}
