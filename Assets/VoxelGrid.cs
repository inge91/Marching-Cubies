using UnityEngine;
using System.Collections.Generic;


public class VoxelGrid : MonoBehaviour {

	public int resolution = 8;
	public GameObject voxelPrefab;
	private GameObject[,,] voxels;

	public List<Vector3> vertices;
	public List<int> triangles;

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
				MarchingCube();
			}
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
		int c = 0;

		List<int> active = new List<int> ();
		for(int i = 0; i < 8 ; i ++)
		{
			if (objects [i].GetComponent<Voxel> ().IsActive ()) {
				active.Add (i);
			}
		}
		if (active.Count == 1) {
			oneActiveCase (active [0], objects);
		} else if (active.Count == 2) {
			twoActiveCase (active.ToArray (), objects);
		} else if (active.Count == 3) {
			threeActiveCase(active.ToArray(), objects);
		} else if (active.Count == 4) {
			fourActiveCase(active.ToArray (), objects);
		}
	}

	void oneActiveCase(int i, GameObject[] objects)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;

		Vector3[] midpoints = GetAvailableMidpoints(i, objects);
		Vector3 activePointLocation = objects[i].transform.localPosition;
		AddTriangles(ref vertexCount, midpoints[0], singleTriangle, activePointLocation);
		AddTriangles(ref vertexCount, midpoints[1], singleTriangle, activePointLocation);
		AddTriangles(ref vertexCount, midpoints[2], singleTriangle, activePointLocation);
	}

	void twoActiveCase(int[] activeIndices, GameObject[] objects)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		int indexFirst = activeIndices [0];
		int indexSecond = activeIndices [1];
		Vector3[] midPointsFirst = GetAvailableMidpoints(activeIndices [0], objects);
		Vector3[] midPointsSecond = GetAvailableMidpoints(activeIndices [1], objects);
		Debug.Log (midPointsFirst.Length);
		Vector3 activePointLocation = objects[indexFirst].transform.localPosition;
		// In case our point has 2 possible midpoints, it is neighbours with the other active voxel.
		if (midPointsFirst.Length == 2) {
			AddTriangles(ref vertexCount, midPointsFirst[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsFirst[1], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsSecond[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsSecond[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsSecond[1], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsFirst[1], singleTriangle, activePointLocation);
			
		} else {
			oneActiveCase (activeIndices [0], objects);
			oneActiveCase (activeIndices [1], objects);
		}
	}

	void threeActiveCase(int[] activeIndices, GameObject[] objects)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		Vector3[] midPointsFirst = GetAvailableMidpoints(activeIndices [0], objects);
		Vector3[] midPointsSecond = GetAvailableMidpoints(activeIndices [1], objects);
		Vector3[] midPointsThird = GetAvailableMidpoints(activeIndices [2], objects);
		Vector3 activePointLocation = objects[0].transform.localPosition;
		int midPointSum = midPointsFirst.Length 
			+ midPointsSecond.Length
			+ midPointsThird.Length;
		if (midPointSum == 9) {
			for(int i = 0; i < 3; i ++)
			{
				oneActiveCase(activeIndices[i], objects);
			}
		}
		// Working on this one

		if (midPointSum == 5) {
			if(midPointsFirst.Length == 1)
			{
				AddTriangles(ref vertexCount, midPointsFirst[0], singleTriangle, activePointLocation);

			}
			if(midPointsSecond.Length == 1)
			{
				AddTriangles(ref vertexCount, midPointsSecond[0], singleTriangle, activePointLocation);

			}
			if(midPointsThird.Length == 1)
			{
				AddTriangles(ref vertexCount, midPointsThird[0], singleTriangle, activePointLocation);

			}
		}
		if (midPointSum == 7) {
			for(int i = 0; i < 3; i ++)
			{
				int[] neighbourIndices = new int[2];
				if(midPointsFirst.Length == 3)
				{
					oneActiveCase(activeIndices[0], objects);
					neighbourIndices[0] = activeIndices[1];
					neighbourIndices[1] = activeIndices[2];
					twoActiveCase(neighbourIndices, objects);
				}
				if(midPointsSecond.Length == 3)
				{
					oneActiveCase(activeIndices[1], objects);
					neighbourIndices[0] = activeIndices[0];
					neighbourIndices[1] = activeIndices[1];
					twoActiveCase(neighbourIndices, objects);
				}
				if(midPointsThird.Length == 3)
				{
					oneActiveCase(activeIndices[2], objects);
					neighbourIndices[0] = activeIndices[0];
					neighbourIndices[1] = activeIndices[1];
					twoActiveCase(neighbourIndices, objects);
				}
			}
		}
	}
	//0 1 2 3 - 5 - - - 9 10 11 - -
	void fourActiveCase(int[] activeIndices, GameObject[] objects)
	{
		Vector3[] singleTriangle = new Vector3[3];
		int vertexCount = 0;
		Vector3[] midPointsFirst = GetAvailableMidpoints(activeIndices [0], objects);
		Vector3[] midPointsSecond = GetAvailableMidpoints(activeIndices [1], objects);
		Vector3[] midPointsThird = GetAvailableMidpoints(activeIndices [2], objects);
		Vector3[] midPointsFourth = GetAvailableMidpoints(activeIndices [3], objects);
		Vector3 activePointLocation = objects[0].transform.localPosition;
		int midPointSum = midPointsFirst.Length 
			+ midPointsSecond.Length
			+ midPointsThird.Length
			+ midPointsFourth.Length;
		// All vertices are on the same side of the cube
		if (midPointSum == 4) {
			AddTriangles(ref vertexCount, midPointsFirst[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsSecond[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsThird[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsThird[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsFourth[0], singleTriangle, activePointLocation);
			AddTriangles(ref vertexCount, midPointsSecond[0], singleTriangle, activePointLocation);
		}
		if (midPointSum == 8) {
			
		}
		if (midPointSum == 12) {
			for(int i = 0; i < 4; i ++)
			{
				oneActiveCase(i, objects);
			}
		}
	}


	Vector3[] GetAvailableMidpoints(int i, GameObject[] objects)
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

		if(!objects[i + offsetX].GetComponent<Voxel>().IsActive())
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
		if(!objects[i + offsetY].GetComponent<Voxel>().IsActive())
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
		if(!objects[i + offsetZ].GetComponent<Voxel>().IsActive())
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

	
	public void AddTriangles(ref int vertexCount, Vector3 vertexPos, Vector3[] singleTriangle, Vector3 activePoint)
	{
		singleTriangle[vertexCount] = vertexPos;
	
		vertexCount ++;
	
		if(vertexCount == 3)
		{
			CreateTriangle(singleTriangle, activePoint);
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

	void CreateTriangle(Vector3[] v, Vector3 pointPosition)
	{
		int vertexIndex = vertices.Count;
		vertices.Add (v[0]);
		vertices.Add (v[1]);
		vertices.Add (v[2]);

		int minI = indexMinX(v);
		int maxI = indexMaxX(v);
	
		// Determine on what side of the triangle the active point is,
		// so that points are added to the triangle in the right order.
		Vector3 normal = Vector3.Normalize(Vector3.Cross (v [minI] - v[maxI], v [maxI] - v[3 - minI - maxI]));
		Vector3 w = - (v[0] - pointPosition);
		float distance = Vector3.Dot (normal, w);
	
		if (distance > 0) {
		triangles.Add (vertexIndex + maxI);	
		triangles.Add (vertexIndex + minI);	
		triangles.Add (vertexIndex + 3 - maxI - minI);

		} else {
			triangles.Add (vertexIndex + minI);	
			triangles.Add (vertexIndex + maxI);	
			triangles.Add (vertexIndex + 3 - maxI - minI);
		}
	}

}
