using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Core
{
	[CreateAssetMenu(fileName = "AudioEventSheet", menuName = "Core/Audio/AudioEventSheet")]
	public class AudioEventSheet : VgSerializedScriptableObject
	{
		[OdinSerialize]
		[ListDrawerSettings(ShowFoldout = true)]
		public List<AudioEntry> Entries = new();
	}
}
