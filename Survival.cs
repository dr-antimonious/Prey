#define PRODUCTION

using System;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using System.Collections.ObjectModel;
using UnityEngine.SceneManagement;

public class Survival : MonoBehaviour
{
    public Light moon;
    public AudioSource mainAudioSource;
    public AudioSource playerSource;
    public AudioSource lowHealthSource;
    public AudioClip normalAudio;
    public AudioClip eventOneClip;
    public AudioClip eventTwoClip;
    public AudioClip eventThreeClip;
    public AudioClip bossClip;
    public AudioClip poisonClip;
    public AudioClip smallEventClip;
    public MeshCollider eventOneBarrier;
    public MeshCollider eventTwoBarrier;
    public MeshCollider eventThreeBarrier;
    public Slider healthSlider;
    public Image healthFill;
    public Slider energySlider;
    public Image energyFill;
    public TMP_Text text;
    public TMP_Text helper;
    public TMP_Text tips;

    private enum BARRIERS
    {
        ONE,
        TWO,
        THREE,
    }

    private int isNewPlayer = 1;

    private float health = 100f;
    public static readonly float recoverHealth = 2f;
    public static readonly float lowEnergyHealth = 1f;
    public static readonly float lowEnergyRunHealth = 2f;
    public static readonly float poisonHealth = 3f;
    public static readonly float largeEventHealth = 5f;

    public void AddHealth(float amount)
    {
        health = (health + amount) > defaultVals[nameof(health)]
                 ? defaultVals[nameof(health)]
                 : health + amount;
    }

    public void LowerHealth(float amount) { health -= amount; }

    private float energy = 100f;
    public static readonly float waterEnergy = 5f;
    public static readonly float berryEnergy = 10f;
    public static readonly float idleEnergy = 1f;
    public static readonly float poisonEnergy = 4f;
    public static readonly float walkEnergy = 1f;
    public static readonly float runEnergy = 3f;
    public static readonly float healthRecoverEnergy = 4f;
    public static readonly float smallEventEnergy = 5f;
    public static readonly float largeEventEnergy = 10f;
    
    public void AddEnergy(float amount)
    {
        energy = (energy + amount) > defaultVals[nameof(energy)]
                 ? defaultVals[nameof(energy)]
                 : energy + amount;
    }

    public void LowerEnergy(float amount)
    {
        energy = (energy - amount) < 0f
                 ? 0f
                 : energy - amount;
    }

    public bool isLowEnergy = false;
    public bool isPoisoned = false;
    public int wasPoisonedFirstTime = 0;
    public bool wasPoisonSoundPlayed = false;
    private bool isDrinkSoundQueued = false;
    private bool isEatSoundQueued = false;
    private bool isPoisonSoundQueued = false;

    private bool isSmallEventOngoing = false;
    private bool isLargeEventOngoing = false;
    public enum EVENTS
    {
        SMALL,
        LARGE
    }

    public bool isInHolyArea = false;
    private bool isHolyAreaActive = true;
    private int holyAreaUseCount = 0;
    private void SetEvent(Enum eventEnum)
    {
        switch (eventEnum)
        {
            case EVENTS.SMALL:
                isSmallEventOngoing = true;
                break;
            case EVENTS.LARGE:
                isLargeEventOngoing = true;
                break;
        }
    }
    private void ResetEvent(Enum eventEnum)
    {
        switch (eventEnum)
        {
            case EVENTS.SMALL:
                isSmallEventOngoing = false;
                break;
            case EVENTS.LARGE:
                isLargeEventOngoing = false;
                break;
        }
    }

    private bool isGreetingShown = false;
    private bool isEventOneTriggered = false;
    private bool isEventTwoTriggered = false;
    private bool isEventThreeTriggered = false;
    private bool isBossEventTriggered = false;

    private static float smallEventTimer;
    private void SetSmallEventTimer() { smallEventTimer = Random.Range(14f, 37f); }

    private float idleTimer = 2f;
    private float walkTimer = 1f;
    private float runTimer = 1f;
    public float poisonTimer = 1f;
    public float poisonSoundTimer = 10f;
    private float lowEnergyTimer = 1f;
    private float lowEnergyRunTimer = 1f;
    private float healthRecoverTimer = 1f;

    private float smallEventDuration = 0.05f;
    private bool didSmallEventDurationCross1s = false;

    private StarterAssetsInputs _input;

    private static readonly Dictionary<string, float> defaultVals = new()
    {
        { nameof(idleTimer), 2f },
        { nameof(walkTimer), 1f },
        { nameof(runTimer), 1f },
        { nameof(poisonTimer), 1f },
        { "_survival.poisonTimer", 1f },
        { nameof(poisonSoundTimer), 10f },
        { nameof(lowEnergyTimer), 1f },
        { nameof(lowEnergyRunTimer), 1f },
        { nameof(healthRecoverTimer), 1f },
        { nameof(health), 100f },
        { nameof(energy), 100f }
    };

    public void ResetValue(ref float variable, string varName)
    {
        variable = defaultVals[varName];
    }

    private const string poisonSoundName = "ahogarse-47092";
    private const string eatSoundName = "eatingsfxwav-14588";
    private const string drinkSoundName = "drink-sip-and-swallow-6974";

    private static readonly string firstPoisonTip = "YOU ARE POISONED. DRINK WATER";

    private static readonly string newPlayerGreeting =
        "HOW NICE, A FRESH SACRIFICE.\nJUST WHEN I GOT BORED OF THE OLD ONE.";
    private static readonly IList<String> returningPlayerGreeting =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "DEATH IS INEVITABLE. AND IRRESISTIBLE TO YOU, IT SEEMS.",
                "HAVE YOU COME BACK TO SUFFER ONCE MORE?",
                "DEATH. DEATH. DEATH. DEATH. DEATH. DEATH. DEATH. DEATH.\nDEATH. DEATH. DEATH. DEATH. DEATH. DEATH. DEATH. DEATH.",
                "OH, IT'S YOU AGAIN? BRING A FRIEND NEXT TIME,\nIT'S MUCH MORE FUN WHEN THERE'S TWO OF YOU.",
                "NEVER GONNA GIVE YOU UP, NEVER GONNA LET YOU DOWN,\nNEVER GONNA RUN AROUND AND DESERT YOU."
            });

    private static readonly IList<String> newPlayerSmallEventLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "HE IS DEATH. HE IS DEATH. HE IS DEATH. HE IS DEATH. HE IS DEATH.",
                "ESCAPE. ESCAPE. ESCAPE. ESCAPE. ESCAPE. ESCAPE. ESCAPE. ESCAPE.",
                "RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN. RUN.",
                "HELP US. HELP US. HELP US. HELP US. HELP US. HELP US. HELP US.",
                "THESE VIOLENT DELIGHTS HAVE VIOLENT ENDS."
            });
    private static readonly IList<String> returningPlayerSmallEventLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "STAY AWAY. STAY AWAY. STAY AWAY. STAY AWAY. STAY AWAY. STAY AWAY.",
                "WHY DID YOU COME BACK? WHY DID YOU COME BACK? WHY DID YOU COME BACK?",
                "FREE YOURSELF. FREE YOURSELF. FREE YOURSELF. FREE YOURSELF.",
                "IT'S TOO LATE."
            });
    private static List<string> smallEventLines = new List<string>();

    private static readonly IList<String> eventOneLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "BEST HUNTER OF THE TRIBE, IS IT?\nHOW ARE YOU FINDING YOUR NEW ROLE, THAT OF PREY?",
                "YOU'VE LEARNED TO TRACK, STALK, AND KILL.\nHOW WELL DID YOU LEARN TO SURVIVE?",
                "EVERY SHADOW, EVERY RUSTLE...\n...WE HUNGER FOR YOU."
            });
    private static readonly IList<String> eventTwoLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "EVERY STEP YOU TAKE LEADS\nYOU DEEPER INTO MY WORLD.",
                "IT'S THE SAME FOREST WHERE YOU ONCE OFFERED ME BLOOD,\nYET IT'S YOURS NOW THAT STAINS IT.",
                "YOUR HEART LEARNS TO RACE DIFFERENTLY.\nLEGS, ONCE SWIFT, NOW TREMBLE."
            });
    private static readonly IList<String> eventThreeLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "YOU WILL RUN.\nI WILL CHASE.",
                "YOU WILL REST.\nI WILL NOT.",
                "SOON YOU WILL SIT TOO STILL\nOR SLEEP TOO DEEP.",
                "THEN YOU WILL NOTICE A SHADOW NEXT TO YOU.\nYOUR LIFE WILL THEN BE OVER.",
                "EVERY TIME YOU FALL.\nI CHASE YOU AGAIN.",
                "YOUR FATE CARVED WITH OBSIDIAN.\nTHE SAME YOU CARVED PREY WITH.",
                "YOU HEARD SOME OF THOSE CRIES?\nI HEAR THEM ALL, BEYOND MICTLAN.",
                "ALL HUNTERS END WITH ONE LAST CHASE.\nI HUNT THEM ALL.",
                "ALREADY I HAVE TAKEN THE MOON,\nTHE STARS, AND THE DAYLIGHT.",
                "NOW, I WILL HUNT ETERNITY."
            });
    private static readonly IList<String> bossEventLines =
        new ReadOnlyCollection<string>
            (new List<String>
            {
                "I'LL TEACH YOU HOW LONG A SECOND OF ETERNITY IS.",
                "THERE IS A MOUNTAIN SO VAST\nTHAT IT TOUCHES CLOUDS.",
                "EVERY 52-YEAR CYCLE\nA LITTLE BIRD COMES,",
                "AND SHARPENS ITS BEAK\nON THE OBSIDIAN MOUNTAIN.",
                "ONLY WHEN THE ENTIRE MOUNTAIN\nHAS BEEN CHISELED AWAY,",
                "THE FIRST SECOND OF ETERNITY WILL HAVE PASSED."
            });


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

#if PRODUCTION
        if (!PlayerPrefs.HasKey(nameof(isNewPlayer)))
            PlayerPrefs.SetInt(nameof(isNewPlayer), 1);
        isNewPlayer = PlayerPrefs.GetInt(nameof(isNewPlayer));

        if (!PlayerPrefs.HasKey(nameof(wasPoisonedFirstTime)))
            PlayerPrefs.SetInt(nameof(wasPoisonedFirstTime), 0);
        wasPoisonedFirstTime = PlayerPrefs.GetInt(nameof(wasPoisonedFirstTime));
#endif        

        smallEventLines.AddRange(newPlayerSmallEventLines);
        if (isNewPlayer == 0) smallEventLines.AddRange(returningPlayerSmallEventLines); 

        _input = GetComponent<StarterAssetsInputs>();
        SetSmallEventTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            PlayerPrefs.SetInt(nameof(isNewPlayer), 0);
            SceneManager.LoadScene(2);
        }

        if (!isGreetingShown)
        {
            isGreetingShown = true;
            StartCoroutine(UpdateText(
                isNewPlayer == 1
                    ? newPlayerGreeting
                    : returningPlayerGreeting[
                        Random.Range(0, returningPlayerGreeting.Count)
                    ]));
        }

        if (_input.move == Vector2.zero)
        {
            if ((idleTimer -= Time.deltaTime) <= 0f)
            {
                AddEnergy(idleEnergy);
                ResetValue(ref idleTimer, nameof(idleTimer));
            }
        }
        else
        {
            if (_input.sprint)
            {
                if ((runTimer -= Time.deltaTime) <= 0f)
                {
                    LowerEnergy(runEnergy);
                    ResetValue(ref runTimer, nameof(runTimer));
                }
                if (energy <= 30f && (lowEnergyRunTimer -= Time.deltaTime) <= 0f)
                {
                    LowerHealth(lowEnergyRunHealth);
                    ResetValue(ref lowEnergyRunTimer, nameof(lowEnergyRunTimer));
                }
            }
            else if ((walkTimer -= Time.deltaTime) <= 0f)
            {
                LowerEnergy(walkEnergy);
                ResetValue(ref walkTimer, nameof(walkTimer));
            }
        }

        if (isLowEnergy && (lowEnergyTimer -= Time.deltaTime) <= 0f)
        {
            LowerHealth(lowEnergyHealth);
            ResetValue(ref lowEnergyTimer, nameof(lowEnergyTimer));
        }

        if (isPoisoned)
        {
            if (wasPoisonedFirstTime.Equals(0))
            {
                wasPoisonedFirstTime = 1;
#if PRODUCTION
                PlayerPrefs.SetInt(nameof(wasPoisonedFirstTime), wasPoisonedFirstTime);
#endif
                tips.text = firstPoisonTip;
            }

            if ((poisonTimer -= Time.deltaTime) <= 0f)
            {
                LowerEnergy(poisonEnergy);
                LowerHealth(poisonHealth);
                ResetValue(ref poisonTimer, nameof(poisonTimer));
            }

            if (!wasPoisonSoundPlayed
                || (poisonSoundTimer -= Time.deltaTime) <= 0f)
            {
                ResetValue(ref poisonSoundTimer, nameof(poisonSoundTimer));
                StartCoroutine(PlayPlayerSound(poisonClip));
                if (!wasPoisonSoundPlayed)
                    wasPoisonSoundPlayed = true;
            }
        }

        if ((smallEventTimer -= Time.deltaTime) <= 0f)
        {
            StartCoroutine(DoSmallEvent());
            SetSmallEventTimer();
        }

        if (energy >= 80f && !health.Equals(defaultVals[nameof(health)]))
        {
            if ((healthRecoverTimer -= Time.deltaTime) <= 0f)
            {
                LowerEnergy(healthRecoverEnergy);
                AddHealth(recoverHealth);
                ResetValue(ref healthRecoverTimer, nameof(healthRecoverTimer));
            }
        }

        if (energy <= 10f)
        {
            if (!isLowEnergy)
                isLowEnergy = true;
            if (energyFill.color != Color.red)
                energyFill.color = Color.red;
        }
        else
        {
            if (energy <= 30f)
            {
                if (energyFill.color != Color.yellow)
                    energyFill.color = Color.yellow;
            }
            else
            {
                if (energyFill.color != Color.blue)
                    energyFill.color = Color.blue;
            }
            if (!lowEnergyTimer.Equals(defaultVals[nameof(lowEnergyTimer)]))
                ResetValue(ref lowEnergyTimer, nameof(lowEnergyTimer));
            if (isLowEnergy)
                isLowEnergy = false;
        }

        if (health <= 20f)
        {
            if (healthFill.color != Color.red)
                healthFill.color = Color.red;
            if (!lowHealthSource.isPlaying)
                lowHealthSource.Play();
        }
        else
        {
            if (energy <= 50f)
            {
                if (healthFill.color != Color.yellow)
                    healthFill.color = Color.yellow;
            }
            else
            {
                if (healthFill.color != Color.green)
                    healthFill.color = Color.green;
            }
            if (lowHealthSource.isPlaying)
                lowHealthSource.Stop();
        }

        healthSlider.value = health;
        energySlider.value = energy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        switch (other.gameObject.name)
        {
            case "EventOne":
                if (!isEventOneTriggered)
                    StartCoroutine(DoLargeEvent(DoEventOne, BARRIERS.ONE));
                break;
            case "EventTwo":
                if (!isEventTwoTriggered)
                    StartCoroutine(DoLargeEvent(DoEventTwo, BARRIERS.TWO));
                break;
            case "EventThree":
                if (!isEventThreeTriggered)
                    StartCoroutine(DoLargeEvent(DoEventThree, BARRIERS.THREE));
                break;
            case "BossEvent":
                if (!isBossEventTriggered)
                    StartCoroutine(DoLargeEvent(DoBossEvent));
                break;

            case "HolyArea":
                if (!isInHolyArea) isInHolyArea = true;
                if (isHolyAreaActive)
                {
                    isHolyAreaActive = false;
                    ++holyAreaUseCount;
                    ResetValue(ref energy, nameof(energy));
                    ResetValue(ref health, nameof(health));
                    StartCoroutine(ReactivateHolyArea());
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (other.gameObject.name == "HolyArea"
            && isInHolyArea)
            isInHolyArea = false;
    }

    private void RedMoon()
    {
        moon.color = Color.red;
        moon.intensity = 2f;
    }

    private void NormalMoon()
    {
        moon.color = Color.white;
        moon.intensity = 0.1f;
    }

    private void PlayClip(AudioClip clip, bool loop)
    {
        mainAudioSource.Stop();
        mainAudioSource.clip = clip;
        mainAudioSource.loop = loop;
        mainAudioSource.Play();
    }

    IEnumerator ReactivateHolyArea()
    {
        yield return new WaitForSeconds(holyAreaUseCount * 60f);
        isHolyAreaActive = true;
    }

#nullable enable
    IEnumerator WaitForSound(Enum? option = null)
    {
        while (mainAudioSource.isPlaying)
            yield return null;

        ResetEvent(EVENTS.LARGE);
        NormalMoon();
        PlayClip(normalAudio, true);
        
        switch (option)
        {
            case null:
                yield break;
            case BARRIERS.ONE:
                eventOneBarrier.enabled = false;
                break;
            case BARRIERS.TWO:
                eventTwoBarrier.enabled = false;
                break;
            case BARRIERS.THREE:
                eventThreeBarrier.enabled = false;
                break;
        }
    }
#nullable disable

    public IEnumerator PlayPlayerSound(AudioClip sound)
    {
        switch (sound.name)
        {
            case eatSoundName:
                if (isEatSoundQueued) yield break;
                isEatSoundQueued = true;
                break;
            case drinkSoundName:
                if (isDrinkSoundQueued) yield break;
                isDrinkSoundQueued = true;
                break;
            case poisonSoundName:
                if (isPoisonSoundQueued) yield break;
                isPoisonSoundQueued = true;
                break;
        }
        
        while (playerSource.isPlaying)
            yield return null;

        playerSource.clip = sound;
        playerSource.Play();

        switch (sound.name)
        {
            case eatSoundName:
                isEatSoundQueued = false;
                break;
            case drinkSoundName:
                isDrinkSoundQueued = false;
                break;
            case poisonSoundName:
                isPoisonSoundQueued = false;
                break;
        }
    }

    IEnumerator DoEventOne()
    {
        isEventOneTriggered = true;
        RedMoon();
        PlayClip(eventOneClip, false);
        LowerEnergy(largeEventEnergy);
        LowerHealth(largeEventHealth);
        yield return new WaitForSeconds(3);

        foreach (string str in eventOneLines)
        {
            text.text = str;
            yield return new WaitForSeconds(6);
        }

        text.text = "";
    }

    IEnumerator DoEventTwo()
    {
        isEventTwoTriggered = true;
        RedMoon();
        PlayClip(eventTwoClip, false);
        LowerEnergy(largeEventEnergy);
        LowerHealth(largeEventHealth);
        yield return new WaitForSeconds(3);

        foreach (string str in eventTwoLines)
        {
            text.text = str;
            yield return new WaitForSeconds(6);
        }

        text.text = "";
    }

    IEnumerator DoEventThree()
    {
        isEventThreeTriggered = true;
        RedMoon();
        PlayClip(eventThreeClip, false);
        LowerEnergy(largeEventEnergy);
        LowerHealth(largeEventHealth);
        yield return new WaitForSeconds(3);

        foreach (string str in eventThreeLines)
        {
            text.text = str;
            yield return new WaitForSeconds(5);
        }

        text.text = "";
    }

    IEnumerator DoBossEvent()
    {
        isBossEventTriggered = true;
        RedMoon();
        PlayClip(bossClip, true);
        LowerEnergy(largeEventEnergy);
        LowerHealth(largeEventHealth);
        yield return new WaitForSeconds(3);

        foreach (string str in bossEventLines)
        {
            text.text = str;
            yield return new WaitForSeconds(6);
        }

        text.text = "";
    }

    IEnumerator DoSmallEvent()
    {
        while (isSmallEventOngoing)
            yield return null;

        SetEvent(EVENTS.SMALL);
        if (!isLargeEventOngoing)
        {
            RedMoon();
            mainAudioSource.Stop();
            mainAudioSource.clip = smallEventClip;
            mainAudioSource.Play();
        }

        string helperString = helper.text;
        string textString = text.text;
        string tipsString = tips.text;
        
        string randomString = smallEventLines[Random.Range(0, smallEventLines.Count)];
        helper.text = randomString;
        text.text = randomString;
        tips.text = randomString;

        LowerEnergy(smallEventEnergy);
        yield return new WaitForSeconds(smallEventDuration);

        helper.text = helperString;
        text.text = textString;
        tips.text = tipsString;

        if (!isLargeEventOngoing)
        {
            mainAudioSource.Stop();
            mainAudioSource.clip = normalAudio;
            mainAudioSource.Play();
            NormalMoon();
        }

        ResetEvent(EVENTS.SMALL);

        if ((smallEventDuration += 0.05f).Equals(1.05f))
            didSmallEventDurationCross1s = true;
        if (didSmallEventDurationCross1s)
            smallEventDuration = Random.Range(0.1f, 0.9f);
    }

#nullable enable
    IEnumerator DoLargeEvent(Func<IEnumerator> eventFunc, Enum? barrierOpt = null)
    {
        while (isSmallEventOngoing || isLargeEventOngoing)
            yield return null;

        SetEvent(EVENTS.LARGE);
        StartCoroutine(eventFunc());
        if (eventFunc != DoBossEvent) {
            yield return new WaitForSeconds(5);
            StartCoroutine(WaitForSound(barrierOpt));
        }
    }
#nullable disable

    public IEnumerator UpdateText(string newText)
    {
        while (isSmallEventOngoing || isLargeEventOngoing)
            yield return null;

        text.text = newText;
        yield return new WaitForSeconds(5);

        while (isSmallEventOngoing || isLargeEventOngoing)
            yield return null;
        text.text = "";
    }

    public IEnumerator UpdateHelper(string newHelper)
    {
        while (isSmallEventOngoing)
            yield return null;

        helper.text = newHelper;
    }

    public IEnumerator ClearPoisonTip()
    {
        while (isSmallEventOngoing)
            yield return null;

        if (tips.text.Equals(firstPoisonTip))
            tips.text = "";
    }
}