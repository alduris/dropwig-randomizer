using System;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace DropwigRandomizer
{
    internal class Options : OptionInterface
    {
        internal static Configurable<int> DropwigCount;
        internal static Configurable<SpawnMode> DropwigSpawnMode;

        public Options()
        {
            DropwigCount = config.Bind("dropwig_count", 50, new ConfigAcceptableRange<int>(1, 999));
            DropwigSpawnMode = config.Bind("dropwig_spawn_mode", SpawnMode.StoryRegions);
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = [new OpTab(this)];

            var title = new OpLabel(new Vector2(0f, 345f), new Vector2(600f, 30f), "DROPWIG RANDOMIZER", FLabelAlignment.Center, true)
            {
                verticalAlignment = OpLabel.LabelVAlignment.Center
            };
            title.label.shader = Custom.rainWorld.Shaders["MenuText"];

            OpResourceSelector modePicker;
            OpLabel modePickerLabel;

            var dragger = new OpDragger(DropwigCount, new Vector2(288, 288));
            Tabs[0].AddItems(
                title,

                // Dropwig count dragger
                new OpLabel(new Vector2(0f, 288f), new Vector2(280f, 24f), "Dropwigs to spawn:", FLabelAlignment.Right, false) { verticalAlignment = OpLabel.LabelVAlignment.Center },
                new OpLabel(new Vector2(320f, 288f), new Vector2(280f, 24f), "Between 1 and 999", FLabelAlignment.Left, false) { verticalAlignment = OpLabel.LabelVAlignment.Center },
                dragger,

                // Dropwig mode selector
                new OpLabel(new Vector2(0f, 228f), new Vector2(232f, 24f), "Region selection:", FLabelAlignment.Right, false) { verticalAlignment = OpLabel.LabelVAlignment.Center },
                modePicker = new OpResourceSelector(DropwigSpawnMode, new Vector2(240f, 228f), 120f),
                modePickerLabel = new OpLabel(new Vector2(368f, 228f), new Vector2(232f, 24f), DescriptionForMode(DropwigSpawnMode.Value), FLabelAlignment.Left, false) { verticalAlignment = OpLabel.LabelVAlignment.Center }
                );

            modePicker.OnValueUpdate += (_, _, _) => modePickerLabel.text = DescriptionForMode((SpawnMode)Enum.Parse(typeof(SpawnMode), modePicker.value));

            static string DescriptionForMode(SpawnMode mode)
            {
                switch (mode)
                {
                    case SpawnMode.CurrentRegion:
                        return "Where your save is located.";
                    case SpawnMode.StoryRegions:
                        return "Story or optional regions.";
                    case SpawnMode.AnyRegion:
                        return "Any region. At all.";
                    case SpawnMode.EveryRegion:
                        return "You should be scared.";
                    default:
                        return "";
                }
            }
        }

        public enum SpawnMode
        {
            CurrentRegion,
            StoryRegions,
            AnyRegion,
            EveryRegion
        }
    }
}
