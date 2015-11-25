using ICities;
using UnityEngine;
using ColossalFramework.UI;
using System;

namespace AutosaveCountdownMod
{
    public class AutoSaveCountdown
    {
        public static bool bAutosaveEnabled;
        public static int autoSaveInterval;
        public static UIView uiView;
        public static UILabel label;
        public static float timer;

        public class LoadingExtension : LoadingExtensionBase
        {
            public override void OnLevelLoaded(LoadMode mode)
            {
                uiView = UIView.GetAView();

                UICheckBox autosaveCheckbox = uiView.FindUIComponent<UICheckBox>("AutoSave");
                UITextField autosaveIntervalTextField = uiView.FindUIComponent<UITextField>("AutoSaveInterval");

                autosaveCheckbox.eventClicked += new MouseEventHandler(CheckBoxChanged);
                autosaveIntervalTextField.eventTextSubmitted += new PropertyChangedEventHandler<string>(IntervalChanged);

                createLabel();

                bAutosaveEnabled = autosaveCheckbox.isChecked;
                autoSaveInterval = int.Parse(autosaveIntervalTextField.text);

                timer = autoSaveInterval*60;

                updateLabel(bAutosaveEnabled, (int)timer);
            }
        }

        public class ThreadingExtension : ThreadingExtensionBase
        {
            public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
            {
                if (timer >= 0)
                    timer -= Time.deltaTime;
                else
                    timer = autoSaveInterval * 60 + 1;

                updateLabel(bAutosaveEnabled, (int)timer);
            }
        }

        public static void CheckBoxChanged(UIComponent c, UIMouseEventParameter p)
        {
            UICheckBox cb = c as UICheckBox;
            bAutosaveEnabled = cb.isChecked;
            timer = autoSaveInterval * 60;
            updateLabel(bAutosaveEnabled, autoSaveInterval);
        }

        public static void IntervalChanged(UIComponent c, string value)
        {
            autoSaveInterval = int.Parse(value);
            timer = autoSaveInterval * 60;
            updateLabel(bAutosaveEnabled, autoSaveInterval);
        }

        public static void updateLabel(bool enabled, int interval)
        {
            label.isVisible = enabled;
            TimeSpan t = TimeSpan.FromSeconds(interval);
            label.text =  "Next Autosave: " + (t.Hours < 10 ? "0" : "") + t.Hours + ":" + (t.Minutes < 10 ? "0" : "") + t.Minutes + ":" + (t.Seconds < 10 ? "0" : "") + t.Seconds;
            if (interval <= 10)
                label.textColor = Color.red;
            else if (interval <= 20)
                label.textColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
            else if (interval <= 30)
                label.textColor = Color.yellow;
            else
                label.textColor = Color.white;
        }

        public static void createLabel()
        {
            UISprite happiness = uiView.FindUIComponent<UISprite>("Happiness");
            UIPanel panel = uiView.FindUIComponent<UIPanel>("InfoPanel");

            label = panel.AddUIComponent(typeof(UILabel)) as UILabel;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical | UIAnchorStyle.Proportional;
            label.relativePosition = new Vector2(1550, 8);
        }
    }
}