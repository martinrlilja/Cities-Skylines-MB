using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace MoreBeautification
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            var locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonLite<LocaleManager>.instance);
            for (uint i = 0; i < PrefabCollection<PropInfo>.PrefabCount(); i++)
            {
                var prefab = PrefabCollection<PropInfo>.GetPrefab(i);
                if (prefab == null)
                {
                    continue;
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

            var tabstrip = (UITabstrip)ToolsModifierControl.mainToolbar.component;
            var beautificationTabstrip = this.BeautificationTabstrip(tabstrip);

            if (beautificationTabstrip != null && !this.IsCreated(beautificationTabstrip))
            {
                this.AddPropsPanels(beautificationTabstrip, new EditorProps[] {
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
                        "PropsParksFountains",
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
                    }, "SubBarPropsResidentialGroundTiles", "Ground Tiles"),

                    new EditorProps ("PropsUnsorted", new[] {
                        PrefabInfo.kDefaultCategory,
                        PrefabInfo.kSameAsGameCategory,
                        "PropsMarkers"
                    }, "ToolbarIconHelp", "Unsorted"),

                });
            }
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

        private void AddPropsPanels(UITabstrip tabstrip, EditorProps[] props)
        {
            foreach (var prop in props)
            {
                var button = this.AddButton(typeof(EditorPropsPanel), tabstrip, prop.m_category, prop.m_categories, prop.m_tooltip, true);
                if (button == null)
                {
                    continue;
                }
                this.SetButtonSprites(button, prop.m_icon, "SubBarButtonBase");
            }
        }

        private UIButton[] AllButtons(UITabstrip s)
        {
            return s.components.OfType<UIButton>().ToArray();
        }

        private UIButton FindButton(UITabstrip s, string name)
        {
            var buttons = this.AllButtons(s);
            return Array.Find(buttons, b => b.name == name);
        }

        private bool IsCreated(UITabstrip strip)
        {
            var button = this.FindButton(strip, "TerrainDefault");
            return button != null;
        }

        private UITabstrip BeautificationTabstrip(UITabstrip s)
        {
            var button = this.FindButton(s, "Beautification");
            if (button == null)
            {
                return null;
            }

            var groupPanelType = typeof(BeautificationGroupPanel);
            var groupPanel = (GeneratedGroupPanel)s.GetComponentInContainer(button, groupPanelType);

            var strip = groupPanel?.Find<UITabstrip>("GroupToolstrip");
            return strip;
        }

        private UIButton AddButton(Type type, UITabstrip strip, string category, string tooltip, bool enabled)
        {
            return this.AddButton(type, strip, category, null, tooltip, enabled);
        }

        private UIButton AddButton(Type type, UITabstrip strip, string category, string[] editorCategories, string tooltip, bool enabled)
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

        private void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase)
        {
            this.SetButtonSprites(button, foregroundSpriteBase, backgroundSpriteBase, "", "");
        }

        private void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase, string normalBgPostfix)
        {
            this.SetButtonSprites(button, foregroundSpriteBase, backgroundSpriteBase, normalBgPostfix, "");
        }

        private void SetButtonSprites(UIButton button, string backgroundSpriteBase, string foregroundSpriteBase, string normalBgPostfix, string normalFgPostfix)
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

