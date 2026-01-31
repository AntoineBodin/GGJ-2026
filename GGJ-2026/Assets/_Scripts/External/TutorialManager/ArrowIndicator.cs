using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays an arrow that can point to either world objects or UI elements.
/// Supports positioning relative to a target with optional directional offset
/// (top, bottom, left, right) and manual anchored placement.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ArrowIndicator : MonoBehaviour
{
	[Header("Setup")]
	[Tooltip("Canvas where this arrow lives (Screen Space or World Space).")]
	[SerializeField]
	private Canvas canvas;

	[Tooltip("Optional graphic to toggle on/off (if null, we'll toggle the whole GameObject).")]
	[SerializeField]
	private Graphic graphic;

	[Tooltip("Defines which side of the target the arrow should appear.")]
	[SerializeField]
	private ArrowPlacement direction = ArrowPlacement.Bottom;

	private RectTransform arrowRect;

	/// <summary>
	/// Ensure arrowRect is cached (also works in edit mode before Awake is called).
	/// </summary>
	private void EnsureRect()
	{
		if (arrowRect == null)
			arrowRect = GetComponent<RectTransform>();
	}

	/// <summary>
	/// Unity Awake callback. Caches the RectTransform and ensures the arrow starts hidden.
	/// </summary>
	private void Awake()
	{
		EnsureRect();
		if (graphic == null)
			graphic = GetComponent<Graphic>();
		Hide();
	}

	/// <summary>
	/// Points the arrow to a world-space object (single evaluation, no follow) using current <see cref="direction"/>.
	/// </summary>
	/// <param name="target">World transform to point near.</param>
	public void PointToWorldObject(Transform target) => PointToWorldObject(target, null);

	/// <summary>
	/// Points the arrow to a world-space object (single evaluation, no follow) and optionally applies a rotation.
	/// </summary>
	/// <param name="target">World transform to point near.</param>
	/// <param name="angleDeg">Optional Z rotation in degrees (null to keep current).</param>
	public void PointToWorldObject(Transform target, float? angleDeg)
	{
		if (target == null)
			return;

		// Base world position at target origin.
		Vector3 worldPos = target.position;

		// If renderer available, offset world position outward in chosen direction USING world extents.
		if (target.TryGetComponent(out Renderer rend))
		{
			Vector3 ext = rend.bounds.extents;
			switch (direction)
			{
				case ArrowPlacement.Top:
					worldPos.y += ext.y;
					break;
				case ArrowPlacement.Bottom:
					worldPos.y -= ext.y;
					break;
				case ArrowPlacement.Left:
					worldPos.x -= ext.x;
					break;
				case ArrowPlacement.Right:
					worldPos.x += ext.x;
					break;
			}
		}

		// Screen projection.
		Camera cam = GetWorldCamera();
		Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

		// Convert to local (anchored) position relative to canvas root.
		if (ScreenToLocal(screenPos, out Vector2 localPoint))
		{
			// Apply additional arrow size offset so arrow sits outside target edge.
			localPoint += ArrowSizeOffset();
			ApplyAnchoredPosition(localPoint, angleDeg);
		}
	}

	/// <summary>
	/// Points the arrow to a UI RectTransform using its rect center (single evaluation, no follow) using current <see cref="direction"/>.
	/// </summary>
	/// <param name="target">UI RectTransform.</param>
	public void PointToUIElement(RectTransform target) => PointToUIElement(target, null);

	/// <summary>
	/// Points the arrow to a UI RectTransform (single evaluation, no follow) and optionally sets rotation.
	/// </summary>
	/// <param name="target">UI RectTransform.</param>
	/// <param name="angleDeg">Optional Z rotation in degrees (null to keep current).</param>
	public void PointToUIElement(RectTransform target, float? angleDeg)
	{
		if (target == null)
			return;

		// Convert target center to world then to screen.
		Vector3 worldPos = target.TransformPoint(target.rect.center);
		Camera cam = GetUICamera();
		Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

		if (ScreenToLocal(screenPos, out Vector2 localPoint))
		{
			// Offset by half target size in chosen direction so arrow sits on edge.
			switch (direction)
			{
				case ArrowPlacement.Top:
					localPoint.y += target.rect.height / 2f;
					break;
				case ArrowPlacement.Bottom:
					localPoint.y -= target.rect.height / 2f;
					break;
				case ArrowPlacement.Left:
					localPoint.x -= target.rect.width / 2f;
					break;
				case ArrowPlacement.Right:
					localPoint.x += target.rect.width / 2f;
					break;
			}
			localPoint += ArrowSizeOffset();
			ApplyAnchoredPosition(localPoint, angleDeg);
		}
	}

	/// <summary>
	/// Computes extra offset so the arrow appears just outside the target on the chosen side.
	/// Uses the arrow's own dimensions.
	/// </summary>
	private Vector2 ArrowSizeOffset()
	{
		EnsureRect();
		return direction switch
		{
			ArrowPlacement.Top => new(0, arrowRect.rect.height / 2f),
			ArrowPlacement.Bottom => new(0, -arrowRect.rect.height / 2f),
			ArrowPlacement.Left => new(-arrowRect.rect.width / 2f, 0),
			ArrowPlacement.Right => new(arrowRect.rect.width / 2f, 0),
			_ => new(0, 0),
		};
	}

	/// <summary>
	/// Directly place the arrow using a RectTransform anchoredPosition (already in parent local space).
	/// </summary>
	/// <param name="anchoredPosition">Target anchored position relative to the parent.</param>
	/// <param name="angleDeg">Rotation in degrees (Z axis).</param>
	public void ShowAtAnchored(Vector2 anchoredPosition, float angleDeg)
	{
		ApplyAnchoredPosition(anchoredPosition, angleDeg);
	}

	/// <summary>
	/// Sets the Z rotation of the arrow (2D rotation in degrees).
	/// </summary>
	public void SetRotation(float angleDeg)
	{
		EnsureRect();
		if (arrowRect == null)
			return;
		var e = arrowRect.localEulerAngles;
		e.z = angleDeg;
		arrowRect.localEulerAngles = e;
	}

	/// <summary>
	/// Shows the arrow.
	/// </summary>
	public void Show() => SetVisible(true);

	/// <summary>
	/// Hides the arrow (retains last position / rotation).
	/// </summary>
	public void Hide() => SetVisible(false);

	/// <summary>
	/// Programmatically change placement direction (e.g. dynamic layout decisions).
	/// </summary>
	public void SetPlacement(ArrowPlacement newPlacement) => direction = newPlacement;

	/// <summary>
	/// True if the arrow has an assigned graphic and is enabled in hierarchy.
	/// </summary>
	public bool IsVisible => graphic != null ? graphic.gameObject.activeSelf : gameObject.activeSelf;

	// ---- Helpers ----------------------------------------------------------

	/// <summary>
	/// Apply anchored position (and optional rotation) then ensure visibility.
	/// </summary>
	private void ApplyAnchoredPosition(Vector2 anchoredPosition, float? angleDeg)
	{
		EnsureRect();
		if (angleDeg.HasValue)
			SetRotation(angleDeg.Value);
		if (arrowRect == null)
			return;
		arrowRect.anchoredPosition = anchoredPosition;
		SetVisible(true);
	}

	/// <summary>
	/// Converts a screen position to local anchored position in this arrow's parent Canvas.
	/// </summary>
	private bool ScreenToLocal(Vector2 screenPos, out Vector2 localPoint)
	{
		EnsureRect();
		localPoint = default;
		if (canvas == null || arrowRect == null)
			return false;
		var root = canvas.transform as RectTransform;
		Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : GetUICamera();
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(
			root,
			screenPos,
			cam,
			out localPoint
		);
	}

	/// <summary>
	/// Returns the camera used for UI calculations (canvas worldCamera or main).
	/// </summary>
	private Camera GetUICamera() =>
		canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

	/// <summary>
	/// Returns the camera used for world to screen operations (prefers main).
	/// </summary>
	private Camera GetWorldCamera() => Camera.main != null ? Camera.main : GetUICamera();

	/// <summary>
	/// Low-level visibility toggle; hides the graphic or whole GameObject.
	/// </summary>
	private void SetVisible(bool on)
	{
		if (graphic != null)
			graphic.gameObject.SetActive(on);
		else
			gameObject.SetActive(on);
	}

	public enum ArrowPlacement
	{
		Top,
		Bottom,
		Left,
		Right,
	}
}
