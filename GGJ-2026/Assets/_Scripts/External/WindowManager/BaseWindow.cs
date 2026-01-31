using System;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;

public class BaseWindow : MonoBehaviour
{
	[SerializeField]
	private RectTransform _windowContainer;

	protected virtual GameState TargetState => GameState.Playing;
	protected virtual event Action OnWindowShown;
	protected virtual event Action OnWindowStartShowing;
	protected bool _isShown => _canvas.enabled;
	private Canvas _canvas;

	private void Awake()
	{
		_canvas = GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		GameManager.OnGameStateChanged += HandleStateChanged;
	}

	private void HandleStateChanged(GameState newState)
	{
		if (newState == TargetState)
		{
			ShowWindow();
		}
		else
		{
			HideWindow();
		}
	}

	private void ShowWindow()
	{
		OnWindowStartShowing?.Invoke();
		GetComponent<Canvas>().enabled = true;
		_windowContainer.localScale = Vector3.zero;
		_windowContainer
			.DOScale(Vector3.one, 0.25f)
			.SetEase(Ease.OutBack)
			.SetUpdate(true)
			.OnComplete(() => OnWindowShown?.Invoke());
	}

	private void HideWindow()
	{
		_windowContainer
			.DOScale(Vector3.zero, 0.25f)
			.SetEase(Ease.InBack)
			.OnComplete(() => GetComponent<Canvas>().enabled = false)
			.SetUpdate(true);
	}
}

[System.Flags]
public enum GameState
{
	None = 0,
	Playing = 1 << 0,
	Upgrade = 1 << 1,
	Trading = 1 << 2,
	WinningScreen = 1 << 3,
	Leaderboard = 1 << 4,
}
