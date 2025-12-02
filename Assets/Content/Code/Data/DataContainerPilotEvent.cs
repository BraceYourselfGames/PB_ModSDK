using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    // While string keyed data containers are convenient, pilot events are in a hot path similar to game events and need int-based keying
    public enum PilotEventType
    {
        Unknown = 0,

        CombatPilotKnockoutEnemy,
        CombatPilotKnockoutAlly,
        
        CombatUnitTakedownEnemy,
        CombatUnitTakedownAlly,
        
        CombatUnitPartLossSelf,
        CombatUnitPartLossEnemy,
        CombatUnitPartLossAlly,
        
        CombatUnitDamageIncoming,
        CombatUnitDamageOutgoingEnemy,
        CombatUnitDamageOutgoingAlly,  
        
        CombatUnitSelfOverheat,
        CombatEjection,
        
        CombatSharedUnitTakedownEnemy,
        CombatSharedUnitEjectionEnemy,
        
        CombatUnitCrashLost,
        CombatUnitCrashWon,
        CombatUnitCrashWonShielded,
        CombatUnitCrashWonUnique,
        
        CombatUnitStatusGainSelf,
        CombatUnitStatusLossSelf,
        
        CombatUnitArrival,
        CombatTurnEnd,
        
        CombatUnitBarrierDepleted,
        CombatUnitBarrierRestored,
        
        CombatActionMovementStart,
        CombatActionMovementEnd,
        CombatActionDashStart,
        CombatActionDashEnd,
        CombatActionAttackStart,
        CombatActionAttackEnd,
        CombatActionShieldStart,
        CombatActionShieldEnd,
        CombatActionShieldDeflect,

        CombatMissionEntry,
        CombatMissionExit,
        CombatMissionDefeatEarly,
        CombatMissionVictoryEarly,
        CombatMissionDefeat,
        CombatMissionVictory,
        
        StateHealthLoss,
        StateHealthRecovery,
        StateHealthDepleted,
        
        StateFatigueAcquired,
        StateFatigueRecovery,
        StateFatigueMaxed,

        StateTraumaAcquired,
        StateDeath
    }
    
    public class DataBlockPilotEventUI
    {
        [LabelText ("Text")][YamlIgnore]
        public string textName;
    }
    
    public class DataContainerPilotEvent : DataContainerWithText
    {
        [ShowIf ("@ui != null")]
        [OnInspectorGUI ("DrawHeaderGUI", false)]
        [PropertyRange (0f, 1f)]
        public float hue = 0.388f;

        public PilotEventType type;
        public bool removeOnDeployment = true;
        
        [DropdownReference (true)]
        [LabelText ("UI")]
        public DataBlockPilotEventUI ui;

        [DropdownReference (true)]
        public DataBlockInt experience;
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (type == PilotEventType.Unknown)
                Debug.LogWarning ($"Pilot event config {key} has no valid event enum!");
        }

        public override void ResolveText ()
        {
            if (ui != null)
                ui.textName = DataManagerText.GetText (TextLibs.pilotEvents, $"{key}__name");
        }

        #if UNITY_EDITOR
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerPilotEvent () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (ui != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotEvents, $"{key}__name", ui.textName);
        }
        
        private void DrawHeaderGUI ()
        {
            var rect = UnityEditor.EditorGUILayout.BeginVertical ();
            GUILayout.Label (" ", GUILayout.Height (12));
            UnityEditor.EditorGUILayout.EndVertical ();

            var gc = GUI.color;
            GUI.color = new HSBColor (hue, 0.5f, 1f).ToColor ();
            GUI.DrawTexture (rect, Texture2D.whiteTexture);
            GUI.color = gc;
        }

        #endif
    }
}

