using UnityEngine;

// https://gist.github.com/prucha/866b9535d525adc984c4fe883e73a6c7

public class DynamicCubeMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] c;
    private Vector3[] vertices;
    private Renderer renderer;


    void Start()
    {
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        //2) Define the cube's dimensions
        float length = 1f;
        float width = 1f;
        float height = 1f;

        //3) Define the co-ordinates of each Corner of the cube 
        c = new Vector3[8];

        c[0] = new Vector3(-length * .5f, -width * .5f, height * .5f);
        c[1] = new Vector3(length * .5f, -width * .5f, height * .5f);
        c[2] = new Vector3(length * .5f, -width * .5f, -height * .5f);
        c[3] = new Vector3(-length * .5f, -width * .5f, -height * .5f);

        c[4] = new Vector3(-length * .5f, width * .5f, height * .5f);
        c[5] = new Vector3(length * .5f, width * .5f, height * .5f);
        c[6] = new Vector3(length * .5f, width * .5f, -height * .5f);
        c[7] = new Vector3(-length * .5f, width * .5f, -height * .5f);


        //4) Define the vertices that the cube is composed of:
        //I have used 16 vertices (4 vertices per side). 
        //This is because I want the vertices of each side to have separate normals.
        //(so the object renders light/shade correctly) 
        vertices = new Vector3[]
        {
            c[0], c[1], c[2], c[3], // Bottom
	        c[7], c[4], c[0], c[3], // Left
	        c[4], c[5], c[1], c[0], // Front
	        c[6], c[7], c[3], c[2], // Back
	        c[5], c[6], c[2], c[1], // Right
	        c[7], c[6], c[5], c[4]  // Top
        };


        //5) Define each vertex's Normal
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 forward = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;


        Vector3[] normals = new Vector3[]
        {
            down, down, down, down,             // Bottom
	        left, left, left, left,             // Left
	        forward, forward, forward, forward,	// Front
	        back, back, back, back,             // Back
	        right, right, right, right,         // Right
	        up, up, up, up	                    // Top
        };


        //6) Define each vertex's UV co-ordinates
        Vector2 uv00 = new Vector2(0f, 0f);
        Vector2 uv10 = new Vector2(1f, 0f);
        Vector2 uv01 = new Vector2(0f, 1f);
        Vector2 uv11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
        {
            uv11, uv01, uv00, uv10, // Bottom
	        uv11, uv01, uv00, uv10, // Left
	        uv11, uv01, uv00, uv10, // Front
	        uv11, uv01, uv00, uv10, // Back	        
	        uv11, uv01, uv00, uv10, // Right 
	        uv11, uv01, uv00, uv10  // Top
        };


        //7) Define the Polygons (triangles) that make up the our Mesh (cube)
        //IMPORTANT: Unity uses a 'Clockwise Winding Order' for determining front-facing polygons.
        //This means that a polygon's vertices must be defined in 
        //a clockwise order (relative to the camera) in order to be rendered/visible.
        int[] triangles = new int[]
        {
            3, 1, 0,        3, 2, 1,        // Bottom	
	        7, 5, 4,        7, 6, 5,        // Left
	        11, 9, 8,       11, 10, 9,      // Front
	        15, 13, 12,     15, 14, 13,     // Back
	        19, 17, 16,     19, 18, 17,	    // Right
	        23, 21, 20,     23, 22, 21,	    // Top
        };

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        renderer = GetComponent<Renderer>();
        renderer.enabled = false;
    }

    public void UpdateCorners(Vector3[] c)
    {
        vertices[0] = c[0];
        vertices[1] = c[1];
        vertices[2] = c[2];
        vertices[3] = c[3];
        vertices[4] = c[7];
        vertices[5] = c[4];
        vertices[6] = c[0];
        vertices[7] = c[3];
        vertices[8] = c[4];
        vertices[9] = c[5];
        vertices[10] = c[1];
        vertices[11] = c[0];
        vertices[12] = c[6];
        vertices[13] = c[7];
        vertices[14] = c[3];
        vertices[15] = c[2];
        vertices[16] = c[5];
        vertices[17] = c[6];
        vertices[18] = c[2];
        vertices[19] = c[1];
        vertices[20] = c[7];
        vertices[21] = c[6];
        vertices[22] = c[5];
        vertices[23] = c[4];

        mesh.vertices = vertices;
        renderer.enabled = true;
    }
}