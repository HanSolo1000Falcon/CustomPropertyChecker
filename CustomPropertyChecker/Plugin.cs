using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CustomPropertyChecker;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static Dictionary<VRRig, Hashtable> PlayerProperties = new();

    private bool    isOpen;
    private Vector2 scrollPosition;

    private void Start() => GorillaTagger.OnPlayerSpawned(OnGameInitialized);

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.F5))
            isOpen = !isOpen;
    }

    private void OnGUI()
    {
        if (!isOpen)
            return;

        GUIStyle boxStyle = new(GUI.skin.box)
        {
                fontSize  = 14,
                padding   = new RectOffset(10, 10, 5, 5),
                alignment = TextAnchor.UpperLeft,
        };

        GUIStyle nameStyle = new(GUI.skin.label)
        {
                fontSize  = 16,
                fontStyle = FontStyle.Bold,
        };

        GUILayout.BeginArea(new Rect(20, 20, 400, Screen.height - 40));
        GUILayout.BeginVertical(" ", boxStyle);

        scrollPosition = GUILayout.BeginScrollView(
                scrollPosition,
                false,
                true,
                GUILayout.Width(380),
                GUILayout.Height(Screen.height - 80)
        );

        foreach ((VRRig rig, Hashtable properties) in PlayerProperties)
        {
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label(rig.OwningNetPlayer.SanitizedNickName, nameStyle);
            GUILayout.Space(5);

            foreach (DictionaryEntry entry in properties)
                GUILayout.Label($"• {FormatValue(entry.Key)}: {FormatValue(entry.Value)}");

            GUILayout.EndVertical();
            GUILayout.Space(15);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private string FormatValue(object value, int depth = 0)
    {
        if (value == null)
            return "null";

        if (depth > 6)
            return "...";

        switch (value)
        {
            case string stringValue:
                return stringValue;

            case IDictionary dictionary:
            {
                StringBuilder builder = new();
                builder.Append("{\n");

                bool first = true;
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!first)
                        builder.Append(",\n");

                    string spaces = "  ";
                    for (int i = 0; i < depth; i++)
                        spaces += "  ";

                    builder.Append(spaces + FormatValue(entry.Key, depth + 1));
                    builder.Append(": ");
                    builder.Append(FormatValue(entry.Value, depth + 1));

                    first = false;
                }

                string spacesOther = " ";
                for (int i = 0; i < depth; i++)
                    spacesOther += " ";

                builder.Append(spacesOther + "\n}");

                return builder.ToString();
            }
        }

        if (value is not IEnumerable enumerable)
            return value.ToString();

        {
            StringBuilder builder = new();
            builder.Append("[\n");

            bool first = true;
            foreach (object item in enumerable)
            {
                if (!first)
                    builder.Append(",\n");

                string spaces = "  ";
                for (int i = 0; i < depth; i++)
                    spaces += "  ";

                builder.Append(spaces + FormatValue(item, depth + 1));
                first = false;
            }

            string spacesOther = " ";
            for (int i = 0; i < depth; i++)
                spacesOther += " ";

            builder.Append(spacesOther + "\n]");

            return builder.ToString();
        }
    }

    private void OnGameInitialized()
    {
        gameObject.AddComponent<PunCallbacks>();
        NetworkSystem.Instance.OnJoinedRoomEvent        += (Action)DoRecalculation;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += () => PlayerProperties.Clear();
        NetworkSystem.Instance.OnPlayerJoined           += player => DoRecalculation();
        NetworkSystem.Instance.OnPlayerLeft             += player => DoRecalculation();
    }

    private void DoRecalculation()
    {
        PlayerProperties.Clear();

        foreach ((NetPlayer netPlayer, VRRig rig) in GorillaParent.instance.vrrigDict)
            PlayerProperties[rig] = netPlayer.GetPlayerRef().CustomProperties;
    }
}