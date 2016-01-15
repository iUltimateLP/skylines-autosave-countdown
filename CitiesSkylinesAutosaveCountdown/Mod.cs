using System;
using ICities;
using UnityEngine;

namespace AutosaveCountdownMod
{
    public class CSCountdown : IUserMod
    {
        public string Name { get { return "AutoSave Countdown";} }
        public string Description { get { return "Simple AutoSave Countdown"; } }
    }
}