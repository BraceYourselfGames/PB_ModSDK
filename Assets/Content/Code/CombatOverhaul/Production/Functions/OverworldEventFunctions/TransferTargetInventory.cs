using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class TransferTargetInventory : IOverworldEventFunction
	{
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
			var basePersistent = IDUtility.playerBasePersistent;

			if (basePersistent == null || targetPersistent == null)
			{
				Debug.LogError ($"TransferTargetInventory | Event function failed due to missing base ({basePersistent.ToStringNullCheck ()}) or target ({targetPersistent.ToStringNullCheck ()})");
				return;
			}

			EquipmentUtility.TransferFullInventory (targetPersistent, basePersistent, true);
			
			#endif
		}
	}
}
