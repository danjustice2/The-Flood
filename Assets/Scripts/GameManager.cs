using DigitalRuby.RainMaker;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/*
    This code is written by me except where marked otherwise.
    There's a lot going on in this class. In hindsight I definitely would've divided it up into several different classes.
    I'll try to guide you through it with comments.
*/

public class GameManager : MonoBehaviour
{
    // There're a lot of variables going on there...
    [SerializeField] float TimeAtPeak = 40;
    [SerializeField] GameObject Manager;
    [SerializeField] float TimeUntilStart;
    [SerializeField] float TimeUntilWarning;
    [SerializeField] GameObject FloodWarning;
    [SerializeField] GameObject RainManager;
    InputMaster controls;
    [SerializeField] int RainStart = 5;
    bool FloodStarted = false;
    public bool FloodFinished = false;
    [SerializeField] float WaterSpeed;
    [SerializeField] int WaterMaxHeight;
    [SerializeField] int WaterMinHeight = 39;
    [SerializeField] GameObject Water;
    public bool IsUnderwater;
    [SerializeField] GameObject UnderwaterUI;
    [SerializeField] GameObject DeathUI;
    [SerializeField] GameObject Player;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] float Air = 30;
    [SerializeField] float MaxAir = 30;
    [SerializeField] GameObject BreathRing;
    [SerializeField] bool IsDead = false;
    public bool Paused = false;
    [SerializeField] GameObject EscMenu;
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] AudioMixer Mixer;
    bool isCovered;
    [Range(0f, 271f)]
    [SerializeField] float BodyTemperature = 250f;
    float TemperatureChange;
    [SerializeField] GameObject TemperaturePointer;
    float MinTemp;
    float MaxTemp;
    [SerializeField] GameObject PlayerGroundCheck;
    [Range(0f, 1f)]
    [SerializeField] float Wetness;
    [Range(1, 5)]
    int Exposure;
    public bool IsInWater;
    bool WarningGiven = false;
    [SerializeField] GameObject WinScreen;
    public bool LifeJacket;
    [SerializeField] GameObject LifeJacketParticles;
    public bool AirTank;
    [SerializeField] GameObject AirTankParticles;
    public bool Jacket;
    [SerializeField] GameObject JacketParticles;
    public bool Interactables;
    public GameObject three;
    private Animation anim;
    [SerializeField] float TimeToDeath;
    bool IsFreezing = false;
    float StartTimeToDeath = 30;
    [SerializeField] GameObject Blackness;

    [SerializeField] GameObject WelcomeHome;
    [SerializeField] GameObject RainTutorial;
    public GameObject FloodWarningTutorial;

    [SerializeField] GameObject DoorFeedback;
    [SerializeField] GameObject DoorFeedbackFailed;
    [SerializeField] GameObject JacketFeedback;
    [SerializeField] GameObject LifeJacketFeedback;
    [SerializeField] GameObject AirTankFeedback;

    bool Darkening;
    [SerializeField] Light Light;
    string SceneName;
    [SerializeField] GameObject MenuManager;

#if UNITY_EDITOR
    [SerializeField] bool GodMode = false; // a feature for ignoring death for debugging in the editor
#endif

    private void Awake()
    {
        controls = new InputMaster();
        Mixer.SetFloat("Lowpass", 22000); // The lowpass effect shouldn't be on on start. This is here because I had some problems with this.
    }

    void Start()
    {
        // Make sure it's not raining
        RainManager.GetComponent<RainScript>().RainIntensity = 0f;

        // This is the top blue part of the exposure wheel. We play around with this when the player equips the jacket.
        three = GameObject.Find("3");

        // We get the particle systems attached to all three of the applicable items.
        // We turn off the particles and interactabilty until the flood warning comes.
        LifeJacketParticles = GameObject.Find("LifeJacket/Particle System");
        JacketParticles = GameObject.Find("Jacket/Particle System");
        AirTankParticles = GameObject.Find("AirTank/Particle System");

        // As mentioned before, we turn off interactability until the flood warning comes.
        ToggleInteractables(false);

        // We get this because, at the end of the game, there is some things that happen in one level that don't happen in the others.
        Scene scene = SceneManager.GetActiveScene();
        SceneName = scene.name;


#if !UNITY_EDITOR
        // I want to be able to have the game start with custom water levels in the editor, but in real builds of the game, the water should always start at its minimum level.
        WaterReset();
#endif
        // Finally, we want the WelcomeHome tutorial window to be visible upon load.
        StartTutorial(WelcomeHome);
    }

    void Update()
    {
        // If the paused value is true but time is still running, stop it.
        if (Paused && Time.timeScale == 1) { Time.timeScale = 0f; }

        // If the paused value is false but time is paused, start it.
        else if (!Paused && Time.timeScale == 0f) { Time.timeScale = 1; }

        // If the game isn't paused, we do our normal stuff.
        // The IsDead variable is mostly included for debugging and error prevention.
        else if (!Paused && !IsDead)
        {
            // Check if player is under water. More on the variable down in the function.
            UnderwaterCheck(0.25f);

            // Manage player's air
            AirManagement();

            // Manage the player's body temperature (exposure). Don't do this if the flood is finished.
            // The reason for the if statement is that I want the player to be able to explore the world more freely after winning
            if (!FloodFinished) { ExposureManagement(); }

            // Count down on the timer to start the flood.
            if (TimeUntilStart > 0)
            {
                TimeUntilStart -= Time.deltaTime;
            }

            // Count down on another timer to start the flood warning.
            if (TimeUntilWarning > 0)
            {
                TimeUntilWarning -= Time.deltaTime;
            }

            // We start the rain a little bit before the flood starts so it doesn't seem so abrupt.
            // We don't do this if the flood is finished.
            if (TimeUntilStart < RainStart && RainManager.GetComponent<RainScript>().RainIntensity < 1f && !FloodFinished)
            {
                RainManager.GetComponent<RainScript>().RainIntensity = 1f; // Make it rain
                StartTutorial(RainTutorial); // Open the tutorial for when the rain starts
                Debug.Log("Setting RainIntensity to 1");
                Darkening = true; // This will darken the lighting in the game world a bit
            }

            // If the darkening value is true and the light intensity hasn't already been turned down...
            if (Darkening && Light.intensity > 0.2f)
            {
                Light.intensity -= Time.deltaTime / 5; // Turn it down gradually... (I haven't tested just how gradual this is)
            }

            // Start the flood when it's time.
            if (TimeUntilStart <= 0 && !FloodStarted)
            {
                FloodStart();
            }

            // Start the flood warning a bit before the flood starts coming
            if (TimeUntilWarning <= 0 && !WarningGiven)
            {
                WarningStart();
            }

            // End the flood when it has been at its peak for the specified amount of time
            if (TimeAtPeak <= 0 && !FloodFinished)
            {
                FloodEnd();
            }
        
            // Water rise loop until the water reaches the max height.
            if (FloodStarted && !FloodFinished && Water.transform.position.y <= WaterMaxHeight)
            {
                WaterRise();
            }

            // Same as WaterRise but in reverse for when the flood is over.
            else if (FloodFinished && Water.transform.position.y >= WaterMinHeight)
            {
                WaterRecede();
            }

            // Count down for the time between the water reaching its MaxHeight and when the water should start receding again.
            else if (TimeAtPeak > 0 && Water.transform.position.y >= WaterMaxHeight)
            {
                TimeAtPeak -= Time.deltaTime;
            }
        }

#if UNITY_EDITOR
        // For debugging purposes I wanted to be able to have the player start the game dead.
        else if (IsDead && PlayerCamera.GetComponent<MouseLook>().enabled) { Death(); } 
#endif

        // If the player presses the button and ins't dead, toggle the esc menu
        if (controls.Player.Menu.triggered && !IsDead && !SettingsMenu.activeSelf)
        {
            ToggleEscMenu();
        }

        // The player should be able to press the close button to go to the next level if they are at level2
        // Note that level2 is the *first* level the player plays. This is because I made the second level first.
        if (FloodFinished && SceneName == "level2" && controls.Player.Close.triggered)
        {
            MenuManager.GetComponent<Menu>().LoadScene("level1");
            Debug.Log("Loading level1");
        }
    }

    public void ToggleEscMenu()
    {
        // If we're paused already, close the menu
        if (Paused && EscMenu.activeSelf)
        {
            Paused = false;
            EscMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            if (!IsUnderwater)
            {
                // Don't set the sound back to normal if the player is under water. Otherwise set it to normal.
                Mixer.SetFloat("Lowpass", 22000); 
            }
        }

        // If we're not paused, pause the game and apply the lowpass effect
        else if (!Paused)
        {
            Paused = true;
            EscMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Mixer.SetFloat("Lowpass", 532); // Cool muffle effect on game sounds, inspired by games like GTA V
        }
    }

    void UnderwaterCheck(float yoffset)
    {
        // If we're at least yoffset units unter the water, we are under water for the purposes of this part.
        // If they are not equal, then we know we have to change something.
        // 0.25 units is used for the general UnderWaterCheck because this is the point where the camera is actually about half way under the water. This was just a matter of playing with it and figuring out what works best.
        if (IsUnderwater != WaterCheck(yoffset))
        {
            if (IsUnderwater) 
            {
                IsUnderwater = false;
                UnderwaterUI.SetActive(false); // Turn off the under water effect.
                Mixer.SetFloat("Lowpass", 22000); // Set the sounds to normal again
            }
            else if (!IsUnderwater)
            {
                IsUnderwater = true;
                UnderwaterUI.SetActive(true); // Brown overlay to sell the under water effect.
                Mixer.SetFloat("Lowpass", 532); // Cool muffle effect on game sounds while under water
            }
        }
    }

    void ToggleInteractables(bool state)
    {
        // Turn on/off the interactables particle effects.
        // + error handling
        if (LifeJacketParticles != null) { LifeJacketParticles.SetActive(state); } else { Debug.LogError("LifeJacketParticles is null"); }
        if (JacketParticles != null) { JacketParticles.SetActive(state); } else { Debug.LogError("JacketParticles is null"); }
        if (AirTankParticles != null) { AirTankParticles.SetActive(state); } else { Debug.LogError("AirTankParticles is null"); }
        
        // Set the interactables value to whatever it should be
        Interactables = state;
    }

    public void GiveLifeJacket()
    {
        LifeJacket = true; // Set the value to true
        OpenInteractableFeedback(LifeJacketFeedback); // Open the interactable feedback
        Debug.Log("GameManager: Life jacket given");
    }

    public void GiveJacket()
    {
        OpenInteractableFeedback(JacketFeedback); // Open the interactable feedback
        Jacket = true; // Set the value to true
        StartCoroutine("GiveJacketCoroutine"); // Start the coroutine that animates the change on the exposure wheel 
    }

    public IEnumerator GiveJacketCoroutine()
    // It's easier to understand this just by going in the game and seeing the animation
    // Help from youtube: https://www.youtube.com/watch?v=YqMpVCPX2ls Master UI Animations
    // And unity package: https://assetstore.unity.com/packages/tools/animation/leantween-3595 LeanTween
    {
        GameObject ExposureWindow = GameObject.Find("Exposure");

        LeanTween.scale(three, new Vector3(1.5f, 1.5f, 1.5f), 1.5f).setEaseOutQuart();
        LeanTween.scale(ExposureWindow, new Vector3(1.5f, 1.5f, 1.5f), 1.5f).setEaseOutQuart();
        
        yield return new WaitForSeconds(2f);

        three.GetComponent<Image>().color = new Color(0.5943396f, 0.5943396f, 0.5943396f, 0.5f);

        yield return new WaitForSeconds(1.5f);

        LeanTween.scale(three, new Vector3(0.9f, 0.9f, 0.9f), 1.5f).setEaseInQuart();
        LeanTween.scale(ExposureWindow, new Vector3(1f, 1f, 1f), 1.5f).setEaseInQuart();

        yield return new WaitForSeconds(2f);
        three.GetComponent<Image>().color = new Color(0.5943396f, 0.5943396f, 0.5943396f, 0.2784314f);

        Debug.Log("GameManager: Jacket given");
    }

    // Pretty much the same deal as the other items
    public void GiveAirTank()
    {
        AirTank = true;
        OpenInteractableFeedback(AirTankFeedback);
        MaxAir = MaxAir * 2; // Multiply the max air by 2
        Debug.Log("GameManager: Air tank given");
    }

    public void OpenInteractableFeedback(GameObject feedback)
    {
        CloseInteractableFeedbacks(); // Close any other open feedbacks

        feedback.SetActive(true); // Activate the selected one
    }

    void CloseInteractableFeedbacks()
    {
        // Close all of the interactable feedbacks
        // + error handling
        if (DoorFeedback != null) { DoorFeedback.SetActive(false); } else { Debug.Log("DoorFeedback is null"); }
        if (DoorFeedback != null) { DoorFeedbackFailed.SetActive(false); } else { Debug.Log("DoorFeedback is null"); }
        if (JacketFeedback != null) { JacketFeedback.SetActive(false); } else { Debug.Log("JacketFeedback is null"); }
        if (LifeJacketFeedback != null) { LifeJacketFeedback.SetActive(false); } else { Debug.Log("LifeJacketFeedback is null"); }
        if (AirTankFeedback != null) { AirTankFeedback.SetActive(false); } else { Debug.Log("AirTankFeedback is null"); }
    }

    public void StartTutorial(GameObject tutorial)
    // Same deal as with the feedbacks
    {
        // First we close all other tutorials to be sure they don't overlap.
        CloseTutorials();

        // Then we open the desired tutorial.
        tutorial.SetActive(true);
    }

    void CloseTutorials()
    // Same deal as with the feedbacks - error handling
    {
        // We close all other tutorials to be sure they don't overlap.
        WelcomeHome.SetActive(false);
        RainTutorial.SetActive(false);
        FloodWarningTutorial.SetActive(false);
    }

    public bool WaterCheck(float yoffset)
    // Checks the playercamera's position compared to the water, accomodating for a y-offset.
    {
        if (PlayerCamera.transform.position.y <= Water.transform.position.y + yoffset)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    void AirManagement()
    {
        if (IsUnderwater && Air > 0) // if you're under water and have air left, take air away
        {
            Air -= Time.deltaTime;
        }
        else if (!IsUnderwater && MaxAir > Air) // If you're not under water and you're missing air, breathe!
        {
            Air += Time.deltaTime * 2;
        }
        else if (Air <= 0) // If you run out of air... drown
        {
            Drown();
        }
        else if (Air > MaxAir) // If you managed to have more than MaxAir, set your air to MaxAir.
        {
            Air = MaxAir;
        }

        AirUI(); // Control the breath ring
    }

    void AirUI()
    {
        if (!IsDead) // Don't ever show it if the player is dead...
        {
            if (Air >= MaxAir && BreathRing.activeSelf) // If it's enabled and the player is full air, disable
            {
                BreathRing.SetActive(false);
            }
            else if (Air < MaxAir) // If the air has dropped...
            {
                if (!BreathRing.activeSelf) { BreathRing.SetActive(true); } // Enable the breath ring if it isn't already

                BreathRing.GetComponent<Image>().fillAmount = (Air / MaxAir); // Count down the fill of the breath ring... now that's one important line of code!!
            }
        }
        else if (IsDead && BreathRing.activeSelf)
        // The breath ring doesn't need to be enabled if the player is dead.
        {
            BreathRing.SetActive(false);
        }

    }

    void FloodStart()
    {
        FloodStarted = true; // This function just flips this bool :)
        Debug.Log("Flood started.");
    }

    void WarningStart()
    {
        FloodWarning.SetActive(true); // Open the warning window.
        WarningGiven = true; // Set warning to given so the other voids know what's going on
        ToggleInteractables(true); // Enable the interactables!!
    }

    void FloodEnd()
    {
        Debug.Log("Flood finished");
        RainManager.GetComponent<RainScript>().RainIntensity = 0f; // Stop the rain
        FloodFinished = true; // Keep the rest of the game in the loop
        if (!IsDead) { Win(); } // If the player isn't already dead, they win! :D
        
    }

    void Win()
    {
        WinScreen.SetActive(true); // Open the win UI
        CloseTutorials(); // Close any and all tutorials
        CloseInteractableFeedbacks(); // Close any and all interactable feedbacks
        BodyTemperature = 260; // Warm up so they can take a look around! I had a problem in play tests where the player sometimes died after winning, and that's just weird...
    }

    void WaterRise()
    // Rise the water with Time.deltaTime
    {
        Water.transform.position += new Vector3(0, WaterSpeed * Time.deltaTime, 0);
    }

    void WaterRecede()
    // The exact opposite of WaterRecede
    {
        Water.transform.position -= new Vector3(0, WaterSpeed * Time.deltaTime, 0);
    }

    void WaterReset()
    // Set the water to its default level
    {
        Water.transform.position = new Vector3(0, WaterMinHeight, 0);
    }

    void Drown()
    // Future plans (if there is time) with implementing cause of death on DeathUI... this just directs to Death()
    {
        Death(); 
    }

    void Freeze()
        // Future plans (if there is time) with implementing cause of death on DeathUI... this just directs to Death()
    {
        Death();
    }
    void Death()
    {
#if UNITY_EDITOR
        // I don't want to die if god mode is enabled, but this is only for debugging in the editor.
        if (!GodMode) 
#endif
        {
            IsDead = true; // He's dead, Jim.

            Paused = true; // Pause so the game doesn't continue

            Cursor.lockState = CursorLockMode.None; // Give the cursor back

            DeathUI.SetActive(true); // Death UI enabled

            FloodFinished = true; // Redundant, but I like to keep it neat...

            WaterReset(); // Again, redundant, but neat

            // Disable player control scripts
            Player.GetComponent<PlayerController>().enabled = false;
            PlayerCamera.GetComponent<MouseLook>().enabled = false;

            Debug.Log("He's dead, Jim.");
        }
    }

    public void applySettings(float sensitivity, float fov, float volume)
    // Used by the settings script to apply the settings
    {
        PlayerCamera.GetComponent<MouseLook>().MouseSensitivity = sensitivity;
        PlayerCamera.GetComponent<Camera>().fieldOfView = fov;
        Mixer.SetFloat("Volume", Mathf.Log10(volume) * 20); // How to keep log scale in volume slider: https://www.youtube.com/watch?v=xNHSGMKtlv4
    }

    void ExposureManagement()
    {
        insideCheck(); // Checks if the player is inside, setting bool isCovered accordingly
        InWaterCheck(); // Checks if the player is in water, setting wetness level accordingly.


        /* Here, we set various max/min temperature values according to how the player is situated in the map. These specifications are probably overcomplicated, but it's what I've ended up with. */
        if (isCovered) // Is the player inside? Yes?
        {
            if (Wetness == 0) // The player gets wet if they go through flood waters.
            {
                if (RainManager.GetComponent<RainScript>().RainIntensity > 0) // Is it raining? Yes?
                {
                    MinTemp = 110;
                    MaxTemp = 250;
                }
                else if (RainManager.GetComponent<RainScript>().RainIntensity == 0) // Is it raining? No?
                {
                    MinTemp = 140;
                    MaxTemp = 270;
                }
            }
            else if (Wetness > 0) // Is the player wet?
            {
                MinTemp = 0;
                MaxTemp = 50; // The player will freeze even with a jacket if they are wet.
            }
            
        }
        else if (!isCovered) // Is the player inside? No?
        {
            if (RainManager.GetComponent<RainScript>().RainIntensity > 0)
            /* If the player is outdoors and it is raining, the temperature will get down to the level I call "3". This is important because the jacket will allow the player to survive this. */
            {
                MinTemp = 65;
                MaxTemp = 70;
            }
            else if (RainManager.GetComponent<RainScript>().RainIntensity == 0)
            /* If it's not raining, the player will actually technically be able to get a bit warmer by being outside in the sunlight. */
            {
                MinTemp = 160;
                MaxTemp = 270;
            }
        }

        if (BodyTemperature < MinTemp)
        /* If the player's body temperature is less than the minimum for the current space, start increasing it. 
        /* The wetness level (set to 1 when the player steps in flood waters and decreasing after coming out) is a multiplier for how quickly the player heats up/cools off */
        {
            TemperatureChange = Time.deltaTime * -2 / (Wetness+1); 
            ChangeTemperature();
        }
        else if (BodyTemperature > MaxTemp)
        /* If the player's body temperature is more than the maximum for the current space, start decreasing it. */
        {
            TemperatureChange = Time.deltaTime * 2 * (Wetness+1); 
            ChangeTemperature();
        }
        else if (BodyTemperature > MinTemp && BodyTemperature < MaxTemp)
        /* If the player's body temperature lies within the allowed range in the current space, set change to 0. */
        {
            TemperatureChange = 0;
        }

        ExposureLevel(); // Check the exposure level. This is a simplified way of dealing with temperatures, coordinating it with the player's UI.

        /* If the exposure is too much, the player will start dying. */
        if (Exposure <= 3 && !Jacket || Exposure <= 2 && Jacket)
        {

            if (!IsFreezing)
            // This starts the process of freezing to death.
            {
                IsFreezing = true;
                TimeToDeath = StartTimeToDeath;
                if (!Blackness.activeSelf) { Blackness.SetActive(true); } // Set blackness to active if it isn't already
            }
            else if (IsFreezing)
            {
                if (TimeToDeath > 0)
                // Count down on the timer
                {
                    TimeToDeath -= Time.deltaTime;
                    float Transparency = 1 - (TimeToDeath / StartTimeToDeath);
                    if (Blackness.GetComponent<Image>() != null) // <--- Error handling.
                    // It gets darker and darker
                    {
                        Blackness.GetComponent<Image>().color = new Color(0, 0, 0, Transparency);
                    }

                    else if (Blackness.GetComponent<Image>() == null) { Debug.LogError("Blackness' image is null."); } // Error handling

                }
                else if (TimeToDeath <= 0) // My time has come....
                {
                    Freeze(); // The player freezes to death.
                }
            }
            else if ((Exposure >= 3 && !Jacket || Exposure >= 2 && Jacket) && IsFreezing)
            // If the player has managed to warm up enough, we disable the black overlay. This is a bit roughly done, but oh well.
            {
                if (Blackness.activeSelf) { Blackness.SetActive(false); } // Set blackness to inactive if it is active
                Blackness.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                IsFreezing = false;
            }
        }
    }

    public void InWaterCheck()
    // this checks if the player is standing in water, as opposed to actually being UNDER water
    // That means it compares the water level with the ground check instead of the camera
    {
        if (PlayerGroundCheck.transform.position.y <= Water.transform.position.y)
        {
            if (!IsInWater) { IsInWater = true; }

            if (Wetness < 1f)
            {
                Wetness = 1;
            }

        }
        else if (PlayerCamera.transform.position.y > Water.transform.position.y)
        {
            if (IsInWater) { IsInWater = false; }

            if ((isCovered || RainManager.GetComponent<RainScript>().RainIntensity == 0) && Wetness > 0f)
            {
                Wetness -= Time.deltaTime / 20;
            }

        }
    }

    void ExposureLevel()
    {
    // Exposure levels 1-5 correspond to the blocks on the exposure UI wheel. I got these values by manually rotating the pointer on the wheel and finding what I found as suitable "max" values for each block.
        float top1 = 21.8f;
        float top2 = 58.7f;
        float top3 = 94f;
        float top4 = 247.5f;

        if (BodyTemperature <= top1) { Exposure = 1; }
        if (BodyTemperature > top1 && BodyTemperature <= top2) { Exposure = 2; }
        if (BodyTemperature > top2 && BodyTemperature <= top3) { Exposure = 3; }
        if (BodyTemperature > top3 && BodyTemperature <= top4) { Exposure = 4; }
        if (BodyTemperature > top4) { Exposure = 5; }
    }

    void ChangeTemperature()
    // The temperature change value set before get applied here.
    {
        if (BodyTemperature < 270 && BodyTemperature > 0)
        {
            BodyTemperature = BodyTemperature - TemperatureChange;
        }
        else if (BodyTemperature > 270)
        {
            BodyTemperature = 270;
        }
        else if (BodyTemperature < 0)
        {
            BodyTemperature = 0;
        }

        ChangePointer(); // Update the UI
    }

    void ChangePointer()
    {
        // Change the pointer
        TemperaturePointer.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 135.5f - BodyTemperature));

        // Change the pointer's color according to if the change is less than, equal to, or greater than 0.
        // This was supposed to become something more (a red or blue minus, a beige minus, or a green plus) but this wasn't a high priority
        if (TemperatureChange < 0 && TemperaturePointer.GetComponent<Image>().color != new Color32(124, 255, 255, 255))
        {
            TemperaturePointer.GetComponent<Image>().color = new Color32(124, 255, 255, 255);
        }
        else if (TemperatureChange > 0 && TemperaturePointer.GetComponent<Image>().color != new Color32(252, 3, 3, 255))
        {
            TemperaturePointer.GetComponent<Image>().color = new Color32(252, 3, 3, 255);
        }
        else if (TemperatureChange == 0 && TemperaturePointer.GetComponent<Image>().color != new Color32(255, 255, 255, 255))
        {
            TemperaturePointer.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }

    void insideCheck()
    // Checks if the player is covered under cover with raycasting
    {
        Vector3 up = PlayerCamera.transform.TransformDirection(Vector3.up);

        if (!Physics.Raycast(PlayerCamera.transform.position, up, 10))
        { isCovered = false; }
        else { isCovered = true; }
    }

    public IEnumerator AnimateExposure()
    // To animate the exposure ring to get bigger then smaller again.
    // This called by the RainTutorial.cs script... because it gets disabled right after it can't run Coroutines.
    // There was probably a more neat solution to this, but it wasn't a priority
    
    // Help from youtube: https://www.youtube.com/watch?v=YqMpVCPX2ls Master UI Animations
    // And unity package: https://assetstore.unity.com/packages/tools/animation/leantween-3595 LeanTween
    {
        GameObject ExposureWindow = GameObject.Find("Exposure");

        LeanTween.scale(ExposureWindow, new Vector3(1.5f, 1.5f, 1.5f), 1.5f).setEaseOutQuart();

        yield return new WaitForSeconds(2f);

        LeanTween.scale(ExposureWindow, new Vector3(1f, 1f, 1f), 1.5f).setEaseInQuart();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
