using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationFromMatrix : MonoBehaviour
{
    public static Matrix4x4 Calculate_Hand_Coordinate_System_Transform(	bool is_right,
													float index_baseX,	float index_baseY,	float index_baseZ,
													float middle_baseX,	float middle_baseY,	float middle_baseZ,
													float ring_baseX,	float ring_baseY,	float ring_baseZ,
													float pinky_baseX,	float pinky_baseY,	float pinky_baseZ,
													float indexTipX,	float indexTipY,	float indexTipZ,
													float middle_tipX,	float middle_tipY,	float middle_tipZ,
													float ring_tipX,	float ring_tipY,	float ring_tipZ,
													float pinky_tipX,	float pinky_tipY,	float pinky_tipZ
													)
    {
        // get mid points from the fingers
        Matrix4x4 out_transform4x4 = new Matrix4x4();
        float centerX	= (index_baseX + middle_baseX + ring_baseX + pinky_baseX) / 4;
        float centerY	= (index_baseY + middle_baseY + ring_baseY + pinky_baseY) / 4;
        float centerZ	= (index_baseZ + middle_baseZ + ring_baseZ + pinky_baseZ) / 4;
        float tipX		= (indexTipX  + middle_tipX  + ring_tipX  + pinky_tipX)   / 4;
        float tipY		= (indexTipY  + middle_tipY  + ring_tipY  + pinky_tipY)   / 4;
        float tipZ		= (indexTipZ  + middle_tipZ  + ring_tipZ  + pinky_tipZ)   / 4;


        // calculate average height from the base knuckles and use it for all points (that we need) to enforce a flat plane
        float avg_height	= centerY;
            centerY		= avg_height;
            tipY			= avg_height;
            index_baseY	= avg_height;
            pinky_baseY	= avg_height;


        // calculate (normalized) right vector, points across the finger bases (from index towards pinky for right hand (inverse for left))
        float rightX, rightY, rightZ;
        if(is_right)
        {
            rightX = pinky_baseX - index_baseX;
            rightY = pinky_baseY - index_baseY;
            rightZ = pinky_baseZ - index_baseZ;
        }
        else
        {
            rightX = index_baseX - pinky_baseX;
            rightY = index_baseY - pinky_baseY;
            rightZ = index_baseZ - pinky_baseZ;
        }
        float right_length = (float)Math.Sqrt((double)(rightX * rightX + rightY * rightY + rightZ * rightZ));
        
        if(right_length != 0)
        {
            rightX /= right_length;
            rightY /= right_length;
            rightZ /= right_length;
        }

        // calculate (normalized) forward vector, points in the direction of the fingers
        float forwardX		 = tipX - centerX;
        float forwardY		 = tipY - centerY;
        float forwardZ		 = tipZ - centerZ;
        float forward_length = (float)Math.Sqrt((double)(forwardX * forwardX + forwardY * forwardY + forwardZ * forwardZ));
        if(forward_length != 0)
        {
            forwardX /= forward_length;
            forwardY /= forward_length;
            forwardZ /= forward_length;
        }

        // calculate up vector, cross product of right & forward
        float upX = rightY * forwardZ - rightZ * forwardY;
        float upY = rightZ * forwardX - rightX * forwardZ;
        float upZ = rightX * forwardY - rightY * forwardX;


        // construct a transform matrix defining our coordinate system
        out_transform4x4[0]  = forwardX;	out_transform4x4[1]  = forwardY;	out_transform4x4[2]  = forwardZ;	out_transform4x4[3]  = 0;
        out_transform4x4[4]  = upX;			out_transform4x4[5]  = upY;			out_transform4x4[6]  = upZ;			out_transform4x4[7]  = 0;
        out_transform4x4[8]  = rightX;		out_transform4x4[9]  = rightY;		out_transform4x4[10] = rightZ;		out_transform4x4[11] = 0;
        out_transform4x4[12] = centerX;		out_transform4x4[13] = centerY;		out_transform4x4[14] = centerZ;		out_transform4x4[15] = 1f;
        return out_transform4x4;
    }
}
