using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class AdaptiveLetterbox : MonoBehaviour
{
	public enum LetterboxMode
	{
		Vertical,
		Horizontal,
	}

	[Tooltip("Target aspect ratio (width / height)")]
	public Vector2 targetAspectRatio = new(16f, 9f);
	private float TargetAspect => targetAspectRatio.x / targetAspectRatio.y;

	[Tooltip("Choose whether to add bars when the screen is too tall or too wide")]
	public LetterboxMode mode = LetterboxMode.Vertical;

	[Tooltip("Color used for the letterbox bars")]
	public Color barColor = Color.black;

	[Header("Clear full screen to avoid UI trails outside camera rect")]
	[SerializeField]
	private bool useBackgroundClearCamera = true;

	private Camera cam;
	private Camera clearCamera;
	private float lastAspect = -1f;
	private const string ClearCamName = "__LetterboxClearCamera";

	private void OnEnable()
	{
		cam = GetComponent<Camera>();
		EnsureClearCamera();
		ApplySettings();
		SyncClearCamera();
	}

	private void OnDisable()
	{
		// Keep it optional to auto-clean in editor/play mode switches
#if UNITY_EDITOR
		if (!Application.isPlaying && clearCamera != null)
		{
			if (clearCamera.gameObject != null)
				DestroyImmediate(clearCamera.gameObject);
			clearCamera = null;
		}
#endif
	}

	private void Update()
	{
		float currentAspect = (float)Screen.width / Screen.height;

		if (!Mathf.Approximately(currentAspect, lastAspect))
		{
			lastAspect = currentAspect;
			ApplySettings();
		}

		// Keep background colors and clear-camera settings in sync
		if (cam.backgroundColor != barColor)
			cam.backgroundColor = barColor;

		SyncClearCamera();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (cam == null)
			cam = GetComponent<Camera>();
		EnsureClearCamera();
		ApplySettings();
		cam.backgroundColor = barColor;
		SyncClearCamera();

		if (!Application.isPlaying)
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}
#endif

	private void ApplySettings()
	{
		cam.rect = new Rect(0f, 0f, 1f, 1f);

		float windowAspect = (float)Screen.width / Screen.height;
		Rect rect = new(0f, 0f, 1f, 1f);

		if (mode == LetterboxMode.Vertical)
		{
			if (windowAspect < TargetAspect)
			{
				float scaleHeight = windowAspect / TargetAspect;
				rect.height = scaleHeight;
				rect.y = (1f - scaleHeight) / 2f;
			}
		}
		else
		{
			if (windowAspect > TargetAspect)
			{
				float scaleWidth = TargetAspect / windowAspect;
				rect.width = scaleWidth;
				rect.x = (1f - scaleWidth) / 2f;
			}
		}

		cam.rect = rect;
		cam.backgroundColor = barColor;
	}

	private void EnsureClearCamera()
	{
		if (!useBackgroundClearCamera)
		{
			if (clearCamera != null)
				clearCamera.enabled = false;
			return;
		}

		if (clearCamera == null)
		{
			// Try find existing child
			Transform t = transform.Find(ClearCamName);
			if (t != null)
				clearCamera = t.GetComponent<Camera>();

			if (clearCamera == null)
			{
				var go = new GameObject(ClearCamName);
				go.transform.SetParent(transform, false);
				clearCamera = go.AddComponent<Camera>();
#if UNITY_EDITOR
				go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
#else
				go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
#endif
			}
		}
		clearCamera.enabled = true;
		SyncClearCamera();
	}

	private void SyncClearCamera()
	{
		if (clearCamera == null)
		{
			if (useBackgroundClearCamera)
				EnsureClearCamera();
			return;
		}

		clearCamera.clearFlags = CameraClearFlags.SolidColor;
		clearCamera.backgroundColor = barColor;
		clearCamera.cullingMask = 0; // render nothing, only clear
		clearCamera.rect = new Rect(0f, 0f, 1f, 1f); // always full screen
		clearCamera.depth = cam.depth - 1f; // render before main camera
		clearCamera.orthographic = true;
		clearCamera.orthographicSize = 5f;
		clearCamera.allowHDR = false;
		clearCamera.allowMSAA = false;
	}
}
