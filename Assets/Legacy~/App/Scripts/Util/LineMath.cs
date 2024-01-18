using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;

public class LineMath
{
    static bool is_between(float x0, float x, float x1)
    {
        return (x >= x0) && (x <= x1);
    }

    static bool line_segment_intersection(Point a, Point b, Point c, Point d, out float ab, out float cd)
    {
        // from: http://stackoverflow.com/a/14143738/292237
        // Get the intersection between line segments ab and cd.
        // Return false if no such intersection exists.
        // Returned values ab and cd are the fractional distance along ab and cd.
        // These are only defined when the result is true.

        float incd = 0;
        bool partial = false;
        double denom = (c.y - d.y) * (a.x - b.x) - (a.y - b.y) * (c.x - d.x);
        if (denom == 0)
        {
            ab = -1;
            cd = -1;
        }
        else
        {
            ab = (float)((c.x * (b.y - d.y) + d.x * (c.y - b.y) + b.x * (d.y - c.y)) / denom);
            partial = is_between(0, ab, 1);
            if (partial)
            {
                // no point calculating this unless ab is between 0 & 1
                incd = (float)((b.y * (a.x - d.x) + d.y * (b.x - a.x) + a.y * (d.x - b.x)) / denom);
                cd = incd;
            }
        }

        if (partial && is_between(0, incd, 1))
        {
            cd = 1 - incd;
            ab = 1 - ab;
            return true;
        }
        else
        {
            ab = 0;
            cd = 0;
            return false;
        }
    }

    static bool get_rect_line_intersect(RotatedRect r, Point a, Point b, out float abp)
    {
        // Get the intersection point between a rotated rectangle and line segment ab.
        // The function will return false if no such point exists.
        // Value abp is the fractional distance along segment ab.

        // Get the rectangle points.
        var points = new Point[4];
        r.points(points);

        // Check each rectangle line segment in turn.
        for (int i = 0; i < 4; i++)
        {
            float linep, rlinep;
            if (line_segment_intersection(a, b, points[i], points[(i + 1) % 4], out linep, out rlinep))
            {
                abp = linep;
                return true;
            }
        }

        abp = 0;
        return false;
    }

    static Point get_fractional_line_point(Point a, Point b, float progress)
    {
        // Get the point corresponding to the fractional progress along line segment ab.
        float x = (float)(a.x + (b.x - a.x) * progress);
        float y = (float)(a.y + (b.y - a.y) * progress);
        return new Point(x, y);
    }

    public static bool get_connector_line(RotatedRect a, RotatedRect b, out Point pta, out Point ptb)
    {
        // Get the line segment between the centers of the two rectangles.
        // Return false if no such line segment exists.

        var apts = new Point[4];
        var bpts = new Point[4];
        a.points(apts);
        b.points(bpts);

        var acen = a.center;
        var bcen = b.center;

        float alp, blp;
        bool al_intersect = get_rect_line_intersect(a, acen, bcen, out alp);
        bool bl_intersect = get_rect_line_intersect(b, acen, bcen, out blp);

        // Verify that the rectangles both intersect the center line.
        if (al_intersect && bl_intersect)
        {
            // Verify that the rectangles don't overlap.
            if (alp < blp)
            {
                pta = get_fractional_line_point(acen, bcen, alp);
                ptb = get_fractional_line_point(acen, bcen, blp);
                return true;
            }
        }

        pta = new Point();
        ptb = new Point();
        return false;
    }
}