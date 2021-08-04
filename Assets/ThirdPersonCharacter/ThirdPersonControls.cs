// GENERATED AUTOMATICALLY FROM 'Assets/ThirdPersonCharacter/ThirdPersonControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @ThirdPersonControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @ThirdPersonControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ThirdPersonControls"",
    ""maps"": [
        {
            ""name"": ""Camera"",
            ""id"": ""cb4786bd-5db7-4bae-a5d2-e71346989a81"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""PassThrough"",
                    ""id"": ""10f6be21-fa32-4544-b35d-9c6ebce3c330"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""StickDeadzone"",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TargetView"",
                    ""type"": ""Button"",
                    ""id"": ""a0a2fddd-222b-4d33-925a-b82dfbe50ff5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ExitFPV"",
                    ""type"": ""Button"",
                    ""id"": ""ee3c1648-87ea-4a5e-9841-e5e5800333d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""db1e7124-f523-477a-b1ee-940b157691fc"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""TargetView"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a6549ea8-075a-46e4-b07a-cf85ec3bc248"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""ExitFPV"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d146959e-b4db-4e1f-bdc9-32b2bd886dc4"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""ExitFPV"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d531ee17-651b-4cd0-9f0f-335d22f0b6e4"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c2eae0e7-0978-43fd-b757-847261360c9f"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.1,y=0.1)"",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Movement"",
            ""id"": ""d450de68-f619-41a6-9a5c-9385dac66178"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""7e7dfda0-c0cc-42cb-b833-3afa081fb5a5"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""77a3db56-5ee6-4979-88c9-d352e1de20c6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""cab557b6-f898-400a-a706-266f82c1805b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""a9bf65e9-4031-425e-b949-9770aaf5eaf3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Roll"",
                    ""type"": ""Button"",
                    ""id"": ""c5653e8d-8b71-4843-8343-e64a2b8f82c2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""dcfee42b-ca14-4be7-98d6-259650e9b901"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""4401d8fd-6a6b-4c54-8981-ad699da424df"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""69b5218b-5c20-483f-afbb-f4c11aaae91a"",
                    ""path"": ""<Keyboard>/#(W)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a69ba73e-624b-43ff-80ae-0b1f06470a9a"",
                    ""path"": ""<Keyboard>/#(S)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""2b6747d0-e3a8-4d1a-bf7d-01ae4b3d4855"",
                    ""path"": ""<Keyboard>/#(A)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""267d86ef-5009-49cf-91e2-8268d585b8b3"",
                    ""path"": ""<Keyboard>/#(D)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5ce60a31-1cc2-40c3-b4dc-92484a11af6a"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be468282-5b06-4537-a93c-de3d73226dd2"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""56b1e74d-6408-4ed3-be2f-deb1d1002ced"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8af99a06-4ba0-4976-b7fe-0e9f23bf87b9"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3a689fa-6628-4495-80ae-3c8aa1095161"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a830250a-c097-4333-93b2-8a058fd033b4"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""07c19899-ac65-414e-ba62-47967f339747"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65ebd264-5813-497e-8be8-c42251d8c87a"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""ThirdPersonCharacterControlScheme"",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""ThirdPersonCharacterControlScheme"",
            ""bindingGroup"": ""ThirdPersonCharacterControlScheme"",
            ""devices"": []
        }
    ]
}");
        // Camera
        m_Camera = asset.FindActionMap("Camera", throwIfNotFound: true);
        m_Camera_Look = m_Camera.FindAction("Look", throwIfNotFound: true);
        m_Camera_TargetView = m_Camera.FindAction("TargetView", throwIfNotFound: true);
        m_Camera_ExitFPV = m_Camera.FindAction("ExitFPV", throwIfNotFound: true);
        // Movement
        m_Movement = asset.FindActionMap("Movement", throwIfNotFound: true);
        m_Movement_Move = m_Movement.FindAction("Move", throwIfNotFound: true);
        m_Movement_Jump = m_Movement.FindAction("Jump", throwIfNotFound: true);
        m_Movement_Sprint = m_Movement.FindAction("Sprint", throwIfNotFound: true);
        m_Movement_Crouch = m_Movement.FindAction("Crouch", throwIfNotFound: true);
        m_Movement_Roll = m_Movement.FindAction("Roll", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Camera
    private readonly InputActionMap m_Camera;
    private ICameraActions m_CameraActionsCallbackInterface;
    private readonly InputAction m_Camera_Look;
    private readonly InputAction m_Camera_TargetView;
    private readonly InputAction m_Camera_ExitFPV;
    public struct CameraActions
    {
        private @ThirdPersonControls m_Wrapper;
        public CameraActions(@ThirdPersonControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_Camera_Look;
        public InputAction @TargetView => m_Wrapper.m_Camera_TargetView;
        public InputAction @ExitFPV => m_Wrapper.m_Camera_ExitFPV;
        public InputActionMap Get() { return m_Wrapper.m_Camera; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraActions set) { return set.Get(); }
        public void SetCallbacks(ICameraActions instance)
        {
            if (m_Wrapper.m_CameraActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnLook;
                @TargetView.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnTargetView;
                @TargetView.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnTargetView;
                @TargetView.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnTargetView;
                @ExitFPV.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnExitFPV;
                @ExitFPV.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnExitFPV;
                @ExitFPV.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnExitFPV;
            }
            m_Wrapper.m_CameraActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @TargetView.started += instance.OnTargetView;
                @TargetView.performed += instance.OnTargetView;
                @TargetView.canceled += instance.OnTargetView;
                @ExitFPV.started += instance.OnExitFPV;
                @ExitFPV.performed += instance.OnExitFPV;
                @ExitFPV.canceled += instance.OnExitFPV;
            }
        }
    }
    public CameraActions @Camera => new CameraActions(this);

    // Movement
    private readonly InputActionMap m_Movement;
    private IMovementActions m_MovementActionsCallbackInterface;
    private readonly InputAction m_Movement_Move;
    private readonly InputAction m_Movement_Jump;
    private readonly InputAction m_Movement_Sprint;
    private readonly InputAction m_Movement_Crouch;
    private readonly InputAction m_Movement_Roll;
    public struct MovementActions
    {
        private @ThirdPersonControls m_Wrapper;
        public MovementActions(@ThirdPersonControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Movement_Move;
        public InputAction @Jump => m_Wrapper.m_Movement_Jump;
        public InputAction @Sprint => m_Wrapper.m_Movement_Sprint;
        public InputAction @Crouch => m_Wrapper.m_Movement_Crouch;
        public InputAction @Roll => m_Wrapper.m_Movement_Roll;
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Sprint.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnSprint;
                @Crouch.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                @Crouch.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                @Crouch.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnCrouch;
                @Roll.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnRoll;
                @Roll.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnRoll;
                @Roll.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnRoll;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
                @Roll.started += instance.OnRoll;
                @Roll.performed += instance.OnRoll;
                @Roll.canceled += instance.OnRoll;
            }
        }
    }
    public MovementActions @Movement => new MovementActions(this);
    private int m_ThirdPersonCharacterControlSchemeSchemeIndex = -1;
    public InputControlScheme ThirdPersonCharacterControlSchemeScheme
    {
        get
        {
            if (m_ThirdPersonCharacterControlSchemeSchemeIndex == -1) m_ThirdPersonCharacterControlSchemeSchemeIndex = asset.FindControlSchemeIndex("ThirdPersonCharacterControlScheme");
            return asset.controlSchemes[m_ThirdPersonCharacterControlSchemeSchemeIndex];
        }
    }
    public interface ICameraActions
    {
        void OnLook(InputAction.CallbackContext context);
        void OnTargetView(InputAction.CallbackContext context);
        void OnExitFPV(InputAction.CallbackContext context);
    }
    public interface IMovementActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnRoll(InputAction.CallbackContext context);
    }
}
