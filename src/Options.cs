using Menu.Remix.MixedUI;
using UnityEngine;

namespace DropwigRandomizer
{
    internal class Options : OptionInterface
    {
        internal static Configurable<int> DropwigCount;

        public Options()
        {
            DropwigCount = config.Bind("dropwig_count", 50, new ConfigAcceptableRange<int>(1, 999));
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = [new OpTab(this)];

            var dragger = new OpDragger(DropwigCount, new Vector2(300f - 12f, 300f - 12f));
            Tabs[0].AddItems(dragger);
        }
    }
}
