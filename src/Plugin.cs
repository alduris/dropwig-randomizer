using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Watcher;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace DropwigRandomizer;

[BepInPlugin("alduris.dropwigs", "Dropwig Randomizer", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    bool IsInit;

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += OnModsInit;
        On.OverWorld.ctor += OverWorld_ctor;
        On.World.ctor += World_ctor;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        MachineConnector.SetRegisteredOI("alduris.dropwigs", new Options());
    }

    internal string selectedRegion = null;
    internal bool hasSpawned = false;

    private void OverWorld_ctor(On.OverWorld.orig_ctor orig, OverWorld self, RainWorldGame game)
    {
        if (game.IsStorySession)
        {
            hasSpawned = false;
            var regions = GetSlugcatRegions(game.StoryCharacter);
            if (regions.Count > 0)
                selectedRegion = regions[Random.Range(0, regions.Count)];
            else
                selectedRegion = null;

            Plugin.Logger.LogDebug("Selected region: " + selectedRegion ?? "<NULL>");
        }

        orig(self, game);

        static List<string> GetSlugcatRegions(SlugcatStats.Name storyCharacter)
        {
            if (ModManager.Watcher && storyCharacter == WatcherEnums.SlugcatStatsName.Watcher)
            {
                return [
                    "WBLA", "WPTA", "WRRA", "WVWA",
                    "WTDA", "WTDB", "WRFA", "WRFB", "WSKA", "WSKB", "WSKC", "WSKD",
                    "WARA", "WARB", "WARC", "WARD", "WARE", "WARF", "WARG",
                    "WAUA", "WRSA",
                    "WSSR", "WSUR", "WHIR", "WDSR", "WGWR", "WORA"];
            }
            return [.. SlugcatStats.SlugcatStoryRegions(storyCharacter).Union(SlugcatStats.SlugcatOptionalRegions(storyCharacter))];
        }
    }

    private void World_ctor(On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld)
    {
        orig(self, game, region, name, singleRoomWorld);

        if (selectedRegion != null && game != null && game.IsStorySession && string.Equals(name, selectedRegion, System.StringComparison.InvariantCultureIgnoreCase))
        {
#if !DEBUG
            self.worldProcesses.Add(new DropwigSpawner(self, this));
#else
            self.worldProcesses.Add(new DemoSpawner(self));
#endif
        }
    }

    private class DropwigSpawner(World world, Plugin plugin) : World.WorldProcess(world)
    {
        private readonly Plugin plugin = plugin;

        public override void Update()
        {
            base.Update();
            if (!plugin.hasSpawned)
            {
                var rooms = world.abstractRooms.Where(room => room.nodes.Any(node => node.type == AbstractRoomNode.Type.Den)).ToArray();
                if (rooms.Length > 0)
                {
                    var room = rooms[Random.Range(0, rooms.Length)];
                    Plugin.Logger.LogDebug("Room spawned in: " + room.name);
                    var possibleNodes = room.nodes.Select((node, i) => node.type == AbstractRoomNode.Type.Den ? i : -1).Where(x => x > -1).ToArray();
                    int toSpawn = Options.DropwigCount.Value;
                    for (int i = 0; i < toSpawn; i++)
                    {
                        var node = possibleNodes[Random.Range(0, possibleNodes.Length)];
                        var coord = new WorldCoordinate(room.index, -1, -1, node);
                        var crit = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DropBug), null, coord, world.game.GetNewID())
                        {
                            saveCreature = false
                        };
                        room.AddEntity(crit);
                    }
                }
                plugin.hasSpawned = true;
            }
        }
    }

    private class DemoSpawner(World world) : World.WorldProcess(world)
    {
        // Since this class is a copy purely for testing in specific rooms, we don't care about having the region loaded multiple times
        private bool spawned = false;

        public override void Update()
        {
            base.Update();

            if (!spawned)
            {
                spawned = true;
                var room = world.abstractRooms.FirstOrDefault(x => x.name == "SU_B02");
                if (room != null)
                {
                    var possibleNodes = room.nodes.Select((node, i) => node.type == AbstractRoomNode.Type.Den ? i : -1).Where(x => x > -1).ToArray();
                    int toSpawn = 50;
                    for (int i = 0; i < toSpawn; i++)
                    {
                        var node = possibleNodes[Random.Range(0, possibleNodes.Length)];
                        var coord = new WorldCoordinate(room.index, -1, -1, node);
                        var crit = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DropBug), null, coord, world.game.GetNewID())
                        {
                            saveCreature = false
                        };
                        room.AddEntity(crit);
                    }
                }
            }
        }
    }
}
