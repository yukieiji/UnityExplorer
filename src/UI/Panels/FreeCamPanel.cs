using UniverseLib.Input;
using UniverseLib.UI;
using UniverseLib.UI.Models;
#if UNHOLLOWER
using UnhollowerRuntimeLib;
#endif
#if INTEROP
using Il2CppInterop.Runtime.Injection;
#endif

namespace UnityExplorer.UI.Panels
{
    internal class FreeCamPanel : UEPanel
    {
        public FreeCamPanel(UIBase owner) : base(owner)
        {
        }

        public override string Name => TranslationManager.Get("panel_name_freecam");
        public override UIManager.Panels PanelType => UIManager.Panels.Freecam;
        public override int MinWidth => 400;
        public override int MinHeight => 320;
        public override Vector2 DefaultAnchorMin => new(0.4f, 0.4f);
        public override Vector2 DefaultAnchorMax => new(0.6f, 0.6f);
        public override bool NavButtonWanted => true;
        public override bool ShouldSaveActiveState => true;

        internal static bool inFreeCamMode;
        internal static bool usingGameCamera;
        internal static Camera ourCamera;
        internal static Camera lastMainCamera;
        internal static FreeCamBehaviour freeCamScript;

        internal static float desiredMoveSpeed = 10f;

        internal static Vector3 originalCameraPosition;
        internal static Quaternion originalCameraRotation;

        internal static Vector3? currentUserCameraPosition;
        internal static Quaternion? currentUserCameraRotation;

        internal static Vector3 previousMousePosition;

        internal static Vector3 lastSetCameraPosition;

        static ButtonRef startStopButton;
        static Toggle useGameCameraToggle;
        static InputFieldRef positionInput;
        static InputFieldRef moveSpeedInput;
        static ButtonRef inspectButton;

        internal static void BeginFreecam()
        {
            inFreeCamMode = true;

            previousMousePosition = InputManager.MousePosition;

            CacheMainCamera();
            SetupFreeCamera();

            inspectButton.GameObject.SetActive(true);
        }

        static void CacheMainCamera()
        {
            Camera currentMain = Camera.main;
            if (currentMain)
            {
                lastMainCamera = currentMain;
                originalCameraPosition = currentMain.transform.position;
                originalCameraRotation = currentMain.transform.rotation;

                if (currentUserCameraPosition == null)
                {
                    currentUserCameraPosition = currentMain.transform.position;
                    currentUserCameraRotation = currentMain.transform.rotation;
                }
            }
            else
                originalCameraRotation = Quaternion.identity;
        }

        static void SetupFreeCamera()
        {
            if (useGameCameraToggle.isOn)
            {
                if (!lastMainCamera)
                {
                    ExplorerCore.LogWarning(TranslationManager.Get("log_no_previous_camera"));
                    useGameCameraToggle.isOn = false;
                }
                else
                {
                    usingGameCamera = true;
                    ourCamera = lastMainCamera;
                }
            }

            if (!useGameCameraToggle.isOn)
            {
                usingGameCamera = false;

                if (lastMainCamera)
                    lastMainCamera.enabled = false;
            }

            if (!ourCamera)
            {
                ourCamera = new GameObject("UE_Freecam").AddComponent<Camera>();
                ourCamera.gameObject.tag = "MainCamera";
                GameObject.DontDestroyOnLoad(ourCamera.gameObject);
                ourCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            if (!freeCamScript)
                freeCamScript = ourCamera.gameObject.AddComponent<FreeCamBehaviour>();

            ourCamera.transform.position = (Vector3)currentUserCameraPosition;
            ourCamera.transform.rotation = (Quaternion)currentUserCameraRotation;

            ourCamera.gameObject.SetActive(true);
            ourCamera.enabled = true;
        }

        internal static void EndFreecam()
        {
            inFreeCamMode = false;

            if (usingGameCamera)
            {
                ourCamera = null;

                if (lastMainCamera)
                {
                    lastMainCamera.transform.position = originalCameraPosition;
                    lastMainCamera.transform.rotation = originalCameraRotation;
                }
            }

            if (ourCamera)
                ourCamera.gameObject.SetActive(false);
            else
                inspectButton.GameObject.SetActive(false);

            if (freeCamScript)
            {
                GameObject.Destroy(freeCamScript);
                freeCamScript = null;
            }

            if (lastMainCamera)
                lastMainCamera.enabled = true;
        }

        static void SetCameraPosition(Vector3 pos)
        {
            if (!ourCamera || lastSetCameraPosition == pos)
                return;

            ourCamera.transform.position = pos;
            lastSetCameraPosition = pos;
        }

        internal static void UpdatePositionInput()
        {
            if (!ourCamera)
                return;

            if (positionInput.Component.isFocused)
                return;

            lastSetCameraPosition = ourCamera.transform.position;
            positionInput.Text = ParseUtility.ToStringForInput<Vector3>(lastSetCameraPosition);
        }

        // ~~~~~~~~ UI construction / callbacks ~~~~~~~~

        protected override void ConstructPanelContent()
        {
            startStopButton = UIFactory.CreateButton(ContentRoot, "ToggleButton", TranslationManager.Get("button_freecam"));
            UIFactory.SetLayoutElement(startStopButton.GameObject, minWidth: 150, minHeight: 25, flexibleWidth: 9999);
            startStopButton.OnClick += StartStopButton_OnClick;
            SetToggleButtonState();

            AddSpacer(5);

            GameObject toggleObj = UIFactory.CreateToggle(ContentRoot, "UseGameCameraToggle", out useGameCameraToggle, out Text toggleText);
            UIFactory.SetLayoutElement(toggleObj, minHeight: 25, flexibleWidth: 9999);
            useGameCameraToggle.onValueChanged.AddListener(OnUseGameCameraToggled);
            useGameCameraToggle.isOn = false;
            toggleText.text = TranslationManager.Get("toggle_use_game_camera");

            AddSpacer(5);

            GameObject posRow = AddInputField(TranslationManager.Get("label_position"),
                                              TranslationManager.Get("label_position") + ":", // Or a more specific key like "label_freecam_pos_colon"
                                              TranslationManager.Get("input_freecam_pos_placeholder"),
                                              out positionInput, PositionInput_OnEndEdit);

            ButtonRef resetPosButton = UIFactory.CreateButton(posRow, "ResetButton", TranslationManager.Get("button_reset")); // Assuming "button_reset" is general enough
            UIFactory.SetLayoutElement(resetPosButton.GameObject, minWidth: 70, minHeight: 25);
            resetPosButton.OnClick += OnResetPosButtonClicked;

            AddSpacer(5);

            AddInputField(TranslationManager.Get("label_movespeed"), // Or "label_move_speed"
                          TranslationManager.Get("label_move_speed_colon"),
                          TranslationManager.Get("input_movespeed_placeholder"),
                          out moveSpeedInput, MoveSpeedInput_OnEndEdit);
            moveSpeedInput.Text = desiredMoveSpeed.ToString();

            AddSpacer(5);

            Text instructionsText = UIFactory.CreateLabel(ContentRoot, "Instructions", TranslationManager.Get("text_freecam_controls"), TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(instructionsText.gameObject, flexibleWidth: 9999, flexibleHeight: 9999);

            AddSpacer(5);

            inspectButton = UIFactory.CreateButton(ContentRoot, "InspectButton", TranslationManager.Get("button_inspect_free_camera"));
            UIFactory.SetLayoutElement(inspectButton.GameObject, flexibleWidth: 9999, minHeight: 25);
            inspectButton.OnClick += () => { InspectorManager.Inspect(ourCamera); };
            inspectButton.GameObject.SetActive(false);

            AddSpacer(5);
        }

        void AddSpacer(int height)
        {
            GameObject obj = UIFactory.CreateUIObject("Spacer", ContentRoot);
            UIFactory.SetLayoutElement(obj, minHeight: height, flexibleHeight: 0);
        }

        GameObject AddInputField(string name, string labelText, string placeHolder, out InputFieldRef inputField, Action<string> onInputEndEdit)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(ContentRoot, $"{name}_Group", false, false, true, true, 3, default, new(1, 1, 1, 0));

            Text posLabel = UIFactory.CreateLabel(row, $"{name}_Label", labelText);
            UIFactory.SetLayoutElement(posLabel.gameObject, minWidth: 100, minHeight: 25);

            inputField = UIFactory.CreateInputField(row, $"{name}_Input", placeHolder);
            UIFactory.SetLayoutElement(inputField.GameObject, minWidth: 125, minHeight: 25, flexibleWidth: 9999);
            inputField.Component.GetOnEndEdit().AddListener(onInputEndEdit);

            return row;
        }

        void StartStopButton_OnClick()
        {
            EventSystemHelper.SetSelectedGameObject(null);

            if (inFreeCamMode)
                EndFreecam();
            else
                BeginFreecam();

            SetToggleButtonState();
        }

        void SetToggleButtonState()
        {
            if (inFreeCamMode)
            {
                RuntimeHelper.SetColorBlockAuto(startStopButton.Component, new(0.4f, 0.2f, 0.2f));
                startStopButton.ButtonText.text = TranslationManager.Get("button_end_freecam");
            }
            else
            {
                RuntimeHelper.SetColorBlockAuto(startStopButton.Component, new(0.2f, 0.4f, 0.2f));
                startStopButton.ButtonText.text = TranslationManager.Get("button_begin_freecam");
            }
        }

        void OnUseGameCameraToggled(bool value)
        {
            EventSystemHelper.SetSelectedGameObject(null);

            if (!inFreeCamMode)
                return;

            EndFreecam();
            BeginFreecam();
        }

        void OnResetPosButtonClicked()
        {
            currentUserCameraPosition = originalCameraPosition;
            currentUserCameraRotation = originalCameraRotation;

            if (inFreeCamMode && ourCamera)
            {
                ourCamera.transform.position = (Vector3)currentUserCameraPosition;
                ourCamera.transform.rotation = (Quaternion)currentUserCameraRotation;
            }

            positionInput.Text = ParseUtility.ToStringForInput<Vector3>(originalCameraPosition);
        }

        void PositionInput_OnEndEdit(string input)
        {
            EventSystemHelper.SetSelectedGameObject(null);

            if (!ParseUtility.TryParse(input, out Vector3 parsed, out Exception parseEx))
            {
                ExplorerCore.LogWarning(string.Format(TranslationManager.Get("log_could_not_parse_position"), parseEx.ReflectionExToString()));
                UpdatePositionInput();
                return;
            }

            SetCameraPosition(parsed);
        }

        void MoveSpeedInput_OnEndEdit(string input)
        {
            EventSystemHelper.SetSelectedGameObject(null);

            if (!ParseUtility.TryParse(input, out float parsed, out Exception parseEx))
            {
                ExplorerCore.LogWarning(string.Format(TranslationManager.Get("log_could_not_parse_value"), parseEx.ReflectionExToString()));
                moveSpeedInput.Text = desiredMoveSpeed.ToString();
                return;
            }

            desiredMoveSpeed = parsed;
        }
    }

    internal class FreeCamBehaviour : MonoBehaviour
    {
#if CPP
        static FreeCamBehaviour()
        {
            ClassInjector.RegisterTypeInIl2Cpp<FreeCamBehaviour>();
        }

        public FreeCamBehaviour(IntPtr ptr) : base(ptr) { }
#endif

        internal void Update()
        {
            if (FreeCamPanel.inFreeCamMode)
            {
                if (!FreeCamPanel.ourCamera)
                {
                    FreeCamPanel.EndFreecam();
                    return;
                }

                Transform transform = FreeCamPanel.ourCamera.transform;

                FreeCamPanel.currentUserCameraPosition = transform.position;
                FreeCamPanel.currentUserCameraRotation = transform.rotation;

                float moveSpeed = FreeCamPanel.desiredMoveSpeed * Time.deltaTime;

                if (InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetKey(KeyCode.RightShift))
                    moveSpeed *= 10f;

                if (InputManager.GetKey(KeyCode.LeftArrow) || InputManager.GetKey(KeyCode.A))
                    transform.position += transform.right * -1 * moveSpeed;

                if (InputManager.GetKey(KeyCode.RightArrow) || InputManager.GetKey(KeyCode.D))
                    transform.position += transform.right * moveSpeed;

                if (InputManager.GetKey(KeyCode.UpArrow) || InputManager.GetKey(KeyCode.W))
                    transform.position += transform.forward * moveSpeed;

                if (InputManager.GetKey(KeyCode.DownArrow) || InputManager.GetKey(KeyCode.S))
                    transform.position += transform.forward * -1 * moveSpeed;

                if (InputManager.GetKey(KeyCode.Space) || InputManager.GetKey(KeyCode.PageUp))
                    transform.position += transform.up * moveSpeed;

                if (InputManager.GetKey(KeyCode.LeftControl) || InputManager.GetKey(KeyCode.PageDown))
                    transform.position += transform.up * -1 * moveSpeed;

                if (InputManager.GetMouseButton(1))
                {
                    Vector3 mouseDelta = InputManager.MousePosition - FreeCamPanel.previousMousePosition;

                    float newRotationX = transform.localEulerAngles.y + mouseDelta.x * 0.3f;
                    float newRotationY = transform.localEulerAngles.x - mouseDelta.y * 0.3f;
                    transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
                }

                FreeCamPanel.UpdatePositionInput();

                FreeCamPanel.previousMousePosition = InputManager.MousePosition;
            }
        }
    }
}
