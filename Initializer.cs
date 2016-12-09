using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace MoreBeautification
{
    public class Initializer : MonoBehaviour
    {
        public void Update()
        {
            var rocksPanel = GameObject.Find("LandscapingRocksPanel");
            var scrollPanel = rocksPanel?.GetComponent<GeneratedScrollPanel>();
            if (scrollPanel == null)
            {
                return;
            }
            Initialize();
            Destroy(this);
        }


        public static void Initialize()
        {
            var uiCategory = FixPrefabsMetadata();
            var tabstrip = (UITabstrip)ToolsModifierControl.mainToolbar.component;
            var beautificationTabstrip = BeautificationTabstrip(tabstrip);

            if (beautificationTabstrip != null && !IsCreated(beautificationTabstrip))
            {
                AddPropsPanels(beautificationTabstrip, new[] {
                    new EditorProps ("PropsBillboards", new[] {
                        "PropsBillboardsLogo",
                        "PropsBillboardsSmallBillboard",
                        "PropsBillboardsMediumBillboard",
                        "PropsBillboardsLargeBillboard",
                    }, "ToolbarIconPropsBillboards", "Billboards"),

                    new EditorProps ("PropsSpecialBillboards", new[] {
                        "PropsBillboardsRandomLogo",
                        "PropsSpecialBillboardsRandomSmallBillboard",
                        "PropsSpecialBillboardsRandomMediumBillboard",
                        "PropsSpecialBillboardsRandomLargeBillboard",
                        "PropsSpecialBillboards3DBillboard",
                        "PropsSpecialBillboardsAnimatedBillboard",
                    }, "ToolbarIconPropsSpecialBillboards", "Special Billboards"),

                    new EditorProps ("PropsIndustrial", new[] {
                        "PropsIndustrialContainers",
                        "PropsIndustrialConstructionMaterials",
                        "PropsIndustrialStructures",
                    }, "ToolbarIconPropsIndustrial", "Industrial"),

                    new EditorProps ("PropsParks", new[] {
                        "PropsParksPlaygrounds",
                        "PropsParksFlowersAndPlants",
                        "PropsParksParkEquipment",
                        "PropsParksFountains"
                    }, "ToolbarIconPropsParks", "Parks"),

                    new EditorProps ("PropsCommon", new[] {
                        "PropsCommonAccessories",
                        "PropsCommonGarbage",
                        "PropsCommonCommunications",
                        "PropsCommonStreets"
                    }, "ToolbarIconPropsCommon", "Common"),

                    new EditorProps ("PropsResidential", new[] {
                        "PropsResidentialHomeYard",
                        "PropsResidentialRooftopAccess",
                        "PropsResidentialRandomRooftopAccess",
                    }, "ToolbarIconPropsResidential", "Residential"),

                    new EditorProps ("PropsLights", new[] {
                        "PropsCommonStreets",
                        "PropsCommonLights",
                        PrefabInfo.kDefaultCategory,
                        PrefabInfo.kSameAsGameCategory,
                    }, "SubBarPropsCommonLights", "Lights"),


                    new EditorProps ("PropsGroundTiles", new[] {
                        "PropsResidentialGroundTiles"
                    }, "SubBarPropsResidentialGroundTiles", "Ground Decals"),

                    new EditorProps ("PropsUnsorted", new[] {
                        PrefabInfo.kDefaultCategory,
                        PrefabInfo.kSameAsGameCategory,
                        "PropsMarkers"
                    }, "ToolbarIconHelp", "Unsorted"),

                });
            }
            try
            {
                var parent = GameObject.Find(SimulationManager.instance.m_metaData.m_environment + " Collections");
                foreach (var t in from Transform t in parent.transform where t.name == "Landscaping" select t)
                {
                    foreach (var prefab in t.gameObject.GetComponent<BuildingCollection>().m_prefabs)
                    {
                        uiCategory.SetValue(prefab, "LandscapingRocks");
                        prefab.m_availableIn = ItemClass.Availability.All;
                    }
                    foreach (var prefab in t.gameObject.GetComponent<NetCollection>().m_prefabs)
                    {
                        if ((prefab.m_availableIn & ItemClass.Availability.Game) == 0)
                        {
                            uiCategory.SetValue(prefab, "LandscapingRocks");
                            prefab.m_availableIn = ItemClass.Availability.All;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            GameObject.Find("LandscapingRocksPanel").GetComponent<GeneratedScrollPanel>().RefreshPanel();
        }

        private static FieldInfo FixPrefabsMetadata()
        {
            var uiCategory = typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic);
            var locale =
                (Locale)
                    typeof(LocaleManager).GetField("m_Locale", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(SingletonLite<LocaleManager>.instance);
            for (uint i = 0; i < PrefabCollection<PropInfo>.PrefabCount(); i++)
            {
                var prefab = PrefabCollection<PropInfo>.GetPrefab(i);
                if (prefab == null)
                {
                    continue;
                }
                if (prefab.editorCategory == "PropsRocks")
                {
                    uiCategory.SetValue(prefab, "LandscapingRocks");
                    prefab.m_availableIn = ItemClass.Availability.All;
                }
                var key = new Locale.Key { m_Identifier = "PROPS_TITLE", m_Key = prefab.name };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, prefab.name);
                }
                key = new Locale.Key { m_Identifier = "PROPS_DESC", m_Key = prefab.name };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, prefab.name);
                }
            }
            return uiCategory;
        }

        private struct EditorProps
        {
            public string m_category;
            public string[] m_categories;
            public string m_icon;
            public string m_tooltip;

            public EditorProps(string category, string[] categories, string icon, string tooltip)
            {
                this.m_category = category;
                this.m_categories = categories;
                this.m_icon = icon;
                this.m_tooltip = tooltip;
            }
        }

        private static void AddPropsPanels(UITabstrip tabstrip, EditorProps[] props)
        {
            foreach (var prop in props)
            {
                var button = AddButton(typeof(EditorPropsPanel), tabstrip, prop.m_category, prop.m_categories, prop.m_tooltip, true);
                if (button == null)
                {
                    continue;
                }
                SetButtonSprites(button, prop.m_icon, "SubBarButtonBase");
            }
        }

        private static UIButton[] AllButtons(UITabstrip s)
        {
            return s.components.OfType<UIButton>().ToArray();
        }

        private static UIButton FindButton(UITabstrip s, string name)
        {
            var buttons = AllButtons(s);
            return Array.Find(buttons, b => b.name == name);
        }

        private static bool IsCreated(UITabstrip strip)
        {
            var button = FindButton(strip, "TerrainDefault");
            return button != null;
        }

        private static UITabstrip BeautificationTabstrip(UITabstrip s)
        {
            var button = FindButton(s, "Landscaping");
            if (button == null)
            {
                return null;
            }

            var groupPanelType = typeof(LandscapingGroupPanel);
            var groupPanel = (GeneratedGroupPanel)s.GetComponentInContainer(button, groupPanelType);

            var strip = groupPanel?.Find<UITabstrip>("GroupToolstrip");
            return strip;
        }

        private UIButton AddButton(Type type, UITabstrip strip, string category, string tooltip, bool enabled)
        {
            return AddButton(type, strip, category, null, tooltip, enabled);
        }

        private static UIButton AddButton(Type type, UITabstrip strip, string category, string[] editorCategories, string tooltip, bool enabled)
        {
            if (GameObject.Find($"{category}Panel") != null)
            {
                return null;
            }

            var subbarButtonTemplate = UITemplateManager.GetAsGameObject("SubbarButtonTemplate");
            var subbarPanelTemplate = UITemplateManager.GetAsGameObject("SubbarPanelTemplate");


            var button = (UIButton)strip.AddTab(category, subbarButtonTemplate, subbarPanelTemplate, type);
            button.isEnabled = enabled;

            var generatedScrollPanel = (GeneratedScrollPanel)strip.GetComponentInContainer(button, type);
            if (generatedScrollPanel != null)
            {
                generatedScrollPanel.component.isInteractive = true;
                generatedScrollPanel.m_OptionsBar = ToolsModifierControl.mainToolbar.m_OptionsBar;
                generatedScrollPanel.m_DefaultInfoTooltipAtlas = ToolsModifierControl.mainToolbar.m_DefaultInfoTooltipAtlas;

                var panel = generatedScrollPanel as EditorPropsPanel;
                if (panel != null)
                {
                    panel.m_editorCategories = editorCategories;
                    panel.category = category;
                }

                if (enabled)
                {
                    generatedScrollPanel.RefreshPanel();
                }
            }

            button.tooltip = tooltip;
            return button;
        }

        private static void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase)
        {
            SetButtonSprites(button, foregroundSpriteBase, backgroundSpriteBase, "", "");
        }

        private void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase, string normalBgPostfix)
        {
            SetButtonSprites(button, foregroundSpriteBase, backgroundSpriteBase, normalBgPostfix, "");
        }

        private static void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase, string normalBgPostfix, string normalFgPostfix)
        {
            button.atlas = ResourceUtils.CreateAtlas(new string[] {
                backgroundSpriteBase + normalBgPostfix,
                backgroundSpriteBase + "Focused",
                backgroundSpriteBase + "Hovered",
                backgroundSpriteBase + "Pressed",
                backgroundSpriteBase + "Disabled",
                foregroundSpriteBase + normalFgPostfix,
                foregroundSpriteBase + "Focused",
                foregroundSpriteBase + "Hovered",
                foregroundSpriteBase + "Pressed",
                foregroundSpriteBase + "Disabled"
            });

            button.normalBgSprite = backgroundSpriteBase + normalBgPostfix;
            button.focusedBgSprite = backgroundSpriteBase + "Focused";
            button.hoveredBgSprite = backgroundSpriteBase + "Hovered";
            button.pressedBgSprite = backgroundSpriteBase + "Pressed";
            button.disabledBgSprite = backgroundSpriteBase + "Disabled";

            button.normalFgSprite = foregroundSpriteBase + normalFgPostfix;
            button.focusedFgSprite = foregroundSpriteBase + "Focused";
            button.hoveredFgSprite = foregroundSpriteBase + "Hovered";
            button.pressedFgSprite = foregroundSpriteBase + "Pressed";
            button.disabledFgSprite = foregroundSpriteBase + "Disabled";
        }
    }
}