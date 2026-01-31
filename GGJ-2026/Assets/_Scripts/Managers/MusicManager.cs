using System.Security.Cryptography;

using Assets._Scripts;
using Assets._Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicManager : SingletonPersistent<MusicManager> {
	private AudioSource _audioSource;
	private Image _buttonImage;

	[SerializeField] private GameObject _musicButton;
	[SerializeField] private Sprite _playingButton;
	[SerializeField] private Sprite _mutedButton;

	override protected void Awake() {
		base.Awake();
		_buttonImage = _musicButton.GetComponent<Image>();
		_musicButton.GetComponent<Button>().onClick.AddListener(Toggle);
		_audioSource = GetComponent<AudioSource>();
		if (!_audioSource.isPlaying) _audioSource.Play();
	}

	public void Toggle() {
		Debug.Log("Toggle");
		if (!_audioSource.mute) {
			SFXManager.Instance.PlayOneShot("button", GameManager.Instance.GlobalSettings.ButtonClickVolume, 1.1f, 1.1f);
			Mute();
		} else {
			SFXManager.Instance.PlayOneShot("button", GameManager.Instance.GlobalSettings.ButtonClickVolume, 0.9f, 0.9f);
			Unmute();
		}
	}

	public void Mute() {
		_audioSource.mute = true;
		_buttonImage.sprite = _mutedButton;
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void Unmute() {
		_audioSource.mute = false;
		_buttonImage.sprite = _playingButton;
		EventSystem.current.SetSelectedGameObject(null);
	}

	public bool IsMusicMuted() {
		return _audioSource.mute;
	}
}
