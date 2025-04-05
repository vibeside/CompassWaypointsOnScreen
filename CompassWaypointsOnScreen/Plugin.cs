using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Compass;
using MelonLoader;
using System;
using UnityEngine;
using AeLa.EasyFeedback.APIs;
using System.Collections.Generic;
using ScheduleOne.PlayerScripts;
using static ScheduleOne.UI.Compass.CompassManager;
using System.Collections.Specialized;
using System.IO;
using UnityEngine.UI;
using System.Linq;

namespace CompassWaypointsOnScreen
{
    public class Plugin : MelonMod
    {
        
        //public static HarmonyLib.Harmony harmony;
        public static CompassManager manager;
        public static PlayerCamera cam;

        public static Texture2D texture;

        public static Dictionary<CompassManager.Element,GameObject> dict = new Dictionary<CompassManager.Element,GameObject>();

        public static AssetBundle elementBundle;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("Loaded!");
            //harmony = new HarmonyLib.Harmony("com.coolpaca.compasswaypoint");
            string sAssemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            elementBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "onscreenelementprefab"));
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if(Singleton<CompassManager>.Instance != null) manager = Singleton<CompassManager>.Instance;
            if (PlayerSingleton<PlayerCamera>.Instance != null) cam = PlayerSingleton<PlayerCamera>.Instance;
            if (manager == null) return;
            if (cam == null) return;
            // for each KVP
            // if gameObject == null
            // make new one
            // but the list is populated elsewhere, so maybe two loops, first adds entries, second populates?
            foreach (CompassManager.Element e in manager.elements)
            {
                if (!dict.ContainsKey(e) && e.Visible)
                {
                    GameObject go = elementBundle.LoadAsset<GameObject>("Assets/OnScreenElementPrefab.prefab");
                    RectTransform rt = manager.ElementUIContainer;
                    dict.Add(e, GameObject.Instantiate(go, rt));
                }
                if (dict.ContainsKey(e) && !e.Visible)
                {
                    GameObject.Destroy(dict[e]);
                    dict.Remove(e);
                }
            }
            foreach(KeyValuePair<CompassManager.Element,GameObject> e in dict)
            {
                if(e.Value != null)
                {
                    if(cam == null) return;
                    if(cam.Camera == null) return;
                    if(e.Key.Transform == null) return;
                    dict[e.Key].transform.position = cam.Camera.WorldToScreenPoint(e.Key.Transform.position);
                    
                    if (dict[e.Key].transform.position.z >= 0)
                    {
                        System.Random r = new System.Random(dict[e.Key].GetInstanceID());
                        dict[e.Key].GetComponent<Image>().color = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
                    }
                    else
                    {
                        dict[e.Key].GetComponent<Image>().color = Color.clear;
                    }
                }
            }
        }

        public static Texture2D LoadImage(string Name)
        {
            string imgPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + Name;
            byte[] imgData = File.ReadAllBytes(imgPath);
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(imgData))
            {
                MelonLogger.Error($"Failed to load image. Make sure its named \"{Name}\" exactly.");
                return null;
            }
            else
            {
                
                return tex;
                // set app icon
            }
        }
    }
}
