using UnityEngine;

[CreateAssetMenu(fileName = "GlobalSettings", menuName = "Scriptable Objects/GlobalSettings")]
public class GlobalSettings : ScriptableObject
{
	[SerializeField]
	private uint _profilesPerDay;
	public uint ProfilesPerDay => _profilesPerDay;

	[Range(0.0f, 1.0f)]
	[SerializeField]
	private float _alienPercentagePerDayMin;
	public float AlienPercentagePerDayMin => _alienPercentagePerDayMin;

	[Range(0.0f, 1.0f)]
	[SerializeField]
	private float _alienPercentagePerDayMax;
	public float AlienPercentagePerDayMax => _alienPercentagePerDayMax;

	[SerializeField]
	private uint _attributesCountMin;
	public uint AttributesCountMin => _attributesCountMin;

	[SerializeField]
	private uint _attributesCountMax;
	public uint AttributesCountMax => _attributesCountMax;

	// @TODO: private ENUM Curve;
	[SerializeField]
	private uint _daysCount;
	public uint DaysCount => _daysCount;

	[SerializeField]
	private uint _dayDurationSecond;
	public uint DayDurationSecond => _dayDurationSecond;

	[SerializeField]
	private uint _livesCount;
	public uint LivesCount => _livesCount;

	[SerializeField]
	private bool _resetLivesAfterDay = true;
	public bool ResetLivesAfterDay => _resetLivesAfterDay;
}
