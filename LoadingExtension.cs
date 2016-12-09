using ICities;
using UnityEngine;

namespace MoreBeautification
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame && mode!= LoadMode.NewGameFromScenario)
            {
                return;
            }
            new GameObject("MoreBeautificationInitializer").AddComponent<Initializer>();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            var initializer = GameObject.Find("MoreBeautificationInitializer");
            if (initializer != null)
            {
                Object.Destroy(initializer);
            }
        }
    }
}

