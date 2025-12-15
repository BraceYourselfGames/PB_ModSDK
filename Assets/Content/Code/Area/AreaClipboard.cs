using System.Collections.Generic;
using UnityEngine;

namespace Area
{
	[System.Serializable]
	public class AreaPlacementPropSerialized
	{
		public int id;
		public Vector3Int clipboardPosition; //only used for copy/paste
		public byte rotation;
		public bool flipped;
		public float offsetX;
		public float offsetZ;

		public Vector4 hsbPrimary;
		public Vector4 hsbSecondary;
	}

	[System.Serializable]
	public class AreaVolumePointSerialized
	{
		public AreaVolumePointState pointState = AreaVolumePointState.Empty;
		public Vector3Int pointPositionIndex = new Vector3Int (-1, -1, -1);

		public byte spotConfiguration = 0;
		public byte spotConfigurationWithDamage = 0;

		public int blockTileset = 0;
		public byte blockGroup = 0;
		public byte blockSubtype = 0;
		public byte blockRotation = 0;
		public bool blockFlipped = false;
		public float terrainOffset = 0f;

		public TilesetVertexProperties customization = TilesetVertexProperties.defaults;
	}

	[System.Serializable]
	public class AreaClipboardSerialized
	{
		public Vector3Int clipboardBoundsSaved;
		public Vector3Int clipboardDirection;

		public List<AreaVolumePointSerialized> clipboardPointsSaved = new List<AreaVolumePointSerialized>();
		public List<AreaPlacementPropSerialized> clipboardPropsSaved = new List<AreaPlacementPropSerialized>();
	}

	public class AreaClipboard
	{
		public string name;
		public List<AreaVolumePoint> clipboardPointsSaved = new List<AreaVolumePoint>();
		public List<AreaPlacementProp> clipboardPropsSaved = new List<AreaPlacementProp>();
		public Vector3Int clipboardBoundsSaved;
		public Vector3Int clipboardDirection;

		public bool IsValid => clipboardPointsSaved.Count > 0;

		public AreaClipboard()
		{
		}

		void Reset()
		{
			clipboardPointsSaved.Clear();
			clipboardPropsSaved.Clear();
		}

		public void CopyFromArea(AreaManager am, Vector3Int origin, Vector3Int size)
		{
			int volumeLength = size.x * size.y * size.z;
			clipboardBoundsSaved = new Vector3Int (size.x, size.y, size.z);
			clipboardDirection = new Vector3Int (1, 0, 0);
			clipboardPointsSaved.Clear();// = new List<AreaVolumePoint> (new AreaVolumePoint[volumeLength]);
			clipboardPropsSaved.Clear();

			for (int i = 0; i < volumeLength; ++i)
			{
				clipboardPointsSaved.Add(new AreaVolumePoint());

				Vector3Int clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
				Vector3Int sourcePointPosition = clipboardPointPosition + origin;
				int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (sourcePointPosition, am.boundsFull);
				var sourcePoint = am.points[sourcePointIndex];
				var clipboardPoint = new AreaVolumePoint ();

				clipboardPointsSaved[i] = clipboardPoint;
				clipboardPoint.pointState = sourcePoint.pointState;
				clipboardPoint.pointPositionIndex = clipboardPointPosition;
				clipboardPoint.spotConfiguration = sourcePoint.spotConfiguration;
				clipboardPoint.spotConfigurationWithDamage = sourcePoint.spotConfigurationWithDamage;
				clipboardPoint.blockFlippedHorizontally = sourcePoint.blockFlippedHorizontally;
				clipboardPoint.blockGroup = sourcePoint.blockGroup;
				clipboardPoint.blockRotation = sourcePoint.blockRotation;
				clipboardPoint.blockSubtype = sourcePoint.blockSubtype;
				clipboardPoint.blockTileset = sourcePoint.blockTileset;
				clipboardPoint.customization = sourcePoint.customization;
				clipboardPoint.terrainOffset = sourcePoint.terrainOffset;

				//Check if we're on the far edge of the copy bounds - spot data there is outside the copy box
				bool isEdgePoint = clipboardPointPosition.x+1 >= clipboardBoundsSaved.x ||
				                   clipboardPointPosition.y+1 >= clipboardBoundsSaved.y ||
				                   clipboardPointPosition.z+1 >= clipboardBoundsSaved.z;

				//Copy props
				if(!isEdgePoint && am.indexesOccupiedByProps.ContainsKey(sourcePointIndex))
				{
					foreach (var prop in am.indexesOccupiedByProps[sourcePointIndex])
					{
						var clone = prop.SimpleClone();

						clone.clipboardPosition = clipboardPointPosition;

						clipboardPropsSaved.Add(clone);
					}
				}
            }
		}

		private static void CopySpotData(AreaVolumePoint from, AreaVolumePoint to)
		{
			//to.spotConfiguration = from.spotConfiguration;
			//to.spotConfigurationWithDamage = from.spotConfigurationWithDamage;
			to.blockFlippedHorizontally = from.blockFlippedHorizontally;
			to.blockGroup = from.blockGroup;
			to.blockRotation = from.blockRotation;
			to.blockSubtype = from.blockSubtype;
			to.blockTileset = from.blockTileset;
			to.customization = from.customization;
		}

		private static byte RotateByte(byte value, bool rotateClockwise)
		{
			if(!rotateClockwise)
				return (byte)((value + 1) % 4);
			else
				return (byte)((value - 1 + 4) % 4);
		}

		public Vector3Int Rotate(bool clockwise)
		{
			//Rotates v1 by v2, treating them as 2d vectors and ignoring Y axis
			Vector3Int RotateXZByVector(Vector3Int v1, Vector3Int v2)
			{
				return new Vector3Int
				(
					v1.x * v2.x - v1.z * v2.z,
					v1.y,
					v1.x * v2.z + v1.z * v2.x
				);
			}

			var oldBounds = clipboardBoundsSaved;
			var oldCenter = clipboardBoundsSaved / 2;

			//Shift tracks the fact that we will have to move the copy volume origin (since coords can't be negative)
			int xShift = 0;
			int zShift = 0;
			var rotateV = clockwise?new Vector3Int(0,0,-1):new Vector3Int(0,0,1);
			var antiV = rotateV * -1;
			clipboardBoundsSaved = RotateXZByVector(clipboardBoundsSaved, rotateV);
			if(clipboardBoundsSaved.x < 0)
			{
				clipboardBoundsSaved.x = Mathf.Abs(clipboardBoundsSaved.x);
				xShift = clipboardBoundsSaved.x-1;
			}

			if (clipboardBoundsSaved.z < 0) 
			{
				clipboardBoundsSaved.z = Mathf.Abs(clipboardBoundsSaved.z);
				zShift = clipboardBoundsSaved.z - 1;
			}

			clipboardDirection = RotateXZByVector(clipboardDirection, rotateV);

			//Adjust the paste position to keep the center in the same place
			var newCenter = clipboardBoundsSaved / 2;

			//We need to remap the point order inside the clipboard array to maintain the same XZY axis ordering
			var newList = new List<AreaVolumePoint> (new AreaVolumePoint[clipboardPointsSaved.Count]);
			for (int i = 0; i < newList.Count; ++i)
			{
				Vector3Int clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);

				clipboardPointPosition.x -= xShift;
				clipboardPointPosition.z -= zShift;

				var oldCoord = RotateXZByVector(clipboardPointPosition, antiV);

				var oldIndex = AreaUtility.GetIndexFromInternalPosition(oldCoord, oldBounds);

				newList[i] = clipboardPointsSaved[oldIndex];
				newList[i].pointPositionIndex = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
			}

			//Move the spot data around since after the rotation, it's effectively offset
			for (int i = 0; i < newList.Count; ++i)
			{
				Vector3Int clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
				
				if(clockwise)
					clipboardPointPosition.z += 1;
				else
					clipboardPointPosition.x += 1;

				if(clipboardPointPosition.x >= clipboardBoundsSaved.x || clipboardPointPosition.z >= clipboardBoundsSaved.z)
					continue;		

				var spotShiftIndex = AreaUtility.GetIndexFromInternalPosition(clipboardPointPosition, clipboardBoundsSaved);
				CopySpotData(newList[spotShiftIndex], newList[i]);

				newList[i].blockRotation = RotateByte(newList[i].blockRotation, clockwise);
			}

			//Fix up spot configuration data
			for (int i = 0; i < newList.Count; ++i)
			{
				Vector3Int clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);

				byte configByte = 0;
				byte configByteDmg = 0;
				bool anyDestroyed = false;

				for (int j = 0; j < 8; ++j)
				{
					int jRemapped = AreaUtility.configurationIndexRemapping[j];
					int xOffset = jRemapped % 2;
					int zOffset = (jRemapped / 2) % 2;
					int yOffset = jRemapped / 4;

					var offsetPos = clipboardPointPosition + new Vector3Int(xOffset, yOffset, zOffset);

					if(offsetPos.x >= clipboardBoundsSaved.x || offsetPos.y >= clipboardBoundsSaved.y || offsetPos.z >= clipboardBoundsSaved.z)
						continue;

					var offsetIndex = AreaUtility.GetIndexFromInternalPosition(offsetPos, clipboardBoundsSaved);

					var state = newList[offsetIndex].pointState;
					if(state != AreaVolumePointState.Empty)
						configByte |= (byte)(1 << (7 - j));

					if(state == AreaVolumePointState.Full)
						configByteDmg |= (byte)(1 << (7 - j));

					if(state == AreaVolumePointState.FullDestroyed)
						anyDestroyed = true;
				}

				newList[i].spotHasDamagedPoints = anyDestroyed;
				newList[i].spotConfiguration = configByte;
				newList[i].spotConfigurationWithDamage = anyDestroyed ? configByteDmg : configByte;
			}

			clipboardPointsSaved = newList;

			//Rotate the copied props
			for(int i = 0; i < clipboardPropsSaved.Count; ++i)
			{
				var prop = clipboardPropsSaved[i];

				prop.clipboardPosition = RotateXZByVector(prop.clipboardPosition, rotateV);
				prop.clipboardPosition.x += xShift;
				prop.clipboardPosition.z += zShift;

				//Since props are on spots, not points, we have to compensate for the spots being shifted
				if(clockwise)
					prop.clipboardPosition.z -= 1;
				else
					prop.clipboardPosition.x -= 1;

				prop.rotation = RotateByte(prop.rotation, clockwise);
			}

			return oldCenter - newCenter;
		}

		static AreaVolumePointSerialized Convert(AreaVolumePoint sourcePoint)
		{
			return new AreaVolumePointSerialized
			{
				pointState = sourcePoint.pointState,
				pointPositionIndex = sourcePoint.pointPositionIndex,
				spotConfiguration = sourcePoint.spotConfiguration,
				spotConfigurationWithDamage = sourcePoint.spotConfigurationWithDamage,
				blockFlipped = sourcePoint.blockFlippedHorizontally,
				blockGroup = sourcePoint.blockGroup,
				blockRotation = sourcePoint.blockRotation,
				blockSubtype = sourcePoint.blockSubtype,
				blockTileset = sourcePoint.blockTileset,
				customization = sourcePoint.customization,
				terrainOffset = sourcePoint.terrainOffset
			};
		}

		static AreaPlacementPropSerialized Convert(AreaPlacementProp sourceProp)
		{
			return new AreaPlacementPropSerialized
			{
				id = sourceProp.id,
				clipboardPosition = sourceProp.clipboardPosition,
				rotation = sourceProp.rotation,
				flipped = sourceProp.flipped,
				offsetX = sourceProp.offsetX,
				offsetZ = sourceProp.offsetZ,
				hsbPrimary = sourceProp.hsbPrimary,
				hsbSecondary = sourceProp.hsbSecondary
			};
		}

		static AreaVolumePoint Convert(AreaVolumePointSerialized dataPoint)
		{
			return new AreaVolumePoint
			{
				pointState = dataPoint.pointState,
				pointPositionIndex = dataPoint.pointPositionIndex,
				spotConfiguration = dataPoint.spotConfiguration,
				spotConfigurationWithDamage = dataPoint.spotConfigurationWithDamage,
				blockFlippedHorizontally = dataPoint.blockFlipped,
				blockGroup = dataPoint.blockGroup,
				blockRotation = dataPoint.blockRotation,
				blockSubtype = dataPoint.blockSubtype,
				blockTileset = dataPoint.blockTileset,
				customization = dataPoint.customization,
				terrainOffset = dataPoint.terrainOffset
			};
		}

		static AreaPlacementProp Convert(AreaPlacementPropSerialized dataProp)
		{
			return new AreaPlacementProp
			{
				id = dataProp.id,
				clipboardPosition = dataProp.clipboardPosition,
				rotation = dataProp.rotation,
				flipped = dataProp.flipped,
				offsetX = dataProp.offsetX,
				offsetZ = dataProp.offsetZ,
				hsbPrimary = dataProp.hsbPrimary,
				hsbSecondary = dataProp.hsbSecondary
			};
		}

		public AreaClipboardSerialized ToSerializedData()
		{
			var data = new AreaClipboardSerialized();

			data.clipboardBoundsSaved = clipboardBoundsSaved;
			data.clipboardDirection = clipboardDirection;

			foreach (var fromPt in clipboardPointsSaved)
				data.clipboardPointsSaved.Add(Convert(fromPt));			
			
			foreach (var fromProp in clipboardPropsSaved)
				data.clipboardPropsSaved.Add(Convert(fromProp));	

			return data;
		}

		public void SetFromSerializedData(AreaClipboardSerialized data)
		{
			Reset();

			clipboardBoundsSaved = data.clipboardBoundsSaved;
			clipboardDirection = data.clipboardDirection;

			foreach (var fromPt in data.clipboardPointsSaved)
				clipboardPointsSaved.Add(Convert(fromPt));			
			
			foreach (var fromProp in data.clipboardPropsSaved)
				clipboardPropsSaved.Add(Convert(fromProp));
		}

		public void SaveToYAML(string filename)
		{
			var data = ToSerializedData();
			UtilitiesYAML.SaveDataToFile(filename, data, false);
		}

		public void LoadFromYAML(string filename)
		{
			var data = UtilitiesYAML.LoadDataFromFile<AreaClipboardSerialized>(filename, appendApplicationPath: false);

			if(data == null)
				return;

			SetFromSerializedData(data);
		}
	}
}