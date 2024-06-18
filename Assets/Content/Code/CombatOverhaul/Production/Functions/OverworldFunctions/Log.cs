using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class Log : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction, ICombatFunctionTargeted
	{
		public string message;

		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var eventText = eventData != null ? eventData.key : "?";
			Debug.Log ($"Logging from event function with event: {eventText} | Target: {target.ToLog ()} | Message: {message}");
			
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
}
