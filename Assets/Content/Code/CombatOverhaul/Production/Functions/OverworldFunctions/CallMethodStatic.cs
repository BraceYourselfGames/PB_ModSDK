using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class CallMethodStatic : IOverworldFunction
	{
		[InfoBox ("Warning! This method is primarily for use in tutorials.", InfoMessageType.Warning)]
		[Tooltip ("Type of the view class to target. For example, CIViewOverworldRoot")]
		public string typeName = "";
		
		[Tooltip ("Method name. Method must be public and static to be discovered.")]
		public string methodName = "";
		
		[Tooltip ("Method name. Method must be public and static to be discovered.")]
		public string argument = "";

		[Tooltip ("By how many frames to delay this call")]
		[PropertyRange (0, 10)]
		public int frameDelay = 0;

		public void Run ()
		{
			#if !PB_MODSDK

			if (frameDelay == 0)
				RunLate ();
			else
				Co.DelayFrames (frameDelay, RunLate);
			
			#endif
		}

		private void RunLate ()
		{
			#if !PB_MODSDK

			if (string.IsNullOrEmpty (typeName))
			{
				Debug.LogWarning ($"CallMethodStatic | No type name provided");
				return;
			}
			
			var type = Type.GetType (typeName);
			if (type == null)
			{
				Debug.LogWarning ($"Failed to find type {typeName}");
				return;
			}
			
			if (string.IsNullOrEmpty (methodName))
			{
				Debug.LogWarning ($"CallMethodStatic | No method name provided for type {typeName}");
				return;
			}
			
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			var methodInfo = type.GetMethod (methodName, flags);
			if (methodInfo == null)
			{
				Debug.LogWarning ($"Failed to find method {methodName} on type {typeName}");
				return;
			}

			if (!string.IsNullOrEmpty (argument))
			{
				Debug.LogWarning ($"Invoking method {methodName} on type {typeName} with an argument: {argument}");
				methodInfo.Invoke (null, new object[] { argument });
			}
			else
			{
				Debug.LogWarning ($"Invoking method {methodName} on type {typeName} without an argument");
				methodInfo.Invoke (null, null);
			}
			
			#endif
		}
	}
}
