using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }

    // 二叉树
    class Tree
    {
        public Node root;

        public void Insert(int varData)
        {
            if (root == null)
            {
                root = new Node(varData);
                return;
            }

            // 从 root 开始一直循环
            Node cur = root;
            while (true)
            {
                if (cur.data > varData)
                {
                    // 小就看左边
                    if(cur.left == null)
                    {
                        // 左边是空的 -> 决定就放这里了
                        cur.left = new Node(varData);
                        break;
                    }
                    else
                    {
                        // 左边不是空的，就再拿左边判断
                        cur = cur.left;
                    }
                }
                else
                {
                    // 大就看右边
                    if (cur.right == null)
                    {
                        // 如果右边是空的 -> 决定就放这里了
                        cur.right = new Node(varData);
                        break;
                    }
                    else
                    {
                        // 右边不是空的， 就再拿右边判断
                        cur = cur.right;
                    }
                }
            }
        }
    }

    class Node 
    {
        public int data;
        public Node left;
        public Node right;

        public Node(int data)
        {
            this.data = data;
        }
    }
}
