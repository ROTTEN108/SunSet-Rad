using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using System.Collections.Generic;


namespace SETTINGSUN_RADIANCE
{
    public class SUNSET_RADIANCE : Mod,IMod, Modding.ILogger
    {
        public override string GetVersion()
        {
            return "0.0.0.9";
        }
        public class Settings
        {
            public bool on = true;
        }
        public class SUNSET : Mod, IGlobalSettings<Settings>, IMenuMod
        {
            public override string GetVersion()
            {
                return "0.0.0.9";
            }
            public SUNSET() : base("SUNSET")
            {
            }
            public static Settings settings_ = new Settings();
            public bool ToggleButtonInsideMenu => true;
            public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
            {
                List<IMenuMod.MenuEntry> menus = new List<IMenuMod.MenuEntry>();
                if (toggleButtonEntry != null)
                {
                    menus.Add(toggleButtonEntry.Value);
                }
                menus.Add(new IMenuMod.MenuEntry
                {
                    Name = this.OtherLanguage("召唤残阳", "Summon SETTING SUN"),
                    Description = this.OtherLanguage("古老的太阳以光之神形体逼近现世。", "The ancient sun approached this world in the form of the God of light."),
                    Values = new string[]
                    {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu")
                    },
                    Loader = (() => (!settings_.on) ? 1 : 0),
                    Saver = delegate (int i)
                    {
                        settings_.on = (i == 0);
                    }
                });

                return menus;
            }
            public void OnLoadGlobal(Settings settings) => settings_ = settings;
            public Settings OnSaveGlobal() => settings_; 
            private string OtherLanguage(string chinese, string english)
            {
                if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                {
                    return chinese;
                }
                return english;
            }
        }
        static System.Random random = new System.Random();
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
        public static double DegreesToRadians(double Degrees)
        {
            return Degrees * (Math.PI / 180);
        }
        public static GameObject BEAM;
        public static GameObject ORBBLASTPT;
        public static GameObject BEAMBLASTPT;
        public static GameObject DREAMPTCHARG;
        public static GameObject DREAMPTCHARG2;
        public static GameObject DREAMPTBLOCK;
        public static GameObject DREAMVER;
        public static GameObject Camera;
        public static AudioClip NAILSHOT;
        public static AudioClip NAILCHARGE;
        public static int HardMode = 0;
        public static double R => random.NextDouble();
        public static float SIGH => (random.Next(0, 2) * 2 - 1);
        public static float RN => (float)(R * SIGH);
        public static float OrbWaitTime = 3.3f;
        public static bool ZH;

        public static void ModHooks_HeroUpdateHook()
        {
            GameObject GC = GameCameras.instance.gameObject;
            GameObject CP = GC.transform.Find("CameraParent").gameObject;
            Camera = CP.transform.Find("tk2dCamera").gameObject;
            if (Camera != null)
            {
                if(Camera.GetComponent<CameraControl>() == null)
                {
                    Camera.AddComponent<SceneSwitchDetector>();
                    Camera.AddComponent<ChangeColor>();
                    Camera.AddComponent<CameraControl>();
                    //Camera.AddComponent<ChangeWait>();
                    ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
                }
            }
        }
        public class SceneSwitchDetector : MonoBehaviour
        {
            public void Repeat()
            {
                GameObject ST = GameObject.Find("GG_Statue_Radiance").gameObject;
                GameObject SW = ST.transform.Find("dream_version_switch").gameObject;
                var LP = SW.transform.Find("lit_pieces").gameObject;
                var haze = LP.transform.Find("haze").gameObject;
                var glow = LP.transform.Find("plinth_glow").gameObject;
                var guy = LP.transform.Find("dream_glowy_guy").gameObject;
                haze.GetComponent<SpriteRenderer>().color = new Color(1f, 0.45f, 0.71f, 1f);
                glow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.67f, 0.71f, 1f);
                guy.GetComponent<SpriteRenderer>().color = new Color(1f, 0.87f, 1f, 1f);
                if (guy.GetComponent<SpriteRenderer>().color != new Color(1f, 0.87f, 1f, 1f) || glow.GetComponent<SpriteRenderer>().color != new Color(1f, 0.67f, 0.71f, 1f) || haze.GetComponent<SpriteRenderer>().color != new Color(1f, 0.45f, 0.71f, 1f))
                {
                    SW.GetComponent<WorkShopWait>().Repeat();
                }
            }
            void OnEnable()
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }
            void OnDisable()
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            }
            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "GG_Radiance")
                {
                    if (Camera.GetComponent<tk2dCamera>().ZoomFactor < 0.9)
                    {
                        Camera.GetComponent<tk2dCamera>().ZoomFactor = 1;
                        if(Camera.GetComponent<ChangeColor>() != null)
                        {
                            Destroy(Camera.GetComponent<ChangeColor>());
                            Destroy(Camera.GetComponent<Away>());
                        }
                    }
                    ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
                }
                else
                {
                    Camera.AddComponent<ChangeColor>();
                    Camera.AddComponent<Away>();
                    Camera.GetComponent<Away>().enabled = true;
                    if (SUNSET.settings_.on == true)
                    {
                        if(Camera != null && Camera.GetComponent<ChangeColor>() != null)
                        {
                            Camera.GetComponent<ChangeColor>().ChangeStart();
                            Camera.GetComponent<CameraControl>().StartControl();
                        }
                        if (DREAMVER.activeSelf == false)
                            HardMode = 0;
                        else
                            HardMode = 1;
                    }
                }

                if (scene.name == "GG_Workshop")
                {
                    GameObject ST = GameObject.Find("GG_Statue_Radiance").gameObject;
                    GameObject SW = ST.transform.Find("dream_version_switch").gameObject;
                    GameObject SP = SW.transform.Find("Statue Pt").gameObject;
                    SW.SetActive(true);
                    if(SW.GetComponent<WorkShopWait>() == null)
                        SW.AddComponent<WorkShopWait>();

                    SW.transform.Find("GG_statue_plinth_dream").gameObject.SetActive(true);
                    SW.transform.Find("GG_statue_plinth_orb_off").gameObject.SetActive(true);
                    SP.GetComponent<ParticleSystem>().startColor = new Color(1, 0.68f, 0.56f, 1);
                    //SP.GetComponent<ParticleSystem>().emissionRate = 5;
                    SP.GetComponent<ParticleSystem>().maxParticles = 5000;
                    SP.GetComponent<ParticleSystem>().startLifetime = 7f;
                    SP.GetComponent<ParticleSystem>().startSize = 2.4f;
                    var toggle = ST.GetComponentInChildren<BossStatueDreamToggle>(true);
                    toggle.SetState(true);
                    Modding.ReflectionHelper.SetField
                    (
                        toggle,
                        "colorFaders",
                        toggle.litPieces.GetComponentsInChildren<ColorFader>(true)
                    );
                    var bs = ST.GetComponent<BossStatue>();
                    toggle.SetOwner(bs);
                    var scene1 = ScriptableObject.CreateInstance<BossScene>();
                    scene1.sceneName = "GG_Radiance";
                    bs.dreamBossScene = scene1;
                    bs.dreamStatueStatePD = "statueStateRadiance";
                    var details = new BossStatue.BossUIDetails();
                    details.nameKey = details.nameSheet = "NAME_FINAL_BOSS";
                    details.descriptionKey =  "GG_S_RADIANCE";
                    details.descriptionSheet = "CP3";
                    bs.dreamBossDetails = details;
                    var LP = SW.transform.Find("lit_pieces").gameObject;
                    var haze = LP.transform.Find("haze").gameObject;
                    var glow = LP.transform.Find("plinth_glow").gameObject;
                    var guy = LP.transform.Find("dream_glowy_guy").gameObject;
                    haze.GetComponent<SpriteRenderer>().color = new Color(1f, 0.45f, 0.71f, 1f);
                    glow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.67f, 0.71f, 1f);
                    guy.GetComponent<SpriteRenderer>().color = new Color(1f, 0.87f, 1f, 1f);
                    DREAMVER = LP;
                    if (guy.GetComponent<SpriteRenderer>().color != new Color(1f, 0.87f, 1f, 1f) || glow.GetComponent<SpriteRenderer>().color != new Color(1f, 0.67f, 0.71f, 1f) || haze.GetComponent<SpriteRenderer>().color != new Color(1f, 0.45f, 0.71f, 1f))
                    {
                        SW.GetComponent<WorkShopWait>().Repeat();
                    }
                }
            }
        }
        public class Away : MonoBehaviour
        {
            bool Stop = false;
            float Timer;
            public void Start()
            {
                Timer = 0f;
                Stop = false;
            }
            public void FixedUpdate()
            {
                if (Stop == false && Timer <= 4)
                {
                    Timer += Time.deltaTime;
                    float Speed = 0f;
                    Speed += (Camera.GetComponent<tk2dCamera>().ZoomFactor - 0.75f) / 20 * Time.deltaTime / 0.02f;
                    Camera.GetComponent<tk2dCamera>().ZoomFactor -= Speed;
                }
                else
                {
                    gameObject.GetComponent<Away>().enabled = false;
                }
            }
        }
        public class CameraControl : MonoBehaviour
        {
            public float Timer = 0f;
            public bool Start = false;
            public void StartControl()
            {
                GameObject GC = GameCameras.instance.gameObject;
                GameObject CP = GC.transform.Find("CameraParent").gameObject;
                Camera = CP.transform.Find("tk2dCamera").gameObject;
                if(Camera.GetComponent<SceneSwitchDetector>() == null)
                {
                    Camera.AddComponent<SceneSwitchDetector>();
                }
            }
        }
        public class ChangeColor : MonoBehaviour
        {
            public static List<GameObject> PILLARS = new List<GameObject>();
            public static List<GameObject> CLOUDS = new List<GameObject>();
            public static GameObject Haze1;
            public static Color Haze1color1 = new Color(0.74f,0.66f,0.56f,1f);
            public static Color Haze1color2 = new Color(0.7f, 0.5f, 0.42f, 1f);
            public static Color Haze1color3 = new Color(0.6f, 0.32f, 0.27f, 1f);
            public static Color Pillarcolor1 = new Color(1f, 0.77f, 0.67f, 1f);
            public static Color Pillarcolor2 = new Color(0.77f, 0.65f, 0.49f, 1f);
            public static Color Pillarcolor3 = new Color(0.57f, 0.34f, 0.24f, 1f);
            public static float BGcolorR = 0.1f;
            public static float BGcolorG = 0.03f;
            public static float BGcolorB = 0.14f;
            public static float CLcolorR = 0.45f;
            public static float CLcolorG = 0.28f;
            public static float CLcolorB = 0.2f;
            public static float STcolorR = 1f;
            public static float STcolorG = 0.78f;
            public static float STcolorB = 0.82f;
            public static float STcolorA = 0.42f;
            public static float Timer = 0;
            public static bool Started1 = false;
            public static bool Started2 = false;
            public static bool Started3 = false;
            public static int UnDone = 0;
            public void ChangeStart()
            {
                Started1 = true;
                Started2 = false;
                Started3 = false;
                if (Started1 == true)
                {
                    UnDone = 0;
                    Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                    if (currentScene.name == "GG_Radiance")
                    {
                        GameObject GG_Arena_Prefab = GameObject.Find("GG_Arena_Prefab");
                        GameObject bg = GG_Arena_Prefab.transform.Find("BG").gameObject;
                        GameObject throne = bg.transform.Find("throne").gameObject;
                        throne.transform.position += new Vector3(3f, 0f, 0f);
                        throne.transform.eulerAngles += new Vector3(0f, 0f, -15f);
                        GameObject PL = GameObject.Find("GG_pillar_top").gameObject;
                        PL.transform.position += new Vector3(1f, 0f, 0f);
                        PL.transform.eulerAngles += new Vector3(0f, 0f, -15f);
                        GameObject GodSeeker = GameObject.Find("Godseeker Crowd").gameObject;
                        GodSeeker.SetActive(false);

                        List<GameObject> OBJECTS = new List<GameObject>();
                        for (int i = 0; i < bg.transform.childCount; i++)
                        {
                            OBJECTS.Add(bg.transform.GetChild(i).gameObject);
                        }
                        foreach (GameObject g in OBJECTS)
                        {
                            if (g.name.Contains("haze"))
                            {
                                SpriteRenderer render = g.GetComponent<SpriteRenderer>();
                                bool succeed = render != null;
                                if (succeed)
                                {
                                    g.GetComponent<SpriteRenderer>().color = new Color(BGcolorR, BGcolorG, BGcolorB, g.GetComponent<SpriteRenderer>().color.a);
                                    if (g.name.Contains("haze2 (1)"))
                                    {
                                        g.GetComponent<SpriteRenderer>().color = Haze1color1;
                                        Haze1 = g;
                                    }
                                    if (g.name.Contains("haze2 (5)"))
                                        g.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.8f, 1f, 1f);
                                }
                                else
                                    UnDone++;
                            }
                            if (g.name.Contains("GG_scenery_0004_17"))
                            {
                                SpriteRenderer render = g.GetComponent<SpriteRenderer>();
                                bool succeed = render != null;
                                if (succeed)
                                {
                                    g.GetComponent<SpriteRenderer>().color = new Color(CLcolorR, CLcolorG, CLcolorB, g.GetComponent<SpriteRenderer>().color.a);
                                    CLOUDS.Add(g);
                                }
                                else
                                    UnDone++;
                            }
                            if (g.name.Contains("ray"))
                            {
                                for (int i = 0; i < g.transform.childCount; i++)
                                {
                                    if (g.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>() != null)
                                    {
                                        g.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = new Color(STcolorR, STcolorG, STcolorB, STcolorA);
                                    }
                                }
                            }
                            if (g.name.Contains("pillar"))
                            {
                                for (int i = 0; i < g.transform.childCount; i++)
                                {
                                    if (g.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>() != null)
                                    {
                                        g.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Pillarcolor1;
                                        PILLARS.Add(g.transform.GetChild(i).gameObject);
                                    }
                                }
                            }
                        }/*
                        if (UnDone >= 1)
                        {
                            Camera.GetComponent<ChangeWait>().Repeat();
                        }
                        */
                    }
                    
                }
            }
            public void Change2()
            {
                Started2 = true;
                Timer = 0;
            }
            public void Change3()
            {
                Started3 = true;
                Timer = 0;
            }
            public void FixedUpdate()
            {
                if (Started2 == true)
                {
                    if (Timer <= 5)
                    {
                        Timer += Time.deltaTime;
                        Haze1.GetComponent<SpriteRenderer>().color += (Haze1color2 - Haze1.GetComponent<SpriteRenderer>().color) / 25 * Time.deltaTime / 0.02f;
                        foreach (var p in PILLARS)
                        {
                            p.GetComponent<SpriteRenderer>().color += (Pillarcolor2 - p.GetComponent<SpriteRenderer>().color) / 25 * Time.deltaTime / 0.02f;
                        }
                    }
                }
                if (Started3 == true)
                {
                    if (Timer <= 6)
                    {
                        Timer += Time.deltaTime;
                        Haze1.GetComponent<SpriteRenderer>().color += (Haze1color3 - Haze1.GetComponent<SpriteRenderer>().color) / 30 * Time.deltaTime / 0.02f;
                        foreach (var p in PILLARS)
                        {
                            p.GetComponent<SpriteRenderer>().color += (Pillarcolor3 - p.GetComponent<SpriteRenderer>().color) / 25 * Time.deltaTime / 0.02f;
                        }
                    }
                }
            }
        }
        public class WorkShopWait : MonoBehaviour
        {
            public float Timer;
            public void Repeat()
            {
                Timer = 0f;
            }
            public void Update()
            {
                if (Timer < 0.2f)
                    Timer += Time.deltaTime;
                else
                    Camera.GetComponent<SceneSwitchDetector>().Repeat();
            }
        }
        public class ChangeWait : MonoBehaviour
        {
            public float Timer;
            public void Repeat()
            {
                Timer = 0f;
            }
            public void Update()
            {
                if (Timer < 0.2f)
                    Timer += Time.deltaTime;
                else
                    Camera.GetComponent<ChangeColor>().ChangeStart();
            }
        }
        public class PtRecycle : MonoBehaviour
        {
            public float Timer = 6f;
            public void FixedUpdate()
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                    gameObject.Recycle();
            }
        }
        public class HaloExpand : MonoBehaviour
        {
            public float Timer = 0f;
            public float TimerLimit = 0f;
            public Vector3 DefaultHaloScale;
            private Vector3 HaloScale = new Vector3(0, 0, 0);
            private Vector3 HaloScaleMax = new Vector3(0.03f, 0.03f, 0);
            private bool HaloExpandInit = false;
            private void Start()
            {
                DefaultHaloScale = gameObject.transform.Find("Halo").gameObject.transform.localScale;
                HaloScaleMax = DefaultHaloScale + new Vector3(1, 1, 0);
            }
            public void HaloExpandStart()
            {
                HaloExpandInit = true;
            }
            public void HaloExpandEnd()
            {
                HaloExpandInit = false;
                Timer = 3f;
                TimerLimit = 3f;
            }
            public void FixedUpdate()
            {
                Timer -= Time.deltaTime;
                if (HaloExpandInit == true && Timer <= TimerLimit)
                {
                    TimerLimit -= 0.02f;
                    //HaloScaleAdd += (HaloScaleAddMax - HaloScaleAdd) / 12;
                    gameObject.transform.Find("Halo").gameObject.transform.localScale += (HaloScaleMax - gameObject.transform.Find("Halo").gameObject.transform.localScale) / 12;
                }
                if (HaloExpandInit == false && Timer >= 0f && Timer <= TimerLimit)
                {
                    TimerLimit -= 0.02f;
                    //HaloScaleAdd += (DefaultHaloScale - HaloScaleAdd / 7);
                    gameObject.transform.Find("Halo").gameObject.transform.localScale += (DefaultHaloScale - gameObject.transform.Find("Halo").gameObject.transform.localScale) / 7;
                }
            }
        }
        public class HaloRotate : MonoBehaviour
        {
            public float Timer = 0f;
            public float TimerLimit = 0f;
            public float DefaultHaloColorA;
            private float HaloRotateAdd = 0;
            private float HaloRotateSpeedMax = 0.8f;
            private bool HaloSpinInit = false;
            private void Start()
            {
                DefaultHaloColorA = gameObject.transform.Find("Halo").gameObject.GetComponent<SpriteRenderer>().color.a;
            }
            public void HaloSpinStart()
            {
                HaloSpinInit = true;
            }
            public void HaloSpinEnd()
            {
                HaloSpinInit = false;
                Timer = 3f;
                TimerLimit = 3f;
            }
            public void FixedUpdate()
            {
                Timer -= Time.deltaTime;
                if (HaloSpinInit == true && Timer <= TimerLimit)
                {
                    TimerLimit -= 0.02f;
                    gameObject.transform.Find("Halo").gameObject.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, (1 - gameObject.transform.Find("Halo").gameObject.GetComponent<SpriteRenderer>().color.a) / 15);
                    HaloRotateAdd += (HaloRotateSpeedMax - HaloRotateAdd) / 15;
                    gameObject.transform.Find("Halo").gameObject.transform.Rotate(0, 0, HaloRotateAdd);
                }
                if (HaloSpinInit == false && Timer >= 0f && Timer <= TimerLimit)
                {
                    TimerLimit -= 0.02f;
                    HaloRotateAdd += (0f - HaloRotateAdd / 7);
                    gameObject.transform.Find("Halo").gameObject.transform.Rotate(0, 0, HaloRotateAdd);
                    gameObject.transform.Find("Halo").gameObject.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, (DefaultHaloColorA - gameObject.transform.Find("Halo").gameObject.GetComponent<SpriteRenderer>().color.a) / 10);
                }
            }
        }
        public class AddGlowEffectBlack : MonoBehaviour
        {
            public Color color = new Color(1, 1, 1, 1);
            private void Update()
            {
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount",-1);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
            }
        }
        public class AddGlowEffect : MonoBehaviour
        {
            public Color color = new Color(1, 1, 1, 1);
            private void Update()
            {
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount", 1);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
            }
        }
        public class OrbWhite : MonoBehaviour
        {
            private void Update()
            {
                var color = new Color(0.83f, 0.72f, 0f, 1f);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount", -1);
            }
        }
        public class BeamWhite : MonoBehaviour
        {
            private void Update()
            {
                //var color = new Color(1f, 0.9152f, 0.52f, 1f);
                var color = new Color(1f, 1f, 1f, 1f);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount", 1);
            }
        }
        public class BeamBlack : MonoBehaviour
        {
            private void Update()
            {
                var color = new Color(0, 0, 0, 1);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount", 1);
            }
        }
        public class NailBlack : MonoBehaviour
        {
            public Color color = new Color(0.01f, 0.01f, 0.01f, 1);
            private void Update()
            {
                gameObject.GetComponent<MeshRenderer>().material.SetInt("_FlashAmount", 1);
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_FlashColor", color);
            }
        }
        public class NailBlock : MonoBehaviour
        {
            public bool Blocked = false;
            public void OnTriggerEnter2D(Collider2D collision)
            {
                var nail = gameObject;
                var receiver = collision.gameObject;
                if (receiver.gameObject.name == "Slash"|| receiver.gameObject.name == "AltSlash"|| receiver.gameObject.name == "UpSlash"|| receiver.gameObject.name == "DownSlash"|| receiver.gameObject.name == "Hit L"|| receiver.gameObject.name == "Hit R"|| receiver.gameObject.name == "Great Slash")
                {
                    float angle = BossBehavior.RX * 15f;
                    gameObject.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value += 180f + angle;
                    gameObject.transform.Rotate(0, 0, 180f + angle);
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = Math.Sign(gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value) * 120f;
                    var Pt1 = GameObject.Instantiate(DREAMPTBLOCK, gameObject.transform.position, Quaternion.Euler(0, 0, gameObject.transform.eulerAngles.z + 90));
                    Pt1.gameObject.SetActive(true);
                    Pt1.GetComponent<ParticleSystem>().Play();
                    Pt1.AddComponent<PtRecycle>();
                    var Pt2 = GameObject.Instantiate(DREAMPTBLOCK, gameObject.transform.position, Quaternion.Euler(0, 0, gameObject.transform.eulerAngles.z - 90));
                    Pt2.gameObject.SetActive(true);
                    Pt2.GetComponent<ParticleSystem>().Play();
                    Pt2.AddComponent<PtRecycle>();
                }
            }
        }
        private class CombL : MonoBehaviour
        {
            private List<GameObject> Nails = new List<GameObject>();
            public int RN1 => random.Next(1, 5);
            public int RN2 => random.Next(5, 8);
            public int RN3 => random.Next(1, 10);
            public float CombTime;
            public float CombTimeLimit;
            private float X = 41.5f;
            private float Y = 21f;
            private bool Summoned1 = false;
            private int NailCount = 1;
            public void Tri()
            {
                if (CombTime <= 0)
                {
                    CombTime = 1.5f;
                    CombTimeLimit = 1.5f;
                }
            }
            public void Reset()
            {
                CombTime = 0;
                CombTimeLimit = 0;
                Summoned1 = false;
                NailCount = 1;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            Y = 21f;
                            while (NailCount < 10)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, -90f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 18f - NailCount * 1.5f;
                                    nail.gameObject.AddComponent<AddGlowEffect>();
                                    Nails.Add(nail);
                                }
                                Y += 2f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
                else
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            Y = 21f;
                            while (NailCount < 15)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, -90f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 21f - NailCount * 1.3f;
                                    nail.gameObject.AddComponent<AddGlowEffect>();
                                    Nails.Add(nail);
                                }
                                Y += 1.6f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
            }
        }
        private class CombR : MonoBehaviour
        {

            private List<GameObject> Nails = new List<GameObject>();
            public int RN1 => random.Next(1, 5);
            public int RN2 => random.Next(5, 8);
            public int RN3 => random.Next(1, 10);
            public float CombTime;
            public float CombTimeLimit;
            private float X = 79f;
            private float Y = 21f;
            private bool Summoned1 = false;
            private int NailCount = 1;
            public void Tri()
            {
                if (CombTime <= 0)
                {
                    CombTime = 1.5f;
                    CombTimeLimit = 1.5f;
                }
            }
            public void Reset()
            {
                CombTime = 0;
                CombTimeLimit = 0;
                Summoned1 = false;
                NailCount = 1;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            Y = 21f;
                            while (NailCount < 10)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 90f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 18f - NailCount * 1.5f;
                                    nail.gameObject.AddComponent<AddGlowEffect>();
                                    Nails.Add(nail);
                                }
                                Y += 2f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
                else
                {

                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            Y = 21f;
                            while (NailCount < 15)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 90f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 21f - NailCount * 1.3f;
                                    nail.gameObject.AddComponent<AddGlowEffect>();
                                    Nails.Add(nail);
                                }
                                Y += 1.6f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
            }
        }
        private class CombU : MonoBehaviour
        {

            private List<GameObject> Nails = new List<GameObject>();
            public int RN1 => random.Next(1, 5);
            public int RN2 => random.Next(5, 10);
            public int RN3 => random.Next(10, 15);
            public float CombTime;
            public float CombTimeLimit;
            private float X = -15f;
            private float Y = 10.5f;
            private bool Summoned1 = false;
            private int NailCount = 1;
            public void Tri()
            {
                if (CombTime <= 0)
                {
                    CombTime = 1.5f;
                    CombTimeLimit = 1.5f;
                }
            }
            public void Reset()
            {
                CombTime = 0;
                CombTimeLimit = 0;
                Summoned1 = false;
                NailCount = 1;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            X = -15;
                            while (NailCount < 10)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL,HeroController.instance.gameObject.transform.position + new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 180f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 10f - BossBehavior.RX * 4.5f;
                                    nail.gameObject.AddComponent<NailBlack>();
                                    Nails.Add(nail);
                                }
                                X += 3f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
                else
                {

                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            X = -15;
                            while (NailCount < 15)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, HeroController.instance.gameObject.transform.position + new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 180f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 12f - BossBehavior.RX * 4.5f;
                                    nail.gameObject.AddComponent<NailBlack>();
                                    Nails.Add(nail);
                                }
                                X += 2f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
            }
        }
        private class CombD : MonoBehaviour
        {

            private List<GameObject> Nails = new List<GameObject>();
            public int RN1 => random.Next(1, 5);
            public int RN2 => random.Next(5, 10);
            public int RN3 => random.Next(10, 15);
            public float CombTime;
            public float CombTimeLimit;
            private float X = -15f;
            private float Y = -9f;
            private bool Summoned1 = false;
            private int NailCount = 1;
            public void Tri()
            {
                if (CombTime <= 0)
                {
                    CombTime = 1.5f;
                    CombTimeLimit = 1.5f;
                }
            }
            public void Reset()
            {
                CombTime = 0;
                CombTimeLimit = 0;
                Summoned1 = false;
                NailCount = 1;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            X = -15;
                            while (NailCount < 10)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL,HeroController.instance.gameObject.transform.position + new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 0f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 9f - BossBehavior.RX * 3.5f;
                                    nail.gameObject.AddComponent<NailBlack>();
                                    Nails.Add(nail);
                                }
                                X += 3f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
                else
                {
                    if (CombTime > 0)
                    {
                        CombTime -= Time.deltaTime;
                        if (Summoned1 == false)
                        {
                            Summoned1 = true;
                            NailCount = 1;
                            X = -15;
                            while (NailCount < 15)
                            {
                                NailCount++;
                                if (NailCount != RN1 && NailCount != RN2 && NailCount != RN3)
                                {
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, HeroController.instance.gameObject.transform.position + new Vector3(X, Y, 0), Quaternion.Euler(0f, 0f, 0f));
                                    nail.SetActive(true);
                                    nail.gameObject.AddComponent<NailBlock>();
                                    nail.gameObject.AddComponent<NailSpeedUp>();
                                    nail.gameObject.GetComponent<NailSpeedUp>().NailSpeed = 11f - BossBehavior.RX * 3.5f;
                                    nail.gameObject.AddComponent<AddGlowEffect>();
                                    Nails.Add(nail);
                                }
                                X += 2f;
                            }
                        }
                    }
                    else
                    {
                        Summoned1 = false;
                        Nails.Clear();
                    }
                }
            }
        }
        public class OrbFade : MonoBehaviour
        {
            private float OrbTime = 0.8f;
            public void Start()
            {
                OrbTime = 0.8f;
                gameObject.SetActive(true);
                gameObject.SetActiveChildren(true);
            }
            public void Update()
            {
                OrbTime -= Time.deltaTime;
                if (OrbTime <= 0.5)
                {
                    gameObject.LocateMyFSM("Orb Control").SetState("Recycle");
                }
                if (OrbTime <= 0)
                {
                    gameObject.Recycle();
                }
            }
        }
        public class OrbImpact : MonoBehaviour
        {
            private float OrbTime = 2f;
            private bool Impacted = false;
            public void Start()
            {
                OrbTime = 0.8f;
                gameObject.SetActive(true);
                gameObject.SetActiveChildren(true);
            }
            public void FixedUpdate()
            {
                OrbTime -= Time.deltaTime;
                if (OrbTime <= 2 && Impacted == false)
                {
                    gameObject.LocateMyFSM("Orb Control").SetState("Impact");
                    Impacted = true;
                }
                if (OrbTime <= 0)
                {
                    gameObject.Recycle();
                }
            }
        }
        public class Meteorolite : MonoBehaviour
        {
            double RX01 => random.NextDouble();
            double RY01 => random.NextDouble();
            public float SIGHX1 => (random.Next(0, 2) * 2 - 1);
            public float SIGHY1 => (random.Next(0, 2) * 2 - 1);
            public float RX1 => (float)(RX01 * SIGHX1);
            public float RY1 => (float)(RY01 * SIGHY1);
            double RX02 => random.NextDouble();
            double RY02 => random.NextDouble();
            public float SIGHX2 => (random.Next(0, 2) * 2 - 1);
            public float SIGHY2 => (random.Next(0, 2) * 2 - 1);
            public float RX2 => (float)(RX02 * SIGHX2);
            public float RY2 => (float)(RY02 * SIGHY2);
            double RX03 => random.NextDouble();
            double RY03 => random.NextDouble();
            public float SIGHX3 => (random.Next(0, 2) * 2 - 1);
            public float SIGHY3 => (random.Next(0, 2) * 2 - 1);
            public float RX3 => (float)(RX03 * SIGHX3);
            public float RY3 => (float)(RY03 * SIGHY3);
            public Vector3 MeteorolitePosition1;
            public Vector3 MeteorolitePosition2;
            public Vector3 MeteorolitePosition3;
            public float MeteoroliteTime = 0f;
            public float MeteoroliteTimeLimit = 0f;
            public float MeteoroliteTimeLimit1 = 0f;
            public float MeteoroliteSpeed1 = 0f;
            public float MeteoroliteSpeed2 = 0f;
            public float MeteoroliteSpeed3 = 0f;
            public bool MeteoroliteSummoned1 = false;
            public bool MeteoroliteSummoned2 = false;
            public bool MeteoroliteSummoned3 = false;
            public void Tri()
            {
                if (MeteoroliteTime <= 0)
                {
                    MeteoroliteTime = 6f;
                    MeteoroliteTimeLimit = 6f;
                    MeteoroliteTimeLimit1 = 6f;
                }
            }
            public void Reset()
            {
                MeteoroliteTime = 0f;
                MeteoroliteTimeLimit = 0f;
                MeteoroliteTimeLimit1 = 0f;
                MeteoroliteSummoned1 = false;
                MeteoroliteSummoned2 = false;
                MeteoroliteSummoned3 = false;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (MeteoroliteTime > 0f)
                    {
                        if (MeteoroliteTime < 6f && MeteoroliteSummoned1 == false)
                        {
                            MeteoroliteSpeed1 = 6f;
                            MeteoroliteSummoned1 = true;
                            MeteorolitePosition1 = HeroController.instance.gameObject.transform.position + new Vector3(RX1 * 15f, 13f, 0f);
                        }
                        if (MeteoroliteTime < 5.5f && MeteoroliteSummoned2 == false)
                        {
                            MeteoroliteSpeed2 = 6f;
                            MeteoroliteSummoned2 = true;
                            MeteorolitePosition2 = HeroController.instance.gameObject.transform.position + new Vector3(RX2 * 15f, 13f, 0f);
                        }
                        if (MeteoroliteTime < 5f && MeteoroliteSummoned3 == false)
                        {
                            MeteoroliteSpeed3 = 6f;
                            MeteoroliteSummoned3 = true;
                            MeteorolitePosition3 = HeroController.instance.gameObject.transform.position + new Vector3(RX3 * 15f, 13f, 0f);
                        }
                        MeteoroliteTime -= Time.deltaTime;
                        if (MeteoroliteTime <= MeteoroliteTimeLimit)
                        {
                            MeteoroliteTimeLimit -= 0.02f;
                            MeteorolitePosition1 += new Vector3(0f, -MeteoroliteSpeed1 / 40f, 0f);
                            MeteorolitePosition2 += new Vector3(0f, -MeteoroliteSpeed2 / 40f, 0f);
                            MeteorolitePosition3 += new Vector3(0f, -MeteoroliteSpeed3 / 40f, 0f);

                            if (MeteoroliteTime <= MeteoroliteTimeLimit1)
                            {
                                MeteoroliteTimeLimit1 -= 0.24f;
                                if (MeteoroliteTime < 6f)
                                {
                                    var orb1 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition1 + new Vector3(RX1 * 0.5f, RY1 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb1.gameObject.AddComponent<OrbFade>();
                                    orb1.gameObject.AddComponent<AddGlowEffect>();
                                }
                                if (MeteoroliteTime < 5.5f)
                                {
                                    var orb2 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition2 + new Vector3(RX2 * 0.5f, RY2 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb2.gameObject.AddComponent<OrbFade>();
                                    orb2.gameObject.AddComponent<AddGlowEffect>();
                                }
                                if (MeteoroliteTime < 5f)
                                {
                                    var orb3 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition3 + new Vector3(RX3 * 0.5f, RY3 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb3.gameObject.AddComponent<OrbFade>();
                                    orb3.gameObject.AddComponent<AddGlowEffect>();
                                }
                            }
                        }
                    }
                    else
                    {
                        MeteoroliteSummoned1 = false;
                        MeteoroliteSummoned2 = false;
                        MeteoroliteSummoned3 = false;
                    }
                }
                else
                {
                    if (MeteoroliteTime > 0f)
                    {
                        if (MeteoroliteTime < 6f && MeteoroliteSummoned1 == false)
                        {
                            MeteoroliteSpeed1 = 6f;
                            MeteoroliteSummoned1 = true;
                            MeteorolitePosition1 = HeroController.instance.gameObject.transform.position + new Vector3(RX1 * 15f, 13f, 0f);
                        }
                        if (MeteoroliteTime < 5.5f && MeteoroliteSummoned2 == false)
                        {
                            MeteoroliteSpeed2 = 6f;
                            MeteoroliteSummoned2 = true;
                            MeteorolitePosition2 = HeroController.instance.gameObject.transform.position + new Vector3(RX2 * 15f, 13f, 0f);
                        }
                        if (MeteoroliteTime < 5f && MeteoroliteSummoned3 == false)
                        {
                            MeteoroliteSpeed3 = 6f;
                            MeteoroliteSummoned3 = true;
                            MeteorolitePosition3 = HeroController.instance.gameObject.transform.position + new Vector3(RX3 * 15f, 13f, 0f);
                        }
                        MeteoroliteTime -= Time.deltaTime;
                        if (MeteoroliteTime <= MeteoroliteTimeLimit)
                        {
                            MeteoroliteTimeLimit -= 0.02f;
                            MeteorolitePosition1 += new Vector3(0f, -MeteoroliteSpeed1 / 30f, 0f);
                            MeteorolitePosition2 += new Vector3(0f, -MeteoroliteSpeed2 / 30f, 0f);
                            MeteorolitePosition3 += new Vector3(0f, -MeteoroliteSpeed3 / 30f, 0f);

                            if (MeteoroliteTime <= MeteoroliteTimeLimit1)
                            {
                                MeteoroliteTimeLimit1 -= 0.2f;
                                if (MeteoroliteTime < 6f)
                                {
                                    var orb1 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition1 + new Vector3(RX1 * 0.5f, RY1 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb1.gameObject.AddComponent<OrbFade>();
                                    orb1.gameObject.AddComponent<AddGlowEffect>();
                                }
                                if (MeteoroliteTime < 5.5f)
                                {
                                    var orb2 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition2 + new Vector3(RX2 * 0.5f, RY2 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb2.gameObject.AddComponent<OrbFade>();
                                    orb2.gameObject.AddComponent<AddGlowEffect>();
                                }
                                if (MeteoroliteTime < 5f)
                                {
                                    var orb3 = GameObject.Instantiate(BossBehavior.ORB, MeteorolitePosition3 + new Vector3(RX3 * 0.5f, RY3 * 0.5f, 0f), new Quaternion(0, 0, 0, 0));
                                    orb3.gameObject.AddComponent<OrbFade>();
                                    orb3.gameObject.AddComponent<AddGlowEffect>();
                                }
                            }
                        }
                    }
                    else
                    {
                        MeteoroliteSummoned1 = false;
                        MeteoroliteSummoned2 = false;
                        MeteoroliteSummoned3 = false;
                    }
                }
            }
        }
        public class TriangleBeam : MonoBehaviour
        {
            public List<GameObject> Beams = new List<GameObject>();
            public List<GameObject> Beams2 = new List<GameObject>();
            public List<GameObject> Beams3 = new List<GameObject>();
            public float TriangleBeamTime = 0f;
            public float TriangleBeamTimeLimit = 0f;
            public float BeamAngle = 0f;
            public float Angle = 2f;
            public float BeamSigh = 0f;
            public float X = 0f;
            public float Y = 0f;
            public bool BeamsSummoned = false;
            public bool BeamsFired = false;
            public int Count;
            public void Tri()
            {
                if (TriangleBeamTime <= 0)
                {
                    TriangleBeamTime = 3f;
                    TriangleBeamTimeLimit = 3f;
                }
            }
            public void Reset()
            {
                TriangleBeamTime = 0f;
                TriangleBeamTimeLimit = 0f;
                BeamAngle = 0f;
                Angle = 2f;
                BeamSigh = 0f;
                BeamsSummoned = false;
                BeamsFired = false;
                Count = 0;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (TriangleBeamTime > 0)
                    {
                        TriangleBeamTime -= Time.deltaTime;
                        if (TriangleBeamTime <= TriangleBeamTimeLimit)
                        {
                            TriangleBeamTimeLimit -= 0.05f;
                            if (BeamsSummoned == false)
                            {
                                BeamsSummoned = true;
                                X = BossBehavior.BOSS.transform.position.x;
                                Y = BossBehavior.BOSS.transform.position.y + 1f;
                                BeamsFired = false;
                                Count = 0;
                                BeamAngle = BossBehavior.Angle - 90f;
                                BeamSigh = BossBehavior.SIGHX;
                                Angle = 2f;
                            }
                            if (Count < 30)
                            {
                                Count++;
                                var beam1 = GameObject.Instantiate(BEAM, new Vector3(X, Y, 0), Quaternion.Euler(0, 0, BeamAngle + 90));
                                var beam2 = GameObject.Instantiate(BEAM, new Vector3(X, Y, 0), Quaternion.Euler(0, 0, BeamAngle + 270));
                                beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.2f, beam1.transform.localScale.z);
                                beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.2f, beam2.transform.localScale.z);
                                beam1.transform.localPosition = beam1.transform.localPosition + new Vector3((float)Math.Cos(DegreesToRadians(BeamAngle)) * 7f, (float)Math.Sin(DegreesToRadians(BeamAngle)) * 7f, 0);
                                beam2.transform.localPosition = beam2.transform.localPosition + new Vector3((float)Math.Cos(DegreesToRadians(BeamAngle)) * 7f, (float)Math.Sin(DegreesToRadians(BeamAngle)) * 7f, 0);
                                beam1.SetActive(true);
                                beam1.SetActiveChildren(true);
                                beam1.LocateMyFSM("Control").SetState("Antic");
                                beam1.LocateMyFSM("Control").SendEvent("ANTIC");
                                beam2.SetActive(true);
                                beam2.SetActiveChildren(true);
                                beam2.LocateMyFSM("Control").SetState("Antic");
                                beam2.LocateMyFSM("Control").SendEvent("ANTIC");
                                Beams.Add(beam1);
                                Beams.Add(beam2); 
                                foreach (var beam in Beams)
                                {
                                    beam.AddComponent<PtRecycle>();
                                    beam.gameObject.AddComponent<BeamWhite>();
                                }
                                BeamAngle += 12f * BeamSigh;
                            }
                            if (TriangleBeamTime <= 0.3f && BeamsFired == false)
                            {
                                BeamsFired = true;
                                AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.4f);
                                foreach (var beam in Beams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("FIRE");
                                }
                            }
                            if (TriangleBeamTime <= 0.15f)
                            {
                                foreach (var beam in Beams)
                                {
                                    beam.LocateMyFSM("Control").SetState("End");
                                    beam.LocateMyFSM("Control").SendEvent("END");
                                }
                                Beams.Clear();
                            }
                        }
                    }
                    else
                        BeamsSummoned = false;
                }
                else
                {
                    if (TriangleBeamTime > 0)
                    {
                        TriangleBeamTime -= Time.deltaTime;
                        if (TriangleBeamTime <= TriangleBeamTimeLimit)
                        {
                            TriangleBeamTimeLimit -= 0.02f;
                            if (BeamsSummoned == false)
                            {
                                BeamsSummoned = true;
                                X = BossBehavior.BOSS.transform.position.x;
                                Y = BossBehavior.BOSS.transform.position.y + 1f;
                                BeamsFired = false;
                                Count = 0;
                                BeamAngle = BossBehavior.Angle - 90f;
                                BeamSigh = BossBehavior.SIGHX;
                                Angle = 2f;
                            }
                            if (Count < 36)
                            {
                                Count++;
                                var beam1 = GameObject.Instantiate(BEAM, new Vector3(X, Y, 0), Quaternion.Euler(0, 0, BeamAngle + 90));
                                var beam2 = GameObject.Instantiate(BEAM, new Vector3(X, Y, 0), Quaternion.Euler(0, 0, BeamAngle + 270));
                                beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.2f, beam1.transform.localScale.z);
                                beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.2f, beam2.transform.localScale.z);
                                beam1.transform.localPosition = beam1.transform.localPosition + new Vector3((float)Math.Cos(DegreesToRadians(BeamAngle)) * 7f, (float)Math.Sin(DegreesToRadians(BeamAngle)) * 7f, 0);
                                beam2.transform.localPosition = beam2.transform.localPosition + new Vector3((float)Math.Cos(DegreesToRadians(BeamAngle)) * 7f, (float)Math.Sin(DegreesToRadians(BeamAngle)) * 7f, 0);
                                beam1.SetActive(true);
                                beam1.SetActiveChildren(true);
                                beam1.LocateMyFSM("Control").SetState("Antic");
                                beam1.LocateMyFSM("Control").SendEvent("ANTIC");
                                beam2.SetActive(true);
                                beam2.SetActiveChildren(true);
                                beam2.LocateMyFSM("Control").SetState("Antic");
                                beam2.LocateMyFSM("Control").SendEvent("ANTIC");
                                Beams.Add(beam1);
                                Beams.Add(beam2);
                                foreach (var beam in Beams)
                                {
                                    beam.gameObject.AddComponent<BeamWhite>();
                                    beam.gameObject.AddComponent<PtRecycle>();
                                }
                                BeamAngle += 10f * BeamSigh;
                            }
                            if (TriangleBeamTime <= 0.8f && BeamsFired == false)
                            {
                                BeamsFired = true;
                                AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.4f);
                                foreach (var beam in Beams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("FIRE");
                                }
                            }
                            if (TriangleBeamTime <= 0.4f)
                            {
                                foreach (var beam in Beams)
                                {
                                    beam.LocateMyFSM("Control").SetState("End");
                                    beam.LocateMyFSM("Control").SendEvent("END");
                                }
                                Beams.Clear();
                            }
                        }
                    }
                    else
                        BeamsSummoned = false;
                }
            }
        }
        public class OrbBlastPt : MonoBehaviour
        {
            public List<GameObject> Pts = new List<GameObject>();
            public float OrbBlastPtTime = 0;
            public float OrbBlastPtTimeLimit = 0;
            public bool OrbBlastPtSummoned = false;
            public void Tri()
            {
                OrbBlastPtTime = 3.8f;
                OrbBlastPtTimeLimit = 3.8f;
                OrbBlastPtSummoned = false;
            }
            public void Reset()
            {
                OrbBlastPtTime = 0;
                OrbBlastPtTimeLimit = 0;
                OrbBlastPtSummoned = false;
            }
            public void Start()
            {
                Pts.Clear();
                var OrbBlastPt1 = GameObject.Instantiate(ORBBLASTPT, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
                OrbBlastPt1.SetActive(true);
                OrbBlastPt1.GetComponent<ParticleSystem>().emissionRate = 0f;
                Pts.Add(OrbBlastPt1);
                OrbBlastPtSummoned = false;
            }
            public void FixedUpdate()
            {
                if (OrbBlastPtTime > 0)
                {
                    OrbBlastPtTime -= Time.deltaTime;
                    if (OrbBlastPtSummoned == false)
                    {
                        OrbBlastPtSummoned = true;
                        foreach (var Pt in Pts)
                        {
                            Pt.transform.position = new Vector3(BossBehavior.BOSS.GetComponent<OrbBlast>().SrikePointX, BossBehavior.BOSS.GetComponent<OrbBlast>().SrikePointY, 0);
                        }
                    }
                    if (OrbBlastPtTime <= OrbBlastPtTimeLimit)
                    {
                        OrbBlastPtTimeLimit -= 0.02f;
                        foreach (var Pt in Pts)
                        {
                            Pt.GetComponent<ParticleSystem>().emissionRate += 1.5f;
                            Pt.GetComponent<ParticleSystem>().Play();
                        }
                    }
                }
                else
                {
                    foreach (var Pt in Pts)
                    {
                        Pt.GetComponent<ParticleSystem>().emissionRate = 0f;
                    }
                }
            }
        }
        public class FinalNailSniperSummon : MonoBehaviour
        {
            public List<GameObject> nail1 = new List<GameObject>();
            public List<GameObject> nail2 = new List<GameObject>();
            public List<GameObject> nail3 = new List<GameObject>();
            public float MoreNailSniperTime = -0.1f;
            public float MoreNailSniperTimeLimit = -0.1f;
            private float X1;
            private float Y1;
            public bool NailSummoned1 = false;
            public int NailCount;
            public int NailCounting;
            public void Reset()
            {
                MoreNailSniperTime = -0.1f;
                MoreNailSniperTimeLimit = -0.1f;
                NailSummoned1 = false;
                NailCount = 0;
                NailCounting = 0;
            }
            public void Start()
            {
                NailSummoned1 = false;
                NailCount = 1;
                X1 = 44.5f;
                Y1 = 168f;
                NailCounting = 0;
                BossBehavior.BOSS.LocateMyFSM("Control").GetState("Final Impact").AddMethod(() =>
                {
                    foreach (var nail in nail1)
                        nail.LocateMyFSM("Control").SetState("Recycle");
                    string nameToFind1 = "Radiant Nail(Clone)";
                    GameObject[] nails = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == nameToFind1).ToArray();
                    foreach(var n in nails)
                    {
                        n.Recycle();
                    }
                    string nameToFind2 = "Radiant Beam(Clone)";
                    GameObject[] beams = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == nameToFind2).ToArray();
                    foreach(var b in beams)
                    {
                        b.LocateMyFSM("Control").SetState("Recycle");
                    }
                });
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (MoreNailSniperTime <= 0)
                    {
                        NailSummoned1 = false;
                        MoreNailSniperTime = 2.6f;
                        MoreNailSniperTimeLimit = 2.6f;

                        if (nail1.Count != 0)
                        {
                            int randomnub = random.Next(nail1.Count);

                            var randomnail = nail1[randomnub];
                            randomnail.gameObject.AddComponent<NailSniper>();
                            randomnail.gameObject.AddComponent<NailBlock>();
                            if (randomnail.transform.position.x - HeroController.instance.gameObject.transform.position.x < 0)
                            {
                                randomnail.gameObject.GetComponent<NailSniper>().StrikeX = (float)Math.Cos(Math.PI + Math.Atan((randomnail.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                                randomnail.gameObject.GetComponent<NailSniper>().StrikeY = (float)Math.Sin(Math.PI + Math.Atan((randomnail.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                            }
                            else
                            {
                                randomnail.gameObject.GetComponent<NailSniper>().StrikeX = (float)Math.Cos(Math.Atan((randomnail.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                                randomnail.gameObject.GetComponent<NailSniper>().StrikeY = (float)Math.Sin(Math.Atan((randomnail.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                            }
                            NailCounting++;
                            nail1.Remove(randomnail);
                        }
                    }
                    if (MoreNailSniperTime > 0)
                    {
                        MoreNailSniperTime -= Time.deltaTime;
                        if (NailSummoned1 == false)
                        {
                            NailSummoned1 = true;
                            while (NailCount <= 20)
                            {
                                NailCount++;
                                X1 += 1.85f;
                                var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X1, Y1, 0), Quaternion.Euler(0, 0, 180));
                                nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                                nail.SetActive(true);
                                nail.gameObject.AddComponent<AddGlowEffect>();
                                nail1.Add(nail);
                            }
                        }
                    }
                }
                else
                {
                    if (MoreNailSniperTime <= 0)
                    {
                        NailSummoned1 = false;
                        MoreNailSniperTime = 1.3f;
                        MoreNailSniperTimeLimit = 1.3f;
                        if (nail1.Count != 0)
                        {
                            int randomnub1 = random.Next(nail1.Count);
                            var randomnail1 = nail1[randomnub1];
                            randomnail1.gameObject.AddComponent<NailSniper>();
                            randomnail1.gameObject.AddComponent<NailBlock>();
                            if (randomnail1.transform.position.x - HeroController.instance.gameObject.transform.position.x < 0)
                            {
                                randomnail1.gameObject.GetComponent<NailSniper>().StrikeX = (float)Math.Cos(Math.PI + Math.Atan((randomnail1.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail1.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                                randomnail1.gameObject.GetComponent<NailSniper>().StrikeY = (float)Math.Sin(Math.PI + Math.Atan((randomnail1.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail1.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                            }
                            else
                            {
                                randomnail1.gameObject.GetComponent<NailSniper>().StrikeX = (float)Math.Cos(Math.Atan((randomnail1.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail1.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                                randomnail1.gameObject.GetComponent<NailSniper>().StrikeY = (float)Math.Sin(Math.Atan((randomnail1.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (randomnail1.transform.position.x - HeroController.instance.gameObject.transform.position.x))) * 22;
                            }
                            nail1.Remove(randomnail1);
                            NailCounting++;
                        }
                    }
                    if (MoreNailSniperTime > 0)
                    {
                        MoreNailSniperTime -= Time.deltaTime;
                        if (NailSummoned1 == false)
                        {
                            NailSummoned1 = true;
                            while (NailCount <= 30)
                            {
                                NailCount++;
                                X1 += 1.2333f;
                                var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(X1, Y1, 0), Quaternion.Euler(0, 0, 180));
                                nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                                nail.gameObject.AddComponent<AddGlowEffect>();
                                nail.SetActive(true);
                                nail1.Add(nail);
                            }
                        }
                    }
                }
            }
        }
        public class MoreNailSniper : MonoBehaviour
        {
            public List<GameObject> nail1 = new List<GameObject>();
            public List<GameObject> nail2 = new List<GameObject>();
            public List<GameObject> nail3 = new List<GameObject>();
            public float MoreNailSniperTime = 0f;
            public float MoreNailSniperTimeLimit = 0f;
            public float Angle = 0f;
            public float X1;
            public float Y1;
            public float X2;
            public float Y2;
            public float X3;
            public float Y3;
            public bool NailSummoned1 = false;
            public bool NailSummoned2 = false;
            public bool NailSummoned3 = false;
            public void Tri()
            {
                if (MoreNailSniperTime <= 0f)
                {
                    NailSummoned1 = false;
                    NailSummoned2 = false;
                    NailSummoned3 = false;
                    MoreNailSniperTime = 6.5f;
                    MoreNailSniperTimeLimit = 6.5f;
                }
            }
            public void Reset()
            {
                MoreNailSniperTime = 0f;
                MoreNailSniperTimeLimit = 0f;
                Angle = 0f;
                NailSummoned1 = false;
                NailSummoned2 = false;
                NailSummoned3 = false;
            }
            public void FixedUpdate()
            {
                if (MoreNailSniperTime > 0)
                {
                    MoreNailSniperTime -= Time.deltaTime;
                    if (MoreNailSniperTime <= 6.5f && NailSummoned1 == false)
                    {
                        NailSummoned1 = true;
                        Angle = BossBehavior.RX * 180f;
                        X1 = (float)Math.Cos(DegreesToRadians(Angle)) * 20;
                        Y1 = (float)Math.Sin(DegreesToRadians(Angle)) * 20;
                        var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(HeroController.instance.gameObject.transform.position.x + X1, HeroController.instance.gameObject.transform.position.y + Y1, 0), Quaternion.Euler(0, 0, 0));
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                        nail.gameObject.AddComponent<AddGlowEffect>();
                        nail.gameObject.AddComponent<NailSniper>();
                        nail.gameObject.AddComponent<NailBlock>();
                        nail.gameObject.GetComponent<NailSniper>().StrikeX = X1;
                        nail.gameObject.GetComponent<NailSniper>().StrikeY = Y1;
                        nail.SetActive(true);
                        nail1.Add(nail);
                    }
                    if (MoreNailSniperTime <= 4f && NailSummoned2 == false)
                    {
                        NailSummoned2 = true;
                        Angle = BossBehavior.RX * 180f;
                        X2 = (float)Math.Cos(DegreesToRadians(Angle)) * 20;
                        Y2 = (float)Math.Sin(DegreesToRadians(Angle)) * 20;
                        var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(HeroController.instance.gameObject.transform.position.x + X1, HeroController.instance.gameObject.transform.position.y + Y1, 0), Quaternion.Euler(0, 0, 0));
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                        nail.gameObject.AddComponent<AddGlowEffect>();
                        nail.gameObject.AddComponent<NailSniper>();
                        nail.gameObject.AddComponent<NailBlock>();
                        nail.gameObject.GetComponent<NailSniper>().StrikeX = X2;
                        nail.gameObject.GetComponent<NailSniper>().StrikeY = Y2;
                        nail.SetActive(true);
                        nail2.Add(nail);
                    }
                    if (MoreNailSniperTime <= 1.5f && NailSummoned3 == false)
                    {
                        NailSummoned3 = true;
                        Angle = BossBehavior.RX * 180f;
                        X3 = (float)Math.Cos(DegreesToRadians(Angle)) * 20;
                        Y3 = (float)Math.Sin(DegreesToRadians(Angle)) * 20;
                        var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(HeroController.instance.gameObject.transform.position.x + X1, HeroController.instance.gameObject.transform.position.y + Y1, 0), Quaternion.Euler(0, 0, 0));
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                        nail.gameObject.AddComponent<AddGlowEffect>();
                        nail.gameObject.AddComponent<NailSniper>();
                        nail.gameObject.AddComponent<NailBlock>();
                        nail.gameObject.GetComponent<NailSniper>().StrikeX = X3;
                        nail.gameObject.GetComponent<NailSniper>().StrikeY = Y3;
                        nail.SetActive(true);
                        nail3.Add(nail);
                    }
                }
            }

        }
        private class NailSniperCurving : MonoBehaviour
        {
            private List<GameObject> Beams = new List<GameObject>();
            private float NailSniperTime;
            private float NailSniperTimeLimit = 0.3f;
            public float StrikeX = 0;
            public float StrikeY = 0;
            private float StrikeAngle;
            public float Speed = 125f;
            private float ADDANGLE;
            private float SIGH;
            private bool Light1 = false;
            private bool Light2 = false;
            private bool Fired = false;
            private bool FireReady = false;
            public void Reset()
            {
                NailSniperTime = 0f;
                NailSniperTimeLimit = 0f;
                StrikeX = 0;
                StrikeY = 0;
                StrikeAngle = 0f;
                Light1 = false;
                Light2 = false;
                Fired = false;
                FireReady = false;
            }
            private void Start()
            {
                gameObject.LocateMyFSM("Control").GetState("Fire CW").RemoveAction<FaceAngle>();
                gameObject.LocateMyFSM("Control").GetState("Recycle").RemoveAction<RecycleSelf>();
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<FloatAdd>().add.Value = 0f;
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 40f;
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = 0f;
                NailSniperTime = 3.7f;
                NailSniperTimeLimit = 3.7f;
                Light1 = false;
                Light2 = false;
                Fired = false;
                FireReady = false;
                SIGH = BossBehavior.SIGHX;
            }
            private void FixedUpdate()
            {
                NailSniperTime -= Time.deltaTime;
                if (Math.Sign(HeroController.instance.gameObject.transform.position.x - gameObject.transform.position.x) < 0)
                {
                    ADDANGLE = 180;
                }
                else
                {
                    ADDANGLE = 0;
                }
                StrikeAngle = ADDANGLE + (float)RadiansToDegrees(Math.Atan((gameObject.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (gameObject.transform.position.x - HeroController.instance.gameObject.transform.position.x)));
                if (NailSniperTime > 1.3f)
                    gameObject.transform.eulerAngles = new Vector3(0, 0, StrikeAngle - 90f);

                if (NailSniperTime <= NailSniperTimeLimit)
                {
                    NailSniperTimeLimit -= 0.02f;
                    if (Fired == true)
                    {
                        var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, gameObject.transform.position + new Vector3(0, 0, 0), Quaternion.Euler(gameObject.transform.eulerAngles + new Vector3(0, 0, 180f)));
                        Pt.gameObject.GetComponent<Transform>().localScale = Pt.gameObject.GetComponent<Transform>().localScale + new Vector3(-0.9f, 0, 0);
                        Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 20f;
                        Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 90f;
                        Pt.gameObject.GetComponent<ParticleSystem>().Play();

                        Pt.AddComponent<PtRecycle>();
                    }
                    else
                    {
                        if(StrikeX != 0 || StrikeY != 0)
                        {
                            gameObject.transform.position = gameObject.transform.position + new Vector3((HeroController.instance.gameObject.transform.position.x + StrikeX * 4.1f/5 - gameObject.transform.position.x) / 8.5f, (HeroController.instance.gameObject.transform.position.y + StrikeY * 3.12f/5 - gameObject.transform.position.y) / 8.5f, 0);
                        }
                    }
                }

                if (NailSniperTime <= 2.9f && Light1 == false)
                {
                    Light1 = true;
                    var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle));
                    var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle + 180));
                    beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.5f, beam1.transform.localScale.z);
                    beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.5f, beam2.transform.localScale.z);
                    Beams.Add(beam1);
                    Beams.Add(beam2);
                    foreach (var beam in Beams)
                    {
                        beam.AddComponent<PtRecycle>();
                        beam.gameObject.AddComponent<BeamBlack>();
                        beam.SetActive(true);
                        beam.SetActiveChildren(true);
                        beam.LocateMyFSM("Control").SetState("Antic");
                        beam.LocateMyFSM("Control").SendEvent("ANTIC");
                    }
                }
                if (NailSniperTime <= 2.86f && NailSniperTime >= 2.2 && Beams.Count != 0)
                {
                    foreach (var beam in Beams)
                    {
                        beam.LocateMyFSM("Control").SendEvent("FIRE");
                        beam.LocateMyFSM("Control").SetState("End");
                        beam.LocateMyFSM("Control").SendEvent("END");
                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILCHARGE, 1.2f);
                    }
                    Beams.Clear();
                }
                if (NailSniperTime <= 2.1f && Light2 == false)
                {
                    Light2 = true;
                    var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle));
                    var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle + 180));
                    beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.5f, beam1.transform.localScale.z);
                    beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.5f, beam2.transform.localScale.z);
                    Beams.Add(beam1);
                    Beams.Add(beam2);
                    foreach (var beam in Beams)
                    {
                        beam.AddComponent<PtRecycle>();
                        beam.gameObject.AddComponent<BeamBlack>();
                        beam.SetActive(true);
                        beam.SetActiveChildren(true);
                        beam.LocateMyFSM("Control").SetState("Antic");
                        beam.LocateMyFSM("Control").SendEvent("ANTIC");
                    }
                }
                if (NailSniperTime <= 2.06f && NailSniperTime >= 1.4 && Beams.Count != 0)
                {
                    foreach (var beam in Beams)
                    {
                        beam.LocateMyFSM("Control").SendEvent("FIRE");
                        beam.LocateMyFSM("Control").SetState("End");
                        beam.LocateMyFSM("Control").SendEvent("END");
                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILCHARGE, 1.2f);
                    }
                    Beams.Clear();
                }
                if (NailSniperTime <= 1.3f && FireReady == true && Fired == false)
                {
                    Fired = true;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 500f;
                    gameObject.LocateMyFSM("Control").SetState("Fire CW");
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = - Speed;
                    var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, gameObject.transform.position + new Vector3(0, 0, 0), Quaternion.Euler(gameObject.transform.eulerAngles + new Vector3(0, 0, 180f)));
                    Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 300f;
                    Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 60f;
                    Pt.gameObject.GetComponent<ParticleSystem>().Play();
                    Pt.AddComponent<PtRecycle>();
                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILSHOT, 1.2f);
                }
                if (NailSniperTime <= 1.3f && Fired == true)
                {
                    gameObject.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value += 3f * SIGH * Time.deltaTime/0.02f;
                    gameObject.transform.Rotate(0, 0, 3f * SIGH * Time.deltaTime/0.02f,Space.Self);
                }
                if (NailSniperTime <= 1.3f && FireReady == false)
                {
                    FireReady = true;
                    gameObject.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value = StrikeAngle + 180 ;
                    gameObject.transform.localScale = gameObject.transform.localScale + new Vector3(0, 0.7f, 0);
                }
                if (NailSniperTime <= 0f)
                {
                    gameObject.Recycle();
                }
            }

        }
        private class NailSniper : MonoBehaviour
        {
            private List<GameObject> Beams = new List<GameObject>();
            private float NailSniperTime;
            private float NailSniperTimeLimit = 0.3f;
            public float StrikeX = 0;
            public float StrikeY = 0;
            private float StrikeAngle;
            public float Speed = 125f;
            private float ADDANGLE;
            private bool Light1 = false;
            private bool Light2 = false;
            private bool Fired = false;
            private bool FireReady = false;
            public void Reset()
            {
                NailSniperTime = 0f;
                NailSniperTimeLimit = 0f;
                StrikeX = 0;
                StrikeY = 0;
                StrikeAngle = 0f;
                Light1 = false;
                Light2 = false;
                Fired = false;
                FireReady = false;
            }
            private void Start()
            {
                gameObject.LocateMyFSM("Control").GetState("Fire CW").RemoveAction<FaceAngle>();
                gameObject.LocateMyFSM("Control").GetState("Recycle").RemoveAction<RecycleSelf>();
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<FloatAdd>().add.Value = 0f;
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 40f;
                gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = 0f;
                NailSniperTime = 3.7f;
                NailSniperTimeLimit = 3.7f;
                Light1 = false;
                Light2 = false;
                Fired = false;
                FireReady = false;
            }
            private void FixedUpdate()
            {
                NailSniperTime -= Time.deltaTime;
                if (Math.Sign(HeroController.instance.gameObject.transform.position.x - gameObject.transform.position.x) < 0)
                {
                    ADDANGLE = 180;
                }
                else
                {
                    ADDANGLE = 0;
                }
                StrikeAngle = ADDANGLE + (float)RadiansToDegrees(Math.Atan((gameObject.transform.position.y - HeroController.instance.gameObject.transform.position.y) / (gameObject.transform.position.x - HeroController.instance.gameObject.transform.position.x)));
                if (NailSniperTime > 1.3f)
                    gameObject.transform.eulerAngles = new Vector3(0, 0, StrikeAngle - 90f);

                if (NailSniperTime <= NailSniperTimeLimit)
                {
                    NailSniperTimeLimit -= 0.02f;
                    if (Fired == true)
                    {
                        var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, gameObject.transform.position + new Vector3(0, 0, 0), Quaternion.Euler(gameObject.transform.eulerAngles + new Vector3(0, 0, 180f)));
                        Pt.gameObject.GetComponent<Transform>().localScale = Pt.gameObject.GetComponent<Transform>().localScale + new Vector3(-0.9f, 0, 0);
                        Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 20f;
                        Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 90f;
                        Pt.gameObject.GetComponent<ParticleSystem>().Play();

                        Pt.AddComponent<PtRecycle>();
                    }
                    else
                    {
                        if(StrikeX != 0 || StrikeY != 0)
                        {
                            gameObject.transform.position = gameObject.transform.position + new Vector3((HeroController.instance.gameObject.transform.position.x + StrikeX * 4.1f/5 - gameObject.transform.position.x) / 8.5f, (HeroController.instance.gameObject.transform.position.y + StrikeY * 3.12f/5 - gameObject.transform.position.y) / 8.5f, 0);
                        }
                    }
                }

                if (NailSniperTime <= 2.9f && Light1 == false)
                {
                    Light1 = true;
                    var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle));
                    var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle + 180));
                    beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.5f, beam1.transform.localScale.z);
                    beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.5f, beam2.transform.localScale.z);
                    Beams.Add(beam1);
                    Beams.Add(beam2);
                    foreach (var beam in Beams)
                    {
                        beam.gameObject.AddComponent<BeamWhite>();
                        beam.SetActive(true);
                        beam.SetActiveChildren(true);
                        beam.LocateMyFSM("Control").SetState("Antic");
                        beam.LocateMyFSM("Control").SendEvent("ANTIC");
                        beam.AddComponent<PtRecycle>();
                    }
                }
                if (NailSniperTime <= 2.86f && NailSniperTime >= 2.2 && Beams.Count != 0)
                {
                    foreach (var beam in Beams)
                    {
                        beam.LocateMyFSM("Control").SendEvent("FIRE");
                        beam.LocateMyFSM("Control").SetState("End");
                        beam.LocateMyFSM("Control").SendEvent("END");
                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILCHARGE, 1.2f);
                    }
                    Beams.Clear();
                }
                if (NailSniperTime <= 2.1f && Light2 == false)
                {
                    Light2 = true;
                    var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle));
                    var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position, Quaternion.Euler(0, 0, StrikeAngle + 180));
                    beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 0.5f, beam1.transform.localScale.z);
                    beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 0.5f, beam2.transform.localScale.z);
                    Beams.Add(beam1);
                    Beams.Add(beam2);
                    foreach (var beam in Beams)
                    {
                        beam.AddComponent<PtRecycle>();
                        beam.gameObject.AddComponent<BeamWhite>();
                        beam.SetActive(true);
                        beam.SetActiveChildren(true);
                        beam.LocateMyFSM("Control").SetState("Antic");
                        beam.LocateMyFSM("Control").SendEvent("ANTIC");
                    }
                }
                if (NailSniperTime <= 2.06f && NailSniperTime >= 1.4 && Beams.Count != 0)
                {
                    foreach (var beam in Beams)
                    {
                        beam.LocateMyFSM("Control").SendEvent("FIRE");
                        beam.LocateMyFSM("Control").SetState("End");
                        beam.LocateMyFSM("Control").SendEvent("END");
                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILCHARGE, 1.2f);
                    }
                    Beams.Clear();
                }
                if (NailSniperTime <= 1.3f && FireReady == true && Fired == false)
                {
                    Fired = true;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 500f;
                    gameObject.LocateMyFSM("Control").SetState("Fire CW");
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = - Speed;
                    var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, gameObject.transform.position + new Vector3(0, 0, 0), Quaternion.Euler(gameObject.transform.eulerAngles + new Vector3(0, 0, 180f)));
                    Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 300f;
                    Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 60f;
                    Pt.gameObject.GetComponent<ParticleSystem>().Play();
                    Pt.AddComponent<PtRecycle>();
                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(NAILSHOT, 1.2f);
                }
                if (NailSniperTime <= 1.3f && FireReady == false)
                {
                    FireReady = true;
                    gameObject.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value = StrikeAngle + 180 ;
                    gameObject.transform.localScale = gameObject.transform.localScale + new Vector3(0, 0.7f, 0);
                }
                if (NailSniperTime <= 0f)
                {
                    gameObject.Recycle();
                }
            }

        }
        public class NailSpeedUp : MonoBehaviour
        {
            private float NailSpeedAdd;
            private float NailSpeedAdding;
            public float NailSpeed = 20f;
            private float NailSpeedTime;
            private float NailSpeedTimeLimit;
            private float NailLength;
            private void Start()
            {
                gameObject.LocateMyFSM("Control").GetState("Set Collider").AddMethod(() =>
                {
                    gameObject.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value = gameObject.transform.eulerAngles.z + 90f;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").RemoveAction<FaceAngle>();
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<FloatAdd>().add.Value = 0f;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = 0f;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 99f;
                    gameObject.LocateMyFSM("Control").SetState("Fire CW");
                });
                NailLength = gameObject.transform.localScale.y;
                NailSpeedTime = 10f;
                NailSpeedTimeLimit = 10f;
                NailSpeedAdd = 0f;
                NailSpeedAdding = NailSpeed/1000f;
            }
            private void FixedUpdate()
            {
                NailSpeedTime -= Time.deltaTime;
                if (NailSpeedTime <= NailSpeedTimeLimit)
                {
                    NailSpeedTimeLimit -= 0.02f;

                    //gameObject.LocateMyFSM("Control").SetState("Fire CW");
                    NailSpeedAdd += NailSpeedAdding;
                    if (NailSpeedAdding > 0)
                        NailSpeedAdding -= NailSpeed/100000f;
                    gameObject.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value += NailSpeedAdd;
                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, NailLength + NailSpeedAdd * 0.2f, gameObject.transform.localScale.z);
                    //gameObject.transform.position = gameObject.transform.position + new Vector3((float)Math.Cos(DegreesToRadians(gameObject.transform.eulerAngles.z + 90f)) * NailSpeedAdd, (float)Math.Sin(DegreesToRadians(gameObject.transform.eulerAngles.z + 90f)) * NailSpeedAdd, 0);
                    if (NailSpeedTime <= 0)
                    {
                        gameObject.Recycle();
                    }
                }
            }
        }
        public class P3BigOrb : MonoBehaviour
        {
            public List<GameObject> Orb = new List<GameObject>();
            public float P3BigOrbTime1;
            public float P3BigOrbTimeLimit1;
            public bool Summoned = false;
            public bool Destory = false;
            public void Tri()
            {
                P3BigOrbTime1 = 999f;
                P3BigOrbTimeLimit1 = 999f;
                Summoned = false;
                Destory = false;
            }
            public void Reset()
            {
                P3BigOrbTime1 = 0;
                P3BigOrbTimeLimit1 = 0;
                Summoned = false;
                Destory = false;
            }
            private void FixedUpdate()
            {
                if (P3BigOrbTime1 > 0 && Destory == false)
                {
                    P3BigOrbTime1 -= Time.deltaTime;
                    if (HardMode == 0)
                    {
                        if (P3BigOrbTime1 <= P3BigOrbTimeLimit1)
                        {
                            P3BigOrbTimeLimit1 -= 0.02f;
                            if (Summoned == false)
                            {
                                var orb = GameObject.Instantiate(BossBehavior.ORB, HeroController.instance.gameObject.transform.position + new Vector3(BossBehavior.RX * 7, 30, 0), Quaternion.Euler(0, 0, 0));
                                orb.transform.localScale = new Vector3(3f, 3f, orb.transform.localScale.z);
                                orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<Wait>(2).time.Value = 99999f;
                                orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<Wait>(4).time.Value = 99999f;
                                orb.SetActive(true);
                                orb.LocateMyFSM("Orb Control").SetState("Chase Hero");
                                orb.AddComponent<OrbWhite>();
                                Orb.Add(orb);
                                Summoned = true;
                            }
                            else
                            {
                                if (Destory != true && Orb.Count == 1)
                                    foreach (var orb in Orb)
                                    {
                                        orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<ChaseObjectV2>().speedMax.Value = 3f + ((float)Math.Sqrt((orb.transform.position.y - HeroController.instance.gameObject.transform.position.y) * (orb.transform.position.y - HeroController.instance.gameObject.transform.position.y) + (orb.transform.position.x - HeroController.instance.gameObject.transform.position.x) * (orb.transform.position.x - HeroController.instance.gameObject.transform.position.x))) / 3f;
                                        orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<ChaseObjectV2>().accelerationForce = 50f;
                                    }
                            }
                        }
                    }
                    else
                    {
                        if (P3BigOrbTime1 <= P3BigOrbTimeLimit1)
                        {
                            P3BigOrbTimeLimit1 -= 0.02f;
                            if (Summoned == false)
                            {
                                var orb = GameObject.Instantiate(BossBehavior.ORB, HeroController.instance.gameObject.transform.position + new Vector3(BossBehavior.RX * 7, 30, 0), Quaternion.Euler(0, 0, 0));
                                orb.transform.localScale = new Vector3(5f, 5f, orb.transform.localScale.z);
                                orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<Wait>(2).time.Value = 99999f;
                                orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<Wait>(4).time.Value = 99999f;
                                orb.SetActive(true);
                                orb.LocateMyFSM("Orb Control").SetState("Chase Hero");
                                orb.AddComponent<OrbWhite>();
                                Orb.Add(orb);
                                Summoned = true;
                            }
                            else
                            {
                                if (Orb.Count == 1)
                                    foreach (var orb in Orb)
                                    {
                                        orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<ChaseObjectV2>().speedMax.Value = 3f + ((float)Math.Sqrt((orb.transform.position.y - HeroController.instance.gameObject.transform.position.y) * (orb.transform.position.y - HeroController.instance.gameObject.transform.position.y) + (orb.transform.position.x - HeroController.instance.gameObject.transform.position.x) * (orb.transform.position.x - HeroController.instance.gameObject.transform.position.x))) / 3f;
                                        orb.LocateMyFSM("Orb Control").GetState("Chase Hero").GetAction<ChaseObjectV2>().accelerationForce = 50f;
                                    }
                            }
                        }
                    }
                }
                if (Destory == true && Orb.Count == 1)
                {
                    foreach (var orb in Orb)
                    {
                        orb.LocateMyFSM("Orb Control").SendEvent("DISSIPATE");
                    }
                }
            }
        }
        public class NailCombRL : MonoBehaviour
        {
            public bool NailCombR = true;
            public bool NailCombStart = false;
            public float NailCombTime1 = 0;
            public float NailCombTimeLimit1 = 0;
            public bool set = true;
            public void Tri()
            {
                NailCombTime1 = 0f;
                NailCombTimeLimit1 = 0f;
                NailCombStart = true;
            }
            public void Reset() 
            {
                NailCombR = true;
                NailCombStart = false;
                set = true;
            }
            private void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (NailCombTime1 > 0.1)
                    {
                        NailCombTime1 -= Time.deltaTime;
                        if (NailCombTime1 <= NailCombTimeLimit1 && NailCombStart == true)
                        {
                            NailCombTimeLimit1 -= 1.9f;
                            if (NailCombR == true)
                            {
                                BossBehavior.BOSS.LocateMyFSM("Attack Commands").SendEvent("COMB R");
                                NailCombR = false;
                            }
                            else
                            {
                                BossBehavior.BOSS.LocateMyFSM("Attack Commands").SendEvent("COMB L");
                                NailCombR = true;
                            }
                        }
                        if (BossBehavior.BOSS.LocateMyFSM("Control").ActiveStateName != "RG" && BossBehavior.BOSS.LocateMyFSM("Control").ActiveStateName != "Rage1 Loop")
                        {
                            NailCombStart = false;
                        }
                    }
                    else
                    {
                        NailCombTime1 = 3f;
                        NailCombTimeLimit1 = 3f;
                    }
                }
                else
                {
                    if (NailCombTime1 > 0.1 && NailCombStart == true)
                    {
                        NailCombTime1 -= Time.deltaTime;
                    }
                    if (NailCombTime1 <= 0.1 && NailCombStart == true)
                    {
                        NailCombTime1 = 4f;
                        BossBehavior.BOSS.GetComponent<BeamSkill1>().Tri();
                    }
                }
            }
        }
        public class OrbBlast : MonoBehaviour
        {
            public List<GameObject> LastOrb = new List<GameObject>();
            public List<GameObject> Beams = new List<GameObject>();
            System.Random random = new System.Random();

            //随机数组件 RX与RY为-1~1的f随机数且相互独立
            
            double RX0 => random.NextDouble();
            double RY0 => random.NextDouble();
            public double R1 => random.NextDouble();
            public float SIGHX => (random.Next(0, 2) * 2 - 1);
            public float SIGHY => (random.Next(0, 2) * 2 - 1);
            public float RX => (float)(RX0 * SIGHX);
            public float RY => (float)(RY0 * SIGHY);
            public float OrbBlastTime1;
            public float OrbBlastTimeLimit1;
            public float OrbSummonX;
            public float OrbSummonY;
            public float OrbSigh = 1;
            public float OrbR = 5f;
            public float OrbAngle = 0;
            public float SrikePointX = 0;
            public float SrikePointY = 0;
            public float FlashLimit;
            public void Tri()
            {
                if (OrbBlastTime1 <= 0)
                {
                    OrbBlastTime1 = 6f;
                    OrbBlastTimeLimit1 = 6f;
                    FlashLimit = 4f;
                    SrikePointX = HeroController.instance.gameObject.transform.position.x + RX * 10;
                    SrikePointY = 23 + RX * 3;
                }
            }
            public void Reset()
            {
                OrbBlastTime1 = 0;
                OrbBlastTimeLimit1 = 0;
            }
            private void FixedUpdate()
            {
                if (OrbBlastTime1 > 0)
                {
                    OrbBlastTime1 -= Time.deltaTime;
                    if (OrbBlastTime1 <= OrbBlastTimeLimit1)
                    {
                        OrbBlastTimeLimit1 -= 0.1f;
                        foreach (var lastorb in LastOrb)
                        {
                            lastorb.LocateMyFSM("Orb Control").SetState("Impact");
                        }
                        LastOrb.Clear();
                        foreach (var beam in Beams)
                        {
                            beam.LocateMyFSM("Control").SendEvent("FIRE");
                            beam.LocateMyFSM("Control").SetState("End");
                            beam.LocateMyFSM("Control").SendEvent("END");
                        }
                        Beams.Clear();
                        /*
                        if (OrbBlastTime1 > 2.2f && OrbBlastTime1 <= (FlashLimit + 2))
                        {
                            FlashLimit /= 1.25f;

                            var beam1 = GameObject.Instantiate(BEAM,new Vector3(SrikePointX, SrikePointY, 0), Quaternion.Euler(0, 0, 90 + RX * 60));
                            var beam2 = GameObject.Instantiate(BEAM,new Vector3(SrikePointX, SrikePointY, 0), Quaternion.Euler(0, 0, beam1.transform.eulerAngles.z + 180));
                            beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 1, beam1.transform.localScale.z);
                            beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 1, beam2.transform.localScale.z);
                            Beams.Add(beam1);
                            Beams.Add(beam2);
                            foreach (var beam in Beams)
                            {
                                beam.SetActive(true);
                                beam.SetActiveChildren(true);
                                beam.LocateMyFSM("Control").SetState("Antic");
                                beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            }
                        }
                        */
                        if (OrbBlastTimeLimit1 > 0.5f && OrbBlastTime1 <= 2.2f)
                        {
                            OrbSummonX = RX * OrbR;
                            OrbSummonY = RY * OrbR;
                            while (OrbSummonY >= (float)Math.Sqrt(16 - OrbSummonX * OrbSummonX))
                            {
                                OrbSummonY = RY * OrbR;
                            }
                            var orb = GameObject.Instantiate(BossBehavior.ORB, new Vector3(SrikePointX, SrikePointY, 0) + new Vector3(OrbSummonX, OrbSummonY, 0), Quaternion.Euler(0, 0, 0));
                            orb.transform.localScale = new Vector3(4f, 4f, orb.transform.localScale.z);
                            orb.SetActive(true);
                            orb.SetActiveChildren(true);
                            LastOrb.Add(orb);
                        }
                        else
                        {
                            foreach (var orb in LastOrb)
                            {
                                orb.Recycle();
                            }
                        }
                    }
                }
            }
        }
        public class OrbFlash : MonoBehaviour
        {
            public List<GameObject> LastOrb = new List<GameObject>();
            System.Random random = new System.Random();

            //随机数组件 RX与RY为-1~1的f随机数且相互独立
            double RX0 => random.NextDouble();
            double RY0 => random.NextDouble();
            public double R1 => random.NextDouble();
            public float SIGHX => (random.Next(0, 2) * 2 - 1);
            public float SIGHY => (random.Next(0, 2) * 2 - 1);
            public float RX => (float)(RX0 * SIGHX);
            public float RY => (float)(RY0 * SIGHY);
            public float OrbFlashTime1;
            public float OrbFlashTimeLimit1;
            public float OrbSummonX;
            public float OrbSummonY;
            public float OrbSigh = 1;
            public float OrbR = 3f;
            public float OrbAngle = 0;
            public void Tri()
            {
                OrbFlashTime1 = 0.1f;
                OrbFlashTimeLimit1 = 0.1f;
                OrbR = 0.1f;
            }
            public void Tri2()
            {
                OrbFlashTime1 = 2f;
                OrbFlashTimeLimit1 = 2f;
                OrbR = 3f;
            }
            private void Start()
            {
                LastOrb.Clear();
            }
            private void FixedUpdate()
            {
                if (OrbFlashTime1 > 0)
                {
                    OrbFlashTime1 -= Time.deltaTime;
                    if (OrbFlashTime1 <= OrbFlashTimeLimit1)
                    {
                        OrbFlashTimeLimit1 -= 0.12f;
                        if (OrbFlashTime1 > 0.05f)
                        {
                            OrbSummonX = RX * OrbR;
                            OrbSummonY = RY * OrbR;
                            while (OrbSummonY >= (float)Math.Sqrt(16 - OrbSummonX * OrbSummonX))
                            {
                                OrbSummonY = RY * OrbR;
                            }
                            var orb = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(OrbSummonX, OrbSummonY + 2.5f, 0), Quaternion.Euler(0, 0, 0));
                            orb.SetActive(true);
                            orb.SetActiveChildren(true);
                            orb.LocateMyFSM("Orb Control").SetState("Dissipate");
                        }
                        else 
                        {
                            foreach (var orb in LastOrb)
                            {
                                orb.Recycle();
                            }
                        }
                    }
                }
            }
        }
        public class OrbSkill1 : MonoBehaviour
        {
            System.Random random = new System.Random();

            //随机数组件 RX与RY为-1~1的f随机数且相互独立
            double RX0 => random.NextDouble();
            double RY0 => random.NextDouble();
            public float SIGHX => (random.Next(0, 2) * 2 - 1);
            public float SIGHY => (random.Next(0, 2) * 2 - 1);
            public float RX => (float)(RX0 * SIGHX);
            public float RY => (float)(RY0 * SIGHY);
            
            public List<GameObject> Orbs = new List<GameObject>();
            public float OrbSkillTime1;
            public float OrbSkillTimeLimit1;
            public float OrbSummonX;
            public float OrbSummonY;
            public float OrbSigh = 1;
            public float OrbR = 6;
            public float OrbAngle = 0;
            public float OrbSize;
            public float SIGH;
            public float SIZE;
            public bool OrbSummonFrist = false;
            public void Tri()
            {
                OrbSkillTime1 = 5f;
                OrbSkillTimeLimit1 = 5f;
                OrbSummonFrist = false;
            }
            public void Reset()
            {
                OrbSkillTime1 = 0;
                OrbSkillTimeLimit1 = 0;
                OrbSummonFrist = false;
            }
            private void FixedUpdate()
            {
                if (HardMode == 1)
                {
                    if (OrbSkillTime1 > 0)
                    {
                        OrbSkillTime1 -= Time.deltaTime;
                        if (OrbSkillTime1 <= OrbSkillTimeLimit1)
                        {
                            OrbSkillTimeLimit1 -= 0.25f;
                            if (OrbSummonFrist == false)
                            {
                                OrbSummonFrist = true;
                                SIGH = BossBehavior.SIGHX;
                                SIZE = RY;
                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinStart();
                                BossBehavior.BOSS.GetComponent<HaloExpand>().HaloExpandStart();
                                if(BossBehavior.Distance >= 9)
                                    OrbR = RX * 2 + 5;
                                else
                                    OrbR = RX * 2 + 14;
                                OrbAngle = BossBehavior.Angle + 180f;
                                OrbSummonX = (float)Math.Cos(DegreesToRadians(OrbAngle)) * OrbR;
                                OrbSummonY = (float)Math.Sin(DegreesToRadians(OrbAngle)) * OrbR;
                                var orb = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(OrbSummonX, OrbSummonY, 0), Quaternion.Euler(0, 0, 0));
                                orb.gameObject.AddComponent<OrbWhite>();
                                Orbs.Add(orb);
                            }
                            if (OrbSkillTime1 >= 1.4 && OrbSkillTime1 < 4.6)
                            {
                                foreach (var orb1 in Orbs)
                                {
                                    orb1.SetActive(true);
                                    orb1.LocateMyFSM("Orb Control").SetState("Chase Hero 2");
                                }
                                OrbAngle += (30f + RX * 30) * SIGH;
                                OrbSize = RY * 1.5f;
                                OrbSummonX = (float)Math.Cos(DegreesToRadians(OrbAngle)) * OrbR;
                                OrbSummonY = (float)Math.Sin(DegreesToRadians(OrbAngle)) * OrbR;
                                var orb = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(OrbSummonX, OrbSummonY, 0), Quaternion.Euler(0, 0, 0));
                                orb.gameObject.AddComponent<OrbWhite>();
                                orb.LocateMyFSM("Orb Control").GetState("Orbiting").AddMethod(() =>
                                {
                                    orb.transform.localScale = new Vector3(orb.transform.localScale.x + 0.2f * OrbSize, orb.transform.localScale.y + 0.2f * OrbSize, orb.transform.localScale.z);
                                    orb.SetActive(true);
                                    orb.LocateMyFSM("Orb Control").SetState("Chase Hero 2");
                                });
                                Orbs.Add(orb);
                            }
                            if (OrbSkillTime1 < 1.4 && Orbs.Count != 0)
                            {
                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinEnd();
                                BossBehavior.BOSS.GetComponent<HaloExpand>().HaloExpandEnd();
                                foreach (var orb in Orbs)
                                {
                                }
                                Orbs.Clear();
                            }
                        }
                    }
                }
                else
                {
                    if (OrbSkillTime1 > 0)
                    {
                        OrbSkillTime1 -= Time.deltaTime;
                        if (OrbSkillTime1 <= OrbSkillTimeLimit1)
                        {
                            OrbSkillTimeLimit1 -= 0.15f;
                            if (OrbSummonFrist == false)
                            {
                                SIGH = BossBehavior.SIGHX;
                                SIZE = RY;
                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinStart();
                                OrbR = RX * 2 + 5;
                                while (Math.Abs(OrbR - BossBehavior.Distance) < 3)
                                {
                                    OrbR += 5f;
                                }
                                OrbSummonFrist = true;
                                OrbAngle = BossBehavior.Angle;
                                OrbSummonX = (float)Math.Cos(DegreesToRadians(OrbAngle)) * OrbR;
                                OrbSummonY = (float)Math.Sin(DegreesToRadians(OrbAngle)) * OrbR;
                                var orb = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(OrbSummonX, OrbSummonY, 0), Quaternion.Euler(0, 0, 0));
                                orb.AddComponent<OrbWhite>();
                                Orbs.Add(orb);
                            }
                            if (OrbSkillTime1 >= 3 && OrbSkillTime1 < 5.2)
                            {
                                OrbAngle += (30f + RX * 30) * SIGH;
                                OrbSize = SIZE * 1.5f;
                                OrbSummonX = (float)Math.Cos(DegreesToRadians(OrbAngle)) * OrbR;
                                OrbSummonY = (float)Math.Sin(DegreesToRadians(OrbAngle)) * OrbR;
                                var orb = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(OrbSummonX, OrbSummonY, 0), Quaternion.Euler(0, 0, 0));
                                orb.AddComponent<OrbWhite>();
                                orb.LocateMyFSM("Orb Control").GetState("Orbiting").AddMethod(() =>
                                {
                                    orb.transform.localScale = new Vector3(orb.transform.localScale.x + 0.3f * OrbSize, orb.transform.localScale.y + 0.3f * OrbSize, orb.transform.localScale.z);
                                });
                                Orbs.Add(orb);
                            }
                            if (OrbSkillTime1 < 3 && Orbs.Count != 0)
                            {
                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinEnd();
                                foreach (var orb in Orbs)
                                {
                                    orb.SetActive(true);
                                    orb.LocateMyFSM("Orb Control").SetState("Chase Hero 2");

                                }
                                Orbs.Clear();

                            }
                        }
                    }
                }
            }
        }
        public class Nail1: MonoBehaviour
        {
            public List<GameObject> nails1 = new List<GameObject>();
            public float nailtime1;
            public float nailtimelimit1;
            public float nailSummonX;
            public float nailSummonY;
            public float nailX;
            public float nailY;
            public float nailSigh = 1;
            public float nailR = 4;
            public float nailAngle = 0;
            public int nailCount = 0;
            public int nailCountmax = 8;
            public float SIGH;
            public float Count;
            public float Angle;
            public bool nailSummonFrist = false;
            public void Tri()
            {
                nailtime1 = 2.2f;
                nailtimelimit1 = 2.2f;
                nailCount = 0;
            }
            public void Reset()
            {
                nailtime1 = 0;
                nailtimelimit1 = 0;
                nailSummonFrist = false;
                Count = 0;
            }
            private void FixedUpdate()
            {
                if (nailtime1 > 0)
                {
                    nailtime1 -= Time.deltaTime;
                    if (nailtime1 <= nailtimelimit1)
                    {
                        if (nailSummonFrist == false)
                        {
                            nailSummonFrist = true;
                            SIGH = BossBehavior.SIGHX;
                            if (HardMode == 0)
                                Angle = (BossBehavior.Angle + 22.5f);
                            else
                                Angle = (BossBehavior.Angle + 30f);

                            nailX = (float)Math.Cos(DegreesToRadians(Angle + 180f)) * 25;
                            nailY = (float)Math.Sin(DegreesToRadians(Angle + 180f)) * 25;

                            nailR = BossBehavior.RX * 2 + 5;
                            if (BossBehavior.Distance - nailR < 3)
                            {
                                nailR += 4f;
                            }
                            nailAngle = Angle;
                            Count = (random.Next(1, 7));
                        }

                        switch (HardMode)
                        {
                            case 0:
                                nailtimelimit1 -= 0.2f;

                                if (nailtime1 >= 0.4 && nailtime1 <= 2 && nailCount < 8)
                                {
                                    nailAngle += 45f;
                                    nailCount += 1;
                                    nailSummonX = (float)Math.Cos(DegreesToRadians(nailAngle)) * nailR;
                                    nailSummonY = (float)Math.Sin(DegreesToRadians(nailAngle)) * nailR;
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, BossBehavior.BOSS.transform.position + new Vector3(nailSummonX, nailSummonY + 2.5f, 0), Quaternion.Euler(0, 0, nailAngle + 90f));
                                    nail.SetActive(true);
                                    nail.AddComponent<AddGlowEffect>();
                                    nails1.Add(nail);
                                }
                                if (nailtime1 < 0.2)
                                {
                                    foreach (var nail in nails1)
                                    {
                                        nail.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value = nail.transform.eulerAngles.z + 90;
                                        nail.LocateMyFSM("Control").GetState("Fire CW").RemoveAction<FaceAngle>();
                                        nail.LocateMyFSM("Control").GetState("Fire CW").GetAction<FloatAdd>().add.Value = 0f;
                                        nail.LocateMyFSM("Control").GetState("Fire CW").GetAction<SetVelocityAsAngle>().speed.Value = 0f;
                                        nail.LocateMyFSM("Control").GetState("Fire CW").GetAction<Wait>().time.Value = 8f;
                                        nail.LocateMyFSM("Control").SetState("Fire CW");
                                        nail.AddComponent<NailSpeedUp>();
                                        nail.AddComponent<NailBlock>();
                                    }
                                    nails1.Clear();
                                }
                                break;

                            case 1:
                                nailtimelimit1 -= 0.3f;

                                if (nailCount < 6)
                                {
                                    nailAngle += 60f;
                                    nailCount += 1;
                                    nailSummonX = (float)Math.Cos(nailAngle) * nailR;
                                    nailSummonY = (float)Math.Sin(nailAngle) * nailR;
                                    var nail = GameObject.Instantiate(BossBehavior.NAIL, BossBehavior.BOSS.transform.position + new Vector3(nailSummonX, nailSummonY + 2.5f, 0), Quaternion.Euler(0, 0, nailAngle + 90f));
                                    nail.gameObject.AddComponent<NailBlock>();
                                    if (SIGH > 0)
                                    {
                                        if (nailCount == Count)
                                        {
                                            nail.gameObject.AddComponent<AddGlowEffect>();
                                            nail.gameObject.AddComponent<NailSniper>();
                                            nail.SetActive(true);
                                            nail.gameObject.GetComponent<NailSniper>().StrikeX = (float)Math.Cos(nailAngle) * 20;
                                            nail.gameObject.GetComponent<NailSniper>().StrikeY = (float)Math.Sin(nailAngle) * 20;
                                        }
                                        else
                                        {
                                            nail.gameObject.AddComponent<NailBlack>();
                                            nail.gameObject.AddComponent<NailSniperCurving>();
                                            nail.SetActive(true);
                                            nail.gameObject.GetComponent<NailSniperCurving>().StrikeX = (float)Math.Cos(nailAngle) * 20;
                                            nail.gameObject.GetComponent<NailSniperCurving>().StrikeY = (float)Math.Sin(nailAngle) * 20;
                                        }
                                    }
                                    else
                                    {
                                        nail.gameObject.AddComponent<NailSniper>();
                                        nail.gameObject.AddComponent<AddGlowEffect>();
                                        nail.SetActive(true);
                                        nail.gameObject.GetComponent<NailSniper>().StrikeX = nailX;
                                        nail.gameObject.GetComponent<NailSniper>().StrikeY = nailY;
                                    }
                                }
                                break;
                        }

                    }
                }
                else
                    nailSummonFrist = false;
            }
        }
        public class BeamSight : MonoBehaviour
        {
            public List<GameObject> SightBeams = new List<GameObject>();
            public float BeamSightTime1 = 0;
            public float BeamSightTimeLimit1 = 0;
            public float BeamSightAngleAdd = 10f;
            public float SightPointX;
            public float SightPointY;
            public float SightPointXadd;
            public float SightPointYadd;
            public bool BeamSightSummoned = false;
            public bool BeamFired = false;
            public bool BeamSighFired = false;
            public bool BeamSightEnded = false;
            public void Tri()
            {
                if (BeamSightTime1 <= 0)
                {
                    BeamSightTime1 = 2f;
                    BeamSightTimeLimit1 = 2f;
                    BeamSightSummoned = false;
                    BeamSighFired = false;
                    BeamSightEnded = false;
                }
            }
            public void Reset()
            {
                BeamSightTime1 = 0;
                BeamSightTimeLimit1 = 0;
                BeamSightAngleAdd = 10f;
                BeamSightSummoned = false;
                BeamFired = false;
                BeamSighFired = false;
                BeamSightEnded = false;
            }
            private void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (BeamSightTime1 > 0)
                    {
                        BeamSightTime1 -= Time.deltaTime;
                        if (BeamSightSummoned == false)
                        {
                            BeamFired = false;
                            BeamSightSummoned = true;
                            BeamSightAngleAdd = (float)BossBehavior.R1 * 90f;
                            SightPointX = BossBehavior.SIGHX * 25f;
                            SightPointY = BossBehavior.RY * 25f;
                            var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 0f));
                            var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 120f));
                            var beam3 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 240f));
                            beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 1, beam1.transform.localScale.z);
                            beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 1, beam2.transform.localScale.z);
                            beam3.transform.localScale = new Vector3(beam3.transform.localScale.x, 1, beam3.transform.localScale.z);
                            SightBeams.Add(beam1);
                            SightBeams.Add(beam2);
                            SightBeams.Add(beam3);
                            BeamSightAngleAdd = 10f;
                            foreach (var beam in SightBeams)
                            {
                                beam.AddComponent<BeamWhite>();
                                beam.AddComponent<PtRecycle>();
                                beam.SetActive(true);
                                beam.SetActiveChildren(true);
                                beam.LocateMyFSM("Control").SetState("Antic");
                                beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            }
                        }
                        if (BeamSightTime1 <= BeamSightTimeLimit1)
                        {
                            BeamSightTimeLimit1 -= 0.02f;
                            if (BeamSightTime1 > 0.3)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.transform.Rotate(0, 0, BeamSightAngleAdd);
                                    beam.transform.position += new Vector3((HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) * 0.06f, (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y - 0.3f) * 0.06f, 0);
                                    if ((float)Math.Sqrt((HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) * (HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) + (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y) * (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y - 0.3f)) < 0.5f)
                                    {
                                    }
                                }
                                BeamSightAngleAdd /= 1.08f;
                            }
                            if (BeamSightTime1 <= 0.3)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("FIRE");
                                    if (BeamFired == false)
                                    {
                                        BeamFired = true;
                                        var orb = GameObject.Instantiate(BossBehavior.ORB, beam.transform.position, Quaternion.Euler(0, 0, 0));
                                        orb.transform.localScale = new Vector3(orb.transform.localScale.x * 0.9f, orb.transform.localScale.y * 0.9f, 1);
                                        orb.LocateMyFSM("Orb Control").SetState("Impact");
                                    }
                                }
                            }
                            if (BeamSightTime1 <= 0.2)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("END");
                                }
                                SightBeams.Clear();
                            }
                        }
                    }
                }
                else
                {
                    if (BeamSightTime1 > 0)
                    {
                        BeamSightTime1 -= Time.deltaTime;
                        if (BeamSightSummoned == false)
                        {
                            BeamFired = false;
                            BeamSightSummoned = true;
                            BeamSightAngleAdd = (float)BossBehavior.R1 * 90f;
                            SightPointX = BossBehavior.SIGHX * 25f;
                            SightPointY = BossBehavior.RY * 25f;
                            var beam1 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 45f));
                            var beam2 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 135f));
                            var beam3 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 225f));
                            var beam4 = GameObject.Instantiate(BEAM, HeroController.instance.gameObject.transform.position + new Vector3(SightPointX, SightPointY, 0), Quaternion.Euler(0, 0, BeamSightAngleAdd + 315f));
                            beam1.transform.localScale = new Vector3(beam1.transform.localScale.x, 1, beam1.transform.localScale.z);
                            beam2.transform.localScale = new Vector3(beam2.transform.localScale.x, 1, beam2.transform.localScale.z);
                            beam3.transform.localScale = new Vector3(beam3.transform.localScale.x, 1, beam3.transform.localScale.z);
                            beam4.transform.localScale = new Vector3(beam4.transform.localScale.x, 1, beam4.transform.localScale.z);
                            SightBeams.Add(beam1);
                            SightBeams.Add(beam2);
                            SightBeams.Add(beam3);
                            SightBeams.Add(beam4);
                            BeamSightAngleAdd = 10f;
                            foreach (var beam in SightBeams)
                            {
                                beam.AddComponent<BeamWhite>();
                                beam.AddComponent<PtRecycle>();
                                beam.SetActive(true);
                                beam.SetActiveChildren(true);
                                beam.LocateMyFSM("Control").SetState("Antic");
                                beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            }
                        }
                        if (BeamSightTime1 <= BeamSightTimeLimit1)
                        {
                            BeamSightTimeLimit1 -= 0.02f;
                            if (BeamSightTime1 > 0.3)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.transform.Rotate(0, 0, BeamSightAngleAdd);
                                    beam.transform.position += new Vector3((HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) * 0.06f, (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y - 0.3f) * 0.06f, 0);
                                    if ((float)Math.Sqrt((HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) * (HeroController.instance.gameObject.transform.position.x - beam.transform.position.x) + (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y) * (HeroController.instance.gameObject.transform.position.y - beam.transform.position.y - 0.3f)) < 0.5f)
                                    {
                                    }
                                }
                                BeamSightAngleAdd /= 1.08f;
                            }
                            if (BeamSightTime1 <= 0.3)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("FIRE");
                                    if (BeamFired == false)
                                    {
                                        BeamFired = true;
                                        var orb = GameObject.Instantiate(BossBehavior.ORB, beam.transform.position, Quaternion.Euler(0, 0, 0));
                                        orb.transform.localScale = new Vector3(orb.transform.localScale.x * 0.9f, orb.transform.localScale.y * 0.9f, 1);
                                        orb.LocateMyFSM("Orb Control").SetState("Impact");
                                    }
                                }
                            }
                            if (BeamSightTime1 <= 0.2)
                            {
                                foreach (var beam in SightBeams)
                                {
                                    beam.LocateMyFSM("Control").SendEvent("END");
                                }
                                SightBeams.Clear();
                            }
                        }

                    }
                }
            }
        }
        public class BeamSweep : MonoBehaviour
        {
            public List<GameObject> BeamSweeping1 = new List<GameObject>();
            public float BeamSweepTime1;
            public float BeamSweepTimeLimit1;
            public bool BeamSweepSummoned1 = false;
            public bool BeamSweepFired1 = false;
            public bool BeamSweepFired2 = false;
            public bool BeamSweepFired3 = false;
            public bool BeamSweepEnded1 = false;
            public bool BeamSweepEnded2 = false;
            public bool BeamSweepEnded3 = false;
            public float BeamAngle = 0;
            public float BeamAngleAdd = 0;
            public float BeamAngleAdding = 0;
            public float BeamAngleLast = 0;
            public void Tri()
            {
                BeamSweepSummoned1 = false;
                BeamSweepFired1 = false;
                BeamSweepFired2 = false;
                BeamSweepFired3 = false;
                BeamSweepEnded1 = false;
                BeamSweepEnded2 = false;
                BeamSweepEnded3 = false;
                BeamSweepTime1 = 2.3f;
                BeamSweepTimeLimit1 = 2.3f;
            }
            public void Reset()
            {
                BeamSweepTime1 = 0;
                BeamSweepTimeLimit1 = 0;
                BeamSweepSummoned1 = false;
                BeamSweepFired1 = false;
                BeamSweepFired2 = false;
                BeamSweepFired3 = false;
                BeamSweepEnded1 = false;
                BeamSweepEnded2 = false;
                BeamSweepEnded3 = false;
            }
            private void FixedUpdate()
            {
                if (BeamSweepTime1 > 0 && HardMode == 0)
                {
                    BeamSweepTime1 -= Time.deltaTime;

                    if (BeamSweepTime1 <= BeamSweepTimeLimit1)
                    {
                        foreach (var b in BeamSweeping1)
                        {
                            b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                            BeamAngle = b.transform.rotation.eulerAngles.z;
                            if (b.transform.rotation.eulerAngles.z < 90)
                            {
                                BeamAngle = b.transform.rotation.eulerAngles.z + 360f;
                            }
                            else
                            {
                                BeamAngle = b.transform.rotation.eulerAngles.z;
                            }

                            if (BossBehavior.Angle - BeamAngle > 180f || BossBehavior.Angle - BeamAngle < -180f)
                            {
                                if (BossBehavior.Angle - BeamAngle > 180f)
                                {
                                    BeamAngleAdding = (BossBehavior.Angle - BeamAngle - 360f) * 0.06f;
                                }
                                if (BossBehavior.Angle - BeamAngle < -180f)
                                {
                                    BeamAngleAdding = (BossBehavior.Angle - BeamAngle + 360f) * 0.06f;
                                }
                            }
                            else
                            {
                                BeamAngleAdding = (BossBehavior.Angle - BeamAngle) * 0.06f;
                            }
                            //BeamAngleAdding = (BossBehavior.Angle - BeamAngle) * 0.06f;
                            if (Math.Abs(BeamAngleAdd) < 1.4f || Math.Sign(BeamAngleAdd) != Math.Sign(BeamAngleAdding))
                                BeamAngleAdd += BeamAngleAdding;
                            b.transform.Rotate(0, 0, (BeamAngleAdding), Space.Self);
                        }
                        BeamSweepTimeLimit1 -= 0.02f;
                        if (BeamSweepSummoned1 == false)
                        {
                            BeamSweepSummoned1 = true;
                            BeamAngleAdd = 0;
                            BeamAngleAdding = 0;
                            var beam = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle));
                            beam.AddComponent<BeamWhite>();
                            beam.transform.localScale = new Vector3(beam.transform.localScale.x, beam.transform.localScale.y * 0.8f, beam.transform.localScale.z);
                            beam.SetActive(true);
                            beam.SetActiveChildren(true);
                            beam.LocateMyFSM("Control").SetState("Antic");
                            beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            BeamSweeping1.Add(beam);
                            AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                            BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                        }
                        
                        if (BeamSweepTime1 <= 1.6)
                        {
                            if (BeamSweepTime1 <= 1.6 && BeamSweepFired1 == false)
                            {
                                AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.0f);
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                    BossBehavior.BOSS.GetComponent<OrbFlash>().Tri();
                                    var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, 0));
                                    Pt.transform.eulerAngles = b.transform.eulerAngles + new Vector3(0, 0, -90f);
                                    Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().Play();
                                    Pt.AddComponent<PtRecycle>();
                                }
                                BeamSweepFired1 = true;
                            }
                            if (BeamSweepTime1 <= 1.5 && BeamSweepEnded1 == false)
                            {
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SetState("End");
                                    b.LocateMyFSM("Control").SetState("Antic");
                                    b.LocateMyFSM("Control").SendEvent("ANTIC");
                                }
                                AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                                BeamSweepEnded1 = true;
                            }
                            if (BeamSweepTime1 <= 0.9 && BeamSweepFired2 == false)
                            {
                                AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.0f);
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                    BossBehavior.BOSS.GetComponent<OrbFlash>().Tri();
                                    var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(b.transform.eulerAngles + new Vector3(0, 0, -90f)));
                                    //Pt.transform.eulerAngles = b.transform.eulerAngles + new Vector3(0, 0, -90f);
                                    Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().Play();
                                    Pt.AddComponent<PtRecycle>();
                                }
                                BeamSweepFired2 = true;
                            }
                            if (BeamSweepTime1 <= 0.8 && BeamSweepEnded2 == false)
                            {
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SetState("End");
                                    b.LocateMyFSM("Control").SetState("Antic");
                                    b.LocateMyFSM("Control").SendEvent("ANTIC");
                                }
                                AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                                BeamSweepEnded2 = true;
                            }
                            if (BeamSweepTime1 <= 0.2 && BeamSweepFired3 == false)
                            {
                                AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.0f);
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                    BossBehavior.BOSS.GetComponent<OrbFlash>().Tri();
                                    var Pt = GameObject.Instantiate(BossBehavior.DREAMPT, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(b.transform.eulerAngles + new Vector3(0, 0, -90f)));
                                    //Pt.transform.eulerAngles = b.transform.eulerAngles + new Vector3(0, 0, -90f);
                                    Pt.gameObject.GetComponent<ParticleSystem>().emissionRate = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().startSpeed = 130f;
                                    Pt.gameObject.GetComponent<ParticleSystem>().Play();
                                    Pt.AddComponent<PtRecycle>();
                                }
                                BeamSweepFired3 = true;
                            }
                            if (BeamSweepTime1 <= 0.1 && BeamSweepEnded3 == false)
                            {
                                foreach (var b in BeamSweeping1)
                                {
                                    b.AddComponent<PtRecycle>();
                                    BeamAngleLast = b.transform.rotation.z;
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                BeamSweeping1.Clear();
                                BeamSweepEnded3 = true;
                            }
                        }
                    }
                }
            }
        }
        public class SuperBeamCannon : MonoBehaviour
        {
            public List<GameObject> BeamSweeping = new List<GameObject>();
            public List<GameObject> BeamSweeping1 = new List<GameObject>();
            public List<GameObject> BeamSweeping2 = new List<GameObject>();
            public List<GameObject> BeamSweeping3 = new List<GameObject>();
            public List<GameObject> BeamSweeping4 = new List<GameObject>();
            public GameObject Pt1;
            public GameObject Pt2;
            public float BeamSweepTime1;
            public float BeamSweepTimeLimit1;
            public bool BeamSweepSummoned = false;
            public bool BeamSweepFired = false;
            public bool BeamSweepEnded = false;
            public float BeamAngle = 0;
            public float BeamAngleAdd = 0;
            public float BeamAngleAdding = 0;
            public float BeamL = 8f;
            public float BeamLMinus = 0.8f;
            public float RecoilAdd = 0f;
            public float RecoilAddX = 0f;
            public float RecoilAddY = 0f;
            public float RecoilAdding = 0.00058f;
            public float DefaultHaloColorA;
            public float HaloRotateAdd = 0f;
            public void Tri()
            {
                BeamSweepSummoned = false;
                BeamSweepFired = false;
                BeamSweepEnded = false;
                BeamSweepTime1 = 3f;
                BeamSweepTimeLimit1 = 3f;
            }
            public void Reset()
            {
                BeamSweepTime1 = 0;
                BeamSweepTimeLimit1 = 0;
                BeamSweepSummoned = false;
                BeamSweepFired = false;
                BeamSweepEnded = false;
            }
            private void Start()
            {
                if (HardMode == 0)
                {
                    Pt1 = GameObject.Instantiate(BossBehavior.DREAMPT, BossBehavior.BOSS.transform.position, Quaternion.Euler(0, 0, 0));
                    Pt1.gameObject.GetComponent<Transform>().localScale += new Vector3(3f, 10f, 0f);
                    Pt2 = GameObject.Instantiate(DREAMPTCHARG, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), new Quaternion());
                    Pt2.gameObject.SetActive(true);
                }
                else
                {
                    Pt1 = GameObject.Instantiate(BossBehavior.DREAMPT, BossBehavior.BOSS.transform.position, Quaternion.Euler(0, 0, 0));
                    Pt1.gameObject.GetComponent<Transform>().localScale += new Vector3(5f, 10f, 0f);
                    Pt2 = GameObject.Instantiate(DREAMPTCHARG, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), new Quaternion());
                    Pt2.gameObject.SetActive(true);
                }
                
            }
            private void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    Pt2.gameObject.GetComponent<ParticleSystem>().emissionRate = 200f;
                    Pt2.gameObject.GetComponent<ParticleSystem>().Play();
                    if (BeamSweepTime1 > 0)
                    {
                        BeamSweepTime1 -= Time.deltaTime;

                        if (BeamSweepSummoned == false)
                        {
                            BeamSweepSummoned = true;
                            BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinStart();
                            BeamL = 8f;
                            BeamLMinus = 0.4f;
                            BeamAngleAdd = 0;
                            BeamAngleAdding = 0;
                            RecoilAdding = 0.00062f;
                            var beam = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle));
                            var otherbeam1 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle + 50f));
                            var otherbeam2 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle - 50f));
                            var otherbeam3 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle + 100f));
                            var otherbeam4 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle - 100f));
                            beam.gameObject.AddComponent<BeamWhite>();
                            otherbeam1.gameObject.AddComponent<BeamWhite>();
                            otherbeam2.gameObject.AddComponent<BeamWhite>();
                            otherbeam3.gameObject.AddComponent<BeamWhite>();
                            otherbeam4.gameObject.AddComponent<BeamWhite>();
                            beam.gameObject.AddComponent<PtRecycle>();
                            otherbeam1.gameObject.AddComponent<PtRecycle>();
                            otherbeam2.gameObject.AddComponent<PtRecycle>();
                            otherbeam3.gameObject.AddComponent<PtRecycle>();
                            otherbeam4.gameObject.AddComponent<PtRecycle>();
                            beam.transform.localScale = new Vector3(beam.transform.localScale.x, BeamL, beam.transform.localScale.z);
                            beam.SetActive(true);
                            beam.SetActiveChildren(true);
                            beam.LocateMyFSM("Control").SetState("Antic");
                            beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam1.transform.localScale = new Vector3(otherbeam1.transform.localScale.x, 1.2f, otherbeam1.transform.localScale.z);
                            otherbeam1.SetActive(true);
                            otherbeam1.SetActiveChildren(true);
                            otherbeam1.LocateMyFSM("Control").SetState("Antic");
                            otherbeam1.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam2.transform.localScale = new Vector3(otherbeam2.transform.localScale.x, 1.2f, otherbeam2.transform.localScale.z);
                            otherbeam2.SetActive(true);
                            otherbeam2.SetActiveChildren(true);
                            otherbeam2.LocateMyFSM("Control").SetState("Antic");
                            otherbeam2.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam3.transform.localScale = new Vector3(otherbeam3.transform.localScale.x, 0.8f, otherbeam3.transform.localScale.z);
                            otherbeam3.SetActive(true);
                            otherbeam3.SetActiveChildren(true);
                            otherbeam3.LocateMyFSM("Control").SetState("Antic");
                            otherbeam3.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam4.transform.localScale = new Vector3(otherbeam4.transform.localScale.x, 0.8f, otherbeam4.transform.localScale.z);
                            otherbeam4.SetActive(true);
                            otherbeam4.SetActiveChildren(true);
                            otherbeam4.LocateMyFSM("Control").SetState("Antic");
                            otherbeam4.LocateMyFSM("Control").SendEvent("ANTIC");
                            BeamSweeping.Add(beam);
                            BeamSweeping1.Add(otherbeam1);
                            BeamSweeping2.Add(otherbeam2);
                            BeamSweeping3.Add(otherbeam3);
                            BeamSweeping4.Add(otherbeam4);
                            AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                            BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                        }
                        if (BeamSweepTime1 <= BeamSweepTimeLimit1)
                        {
                            BeamSweepTimeLimit1 -= 0.02f;
                            if (BeamSweepTime1 > 2)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                }
                                foreach (var b in BeamSweeping1)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, -1f, Space.Self);
                                }
                                foreach (var b in BeamSweeping2)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, 1f, Space.Self);
                                }
                                foreach (var b in BeamSweeping3)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, -2f, Space.Self);
                                }
                                foreach (var b in BeamSweeping4)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, 2f, Space.Self);
                                }
                                Pt2.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f);
                                Pt2.gameObject.GetComponent<ParticleSystem>().Play();
                                if (BeamSweepTime1 > 2.4)
                                    Pt2.gameObject.GetComponent<ParticleSystem>().enableEmission = true;
                                else
                                    Pt2.gameObject.GetComponent<ParticleSystem>().enableEmission = false;
                            }
                            if (BeamSweepTime1 <= 2)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    BeamL -= BeamLMinus;
                                    BeamLMinus /= 1.1f;
                                    b.transform.localScale = new Vector3(b.transform.localScale.x, BeamL, b.transform.localScale.z);
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    BeamAngle = b.transform.rotation.eulerAngles.z;

                                    if (b.transform.rotation.eulerAngles.z < 90)
                                    {
                                        BeamAngle = b.transform.rotation.eulerAngles.z + 360f;
                                    }
                                    else
                                    {
                                        BeamAngle = b.transform.rotation.eulerAngles.z;
                                    }
                                    if (BossBehavior.Angle - BeamAngle > 180f || BossBehavior.Angle - BeamAngle < -180f)
                                    {
                                        if (BossBehavior.Angle - BeamAngle > 180f)
                                        {
                                            BeamAngleAdding = (BossBehavior.Angle - BeamAngle - 360f) * 0.0012f;
                                        }
                                        if (BossBehavior.Angle - BeamAngle < -180f)
                                        {
                                            BeamAngleAdding = (BossBehavior.Angle - BeamAngle + 360f) * 0.0012f;
                                        }
                                    }
                                    else
                                    {
                                        BeamAngleAdding = (BossBehavior.Angle - BeamAngle) * 0.0012f;
                                    }
                                    if (Math.Abs(BeamAngleAdd) < 1.4f || Math.Sign(BeamAngleAdd) != Math.Sign(BeamAngleAdding))
                                        BeamAngleAdd += BeamAngleAdding - BeamAngleAdd/100;
                                    b.transform.Rotate(0, 0, (BeamAngleAdd), Space.Self);
                                    Pt1.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f);
                                    Pt1.transform.eulerAngles = b.transform.eulerAngles + new Vector3(0, 0, -90f);
                                    Pt1.gameObject.GetComponent<ParticleSystem>().emissionRate = 150f;
                                    Pt1.gameObject.GetComponent<ParticleSystem>().startSpeed = 200f;
                                    Pt1.gameObject.GetComponent<ParticleSystem>().Play();
                                }
                                if (BeamSweepFired == false)
                                {
                                    RecoilAdd = 0f;
                                    AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.4f);
                                    foreach (var b in BeamSweeping)
                                    {
                                        BossBehavior.BOSS.GetComponent<OrbFlash>().Tri2();
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                    }
                                    foreach (var b in BeamSweeping1)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping2)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping3)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping4)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    BeamSweepFired = true;
                                }
                                if (BeamSweepTime1 > 0.5)
                                {
                                    RecoilAdd += RecoilAdding;
                                    RecoilAddX = (float)Math.Cos(DegreesToRadians(BeamAngle + 180)) * RecoilAdd;
                                    RecoilAddY = (float)Math.Sin(DegreesToRadians(BeamAngle + 180)) * RecoilAdd;
                                    BossBehavior.BOSS.transform.position = BossBehavior.BOSS.transform.position + new Vector3(RecoilAddX, RecoilAddY, 0);
                                }
                                else
                                {
                                    RecoilAddX /= 1.16f;
                                    RecoilAddY /= 1.16f;
                                    BossBehavior.BOSS.transform.position = BossBehavior.BOSS.transform.position + new Vector3(RecoilAddX, RecoilAddY, 0);

                                }
                            }
                            if (BeamSweepTime1 <= 0.2 && BeamSweepEnded == false)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                BeamSweepEnded = true;
                                BeamSweeping.Clear();
                                BeamSweeping1.Clear();
                                BeamSweeping2.Clear();
                                BeamSweeping3.Clear();
                                BeamSweeping4.Clear();

                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinEnd();
                            }
                        }

                    }
                }
                else
                {
                    Pt2.gameObject.GetComponent<ParticleSystem>().emissionRate = 400f;
                    Pt2.gameObject.GetComponent<ParticleSystem>().Play();
                    if (BeamSweepTime1 > 0)
                    {
                        BeamSweepTime1 -= Time.deltaTime;

                        if (BeamSweepSummoned == false)
                        {
                            BeamSweepSummoned = true;
                            BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinStart();
                            BossBehavior.BOSS.GetComponent<HaloExpand>().HaloExpandStart();
                            BeamL = 11f;
                            BeamLMinus = 0.5f;
                            BeamAngleAdd = 0;
                            BeamAngleAdding = 0;
                            RecoilAdding = 0.00064f;
                            var beam = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle));
                            var otherbeam1 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle + 50f));
                            var otherbeam2 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle - 50f));
                            var otherbeam3 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle + 100f));
                            var otherbeam4 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, 0, BossBehavior.Angle - 100f));
                            beam.gameObject.AddComponent<BeamWhite>();
                            otherbeam1.gameObject.AddComponent<BeamWhite>();
                            otherbeam2.gameObject.AddComponent<BeamWhite>();
                            otherbeam3.gameObject.AddComponent<BeamWhite>();
                            beam.gameObject.AddComponent<PtRecycle>();
                            otherbeam1.gameObject.AddComponent<PtRecycle>();
                            otherbeam2.gameObject.AddComponent<PtRecycle>();
                            otherbeam3.gameObject.AddComponent<PtRecycle>();
                            otherbeam4.gameObject.AddComponent<PtRecycle>();
                            otherbeam4.transform.localScale = new Vector3(beam.transform.localScale.x, BeamL, beam.transform.localScale.z);
                            beam.SetActive(true);
                            beam.SetActiveChildren(true);
                            beam.LocateMyFSM("Control").SetState("Antic");
                            beam.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam1.transform.localScale = new Vector3(otherbeam1.transform.localScale.x, 1.2f, otherbeam1.transform.localScale.z);
                            otherbeam1.SetActive(true);
                            otherbeam1.SetActiveChildren(true);
                            otherbeam1.LocateMyFSM("Control").SetState("Antic");
                            otherbeam1.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam2.transform.localScale = new Vector3(otherbeam2.transform.localScale.x, 1.2f, otherbeam2.transform.localScale.z);
                            otherbeam2.SetActive(true);
                            otherbeam2.SetActiveChildren(true);
                            otherbeam2.LocateMyFSM("Control").SetState("Antic");
                            otherbeam2.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam3.transform.localScale = new Vector3(otherbeam3.transform.localScale.x, 0.8f, otherbeam3.transform.localScale.z);
                            otherbeam3.SetActive(true);
                            otherbeam3.SetActiveChildren(true);
                            otherbeam3.LocateMyFSM("Control").SetState("Antic");
                            otherbeam3.LocateMyFSM("Control").SendEvent("ANTIC");
                            otherbeam4.transform.localScale = new Vector3(otherbeam4.transform.localScale.x, 0.8f, otherbeam4.transform.localScale.z);
                            otherbeam4.SetActive(true);
                            otherbeam4.SetActiveChildren(true);
                            otherbeam4.LocateMyFSM("Control").SetState("Antic");
                            otherbeam4.LocateMyFSM("Control").SendEvent("ANTIC");
                            BeamSweeping.Add(beam);
                            BeamSweeping1.Add(otherbeam1);
                            BeamSweeping2.Add(otherbeam2);
                            BeamSweeping3.Add(otherbeam3);
                            BeamSweeping4.Add(otherbeam4);
                            AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                            BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                        }
                        if (BeamSweepTime1 <= BeamSweepTimeLimit1)
                        {
                            BeamSweepTimeLimit1 -= 0.02f;
                            if (BeamSweepTime1 > 2)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                }
                                foreach (var b in BeamSweeping1)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, -1f, Space.Self);
                                }
                                foreach (var b in BeamSweeping2)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, 1f, Space.Self);
                                }
                                foreach (var b in BeamSweeping3)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, -2f, Space.Self);
                                }
                                foreach (var b in BeamSweeping4)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.transform.Rotate(0, 0, 2f, Space.Self);
                                }
                                Pt2.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f);
                                Pt2.gameObject.GetComponent<ParticleSystem>().Play();
                                if (BeamSweepTime1 > 2.4)
                                    Pt2.gameObject.GetComponent<ParticleSystem>().enableEmission = true;
                                else
                                    Pt2.gameObject.GetComponent<ParticleSystem>().enableEmission = false;
                            }
                            if (BeamSweepTime1 <= 2)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    BeamL -= BeamLMinus;
                                    BeamLMinus /= 1.1f;
                                    b.transform.localScale = new Vector3(b.transform.localScale.x, BeamL, b.transform.localScale.z);
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    BeamAngle = b.transform.rotation.eulerAngles.z;

                                    if (b.transform.rotation.eulerAngles.z < 90)
                                    {
                                        BeamAngle = b.transform.rotation.eulerAngles.z + 360f;
                                    }
                                    else
                                    {
                                        BeamAngle = b.transform.rotation.eulerAngles.z;
                                    }
                                    if (BossBehavior.Angle - BeamAngle > 180f || BossBehavior.Angle - BeamAngle < -180f)
                                    {
                                        if (BossBehavior.Angle - BeamAngle > 180f)
                                        {
                                            BeamAngleAdding = (BossBehavior.Angle - BeamAngle - 360f) * 0.0012f;
                                        }
                                        if (BossBehavior.Angle - BeamAngle < -180f)
                                        {
                                            BeamAngleAdding = (BossBehavior.Angle - BeamAngle + 360f) * 0.0012f;
                                        }
                                    }
                                    else
                                    {
                                        BeamAngleAdding = (BossBehavior.Angle - BeamAngle) * 0.0012f;
                                    }
                                    if (Math.Abs(BeamAngleAdd) < 1.4f || Math.Sign(BeamAngleAdd) != Math.Sign(BeamAngleAdding))
                                        BeamAngleAdd += BeamAngleAdding - BeamAngleAdd / 100;
                                    b.transform.Rotate(0, 0, (BeamAngleAdd), Space.Self);
                                    Pt1.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f);
                                    Pt1.transform.eulerAngles = b.transform.eulerAngles + new Vector3(0, 0, -90f);
                                    Pt1.gameObject.GetComponent<ParticleSystem>().emissionRate = 150f;
                                    Pt1.gameObject.GetComponent<ParticleSystem>().startSpeed = 200f;
                                    Pt1.gameObject.GetComponent<ParticleSystem>().Play();
                                }
                                if (BeamSweepFired == false)
                                {
                                    RecoilAdd = 0f;
                                    AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.4f);
                                    foreach (var b in BeamSweeping)
                                    {
                                        BossBehavior.BOSS.GetComponent<OrbFlash>().Tri2();
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                    }
                                    foreach (var b in BeamSweeping1)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping2)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping3)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    foreach (var b in BeamSweeping4)
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                        b.LocateMyFSM("Control").SendEvent("END");
                                    }
                                    BeamSweepFired = true;
                                }
                                if (BeamSweepTime1 > 0.5)
                                {
                                    RecoilAdd += RecoilAdding;
                                    RecoilAddX = (float)Math.Cos(DegreesToRadians(BeamAngle + 180)) * RecoilAdd;
                                    RecoilAddY = (float)Math.Sin(DegreesToRadians(BeamAngle + 180)) * RecoilAdd;
                                    BossBehavior.BOSS.transform.position = BossBehavior.BOSS.transform.position + new Vector3(RecoilAddX, RecoilAddY, 0);
                                }
                                else
                                {
                                    RecoilAddX /= 1.16f;
                                    RecoilAddY /= 1.16f;
                                    BossBehavior.BOSS.transform.position = BossBehavior.BOSS.transform.position + new Vector3(RecoilAddX, RecoilAddY, 0);

                                }
                            }
                            if (BeamSweepTime1 <= 0.2 && BeamSweepEnded == false)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                BeamSweepEnded = true;
                                BeamSweeping.Clear();
                                BeamSweeping1.Clear();
                                BeamSweeping2.Clear();
                                BeamSweeping3.Clear();
                                BeamSweeping4.Clear();

                                BossBehavior.BOSS.GetComponent<HaloRotate>().HaloSpinEnd();
                                BossBehavior.BOSS.GetComponent<HaloExpand>().HaloExpandEnd();
                            }
                        }

                    }
                }
            }
        }
        public class BeamCannon : MonoBehaviour
        {
            public List<GameObject> BeamSweeping = new List<GameObject>();
            public List<GameObject> BeamSweeping1 = new List<GameObject>();
            public List<GameObject> BeamSweeping2 = new List<GameObject>();
            public float BeamSweepTime1;
            public float BeamSweepTimeLimit1;
            public bool BeamSweepSummoned = false;
            public bool BeamSweepFired = false;
            public bool BeamSweepEnded = false;
            public float BeamAngle = 0;
            public float BeamAngleAdd = 0;
            public float BeamAngleAdding = 0;
            public float BeamL = 5f;
            public float BeamLMinus = 0.2f;
            public void Tri()
            {
                if (BeamSweepTime1 <= 0)
                {
                    BeamSweepSummoned = false;
                    BeamSweepFired = false;
                    BeamSweepEnded = false;
                    BeamSweepTime1 = 2.5f;
                    BeamSweepTimeLimit1 = 2.5f;
                }
            }
            public void Reset()
            {
                BeamSweepTime1 = 0;
                BeamSweepTimeLimit1 = 0;
                BeamSweepSummoned = false;
                BeamSweepFired = false;
                BeamSweepEnded = false;
            }
            private void FixedUpdate()
            {
                if (BeamSweepTime1 > 0)
                {
                    BeamSweepTime1 -= Time.deltaTime;

                    if (BeamSweepSummoned == false)
                    {
                        BeamSweepSummoned = true;
                        BeamAngleAdd = 0;
                        BeamAngleAdding = 0;
                        BeamL = 5f;
                        BeamLMinus = 0.25f;
                        var beam = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0,2.5f,0), Quaternion.Euler(0, 0, BossBehavior.Angle));
                        var otherbeam1 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0,2.5f,0), Quaternion.Euler(0, 0, BossBehavior.Angle + 15f));
                        var otherbeam2 = GameObject.Instantiate(BEAM, BossBehavior.BOSS.transform.position + new Vector3(0,2.5f,0), Quaternion.Euler(0, 0, BossBehavior.Angle - 15f));
                        beam.transform.localScale = new Vector3(otherbeam1.transform.localScale.x * 2, BeamL, otherbeam1.transform.localScale.z);

                        beam.gameObject.AddComponent<PtRecycle>();
                        otherbeam1.gameObject.AddComponent<PtRecycle>();
                        otherbeam2.gameObject.AddComponent<PtRecycle>();
                        beam.gameObject.AddComponent<BeamWhite>();
                        otherbeam1.gameObject.AddComponent<BeamWhite>();
                        otherbeam2.gameObject.AddComponent<BeamWhite>();
                        beam.SetActive(true);
                        beam.SetActiveChildren(true);
                        beam.LocateMyFSM("Control").SetState("Antic");
                        beam.LocateMyFSM("Control").SendEvent("ANTIC");
                        otherbeam1.transform.localScale = new Vector3(otherbeam1.transform.localScale.x * 2, 1, otherbeam1.transform.localScale.z);
                        otherbeam1.SetActive(true);
                        otherbeam1.SetActiveChildren(true);
                        otherbeam1.LocateMyFSM("Control").SetState("Antic");
                        otherbeam1.LocateMyFSM("Control").SendEvent("ANTIC");
                        otherbeam2.transform.localScale = new Vector3(otherbeam2.transform.localScale.x * 2, 1, otherbeam2.transform.localScale.z);
                        otherbeam2.SetActive(true);
                        otherbeam2.SetActiveChildren(true);
                        otherbeam2.LocateMyFSM("Control").SetState("Antic");
                        otherbeam2.LocateMyFSM("Control").SendEvent("ANTIC");
                        BeamSweeping.Add(beam);
                        BeamSweeping1.Add(otherbeam1);
                        BeamSweeping2.Add(otherbeam2);
                        AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                    }
                    if (BeamSweepTime1 <= BeamSweepTimeLimit1)
                    {
                        BeamSweepTimeLimit1 -= 0.02f;
                        if (BeamSweepTime1 > 1.5)
                        {
                            foreach (var b in BeamSweeping)
                            {
                                b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                            }
                            foreach (var b in BeamSweeping1)
                            {
                                b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                b.transform.Rotate(0, 0, -0.3f, Space.Self);
                            }
                            foreach (var b in BeamSweeping2)
                            {
                                b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                b.transform.Rotate(0, 0, 0.3f, Space.Self);
                            }
                        }
                        if (BeamSweepTime1 <= 1.5)
                        {
                            foreach (var b in BeamSweeping)
                            {
                                BeamL -= BeamLMinus;
                                BeamLMinus /= 1.1f;
                                b.transform.localScale = new Vector3(b.transform.localScale.x, BeamL, b.transform.localScale.z);
                                b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                BeamAngle = b.transform.rotation.eulerAngles.z;
                                if (b.transform.rotation.eulerAngles.z < 90)
                                {
                                    BeamAngle = b.transform.rotation.eulerAngles.z + 360f;
                                }
                                else
                                {
                                    BeamAngle = b.transform.rotation.eulerAngles.z;
                                }
                                BeamAngleAdding = (BossBehavior.Angle - BeamAngle) * 0.00117f * 50f/ (BossBehavior.Distance + 50f);
                                if (Math.Abs(BeamAngleAdd) < 1.4f || Math.Sign(BeamAngleAdd) != Math.Sign(BeamAngleAdding)) 
                                    BeamAngleAdd += BeamAngleAdding;
                                b.transform.Rotate(0, 0, (BeamAngleAdd), Space.Self);
                            }
                            if (BeamSweepFired == false)
                            {
                                foreach (var b in BeamSweeping)
                                {
                                    AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1.2f);
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                }
                                foreach (var b in BeamSweeping1)
                                {
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                foreach (var b in BeamSweeping2)
                                {
                                    b.LocateMyFSM("Control").SendEvent("FIRE");
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                BeamSweepFired = true;
                            }

                        }
                        if (BeamSweepTime1 <= 0.2 && BeamSweepEnded == false)
                        {
                            foreach (var b in BeamSweeping)
                            {
                                b.transform.position = BossBehavior.BOSS.transform.position + new Vector3(0, 2.5f, 0);
                                b.LocateMyFSM("Control").SendEvent("END");
                            }
                            BeamSweepEnded = true;
                            BeamSweeping.Clear();
                            BeamSweeping1.Clear();
                            BeamSweeping2.Clear();
                        }
                    }
                    
                }
            }

        }
        public class BeamSkill1 : MonoBehaviour
        {
            public List<GameObject> beams = new List<GameObject>();
            public float beamtime1;
            public float beamtimelimit1;
            public float Factor = 1.2f;
            public bool BeamSummoned;
            public bool BeamRepeated1;
            public bool BeamRepeated2;
            public bool BeamFired = false;
            public bool BeamEnded = false;
            public void Tri()
            {
                BeamSummoned = false;
                BeamRepeated1 = false;
                BeamRepeated2 = false;
                beamtime1 = 1f;
                beamtimelimit1 = 1f;
                Factor = 2f;
            }
            public void Reset()
            {
                beamtime1 = 0;
                beamtimelimit1 = 0;
                BeamSummoned = false;
                BeamRepeated1 = false;
                BeamRepeated2 = false;
                BeamFired = false;
                BeamEnded = false;
            }
            public void FixedUpdate()
            {
                if (HardMode == 0)
                {
                    if (beamtime1 > 0)
                    {
                        beamtime1 -= Time.deltaTime;

                        if (beamtime1 <= beamtimelimit1)
                        {
                            beamtimelimit1 -= 0.02f;
                            if (BeamSummoned == false)
                            {
                                BeamSummoned = true;
                                var beam0 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 - 8 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam1 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 - 6 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam2 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 - 4 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam3 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 - 2 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam4 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 - 0 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam5 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 + 2 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam6 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 + 4 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam7 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 + 6 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam8 = GameObject.Instantiate(BEAM, new Vector3(BossBehavior.BOSS.transform.position.x + BossBehavior.RX / 2 + 8 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                beams.Add(beam0);
                                beams.Add(beam1);
                                beams.Add(beam2);
                                beams.Add(beam3);
                                beams.Add(beam4);
                                beams.Add(beam5);
                                beams.Add(beam6);
                                beams.Add(beam7);
                                beams.Add(beam8);
                                foreach (var beam in beams)
                                {
                                    beam.AddComponent<PtRecycle>();
                                    beam.AddComponent<BeamWhite>();
                                    beam.SetActive(true);
                                    beam.SetActiveChildren(true);
                                    beam.LocateMyFSM("Control").SetState("Antic");
                                    beam.LocateMyFSM("Control").SendEvent("ANTIC");
                                    if (BeamRepeated2 == true)
                                    {
                                        beam.gameObject.transform.localScale = new Vector3(beam.gameObject.transform.localScale.x * 2.5f, beam.gameObject.transform.localScale.y * 2.5f, beam.gameObject.transform.localScale.z);
                                    }
                                    if (BeamRepeated2 == true)
                                    {
                                        var orb1 = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f), new Quaternion(0, 0, 0, 0));
                                        orb1.transform.localScale += new Vector3(5.5f, 5.5f, 0f);
                                        orb1.gameObject.AddComponent<OrbImpact>();
                                    }
                                }
                                AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                                Factor -= 0.6f;
                                BeamFired = false;
                            }
                            if (beamtimelimit1 <= 0.25 && beamtimelimit1 > 0.05)
                            {
                                foreach (var b in beams)
                                {
                                    if (b.LocateMyFSM("Control").ActiveStateName == "Antic")
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                    }
                                }
                                if (BeamFired == false)
                                {
                                    BeamFired = true;
                                    AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;

                                    BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1f);
                                }
                            }
                            if (beamtimelimit1 <= 0.1)
                            {
                                foreach (var b in beams)
                                {
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                beams.Clear();

                                if (BeamRepeated2 == false && BeamRepeated1 == true)
                                {
                                    beamtime1 = 1.25f;
                                    beamtimelimit1 = 1.25f;
                                    BeamRepeated2 = true;
                                    BeamSummoned = false;
                                }
                                if (BeamRepeated2 == false && BeamRepeated1 == false)
                                {
                                    beamtime1 = 0.95f;
                                    beamtimelimit1 = 0.95f;
                                    BeamRepeated1 = true;
                                    BeamSummoned = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        beams.Clear();
                    }
                }
                else
                {
                    if (beamtime1 > 0)
                    {
                        beamtime1 -= Time.deltaTime;

                        if (beamtime1 <= beamtimelimit1)
                        {
                            beamtimelimit1 -= 0.02f;
                            if (BeamSummoned == false)
                            {
                                BeamSummoned = true;

                                if (BeamRepeated2 == true)
                                    Factor = 1.6f;
                                else
                                    Factor = 1.3f;
                                var beam0 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX - 8 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam1 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX - 6 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam2 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX - 4 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam3 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX - 2 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam4 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX - 0 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam5 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX + 2 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam6 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX + 4 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam7 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX + 6 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                var beam8 = GameObject.Instantiate(BEAM, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX + 8 * Factor, 20, 0), Quaternion.Euler(0, 0, 90));
                                beams.Add(beam0);
                                beams.Add(beam1);
                                beams.Add(beam2);
                                beams.Add(beam3);
                                beams.Add(beam4);
                                beams.Add(beam5);
                                beams.Add(beam6);
                                beams.Add(beam7);
                                beams.Add(beam8);
                                foreach (var beam in beams)
                                {
                                    beam.AddComponent<PtRecycle>();
                                    beam.AddComponent<BeamWhite>();
                                    beam.SetActive(true);
                                    beam.SetActiveChildren(true);
                                    beam.LocateMyFSM("Control").SetState("Antic");
                                    beam.LocateMyFSM("Control").SendEvent("ANTIC");
                                    if (BeamRepeated2 == true)
                                    {
                                        beam.gameObject.transform.localScale = new Vector3(beam.gameObject.transform.localScale.x * 2.5f, beam.gameObject.transform.localScale.y * 2.5f, beam.gameObject.transform.localScale.z);
                                    }
                                    if (BeamRepeated2 == true)
                                    {
                                        var orb1 = GameObject.Instantiate(BossBehavior.ORB, BossBehavior.BOSS.transform.position + new Vector3(0f, 2.5f, 0f), new Quaternion(0, 0, 0, 0));
                                        orb1.transform.localScale += new Vector3(5.5f, 5.5f, 0f);
                                        orb1.gameObject.AddComponent<OrbImpact>();
                                    }
                                }
                                AudioClip Antic = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
                                BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Antic, 1.0f);
                                BeamFired = false;
                            }
                            if (beamtimelimit1 <= 0.25 && beamtimelimit1 > 0.05)
                            {
                                foreach (var b in beams)
                                {
                                    if (b.LocateMyFSM("Control").ActiveStateName == "Antic")
                                    {
                                        b.LocateMyFSM("Control").SendEvent("FIRE");
                                    }
                                }
                                if (BeamFired == false)
                                {
                                    BeamFired = true;
                                    AudioClip Fire = BossBehavior.BOSS.LocateMyFSM("Attack Commands").GetState("EB 1").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
                                    if (BeamRepeated2 == false)
                                    {
                                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1f);
                                    }
                                    else
                                    {
                                        BossBehavior.BOSS.GetComponent<AudioSource>().PlayOneShot(Fire, 1f);
                                    }
                                }
                            }
                            if (beamtimelimit1 <= 0.05)
                            {
                                foreach (var b in beams)
                                {
                                    b.LocateMyFSM("Control").SendEvent("END");
                                }
                                beams.Clear();

                                if (BeamRepeated2 == false && BeamRepeated1 == true)
                                {
                                    beamtime1 = 1.25f;
                                    beamtimelimit1 = 1.25f;
                                    BeamRepeated2 = true;
                                    BeamSummoned = false;
                                }
                                if (BeamRepeated2 == false && BeamRepeated1 == false)
                                {
                                    beamtime1 = 0.95f;
                                    beamtimelimit1 = 0.95f;
                                    BeamRepeated1 = true;
                                    BeamSummoned = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        beams.Clear();
                    }
                }
            }
        }
        public class BossBehavior : MonoBehaviour
        {
            public List<GameObject> beams = new List<GameObject>();
            public List<GameObject> nails = new List<GameObject>();

            //GameObject变量
            public static GameObject BOSS;
            public static GameObject NAIL;
            public static GameObject ORB;
            public static GameObject DREAMPT;
            public static GameObject DREAMPT2;

            static System.Random random = new System.Random();

            //随机数组件 RX与RY为-1~1的f随机数且相互独立
            static double RX0 => random.NextDouble();
            static double RY0 => random.NextDouble();
             public static double R1 => random.NextDouble();
            public static float SIGHX => (random.Next(0, 2) * 2 - 1);
            public static float SIGHY => (random.Next(0, 2) * 2 - 1);
            public static float RX => (float)(RX0 * SIGHX);
            public static float RY => (float)(RY0 * SIGHY);
            public static float RX1 ;
            public static float RY1 ;

            //其他变量

            public static bool BeamRepeated1 = false;
            public static bool BeamRepeated2 = false;
            public static bool DashUp = false;
            public static float orbtime1 = 0;
            public static float nailtime1 = 0;
            public static float beamtime1 = 0;
            public static float dashtime1 = 0;
            public static float orbtimelimit1 = 0;
            public static float nailtimelimit1 = 0;
            public static float beamtimelimit1 = 0;
            public static float dashtimelimit1 = 0;
            public static float NailRotation = 180;
            public static float Angle;
            public static float DashX;
            public static float DashY;
            public static int ADDANGLE = 0;
            public static float Distance = 0;
            public static float X = 0;
            public static float Y = 0;
            public void Start()
            {
                BOSS.gameObject.AddComponent<HaloRotate>();
                BOSS.gameObject.AddComponent<HaloExpand>();
                BOSS.gameObject.transform.parent.gameObject.LocateMyFSM("Control").GetState("Battle Start").AddMethod(()=> 
                {
                    BOSS.GetComponent<HealthManager>().hp = 5500;
                    BOSS.LocateMyFSM("Phase Control").GetState("Check 1").GetAction<IntCompare>().integer2.RawValue = 4500;
                    BOSS.LocateMyFSM("Phase Control").GetState("Check 2").GetAction<IntCompare>().integer2.RawValue = 3700;
                    BOSS.LocateMyFSM("Phase Control").GetState("Check 3").GetAction<IntCompare>().integer2.RawValue = 3000;
                    BOSS.LocateMyFSM("Phase Control").GetState("Check 4").GetAction<IntCompare>().integer2.RawValue = 1000;
                });
            }
            private void FixedUpdate()
            {
                if (Math.Sign(BOSS.transform.position.x - HeroController.instance.gameObject.transform.position.x) < 0)
                {
                    ADDANGLE = 180;
                }
                else
                {
                    ADDANGLE = 0;
                }
                Angle = (float)RadiansToDegrees(Math.Atan((BOSS.transform.position.y - HeroController.instance.gameObject.transform.position.y + 2.5)/(BOSS.transform.position.x - HeroController.instance.gameObject.transform.position.x)))+ADDANGLE + 180;
                NailRotation = Angle + RX * 30;
                X = BOSS.transform.position.x - HeroController.instance.gameObject.transform.position.x;
                Y = BOSS.transform.position.y - HeroController.instance.gameObject.transform.position.y;
                Distance = (float)Math.Sqrt(X * X + Y * Y);
                if (dashtime1 >= 0)
                {
                    if (dashtime1 == 1)
                    {
                        if (DashUp == false)
                        {
                            DashX = 0.8f / (2 + Distance * Distance / 4) * X;
                            DashY = 0.8f / (2 + Distance * Distance / 4) * Y;
                        }
                        if (DashUp == true)
                        {
                            DashX = 0.8f / (2 + Distance * Distance / 4) * X;
                            DashY = 0.5f;
                            DashUp = false;
                        }
                    }
                    if (dashtime1 <= dashtimelimit1)
                    {
                        dashtimelimit1 -= 0.02f;
                        BOSS.transform.position += new Vector3(DashX, DashY, 0);
                        DashX /= 1.15f;
                        DashY /= 1.15f;
                    }
                    dashtime1 -= Time.deltaTime;
                }
            }
         }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Workshop","GG_Statue_Radiance/Spotlight/Glow Response statue_beam/light_beam_particles 3"),
                ("GG_Radiance","Boss Control"),
                ("GG_Soul_Master","Mage Lord")
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var radiance = preloadedObjects["GG_Radiance"]["Boss Control"].transform.Find("Absolute Radiance").gameObject;
            var burst = radiance.transform.Find("Eye Beam Glow").gameObject.transform.Find("Burst 1").gameObject;
            BEAM = burst.transform.Find("Radiant Beam").gameObject;
            BossBehavior.DREAMPT = radiance.transform.Find("Eye Final Pt").gameObject;

            var MageLord = preloadedObjects["GG_Soul_Master"]["Mage Lord"].gameObject;
            var audioclip1 = MageLord.LocateMyFSM("Mage Lord").GetState("HS Ret Left").GetAction<AudioPlaySimple>().oneShotClip.Value as AudioClip;
            var audioclip2 = MageLord.LocateMyFSM("Mage Lord").GetState("Teleport").GetAction<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
            NAILSHOT = audioclip1;
            NAILCHARGE = audioclip2;

            var Pt1 = radiance.transform.Find("Pt Tele Out").gameObject;
            Pt1.gameObject.GetComponent<Transform>().localScale = new Vector3(6f, 6f, 1.3954f);
            Pt1.gameObject.GetComponent<ParticleSystem>().emissionRate = 10;
            Pt1.gameObject.GetComponent<ParticleSystem>().maxParticles = 9999;
            Pt1.gameObject.GetComponent<ParticleSystem>().startLifetime = 1f;
            Pt1.gameObject.GetComponent<ParticleSystem>().startSize = 3f;
            Pt1.gameObject.GetComponent<ParticleSystem>().startSpeed = 0f;
            ORBBLASTPT = Pt1;

            var Pt2 = radiance.transform.Find("Shot Charge").gameObject;
            Pt2.transform.GetComponent<ParticleSystem>().emissionRate = 180f;
            Pt2.transform.GetComponent<ParticleSystem>().maxParticles = 9999;
            Pt2.transform.GetComponent<ParticleSystem>().startSpeed = -24f;
            Pt2.transform.GetComponent<ParticleSystem>().startColor = new Color(1f, 0.9f, 0.73f, 1f);
            Pt2.transform.GetComponent<ParticleSystem>().startSize = 1.5f;
            Pt2.transform.GetComponent<Behaviour>().enabled = true;
            Pt2.transform.localPosition = new Vector3(0, 0, 0);
            Pt2.transform.localScale = new Vector3(3f, 3f, Pt2.transform.localScale.z);
            DREAMPTCHARG = Pt2;

            var Pt3 = radiance.gameObject.transform.Find("Eye Final Pt").gameObject;
            Pt3.transform.GetComponent<ParticleSystem>().enableEmission = true;
            Pt3.transform.GetComponent<ParticleSystem>().emissionRate = 60f;
            Pt3.transform.GetComponent<ParticleSystem>().startLifetime = 1f;
            //Pt3.transform.GetComponent<ParticleSystem>().startColor = new Color(1, 1, 1, 1);
            Pt3.transform.GetComponent<ParticleSystem>().startSpeed = 60f;
            Pt3.transform.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);
            Pt3.transform.GetComponent<Transform>().localScale = new Vector3(0.2f, 0.3f, 1f);
            DREAMPTBLOCK = Pt3;


            ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            On.PlayMakerFSM.Start += PlayMakerFSM_Start;
            
        }
        public string ModHooks_LanguageGetHook(string key, string sheet, string text)
        {
            if (SUNSET.settings_.on == true)
            {
                if(HardMode == 0)
                {
                    if (key == "NAME_FINAL_BOSS")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "残阳";
                        }
                        else
                        {
                            text = "SUNSET";
                        }
                    }
                    if (key == "ABSOLUTE_RADIANCE_MAIN" && sheet == "Titles")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "残阳";
                        }
                        else
                        {
                            text = "SUNSET";
                        }
                    }
                    if (key == "ABSOLUTE_RADIANCE_SUPER" && sheet == "Titles")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "现世将尽";
                        }
                        else
                        {
                            text = "DOOMSDAY";
                        }
                    }
                    if (key == "GG_S_RADIANCE" && sheet == "CP3")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "旧时代湮灭的余烬";
                        }
                        else
                        {
                            text = "Ember of former era";
                        }
                    }
                    return text;
                }
                else
                {

                    if (key == "NAME_FINAL_BOSS")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "旧日·残阳";
                        }
                        else
                        {
                            text = "FORMER SUN";
                        }
                    }
                    if (key == "ABSOLUTE_RADIANCE_MAIN" && sheet == "Titles")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "旧日·残阳";
                        }
                        else
                        {
                            text = "FORMER SUN";
                        }
                    }
                    if (key == "ABSOLUTE_RADIANCE_SUPER" && sheet == "Titles")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "万古伊始";
                        }
                        else
                        {
                            text = "BEGINNING OF ETERNITY";
                        }
                    }
                    if (key == "GG_S_RADIANCE" && sheet == "CP3")
                    {
                        if (Language.Language.CurrentLanguage() == Language.LanguageCode.ZH)
                        {
                            text = "铸造与毁灭旧时代之神";
                        }
                        else
                        {
                            text = "The god who forged and destroyed the former era";
                        }
                    }
                    return text;
                }
            }
            else
                return @text;
        }

        private void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            if (SUNSET.settings_.on == true)
            {
                if (self.gameObject.name == "Boss Control" && self.FsmName == "Control")
                {
                    self.GetState("Init").AddMethod(() =>
                    {
                        if (ChangeColor.Started1 == true)
                        {
                            Camera.GetComponent<ChangeColor>().ChangeStart();
                        }
                    });
                }
                if (self.gameObject.name == "Radiant Beam" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (1)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (2)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (3)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (4)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (5)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (6)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (7)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Radiant Beam (8)" && self.FsmName == "Control")
                {
                    self.Recycle();
                }
                if (self.gameObject.name == "Absolute Radiance" && self.FsmName == "Attack Choices")
                {
                    var Pt4 = self.gameObject.transform.Find("Shot Charge").gameObject;
                    Pt4.transform.GetComponent<ParticleSystem>().emissionRate = 180f;
                    Pt4.transform.GetComponent<ParticleSystem>().maxParticles = 9999;
                    Pt4.transform.GetComponent<ParticleSystem>().startSpeed = -24f;
                    Pt4.transform.GetComponent<ParticleSystem>().startColor = new Color(1f, 0.9f, 0.73f, 1f);
                    Pt4.transform.GetComponent<ParticleSystem>().startSize = 1.5f;
                    Pt4.transform.GetComponent<Behaviour>().enabled = true;
                    Pt4.transform.localPosition = new Vector3(0, 0, 0);
                    Pt4.transform.localScale = new Vector3(3f, 3f, Pt4.transform.localScale.z);
                    DREAMPTCHARG2 = Pt4;

                    self.gameObject.AddComponent<MoreNailSniper>();
                    self.gameObject.AddComponent<CombU>();
                    self.gameObject.AddComponent<CombD>();
                    self.GetState("Nail Top Sweep").GetAction<Wait>().time = 1f;
                    self.GetState("Beam Sweep R 2").RemoveAction<SendEventByName>();
                    self.GetState("Beam Sweep L 2").RemoveAction<SendEventByName>();
                    self.GetState("Nail L Sweep 2").GetAction<SendEventByName>(0).sendEvent.Clear();
                    self.GetState("Nail L Sweep 2").GetAction<SendEventByName>(1).sendEvent.Clear();
                    self.GetState("Nail L Sweep 2").GetAction<SendEventByName>(2).sendEvent.Clear();
                    self.GetState("Nail L Sweep 2").GetAction<SendEventByName>(3).sendEvent.Clear();
                    self.GetState("Nail R Sweep 2").GetAction<SendEventByName>(0).sendEvent.Clear();
                    self.GetState("Nail R Sweep 2").GetAction<SendEventByName>(1).sendEvent.Clear();
                    self.GetState("Nail R Sweep 2").GetAction<SendEventByName>(2).sendEvent.Clear();
                    self.GetState("Nail R Sweep 2").GetAction<SendEventByName>(3).sendEvent.Clear();
                    self.GetState("Nail L Sweep 2").AddMethod(() =>
                    {
                        self.GetComponent<MoreNailSniper>().Tri();
                    });
                    self.GetState("Nail R Sweep 2").AddMethod(() =>
                    {
                        self.GetComponent<MoreNailSniper>().Tri();
                    });

                    self.GetState("Beam Sweep L").RemoveAction<SendEventByName>();
                    self.GetState("Beam Sweep L").AddMethod(() =>
                    {
                        self.GetComponent<OrbBlast>().Tri();
                        self.GetComponent<OrbBlastPt>().Tri();
                        self.GetComponent<CombL>().Tri();
                    });
                    self.GetState("Beam Sweep R").RemoveAction<SendEventByName>();
                    self.GetState("Beam Sweep R").AddMethod(() =>
                    {
                        self.GetComponent<OrbBlast>().Tri();
                        self.GetComponent<OrbBlastPt>().Tri();
                        self.GetComponent<CombL>().Tri();
                    });
                    self.GetState("Beam Sweep L 2").AddMethod(() =>
                    {
                        self.GetComponent<Meteorolite>().Tri();
                        self.GetComponent<CombU>().Tri();
                    });
                    self.GetState("Beam Sweep R 2").AddMethod(() =>
                    {
                        self.GetComponent<Meteorolite>().Tri();
                        self.GetComponent<CombU>().Tri();
                    });
                    self.GetState("Nail L Sweep 2").AddMethod(() =>
                    {
                        self.GetComponent<BeamSight>().Tri();
                    });
                    self.GetState("Nail R Sweep 2").AddMethod(() =>
                    {
                        self.GetComponent<BeamSight>().Tri();
                    });
                }
                if (self.gameObject.name == "Absolute Radiance" && self.FsmName == "Control")
                {
                    self.GetState("Set Arena 1").AddMethod(() =>
                    {
                        Camera.GetComponent<Away>().enabled = true;
                    });
                    self.gameObject.AddComponent<NailCombRL>();
                    var RG = self.CopyState("Rage Comb", "RG");
                    RG.RemoveAction<SpawnObjectFromGlobalPool>();
                    self.ChangeTransition("Rage1 Start", "FINISHED", "RG");
                    self.ChangeTransition("Rage1 Loop", "FINISHED", "RG");
                    self.GetState("RG").AddMethod(() =>
                    {
                        self.gameObject.GetComponent<NailCombRL>().NailCombStart = true;
                    });
                    self.GetState("Stun1 Start").AddMethod(() =>
                    {
                        Camera.gameObject.GetComponent<ChangeColor>().Change2();
                        self.gameObject.GetComponent<NailCombRL>().NailCombStart = false;
                    });
                    self.GetState("Stun1 Out").AddMethod(() =>
                    {
                        var nail = GameObject.Instantiate(BossBehavior.NAIL, new Vector3(HeroController.instance.gameObject.transform.position.x + BossBehavior.RX * 15, HeroController.instance.gameObject.transform.position.y + 25f, 0), Quaternion.Euler(0, 0, 0));
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().emissionRate = 0;
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().startLifetime = 2f;
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().startSize = 0.8f;
                        nail.transform.Find("Idle Pt").GetComponent<ParticleSystem>().startSpeed = 0f;
                        nail.gameObject.AddComponent<AddGlowEffect>();
                        nail.gameObject.AddComponent<NailSniper>();
                        nail.gameObject.AddComponent<NailBlock>();
                        nail.gameObject.GetComponent<NailSniper>().Speed = 150f;
                        nail.SetActive(true);
                    });
                    self.GetState("Ascend Tele").AddMethod(() =>
                    {
                        Camera.gameObject.GetComponent<ChangeColor>().Change3();
                        self.GetComponent<P3BigOrb>().Tri();
                    });
                    self.GetState("Scream").GetAction<SetHP>().hp = 1720;
                    self.GetState("Scream").AddMethod(() =>
                    {
                        self.gameObject.AddComponent<FinalNailSniperSummon>();
                        self.gameObject.GetComponent<P3BigOrb>().Destory = true;
                        string nameToFind2 = "Radiant Beam(Clone)";
                        GameObject[] beams = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == nameToFind2).ToArray();
                        foreach (var b in beams)
                        {
                            b.LocateMyFSM("Control").SetState("Recycle");
                        }
                    });
                }
                if (self.gameObject.name == "Absolute Radiance" && self.FsmName == "Attack Commands")
                {
                    self.gameObject.AddComponent<BossBehavior>();
                    self.gameObject.AddComponent<BeamSweep>();
                    self.gameObject.AddComponent<BeamCannon>();
                    self.gameObject.AddComponent<SuperBeamCannon>();
                    self.gameObject.AddComponent<BeamSight>();
                    self.gameObject.AddComponent<OrbSkill1>();
                    self.gameObject.AddComponent<Nail1>();
                    self.gameObject.AddComponent<OrbBlast>();
                    self.gameObject.AddComponent<OrbFlash>();
                    self.gameObject.AddComponent<NailCombRL>();
                    self.gameObject.AddComponent<P3BigOrb>();
                    self.gameObject.AddComponent<OrbBlastPt>();
                    self.gameObject.AddComponent<BeamSkill1>();
                    //self.gameObject.AddComponent<TriangleBeam>();
                    self.gameObject.AddComponent<CombL>();
                    self.gameObject.AddComponent<CombR>();
                    self.gameObject.AddComponent<Meteorolite>();
                    BossBehavior.BOSS = self.gameObject;
                    BossBehavior.NAIL = self.GetState("CW Spawn").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value as GameObject;
                    BossBehavior.ORB = self.GetState("Spawn Fireball").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value as GameObject;

                    var orbstay1 = self.CopyState("Orb Summon", "OrbStay1");
                    var orbstay2 = self.CopyState("Orb Summon", "OrbStay2");
                    self.GetState("Comb Top 2").RemoveAction<SpawnObjectFromGlobalPool>();
                    self.GetState("Comb Top").RemoveAction<SpawnObjectFromGlobalPool>();

                    self.FsmVariables.FindFsmFloat("Rotation").Value = BossBehavior.NailRotation;

                    self.GetState("Orb Antic").AddMethod(() =>
                    {
                        self.GetComponent<OrbSkill1>().Tri();
                    });
                    self.ChangeTransition("Orb Summon", "FINISHED", "Orb End");
                    if (HardMode == 0)
                        OrbWaitTime = 2f;
                    else
                        OrbWaitTime = 3.3f;
                    self.GetState("Orb Summon").GetAction<Wait>().time = OrbWaitTime;
                    self.GetState("Orb Summon").GetAction<SetParticleEmission>(4).emission = false;
                    self.GetState("Orb Summon").GetAction<SetParticleEmission>(5).emission = false;
                    self.GetState("Orb Summon").RemoveAction<AudioPlayerOneShotSingle>();

                    self.ChangeTransition("NF Glow", "FINISHED", "EB Glow End");
                    self.GetState("NF Glow").GetAction<Wait>().time.Value = 3.3f;

                    self.GetState("CW Spawn").RemoveAction<SpawnObjectFromGlobalPool>();
                    self.GetState("CCW Spawn").RemoveAction<SpawnObjectFromGlobalPool>();
                    self.GetState("CW Restart").GetAction<Wait>().time.Value = 1f;
                    self.GetState("CCW Restart").GetAction<Wait>().time.Value = 1f;
                    if (HardMode == 1)
                        self.GetState("Nail Fan").GetAction<Wait>().time.Value = 3f;
                    self.GetState("Nail Fan").AddMethod(() =>
                    {
                        BossBehavior.dashtime1 = 1f;
                        BossBehavior.dashtimelimit1 = 1f;
                        BossBehavior.DashUp = true;
                        self.GetComponent<BeamSweep>().Tri();
                        self.GetComponent<Nail1>().Tri();
                    });
                    self.GetState("NF Glow").AddMethod(() =>
                    {
                        if (HeroController.instance.gameObject.transform.position.y - BossBehavior.BOSS.transform.position.y - 2.5f >= 0)
                        {
                            if (BossBehavior.RY >= -0.8f)
                            {
                                self.GetComponent<BeamSkill1>().Tri();
                            }
                            else
                            {
                                BossBehavior.dashtime1 = 1f;
                                BossBehavior.dashtimelimit1 = 1f;
                                self.GetComponent<SuperBeamCannon>().Tri();
                            }
                        }
                        else
                        {
                            if (BossBehavior.RY >= 0.4f)
                            {
                                self.GetComponent<BeamSkill1>().Tri();
                            }
                            else
                            {
                                BossBehavior.dashtime1 = 1f;
                                BossBehavior.dashtimelimit1 = 1f;
                                self.GetComponent<SuperBeamCannon>().Tri();
                            }
                        }
                    });
                    self.GetState("Comb L").RemoveAction<SpawnObjectFromGlobalPool>();
                    self.GetState("Comb L").AddMethod(() =>
                    {
                        self.GetComponent<CombL>().Tri();
                        self.GetComponent<BeamSight>().Tri();
                    });
                    self.GetState("Comb R").RemoveAction<SpawnObjectFromGlobalPool>();
                    self.GetState("Comb R").AddMethod(() =>
                    {
                        self.GetComponent<CombR>().Tri();
                        self.GetComponent<BeamSight>().Tri();
                    });
                    self.GetState("Comb Top").AddMethod(() =>
                    {
                        self.GetComponent<Meteorolite>().Tri();
                        self.GetComponent<CombU>().Tri();
                    });
                    var Phase3AB = self.CopyState("AB Start", "Phase3AB1");
                    self.ChangeTransition("AB Start", "FINISHED", "Phase3AB1");
                    self.ChangeTransition("Phase3AB1", "FINISHED", "AB Start");
                    self.GetState("AB Start").AddMethod(() =>
                    {
                        self.GetComponent<BeamCannon>().Tri();
                    });
                }
            }
        }
    }
}
