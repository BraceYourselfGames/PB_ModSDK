using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace PhantomBrigade.Functions
{
    public class CombatUnitIntegrityChange : ICombatFunctionTargeted
    {
        public float value;
        public bool offset;

        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            UnitUtilities.ModifyCombatIntegrityNormalized (unitPersistent, value, offset, sockets);

            #endif
        }
    }
    
    public class CombatUnitIntegrityChangeRandom : ICombatFunctionTargeted
    {
        public Vector2 range;
        public bool offset;
        public bool randomPerPart;

        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            UnitUtilities.ModifyCombatIntegrityNormalized (unitPersistent, range, offset, sockets, randomPerPart);

            #endif
        }
    }
}