using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Scriptable Objects/Settings")]
public class Settings : ScriptableObject
{
	[SerializeField]
	private uint _profilesPerDay;
	public uint ProfilesPerDay
	{
		get => _profilesPerDay;
	}

	[Range(0.0f, 1.0f)]
	[SerializeField]
	private float _alienPercentagePerDayMin;
	public float AlienPercentagePerDayMin
	{
		get => _alienPercentagePerDayMin;
	}

	[Range(0.0f, 1.0f)]
	[SerializeField]
	private float _alienPercentagePerDayMax;
	public float AlienPercentagePerDayMax
	{
		get => _alienPercentagePerDayMax;
	}

	[SerializeField]
	private uint _attributesCountMin;
	public uint AttributesCountMin
	{
		get => _attributesCountMin;
	}

	[SerializeField]
	private uint _attributesCountMax;
	public uint AttributesCountMax
	{
		get => _attributesCountMax;
	}

	// @TODO: private ENUM Curve;
	[SerializeField]
	private uint _daysCount;
	public uint DaysCount
	{
		get => _daysCount;
	}

	[SerializeField]
	private uint _dayDurationSecond;
	public uint DayDurationSecond
	{
		get => _dayDurationSecond;
	}

	[SerializeField]
	private uint _livesCount;
	public uint LivesCount
	{
		get => _livesCount;
	}

	[SerializeField]
	private bool _resetLivesAfterDay = true;
	public bool ResetLivesAfterDay
	{
		get => _resetLivesAfterDay;
	}
}
