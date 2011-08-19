﻿#region License Information (GPL v2)

/*
    ZUploader - A program that allows you to upload images, texts or files
    Copyright (C) 2008-2011 ZScreen Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v2)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RegionCapture
{
    public static class GraphicsPathExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath graphicsPath, RectangleF rect, float radius)
        {
            if (radius <= 0.0f)
            {
                graphicsPath.AddRectangle(rect);
            }
            else
            {
                // If the corner radius is greater than or equal to
                // half the width, or height (whichever is shorter)
                // then return a capsule instead of a lozenge
                if (radius >= (Math.Min(rect.Width, rect.Height) / 2.0f))
                {
                    graphicsPath.AddCapsule(rect);
                }
                else
                {
                    // Create the arc for the rectangle sides and declare
                    // a graphics path object for the drawing
                    float diameter = radius * 2.0f;
                    SizeF size = new SizeF(diameter, diameter);
                    RectangleF arc = new RectangleF(rect.Location, size);

                    // Top left arc
                    graphicsPath.AddArc(arc, 180, 90);

                    // Top right arc
                    arc.X = rect.Right - diameter;
                    graphicsPath.AddArc(arc, 270, 90);

                    // Bottom right arc
                    arc.Y = rect.Bottom - diameter;
                    graphicsPath.AddArc(arc, 0, 90);

                    // Bottom left arc
                    arc.X = rect.Left;
                    graphicsPath.AddArc(arc, 90, 90);

                    graphicsPath.CloseFigure();
                }
            }
        }

        public static void AddCapsule(this GraphicsPath graphicsPath, RectangleF rect)
        {
            float diameter;
            RectangleF arc;

            try
            {
                if (rect.Width > rect.Height)
                {
                    // Horizontal capsule
                    diameter = rect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(rect.Location, sizeF);
                    graphicsPath.AddArc(arc, 90, 180);
                    arc.X = rect.Right - diameter;
                    graphicsPath.AddArc(arc, 270, 180);
                }
                else if (rect.Width < rect.Height)
                {
                    // Vertical capsule
                    diameter = rect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(rect.Location, sizeF);
                    graphicsPath.AddArc(arc, 180, 180);
                    arc.Y = rect.Bottom - diameter;
                    graphicsPath.AddArc(arc, 0, 180);
                }
                else
                {
                    // Circle
                    graphicsPath.AddEllipse(rect);
                }
            }
            catch
            {
                graphicsPath.AddEllipse(rect);
            }

            graphicsPath.CloseFigure();
        }

        public static void AddTriangle(this GraphicsPath graphicsPath, RectangleF rect)
        {
            PointF pt1 = new PointF(rect.X + rect.Width / 2, rect.Y);
            PointF pt2 = new PointF(rect.X, rect.Y + rect.Height);
            PointF pt3 = new PointF(rect.X + rect.Width, rect.Y + rect.Height);
            graphicsPath.AddPolygon(new PointF[] { pt1, pt2, pt3 });
        }
    }
}