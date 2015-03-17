using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp;


public class VoxelObject : MonoBehaviour {
	
	public int size = 8;
	int sizeSQ;
	public float spacing = 1;
	private GameObject[,,] voxels;
	
	public List<Vector3> vertices;
	public List<int> triangles;
	public bool marchingCube = false;
	// Use this for initialization
	void Start () {
		
		Init ();
		GetComponent<MeshFilter>().mesh = new Mesh();
		MarchingCube ();
	}

	// Update is called once per frame
	void Update () {
		if(marchingCube)
		{
			MarchingCube();
		}
	}
	
	void MarchingCube()
	{
		vertices.Clear ();
		triangles.Clear ();
		Vector3 position = this.transform.position;
		GetComponent<MeshFilter> ().mesh.Clear ();
		for (float x = Mathf.FloorToInt(position.x) - 4 - size; x <  Mathf.FloorToInt(position.x) + size + 4; x += spacing) {
			for (float y = Mathf.FloorToInt(position.y) - 4 - size; y <  Mathf.FloorToInt(position.y) + size + 4; y += spacing) {
				for (float z = Mathf.FloorToInt(position.z) - 4 - size; z <  Mathf.FloorToInt(position.z) + size + 4; z += spacing) {
					Vector3[] cubeVoxels = new Vector3[8];
					cubeVoxels[0] = new Vector3(x, y, z);
					cubeVoxels[1] = new Vector3(x, y, z + spacing); 
					cubeVoxels[2] = new Vector3(x + spacing, y, z);	
					cubeVoxels[3] = new Vector3(x + spacing, y, z + spacing);
					
					cubeVoxels[4] = new Vector3(x, y + spacing, z);
					cubeVoxels[5] = new Vector3(x, y + spacing, z + spacing); 
					cubeVoxels[6] = new Vector3(x + spacing, y + spacing, z);	
					cubeVoxels[7] = new Vector3(x + spacing, y + spacing, z + spacing);
					
					DetermineMeshFromCube(cubeVoxels);
					
				}
			}
		}
		Debug.Log (vertices.Count);
		GetComponent<MeshFilter> ().mesh.RecalculateNormals ();
		GetComponent<MeshFilter> ().mesh.vertices = vertices.ToArray();
		GetComponent<MeshFilter> ().mesh.triangles = triangles.ToArray();
	}
	WannabeList<int> active;
	WannabeList<int> nonActive;
	WannabeList<WannabeList<Vector3>> midPoints;
	WannabeList<WannabeList<Vector3>> nonActiveMidPoints;

	WannabeList<Vector3> activeNeighbours;
	void Init()
	{
		sizeSQ = size * size;
		active = new WannabeList<int> (8);
		nonActive = new WannabeList<int> (8);
		midPoints = new WannabeList<WannabeList<Vector3>> (8);
		for (int i = 0; i < midPoints.MaxSize; i++) {
			midPoints.Add(new WannabeList<Vector3>(3));
		}
		nonActiveMidPoints = new WannabeList<WannabeList<Vector3>> (8);
		for (int i = 0; i < nonActiveMidPoints.MaxSize; i++) {
			nonActiveMidPoints.Add(new WannabeList<Vector3>(3));
		}

		activeNeighbours = new WannabeList<Vector3> (3);
	}
	
	void DetermineMeshFromCube(Vector3[] objects)
	{
		int c = 0;
		

		midPoints.Clear ();
		nonActiveMidPoints.Clear ();
		active.Clear ();
		nonActive.Clear ();

		float sizeSQ = size * size;
		for(int i = 0; i < 8 ; i ++)
		{
			if (Vector3.SqrMagnitude(this.transform.position - objects[i]) < sizeSQ) {
				active.Add(i);
				GetAvailableMidpoints(midPoints[midPoints.Count], i, objects, false);
				midPoints.IncrementIndex();
			}
			else{
				nonActive.Add(i);
				GetAvailableMidpoints(nonActiveMidPoints[nonActiveMidPoints.Count], i, objects, true);
				nonActiveMidPoints.IncrementIndex();
			}
		}

		bool pointsAreActive = true;
	
		if (active.Count != 8 && active.Count != 0) {
			if (active.Count > 4) {
			
				pointsAreActive = false;
				active = nonActive;
				midPoints = nonActiveMidPoints;
			}
		
			if (active.Count == 1) {
				oneActiveCase (midPoints[0], objects, active[0], pointsAreActive);
			} else if (active.Count == 2) {
				twoActiveCase (midPoints, objects, active, pointsAreActive);
			} else if (active.Count == 3) {
				threeActiveCase (midPoints, objects, active, pointsAreActive);
			} else if (active.Count == 4) {
				fourActiveCase (midPoints, objects, active, pointsAreActive);
			}
		}
	}
	
	void oneActiveCase(WannabeList<Vector3> midPoints, Vector3[] objects, int activeIndex, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		Vector3 activePointLocation = objects[activeIndex];
		AddTriangles(ref vertexCount, midPoints[0], singleTriangle, activePointLocation, pointsAreActive);
		AddTriangles(ref vertexCount, midPoints[1], singleTriangle, activePointLocation, pointsAreActive);
		AddTriangles(ref vertexCount, midPoints[2], singleTriangle, activePointLocation, pointsAreActive);
	}
	
	void twoActiveCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		
		Vector3 activePointLocation = objects[activeIndices[0]];
		// In case our point has 2 possible midpoints, it is neighbours with the other active voxel.
		if (midPoints[0].Count == 2) {
			AddTriangles(ref vertexCount, midPoints[0][0], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[0][1], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][1], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[0][1], singleTriangle, objects[activeIndices[0]], pointsAreActive);
			
		} else {
			oneActiveCase (midPoints[0], objects, activeIndices[0], pointsAreActive);
			oneActiveCase (midPoints[1], objects, activeIndices[1], pointsAreActive);
		}
	}
	
	int getIndexOfClosestMidpoint(Vector3 midpoint, WannabeList<Vector3> midpoints)
	{
		float smallestDistance = Vector3.Distance (midpoint, midpoints [0]);
		int smallestDistanceIndex = 0;
		for(int i = 1 ; i < midpoints.Count; i ++)
		{
			float distance = Vector3.Distance(midpoint, midpoints[i]);
			if(smallestDistance > distance)
			{
				smallestDistance = distance;
				smallestDistanceIndex = i;
			}
		}
		return smallestDistanceIndex;
	}
	
	int getIndexOfFurthestMidpoint(Vector3 midpoint, WannabeList<Vector3> midpoints)
	{
		float furthestDistance = Vector3.Distance (midpoint, midpoints [0]);
		int furthestDistanceIndex = 0;
		for(int i = 1 ; i < midpoints.Count; i ++)
		{
			float distance = Vector3.Distance(midpoint, midpoints[i]);
			if(furthestDistance < distance)
			{
				furthestDistance = distance;
				furthestDistanceIndex = i;
			}
		}
		return furthestDistanceIndex;
	}
	
	void threeActiveCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		
		
		int midPointSum = calculateNumberOfMidpoints(midPoints);
		if (midPointSum == 9) {
			for(int i = 0; i < 3; i ++)
			{
				oneActiveCase(midPoints[i], objects, activeIndices[i], pointsAreActive);
			}
		}
		if (midPointSum == 5) {
			int nodeWithTwoActiveNeighbours = 0;
			int nodeWithOneActiveNeighbour = 0;
			for(int i = 0; i < 3; i ++)
			{
				if(midPoints[i].Count == 1)
				{
					nodeWithTwoActiveNeighbours = i;
				}
				else{
					nodeWithOneActiveNeighbour = i;
				}
			}
			Vector3 activePointLocation = objects[activeIndices[0]];
			int nodeWithOneActiveNeighbour2 = 3 - nodeWithTwoActiveNeighbours - nodeWithOneActiveNeighbour;
			int closestIndex1 = getIndexOfClosestMidpoint(midPoints[nodeWithTwoActiveNeighbours][0], midPoints[nodeWithOneActiveNeighbour]);
			int closestIndex2 = getIndexOfClosestMidpoint(midPoints[nodeWithTwoActiveNeighbours][0], midPoints[nodeWithOneActiveNeighbour2]);
			
			AddTriangles(ref vertexCount, midPoints[nodeWithTwoActiveNeighbours][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour][closestIndex1], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour2][closestIndex2], singleTriangle, activePointLocation, pointsAreActive);
			
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour][closestIndex1], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour2][closestIndex2], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour][1 - closestIndex1], singleTriangle, activePointLocation, pointsAreActive);
			
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour][1 - closestIndex1], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour2][1-closestIndex2], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[nodeWithOneActiveNeighbour2][closestIndex2], singleTriangle, activePointLocation, pointsAreActive);
		}
		if (midPointSum == 7) {
			int activeNoNeighbours = 0;
			WannabeList<WannabeList<Vector3>> activeNeighbours = new WannabeList<WannabeList<Vector3>>(2);
			WannabeList<int> activeNeighbourIndices = new WannabeList<int>(3);
			for(int i = 0; i < 3; i ++)
			{
				if(midPoints[i].Count == 3)
				{
					activeNoNeighbours = i;
				}
				else{
					activeNeighbours.Add(midPoints[i]);
					activeNeighbourIndices.Add (activeIndices[i]);
				}
			}
			oneActiveCase(midPoints[activeNoNeighbours], objects, activeIndices[activeNoNeighbours], pointsAreActive);
			twoActiveCase(activeNeighbours, objects, activeNeighbourIndices, pointsAreActive);
		}
	}
	
	bool areNeighbouringPositions(int i, int j)
	{
		int offsetX = -2;
		int offsetY = -4;
		int offsetZ = -1;
		if (i < 4) {
			offsetY = 4;
		} 
		if (i % 2 == 0) {
			offsetZ = 1;
		} 
		switch (i) {
		case 0:
		case 1:
		case 4:
		case 5:
			offsetX = 2;
			break;
		}
		if (i + offsetX == j
		    || i + offsetY == j
		    ||i + offsetZ == j) {
			return true;
		}
		return false;
	}
	
	
	void fourActiveCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		
		int midPointSum = calculateNumberOfMidpoints(midPoints);
		
		// All vertices are on the same side of the cube
		if (midPointSum == 4) {
			Vector3 activePointLocation = objects[activeIndices[0]];
			AddTriangles(ref vertexCount, midPoints[0][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[2][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[2][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[3][0], singleTriangle, activePointLocation, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, activePointLocation, pointsAreActive);
		}
		if (midPointSum == 6) {
			
			bool firstCase = false;
			for(int i = 0; i < 4; i ++)
			{
				if(midPoints[i].Count == 0)
				{
					firstCase = true;
					break;
				}
			}
			
			if(firstCase)
			{
				int[] available = new int[3];
				int j = 0; 
				for(int i = 0; i < 4; i ++)
				{
					if(midPoints[i].Count != 0)
					{
						available[j] = i;
						j ++;
					}
				}
				Vector3 activePointLocation = objects[activeIndices[0]];
				int index = getIndexOfClosestMidpoint(midPoints[available[0]][0], midPoints[available[1]]);
				
				AddTriangles(ref vertexCount, midPoints[available[0]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[0]][1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[1]][index], singleTriangle, activePointLocation, pointsAreActive);
				
				int index2 = getIndexOfFurthestMidpoint(midPoints[available[1]][0], midPoints[available[0]]);
				AddTriangles(ref vertexCount, midPoints[available[1]][index], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[1]][1 - index], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[0]][index2], singleTriangle, activePointLocation, pointsAreActive);
				
				index2 = getIndexOfClosestMidpoint(midPoints[available[0]][index2], midPoints[available[2]]);
				AddTriangles(ref vertexCount, midPoints[available[1]][1 - index], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[0]][index2], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[2]][index2], singleTriangle, activePointLocation, pointsAreActive);
				
				AddTriangles(ref vertexCount, midPoints[available[2]][1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[2]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[available[1]][1 - index], singleTriangle, activePointLocation, pointsAreActive);
				
			}
			else{
				int[] oneUnactiveNeighbour = new int[2];
				int oneUnactiveNeighbourIndex = 0;
				int[] twoUnactiveNeighbours = new int[2];
				int twoUnactiveNeighboursIndex = 0;
				for(int i = 0; i < 4; i ++)
				{
					if(midPoints[i].Count == 1)
					{
						oneUnactiveNeighbour[oneUnactiveNeighbourIndex] = i;
						oneUnactiveNeighbourIndex++;
					}
					else{
						twoUnactiveNeighbours[twoUnactiveNeighboursIndex] = i;
						twoUnactiveNeighboursIndex++;
					}
				}
				int index1 = getIndexOfFurthestMidpoint(midPoints[oneUnactiveNeighbour[1]][0], midPoints[twoUnactiveNeighbours[0]]);
				int index2 = getIndexOfFurthestMidpoint(midPoints[oneUnactiveNeighbour[1]][0], midPoints[twoUnactiveNeighbours[1]]);
				
				Vector3 activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]];
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][1], singleTriangle, activePointLocation, pointsAreActive);
				
				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]];
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][1], singleTriangle, activePointLocation, pointsAreActive);
				
				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]];
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][index1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][index2], singleTriangle, activePointLocation, pointsAreActive);
				
				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[0]]];
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[0]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][index1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][index2], singleTriangle, activePointLocation, pointsAreActive);
			}
		}
		if (midPointSum == 8) {
			bool isolatedPointCase = false;
			for(int i = 0; i < 4; i ++)
			{
				if(midPoints[i].Count == 3)
				{
					isolatedPointCase = true;
				}
			}
			if(isolatedPointCase)
			{
				int activeNoNeighbours = 0;
				WannabeList<WannabeList<Vector3>> activeNeighbours = new WannabeList<WannabeList<Vector3>>(3);	
				WannabeList<int> activeNeighbourIndices = new WannabeList<int>(3);
				for(int i = 0; i < 4; i ++)
				{
					if(midPoints[i].Count == 3)
					{
						activeNoNeighbours = i;
					}
					else{
						activeNeighbours.Add(midPoints[i]);
						activeNeighbourIndices.Add(activeIndices[i]);
					}
				}
				
				oneActiveCase(midPoints[activeNoNeighbours], objects, activeIndices[activeNoNeighbours], pointsAreActive);
				threeActiveCase(activeNeighbours, objects, activeNeighbourIndices, pointsAreActive);
			}
			else{
				for(int i = 0; i < 4; i ++)
				{
					for(int j = i; j < 4; j ++)
					{
						if(areNeighbouringPositions(activeIndices[i], activeIndices[j]))
						{
							Vector3 activePointLocation = objects[activeIndices[i]];
							AddTriangles(ref vertexCount, midPoints[i][0], singleTriangle, activePointLocation, pointsAreActive);
							AddTriangles(ref vertexCount, midPoints[i][1], singleTriangle, activePointLocation, pointsAreActive);
							AddTriangles(ref vertexCount, midPoints[j][0], singleTriangle, activePointLocation, pointsAreActive);
							
							AddTriangles(ref vertexCount, midPoints[j][0], singleTriangle, activePointLocation, pointsAreActive);
							AddTriangles(ref vertexCount, midPoints[j][1], singleTriangle, activePointLocation, pointsAreActive);
							AddTriangles(ref vertexCount, midPoints[i][1], singleTriangle, activePointLocation, pointsAreActive);
						}
					}
				}
				
			}
		}
		if (midPointSum == 12) {
			for(int i = 0; i < 4; i ++)
			{
				oneActiveCase(midPoints[i], objects, activeIndices[i], pointsAreActive);
			}
		}
	}
	
	int calculateNumberOfMidpoints(WannabeList<WannabeList<Vector3>> midPoints)
	{
		int sum = 0;
		for(int i = 0; i < midPoints.Count; i ++)
		{
			sum += midPoints[i].Count;
		}
		return sum;
	}
	
	void GetAvailableMidpoints(WannabeList<Vector3> availableNeighbours, int i, Vector3[] objects, bool checkForActive)
	{
		int offsetX = -2;
		int offsetY = -4;
		int offsetZ = -1;
		if (i < 4) {
			offsetY = 4;
		} 
		if (i % 2 == 0) {
			offsetZ = 1;
		} 
		switch (i) {
		case 0:
		case 1:
		case 4:
		case 5:
			offsetX = 2;
			break;
		}
		
		activeNeighbours.Clear();
		
		if(Vector3.SqrMagnitude(this.transform.position- objects[i + offsetX]) < sizeSQ == checkForActive)
		{
			if(offsetX > 0)
			{
				activeNeighbours.Add(objects[i + offsetX] - (0.5f * spacing) * new Vector3(1, 0, 0));
			}
			else
			{
				activeNeighbours.Add(objects[i + offsetX] + (0.5f * spacing) * new Vector3(1, 0, 0));
			}
		}
		if(Vector3.SqrMagnitude(this.transform.position- objects[i + offsetY]) < sizeSQ == checkForActive)
		{
			if(offsetY > 0)
			{
				activeNeighbours.Add(objects[i + offsetY] - (0.5f * spacing) * new Vector3(0, 1, 0));
			}
			else
			{
				activeNeighbours.Add(objects[i + offsetY] + (0.5f * spacing) * new Vector3(0, 1, 0));
			}
		}
		if(Vector3.SqrMagnitude(this.transform.position- objects[i + offsetZ]) < sizeSQ == checkForActive)
		{
			if(offsetZ > 0)
			{
				activeNeighbours.Add(objects[i + offsetZ] - (0.5f * spacing) * new Vector3(0, 0, 1));
			} 
			else
			{
				activeNeighbours.Add(objects[i + offsetZ] + (0.5f * spacing) * new Vector3(0, 0, 1));
			}
		}
	}
	
	
	public void AddTriangles(ref int vertexCount, Vector3 vertexPos, Vector3[] singleTriangle, Vector3 activePoint, bool pointsAreActive)
	{
		singleTriangle[vertexCount] = vertexPos;
		
		vertexCount ++;
		
		if(vertexCount == 3)
		{
			CreateTriangle(singleTriangle, activePoint, pointsAreActive);
			vertexCount = 0;
		}
	}
	
	int indexMaxX(Vector3[] v)
	{
		int maxXIndex = 0;
		float maxX = v [0].x;
		for(int i = 1 ;i < 3; i ++)
		{
			if(v[i].x > maxX)
			{
				maxX = v[i].x;
				maxXIndex = i;
			}
		}
		
		return maxXIndex;
	}
	
	int indexMinX(Vector3[] v)
	{
		int minXIndex = 0;
		float minX = v [0].x;
		for(int i = 1 ;i < 3; i ++)
		{
			if(v[i].x < minX)
			{
				minX = v[i].x;
				minXIndex = i;
			}
		}
		
		return minXIndex;
	}
	
	void CreateTriangle(Vector3[] v, Vector3 pointPosition, bool pointsAreActive)
	{
		int vertexIndex = vertices.Count;
		vertices.Add (v[0]);
		vertices.Add (v[1]);
		vertices.Add (v[2]);
		
		int minI = indexMinX(v);
		int maxI = indexMaxX(v);
		if (minI == maxI) {
			minI ++;
		}
		// Determine on what side of the triangle the active point is,
		// so that points are added to the triangle in the right order.
		Vector3 normal = Vector3.Normalize(Vector3.Cross (v [minI] - v[maxI], v [maxI] - v[3 - minI - maxI]));
		Vector3 w = - (v[0] - pointPosition);
		float distance = Vector3.Dot (normal, w);
		
		if (pointsAreActive) {
			if (distance > 0) {
				triangles.Add (vertexIndex + maxI);	
				triangles.Add (vertexIndex + minI);	
				triangles.Add (vertexIndex + 3 - maxI - minI);
				
			} else {
				triangles.Add (vertexIndex + minI);	
				triangles.Add (vertexIndex + maxI);	
				triangles.Add (vertexIndex + 3 - maxI - minI);
			}
		} else {
			if (distance > 0) {
				triangles.Add (vertexIndex + minI);	
				triangles.Add (vertexIndex + maxI);	
				triangles.Add (vertexIndex + 3 - maxI - minI);
				
			} else {
				triangles.Add (vertexIndex + maxI);	
				triangles.Add (vertexIndex + minI);	
				triangles.Add (vertexIndex + 3 - maxI - minI);
			}
		}
	}
	
}
