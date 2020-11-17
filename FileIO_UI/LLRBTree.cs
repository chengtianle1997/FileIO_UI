using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FileIO_UI
{
    public class LLRBTree
    {
        public Node root = null;
        List<Node> node_list = new List<Node>();

        private const bool RED = true;
        private const bool BLACK = false;

        public class Node
        {
            public float key { set; get; }
            public float x_val { set; get; }
            public float y_val { set; get; }
            public float s_val { set; get; }
            public Node left { set; get; }
            public Node right { set; get; }
            public bool color { set; get; } // color of parent link

            public Node(float _key, float _x_val, float _y_val, float _s_val)
            {
                key = _key;
                x_val = _x_val;
                y_val = _y_val;
                s_val = _s_val;
                left = null;
                right = null;
                color = RED;
            }
        }

        public class Point
        {
            public float x { set; get; }
            public float y { set; get; }
            public float s { set; get; }
            public Point(float _x, float _y, float _s)
            {
                x = _x;
                y = _y;
                s = _s;
            }
        }

        private bool isRed(Node x)
        {
            if (x == null) 
                return false;
            return x.color == RED;
        }

        private Node rotateLeft(Node h)
        {
            Node x = h.right;
            h.right = x.left;
            x.left = h;
            x.color = h.color;
            h.color = RED;
            return x;
        }

        private Node rotateRight(Node h)
        {
            Node x = h.left;
            h.left = x.right;
            x.right = h;
            x.color = h.color;
            h.color = RED;
            return x;
        }

        private void flipColors(Node h)
        {
            // Just change the color
            h.color = RED;
            h.left.color = BLACK;
            h.right.color = BLACK;
        }

        private int compare(float a, float b)
        {
            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }

        public void put(float key, float x_val, float y_val, float s_val)
        {
            root = put_p(root, key, x_val, y_val, s_val);
        }

        private Node put_p(Node x, float key, float x_val, float y_val, float s_val)
        {
            if (x == null)
            {
                return new Node(key, x_val, y_val, s_val);
            }
            int ret = compare(key, x.key);
            if(ret < 0)
            {
                x.left = put_p(x.left, key, x_val, y_val, s_val);
            }
            else if (ret > 0)
            {
                x.right = put_p(x.right, key, x_val, y_val, s_val);
            }
            else
            {
                // Update the value
                x.x_val = x_val;
                x.y_val = y_val;
                x.s_val = s_val;
            }

            if (isRed(x.right) && !isRed(x.left))
            {
                x = rotateLeft(x);
            }
            if (isRed(x.left) && isRed(x.right))
            {
                x = rotateRight(x);
            }
            if (isRed(x.left) && isRed(x.right))
            {
                flipColors(x);
            }

            return x;

        }

        // Find the max key <= K
        public Point floor(float K)
        {
            Node x = floor_p(root, K);
            if (x == null)
            {
                return null;
            }
            Point point = new Point(x.x_val, x.y_val, x.s_val);
            return point;
        }

        private Node floor_p(Node x, float K)
        {
            if (x == null)
            {
                return null;
            }
            int cmp = compare(K, x.key);
            // Case 1: K == x.key
            if (cmp == 0)
            {
                return x;
            }
            // Case 2: K < x.key
            // x.key is too big, try go left to find a smaller one
            else if(cmp < 0)
            {
                return floor_p(x.left, K);
            }
            // Case 3: K > x.key
            // x.key is already smaller than K, try to find a bigger one
            // that is still smaller but more closer to K
            else
            {
                Node t = floor_p(x.right, K);
                if (t == null)
                    return x;
                else
                    return t;
            }
        }

        // Find the keys between Kmin and Kmax
        public List<Point> between(float Kmin, float Kmax)
        {
            point_node_list.Clear();
            between_p(root, Kmin, Kmax);
            if (point_node_list.Count < 1)
            {
                return null;
            }
            List<Point> points = new List<Point>();
            for (int i = 0; i < point_node_list.Count; i++)
            {
                points.Add(new Point(point_node_list[i].x_val, point_node_list[i].y_val, point_node_list[i].s_val));
            }
            return points;
        }

        private List<Node> point_node_list = new List<Node>();

        private void between_p(Node x, float Kmin, float Kmax)
        {
            if (x == null)
            {
                return;
            }
            // Case 1: x.key > Kmax, go left
            if (x.key > Kmax)
            {
                between_p(x.left, Kmin, Kmax);
            }
            // Case 2: x.key < Kmin, go right
            else if (x.key < Kmin)
            {
                between_p(x.right, Kmin, Kmax);
            }
            // Case 3: Kmin <= x.key <= Kmax, save and go both left and right 
            else
            {
                point_node_list.Add(x);
                between_p(x.left, Kmin, Kmax);
                between_p(x.right, Kmin, Kmax);
            }
        }
    }
}
