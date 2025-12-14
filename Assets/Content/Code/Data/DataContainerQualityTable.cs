using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

[Serializable][LabelWidth (180f)]
public class DataContainerQualityTable : DataContainer
{
	public struct QualityRecordInt
	{
		[GUIColor("$GetQualityColor"), SuffixLabel ("$GetRatingText", true)]
		[HideLabel, HorizontalGroup]
		public int qualityIndex;
		
		[HideLabel, HorizontalGroup, SuffixLabel ("$GetChanceText", true)]
		public float weight;

		[HideInInspector, YamlIgnore]
		public float weightNormalized;

		private string GetRatingText => qualityIndex.IsValidIndex (UnitEquipmentQuality.text) ? UnitEquipmentQuality.text[qualityIndex] : "?";
		private string GetChanceText => Mathf.RoundToInt (Mathf.Clamp01 (weightNormalized) * 100f).ToString () + "%";
		private Color GetQualityColor => qualityIndex.IsValidIndex (UnitEquipmentQuality.colors) ? UnitEquipmentQuality.colors[qualityIndex] : Color.white;
	}

	public Color uiColor = Color.white;

	public float threatOffset = 0f;
	public string liveryGrade = "";

	[OnValueChanged ("OnWeightsChanged", true)]
	public List<QualityRecordInt> weightsInternal = new List<QualityRecordInt>();

	public int RollRandomQuality ()
	{
		if (weightsInternal.Count <= 0)
			return Int32.MinValue;

		float rValue = Random.value;
		for (int i = 0; i < weightsInternal.Count; ++i)
		{
			rValue -= weightsInternal[i].weightNormalized;
			if (rValue <= 0f)
				return weightsInternal[i].qualityIndex;
		}

		int result = weightsInternal[weightsInternal.Count - 1].qualityIndex;
		return Mathf.Clamp (result, 0, 4);
	}
	
	private void OnWeightsChanged ()
	{
		float totalWeight = 0f;
		if (weightsInternal == null || weightsInternal.Count == 0)
			return;
		
		for (int i = 0; i < weightsInternal.Count; ++i)
		{
			var entry = weightsInternal[i];
			totalWeight += entry.weight;
		}
		
		for (int i = 0; i < weightsInternal.Count; ++i)
		{
			var entry = weightsInternal[i];
			entry.weightNormalized = entry.weight / totalWeight;
			weightsInternal[i] = entry;
		}
	}

	public override void OnAfterDeserialization (string key)
	{
		base.OnAfterDeserialization (key);
		OnWeightsChanged ();
	}
}