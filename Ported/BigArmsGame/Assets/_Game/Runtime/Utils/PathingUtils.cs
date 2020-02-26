using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public static class Pathing
{

	public delegate bool IsNavigableDelegate(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y);
	public delegate bool CheckMatchDelegate(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y);

	static readonly int[] dirsX = new int[] { 1, -1, 0, 0 };
	static readonly int[] dirsY = new int[] { 0, 0, 1, -1 };

	public static RectInt fullMapZone;

	public static int Hash(int mapSizeX, int x, int y)
	{
		return y * mapSizeX + x;
	}
	public static void Unhash(int mapSizeX, int mapSizeY, int hash, out int x, out int y)
	{
		y = hash / mapSizeX;
		x = hash % mapSizeY;
	}

	public static bool IsNavigableDefault(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x ,y)].TileType != TileTypes.Rock;
	}

	public static bool IsRock(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x, y)].TileType == TileTypes.Rock;
	}
	public static bool IsStore(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x, y)].TileType == TileTypes.Store;
	}
	public static bool IsPlant(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x, y)].TileType == TileTypes.Planted;
	}
	public static bool IsTillable(NativeList<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x, y)].TileType == TileTypes.None;
	}
	public static bool IsReadyForPlant(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
	{
		return array[Hash(mapSizeX, x, y)].TileType == TileTypes.Tilled;
	}
    public static bool IsBlocked(NativeArray<TileDescriptor> array, int mapSizeX, int mapSizeY, int x, int y)
	{
		if (x < 0 || y < 0 || x >= mapSizeX || y >= mapSizeY)
		{
			return true;
		}

		return IsRock(array, mapSizeX, x, y);
	}

	public static Entity FindNearbyRock(NativeArray<TileDescriptor> array, int mapSizeX, int mapSizeY, int x, int y, int range, ref NativeList<int> outputPath)
	{
		NativeArray<int> visitedTiles = new NativeArray<int>(mapSizeX * mapSizeY, Allocator.Temp);
		NativeList<int> activeTiles = new NativeList<int>(Allocator.Temp);
		NativeList<int> nextTiles = new NativeList<int>(Allocator.Temp);
		NativeList<int> outputTiles = new NativeList<int>(Allocator.Temp);
		var result = Entity.Null;

		int rockPosHash = SearchForOne(array, mapSizeX, mapSizeY, x, y, range, IsNavigableDefault, IsRock, fullMapZone, ref visitedTiles, ref activeTiles, ref nextTiles, ref outputTiles);
		if (rockPosHash != -1)
		{
			int rockX, rockY;
			Unhash(mapSizeX, mapSizeY, rockPosHash, out rockX, out rockY);
			AssignLatestPath(mapSizeX, mapSizeY, rockX, rockY, ref visitedTiles, ref outputPath);
			result = array[rockPosHash].Entity;
		}

		visitedTiles.Dispose();
		activeTiles.Dispose();
		nextTiles.Dispose();
		outputTiles.Dispose();
		return result;
	}


	public static int SearchForOne(
        NativeArray<TileDescriptor> array,
        int mapSizeX,
        int mapSizeY,
		int startX,
        int startY,
        int range,
        IsNavigableDelegate IsNavigable,
        CheckMatchDelegate CheckMatch,
        RectInt requiredZone,
        ref NativeArray<int> visitedTiles,
	    ref NativeList<int> activeTiles,
	    ref NativeList<int> nextTiles,
	    ref NativeList<int> outputTiles
        )
	{
		outputTiles = Search(
            array,
            mapSizeX,
            mapSizeY,
            startX,
            startY,
            range,
            IsNavigable,
            CheckMatch,
            requiredZone,
            ref visitedTiles,
            ref activeTiles,
            ref nextTiles,
            ref outputTiles,
            1);
		if (outputTiles.Length == 0)
		{
			return -1;
		}
		else
		{
			return outputTiles[0];
		}
	}

	public static NativeList<int> Search(
		NativeArray<TileDescriptor> array,
		int mapSizeX,
		int mapSizeY,
		int startX,
        int startY,
        int range,
        IsNavigableDelegate IsNavigable,
        CheckMatchDelegate CheckMatch,
        RectInt requiredZone,
		ref NativeArray<int> visitedTiles,
		ref NativeList<int> activeTiles,
		ref NativeList<int> nextTiles,
		ref NativeList<int> outputTiles,
		int maxResultCount = 0)
	{
		for (int x = 0; x < mapSizeX; x++)
		{
			for (int y = 0; y < mapSizeY; y++)
			{
				visitedTiles[Hash(mapSizeX,x,y)] = -1;
			}
		}
		outputTiles.Clear();
		visitedTiles[Hash(mapSizeX, startX, startY)] = 0;
		activeTiles.Clear();
		nextTiles.Clear();
		nextTiles.Add(Hash(mapSizeX,startX, startY));

		int steps = 0;

		while (nextTiles.Length > 0 && (steps < range || range == 0))
		{
			NativeList<int> temp = activeTiles;
			activeTiles = nextTiles;
			nextTiles = temp;
			nextTiles.Clear();

			steps++;

			for (int i = 0; i < activeTiles.Length; i++)
			{
				int x, y;
				Unhash(mapSizeX, mapSizeY, activeTiles[i], out x, out y);

				for (int j = 0; j < dirsX.Length; j++)
				{
					int x2 = x + dirsX[j];
					int y2 = y + dirsY[j];

					if (x2 < 0 || y2 < 0 || x2 >= mapSizeX || y2 >= mapSizeY)
					{
						continue;
					}

					if (visitedTiles[Hash(mapSizeX, x2, y2)] == -1 || visitedTiles[Hash(mapSizeX, x2, y2)] > steps)
					{

						int hash = Hash(mapSizeX, x2, y2);
						if (IsNavigable(array, mapSizeX, x2, y2))
						{
							visitedTiles[Hash(mapSizeX, x2, y2)] = steps;
							nextTiles.Add(hash);
						}
						if (x2 >= requiredZone.xMin && x2 <= requiredZone.xMax)
						{
							if (y2 >= requiredZone.yMin && y2 <= requiredZone.yMax)
							{
								if (CheckMatch(array, mapSizeX, x2, y2))
								{
									outputTiles.Add(hash);
									if (maxResultCount != 0 && outputTiles.Length >= maxResultCount)
									{
										return outputTiles;
									}
								}
							}
						}
					}
				}
			}
		}

		return outputTiles;
	}

	public static void AssignLatestPath(int mapSizeX, int mapSizeY, int endX, int endY, ref NativeArray<int> visitedTiles, ref NativeList<int> outputPath)
	{
		outputPath.Clear();

		int x = endX;
		int y = endY;

		outputPath.Add(Hash(mapSizeX, x, y));

		int dist = int.MaxValue;
		while (dist > 0)
		{
			int minNeighborDist = int.MaxValue;
			int bestNewX = x;
			int bestNewY = y;
			for (int i = 0; i < dirsX.Length; i++)
			{
				int x2 = x + dirsX[i];
				int y2 = y + dirsY[i];
				if (x2 < 0 || y2 < 0 || x2 >= mapSizeX || y2 >= mapSizeY)
				{
					continue;
				}

				int newDist = visitedTiles[Hash(mapSizeX, x2, y2)];
				if (newDist != -1 && newDist < minNeighborDist)
				{
					minNeighborDist = newDist;
					bestNewX = x2;
					bestNewY = y2;
				}
			}
			x = bestNewX;
			y = bestNewY;
			dist = minNeighborDist;
			outputPath.Add(Hash(mapSizeX, x, y));
		}
	}
}
