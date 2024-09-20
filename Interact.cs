using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Interact : MonoBehaviour
{
    public Survival _survival;
    public AudioClip eatingClip;
    public AudioClip drinkingClip;

    private bool eatDebounce = false;
    private bool drinkDebounce = false;
    private static readonly int rayLength = 20;

    private static readonly string eatHelper = "PRESS E TO EAT";
    private bool eatHelperDisplayed = false;
    private static readonly string drinkHelper = "PRESS Q TO DRINK";
    private bool drinkHelperDisplayed = false;

    private static readonly float normalPoisonProbability = 25f;
    private static readonly float holyAreaPoisonProbability = 100f;
    private int eatInHolyAreaCount = 0;

    public static readonly string eatInHolyArea = "INTERESTING. IS IT GREED, OR PERHAPS GLUTTONY?\nI TAKE PITY ON YOU, AND YOU STILL WANT MORE. SUFFER.";
    public static readonly IList<String> shortEatInHolyArea =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "CAREFUL, HUMAN. YOU ARE TESTING YOUR LUCK.",
                "EACH ACTION CARRIES CONSEQUENCES.",
                "WE GAVE PEOPLE BRAINS. IT SEEMS, HOWEVER,\nYOU HAVE FORGOTTEN HOW TO USE YOURS."
            });

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        LayerMask rayLayer = LayerMask.GetMask("Custom Raycast");

        if (Physics.Raycast(transform.position, forward, out RaycastHit hit, rayLength, rayLayer))
        {
            if (!eatHelperDisplayed)
            {
                StartCoroutine(_survival.UpdateHelper(eatHelper));
                eatHelperDisplayed = true;
            }

            if (!eatDebounce && Keyboard.current.eKey.wasPressedThisFrame)
            {
                eatDebounce = true;
                StartCoroutine(_survival.PlayPlayerSound(eatingClip));
                if (Random.Range(0f, 100f)
                    <= (_survival.isInHolyArea
                        ? holyAreaPoisonProbability
                        : normalPoisonProbability))
                    _survival.isPoisoned = true;
                else _survival.AddEnergy(Survival.berryEnergy);

                if (_survival.isInHolyArea)
                {
                    if (++eatInHolyAreaCount == 1)
                        StartCoroutine(_survival.UpdateText(eatInHolyArea));
                    else StartCoroutine(
                        _survival.UpdateText(
                            shortEatInHolyArea[
                                Random.Range(0, shortEatInHolyArea.Count)
                                ]));
                }
            }

            if (eatDebounce && Keyboard.current.eKey.wasReleasedThisFrame)
                eatDebounce = false;
        }

        else if (Physics.Raycast(transform.position, forward, out hit, rayLength)
            && hit.collider.gameObject.tag.Equals("Water"))
        {
            if (!drinkHelperDisplayed)
            {
                StartCoroutine(_survival.UpdateHelper(drinkHelper));
                drinkHelperDisplayed = true;
            }

            if (!drinkDebounce && Keyboard.current.qKey.wasPressedThisFrame)
            {
                drinkDebounce = true;
                StartCoroutine(_survival.PlayPlayerSound(drinkingClip));
                _survival.AddEnergy(Survival.waterEnergy);

                if (_survival.isPoisoned)
                {
                    _survival.isPoisoned = false;
                    _survival.wasPoisonSoundPlayed = false;
                    _survival.ResetValue(ref _survival.poisonTimer, nameof(_survival.poisonTimer));
                    _survival.ResetValue(ref _survival.poisonSoundTimer, nameof(_survival.poisonSoundTimer));

                    StartCoroutine(_survival.ClearPoisonTip());
                }
            }

            if (drinkDebounce && Keyboard.current.qKey.wasReleasedThisFrame)
                drinkDebounce = false;
        }

        else if (eatHelperDisplayed || drinkHelperDisplayed)
        {
            StartCoroutine(_survival.UpdateHelper(""));
            eatHelperDisplayed = false;
            drinkHelperDisplayed = false;
        }
    }
}