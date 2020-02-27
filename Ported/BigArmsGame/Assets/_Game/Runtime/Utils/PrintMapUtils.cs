using System.Text;
using Unity.Collections;
using UnityEngine;

public static class PrintMapUtils
{
    public static void PrintMap(NativeArray<TileDescriptor> array, int mapSizeX, int mapSizeY)
	{
        var stringbuilder = new StringBuilder();

        for(int j = mapSizeY - 1; j >= 0; --j)
        {
            for (int i = 0; i < mapSizeX; ++i)
            {
                var tileDescriptor = array[Pathing.Hash(mapSizeX, i, j)];
                switch(tileDescriptor.TileType)
                {
                    case TileTypes.None:
                        stringbuilder.Append("+");
                        break;
                    case TileTypes.Rock:
                        stringbuilder.Append("R");
                        break;
                    case TileTypes.Tilled:
                        stringbuilder.Append("#");
                        break;
                    case TileTypes.Store:
                        stringbuilder.Append("S");
                        break;
                    case TileTypes.Planted:
                        stringbuilder.Append("P");
                        break;
                }
            }
            stringbuilder.Append("\n");
        }

        Debug.Log(stringbuilder);
	}
}
