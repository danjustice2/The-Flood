using DigitalRuby.RainMaker;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

//Test
public class GameManager : MonoBehaviour
{
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
    [SerializeField] Image Blackness;

    [SerializeField] GameObject WelcomeHome;
    [SerializeField] GameObject RainTutorial;
    public GameObject FloodWarningTutorial;
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
    }

    void Start()
    {
        RainManager.GetComponent<RainScript>().RainIntensity = 0f;
        three = GameObject.Find("3"); // So we can play around with the exposure wheel when the player picks up the jacket.
        anim = gameObject.GetComponent<Animation>();

        LifeJacketParticles = GameObject.Find("LifeJacket/Particle System");
        JacketParticles = GameObject.Find("Jacket/Particle System");
        AirTankParticles = GameObject.Find("AirTank/Particle System");

        ToggleInteractables(false);

        Scene scene = SceneManager.GetActiveScene();
        SceneName = scene.name;


#if !UNITY_EDITOR
        WaterReset(); // I want to be able to have the game start with custom water levels in the editor, but in real builds of the game, the water should always start at its minimum level.
#endif

        StartTutorial(WelcomeHome);
    }

    void Update()
    {
        if (Paused && Time.timeScale == 1) { Time.timeScale = 0f; } // If we're supposed to be paused but the time scale is still running, stop time.
        else if (!Paused && Time.timeScale == 0f) { Time.timeScale = 1; } // The opposite of the above

        else if (!Paused && !IsDead) // We only want to do these things if the game isn't paused. The IsDead variable is for debugging.
        {
            UnderwaterCheck(0.25f); // Check if player is under water.
            AirManagement(); // Manage player's air
            ExposureManagement(); // Manage exposure to the elements. Stuff like, is the player standing in the rain etc.

            if (TimeUntilStart > 0)
            // Count down on the timer to start the flood.
            {
                TimeUntilStart -= Time.deltaTime;
            }

            if (TimeUntilWarning > 0)
            // Count down on the timer to start the flood warning.
            {
                TimeUntilWarning -= Time.deltaTime;
            }

            if (TimeUntilStart < RainStart && RainManager.GetComponent<RainScript>().RainIntensity < 1f)
            // We start the rain a little bit before the flood starts so it doesn't seem so abrupt.
            {
                RainManager.GetComponent<RainScript>().RainIntensity = 1f;
                StartTutorial(RainTutorial);
                Debug.Log("Setting RainIntensity to 1");
                Darkening = true;
            }

            if (Darkening && Light.intensity > 0.2f)
            {
                Light.intensity -= Time.deltaTime / 5;
            }

            if (TimeUntilStart <= 0 && !FloodStarted)
            // Start the flood when it's time.
            {
                FloodStart();
            }

            if (TimeUntilWarning <= 0 && !WarningGiven)
            // Start the flood warning a bit before the flood starts coming
            {
                WarningStart();
            }

            if (TimeAtPeak <= 0 && !FloodFinished)
            // End the flood when it is time
            {
                FloodEnd();
            }
            
            if (FloodStarted && !FloodFinished && Water.transform.position.y <= WaterMaxHeight)
            // Water rise loop until the water reaches the max height.
            {
                WaterRise();
            }
            else if (FloodFinished && Water.transform.position.y >= WaterMinHeight)
            // Same as WaterRise but in reverse for when the flood is over.
            {
                WaterRecede();
            }
            else if (TimeAtPeak > 0 && Water.transform.position.y >= WaterMaxHeight)
            // Count down for the time between the water reaching its MaxHeight and when the water should start receding again.
            {
                TimeAtPeak -= Time.deltaTime;
            }
        }

#if UNITY_EDITOR
        else if (IsDead && PlayerCamera.GetComponent<MouseLook>().enabled) { Death(); } // For debugging purposes I wanted to be able to have the player start the game dead.
#endif

        if (controls.Player.Menu.triggered && !IsDead && !SettingsMenu.activeSelf)
        {
            ToggleEscMenu();
        }

        if (FloodFinished && SceneName == "level2" && controls.Player.Close.triggered)
        {
            MenuManager.GetComponent<Menu>().LoadScene("level1");
            Debug.Log("Loading level1");
        }
    }

    public void ToggleEscMenu()
    {
        if (Paused && EscMenu.activeSelf)
        {
            Paused = false;
            EscMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            if (!IsUnderwater)
            {
                Mixer.SetFloat("Lowpass", 22000); // Don't set the sound back to normal if the player is under water.
            }
        }
        else if (!Paused)
        {
            Paused = true;
            EscMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Mixer.SetFloat("Lowpass", 532); // Cool muffle effect on game sounds
        }
    }

    void UnderwaterCheck(float yoffset)
    {
        if (IsUnderwater != WaterCheck(yoffset))
        {
            if (IsUnderwater) 
            {
                IsUnderwater = false;
                UnderwaterUI.SetActive(false);
                Mixer.SetFloat("Lowpass", 22000);
            }
            else if (!IsUnderwater)
            {
                IsUnderwater = true;
                UnderwaterUI.SetActive(true);
                Mixer.SetFloat("Lowpass", 532); // Cool muffle effect on game sounds
            }
        }
    }

    void ToggleInteractables(bool state)
    {
        if (LifeJacketParticles != null) { LifeJacketParticles.SetActive(state); } else { Debug.LogError("LifeJacketParticles is null"); }
        if (JacketParticles != null) { JacketParticles.SetActive(state); } else { Debug.LogError("JacketParticles is null"); }
        if (AirTankParticles != null) { AirTankParticles.SetActive(state); } else { Debug.LogError("AirTankParticles is null"); }
        Interactables = state;
    }

    public void GiveLifeJacket()
    {
        LifeJacket = true;
        Debug.Log("GameManager: Life jacket given");
    }

    public void GiveJacket()
    {
        StartCoroutine("GiveJacketCoroutine");
    }

    public IEnumerator GiveJacketCoroutine()
    {
        Jacket = true;
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

    public void GiveAirTank()
    {
        AirTank = true;
        MaxAir = MaxAir * 2;
        Debug.Log("GameManager: Air tank given");
    }

    public void StartTutorial(GameObject tutorial)
    {
        // First we close all other tutorials to be sure they don't overlap.
        WelcomeHome.SetActive(false);
        RainTutorial.SetActive(false);
        FloodWarningTutorial.SetActive(false);

        // Then we open the desired tutorial.
        tutorial.SetActive(true);
    }

    public bool WaterCheck(float yoffset)
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
        if (IsUnderwater && Air > 0)
        {
            Air -= Time.deltaTime;
        }
        else if (!IsUnderwater && MaxAir > Air)
        {
            Air += Time.deltaTime * 2;
        }
        else if (Air <= 0)
        {
            Drown();
        }
        else if (Air > MaxAir)
        {
            Air = MaxAir;
        }

        AirUI();
    }

    void AirUI()
    {
        if (!IsDead)
        {
            if (Air >= MaxAir && BreathRing.activeSelf)
            {
                BreathRing.SetActive(false);
            }
            else if (Air < MaxAir)
            {
                if (!BreathRing.activeSelf) { BreathRing.SetActive(true); }

                BreathRing.GetComponent<Image>().fillAmount = (Air / MaxAir);
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
        FloodStarted = true;
        Debug.Log("Flood started.");
    }

    void WarningStart()
    {
        FloodWarning.SetActive(true);
        WarningGiven = true;
        ToggleInteractables(true);
    }

    void FloodEnd()
    {
        Debug.Log("Flood finished");
        FloodFinished = true;
        RainManager.GetComponent<RainScript>().RainIntensity = 0f;
        if (!IsDead) { WinScreen.SetActive(true); }
        
    }

    void WaterRise()
    {
        Water.transform.position += new Vector3(0, WaterSpeed * Time.deltaTime, 0);
    }

    void WaterRecede()
    {
        Water.transform.position -= new Vector3(0, WaterSpeed * Time.deltaTime, 0);
    }

    void WaterReset()
    {
        Water.transform.position = new Vector3(0, WaterMinHeight, 0);
    }

    void Drown()
    {
        Death(); // Future plans (if there is time) with implementing cause of death on DeathUI...
    }
    void Freeze()
    {
        Death(); // Future plans (if there is time) with implementing cause of death on DeathUI...
    }
    void Death()
    {
#if UNITY_EDITOR
        if (!GodMode) // I don't want to die if god mode is enabled, but this is only for debugging in the editor.
#endif
        {
            IsDead = true;
            Paused = true;
            Cursor.lockState = CursorLockMode.None;
            DeathUI.SetActive(true);
            Debug.Log("Player died.");
            FloodFinished = true;
            Water.transform.position = new Vector3(0, 39, 0);
            Player.GetComponent<PlayerController>().enabled = false;
            PlayerCamera.GetComponent<MouseLook>().enabled = false;
        }
    }

    public void applySettings(float sensitivity, float fov, float volume)
    {
        PlayerCamera.GetComponent<MouseLook>().MouseSensitivity = sensitivity;
        PlayerCamera.GetComponent<Camera>().fieldOfView = fov;
        Mixer.SetFloat("Volume", Mathf.Log10(volume) * 20);
    }

    void ExposureManagement()
    {
        insideCheck(); // Checks if the player is inside, setting bool isCovered accordingly
        InWaterCheck(); // Checks if the player is in water, setting wetness level accordingly.


        /* Here, we set various max/min temperature values according to how the player is situated in the map. These specifications are probably overcomplicated, but it's what I've ended up with. */
        if (isCovered)
        {
            if (Wetness == 0) // The player gets wet if they go through flood waters.
            {
                if (RainManager.GetComponent<RainScript>().RainIntensity > 0)
                {
                    MinTemp = 110;
                    MaxTemp = 250;
                }
                else if (RainManager.GetComponent<RainScript>().RainIntensity == 0)
                {
                    MinTemp = 140;
                    MaxTemp = 270;
                }
            }
            else if (Wetness > 0)
            {
                MinTemp = 0;
                MaxTemp = 50; // The player will freeze even with a jacket if they are wet.
            }
            
        }
        else if (!isCovered)
        {
            /* If the player is outdoors and it is raining, the temperature will be on the level I call "3". This is important because the jacket will allow the player to survive this. */
            if (RainManager.GetComponent<RainScript>().RainIntensity > 0)
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

        ExposureLevel();

        /* If the exposure is too much, the player will start dying. */
        if (Exposure <= 3 && !Jacket || Exposure <= 2 && Jacket)
        {

            if (!IsFreezing)
            {
                IsFreezing = true;
                TimeToDeath = StartTimeToDeath;
            }
            else if (IsFreezing)
            {
                if (TimeToDeath > 0)
                {
                    TimeToDeath -= Time.deltaTime;
                    float Transparency = 1 - (TimeToDeath / StartTimeToDeath);
                    if (Blackness != null) 
                    {
                        Blackness.color = new Color(0, 0, 0, Transparency);
                    }

                    else if (Blackness == null) { Debug.LogError("Blackness is null."); }
                    // Insert sounds for extra effect :D
                    // Fade to black...
                }
                else if (TimeToDeath <= 0)
                {
                    Freeze(); // The player freezes to death.
                }
            }
        }
    }

    public void InWaterCheck()
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
                Wetness = Wetness - Time.deltaTime / 20;
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

        ChangePointer();
    }

    void ChangePointer()
    {
        TemperaturePointer.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 135.5f - BodyTemperature));

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

    void insideCheck() // Checks if the player is covered under cover
    {
        Vector3 up = PlayerCamera.transform.TransformDirection(Vector3.up);

        if (!Physics.Raycast(PlayerCamera.transform.position, up, 10))
        { isCovered = false; }
        else { isCovered = true; }
    }

    public IEnumerator AnimateExposure() // To animate the exposure ring to get bigger then smaller again. This called by the RainTutorial.cs script since it gets disabled right after and thus can't run Coroutines.
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
