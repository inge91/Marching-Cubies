using UnityEngine;
using System.Collections.Generic;


public class VoxelGrid : MonoBehaviour {

	public int resolution = 8;
	public GameObject voxelPrefab;
	private GameObject[,,] voxels;

	public List<Vector3> vertices;
	public List<int> triangles;
	public bool marchingCube = false;
	public GameObject target;
	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>().mesh = new Mesh();
		
		voxels = new GameObject[resolution,resolution,resolution];
		for (int x = 0; x < resolution; x++) {
			for (int y = 0; y < resolution; y++) {
				for (int z = 0; z < resolution; z++) {
					voxels[x,y,z] = Instantiate<GameObject>(voxelPrefab);
					voxels[x,y,z].transform.SetParent(this.transform);
					voxels[x,y,z].transform.localPosition = new Vector3(x, y, z);
					voxels[x,y,z].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				}	
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit info;
			if(Physics.Raycast(Camera.main.ScreenPointToRay (Input.mousePosition), out info))
			{
				Vector3 pos = info.transform.localPosition;
				Voxel v = voxels[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Voxel>();
				v.SwitchActive();

			}
		}
		if(marchingCube)
		{
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					for (int z = 0; z < resolution; z++) {
						if(Vector3.Distance(voxels[x,y,z].transform.position, target.transform.position)< 4.0)
						{
							voxels[x,y,z].GetComponent<Voxel>().setActive(true);
						}
						else{
							voxels[x,y,z].GetComponent<Voxel>().setActive(false);
						}
					}	
				}
			}
			MarchingCube();
		}
	}
	
	void MarchingCube()
	{
		vertices.Clear ();
		triangles.Clear ();
		GetComponent<MeshFilter> ().mesh.Clear ();
		for (int x = 0; x < resolution - 1; x ++) {
			for (int y = 0; y < resolution - 1; y ++) {
				for (int z = 0; z < resolution - 1; z ++) {
					GameObject[] cubeVoxels = new GameObject[8];
					cubeVoxels[0] = voxels[x, y, z];
					cubeVoxels[1] = voxels[x, y, z + 1];
					cubeVoxels[2] = voxels[x + 1, y, z];	
					cubeVoxels[3] = voxels[x + 1, y, z + 1];

					cubeVoxels[4] = voxels[x, y + 1, z];
					cubeVoxels[5] = voxels[x, y + 1, z + 1];
					cubeVoxels[6] = voxels[x + 1, y + 1, z];
					cubeVoxels[7] = voxels[x + 1, y + 1, z + 1];

					DetermineMeshFromCube(cubeVoxels);

				}
			}
		}
		GetComponent<MeshFilter> ().mesh.RecalculateNormals ();
		GetComponent<MeshFilter> ().mesh.vertices = vertices.ToArray();
		GetComponent<MeshFilter> ().mesh.triangles = triangles.ToArray();
	}

	void DetermineMeshFromCube(GameObject[] objects)
	{


		List<int> active = new List<int> ();
		List<int> nonActive = new List<int> ();
		List<Vector3[]> midPoints = new List<Vector3[]> ();
		List<Vector3[]> nonActiveMidPoints = new List<Vector3[]> ();
		for(int i = 0; i < 8 ; i ++)
		{
			if (objects [i].GetComponent<Voxel> ().IsActive ()) {
				active.Add (i);
				midPoints.Add(GetAvailableMidpoints(i, objects, false));
			}
			else{
				nonActive.Add(i);
				nonActiveMidPoints.Add(GetAvailableMidpoints(i, objects, true));
			}
		}
		bool pointsAreActive = true;
		if (active.Count > 4) {

			pointsAreActive = false;
			active = nonActive;
			midPoints = nonActiveMidPoints;
		}

		if (active.Count == 1) {
			oneActiveCase (midPoints[0], objects, active[0], pointsAreActive);
		} else if (active.Count == 2) {
			twoActiveCase (midPoints, objects, active.ToArray(), pointsAreActive);
		} else if (active.Count == 3) {
			threeActiveCase(midPoints, objects, active.ToArray(), pointsAreActive);
		} else if (active.Count == 4) {
			fourActiveCase(midPoints, objects, active.ToArray(), pointsAreActive);
		}
	}

	void oneActiveCase(Vector3[] midPoints, GameObject[] objects, int activeIndex, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		Vector3 activePointLocation = objects[activeIndex].transform.localPosition;
		AddTriangles(ref vertexCount, midPoints[0], singleTriangle, activePointLocation, pointsAreActive);
		AddTriangles(ref vertexCount, midPoints[1], singleTriangle, activePointLocation, pointsAreActive);
		AddTriangles(ref vertexCount, midPoints[2], singleTriangle, activePointLocation, pointsAreActive);
	}

	void twoActiveCase(List<Vector3[]> midPoints, GameObject[] objects, int[] activeIndices, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;

		// In case our point has 2 possible midpoints, it is neighbours with the other active voxel.
		if (midPoints[0].Length == 2) {
			AddTriangles(ref vertexCount, midPoints[0][0], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[0][1], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][0], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[1][1], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			AddTriangles(ref vertexCount, midPoints[0][1], singleTriangle, objects[activeIndices[0]].transform.localPosition, pointsAreActive);
			
		} else {
			oneActiveCase (midPoints[0], objects, activeIndices[0], pointsAreActive);
			oneActiveCase (midPoints[1], objects, activeIndices[1], pointsAreActive);
		}
	}

	int getIndexOfClosestMidpoint(Vector3 midpoint, Vector3[] midpoints)
	{
		float smallestDistance = Vector3.Distance (midpoint, midpoints [0]);
		int smallestDistanceIndex = 0;
		for(int i = 1 ; i < midpoints.Length; i ++)
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

	int getIndexOfFurthestMidpoint(Vector3 midpoint, Vector3[] midpoints)
	{
		float furthestDistance = Vector3.Distance (midpoint, midpoints [0]);
		int furthestDistanceIndex = 0;
		for(int i = 1 ; i < midpoints.Length; i ++)
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

	void threeActiveCase(List<Vector3[]> midPoints, GameObject[] objects, int[] activeIndices, bool pointsAreActive)
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
				if(midPoints[i].Length == 1)
				{
					nodeWithTwoActiveNeighbours = i;
				}
				else{
					nodeWithOneActiveNeighbour = i;
				}
			}
			Vector3 activePointLocation = objects[activeIndices[0]].transform.localPosition;
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
			List<Vector3[]> activeNeighbours = new List<Vector3[]>();
			List<int> activeNeighbourIndices = new List<int>();
			for(int i = 0; i < 3; i ++)
			{
				if(midPoints[i].Length == 3)
				{
					activeNoNeighbours = i;
				}
				else{
					activeNeighbours.Add(midPoints[i]);
					activeNeighbourIndices.Add (activeIndices[i]);
				}
			}
			oneActiveCase(midPoints[activeNoNeighbours], objects, activeIndices[activeNoNeighbours], pointsAreActive);
			twoActiveCase(activeNeighbours, objects, activeNeighbourIndices.ToArray(), pointsAreActive);
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


	void fourActiveCase(List<Vector3[]> midPoints, GameObject[] objects, int[] activeIndices, bool pointsAreActive)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;

		int midPointSum = calculateNumberOfMidpoints(midPoints);

		// All vertices are on the same side of the cube
		if (midPointSum == 4) {
			Vector3 activePointLocation = objects[activeIndices[0]].transform.localPosition;
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
				if(midPoints[i].Length == 0)
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
					if(midPoints[i].Length != 0)
					{
						available[j] = i;
						j ++;
					}
				}
				Vector3 activePointLocation = objects[activeIndices[0]].transform.localPosition;
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
					if(midPoints[i].Length == 1)
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

				Vector3 activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]].transform.localPosition;
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][1], singleTriangle, activePointLocation, pointsAreActive);

				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]].transform.localPosition;
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][1], singleTriangle, activePointLocation, pointsAreActive);

				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[1]]].transform.localPosition;
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[1]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][index1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][index2], singleTriangle, activePointLocation, pointsAreActive);

				activePointLocation = objects[activeIndices[oneUnactiveNeighbour[0]]].transform.localPosition;
				AddTriangles(ref vertexCount, midPoints[oneUnactiveNeighbour[0]][0], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[0]][index1], singleTriangle, activePointLocation, pointsAreActive);
				AddTriangles(ref vertexCount, midPoints[twoUnactiveNeighbours[1]][index2], singleTriangle, activePointLocation, pointsAreActive);
			}
		}
		if (midPointSum == 8) {
			bool isolatedPointCase = false;
			for(int i = 0; i < 4; i ++)
			{
				if(midPoints[i].Length == 3)
				{
					isolatedPointCase = true;
				}
			}
			if(isolatedPointCase)
			{
				int activeNoNeighbours = 0;
				List<Vector3[]> activeNeighbours = new List<Vector3[]>();	
				List<int> activeNeighbourIndices = new List<int>();
				for(int i = 0; i < 4; i ++)
				{
					if(midPoints[i].Length == 3)
					{
						activeNoNeighbours = i;
					}
					else{
						activeNeighbours.Add(midPoints[i]);
						activeNeighbourIndices.Add(activeIndices[i]);
					}
				}

				oneActiveCase(midPoints[activeNoNeighbours], objects, activeIndices[activeNoNeighbours], pointsAreActive);
				threeActiveCase(activeNeighbours, objects, activeNeighbourIndices.ToArray(), pointsAreActive);
			}
			else{
				for(int i = 0; i < 4; i ++)
				{
					for(int j = i; j < 4; j ++)
					{
						if(areNeighbouringPositions(activeIndices[i], activeIndices[j]))
						{
							Vector3 activePointLocation = objects[activeIndices[i]].transform.localPosition;
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

	int calculateNumberOfMidpoints(List<Vector3[]> midPoints)
	{
		int sum = 0;
		for(int i = 0; i < midPoints.Count; i ++)
		{
			sum += midPoints[i].Length;
		}
		return sum;
	}

	Vector3[] GetAvailableMidpoints(int i, GameObject[] objects, bool checkForActive)
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

		List<Vector3> activeNeighbours = new List<Vector3>();

		if(objects[i + offsetX].GetComponent<Voxel>().IsActive() == checkForActive)
		{
			if(offsetX > 0)
			{
				activeNeighbours.Add(objects[i + offsetX].transform.localPosition - 0.5f * new Vector3(1, 0, 0));
			}
			else
			{
				activeNeighbours.Add(objects[i + offsetX].transform.localPosition + 0.5f * new Vector3(1, 0, 0));
			}
		}
		if(objects[i + offsetY].GetComponent<Voxel>().IsActive()== checkForActive)
		{
			if(offsetY > 0)
			{
				activeNeighbours.Add(objects[i + offsetY].transform.localPosition - 0.5f * new Vector3(0, 1, 0));
			}
			else
			{
				activeNeighbours.Add(objects[i + offsetY].transform.localPosition + 0.5f * new Vector3(0, 1, 0));
			}
		}
		if(objects[i + offsetZ].GetComponent<Voxel>().IsActive()== checkForActive)
		{
			if(offsetZ > 0)
			{
				activeNeighbours.Add(objects[i + offsetZ].transform.localPosition - 0.5f * new Vector3(0, 0, 1));
			} 
			else
			{
				activeNeighbours.Add(objects[i + offsetZ].transform.localPosition + 0.5f * new Vector3(0, 0, 1));
			}
		}

		return activeNeighbours.ToArray();
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
