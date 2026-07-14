// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace MinersWatch
{
    public partial class @PlayerControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerControls()
        {
            asset = InputActionAsset.FromJson(@"{
                ""name"": ""PlayerControls"",
                ""maps"": [
                    {
                        ""name"": ""Player"",
                        ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                        ""actions"": [
                            {
                                ""name"": ""Move"",
                                ""type"": ""Value"",
                                ""id"": ""b2c3d4e5-f6a7-8901-bcde-f12345678901"",
                                ""expectedControlType"": ""Vector2"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": true
                            },
                            {
                                ""name"": ""Jump"",
                                ""type"": ""Button"",
                                ""id"": ""c3d4e5f6-a7b8-9012-cdef-123456789012"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            },
                            {
                                ""name"": ""Interact"",
                                ""type"": ""Button"",
                                ""id"": ""d4e5f6a7-b8c9-0123-defa-234567890123"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            }
                        ],
                        ""bindings"": [
                            {
                                ""name"": ""WASD"",
                                ""id"": ""e5f6a7b8-c9d0-1234-efab-345678901234"",
                                ""path"": ""2DVector"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": true,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": ""Up"",
                                ""id"": ""f6a7b8c9-d0e1-2345-fabc-456789012345"",
                                ""path"": ""<Keyboard>/w"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Down"",
                                ""id"": ""a7b8c9d0-e1f2-3456-abcd-567890123456"",
                                ""path"": ""<Keyboard>/s"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Left"",
                                ""id"": ""b8c9d0e1-f2a3-4567-bcde-678901234567"",
                                ""path"": ""<Keyboard>/a"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Right"",
                                ""id"": ""c9d0e1f2-a3b4-5678-cdef-789012345678"",
                                ""path"": ""<Keyboard>/d"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Arrow Keys"",
                                ""id"": ""d0e1f2a3-b4c5-6789-defa-890123456789"",
                                ""path"": ""2DVector"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": true,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": ""Up"",
                                ""id"": ""e1f2a3b4-c5d6-7890-efab-901234567890"",
                                ""path"": ""<Keyboard>/upArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Down"",
                                ""id"": ""f2a3b4c5-d6e7-8901-fabc-012345678901"",
                                ""path"": ""<Keyboard>/downArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Left"",
                                ""id"": ""a3b4c5d6-e7f8-9012-abcd-123456789012"",
                                ""path"": ""<Keyboard>/leftArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Right"",
                                ""id"": ""b4c5d6e7-f8a9-0123-bcde-234567890123"",
                                ""path"": ""<Keyboard>/rightArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": """",
                                ""id"": ""c5d6e7f8-a9b0-1234-cdef-345678901234"",
                                ""path"": ""<Gamepad>/leftStick"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Gamepad"",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""d6e7f8a9-b0c1-2345-defa-456789012345"",
                                ""path"": ""<Keyboard>/space"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Jump"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""e7f8a9b0-c1d2-3456-efab-567890123456"",
                                ""path"": ""<Gamepad>/buttonSouth"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Gamepad"",
                                ""action"": ""Jump"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""f8a9b0c1-d2e3-4567-fabc-678901234567"",
                                ""path"": ""<Keyboard>/e"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard"",
                                ""action"": ""Interact"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""a9b0c1d2-e3f4-5678-abcd-789012345678"",
                                ""path"": ""<Gamepad>/buttonWest"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Gamepad"",
                                ""action"": ""Interact"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            }
                        ]
                    }
                ],
                ""controlSchemes"": [
                    {
                        ""name"": ""Keyboard"",
                        ""bindingGroup"": ""Keyboard"",
                        ""devices"": [
                            {
                                ""devicePath"": ""<Keyboard>"",
                                ""isOptional"": false,
                                ""isOR"": false
                            }
                        ]
                    },
                    {
                        ""name"": ""Gamepad"",
                        ""bindingGroup"": ""Gamepad"",
                        ""devices"": [
                            {
                                ""devicePath"": ""<Gamepad>"",
                                ""isOptional"": true,
                                ""isOR"": false
                            }
                        ]
                    }
                ]
            }");

            // Player
            m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
            m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
            m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
            m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
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

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Player
        private readonly InputActionMap m_Player;
        private IPlayerActions m_PlayerActionsCallbackInterface;
        private readonly InputAction m_Player_Move;
        private readonly InputAction m_Player_Jump;
        private readonly InputAction m_Player_Interact;

        public struct PlayerActions
        {
            private @PlayerControls m_Wrapper;

            public PlayerActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Move => m_Wrapper.m_Player_Move;
            public InputAction @Jump => m_Wrapper.m_Player_Jump;
            public InputAction @Interact => m_Wrapper.m_Player_Interact;

            public InputActionMap Get() { return m_Wrapper.m_Player; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;

            public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }

            public void SetCallbacks(IPlayerActions instance)
            {
                if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
                {
                    @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                    @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                    @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                    @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                    @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                    @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                    @Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                    @Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                    @Interact.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                }
                m_Wrapper.m_PlayerActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Move.started += instance.OnMove;
                    @Move.performed += instance.OnMove;
                    @Move.canceled += instance.OnMove;
                    @Jump.started += instance.OnJump;
                    @Jump.performed += instance.OnJump;
                    @Jump.canceled += instance.OnJump;
                    @Interact.started += instance.OnInteract;
                    @Interact.performed += instance.OnInteract;
                    @Interact.canceled += instance.OnInteract;
                }
            }
        }
        public PlayerActions @Player => new PlayerActions(this);

        public interface IPlayerActions
        {
            void OnMove(InputAction.CallbackContext context);
            void OnJump(InputAction.CallbackContext context);
            void OnInteract(InputAction.CallbackContext context);
        }
    }
}
