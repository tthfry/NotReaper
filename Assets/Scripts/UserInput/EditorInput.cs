using System.Collections;
using NotReaper.Grid;
using NotReaper.Managers;
using NotReaper.Models;
using NotReaper.Targets;
using NotReaper.Tools;
using NotReaper.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NotReaper.UserInput {


	public enum EditorTool { Standard, Hold, Horizontal, Vertical, ChainStart, ChainNode, Melee, DragSelect, ChainBuilder, None }


	public class EditorInput : MonoBehaviour {

		public static EditorTool selectedTool = EditorTool.Standard;
        public static EditorTool previousTool = EditorTool.Standard;
		public static TargetHandType selectedHand = TargetHandType.Left;
		public static TargetHandType previousHand = TargetHandType.Left;
		public static SnappingMode selectedSnappingMode = SnappingMode.Grid;
		public static TargetBehavior selectedBehavior = TargetBehavior.Standard;
		public static UITargetVelocity selectedVelocity = UITargetVelocity.Standard;

		public static EditorMode selectedMode = EditorMode.Compose;
		public static bool isOverGrid = false;
		public static bool inUI = false;
		public static bool isFocusGrid = false;

		//public PlaceNote toolPlaceNote;
		[SerializeField] public EditorToolkit Tools;

		public PauseMenu pauseMenu;
		public SoundSelect soundSelect;
		[SerializeField] private Timeline timeline;

		[SerializeField] private TMP_Dropdown soundDropdown;

		[SerializeField] private UIToolSelect uiToolSelect;
		[SerializeField] private HandTypeSelect handTypeSelect;

		[SerializeField] private GameObject focusGrid;


		public HoverTarget hover;


		public GameObject normalGrid;
		public GameObject noGrid;
		public GameObject meleeGrid;



		public UIModeSelect editorMode;

		bool isCTRLDown;
		bool isShiftDown;

		private void Start() {
			InputManager.LoadHotkeys();



			StartCoroutine(WaitForUserColors());


		}

		IEnumerator WaitForUserColors() {
			while (!NRSettings.isLoaded) {
				yield return new WaitForSeconds(0.1f);
			}

			SelectMode(EditorMode.Compose);
			SelectTool(EditorTool.Standard);
			SelectHand(TargetHandType.Left);
			SelectVelocity(UITargetVelocity.Standard);

			NotificationShower.AddNotifToQueue(new NRNotification("Welcome to NotReaper!", 8f));

			pauseMenu.LoadUIColors();
			pauseMenu.OpenPauseMenu();

			soundSelect.LoadUIColors();

			timeline.UpdateUIColors();

			FigureOutIsInUI();


		}

		public static Color GetSelectedColor() {
			if (selectedHand == TargetHandType.Left) {
				return NRSettings.config.leftColor;
			} else if (selectedHand == TargetHandType.Right) {
				return NRSettings.config.rightColor;
			}
			return Color.gray;
		}
		public static Color GetOppositeSelectedColor() {
			if (selectedHand == TargetHandType.Left) {
				return NRSettings.config.rightColor;
			} else if (selectedHand == TargetHandType.Right) {
				return NRSettings.config.leftColor;
			}
			return Color.gray;
		}

		public void FocusGrid(bool focus) {
			focusGrid.SetActive(focus);

			isFocusGrid = focus;
		}

		public void SelectSnappingMode(SnappingMode mode) {
			selectedSnappingMode = mode;

			switch (mode) {
				case SnappingMode.Grid:
					normalGrid.SetActive(true);
					noGrid.SetActive(false);
					meleeGrid.SetActive(false);
					break;
				case SnappingMode.None:
					normalGrid.SetActive(false);
					noGrid.SetActive(true);
					meleeGrid.SetActive(false);
					break;
				case SnappingMode.Melee:
					normalGrid.SetActive(false);
					noGrid.SetActive(true);
					meleeGrid.SetActive(true);
					break;

			}
		}


		public void SelectHand(TargetHandType type) {
			selectedHand = type;

			uiToolSelect.UpdateUINoteSelected(selectedTool);
			handTypeSelect.UpdateUI(type);
			hover.UpdateUIHandColor(GetSelectedColor());


		}

		/// <summary>
		/// Select a sound for the current tool.
		/// </summary>
		/// <param name="velocity">The "sound" type to play.</param>
		public void SelectVelocity(UITargetVelocity velocity) {

			switch (velocity) {
				case UITargetVelocity.Standard:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Standard);
					break;
				case UITargetVelocity.Snare:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Snare);
					break;

				case UITargetVelocity.Percussion:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Percussion);
					break;

				case UITargetVelocity.ChainStart:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.ChainStart);
					break;

				case UITargetVelocity.Chain:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Chain);
					break;

				case UITargetVelocity.Melee:
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Melee);
					break;


			}

			selectedVelocity = velocity;

		}

		public void SelectMode(EditorMode mode) {

			if (selectedMode == EditorMode.Timing && Timeline.inTimingMode) return;

			editorMode.UpdateUI(mode);
            selectedMode = mode;

			FigureOutIsInUI();
		}

		public void SelectTool(EditorTool tool) {
			if(tool == selectedTool) {
				return;
			}

			uiToolSelect.UpdateUINoteSelected(tool);

			hover.UpdateUITool(tool);

            previousTool = selectedTool;
			selectedTool = tool;

			if(previousTool == EditorTool.Melee && selectedHand == TargetHandType.Either) {
				SelectHand(previousHand);
			}

			//Update the UI based on the tool:
			switch (tool) {
				case EditorTool.Standard:
					selectedBehavior = TargetBehavior.Standard;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Standard);
					SelectVelocity(UITargetVelocity.Standard);
					SelectSnappingMode(SnappingMode.Grid);

					break;

				case EditorTool.Hold:
					selectedBehavior = TargetBehavior.Hold;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Standard);
					SelectVelocity(UITargetVelocity.Standard);
					SelectSnappingMode(SnappingMode.Grid);
					break;

				case EditorTool.Horizontal:
					selectedBehavior = TargetBehavior.Horizontal;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Standard);
					SelectVelocity(UITargetVelocity.Standard);
					SelectSnappingMode(SnappingMode.Grid);
					break;

				case EditorTool.Vertical:
					selectedBehavior = TargetBehavior.Vertical;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Standard);
					SelectVelocity(UITargetVelocity.Standard);
					SelectSnappingMode(SnappingMode.Grid);
					break;

				case EditorTool.ChainStart:
					selectedBehavior = TargetBehavior.ChainStart;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.ChainStart);
					SelectVelocity(UITargetVelocity.ChainStart);
					SelectSnappingMode(SnappingMode.Grid);
					break;

				case EditorTool.ChainNode:
					selectedBehavior = TargetBehavior.Chain;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Chain);
					SelectVelocity(UITargetVelocity.ChainStart);
					SelectSnappingMode(SnappingMode.None);
					break;

				case EditorTool.Melee:
					selectedBehavior = TargetBehavior.Melee;
					previousHand = selectedHand;
					soundDropdown.SetValueWithoutNotify((int) UITargetVelocity.Melee);
					SelectVelocity(UITargetVelocity.Melee);
					SelectSnappingMode(SnappingMode.Melee);
					SelectHand(TargetHandType.Either);
					break;

				case EditorTool.DragSelect:
					selectedBehavior = TargetBehavior.None;



					Tools.dragSelect.Activate(true);
					Tools.chainBuilder.Activate(false);
					break;

				case EditorTool.ChainBuilder:
					selectedBehavior = TargetBehavior.None;

					Tools.dragSelect.Activate(false);
					Tools.chainBuilder.Activate(true);
					break;


				default:
					break;
			}


		}

        public void RevertTool() {
            SelectTool(previousTool);
            previousTool = selectedTool;
        }

        public void FigureOutIsInUI() {

			if (pauseMenu.isOpened) {
				inUI = true;
				return;
			}

			switch (selectedMode) {
				case EditorMode.Compose:
					inUI = false;
					break;

				case EditorMode.Metadata:
				case EditorMode.Timing:
				case EditorMode.Settings:
					inUI = true;
					break;
				
			}
		}

        private void Update() {

			if (Timeline.inTimingMode && inUI) {
				if (Input.GetKeyDown(InputManager.timelineTogglePlay)) {
					timeline.TogglePlayback();
				}
			}

			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) {
				timeline.Export();
			}
			



			if (Input.GetKeyDown(KeyCode.Escape)) {
				if (pauseMenu.isOpened)
				{
					if (!Timeline.audioLoaded) return;

					pauseMenu.ClosePauseMenu();
					FigureOutIsInUI();
				} else {
					if (selectedMode == EditorMode.Timing) return;
					pauseMenu.OpenPauseMenu();
					FigureOutIsInUI();
				}
			}

			if (inUI) return;

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) {
                SelectTool(EditorTool.DragSelect);
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) {
				if (Tools.dragSelect.isDraggingNotesOnTimeline) {
					Tools.dragSelect.EndDragTimelineTargetAction();
				}
				else if (Tools.dragSelect.isDraggingNotesOnGrid) {
					Tools.dragSelect.EndDragGridTargetAction();
				}

				Tools.dragSelect.EndTimelineDrag();
				Tools.dragSelect.EndGridDrag();

				foreach (Target target in timeline.selectedNotes) {

						//TODO: does changing target in one list change it in another? no i don't think so

						target.gridTargetPos = target.gridTargetIcon.transform.localPosition;
					}

				Tools.dragSelect.EndSelectionAction();

                RevertTool();
				
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
				isCTRLDown = true;
			} else {
				isCTRLDown = false;
			}

			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				isShiftDown = true;
			} else {
				isShiftDown = false;
			}


			if (Input.GetKeyDown(KeyCode.T)) {
				//Tools.chainBuilder.NewChain(new Vector2(0, 0));
			}


			if (Input.GetMouseButtonDown(0)) {

				switch (selectedTool) {
					case EditorTool.Standard:
					case EditorTool.Hold:
					case EditorTool.Horizontal:
					case EditorTool.Vertical:
					case EditorTool.ChainStart:
					case EditorTool.ChainNode:
					case EditorTool.Melee:
						Tools.placeNote.TryPlaceNote();
						break;

					case EditorTool.ChainBuilder:
						//We let the chain builder script handle input since we activated it from SelectTool()
						break;


					default:
						break;
				}


			}

			if (Input.GetMouseButtonDown(1)) {
				switch (selectedTool) {
					case EditorTool.Standard:
					case EditorTool.Hold:
					case EditorTool.Horizontal:
					case EditorTool.Vertical:
					case EditorTool.ChainStart:
					case EditorTool.ChainNode:
					case EditorTool.Melee:
						Tools.placeNote.TryRemoveNote();
						break;


					default:
						break;
				}
			}


			if (Input.GetKeyDown(InputManager.timelineTogglePlay)) {
				timeline.TogglePlayback();
			}


			if (Input.GetKeyDown(InputManager.selectStandard)) {

				SelectTool(EditorTool.Standard);

			}
			if (Input.GetKeyDown(InputManager.selectHold)) {
				SelectTool(EditorTool.Hold);
			}
			if (Input.GetKeyDown(InputManager.selectHorz)) {
				SelectTool(EditorTool.Horizontal);
			}
			if (Input.GetKeyDown(InputManager.selectVert)) {
				SelectTool(EditorTool.Vertical);
			}
			if (Input.GetKeyDown(InputManager.selectChainStart)) {
				SelectTool(EditorTool.ChainStart);
			}
			if (Input.GetKeyDown(InputManager.selectChainNode)) {
				SelectTool(EditorTool.ChainNode);
			}
			if (Input.GetKeyDown(InputManager.selectMelee)) {
				SelectTool(EditorTool.Melee);
			}

			if (Input.GetKeyDown(KeyCode.V) && !isCTRLDown) {
				SelectTool(EditorTool.DragSelect);
			}

			if (Input.GetKeyDown(KeyCode.G)) {
				SelectSnappingMode(SnappingMode.Grid);
			}
			if (Input.GetKeyDown(KeyCode.N)) {
				SelectSnappingMode(SnappingMode.None);
			}
			if (Input.GetKeyDown(KeyCode.M)) {
				SelectSnappingMode(SnappingMode.Melee);
			}


			if (Input.GetKeyDown(InputManager.toggleColor)) {

				if (selectedHand == TargetHandType.Left) {
					SelectHand(TargetHandType.Right);
				} else if (selectedHand == TargetHandType.Right) {
					SelectHand(TargetHandType.Left);
				} else {
					SelectHand(TargetHandType.Left);
				}

			}

			if (Input.GetKeyDown(InputManager.selectSoundKick)) {
				soundDropdown.value = 0;
			} else if (Input.GetKeyDown(InputManager.selectSoundSnare)) {
				soundDropdown.value = 1;
			} else if (Input.GetKeyDown(InputManager.selectSoundPercussion)) {
				soundDropdown.value = 2;
			} else if (Input.GetKeyDown(InputManager.selectSoundChainStart)) {
				soundDropdown.value = 3;
			} else if (Input.GetKeyDown(InputManager.selectSoundChainNode)) {
				soundDropdown.value = 4;
			} else if (Input.GetKeyDown(InputManager.selectSoundMelee)) {
				soundDropdown.value = 5;
			}


			if (!isShiftDown && isCTRLDown && Input.GetKeyDown(InputManager.undo)) {
				Tools.undoRedoManager.Undo();
				Debug.Log("Undoing...");
			}

			if (isShiftDown && isCTRLDown && Input.GetKeyDown(InputManager.redo)) {
				Tools.undoRedoManager.Redo();
				Debug.Log("Redoing...");
			}

		}

	}


}