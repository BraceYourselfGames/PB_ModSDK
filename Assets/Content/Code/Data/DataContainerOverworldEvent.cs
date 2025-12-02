using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public interface ICustomDataProvider
    {
        string GetKey ();
        bool IsFlagPresent (string key);
        bool TryGetInt (string key, out int result, int fallback = default);
        bool TryGetFloat (string key, out float result, float fallback = default);
        bool TryGetString (string key, out string result, string fallback = default);

        IEnumerable<string> GetFlagKeys ();
        IEnumerable<string> GetIntKeys ();
        IEnumerable<string> GetFloatKeys ();
        IEnumerable<string> GetStringKeys ();
    }
}