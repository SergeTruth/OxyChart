using System;
using System.Text;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;
using System.Collections.Generic;

namespace iChart
{
    public class UtilNodeSort : IComparer<ShapeNode>
    {
        public int Compare(ShapeNode node1, ShapeNode node2)
        {
            if (node1 == null) return 0;
            if (node2 == null) return 0;

            if (node1.Bounds.Y < node2.Bounds.Y)
            {
                return -1;
            }
            else if (node1.Bounds.Y == node2.Bounds.Y)
            {
                if (node1.Bounds.X < node2.Bounds.X)
                {
                    return -1;
                }
                else if (node1.Bounds.X == node2.Bounds.X)
                {
                    return 0;
                }
            }
            return 1;
        }
    }
}