using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Lightweight tutorial manager:
// - Dialogue (typewriter; click/Space to reveal, then click/Space to advance)
// - Click a specific world target
// - Click a specific UI Button
public class TutorialManager : SingletonBehaviour<TutorialManager>
{
	public event Action OnTutorialEnded;

	#region Serialized Fields
	[Header("UI")]
	[SerializeField]
	public GameObject DialogueBox;

	[SerializeField]
	private TMP_Text dialogueText;

	[SerializeField]
	private GameObject canSkipIndicator;

	[SerializeField]
	private TutorialRaycastBlocker overlay;

	[SerializeField]
	private Camera worldCamera;

	[SerializeField]
	private List<Canvas> canvasesToDisableAllButtonsIn;

	[SerializeField]
	private List<Button> individualButtonsToDisable;

	[Tooltip("Enable or disable the typewriter feature globally.")]
	[SerializeField]
	private bool enableTypewriter = true;

	[Tooltip("Default characters per second for the typewriter effect. Can be overridden per step.")]
	[SerializeField]
	private float defaultTypewriterCharsPerSecond = 40f;

	[SerializeField]
	private bool enableArrow = true;

	[SerializeField]
	public ArrowIndicator arrow; // intentional exposure

	[SerializeField]
	private float arrowAngleForUI = 0f; // reserved

	[SerializeField]
	private float arrowAngleForWorld = 0f; // reserved

	[SerializeField]
	private Vector2 arrowUIOffset = new(0, -40f);

	[SerializeField]
	private Vector2 arrowWorldOffset = new(0, -2f);

	[SerializeField]
	private bool overrideDialogBoxTransform;

	[SerializeField]
	public RectTransformSnapshot defaultDialogBoxTransform;

	[Header("Steps")]
	[SerializeField]
	private List<SimpleTutorialStep> steps = new();

	public SimpleTutorialStep GetStepAtIndex(int index) =>
		(index >= 0 && index < steps.Count) ? steps[index] : null;

	[Header("Callbacks")]
	[Tooltip("If enabled, preStepEvent and postStepEvent will be invoked for each step.")]
	[SerializeField]
	private bool useCallbacks = true;

	[Header("Skip")]
	[SerializeField]
	private float minDelayBeforeSkip = 0.3f;

	[SerializeField]
	private List<KeyCode> dialogueSkipKeys = new() { KeyCode.Space, KeyCode.Mouse0 };

	[SerializeField]
	private bool allowSkipAll = true;

	[SerializeField]
	private KeyCode skipAllKey = KeyCode.Escape;

	[Header("Clickable Tween")]
	[SerializeField]
	private bool enableClickableTween = true;

	[SerializeField]
	private bool enableClickableTweenForButtons = true;

	[SerializeField]
	private bool enableClickableTweenForWorld = true;

	[SerializeField]
	private Vector3 targetScale = new(1.2f, 1.2f, 1.2f);

	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private int loops = -1;

	[SerializeField]
	private Ease ease = Ease.InOutSine;
	#endregion

	#region Runtime State
	private Tween clickableButtonTween;
	private Transform clickableTweenTarget; // track target of current tween
	private Vector3 clickableTweenOriginalScale; // track original scale to restore on kill
	private int stepIndex;
	private bool advanceRequested;
	private bool worldClickSatisfied;
	private bool buttonClickSatisfied;
	private bool dialogueFullyShown;
	private SimpleTutorialStep currentStep;
	private readonly List<Button> tempDisabledButtons = new();
	private Coroutine typewriterRoutine;
	private bool tutorialRunning;
	#endregion

	#region Unity Lifecycle
	protected override void Awake()
	{
		base.Awake();
		if (!worldCamera)
			worldCamera = Camera.main;
		TutorialGate.BlockWorld();
	}

	private void Start()
	{
		GameManager.OnResetGame += prestigeLevel =>
		{
			enabled = true;
			stepIndex = 0;
			tutorialRunning = false;
			StartTutorial(prestigeLevel);
		};
		StartTutorial();
	}

	private void Update()
	{
		if (!tutorialRunning)
			return;

		if (allowSkipAll && Input.GetKeyDown(skipAllKey))
		{
			SkipAll();
			return;
		}

		bool advanceKeyDown = dialogueSkipKeys != null && dialogueSkipKeys.Any(Input.GetKeyDown);
#if ENABLE_INPUT_SYSTEM
		if (!advanceKeyDown && Mouse.current != null)
			advanceKeyDown =
				Mouse.current.leftButton.wasPressedThisFrame && dialogueSkipKeys.Contains(KeyCode.Mouse0);
#endif
		if (advanceKeyDown)
			AdvanceOrSkipDialogue();
	}

	private void OnDestroy() => overlay?.OnOverlayClicked.RemoveAllListeners();
	#endregion

	#region Public API
	public void StartTutorial(int prestigeLevel = 0)
	{
		if (prestigeLevel > 0)
		{
			EndTutorial();
			return;
		}
		if (tutorialRunning)
			return;
		tutorialRunning = true;
		StartCoroutine(RunTutorial());
	}

	public void SkipAll()
	{
		if (!tutorialRunning)
			return;
		StopAllCoroutines();
		EndTutorial();
	}

	public void SkipCurrentStep()
	{
		if (!tutorialRunning || currentStep == null)
			return;
		switch (currentStep.type)
		{
			case StepType.Dialogue:
				advanceRequested = true;
				break;
			case StepType.ClickWorld:
				worldClickSatisfied = true;
				break;
			case StepType.ClickButton:
				buttonClickSatisfied = true;
				break;
		}
	}

	public void AdvanceOrSkipDialogue() => OnDialogueAdvanceRequest();

	public void NotifyWorldClick(Transform clicked)
	{
		if (
			currentStep != null
			&& currentStep.type == StepType.ClickWorld
			&& clicked
			&& currentStep.worldTarget == clicked
		)
			worldClickSatisfied = true;
	}

	public void EndTutorial()
	{
		tutorialRunning = false;
		KillCurrentTween();
		StopTypewriterIfNeeded();
		ReenableTempButtons();
		DialogueBox?.SetActive(false);
		overlay?.DisableBlocker();
		arrow?.Hide();
		TutorialGate.AllowWorld();
		OnTutorialEnded?.Invoke();
		enabled = false;
	}
	#endregion

	#region Flow
	private IEnumerator RunTutorial()
	{
		for (stepIndex = 0; stepIndex < steps.Count; stepIndex++)
		{
			currentStep = steps[stepIndex];
			SetupStep(currentStep);
			yield return RunStep(currentStep);
		}
		EndTutorial();
	}

	private IEnumerator RunStep(SimpleTutorialStep step)
	{
		ResetStepState();
		canSkipIndicator?.SetActive(false);

		if (useCallbacks)
			step.preStepEvent?.Invoke();

		ShowDialogue(step);
		RegisterButtonHandler(step);

		if (useCallbacks)
			step.postStepEvent?.Invoke();

		yield return new WaitForSeconds(minDelayBeforeSkip);
		if (step.type == StepType.Dialogue)
			canSkipIndicator?.SetActive(true);
		yield return WaitForStepCompletion(step);
		UnregisterButtonHandler(step);
	}

	private IEnumerator WaitForStepCompletion(SimpleTutorialStep step) =>
		step.type switch
		{
			StepType.Dialogue => new WaitUntil(() => advanceRequested),
			StepType.ClickWorld => new WaitUntil(() => worldClickSatisfied),
			StepType.ClickButton => new WaitUntil(() => buttonClickSatisfied),
			_ => null,
		};
	#endregion

	#region Step Setup
	private void SetupStep(SimpleTutorialStep step)
	{
		KillCurrentTween();
		SetupOverlayAndGate(step);
		ApplyTweenForStep(step);
		ApplyArrowForStep(step);
	}

	private void SetupOverlayAndGate(SimpleTutorialStep step)
	{
		if (overlay)
		{
			if (step.type == StepType.Dialogue)
				overlay.BlockAll();
			else
				overlay.DisableBlocker();
		}
		if (step.type == StepType.ClickWorld)
			TutorialGate.SetWhitelist(new List<Transform> { step.worldTarget });
		else
			TutorialGate.BlockWorld();
	}

	private void ApplyTweenForStep(SimpleTutorialStep step)
	{
		if (!enableClickableTween)
			return;
		if (step.type == StepType.ClickButton && enableClickableTweenForButtons)
			ApplyClickableTweenTo(step.buttonTarget ? step.buttonTarget.transform : null);
		else if (step.type == StepType.ClickWorld && enableClickableTweenForWorld)
			ApplyClickableTweenTo(step.worldTarget);
	}

	private void ApplyArrowForStep(SimpleTutorialStep step)
	{
		if (step.type == StepType.Dialogue || !enableArrow || !arrow)
		{
			arrow?.Hide();
			return;
		}
		arrow.Hide();
		if (!step.showArrow)
			return;
		if (step.overrideArrowTransform)
		{
			arrow.ShowAtAnchored(step.arrowPosition, step.arrowRotation);
			return;
		}
		switch (step.type)
		{
			case StepType.ClickWorld:
				if (step.worldTarget)
					arrow.PointToWorldObject(step.worldTarget, 0f);
				break;
			case StepType.ClickButton:
				if (step.buttonTarget)
					arrow.PointToUIElement(step.buttonTarget.GetComponent<RectTransform>(), 0f);
				break;
		}
	}
	#endregion

	#region Dialogue / Typewriter
	private void ShowDialogue(SimpleTutorialStep step)
	{
		if (!dialogueText)
			return;
		string text = step?.dialogue ?? string.Empty;
		DialogueBox?.SetActive(true);

		if (overrideDialogBoxTransform && step != null && step.OverrideDialogBoxTransform.IsEnabled)
			step.OverrideDialogBoxTransform.ApplyTo(DialogueBox.GetComponent<RectTransform>());
		else
			defaultDialogBoxTransform.ApplyTo(DialogueBox.GetComponent<RectTransform>());

		StopTypewriterIfNeeded();

		if (!ShouldUseTypewriter(step, text))
		{
			dialogueText.text = text;
			dialogueText.ForceMeshUpdate();
			dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
			dialogueFullyShown = true;
			return;
		}

		dialogueFullyShown = false;
		typewriterRoutine = StartCoroutine(TypewriterRoutine(text, GetCharsPerSecond(step)));
	}

	private void StopTypewriterIfNeeded()
	{
		if (typewriterRoutine != null)
		{
			StopCoroutine(typewriterRoutine);
			typewriterRoutine = null;
		}
	}

	private bool ShouldUseTypewriter(SimpleTutorialStep step, string text) =>
		enableTypewriter
		&& !string.IsNullOrEmpty(text)
		&& step != null
		&& step.useTypewriter
		&& GetCharsPerSecond(step) > 0f;

	private float GetCharsPerSecond(SimpleTutorialStep step) =>
		(step != null && step.overrideTypewriterSpeed)
			? Mathf.Max(0f, step.typewriterCharsPerSecond)
			: defaultTypewriterCharsPerSecond;

	private IEnumerator TypewriterRoutine(string text, float charsPerSecond)
	{
		dialogueText.text = text;
		dialogueText.ForceMeshUpdate();
		int total = dialogueText.textInfo.characterCount;
		dialogueText.maxVisibleCharacters = 0;
		float visible = 0f;
		while (dialogueText.maxVisibleCharacters < total)
		{
			visible += charsPerSecond * Time.unscaledDeltaTime;
			int toShow = Mathf.Clamp(Mathf.FloorToInt(visible), 0, total);
			if (toShow != dialogueText.maxVisibleCharacters)
				dialogueText.maxVisibleCharacters = toShow;
			yield return null;
		}
		dialogueText.maxVisibleCharacters = total;
		dialogueFullyShown = true;
		typewriterRoutine = null;
	}

	private void OnDialogueAdvanceRequest()
	{
		if (currentStep == null || currentStep.type != StepType.Dialogue)
			return;
		if (!dialogueFullyShown)
			RevealAllDialogue();
		else
			advanceRequested = true;
	}

	private void RevealAllDialogue()
	{
		if (!dialogueText)
			return;
		StopTypewriterIfNeeded();
		dialogueText.ForceMeshUpdate();
		dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
		dialogueFullyShown = true;
	}
	#endregion

	#region Button Helpers
	private void RegisterButtonHandler(SimpleTutorialStep step)
	{
		if (step.type != StepType.ClickButton || !step.buttonTarget)
			return;
		void Handler()
		{
			buttonClickSatisfied = true;
			SFXManager.Instance.PlayButtonSound();
			ReenableTempButtons();
		}
		step.runtimeHandler = Handler;
		step.buttonTarget.onClick.AddListener(Handler);
		DisableOtherButtons(step.buttonTarget);
	}

	private void UnregisterButtonHandler(SimpleTutorialStep step)
	{
		if (step.type != StepType.ClickButton || !step.buttonTarget || step.runtimeHandler == null)
			return;
		step.buttonTarget.onClick.RemoveListener(step.runtimeHandler);
		ReenableTempButtons();
		step.runtimeHandler = null;
	}
	#endregion

	#region State Helpers
	private void ResetStepState()
	{
		advanceRequested = false;
		worldClickSatisfied = false;
		buttonClickSatisfied = false;
		dialogueFullyShown = false;
	}
	#endregion

	#region UI Button Disable Helpers
	private void DisableOtherButtons(Button except)
	{
		tempDisabledButtons.Clear();
		foreach (var c in canvasesToDisableAllButtonsIn)
		{
			if (!c)
				continue;
			var buttons = c.GetComponentsInChildren<Button>(true);
			foreach (var b in buttons)
			{
				if (b && b != except && b.interactable)
				{
					b.interactable = false;
					tempDisabledButtons.Add(b);
				}
			}
		}
		foreach (var b in individualButtonsToDisable)
		{
			if (b && b != except && b.interactable && !tempDisabledButtons.Contains(b))
			{
				b.interactable = false;
				tempDisabledButtons.Add(b);
			}
		}
	}

	private void ReenableTempButtons()
	{
		if (tempDisabledButtons.Count == 0)
			return;
		foreach (var b in tempDisabledButtons)
			if (b)
				b.interactable = true;
		tempDisabledButtons.Clear();
	}
	#endregion

	#region Tween Helpers
	private void ApplyClickableTweenTo(Transform target)
	{
		if (!target)
			return;
		// Cache original scale so we can restore it when killing the tween
		clickableTweenTarget = target;
		clickableTweenOriginalScale = target.localScale;

		Vector3 newTargetScale = new(
			target.localScale.x * targetScale.x,
			target.localScale.y * targetScale.y,
			target.localScale.z * targetScale.z
		);
		clickableButtonTween = target
			.DOScale(newTargetScale, duration)
			.SetLoops(loops, LoopType.Yoyo)
			.SetEase(ease);
	}

	private void KillCurrentTween()
	{
		if (clickableButtonTween != null && clickableButtonTween.IsActive())
		{
			clickableButtonTween.Kill();
		}
		// Restore original scale if we have a tracked target
		if (clickableTweenTarget)
		{
			clickableTweenTarget.localScale = clickableTweenOriginalScale;
		}
		clickableButtonTween = null;
		clickableTweenTarget = null;
	}
	#endregion

	#region Data Types
	[Serializable]
	public enum StepType
	{
		Dialogue,
		ClickWorld,
		ClickButton,
	}

	[Serializable]
	public class SimpleTutorialStep
	{
		[TextArea]
		public string dialogue;
		public bool useTypewriter = true;
		public bool overrideTypewriterSpeed = false;
		public float typewriterCharsPerSecond = 40f;
		public StepType type = StepType.Dialogue;
		public Transform worldTarget;
		public Button buttonTarget;

		[Header("Arrow")]
		public bool showArrow = true;
		public bool overrideArrowTransform = true;
		public Vector2 arrowPosition = new(0, 0);
		public float arrowRotation = 0f;

		[Header("Callbacks")]
		public UnityEvent preStepEvent;
		public UnityEvent postStepEvent;

		[SerializeField]
		private bool enableOverrideDialogBoxTransform;
		public RectTransformSnapshot OverrideDialogBoxTransform = null;

		[NonSerialized]
		public UnityAction runtimeHandler;

		public static class Prop
		{
			public const string Dialogue = nameof(dialogue);
			public const string UseTypewriter = nameof(useTypewriter);
			public const string OverrideTypewriterSpeed = nameof(overrideTypewriterSpeed);
			public const string TypewriterCharsPerSecond = nameof(typewriterCharsPerSecond);
			public const string Type = nameof(type);
			public const string WorldTarget = nameof(worldTarget);
			public const string ButtonTarget = nameof(buttonTarget);
			public const string ShowArrow = nameof(showArrow);
			public const string OverrideArrowTransform = nameof(overrideArrowTransform);
			public const string ArrowPosition = nameof(arrowPosition);
			public const string ArrowRotation = nameof(arrowRotation);
			public const string PreStepEvent = nameof(preStepEvent);
			public const string PostStepEvent = nameof(postStepEvent);
			public const string EnableOverrideDialogBoxTransform = nameof(
				enableOverrideDialogBoxTransform
			);
			public const string OverrideDialogBoxTransform = nameof(
				SimpleTutorialStep.OverrideDialogBoxTransform
			);
		}
	}

	[System.Serializable]
	public class RectTransformSnapshot
	{
		public Vector2 anchoredPosition;
		public Vector2 sizeDelta;
		public Vector2 pivot;
		public Vector2 anchorMin;
		public Vector2 anchorMax;
		public bool IsEnabled = false;

		public RectTransformSnapshot() { }

		public RectTransformSnapshot(RectTransform rect)
		{
			anchorMin = rect.anchorMin;
			anchorMax = rect.anchorMax;
			anchoredPosition = rect.anchoredPosition;
			sizeDelta = rect.sizeDelta;
			pivot = rect.pivot;
			IsEnabled = true;
		}

		public void ApplyTo(RectTransform rect)
		{
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = sizeDelta;
			rect.pivot = pivot;
		}
	}
	#endregion

	#region Serialized Property Name Constants
	public static class Prop
	{
		public const string EnableArrow = nameof(enableArrow);
		public const string Arrow = nameof(arrow);
		public const string ArrowAngleForUI = nameof(arrowAngleForUI);
		public const string ArrowAngleForWorld = nameof(arrowAngleForWorld);
		public const string ArrowUIOffset = nameof(arrowUIOffset);
		public const string ArrowWorldOffset = nameof(arrowWorldOffset);
		public const string EnableTypewriter = nameof(enableTypewriter);
		public const string DefaultTypewriterCps = nameof(defaultTypewriterCharsPerSecond);
		public const string EnableClickableTween = nameof(enableClickableTween);
		public const string EnableClickableTweenForButtons = nameof(enableClickableTweenForButtons);
		public const string EnableClickableTweenForWorld = nameof(enableClickableTweenForWorld);
		public const string TargetScale = nameof(targetScale);
		public const string Duration = nameof(duration);
		public const string Loops = nameof(loops);
		public const string Ease = nameof(ease);
		public const string Steps = nameof(steps);
		public const string UseCallbacks = nameof(useCallbacks);
		public const string OverrideDialogBoxTransform = nameof(overrideDialogBoxTransform);
		public const string DefaultDialogBoxTransform = nameof(defaultDialogBoxTransform);
	}
	#endregion
}
