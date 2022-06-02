using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NavMeshQueryDataChunk 
{
    private NavMeshQueryDataContainer p0;
    private NavMeshQueryDataContainer p1;
    private NavMeshQueryDataContainer p2;
    private NavMeshQueryDataContainer p3;
    private NavMeshQueryDataContainer p4;
    private NavMeshQueryDataContainer p5;
    private NavMeshQueryDataContainer p6;
    private NavMeshQueryDataContainer p7;
    /*private NavMeshQueryDataContainer p8;
    private NavMeshQueryDataContainer p9;
    private NavMeshQueryDataContainer p10;
    private NavMeshQueryDataContainer p11;
    private NavMeshQueryDataContainer p12;
    private NavMeshQueryDataContainer p13;
    private NavMeshQueryDataContainer p14;
    private NavMeshQueryDataContainer p15;
    private NavMeshQueryDataContainer p16;
    private NavMeshQueryDataContainer p17;
    private NavMeshQueryDataContainer p18;
    private NavMeshQueryDataContainer p19;
    private NavMeshQueryDataContainer p20;
    private NavMeshQueryDataContainer p21;
    private NavMeshQueryDataContainer p22;
    private NavMeshQueryDataContainer p23;
    private NavMeshQueryDataContainer p24;
    private NavMeshQueryDataContainer p25;
    private NavMeshQueryDataContainer p26;
    private NavMeshQueryDataContainer p27;
    private NavMeshQueryDataContainer p28;
    private NavMeshQueryDataContainer p29;
    private NavMeshQueryDataContainer p30;
    private NavMeshQueryDataContainer p31;*/

    public int Length { get; private set; }

    public static int Capacity
    {
        get { return 8; }
    }

    public NavMeshQueryDataContainer this[int index]
    {
        get { return GetValue(index); }
        set
        {
            Length = UnityEngine.Mathf.Max(Length, index + 1);
            SetValue(index, value);
        }
    }

    private NavMeshQueryDataContainer GetValue(int index)
    {
        switch (index)
        {
            case 0: return p0;
            case 1: return p1;
            case 2: return p2;
            case 3: return p3;
            case 4: return p4;
            case 5: return p5;
            case 6: return p6;
            case 7: return p7;
            /*case 8: return p8;
            case 9: return p9;
            case 10: return p10;
            case 11: return p11;
            case 12: return p12;
            case 13: return p13;
            case 14: return p14;
            case 15: return p15;
            case 16: return p16;
            case 17: return p17;
            case 18: return p18;
            case 19: return p19;
            case 20: return p20;
            case 21: return p21;
            case 22: return p22;
            case 23: return p23;
            case 24: return p24;
            case 25: return p25;
            case 26: return p26;
            case 27: return p27;
            case 28: return p28;
            case 29: return p29;
            case 30: return p30;
            case 31: return p31;
           */
            default: return default;
        }
    }

    private void SetValue(int index, NavMeshQueryDataContainer elementValue)
    {
        switch (index)
        {
            case 0: p0 = elementValue; break;
            case 1: p1 = elementValue; break;
            case 2: p2 = elementValue; break;
            case 3: p3 = elementValue; break;
            case 4: p4 = elementValue; break;
            case 5: p5 = elementValue; break;
            case 6: p6 = elementValue; break;
            case 7: p7 = elementValue; break;
            /*case 8: p8 = elementValue; break;
            case 9: p9 = elementValue; break;
            case 10: p10 = elementValue; break;
            case 11: p11 = elementValue; break;
            case 12: p12 = elementValue; break;
            case 13: p13 = elementValue; break;
            case 14: p14 = elementValue; break;
            case 15: p15 = elementValue; break;
            case 16: p16 = elementValue; break;
            case 17: p17 = elementValue; break;
            case 18: p18 = elementValue; break;
            case 19: p19 = elementValue; break;
            case 20: p20 = elementValue; break;
            case 21: p21 = elementValue; break;
            case 22: p22 = elementValue; break;
            case 23: p23 = elementValue; break;
            case 24: p24 = elementValue; break;
            case 25: p25 = elementValue; break;
            case 26: p26 = elementValue; break;
            case 27: p27 = elementValue; break;
            case 28: p28 = elementValue; break;
            case 29: p29 = elementValue; break;
            case 30: p30 = elementValue; break;
            case 31: p31 = elementValue; break;
           */
            default: break;
        }
    }

    public void Add(NavMeshQueryDataContainer elementValue, bool distinct = false)
    {
        if (distinct)
        {
            for (int i = 0; i < Length; i++)
            {
                if (this[i].GetHashCode() == elementValue.GetHashCode())
                    return;
            }
        }

        this[Length] = elementValue;
    }

    public void Clear()
    {
        Length = 0;
	}

	public bool Contains(NavMeshQueryDataContainer elementValue)
	{
		for (int i = 0; i < Length; i++)
		{
			if (this[i].GetHashCode() == elementValue.GetHashCode())
				return true;
		}

		return false;
	}
}
