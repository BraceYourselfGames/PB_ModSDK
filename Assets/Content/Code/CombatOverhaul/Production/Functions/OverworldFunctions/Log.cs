using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class Log : IOverworldTargetedFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction, ICombatFunctionTargeted
	{
		public string message;
		
		public void Run (OverworldEntity entityOverworld)
		{
			#if !PB_MODSDK
			
			Debug.Log ($"Logging from targeted function: {entityOverworld.ToLog ()} | Message: {message}");
			
			#endif
		}

		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

            var actionText = source.hasDataKeyOverworldAction ? source.dataKeyOverworldAction.s : "?";
			var target = IDUtility.GetOverworldActionOverworldTarget (source);
			Debug.Log ($"Logging from action function with action: {actionText} | Target: {target.ToLog ()} | Message: {message}");
			
			#endif
		}
		
		public void Run ()
		{
			#if !PB_MODSDK

			Debug.Log ($"Logging from general function | Message: {message}");
			
			#endif
		}
		
		public void Run (PersistentEntity unitPersistent)
		{
			#if !PB_MODSDK

			var unitText = unitPersistent.ToLog ();
			Debug.Log ($"Logging from targeted combat function with unit: {unitText} | Message: {message}");
			
			#endif
		}
	}
	
	[Serializable]
	public class Comment : 
        DataBlockComment, 
        IOverworldTargetedFunction, 
        IOverworldEntityValidationFunction, 
        IOverworldGlobalValidationFunction,
        IOverworldActionFunction, 
        IOverworldFunction, 
        ICombatFunction, 
        ICombatFunctionTargeted, 
        IPilotTargetedFunction, 
        IPilotValidationFunction
	{
		public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked) => true;
		public bool IsValid (PersistentEntity entityPersistent) => true;
		public bool IsValid () => true;
		public void Run (OverworldEntity entityOverworld) { }
		public void Run (OverworldActionEntity source) { }
		public void Run () { }
		public void Run (PersistentEntity unitPersistent) { }
		public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked) { }
	}
}
