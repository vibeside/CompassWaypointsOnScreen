using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Compass;
using MelonLoader;
using System;
using UnityEngine;
using System.Collections.Generic;
using ScheduleOne.PlayerScripts;
using static ScheduleOne.UI.Compass.CompassManager;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using MonoMod.RuntimeDetour;
using ScheduleOne.Quests;
using System.Reflection;
using ScheduleOne.Map;

namespace CompassWaypointsOnScreen
{
    public class Plugin : MelonMod
    {

        //public static HarmonyLib.Harmony harmony;
        public static CompassManager manager;
        public static PlayerCamera cam;

        public static Sprite texture;
        public static GameObject prefab;

        public static Dictionary<Element, GameObject> dict = [];

        public static AssetBundle elementBundle;

        private static Canvas _overlayCanvas;

        public static Canvas GetOrCreateCanvas()
        {
            if (_overlayCanvas != null) return _overlayCanvas;

            GameObject go = new GameObject("OnScreenMarkers");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            go.AddComponent<CanvasScaler>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            _overlayCanvas = canvas;
            return canvas;
        }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            _overlayCanvas = GetOrCreateCanvas();
        }
        public override void OnInitializeMelon()
        {

            base.OnInitializeMelon();
            MelonLogger.Msg("Loaded!");
            //harmony = new HarmonyLib.Harmony("com.coolpaca.compasswaypoint");
            string sAssemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // These load correctly
            elementBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "onscreenelementprefab"));
            texture = LoadImage("DEAL.png");
            prefab = elementBundle.LoadAsset<GameObject>("Assets/OnScreenElementPrefab.prefab");
            new Hook(
                typeof(CompassManager).GetMethod("UpdateElement", (BindingFlags)~0),
                Plugin.UpdateElementHook
                );
        }
        public static void UpdateElementHook(
            Action<CompassManager, Element> orig,
            CompassManager self,
            Element element)
        {
            orig(self, element);
            if (cam?.Camera == null || element.Transform == null) return;
            if (!dict.ContainsKey(element))
            {
                dict.Add(element, GameObject.Instantiate(prefab, _overlayCanvas.transform));
                System.Random r = new System.Random(dict[element].GetInstanceID());
                dict[element].GetComponent<Image>().color = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
                dict[element].GetComponent<Image>().sprite = texture;
            }
            dict[element].SetActive(element.Visible);
            var screenPoint = cam.Camera.WorldToScreenPoint(element.Transform.position);

            if (screenPoint.z >= 0)
                dict[element].transform.position = screenPoint;
            else
                dict[element].SetActive(false);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (cam == null) cam = PlayerSingleton<PlayerCamera>.Instance;
        }
        public static Sprite LoadImage(string Name)
        {
            string imgPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + Name;
            byte[] imgData = File.ReadAllBytes(imgPath);
            Texture2D tex = new(2, 2);
            if (!tex.LoadImage(imgData))
            {
                MelonLogger.Error($"Failed to load image. Make sure its named \"{Name}\" exactly.");
                return null;
            }
            else
            {
                Sprite icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2);
                return icon;
                // set app icon
            }
        }
    }
}
