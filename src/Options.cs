using System;
using Menu;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
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

            UpdateDraggerColor();
            dragger.OnValueUpdate += (_, _, _) => UpdateDraggerColor();
            modePicker.OnValueUpdate += (_, _, _) => modePickerLabel.text = DescriptionForMode((SpawnMode)Enum.Parse(typeof(SpawnMode), modePicker.value));

            void UpdateDraggerColor()
            {
                float ominous1 = Mathf.InverseLerp(100f, 999f, dragger.GetValueInt());
                float ominous2 = Mathf.InverseLerp(200f, 999f, dragger.GetValueInt());
                float ominous3 = Mathf.InverseLerp(400f, 999f, dragger.GetValueInt());
                dragger.colorEdge = Color.Lerp(MenuColorEffect.rgbMediumGrey, Color.red, ominous1);
                dragger.colorText = Color.Lerp(MenuColorEffect.rgbMediumGrey, Color.red, ominous2);
                dragger.colorFill = Color.Lerp(MenuColorEffect.rgbBlack, Color.red, ominous3 * 0.25f);
            }

            static string DescriptionForMode(SpawnMode mode)
            {
                return mode switch
                {
                    SpawnMode.CurrentRegion => "Where your save is located.",
                    SpawnMode.StoryRegions => "Story or optional regions.",
                    SpawnMode.AnyRegion => "Any region. At all.",
                    SpawnMode.EveryRegion => "You should be scared.",
                    _ => "",
                };
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
