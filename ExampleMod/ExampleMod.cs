using BepInEx;
using BepInEx.Logging;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Assets;
using KSP.UI.Binding;
using KSP.Sim.impl;
using KSP.Game;
using SpaceWarp;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Threading;
using KSP.Api;
using HarmonyLib.Tools;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExampleMod;

public class CommandService : WebSocketBehavior
{

    public event EventHandler<string> OnCommandRecieved;

    protected override void OnMessage(MessageEventArgs e)
    { 
        Console.Write("Emit: " + e.Data);
        OnCommandRecieved?.Invoke(this, e.Data);

    }



    /* private double? getAltitude()
     {
         if (Game.GlobalGameState.GetState() != KSP.Game.GameState.FlightView)
         {
             return null;
         }

         VesselComponent _activeVessel = Game.ViewController.GetActiveSimVessel();

         if (_activeVessel == null)
         {
             return null;
         }

         return _activeVessel.AltitudeFromSeaLevel;


     }

     private void enableSas()
     {
         if (Game.GlobalGameState.GetState() != KSP.Game.GameState.FlightView)
         {
             return;
         }

         VesselComponent _activeVessel = Game.ViewController.GetActiveSimVessel();

         if (_activeVessel == null)
         {
             return;
         }

         _activeVessel.SetAutopilotEnableDisable(true);
     }

     private void targetHeading()
     {
         if (Game.GlobalGameState.GetState() != KSP.Game.GameState.FlightView)
         {
             return;
         }

         VesselComponent _activeVessel = Game.ViewController.GetActiveSimVessel();

         if (_activeVessel == null)
         {
             return;
         }

         var sas = _activeVessel.Autopilot.SAS;

         // Make a Quaternion to point up

         _activeVessel.SetAutopilotMode(KSP.Sim.AutopilotMode.Autopilot);


         var target = new Vector3(x, y, z);

         sas.SetTargetOrientation(new KSP.Sim.Vector(sas.ReferenceFrame, target), false);

     }*/
}

[BepInPlugin("com.SpaceWarpAuthorName.ExampleMod", "ExampleMod", "3.0.0")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class ExampleMod : BaseSpaceWarpPlugin 
{

    private bool drawUI;
    private Rect windowRect;

    public event EventHandler<string> ResponseEvent;

    private HttpServer server;

    static HashSet<string> watchList = new HashSet<string>();



    private void OnCommandRecieve(object sender, string command)
    {

        if (Game.GlobalGameState.GetState() != KSP.Game.GameState.FlightView)
         {
             return;
         }

         VesselComponent _activeVessel = Game.ViewController.GetActiveSimVessel();

         if (_activeVessel == null)
         {
             return;
         }

        dynamic payload = JObject.Parse(command);

        string type = payload.type;

        if(type == "watch")
        {

            var add_watch = payload.data.ToObject<List<string>>();

            for(int i = 0; i < add_watch.Count; i++)
            {
                watchList.Add(add_watch[i]);
            }
            

           
        }


        if(type == "sas")
        {
            bool enable = payload.data.enable;

            _activeVessel.SetAutopilotEnableDisable(enable);

        }

        if(type == "setThrottle")
        {
            float throttle = payload.data.throttle;

            GameManager.Instance.Game.ViewController.flightInputHandler.OverrideInputThrottle(throttle);
        }

    }



    private static ExampleMod Instance { get; set; }

    private CommandService CommandService { get; set; }


    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        Logger.LogInfo("kRPC > Started!");


        server = new HttpServer(IPAddress.Parse("0.0.0.0"), 6674);

        server.AddWebSocketService<CommandService>("/ws", s =>
        {
            CommandService = s;
            CommandService.OnCommandRecieved += OnCommandRecieve;
        });

        server.Start();

        if (server.IsListening)
        {
            Logger.LogInfo(string.Format("kRPC > Listening on port {0}, and providing WebSocket services:", server.Port));
        }


        Appbar.RegisterAppButton(
                "Example Mod",
                "BTN-ExampleMod",
                AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
                ToggleButton
            );
    }

    private void ToggleButton(bool toggle)
    {
        drawUI = toggle;
        GameObject.Find("BTN-ExampleMod")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
    }

    public void OnGUI()
    {
        GUI.skin = Skins.ConsoleSkin;


        if (drawUI)
        {
            windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRect,
                FillWindow, 
                "Window Header",
                GUILayout.Height(350),
                GUILayout.Width(350)
            );
        }
    }

    private void FillWindow(int windowID)
    {
        GUILayout.Label($"{lastUpdateTime}");
        GUI.DragWindow(new Rect(0, 0, 10000, 500));


    }
    
    private float lastUpdateTime = 0.0f;
    private float updateInterval = 0.1f;

    private void LateUpdate()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;

            if (Game.GlobalGameState.GetState() != KSP.Game.GameState.FlightView)
            {
                return;
            }

            VesselComponent _activeVessel = Game.ViewController.GetActiveSimVessel();

            if (_activeVessel == null)
            {
                return;
            }

            foreach (string watch in watchList) {

            
                if(watch == "altitude")
                {

                    var altitude = _activeVessel.AltitudeFromSeaLevel;

                    var jsonResponse = new JObject();

                    jsonResponse["type"] = "altitude";
                    jsonResponse["data"] = altitude;

                    server.WebSocketServices["/ws"].Sessions.Broadcast(jsonResponse.ToString());

                }

                if(watch == "sas")
                {
                    var enabled = _activeVessel.Autopilot._isEnabled;

                    var jsonResponse = new JObject();

                    jsonResponse["type"] = "sas";
                    jsonResponse["data"] = enabled;

                    server.WebSocketServices["/ws"].Sessions.Broadcast(jsonResponse.ToString());
                }

                if(watch == "throttle")
                {
                    float throttle = GameManager.Instance.Game.ViewController.flightInputHandler._inputThrottle;

                    var jsonResponse = new JObject();

                    jsonResponse["type"] = "throttle";
                    jsonResponse["data"] = throttle;

                    server.WebSocketServices["/ws"].Sessions.Broadcast(jsonResponse.ToString());
                }

            }
         

        }
    }

   
}