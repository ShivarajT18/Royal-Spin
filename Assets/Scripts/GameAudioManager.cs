using UnityEngine;
using UnityEngine.UI;

public class GameAudioManager : MonoBehaviour
{
    // Audio sources used to play background music and game sound effects.
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource spinSource;
    public AudioSource stopSource;
    public AudioSource winSource;
    public AudioSource jackpotSource;

    // Audio clips assigned through the Unity Inspector.
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip spinSound;
    public AudioClip reelStopSound;
    public AudioClip winSound;
    public AudioClip jackpotSound;

    // UI images and sprites for the background music toggle.
    [Header("Music Toggle Icon")]
    public Image musicToggleImage;
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;

    // UI images and sprites for the game sound toggle.
    [Header("Game Sound Toggle Icon")]
    public Image gameSoundToggleImage;
    public Sprite gameSoundOnIcon;
    public Sprite gameSoundOffIcon;

    // Track whether music and game sound effects are enabled.
    private bool musicOn = true;
    private bool gameSoundOn = true;

    private void Start()
    {
        // Configure and start background music when the game begins.
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (musicOn)
                musicSource.Play();
        }

        // Display the correct toggle icons.
        UpdateIcons();
    }

    // Enable or disable background music when its UI button is clicked.
    public void ToggleMusic()
    {
        musicOn = !musicOn;

        if (musicSource != null)
        {
            if (musicOn && backgroundMusic != null)
            {
                // Start the background music when enabled.
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
            else
            {
                // Stop the background music when disabled.
                musicSource.Stop();
            }
        }

        UpdateIcons();
    }

    // Enable or disable all game sound effects.
    public void ToggleGameSound()
    {
        gameSoundOn = !gameSoundOn;

        if (!gameSoundOn)
        {
            // Stop any sound effects that are currently playing.
            StopGameSource(spinSource);
            StopGameSource(stopSource);
            StopGameSource(winSource);
            StopGameSource(jackpotSource);
        }

        UpdateIcons();
    }

    // Stop a game sound source without stopping the music source.
    private void StopGameSource(AudioSource source)
    {
        if (source != null && source != musicSource)
            source.Stop();
    }

    // Play the sound effect when the reels begin spinning.
    public void PlaySpin()
    {
        PlayGameSound(spinSource, spinSound);
    }

    // Play the sound effect when the reels stop.
    public void PlayReelStop()
    {
        PlayGameSound(stopSource, reelStopSound);
    }

    // Play the regular winning sound effect.
    public void PlayWin()
    {
        PlayGameSound(winSource, winSound);
    }

    // Play the jackpot sound effect for a jackpot win.
    public void PlayJackpot()
    {
        PlayGameSound(jackpotSource, jackpotSound);
    }

    // Play a sound effect only when game sounds are enabled
    // and the required source and clip are assigned.
    private void PlayGameSound(AudioSource source, AudioClip clip)
    {
        if (!gameSoundOn || source == null || clip == null)
            return;

        // Prevent a game sound from using the music source.
        if (source == musicSource)
            return;

        // Play the sound effect without replacing the source's clip.
        source.PlayOneShot(clip);
    }

    // Update the toggle button icons to reflect the current settings.
    private void UpdateIcons()
    {
        if (musicToggleImage != null)
            musicToggleImage.sprite =
                musicOn ? musicOnIcon : musicOffIcon;

        if (gameSoundToggleImage != null)
            gameSoundToggleImage.sprite =
                gameSoundOn ? gameSoundOnIcon : gameSoundOffIcon;
    }
}