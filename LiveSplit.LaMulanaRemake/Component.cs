﻿using System;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Collections.Generic;
using System.Xml;

[assembly: ComponentFactory(typeof(LiveSplit.LaMulanaRemake.Factory))]

namespace LiveSplit.LaMulanaRemake
{
    public class Factory : IComponentFactory
    {
        public string ComponentName => "La-Mulana Remake Auto Splitter";
        public string Description => "Autosplitter for La-Mulana (remake)";
        public ComponentCategory Category => ComponentCategory.Control;
        public IComponent Create(LiveSplitState state) => new LaMulanaComponent(state);

        public Version Version => new Version(0, 2, 2);
        public string UpdateName => ComponentName;
        public string UpdateURL => "https://worsety.github.io/files/LiveSplit.LaMulanaRemake/";
        public string XMLURL => "https://worsety.github.io/files/LiveSplit.LaMulanaRemake/updates.xml";
    }

    public class LaMulanaComponent : IComponent
    {
        public string ComponentName
        {
            get { return "La-Mulana Remake Auto Splitter"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; set; }

        public float HorizontalWidth => 0;
        public float VerticalHeight => 0;
        public float MinimumWidth => 0;
        public float MinimumHeight => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;
        public float PaddingTop => 0;
        public float PaddingBottom => 0;

        public SplitMatcher autosplitter;
        public SplitMatcher remake = new RemakeSplitter();
        // Maybe some day
        // public SplitMatcher classic = new ClassicSplitter();

        public LiveSplitState state;
        public TimerModel timer;

        public ComponentSettings settings_control;

        public void Dispose() {
            if (state != null)
                state.RunManuallyModified -= RunModified;
        }

        public LaMulanaComponent(LiveSplitState state)
        {
            this.state = state;
            if (timer == null || timer.CurrentState != state)
                timer = new TimerModel { CurrentState = state };

            settings_control = new ComponentSettings(this);

            state.RunManuallyModified += RunModified;
            RunModified(state, null);
        }

        void RunModified(object sender, EventArgs a)
        {
            if (state.Run.GameName == "La-Mulana Remake")
                autosplitter = remake;
            /*else if (state.Run.GameName == "La-Mulana Classic")
                autosplitter = classic;*/
            else
                autosplitter = null;
            settings_control.SetSplits(state, remake);
        }

        public void DrawHorizontal(System.Drawing.Graphics g, LiveSplitState state, float height, System.Drawing.Region clipRegion) { }

        public void DrawVertical(System.Drawing.Graphics g, LiveSplitState state, float width, System.Drawing.Region clipRegion) { }

        public System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return settings_control;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            remake.splits = settings_control.GetSplitMap(remake);

            XmlElement settings = document.CreateElement("Settings");
            XmlElement remakesettings = document.CreateElement("Remake");
            XmlElement remakemapping = document.CreateElement("Mapping");
            foreach (var cat in remake.splits)
            {
                XmlElement e = document.CreateElement("Map");
                e.SetAttribute("from", cat.Key);
                e.SetAttribute("to", cat.Value);
                remakemapping.AppendChild(e);
            }
            remakesettings.AppendChild(remakemapping);
            settings.AppendChild(remakesettings);

            return settings;
        }

        public void SetSettings(XmlNode settings)
        {
            XmlNode remakemapping = settings.SelectSingleNode("./Remake/Mapping");
            if (remakemapping != null)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                foreach (XmlElement e in remakemapping.SelectNodes("./Map"))
                {
                    string username = e.GetAttribute("from"), intname = e.GetAttribute("to");
                    if (username != "" && intname != "" && remake.intsplits.ContainsKey(intname))
                        map.Add(username.Normalize().ToLowerInvariant(), intname);
                }

                remake.splits = map;
            }
            settings_control.SetSplits(state, remake);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            try
            {
                autosplitter?.Update(state, timer);
            }
            catch { }
        }
    }
}