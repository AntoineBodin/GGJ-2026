using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Assets._Scripts
{
    public class SFXManager : SingletonPersistent<SFXManager>
    {
        [Header("Folder Settings")]
        [SerializeField] private string _sfxResourcesPath = "SFX";

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 5;
        [SerializeField] private AudioMixerGroup _outputMixer;
        [SerializeField] private Slider sfxSlider;

        private Dictionary<string, AudioClip> _sfxDictionary;
        private List<AudioSource> _audioPool;
        private AudioSource _mainSource;
        private string _lastPlayedKey;
        private bool _isMuted = false;
        private float oldValue;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
            LoadSFXClips();
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            OnSFXSliderChanged(sfxSlider.value);
        }

        private void OnSFXSliderChanged(float volume)
        {
            foreach (var source in _audioPool)
            {
                source.volume = volume;
            }
            _mainSource.volume = volume;
            _isMuted = volume == 0;
        }

        private void InitializeAudioSources()
        {
            _mainSource = CreateAudioSource();

            _audioPool = new List<AudioSource>();
            for (int i = 0; i < _poolSize; i++)
            {
                _audioPool.Add(CreateAudioSource());
            }
        }

        private AudioSource CreateAudioSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.outputAudioMixerGroup = _outputMixer;
            source.playOnAwake = false;
            return source;
        }

        private void LoadSFXClips()
        {
            _sfxDictionary = new Dictionary<string, AudioClip>();
            var clips = Resources.LoadAll<AudioClip>(_sfxResourcesPath);

            foreach (var clip in clips)
            {
                if (_sfxDictionary.ContainsKey(clip.name))
                {
                    Debug.LogWarning($"Duplicate SFX name detected: {clip.name}");
                }
                _sfxDictionary[clip.name] = clip;
            }
        }

        /// <summary>
        /// Play one-shot sound effect (automatically pools sources)
        /// </summary>
        private void PlayOneShot(string sfxKey, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
        {
            if (!_sfxDictionary.TryGetValue(sfxKey, out AudioClip clip))
            {
                Debug.LogWarning($"SFX clip not found: {sfxKey}");
                return;
            }

            var existingSource = FindPlayingSource(clip);
            if (existingSource != null)
            {
                RestartAudioSource(existingSource, volume, minPitch, maxPitch);
                return;
            }

            var availableSource = GetAvailableSource() ?? GetLongestPlayingSource();
            if (availableSource == null) return;

            ConfigureAudioSource(availableSource, clip, volume, minPitch, maxPitch);
            availableSource.Play();
        }

        /// <summary>
        /// Play persistent sound with dedicated channel
        /// </summary>
        public void PlayMain(string sfxKey, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
        {
            if (!_sfxDictionary.TryGetValue(sfxKey, out AudioClip clip))
            {
                Debug.LogWarning($"SFX clip not found: {sfxKey}");
            }

            if (_mainSource.clip == clip && _mainSource.isPlaying) return;

            ConfigureAudioSource(_mainSource, clip, volume, minPitch, maxPitch);
            _mainSource.Play();
        }

        /// <summary>
        /// Stop persistent sound played via PlayMain()
        /// </summary>
        public void StopMain() => _mainSource.Stop();

        /// <summary>
        /// Play random sound from list (avoids immediate repetition)
        /// </summary>
        public void PlayRandom(List<string> keys, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
        {
            if (keys.Count == 0) return;

            var filteredKeys = keys.Where(k => k != _lastPlayedKey).ToList();
            var availableKeys = filteredKeys.Count > 0 ? filteredKeys : keys;

            var randomIndex = Random.Range(0, availableKeys.Count);
            var selectedKey = availableKeys[randomIndex];

            _lastPlayedKey = selectedKey;
            PlayOneShot(selectedKey, volume, minPitch, maxPitch);
        }

        private AudioSource FindPlayingSource(AudioClip clip)
        {
            return _audioPool.FirstOrDefault(s => s.isPlaying && s.clip == clip);
        }

        private void RestartAudioSource(AudioSource source, float volume, float minPitch, float maxPitch)
        {
            source.Stop();
            SetPitchAndVolume(source, volume, minPitch, maxPitch);
            source.Play();
        }

        private void ConfigureAudioSource(AudioSource source, AudioClip clip, float volume, float minPitch, float maxPitch)
        {
            source.Stop();
            source.clip = clip;
            SetPitchAndVolume(source, volume, minPitch, maxPitch);
        }

        private void SetPitchAndVolume(AudioSource source, float volume, float minPitch, float maxPitch)
        {
            source.pitch = Random.Range(minPitch, maxPitch);
            source.volume = volume;
        }

        private AudioSource GetAvailableSource()
        {
            return _audioPool.FirstOrDefault(s => !s.isPlaying);
        }

        private AudioSource GetLongestPlayingSource()
        {
            return _audioPool
                .OrderByDescending(s => s.time)
                .First();
        }

        internal void OnMuteButtonPressed()
        {
            float newValue;
            if (!_isMuted)
            {
                oldValue = sfxSlider.value;
                newValue = 0;
            }
            else
            {
                newValue = oldValue;
            }

            OnSFXSliderChanged(newValue);
            sfxSlider.value = newValue;
        }

        public void PlayButtonSound(float volume = -1)
        {
            if (volume == -1)
            {
                volume = sfxSlider.value;
            }
            PlayOneShot("button", volume, 0.9f, 1);
        }

        public void PlayUpgradeSound(float volume = -1)
        {
            if (volume == -1)
            {
                volume = sfxSlider.value;
            }
            PlayOneShot("upgrade", volume, 0.9f, 1);
        }

        public void PlayTadaSound(float volume = -1)
        {
            if (volume == -1)
            {
                volume = sfxSlider.value;
            }
            PlayOneShot("tada-military", volume);
        }
    }
}
