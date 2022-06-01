using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

[BurstCompile]
public struct SessionPathesCaclulationJob : IJobParallelFor
{
	public static int Capacity { get { return 4; } }

	[NativeDisableParallelForRestriction] public NavMeshQueryDataChunk chunk0;
	[NativeDisableParallelForRestriction] public NavMeshQueryDataChunk chunk1;
	[NativeDisableParallelForRestriction] public NavMeshQueryDataChunk chunk2;
	[NativeDisableParallelForRestriction] public NavMeshQueryDataChunk chunk3;
	
	public void Assign(int index, ref NavMeshQueryDataChunk chunk)
	{
		if (index < 0 || index >= Capacity)
			return;

		switch (index)
		{
			case 0:
				{
					chunk0 = chunk;
					break;
				}
			case 1:
				{
					chunk1 = chunk;
					break;
				}
			case 2:
				{
					chunk2 = chunk;
					break;
				}
			case 3:
				{
					chunk3 = chunk;
					break;
				}
			default:
				return;
		}
	}

	public void Execute(int index)
	{
		//for (int index = 0; index < 4; index++)
		{
			switch (index)
			{
				case 0:
					{
						HandleChunkCalculation(chunk0);
						break;
					}
				case 1:
					{
						HandleChunkCalculation(chunk1);
						break;
					}
				case 2:
					{
						HandleChunkCalculation(chunk2);
						break;
					}
				case 3:
					{
						HandleChunkCalculation(chunk3);
						break;
					}
				default:
					return;
			}
		}
	}

	private void HandleChunkCalculation(NavMeshQueryDataChunk chunk)
	{
		for (int i = 0; i < chunk.Length; i++)
		{
			int pathLength = chunk[i].pathLength;

			if (pathLength == 0)
				continue;
	
			var characterStraightPath = chunk[i].straightPath;
			var characterStraightPathFlags = chunk[i].straightPathFlags;
			var characterVertexSide = chunk[i].vertexSide;

			int characterPathLength = 0;

			PathQueryStatus pathStatus = PathUtils.FindStraightPath(
				chunk[i].query,
				chunk[i].startPos,
				chunk[i].endPos,
				chunk[i].queryPathResult,
				chunk[i].pathLength,
				ref characterStraightPath,
				ref characterStraightPathFlags,
				ref characterVertexSide,
				ref characterPathLength,
				chunk[i].maxPathLength);

			chunk[i].SwapStraightPath(ref characterStraightPath);
			chunk[i].SetStraightPathLength(characterPathLength);
			chunk[i].SetStatus(pathStatus);
		}
	}
}
