using System;
using System.Collections.Generic;
using Bubble.Character;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<Sprite> numbers = new();
    [SerializeField] private List<Image> digits = new();
    [SerializeField] private List<Sprite> shieldBar = new();

    [SerializeField] private Image shieldBarImage;
    [SerializeField] private Slider dashIconSlider;
    [SerializeField] private Image dashIconBG, dashIconFill;
    [SerializeField] private Sprite dashIconReady, dashIconNotReady;

    private CharacterManager manager;

    private float ticksPassed = 0f;
    private int seconds = 0;
    void Start()
    {
        manager = FindFirstObjectByType<CharacterManager>();
        manager.CharAPI.Get<Action>("onDeath");
        manager.CharAPI.Get<Action<int>>("attributeShields").Subscribe(handler =>
        {
            SetShieldBarNumber(handler);
        });
        manager.CharAPI.Get<Action<float>>("attributeDashCooldownProgress").Subscribe(handler =>
        {
            dashIconSlider.value = handler;
            dashIconFill.gameObject.SetActive(handler < 1f);
            dashIconBG.sprite = handler < 1f ? dashIconNotReady : dashIconReady;
        });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ticksPassed += Time.fixedDeltaTime;
        if (ticksPassed >= 1f)
        {
            ticksPassed = 0f;
            seconds++;
            SetTimerNumber(seconds);
        }
    }

    public void SetShieldBarNumber(int number)
    {
        int n = Math.Clamp(number, 0, 2);
        shieldBarImage.sprite = shieldBar[n];
    }

    public void SetTimerNumber(int seconds)
    {
        // Ensure the seconds value is within a valid range (0 to 9999)
        seconds = Mathf.Clamp(seconds, 0, 9999);

        // Break down the seconds into individual digits
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        // Extract each digit
        int minuteTens = minutes / 10;
        int minuteOnes = minutes % 10;
        int secondTens = remainingSeconds / 10;
        int secondOnes = remainingSeconds % 10;

        // Assign the corresponding sprites to the digits
        digits[0].sprite = numbers[minuteTens];
        digits[1].sprite = numbers[minuteOnes];
        digits[2].sprite = numbers[secondTens];
        digits[3].sprite = numbers[secondOnes];
    }
}
