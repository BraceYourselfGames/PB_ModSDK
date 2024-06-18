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
	public struct QualityRecordString
	{
		//[InfoBox(@"@"""" + chance.ToString(""P"")")]
		[HorizontalGroup]
		[HideLabel]
		[GUIColor ("GetQualityColor")]

		[ValueDropdown ("GetQualityKeys")]
		public string qualityName;

		[YamlIgnore]
		[HideInInspector]
		public int qualityIndex;

		[HorizontalGroup]
		[HideLabel]
		public float weight;

		[HorizontalGroup]
		[YamlIgnore]
		[ReadOnly]
		[HideLabel]
		public string chance;

		private static IEnumerable<string> GetQualityKeys => UnitEquipmentQuality.text;

		private Color GetQualityColor => qualityIndex.IsValidIndex (UnitEquipmentQuality.colors) ? UnitEquipmentQuality.colors[qualityIndex] : Color.white;
	}

	public struct QualityRecordInt
	{
		public int qualityIndex;
		public float weight;

		[YamlIgnore]
		public float weightNormalized;
	}

	public Color uiColor = Color.white;

	public float threatOffset = 0f;
	public string liveryGrade = "";

	[YamlIgnore]
	[LabelText("Weights")]
	[OnValueChanged ("OnWeightsChanged", true)]
	public List<QualityRecordString> weightsVisible = new List<QualityRecordString>();

	[ReadOnly]
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

	static Dictionary<int, float> tmpWeightBuffer = new Dictionary<int, float>();

	void OnWeightsChanged ()
	{
		weightsInternal.Clear();
		tmpWeightBuffer.Clear();

		float totalWeight = 0f;
		for (int i = 0; i < weightsVisible.Count; ++i)
		{
			var rec = weightsVisible[i];
			var index = Array.FindIndex (UnitEquipmentQuality.text, s => s == rec.qualityName);
			if (index < 0)
				continue;

			rec.weight = Mathf.Max (0f, rec.weight);

			rec.qualityIndex = index;
			weightsVisible[i] = rec;

			if(tmpWeightBuffer.ContainsKey (index))
				tmpWeightBuffer[index] += rec.weight;
			else
				tmpWeightBuffer.Add (index, rec.weight);

			totalWeight += rec.weight;
		}

		
		if(totalWeight > 0f)
		{
			foreach(var kv in tmpWeightBuffer)
			{
				weightsInternal.Add (new QualityRecordInt { qualityIndex = kv.Key, weight = kv.Value, weightNormalized = kv.Value / totalWeight });
			}
		}

		for (int i = 0; i < weightsVisible.Count; ++i)
		{
			var rec = weightsVisible[i];
			var index = Array.FindIndex (UnitEquipmentQuality.text, s => s == rec.qualityName);
			
			if (totalWeight > 0f && tmpWeightBuffer.TryGetValue (index, out var weight))
				rec.chance = (weight / totalWeight).ToString ("P");
			else
				rec.chance = "N/A";

			weightsVisible[i] = rec;
		}
	}

	void UpdateVisibleWeights ()
	{
		weightsVisible.Clear ();
		foreach (var record in weightsInternal)
		{
			var name = record.qualityIndex >= 0 && record.qualityIndex < UnitEquipmentQuality.text.Length ? UnitEquipmentQuality.text[record.qualityIndex] : "???";
			weightsVisible.Add (new QualityRecordString { qualityName = name, weight = record.weight });
		}
	}

	public override void OnAfterDeserialization (string key)
	{
		base.OnAfterDeserialization (key);
		UpdateVisibleWeights ();
		OnWeightsChanged ();
	}
}