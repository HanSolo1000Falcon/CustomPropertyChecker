using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CustomPropertyChecker;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static Dictionary<VRRig, Hashtable> PlayerProperties = new();

    private bool isOpen;
    
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        if (!isOpen)
            return;
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            padding = new RectOffset(10, 10, 5, 5),
            alignment = TextAnchor.UpperLeft
        };

        GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };

        GUILayout.BeginArea(new Rect(20, 20, 400, Screen.height - 40));
        GUILayout.BeginVertical(" ", boxStyle);
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(380),
            GUILayout.Height(Screen.height - 80));

        foreach (var (rig, properties) in PlayerProperties)
        {
            GUILayout.BeginVertical(boxStyle);
            
            GUILayout.Label(rig.OwningNetPlayer.NickName, nameStyle);
            GUILayout.Space(5);
            
            foreach (DictionaryEntry entry in properties)
                GUILayout.Label($"• {entry.Key}: {entry.Value}");

            GUILayout.EndVertical();
            GUILayout.Space(15);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.F5))
            isOpen = !isOpen;
    }

    private void OnGameInitialized()
    {
        gameObject.AddComponent<PunCallbacks>();

        NetworkSystem.Instance.OnJoinedRoomEvent += (Action)OnJoinedRoom;
    }

    private void OnJoinedRoom()
    {
        PlayerProperties.Clear();
        
        foreach (var (netPlayer, rig) in GorillaParent.instance.vrrigDict)
            PlayerProperties[rig] = netPlayer.GetPlayerRef().CustomProperties;
    }
    
    private void Start() => GorillaTagger.OnPlayerSpawned(OnGameInitialized);
}