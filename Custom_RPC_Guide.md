# Valheim Custom RPC Guide

In this guide I will show you how to create and use a custom RPC for Valheim. Why would you need to use this? When you have to trigger some code on any Player not actively hosting the chunk, for a specific online player, or for all players online you will need your own RPC.

## RPC Implementation

There are multiple ways to implement an RPC, in this guide I will show you two options:

1. "Peer" PRCs: This implementation will register an RPC for the peer. Use this solution if you need to trigger your code on any/all online players.
2. "Player" RPCs: This implementation will register an RPC on the Player object. Use this solution if you only need to trigger your code when other players are in the same chunk.

There are three parts to this guide: Creating, Calling, and Registering. Be sure to pay attention to which option you need for your solution when looking at the code examples.

## Creating the RPC

This is the code you will need to run on the target client(s) or server. This guide focuses more on client to client RPCs, but a similar approach applies when the target "peer" is the server. Create your own method with a parameter for the sender id as required:

```cs
private static void RPC_MyCustomMethod(long sender)
{
    // Do whatever you need to here
}

```

RPCs can also have more parameters on them, for example:

```cs
private static void RPC_MyCustomMethodExtended(long sender, string info, int num)
{
    // Do whatever you need to here
}

```

## Calling the RPC

Now that you have the RPC, you will want to have a way to call it. Create a new method that will be called on the chunk-hosting client (or server) when your code needs to be triggered. This is the method you will be calling in your game patch when you want the code to run, NOT the RPC method we created in the last step.

You will have to pick a unique name to register your RPC in the next step. It is a good idea to make your RPC name unique by adding in an author or mod initial just in case another mod out there wants to use the same RPC name. I often use "VV" to prefix mine. Create a variable for it so you can reuse the value in your code later.

### Option 1

If you need to call the code for everyone online:

```cs
public const string RPCNAME_MyCustomRPC = "Author_MyCustomRPC";

private static void CallMyCustomRPC()
{
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RPCNAME_MyCustomRPC);
}
```

Otherwise you can call for a specific Player given you have the ID:

```cs
public const string RPCNAME_MyCustomRPC = "Author_MyCustomRPC";

private static void CallMyCustomRPC(long playerID)
{
    if (id != 0)
    {
        ZRoutedRpc.instance.InvokeRoutedRPC(playerID, RPCNAME_MyCustomRPC);
    }
}
```

If calling for a specific player you have to actually find the PlayerID to send the RPC which if you already know the name of the player this can be done very easily. Here I do some string cleanup to support better name matching from manual user inputs or names created with whitespace. This code requires an exact match, but if you would like to support partial matches that can be done too.

```cs
public static long GetPlayerID(string playerName)
{
    var nameSimple = playerName.Trim().ToLower();
    var players = ZNet.instance.GetPlayerList();

    for (int lcv = 0; lcv < players.Count; lcv++)
    {
        var player = players[lcv].m_name.Trim().ToLower();
        //if (player.Contains(nameSimple)) // Alternative partial name match
        if (player.Equals(nameSimple))
        {
            return players[lcv].m_characterID.m_userID;
        }
    }

    return 0L;
}
```

### Option 2

```cs
public const string RPCNAME_MyCustomRPC = "Author_MyCustomRPC";

private static void CallMyCustomRPC(Player player)
{
    player.m_nview?.InvokeRPC(RPCNAME_MyCustomRPC);
}

```

Now you have to actually find the Player object to send the RPC. Depending on if you need a specific player or all players in range there are different ways to do this. Similar to option 1 I perform some string cleanup for the comparison.

```cs
public static void SendToPlayersInRange()
{
    List<Player> nearbyPlayers = new List<Player>();
    Player.GetPlayersInRange(Player.m_localPlayer.transform.position, 100, nearbyPlayers);

    if (nearbyPlayers != null)
    {
        for (int lcv = 0; lcv < nearbyPlayers.Count; lcv++)
        {
            CallMyCustomRPC(nearbyPlayers[lcv]);
        }
    }
}

public static void SendToPlayer(string playerName)
{
    for (int lcv = 0; lcv < Player.m_players.Count; lcv++)
    {
        var nameSimple = Player.m_players[lcv].GetPlayerName().Trim().ToLower();
        //if (player.Contains(nameSimple)) // Alternative partial name match
        if (player.Equals(nameSimple))
        {
            CallMyCustomRPC(nearbyPlayers[lcv]);
        }
    }
}

```

## Registering the RPC

Now that you have your RPC and logic for calling it you need to register the RPC method.

### Option 1

You have to register the RPC for our client only once. You can do this by registering on a Player.Load postfix for the client and ZoneSystem.Load for the server. If the method is not registered on the server instance then your RPC calls will not work on a dedicated server. The Player.Load patch will get called once in the main menu, and again when a world is loaded. You can check to make sure the main scene is loaded, but for simplicity I removed that code from the example. Registering can be done in many different places to the same effect, so feel free to change which methods you are patching if you are confident in doing so.

```cs
// Register for Player
[HarmonyPatch(typeof(Player), nameof(Player.Load))]
public static class Patch_Player_Load
{
    private static void Postfix(Player __instance)
    {
        try
        {
            ZRoutedRpc.instance.Register(RPCNAME_MyCustomRPC, new Action<long, string>(RPC_MyCustomMethod));
            // or the extended example
            ZRoutedRpc.instance.Register(RPCNAME_MyCustomRPC, new Action<long, string, int>(RPC_MyCustomMethodExtended));
        }
        catch
        {
            // The RPC has already been registered!
            // This can happen if you are hosting without a dedicated server
        }
    }
}

// Register for Server
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Load))]
public static class Patch_ZoneSystem_Load
{
    private static void Postfix()
    {
        if (ZNet.instance.IsServer())
        {
            try
            {
                ZRoutedRpc.instance.Register(RPCNAME_MyCustomRPC, new Action<long, string>(RPC_MyCustomMethod));
            }
            catch
            {
                // The RPC has already been registered!
                // This can happen if you are hosting without a dedicated server.
            }
        }
    }
}
```

### Option 2

You have to register the RPC for every Player that enters the game space so you can call the RPC on specific players. You can do this by registering on the Player.Awake postfix. This will get called once in the main menu (this Player object is soon destroyed), and every time a player is spawned. You can check to make sure the main scene is loaded, but for simplicity I removed that code from the example. Unlike Option 1, you do not need to explicitly register on the server since RPCs registered on the Player handle the routing for us (its actually all just magic).

```cs
[HarmonyPatch(typeof(Player), nameof(Player.Awake))]
public static class Patch_Player_Awake
{
    private static void Postfix(Player __instance)
    {
        if (__instance.m_nview != null)
        {
            __instance.m_nview.Register(RPCNAME_MyCustomRPC, new Action<long>(RPC_MyCustomMethod));
            // or the extended example
            __instance.m_nview.Register(RPCNAME_MyCustomRPC, new Action<long, string, int>(RPC_MyCustomMethodExtended));
        }
    }
}
```

## Conclusion

You should now have some functioning RPCs! Depending on what you're patching there may be better ways to grab the target player than from my examples, so take them as inspiration in solving your unique problem.

## Afternotes

If you're one of those people wondering what "lcv" stands for: Loop Control Variable. I highly recommend using this variable name for your loops rather than i and j. People can find it much easier to read and debug since i and j look so similar depending on your font and lcv1 and lcv2 are easily recognizable. This might be an old school tradition, but I picked it up in my first programming course and thought it was a great idea. Join us, thrive!

Here is a helper method to determine if the main game scene is loaded:

```cs
public static bool IsInTheMainScene()
{
    return SceneManager.GetActiveScene().name.Equals("main");
}
```
