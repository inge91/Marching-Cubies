﻿using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp;


public class MetaBalls : MonoBehaviour {
	
	public float size = 1.5f;
	public GameObject[] metaBalls;
	public float spacing = 1;
	private float sizeSQ;
	
	public WannabeList<Vector3> vertices;

	public List<int> triangles;
	public Material objectMaterial;

	public bool drawGizmos;
	GameObject meshObject;
	// Use this for initialization
	void Start () {
		
		Init ();
		meshObject = new GameObject ();
		meshObject.AddComponent<MeshFilter> ().mesh = new Mesh ();
		meshObject.AddComponent<MeshRenderer> ().material = objectMaterial;
		MarchingCube ();
	}

	List<Vector3> cells;


	// Update is called once per frame
	void Update () {
		MarchingCube ();
		
	}
	Vector3[] cubeVoxels = new Vector3[8];

	int currentNumberOfTriangles = 0;
	void MarchingCube()
	{
		vertices.Clear ();
		triangles.Clear ();
		for (int i = 0; i < metaBalls.Length; i ++) {
			Vector3 position = metaBalls[i].transform.position;
			GetComponent<MeshFilter> ().mesh.Clear ();
			float xMin = Mathf.FloorToInt (position.x) - size - 1;
			float xMax = Mathf.FloorToInt (position.x) + size + 1;
			float yMin = Mathf.FloorToInt (position.y) - size - 1;
			float yMax = Mathf.FloorToInt (position.y) + size + 1;
			float zMin = Mathf.FloorToInt (position.z) - size - 1;
			float zMax = Mathf.FloorToInt (position.z) + size + 1;
			for (float x = xMin; x < xMax; x += spacing) {
				for (float y = yMin; y < yMax; y += spacing) {
					for (float z = zMin; z < zMax; z += spacing) {

						cubeVoxels [0] = new Vector3 (x, y, z);
						cubeVoxels [1] = new Vector3 (x, y, z + spacing); 
						cubeVoxels [2] = new Vector3 (x + spacing, y, z);	
						cubeVoxels [3] = new Vector3 (x + spacing, y, z + spacing);
					
						cubeVoxels [4] = new Vector3 (x, y + spacing, z);
						cubeVoxels [5] = new Vector3 (x, y + spacing, z + spacing); 
						cubeVoxels [6] = new Vector3 (x + spacing, y + spacing, z);	
						cubeVoxels [7] = new Vector3 (x + spacing, y + spacing, z + spacing);	

						DetermineMeshFromCube (cubeVoxels);
					}
				}
			}
		}
		Mesh mesh = meshObject.GetComponent<MeshFilter> ().mesh;

		if(currentNumberOfTriangles < triangles.Count)
		{
			mesh.vertices = vertices.arr;
			mesh.triangles = triangles.ToArray();
		}
		else{
			mesh.triangles = triangles.ToArray();
			mesh.vertices = vertices.arr;
		}
		currentNumberOfTriangles = triangles.Count;

		mesh.Optimize (); 
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}
	WannabeList<int> active;
	WannabeList<int> nonActive;
	WannabeList<WannabeList<Vector3>> midPoints;

	WannabeList<Vector3> activeNeighbours;
	void Init()
	{
		cells = new List<Vector3> ();
		sizeSQ = size * size;
		vertices = new WannabeList<Vector3> (65000);
		active = new WannabeList<int> (8);
		nonActive = new WannabeList<int> (8);
		midPoints = new WannabeList<WannabeList<Vector3>> (8);
		for (int i = 0; i < midPoints.MaxSize; i++) {
			midPoints.Add(new WannabeList<Vector3>(3));
		}

		activeNeighbours = new WannabeList<Vector3> (3);
	}


	float threshold = 1f;
	float calculateCellValue(Vector3 cellPos)
	{	
		float total = 0;
		for (int i = 0; i < metaBalls.Length; i++) {
			total += sizeSQ / Vector3.SqrMagnitude(metaBalls[i].transform.position - cellPos);
		}
		return total;
	}


	bool cellIsOn(Vector3 cellPos)
	{
		float value = calculateCellValue(cellPos);
		if (value > threshold) {
			return true;
		}
		return false;
	}

	void DetermineMeshFromCube(Vector3[] objects)
	{	

		int c = 0;
		midPoints.Clear ();

		active.Clear ();
		nonActive.Clear ();    

		for(int i = 0; i < 8 ; i ++)
		{

			if (cellIsOn(objects[i])) {
				cells.Add(objects[i]);
				active.Add(i);
			}
			else{
				nonActive.Add(i);
			}
		}

		bool pointsAreActive = true;
		if (active.Count != 8 && active.Count != 0) {
			// In case there are more points acitve than non active, 
			// the cases are handled in an inverted way.
			if (active.Count > 4) {
				pointsAreActive = false;
				active.CopyFrom(nonActive);
				for(int i = 0; i < active.Count; i ++)
				{
					GetAvailableMidpoints(midPoints[midPoints.Count], active[i], objects, true);
					midPoints.IncrementIndex();
				}
			}
			else
			{
				for(int i = 0; i < active.Count; i ++)
				{
					GetAvailableMidpoints(midPoints[midPoints.Count], active[i], objects, false);
					midPoints.IncrementIndex();
				}
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
	Vector3[] singleTriangle = new Vector3[3];
	int vertexCount = 0;
	
	void oneActiveCase(WannabeList<Vector3> midPoints, Vector3[] objects, int activeIndex, bool pointsAreActive)
	{

		vertexCount = 0;
		Vector3 activePointLocation = metaBalls[0].transform.position;
		AddTriangles(midPoints[0], activePointLocation, pointsAreActive);
		AddTriangles(midPoints[1], activePointLocation, pointsAreActive);
		AddTriangles(midPoints[2], activePointLocation, pointsAreActive);
		

	}
	
	void twoActiveCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
		vertexCount = 0;
		Vector3 activePointLocation = metaBalls[0].transform.position;
		// In case our point has 2 possible midpoints, it is neighbours with the other active voxel.
		if (midPoints[0].Count == 2) {
			float distance0 = Vector3.SqrMagnitude(objects[activeIndices[0]] - midPoints[0][0]);
			float distance1 = Vector3.SqrMagnitude(objects[activeIndices[0]] - midPoints[0][1]);
			float distance2 = Vector3.SqrMagnitude(objects[activeIndices[1]] - midPoints[1][0]);
			float distance3 = Vector3.SqrMagnitude(objects[activeIndices[1]] - midPoints[1][1]);
			float distance4 = Vector3.SqrMagnitude(midPoints[1][1] - midPoints[0][0]);
			float distance5 = Vector3.SqrMagnitude(midPoints[0][1] - midPoints[1][0]);
			if((distance1 > distance0 && distance1 > distance3)  
			   ||(distance2 > distance0 && distance2 > distance3)
			   )
			{

				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			}
			else{
				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			}
	
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
		vertexCount = 0;
		
		
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
			Vector3 activePointLocation = metaBalls[0].transform.position;
			int nodeWithOneActiveNeighbour2 = 3 - nodeWithTwoActiveNeighbours - nodeWithOneActiveNeighbour;
			int closestIndex1 = getIndexOfClosestMidpoint(midPoints[nodeWithTwoActiveNeighbours][0], midPoints[nodeWithOneActiveNeighbour]);
			int closestIndex2 = getIndexOfClosestMidpoint(midPoints[nodeWithTwoActiveNeighbours][0], midPoints[nodeWithOneActiveNeighbour2]);
			
			AddTriangles(midPoints[nodeWithTwoActiveNeighbours][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour][closestIndex1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour2][closestIndex2], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[nodeWithOneActiveNeighbour][closestIndex1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour2][closestIndex2], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour][1 - closestIndex1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[nodeWithOneActiveNeighbour][1 - closestIndex1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour2][1-closestIndex2], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[nodeWithOneActiveNeighbour2][closestIndex2], activePointLocation, pointsAreActive);
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

	/*			6		7
	 * 		4		5			
	 * 
	 * 	
	 *	 		2		3	
	 * 		0		1
	 */
	void SpecialCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
		int sum = 0;
		for(int i = 0; i < activeIndices.Count; i ++)
		{
			sum |= (int)Mathf.Pow(2, activeIndices[i]);
		}

		Vector3 activePointLocation = metaBalls[0].transform.position;

		switch (sum) {
			///0 2 3 6 DONE
		case 77:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			break;
			//1 2 3 7 WORKS
		case 142:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);

			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);

			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);

			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);

			break;
			//0 1 3 5 WORKS
		case 43:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			break;
			//0 1 2 4 DONE
		case 23:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			break;
		// 3 5 6 7 DONE
			//DONE
		case 232:

			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			break;
		// 0 4 5 6 DONE
		case 178:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			break;
		//DONE
		case 212:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);

			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);

			AddTriangles(midPoints[1][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			break;
			//DONE
		case 113:
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			
			AddTriangles(midPoints[2][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][1], activePointLocation, pointsAreActive);
			AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
			break;
		}
	


	}
	
	void fourActiveCase(WannabeList<WannabeList<Vector3>> midPoints, Vector3[] objects, WannabeList<int> activeIndices, bool pointsAreActive)
	{
	
		vertexCount = 0;
		
		int midPointSum = calculateNumberOfMidpoints(midPoints);
		
		// All vertices are on the same side of the cube
		if (midPointSum == 4) {

			float distance0 = Vector3.SqrMagnitude(objects[activeIndices[0]] - midPoints[0][0]);
			float distance1 = Vector3.SqrMagnitude(objects[activeIndices[1]] - midPoints[1][0]);
			float distance2 = Vector3.SqrMagnitude(objects[activeIndices[2]] - midPoints[2][0]);
			float distance3 = Vector3.SqrMagnitude(objects[activeIndices[3]] - midPoints[3][0]);
			if((distance1 > distance2 && distance1 > distance3)  
			   ||(distance0 > distance2 && distance0 > distance3)
			   )
			{	
				
				Vector3 activePointLocation = metaBalls[0].transform.position;
				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);

			}
			else{
				
				Vector3 activePointLocation = metaBalls[0].transform.position;
				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[1][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[2][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[3][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[0][0], activePointLocation, pointsAreActive);

			}
	

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
				SpecialCase(midPoints, objects, activeIndices, pointsAreActive);
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
				
				Vector3 activePointLocation = metaBalls[0].transform.position;
				AddTriangles(midPoints[oneUnactiveNeighbour[1]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[0]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[0]][1], activePointLocation, pointsAreActive);

				AddTriangles(midPoints[oneUnactiveNeighbour[1]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[1]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[1]][1], activePointLocation, pointsAreActive);

				AddTriangles(midPoints[oneUnactiveNeighbour[1]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[0]][index1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[1]][index2], activePointLocation, pointsAreActive);

				AddTriangles(midPoints[oneUnactiveNeighbour[0]][0], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[0]][index1], activePointLocation, pointsAreActive);
				AddTriangles(midPoints[twoUnactiveNeighbours[1]][index2], activePointLocation, pointsAreActive);
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
							Vector3 activePointLocation = metaBalls[0].transform.position;
							AddTriangles(midPoints[i][0], activePointLocation, pointsAreActive);
							AddTriangles(midPoints[i][1], activePointLocation, pointsAreActive);
							AddTriangles(midPoints[j][0], activePointLocation, pointsAreActive);
							
							AddTriangles( midPoints[j][0], activePointLocation, pointsAreActive);
							AddTriangles(midPoints[j][1], activePointLocation, pointsAreActive);
							AddTriangles(midPoints[i][1], activePointLocation, pointsAreActive);
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

	// GetAvailableMidpoints should not be called at the point it is called right now.
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
		
		availableNeighbours.Clear();
		if(cellIsOn(objects[i + offsetX]) == checkForActive)
		{
			float v = calculateCellValue(objects[i]);
			float g = calculateCellValue(objects[i + offsetX]);
			float range = (threshold - v) / (g-v);
			range = 1 - range;
			if(range < 0.1f)
			{
				range = 0;
			}
			else if ( range > 0.9f)
			{
				range = 1;
			}
			if(offsetX > 0)
			{
				availableNeighbours.Add(objects[i + offsetX] - (range * spacing) * new Vector3(1, 0, 0));
			}
			else
			{
				availableNeighbours.Add(objects[i + offsetX] + (range * spacing) * new Vector3(1, 0, 0));
			}
		}
		if(cellIsOn(objects[i + offsetY]) == checkForActive)
		{
			float v = calculateCellValue(objects[i]);
			float g = calculateCellValue(objects[i + offsetY]);
			float range = (threshold - v) / (g-v);
			range = 1 - range;
			if(offsetY > 0)
			{
				availableNeighbours.Add(objects[i + offsetY] - (range * spacing) * new Vector3(0, 1, 0));
			}
			else
			{
				availableNeighbours.Add(objects[i + offsetY] + (range * spacing) * new Vector3(0, 1, 0));
			}
		}
		if(cellIsOn(objects[i + offsetZ]) == checkForActive)
		{
			float v = calculateCellValue(objects[i]);
			float g = calculateCellValue(objects[i + offsetZ]);
			float range = (threshold - v) / (g-v);
			range = 1 - range;
			if(offsetZ > 0)
			{
				availableNeighbours.Add(objects[i + offsetZ] - (range * spacing) * new Vector3(0, 0, 1));
			} 
			else
			{
				availableNeighbours.Add(objects[i + offsetZ] + (range * spacing) * new Vector3(0, 0, 1));
			}
		}
	}
	
	
	public void AddTriangles(Vector3 vertexPos, Vector3 activePoint, bool pointsAreActive)
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


	int addVertexAndGetIndex(Vector3 v)
	{
		for (int i = 0; i < vertices.Count; i ++) {
			if(vertices[i] == v)
			{
				return i;
			}
		}

		vertices.Add (v);
		return vertices.Count - 1;
	}

	void CreateTriangle(Vector3[] v, Vector3 pointPosition, bool pointsAreActive)
	{
		int[] indices = new int[3];
		indices[0] = addVertexAndGetIndex(v[0]);
		indices[1] = addVertexAndGetIndex(v[1]);
		indices[2] = addVertexAndGetIndex(v[2]);
		
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

		if (distance > 0) {
			triangles.Add (indices[maxI]);	
			triangles.Add (indices[minI]);	
			triangles.Add (indices[3 - maxI - minI]);
				
		} else {
			triangles.Add (indices[minI]);	
			triangles.Add (indices[maxI]);	
			triangles.Add (indices[3 - maxI - minI]);
		}
	
	}

	void OnDrawGizmos()
	{
		if (drawGizmos) {
			Gizmos.color = Color.black;
			for (int i = 0; i < cells.Count; i ++) {
				Gizmos.DrawCube (cells [i], new Vector3 (0.1f, 0.1f, 0.1f));
			}
			Gizmos.color = Color.green;
			for (int i = 0; i < vertices.Count; i ++) {
				Gizmos.DrawCube (vertices [i], new Vector3 (0.1f, 0.1f, 0.1f));
			}
		}
		
	}
}

