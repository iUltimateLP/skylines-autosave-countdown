using ICities;
using UnityEngine;
using ColossalFramework.UI;
using System;
using System.Collections;

namespace AutosaveCountdownMod
{
    public class AutoSaveCountdown : MonoBehaviour
    {
        #region Vars
        public static AutoSaveCountdown _inst;

        public bool bAutosaveEnabled;
        public int autoSaveInterval;
        public UIView uiView;
        public UILabel label;
        public float timer;
        public bool isPosEditing = false;
        public bool allowMoveAgain = true;
        public UILabel hintLabel;

        //In pixels:
        public float arrowSteps = 0.8f;
        #endregion

        #region Extensions

        public class LoadingExtension : LoadingExtensionBase
        {
            public override void OnLevelLoaded(LoadMode mode)
            {
                _inst = new AutoSaveCountdown();
                _inst.Init();
            }

            public override void OnLevelUnloading()
            {
                _inst.UnInit();
                Destroy(_inst);
                _inst = null;
            }
        }

        public class ThreadingExtension : ThreadingExtensionBase
        {
            public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
            {
                _inst.Tick();
            }
        }
        #endregion

        #region Init, UnInit, Tick

        public void Init()
        {
            uiView = UIView.GetAView();

            UICheckBox autosaveCheckbox = uiView.FindUIComponent<UICheckBox>("AutoSave");
            UITextField autosaveIntervalTextField = uiView.FindUIComponent<UITextField>("AutoSaveInterval");

            autosaveCheckbox.eventClicked += new MouseEventHandler(CheckBoxChanged);
            autosaveIntervalTextField.eventTextSubmitted += new PropertyChangedEventHandler<string>(IntervalChanged);

            createLabel();

            bAutosaveEnabled = autosaveCheckbox.isChecked;
            autoSaveInterval = int.Parse(autosaveIntervalTextField.text);

            timer = autoSaveInterval * 60;

            label.Enable();

            updateLabel(bAutosaveEnabled, (int)timer);

            hintLabel = uiView.AddUIComponent(typeof(UILabel)) as UILabel;
            hintLabel.name = "AutoSaveCountdownPlacementHint";
            hintLabel.text = "Press ENTER to finish placement.";
            hintLabel.textColor = Color.white;
            hintLabel.relativePosition = new Vector2(0, 0);
            hintLabel.anchor = UIAnchorStyle.Top;
            hintLabel.textScale = 2f;
            hintLabel.Hide();
        }

        public void UnInit()
        {
            if (label != null)
            {
                label.Disable();
            }
        }

        public void Tick()
        {
            if (timer >= 0)
                timer -= Time.deltaTime;
            else
                timer = autoSaveInterval * 60 + 1;

            Vector2 mousePos = Input.mousePosition;

            //Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                hintLabel.Hide();
                isPosEditing = false;
                SaveConfig();
            }

            //Mouse dragging
            if (Input.GetKey(KeyCode.Mouse0) && isPosEditing)
            {
                label.relativePosition = new Vector2(mousePos.x, uiView.fixedHeight - mousePos.y);
            }
            //Arrows
            else if (Input.GetKey(KeyCode.UpArrow) && isPosEditing)
            {
                label.relativePosition = new Vector2(label.relativePosition.x, label.relativePosition.y - arrowSteps);
            }
            else if (Input.GetKey(KeyCode.DownArrow) && isPosEditing)
            {
                label.relativePosition = new Vector2(label.relativePosition.x, label.relativePosition.y + arrowSteps);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && isPosEditing)
            {
                label.relativePosition = new Vector2(label.relativePosition.x - arrowSteps, label.relativePosition.y);
            }
            else if (Input.GetKey(KeyCode.RightArrow) && isPosEditing)
            {
                label.relativePosition = new Vector2(label.relativePosition.x + arrowSteps, label.relativePosition.y);
            }

            updateLabel(bAutosaveEnabled, (int)timer);
        }
        #endregion

        #region ChangedEvents

        public void CheckBoxChanged(UIComponent c, UIMouseEventParameter p)
        {
            UICheckBox cb = c as UICheckBox;
            bAutosaveEnabled = cb.isChecked;
            timer = autoSaveInterval * 60;
            updateLabel(bAutosaveEnabled, autoSaveInterval);
        }

        public void IntervalChanged(UIComponent c, string value)
        {
            autoSaveInterval = int.Parse(value);
            timer = autoSaveInterval * 60;
            updateLabel(bAutosaveEnabled, autoSaveInterval);
        }

        public void ResetPositionClicked()
        {
            if (label != null)
            {
                label.relativePosition = new Vector2(1550, uiView.fixedHeight - 28);
                SaveConfig();
            }
            else
            {
                //ResetConfig();
            }
        }
        #endregion

        #region Label functions

        public void createLabel()
        {
            UISprite happiness = uiView.FindUIComponent<UISprite>("Happiness");
            UIPanel panel = uiView.FindUIComponent<UIPanel>("InfoPanel");

            label = uiView.AddUIComponent(typeof(UILabel)) as UILabel;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            //label.relativePosition = new Vector2(1550, uiView.fixedHeight - 28); //this is overridden by LoadConfig() anyway
            label.eventClicked += new MouseEventHandler(labelClick);

            LoadConfig();
        }

        public void updateLabel(bool enabled, int interval)
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
        
        public void labelClick(UIComponent c, UIMouseEventParameter p)
        {
            if (!isPosEditing)
            {
                isPosEditing = true;
                hintLabel.Show();
            }
        }
        #endregion

        #region Saving & Loading
        public void SaveConfig()
        {
            PlayerPrefs.SetFloat("AutoSaveCountdown_label_x", label.relativePosition.x);
            PlayerPrefs.SetFloat("AutoSaveCountdown_label_y", label.relativePosition.y);
            PlayerPrefs.Save();
        }

        public void LoadConfig()
        {
            if (label != null)
            {
                if (PlayerPrefs.HasKey("AutoSaveCountdown_label_x") && PlayerPrefs.HasKey("AutoSaveCountdown_label_y"))
                {
                    float tmp_x = PlayerPrefs.GetFloat("AutoSaveCountdown_label_x");
                    float tmp_y = PlayerPrefs.GetFloat("AutoSaveCountdown_label_y");
                    label.relativePosition = new Vector2(tmp_x, tmp_y);
                }
                else
                {
                    label.relativePosition = new Vector2(1550, uiView.fixedHeight - 28); //Default location
                    SaveConfig();
                }
            }
        }
        #endregion
    }
}