using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening; // for Ease
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TutorialManager;
using Prop = TutorialManager.Prop; // alias for property name constants
using StepProp = TutorialManager.SimpleTutorialStep.Prop; // alias for step property name constants

[CustomEditor(typeof(TutorialManager))]
public class TutorialManagerEditor : Editor
{
	// Layout constants
	private const float PadTop = 2f;
	private const float PadBottom = 4f;
	private const float PadLeft = 14f; // leave space for the ReorderableList drag handle

	// StepType enum indices (keep in sync with TutorialManager.SimpleTutorialStep.StepType)
	private const int StepType_Dialogue = 0;
	private const int StepType_ClickWorld = 1;
	private const int StepType_ClickButton = 2;

	// Serialized properties (manager level)
	private SerializedProperty sp_enableArrow;
	private SerializedProperty sp_arrow;
	private SerializedProperty sp_arrowAngleForUI;
	private SerializedProperty sp_arrowAngleForWorld;
	private SerializedProperty sp_arrowUIOffset;
	private SerializedProperty sp_arrowWorldOffset;
	private SerializedProperty sp_enableTypewriter;
	private SerializedProperty sp_defaultTypewriterCps;
	private SerializedProperty sp_enableClickableTween;
	private SerializedProperty sp_enableClickableTweenForButtons;
	private SerializedProperty sp_enableClickableTweenForWorld;
	private SerializedProperty sp_targetScale;
	private SerializedProperty sp_duration;
	private SerializedProperty sp_loops;
	private SerializedProperty sp_ease;
	private SerializedProperty sp_steps;
	private SerializedProperty sp_useCallbacks;
	private SerializedProperty sp_overrideDialogBoxPosition;
	private SerializedProperty sp_defaultDialogBoxTransform;

	private ReorderableList stepsList;
	private bool stepsFoldout = true;

	// Cached GUIContent
	private static readonly GUIContent GC_EnableTypewriter = new("Enable Typewriter");
	private static readonly GUIContent GC_DefaultCps = new("Default Characters Per Second");
	private static readonly GUIContent GC_EnableArrow = new("Enable Arrow");
	private static readonly GUIContent GC_EnableTween = new("Enable Clickable Tween");
	private static readonly GUIContent GC_TweenForButtons = new("For UI Buttons");
	private static readonly GUIContent GC_TweenForWorld = new("For World Targets");
	private static readonly GUIContent GC_Dialogue = new("Dialogue");
	private static readonly GUIContent GC_OverrideSpeed = new("Override Speed");
	private static readonly GUIContent GC_ShowArrow = new("Show Arrow");
	private static readonly GUIContent GC_Callbacks = new("Callbacks");
	private static readonly GUIContent GC_UseCallbacks = new("Use Callbacks");
	private static readonly GUIContent GC_OverrideDialogBoxTransform = new(
		"Override Dialog Box Transform"
	);
	private static readonly GUIContent GC_OverrideArrow = new("Override Arrow Transform");
	private static readonly GUIContent GC_ArrowPosition = new("Arrow Position");
	private static readonly GUIContent GC_ArrowRotation = new("Arrow Rotation");
	private static readonly GUIContent GC_Visualize = new("Visualize");
	private static readonly GUIContent GC_ManagerExport = new("Export Manager JSON");
	private static readonly GUIContent GC_ManagerImport = new("Import Manager JSON");
	private static readonly GUIContent GC_ApplyDialogBoxTransform = new(
		"Apply Default Dialog Box Transform"
	);
	private static readonly GUIContent GC_VisualizeDialogBoxTransform = new(
		"Visualize Default Dialog Box Transform"
	);

	private void OnEnable()
	{
		sp_enableArrow = serializedObject.FindProperty(Prop.EnableArrow);
		sp_arrow = serializedObject.FindProperty(Prop.Arrow);
		sp_arrowAngleForUI = serializedObject.FindProperty(Prop.ArrowAngleForUI);
		sp_arrowAngleForWorld = serializedObject.FindProperty(Prop.ArrowAngleForWorld);
		sp_arrowUIOffset = serializedObject.FindProperty(Prop.ArrowUIOffset);
		sp_arrowWorldOffset = serializedObject.FindProperty(Prop.ArrowWorldOffset);
		sp_enableTypewriter = serializedObject.FindProperty(Prop.EnableTypewriter);
		sp_defaultTypewriterCps = serializedObject.FindProperty(Prop.DefaultTypewriterCps);
		sp_enableClickableTween = serializedObject.FindProperty(Prop.EnableClickableTween);
		sp_enableClickableTweenForButtons = serializedObject.FindProperty(
			Prop.EnableClickableTweenForButtons
		);
		sp_enableClickableTweenForWorld = serializedObject.FindProperty(
			Prop.EnableClickableTweenForWorld
		);
		sp_targetScale = serializedObject.FindProperty(Prop.TargetScale);
		sp_duration = serializedObject.FindProperty(Prop.Duration);
		sp_loops = serializedObject.FindProperty(Prop.Loops);
		sp_ease = serializedObject.FindProperty(Prop.Ease);
		sp_steps = serializedObject.FindProperty(Prop.Steps);
		sp_useCallbacks = serializedObject.FindProperty(Prop.UseCallbacks);
		sp_overrideDialogBoxPosition = serializedObject.FindProperty(Prop.OverrideDialogBoxTransform);
		sp_defaultDialogBoxTransform = serializedObject.FindProperty(Prop.DefaultDialogBoxTransform);

		SetupStepsList();
	}

	#region Steps List Setup
	private void SetupStepsList()
	{
		stepsList = new ReorderableList(serializedObject, sp_steps, true, true, true, true)
		{
			drawHeaderCallback = DrawStepsHeader,
			elementHeightCallback = GetStepElementHeight,
			drawElementCallback = DrawStepElement,
		};
	}

	private void DrawStepsHeader(Rect rect)
	{
		const float btnGap = 6f;
		const float smallBtnW = 86f;
		const float exportW = 110f;
		const float importW = 100f;

		var right = rect.x + rect.width;
		var exportRect = new Rect(right - exportW, rect.y + 1f, exportW, rect.height - 2f);
		var importRect = new Rect(
			exportRect.x - btnGap - importW,
			rect.y + 1f,
			importW,
			rect.height - 2f
		);
		var collapseRect = new Rect(
			importRect.x - btnGap - smallBtnW,
			rect.y + 1f,
			smallBtnW,
			rect.height - 2f
		);
		var expandRect = new Rect(
			collapseRect.x - btnGap - smallBtnW,
			rect.y + 1f,
			smallBtnW,
			rect.height - 2f
		);
		var labelRect = new Rect(rect.x, rect.y, expandRect.x - rect.x - btnGap, rect.height);

		EditorGUI.BeginChangeCheck();
		bool newFoldout = EditorGUI.Foldout(labelRect, stepsFoldout, "Steps", true);
		if (EditorGUI.EndChangeCheck())
		{
			stepsFoldout = newFoldout;
			GUI.changed = true;
			Repaint();
			return;
		}

		if (GUI.Button(expandRect, "Expand All"))
			SetAllStepsExpanded(true);
		if (GUI.Button(collapseRect, "Collapse All"))
			SetAllStepsExpanded(false);
		if (GUI.Button(importRect, "Import JSON"))
			ImportStepsFromJson();
		if (GUI.Button(exportRect, "Export to JSON"))
			ExportStepsToJson();
	}

	private float GetStepElementHeight(int index)
	{
		if (!stepsFoldout)
			return 0f;
		float line = EditorGUIUtility.singleLineHeight;
		float vsp = EditorGUIUtility.standardVerticalSpacing;

		var element = sp_steps.GetArrayElementAtIndex(index);
		float height = PadTop + line + vsp; // foldout row
		if (!element.isExpanded)
			return height + PadBottom;

		// Dialogue
		var dialogueProp = element.FindPropertyRelative(StepProp.Dialogue);
		height += EditorGUI.GetPropertyHeight(dialogueProp, true) + vsp;

		// Typewriter
		if (sp_enableTypewriter.boolValue)
		{
			height += line + vsp; // Use Typewriter toggle
			var useTW = element.FindPropertyRelative(StepProp.UseTypewriter);
			if (useTW.boolValue)
			{
				height += line + vsp; // override row
			}
		}

		// Step type enum
		var typeProp = element.FindPropertyRelative(StepProp.Type);
		height += EditorGUI.GetPropertyHeight(typeProp) + vsp;
		int typeIndex = typeProp.enumValueIndex;

		// World or Button target field
		if (typeIndex == StepType_ClickWorld)
		{
			var worldTargetProp = element.FindPropertyRelative(StepProp.WorldTarget);
			height += EditorGUI.GetPropertyHeight(worldTargetProp) + vsp;
		}
		else if (typeIndex == StepType_ClickButton)
		{
			var uiButtonProp = element.FindPropertyRelative(StepProp.ButtonTarget);
			height += EditorGUI.GetPropertyHeight(uiButtonProp) + vsp;
		}

		// Arrow related controls
		if (typeIndex != StepType_Dialogue && sp_enableArrow.boolValue)
		{
			// showArrow toggle
			height += line + vsp;
			var showArrowProp = element.FindPropertyRelative(StepProp.ShowArrow);
			if (showArrowProp.boolValue)
			{
				// overrideArrowTransform toggle + visualize button
				height += line + vsp;
				var overrideProp = element.FindPropertyRelative(StepProp.OverrideArrowTransform);
				if (overrideProp.boolValue)
				{
					// arrowPosition (Vector2) + arrowRotation (float) + capture button
					height += line + vsp; // position
					height += line + vsp; // rotation
					height += line + vsp; // capture button
				}
			}
		}

		// Callbacks
		if (sp_useCallbacks.boolValue)
		{
			var preEvt = element.FindPropertyRelative(StepProp.PreStepEvent);
			var postEvt = element.FindPropertyRelative(StepProp.PostStepEvent);
			height += line + vsp; // Callbacks label
			height += EditorGUI.GetPropertyHeight(preEvt, true) + vsp;
			height += EditorGUI.GetPropertyHeight(postEvt, true) + vsp;
		}

		//DialogueBox Override
		if (sp_overrideDialogBoxPosition.boolValue)
		{
			height += line + vsp; // Override Dialogue Box Position toggle
			var dialogueBoxOverrideProp = element.FindPropertyRelative(
				StepProp.EnableOverrideDialogBoxTransform
			);
			if (dialogueBoxOverrideProp.boolValue)
			{
				height += line + vsp; // Override Dialogue Box Transform toggle
			}
		}

		height += PadBottom;
		return height;
	}

	private void DrawStepElement(Rect rect, int index, bool isActive, bool isFocused)
	{
		if (!stepsFoldout)
			return;
		var element = sp_steps.GetArrayElementAtIndex(index);
		float line = EditorGUIUtility.singleLineHeight;
		float vsp = EditorGUIUtility.standardVerticalSpacing;

		var content = new Rect(
			rect.x + PadLeft,
			rect.y + PadTop,
			rect.width - PadLeft,
			rect.height - (PadTop + PadBottom)
		);
		var typeProp = element.FindPropertyRelative("type");
		string typeLabel = typeProp.enumDisplayNames[typeProp.enumValueIndex];

		// Foldout
		var foldoutRect = new Rect(content.x, content.y, content.width, line);
		EditorGUI.BeginChangeCheck();
		bool newExpanded = EditorGUI.Foldout(
			foldoutRect,
			element.isExpanded,
			$"Step {index + 1} - {typeLabel}",
			true
		);
		if (EditorGUI.EndChangeCheck())
		{
			element.isExpanded = newExpanded;
			GUI.changed = true;
			Repaint();
			return;
		}
		if (!element.isExpanded)
			return;

		float y = content.y + line + vsp;

		// Dialogue
		var dialogueProp = element.FindPropertyRelative("dialogue");
		float dlgH = EditorGUI.GetPropertyHeight(dialogueProp, true);
		EditorGUI.PropertyField(
			new Rect(content.x, y, content.width, dlgH),
			dialogueProp,
			GC_Dialogue,
			true
		);
		y += dlgH + vsp;

		// Typewriter per-step
		if (sp_enableTypewriter.boolValue)
		{
			var useTWProp = element.FindPropertyRelative("useTypewriter");
			var useRect = new Rect(content.x, y, content.width, line);
			bool newUse = EditorGUI.ToggleLeft(useRect, "Use Typewriter", useTWProp.boolValue);
			if (newUse != useTWProp.boolValue)
				useTWProp.boolValue = newUse;
			y += line + vsp;
			if (useTWProp.boolValue)
			{
				var overrideProp = element.FindPropertyRelative("overrideTypewriterSpeed");
				var cpsProp = element.FindPropertyRelative("typewriterCharsPerSecond");
				float boolWidth = 200f;
				var row = new Rect(content.x, y, content.width, line);
				var boolRect = new Rect(row.x, row.y, boolWidth, row.height);
				var cpsRect = new Rect(
					row.x + boolWidth + 6f,
					row.y,
					row.width - (boolWidth + 6f),
					row.height
				);
				EditorGUI.PropertyField(boolRect, overrideProp, GC_OverrideSpeed);
				if (overrideProp.boolValue)
				{
					EditorGUI.PropertyField(cpsRect, cpsProp, GUIContent.none);
				}
				y += line + vsp;
			}
		}

		// Step type
		var typeRect = new Rect(content.x, y, content.width, line);
		EditorGUI.PropertyField(typeRect, typeProp);
		y += line + vsp;
		int typeIndex = typeProp.enumValueIndex;

		// World / Button specific
		if (typeIndex == StepType_ClickWorld)
		{
			var worldTargetProp = element.FindPropertyRelative("worldTarget");
			float wH = EditorGUI.GetPropertyHeight(worldTargetProp, true);
			EditorGUI.PropertyField(new Rect(content.x, y, content.width, wH), worldTargetProp);
			y += wH + vsp;
		}
		else if (typeIndex == StepType_ClickButton)
		{
			var uiButtonProp = element.FindPropertyRelative("buttonTarget");
			float bH = EditorGUI.GetPropertyHeight(uiButtonProp, true);
			EditorGUI.PropertyField(new Rect(content.x, y, content.width, bH), uiButtonProp);
			y += bH + vsp;
		}

		// Arrow controls (only for non-dialogue when arrow globally enabled)
		if (typeIndex != StepType_Dialogue && sp_enableArrow.boolValue)
		{
			var showArrowProp = element.FindPropertyRelative("showArrow");
			var showRect = new Rect(content.x, y, content.width, line);
			bool newShow = EditorGUI.ToggleLeft(showRect, GC_ShowArrow, showArrowProp.boolValue);
			if (newShow != showArrowProp.boolValue)
				showArrowProp.boolValue = newShow;
			y += line + vsp;

			if (showArrowProp.boolValue)
			{
				var overrideProp = element.FindPropertyRelative("overrideArrowTransform");
				// Line with override toggle and visualize button
				const float visualizeW = 90f;
				var overrideRect = new Rect(content.x, y, content.width - visualizeW - 4f, line);
				var visualizeRect = new Rect(overrideRect.xMax + 4f, y, visualizeW, line);
				bool newOverride = EditorGUI.ToggleLeft(
					overrideRect,
					GC_OverrideArrow,
					overrideProp.boolValue
				);
				if (newOverride != overrideProp.boolValue)
					overrideProp.boolValue = newOverride;

				if (GUI.Button(visualizeRect, GC_Visualize))
				{
					VisualizeArrowPlacement(element, typeIndex);
				}
				y += line + vsp;

				if (overrideProp.boolValue)
				{
					var posProp = element.FindPropertyRelative("arrowPosition");
					var rotProp = element.FindPropertyRelative("arrowRotation");

					// Arrow Position (Vector2)
					var posRect = new Rect(content.x, y, content.width, line);
					EditorGUI.PropertyField(posRect, posProp, GC_ArrowPosition);
					y += line + vsp;

					// Arrow Rotation (float)
					var rotRect = new Rect(content.x, y, content.width, line);
					EditorGUI.PropertyField(rotRect, rotProp, GC_ArrowRotation);
					y += line + vsp;

					// Capture button
					var btnRect = new Rect(content.x, y, 180f, line);
					if (GUI.Button(btnRect, "Capture Arrow Transform"))
					{
						var tm = (TutorialManager)target;
						if (tm.arrow != null)
						{
							RectTransform rt = tm.arrow.GetComponent<RectTransform>();
							if (rt != null)
							{
								Undo.RecordObject(tm, "Capture Arrow Transform");
								posProp.vector2Value = rt.anchoredPosition;
								rotProp.floatValue = rt.localEulerAngles.z;
								serializedObject.ApplyModifiedProperties();
								EditorUtility.SetDirty(tm);
							}
						}
						else
						{
							Debug.LogWarning(
								"[TutorialManagerEditor] No ArrowIndicator assigned on TutorialManager."
							);
						}
					}
					y += line + vsp;
				}
			}
		}

		// Callbacks
		if (sp_useCallbacks.boolValue)
		{
			EditorGUI.LabelField(
				new Rect(content.x, y, content.width, line),
				GC_Callbacks,
				EditorStyles.boldLabel
			);
			y += line + vsp;
			var preEvt = element.FindPropertyRelative("preStepEvent");
			var postEvt = element.FindPropertyRelative("postStepEvent");
			float preH = EditorGUI.GetPropertyHeight(preEvt, true);
			EditorGUI.PropertyField(new Rect(content.x, y, content.width, preH), preEvt, true);
			y += preH + vsp;
			float postH = EditorGUI.GetPropertyHeight(postEvt, true);
			EditorGUI.PropertyField(new Rect(content.x, y, content.width, postH), postEvt, true);
			y += postH + vsp;
		}

		// DialogueBox Override
		if (sp_overrideDialogBoxPosition.boolValue)
		{
			var overrideDialogueBoxProp = element.FindPropertyRelative(
				StepProp.EnableOverrideDialogBoxTransform
			);
			var overrideDialogueBoxRect = new Rect(content.x, y, content.width, line);
			bool newOverrideDialogueBox = EditorGUI.ToggleLeft(
				overrideDialogueBoxRect,
				GC_OverrideDialogBoxTransform,
				overrideDialogueBoxProp.boolValue
			);
			if (newOverrideDialogueBox != overrideDialogueBoxProp.boolValue)
				overrideDialogueBoxProp.boolValue = newOverrideDialogueBox;
			y += line + vsp;

			var snapshotProp = element.FindPropertyRelative(StepProp.OverrideDialogBoxTransform);

			if (overrideDialogueBoxProp.boolValue)
			{
				Rect captureRect = new Rect(content.x, y, content.width / 2 - 10, line);
				Rect applyRect = new Rect(
					content.x + captureRect.width + 20,
					y,
					content.width / 2 - 10,
					line
				);

				if (GUI.Button(captureRect, "Capture Box"))
				{
					var manager = (TutorialManager)target;
					if (manager != null)
					{
						var step = manager.GetStepAtIndex(index);
						step.OverrideDialogBoxTransform = new RectTransformSnapshot(
							manager.DialogueBox.transform as RectTransform
						);
						EditorUtility.SetDirty(manager);
					}
					else
					{
						Debug.LogWarning("TutorialManager has no dialogueBox assigned!");
					}
				}
				if (GUI.Button(applyRect, "Visualize"))
				{
					var manager = (TutorialManager)target;
					if (manager.DialogueBox != null)
					{
						var step = manager.GetStepAtIndex(index);
						if (step.OverrideDialogBoxTransform.IsEnabled)
							step.OverrideDialogBoxTransform.ApplyTo(
								manager.DialogueBox.transform as RectTransform
							);
						else
							manager.defaultDialogBoxTransform?.ApplyTo(
								manager.DialogueBox.transform as RectTransform
							);
					}
				}
				y += line + vsp;
			}
		}
	}
	#endregion

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Manager level export/import (full)
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(GC_ManagerExport))
			ExportManagerToJson();
		if (GUILayout.Button(GC_ManagerImport))
			ImportManagerFromJson();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space(6);

		DrawPropertiesExcluding(
			serializedObject,
			// Typewriter
			Prop.EnableTypewriter,
			Prop.DefaultTypewriterCps,
			// Arrow
			Prop.EnableArrow,
			Prop.Arrow,
			Prop.ArrowAngleForUI,
			Prop.ArrowAngleForWorld,
			Prop.ArrowUIOffset,
			Prop.ArrowWorldOffset,
			// Tween
			Prop.EnableClickableTween,
			Prop.EnableClickableTweenForButtons,
			Prop.EnableClickableTweenForWorld,
			Prop.TargetScale,
			Prop.Duration,
			Prop.Loops,
			Prop.Ease,
			// Steps / callbacks (custom draw below)
			Prop.Steps,
			Prop.UseCallbacks
		);

		DrawTypewriterBlock();
		DrawArrowBlock();
		DrawTweenBlock();

		EditorGUILayout.Space(6);
		sp_useCallbacks.boolValue = EditorGUILayout.ToggleLeft(
			GC_UseCallbacks,
			sp_useCallbacks.boolValue
		);

		EditorGUILayout.Space(6);
		sp_overrideDialogBoxPosition.boolValue = EditorGUILayout.ToggleLeft(
			GC_OverrideDialogBoxTransform,
			sp_overrideDialogBoxPosition.boolValue
		);

		if (sp_overrideDialogBoxPosition.boolValue)
		{
			var tm = (TutorialManager)target;
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(GC_ApplyDialogBoxTransform))
				tm.defaultDialogBoxTransform = new RectTransformSnapshot(
					tm.DialogueBox.transform as RectTransform
				);
			if (GUILayout.Button(GC_VisualizeDialogBoxTransform))
				tm.defaultDialogBoxTransform?.ApplyTo(tm.DialogueBox.transform as RectTransform);

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space(8);
		stepsList.draggable = stepsFoldout;
		stepsList.displayAdd = stepsFoldout;
		stepsList.displayRemove = stepsFoldout;
		stepsList.footerHeight = stepsFoldout ? 13f : 0f;
		stepsList.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
	}

	#region Group Drawers
	private void DrawTypewriterBlock()
	{
		EditorGUILayout.LabelField("Dialogue Typewriter", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.PropertyField(sp_enableTypewriter, GC_EnableTypewriter);
		if (sp_enableTypewriter.boolValue)
			EditorGUILayout.PropertyField(sp_defaultTypewriterCps, GC_DefaultCps);
		EditorGUILayout.EndVertical();
	}

	private void DrawArrowBlock()
	{
		EditorGUILayout.Space(4);
		EditorGUILayout.LabelField("Arrow", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.PropertyField(sp_enableArrow, GC_EnableArrow);
		if (sp_enableArrow.boolValue)
		{
			EditorGUILayout.PropertyField(sp_arrow);
			EditorGUILayout.PropertyField(sp_arrowAngleForUI);
			EditorGUILayout.PropertyField(sp_arrowAngleForWorld);
			EditorGUILayout.PropertyField(sp_arrowUIOffset);
			EditorGUILayout.PropertyField(sp_arrowWorldOffset);
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawTweenBlock()
	{
		EditorGUILayout.Space(4);
		EditorGUILayout.LabelField("Clickable Tween", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.PropertyField(sp_enableClickableTween, GC_EnableTween);
		if (sp_enableClickableTween.boolValue)
		{
			EditorGUILayout.PropertyField(sp_enableClickableTweenForButtons, GC_TweenForButtons);
			EditorGUILayout.PropertyField(sp_enableClickableTweenForWorld, GC_TweenForWorld);
			EditorGUILayout.PropertyField(sp_targetScale);
			EditorGUILayout.PropertyField(sp_duration);
			EditorGUILayout.PropertyField(sp_loops);
			EditorGUILayout.PropertyField(sp_ease);
		}
		EditorGUILayout.EndVertical();
	}
	#endregion

	#region Utility
	private void SetAllStepsExpanded(bool expanded)
	{
		for (int i = 0; i < sp_steps.arraySize; i++)
			sp_steps.GetArrayElementAtIndex(i).isExpanded = expanded;
		Repaint();
	}

	private float GetFloat(string name)
	{
		var p = serializedObject.FindProperty(name);
		return p != null ? p.floatValue : 0f;
	}
	#endregion

	#region Import / Export
	private void ExportStepsToJson()
	{
		serializedObject.ApplyModifiedProperties();
		var tm = (TutorialManager)target;
		var export = new TutorialExport
		{
			tutorialName = tm.name,
			defaultTypewriterCPS = GetFloat(Prop.DefaultTypewriterCps),
			steps = new List<StepExport>(),
		};

		for (int i = 0; i < sp_steps.arraySize; i++)
		{
			var element = sp_steps.GetArrayElementAtIndex(i);
			var dialogue = element.FindPropertyRelative("dialogue")?.stringValue ?? string.Empty;
			var useTW = element.FindPropertyRelative("useTypewriter")?.boolValue ?? true;
			var overrideTw = element.FindPropertyRelative("overrideTypewriterSpeed")?.boolValue ?? false;
			var cps = element.FindPropertyRelative("typewriterCharsPerSecond")?.floatValue ?? 0f;
			var typeProp = element.FindPropertyRelative("type");
			int typeIndex = typeProp != null ? typeProp.enumValueIndex : 0;
			string type = typeProp != null ? typeProp.enumDisplayNames[typeIndex] : "Dialogue";
			var showArrowProp = element.FindPropertyRelative("showArrow");
			bool showArrow = showArrowProp != null && showArrowProp.boolValue;
			bool overrideArrow =
				element.FindPropertyRelative("overrideArrowTransform")?.boolValue ?? false;
			Vector2 arrowPos =
				element.FindPropertyRelative("arrowPosition")?.vector2Value ?? Vector2.zero;
			float arrowRot = element.FindPropertyRelative("arrowRotation")?.floatValue ?? 0f;
			string worldTargetPath = null;
			string uiButtonPath = null;

			if (typeIndex == StepType_ClickWorld)
			{
				var tr = element.FindPropertyRelative("worldTarget")?.objectReferenceValue as Transform;
				worldTargetPath = tr ? GetHierarchyPath(tr) : null;
			}
			else if (typeIndex == StepType_ClickButton)
			{
				var btn = element.FindPropertyRelative("buttonTarget")?.objectReferenceValue as Button;
				uiButtonPath = btn ? GetHierarchyPath(btn.transform) : null;
			}

			export.steps.Add(
				new StepExport
				{
					index = i,
					type = type,
					dialogue = dialogue,
					useTypewriter = useTW,
					overrideTypewriterSpeed = overrideTw,
					typewriterCharsPerSecond = cps,
					showArrow = showArrow,
					overrideArrowTransform = overrideArrow,
					arrowPosition = arrowPos,
					arrowRotation = arrowRot,
					worldTargetPath = worldTargetPath,
					uiButtonPath = uiButtonPath,
				}
			);
		}

		string json = JsonUtility.ToJson(export, true);
		WriteJsonWithUniqueName(
			json,
			((TutorialManager)target).gameObject.name + " TutorialSteps.json"
		);
	}

	private void ImportStepsFromJson()
	{
		try
		{
			string startDir = Path.Combine(Application.dataPath, "Resources");
			if (!Directory.Exists(startDir))
				startDir = Application.dataPath;
			string path = EditorUtility.OpenFilePanel("Import Tutorial JSON", startDir, "json");
			if (string.IsNullOrEmpty(path))
				return;
			string json = File.ReadAllText(path);
			var data = JsonUtility.FromJson<TutorialExport>(json);
			if (data == null)
			{
				EditorUtility.DisplayDialog("Import Tutorial", "Invalid JSON format.", "OK");
				return;
			}
			serializedObject.Update();
			if (sp_defaultTypewriterCps != null && data.defaultTypewriterCPS > 0f)
				sp_defaultTypewriterCps.floatValue = data.defaultTypewriterCPS;
			sp_steps.arraySize = data.steps != null ? data.steps.Count : 0;
			for (int i = 0; i < sp_steps.arraySize; i++)
			{
				var src = data.steps[i];
				var dst = sp_steps.GetArrayElementAtIndex(i);
				dst.FindPropertyRelative("dialogue").stringValue = src.dialogue ?? string.Empty;
				var typeProp = dst.FindPropertyRelative("type");
				typeProp.enumValueIndex = ToStepTypeIndex(src.type);
				var useTWProp = dst.FindPropertyRelative("useTypewriter");
				if (useTWProp != null)
					useTWProp.boolValue = src.useTypewriter;
				dst.FindPropertyRelative("overrideTypewriterSpeed").boolValue = src.overrideTypewriterSpeed;
				dst.FindPropertyRelative("typewriterCharsPerSecond").floatValue =
					src.typewriterCharsPerSecond;
				dst.FindPropertyRelative("showArrow").boolValue = src.showArrow;
				dst.FindPropertyRelative("overrideArrowTransform").boolValue = src.overrideArrowTransform;
				dst.FindPropertyRelative("arrowPosition").vector2Value = src.arrowPosition;
				dst.FindPropertyRelative("arrowRotation").floatValue = src.arrowRotation;
				var worldProp = dst.FindPropertyRelative("worldTarget");
				var btnProp = dst.FindPropertyRelative("buttonTarget");
				worldProp.objectReferenceValue = null;
				btnProp.objectReferenceValue = null;
				if (
					typeProp.enumValueIndex == StepType_ClickWorld
					&& !string.IsNullOrEmpty(src.worldTargetPath)
				)
				{
					var tr = ResolveTransformByPath(src.worldTargetPath);
					if (tr == null)
						Debug.LogWarning($"[Tutorial Import] World target not found: '{src.worldTargetPath}'");
					worldProp.objectReferenceValue = tr;
				}
				else if (
					typeProp.enumValueIndex == StepType_ClickButton
					&& !string.IsNullOrEmpty(src.uiButtonPath)
				)
				{
					var tr = ResolveTransformByPath(src.uiButtonPath);
					Button btn = tr ? tr.GetComponent<Button>() : null;
					if (btn == null)
						Debug.LogWarning(
							$"[Tutorial Import] UI Button not found or missing Button component at: '{src.uiButtonPath}'"
						);
					btnProp.objectReferenceValue = btn;
				}
				dst.isExpanded = false;
			}
			serializedObject.ApplyModifiedProperties();
			Undo.RecordObject(target, "Import Tutorial Steps");
			EditorUtility.SetDirty(target);
			stepsList.index =
				(sp_steps.arraySize > 0) ? Mathf.Clamp(stepsList.index, 0, sp_steps.arraySize - 1) : -1;
			EditorUtility.DisplayDialog("Tutorial Import", "Import completed.", "OK");
			Repaint();
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Tutorial Import] Failed: {ex.Message}\n{ex.StackTrace}");
			EditorUtility.DisplayDialog(
				"Tutorial Import",
				"An error occurred. Check the Console for details.",
				"OK"
			);
		}
	}

	// ---- Full Manager Export / Import ------------------------------------
	private void ExportManagerToJson()
	{
		serializedObject.ApplyModifiedProperties();
		var full = BuildManagerExport();
		string json = JsonUtility.ToJson(full, true);
		WriteJsonWithUniqueName(json, full.tutorialName + " TutorialManager.json");
	}

	private void ImportManagerFromJson()
	{
		try
		{
			string startDir = Path.Combine(Application.dataPath, "Resources");
			if (!Directory.Exists(startDir))
				startDir = Application.dataPath;
			string path = EditorUtility.OpenFilePanel("Import Tutorial Manager JSON", startDir, "json");
			if (string.IsNullOrEmpty(path))
				return;
			string json = File.ReadAllText(path);
			var data = JsonUtility.FromJson<TutorialManagerFullExport>(json);
			if (data == null)
			{
				EditorUtility.DisplayDialog("Import Tutorial Manager", "Invalid JSON format.", "OK");
				return;
			}
			serializedObject.Update();
			// Global properties
			SetBool(Prop.EnableTypewriter, data.enableTypewriter);
			SetFloat(Prop.DefaultTypewriterCps, data.defaultTypewriterCPS);
			SetBool(Prop.EnableArrow, data.enableArrow);
			SetFloat(Prop.ArrowAngleForUI, data.arrowAngleForUI);
			SetFloat(Prop.ArrowAngleForWorld, data.arrowAngleForWorld);
			SetVector2(Prop.ArrowUIOffset, data.arrowUIOffset);
			SetVector2(Prop.ArrowWorldOffset, data.arrowWorldOffset);
			SetBool(Prop.UseCallbacks, data.useCallbacks);
			SetFloat("minDelayBeforeSkip", data.minDelayBeforeSkip);
			SetBool("allowSkipAll", data.allowSkipAll);
			SetEnum("skipAllKey", data.skipAllKey);
			// dialogueSkipKeys list
			var skipKeysProp = serializedObject.FindProperty("dialogueSkipKeys");
			if (skipKeysProp != null && data.dialogueSkipKeys != null)
			{
				skipKeysProp.arraySize = data.dialogueSkipKeys.Count;
				for (int i = 0; i < skipKeysProp.arraySize; i++)
					skipKeysProp.GetArrayElementAtIndex(i).enumValueIndex = (int)data.dialogueSkipKeys[i];
			}
			// Tween
			SetBool(Prop.EnableClickableTween, data.enableClickableTween);
			SetBool(Prop.EnableClickableTweenForButtons, data.enableClickableTweenForButtons);
			SetBool(Prop.EnableClickableTweenForWorld, data.enableClickableTweenForWorld);
			SetVector3(Prop.TargetScale, data.targetScale);
			SetFloat(Prop.Duration, data.duration);
			SetInt(Prop.Loops, data.loops);
			SetEase(Prop.Ease, data.ease);

			// Steps
			sp_steps.arraySize = data.steps != null ? data.steps.Count : 0;
			for (int i = 0; i < sp_steps.arraySize; i++)
			{
				var src = data.steps[i];
				var dst = sp_steps.GetArrayElementAtIndex(i);
				dst.FindPropertyRelative("dialogue").stringValue = src.dialogue ?? string.Empty;
				var typeProp = dst.FindPropertyRelative("type");
				typeProp.enumValueIndex = ToStepTypeIndex(src.type);
				dst.FindPropertyRelative("useTypewriter").boolValue = src.useTypewriter;
				dst.FindPropertyRelative("overrideTypewriterSpeed").boolValue = src.overrideTypewriterSpeed;
				dst.FindPropertyRelative("typewriterCharsPerSecond").floatValue =
					src.typewriterCharsPerSecond;
				dst.FindPropertyRelative("showArrow").boolValue = src.showArrow;
				dst.FindPropertyRelative("overrideArrowTransform").boolValue = src.overrideArrowTransform;
				dst.FindPropertyRelative("arrowPosition").vector2Value = src.arrowPosition;
				dst.FindPropertyRelative("arrowRotation").floatValue = src.arrowRotation;
				var worldProp = dst.FindPropertyRelative("worldTarget");
				var btnProp = dst.FindPropertyRelative("buttonTarget");
				worldProp.objectReferenceValue = null;
				btnProp.objectReferenceValue = null;
				if (
					typeProp.enumValueIndex == StepType_ClickWorld
					&& !string.IsNullOrEmpty(src.worldTargetPath)
				)
				{
					var tr = ResolveTransformByPath(src.worldTargetPath);
					if (tr == null)
						Debug.LogWarning($"[Tutorial Import] World target not found: '{src.worldTargetPath}'");
					worldProp.objectReferenceValue = tr;
				}
				else if (
					typeProp.enumValueIndex == StepType_ClickButton
					&& !string.IsNullOrEmpty(src.uiButtonPath)
				)
				{
					var tr = ResolveTransformByPath(src.uiButtonPath);
					Button btn = tr ? tr.GetComponent<Button>() : null;
					if (btn == null)
						Debug.LogWarning(
							$"[Tutorial Import] UI Button not found or missing Button component at: '{src.uiButtonPath}'"
						);
					btnProp.objectReferenceValue = btn;
				}
			}
			serializedObject.ApplyModifiedProperties();
			Undo.RecordObject(target, "Import Tutorial Manager");
			EditorUtility.SetDirty(target);
			EditorUtility.DisplayDialog("Tutorial Manager Import", "Import completed.", "OK");
			Repaint();
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Tutorial Manager Import] Failed: {ex.Message}\n{ex.StackTrace}");
			EditorUtility.DisplayDialog(
				"Tutorial Manager Import",
				"An error occurred. Check the Console for details.",
				"OK"
			);
		}
	}

	private TutorialManagerFullExport BuildManagerExport()
	{
		var tm = (TutorialManager)target;
		var full = new TutorialManagerFullExport
		{
			tutorialName = tm.name,
			enableTypewriter = serializedObject.FindProperty(Prop.EnableTypewriter).boolValue,
			defaultTypewriterCPS = serializedObject.FindProperty(Prop.DefaultTypewriterCps).floatValue,
			enableArrow = serializedObject.FindProperty(Prop.EnableArrow).boolValue,
			arrowAngleForUI = serializedObject.FindProperty(Prop.ArrowAngleForUI).floatValue,
			arrowAngleForWorld = serializedObject.FindProperty(Prop.ArrowAngleForWorld).floatValue,
			arrowUIOffset = serializedObject.FindProperty(Prop.ArrowUIOffset).vector2Value,
			arrowWorldOffset = serializedObject.FindProperty(Prop.ArrowWorldOffset).vector2Value,
			useCallbacks = serializedObject.FindProperty(Prop.UseCallbacks).boolValue,
			minDelayBeforeSkip = serializedObject.FindProperty("minDelayBeforeSkip")?.floatValue ?? 0f,
			dialogueSkipKeys = ReadKeyCodeList("dialogueSkipKeys"),
			allowSkipAll = serializedObject.FindProperty("allowSkipAll")?.boolValue ?? false,
			skipAllKey = (KeyCode)(serializedObject.FindProperty("skipAllKey")?.enumValueIndex ?? 0),
			enableClickableTween = serializedObject.FindProperty(Prop.EnableClickableTween).boolValue,
			enableClickableTweenForButtons = serializedObject
				.FindProperty(Prop.EnableClickableTweenForButtons)
				.boolValue,
			enableClickableTweenForWorld = serializedObject
				.FindProperty(Prop.EnableClickableTweenForWorld)
				.boolValue,
			targetScale = serializedObject.FindProperty(Prop.TargetScale).vector3Value,
			duration = serializedObject.FindProperty(Prop.Duration).floatValue,
			loops = serializedObject.FindProperty(Prop.Loops).intValue,
			ease = (Ease)serializedObject.FindProperty(Prop.Ease).enumValueIndex,
			steps = new List<StepExport>(),
		};

		for (int i = 0; i < sp_steps.arraySize; i++)
		{
			var element = sp_steps.GetArrayElementAtIndex(i);
			var typeProp = element.FindPropertyRelative("type");
			int typeIndex = typeProp.enumValueIndex;
			string type = typeProp.enumDisplayNames[typeIndex];
			string worldTargetPath = null;
			string uiButtonPath = null;
			if (typeIndex == StepType_ClickWorld)
			{
				var tr = element.FindPropertyRelative("worldTarget")?.objectReferenceValue as Transform;
				worldTargetPath = tr ? GetHierarchyPath(tr) : null;
			}
			else if (typeIndex == StepType_ClickButton)
			{
				var btn = element.FindPropertyRelative("buttonTarget")?.objectReferenceValue as Button;
				uiButtonPath = btn ? GetHierarchyPath(btn.transform) : null;
			}
			full.steps.Add(
				new StepExport
				{
					index = i,
					type = type,
					dialogue = element.FindPropertyRelative("dialogue").stringValue,
					useTypewriter = element.FindPropertyRelative("useTypewriter").boolValue,
					overrideTypewriterSpeed = element
						.FindPropertyRelative("overrideTypewriterSpeed")
						.boolValue,
					typewriterCharsPerSecond = element
						.FindPropertyRelative("typewriterCharsPerSecond")
						.floatValue,
					showArrow = element.FindPropertyRelative("showArrow").boolValue,
					overrideArrowTransform = element.FindPropertyRelative("overrideArrowTransform").boolValue,
					arrowPosition = element.FindPropertyRelative("arrowPosition").vector2Value,
					arrowRotation = element.FindPropertyRelative("arrowRotation").floatValue,
					worldTargetPath = worldTargetPath,
					uiButtonPath = uiButtonPath,
				}
			);
		}
		return full;
	}

	private List<KeyCode> ReadKeyCodeList(string propertyName)
	{
		var listProp = serializedObject.FindProperty(propertyName);
		var result = new List<KeyCode>();
		if (listProp == null)
			return result;
		for (int i = 0; i < listProp.arraySize; i++)
		{
			result.Add((KeyCode)listProp.GetArrayElementAtIndex(i).enumValueIndex);
		}
		return result;
	}

	private void WriteJsonWithUniqueName(string json, string baseFilename)
	{
		string resourcesDir = Path.Combine(Application.dataPath, "Resources");
		if (!Directory.Exists(resourcesDir))
			Directory.CreateDirectory(resourcesDir);
		string candidate = baseFilename;
		string fullPath = Path.Combine(resourcesDir, candidate);
		int iSuffix = 2;
		while (File.Exists(fullPath))
		{
			string nameNoExt = Path.GetFileNameWithoutExtension(baseFilename);
			string ext = Path.GetExtension(baseFilename);
			candidate = $"{nameNoExt} ({iSuffix}){ext}";
			fullPath = Path.Combine(resourcesDir, candidate);
			iSuffix++;
		}
		File.WriteAllText(fullPath, json);
#if UNITY_EDITOR
		string relativePath = "Assets/Resources/" + candidate;
		AssetDatabase.ImportAsset(relativePath);
		AssetDatabase.Refresh();
		var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
		if (asset != null)
			EditorGUIUtility.PingObject(asset);
		EditorUtility.DisplayDialog("Tutorial Export", $"Exported to:\n{relativePath}", "OK");
#endif
	}

	// Helper setters for import
	private void SetBool(string name, bool value)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.boolValue = value;
	}

	private void SetFloat(string name, float value)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.floatValue = value;
	}

	private void SetInt(string name, int value)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.intValue = value;
	}

	private void SetVector2(string name, Vector2 v)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.vector2Value = v;
	}

	private void SetVector3(string name, Vector3 v)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.vector3Value = v;
	}

	private void SetEnum(string name, Enum e)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.enumValueIndex = Convert.ToInt32(e);
	}

	private void SetEase(string name, Ease e)
	{
		var p = serializedObject.FindProperty(name);
		if (p != null)
			p.enumValueIndex = (int)e;
	}

	#endregion // Import / Export

	#region Helpers (static)
	private static int ToStepTypeIndex(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
			return StepType_Dialogue;
		var key = typeName.Replace(" ", string.Empty).Trim();
		if (string.Equals(key, "Dialogue", StringComparison.OrdinalIgnoreCase))
			return StepType_Dialogue;
		if (string.Equals(key, "ClickWorld", StringComparison.OrdinalIgnoreCase))
			return StepType_ClickWorld;
		if (string.Equals(key, "ClickButton", StringComparison.OrdinalIgnoreCase))
			return StepType_ClickButton;
		return StepType_Dialogue;
	}

	private static string GetHierarchyPath(Transform t)
	{
		if (t == null)
			return null;
		var stack = new Stack<string>();
		var cur = t;
		while (cur != null)
		{
			stack.Push(cur.name);
			cur = cur.parent;
		}
		return string.Join("/", stack.ToArray());
	}

	private static Transform ResolveTransformByPath(string path)
	{
		if (string.IsNullOrEmpty(path))
			return null;
		var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
			return null;
		var roots = SceneManager.GetActiveScene().GetRootGameObjects();
		GameObject rootGo = null;
		for (int i = 0; i < roots.Length; i++)
		{
			if (roots[i].name == parts[0])
			{
				rootGo = roots[i];
				break;
			}
		}
		if (rootGo == null)
			return null;
		Transform current = rootGo.transform;
		for (int i = 1; i < parts.Length; i++)
		{
			var next = current.Find(parts[i]);
			if (next == null)
				return null;
			current = next;
		}
		return current;
	}
	#endregion

	private void VisualizeArrowPlacement(SerializedProperty stepElement, int stepTypeIndex)
	{
		var tm = (TutorialManager)target;
		if (tm == null || tm.arrow == null)
		{
			Debug.LogWarning("[TutorialManagerEditor] Cannot visualize (no TutorialManager.arrow)");
			return;
		}

		// Hide first to emulate runtime behaviour
		tm.arrow.Hide();

		var showArrowProp = stepElement.FindPropertyRelative(StepProp.ShowArrow);
		if (!showArrowProp.boolValue)
			return; // nothing to show

		var overrideProp = stepElement.FindPropertyRelative(StepProp.OverrideArrowTransform);
		if (overrideProp.boolValue)
		{
			var posProp = stepElement.FindPropertyRelative(StepProp.ArrowPosition);
			var rotProp = stepElement.FindPropertyRelative(StepProp.ArrowRotation);
			tm.arrow.ShowAtAnchored(posProp.vector2Value, rotProp.floatValue);
			return;
		}

		// Non override path replicating runtime logic
		if (stepTypeIndex == StepType_ClickWorld)
		{
			var worldProp = stepElement.FindPropertyRelative(StepProp.WorldTarget);
			var tr = worldProp.objectReferenceValue as Transform;
			if (tr)
				tm.arrow.PointToWorldObject(tr, 0f);
		}
		else if (stepTypeIndex == StepType_ClickButton)
		{
			var btnProp = stepElement.FindPropertyRelative(StepProp.ButtonTarget);
			var btn = btnProp.objectReferenceValue as Button;
			if (btn && btn.TryGetComponent(out RectTransform rt))
				tm.arrow.PointToUIElement(rt, 0f);
		}
	}

	[Serializable]
	private class TutorialExport
	{
		public string tutorialName;
		public float defaultTypewriterCPS;
		public List<StepExport> steps;
	}

	[Serializable]
	private class StepExport
	{
		public int index;
		public string type;
		public string dialogue;
		public bool useTypewriter;
		public bool overrideTypewriterSpeed;
		public float typewriterCharsPerSecond;
		public bool showArrow;
		public bool overrideArrowTransform;
		public Vector2 arrowPosition;
		public float arrowRotation;
		public string worldTargetPath;
		public string uiButtonPath;
	}

	[Serializable]
	private class TutorialManagerFullExport
	{
		public string tutorialName;

		// Globals
		public bool enableTypewriter;
		public float defaultTypewriterCPS;
		public bool enableArrow;
		public float arrowAngleForUI;
		public float arrowAngleForWorld;
		public Vector2 arrowUIOffset;
		public Vector2 arrowWorldOffset;
		public bool useCallbacks;
		public float minDelayBeforeSkip;
		public List<KeyCode> dialogueSkipKeys;
		public bool allowSkipAll;
		public KeyCode skipAllKey;

		// Tween
		public bool enableClickableTween;
		public bool enableClickableTweenForButtons;
		public bool enableClickableTweenForWorld;
		public Vector3 targetScale;
		public float duration;
		public int loops;
		public Ease ease;

		// Steps
		public List<StepExport> steps;
	}
}

