using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

public struct SessionPathDataContainer
{
	public int sessionID;
	public NativeArray<CharacterPathDataContainer> characterPathDataContainers;

	public void Execute()
	{
		for (int i = 0; i < characterPathDataContainers.Length; i++)
		{
			characterPathDataContainers[i].Calculate();
		}
	}
}
