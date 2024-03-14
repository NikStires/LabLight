using UnityEngine;
 
public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        // forward.x = matrix.m02;
        // forward.y = matrix.m12;
        // forward.z = matrix.m22;
        forward.x = matrix.m00;
        forward.y = matrix.m01;
        forward.z = matrix.m02;
 
        Vector3 upwards;
        // upwards.x = matrix.m01;
        // upwards.y = matrix.m11;
        // upwards.z = matrix.m21;
        upwards.x = matrix.m10;
        upwards.y = matrix.m11;
        upwards.z = matrix.m12;
 
        return Quaternion.LookRotation(forward, upwards);
    }
 
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        // position.x = matrix.m03;
        // position.y = matrix.m13;
        // position.z = matrix.m23;
        position.x = matrix.m30;
        position.y = matrix.m31;
        position.z = matrix.m32;
        return position;
    }
 
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}
