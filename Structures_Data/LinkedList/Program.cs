using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //ArrayList ar = new ArrayList();
            //ar[0] = 100;

            Test t = new Test();

            Console.WriteLine(t[1]);
        }
    }

    class Test
    {
        public string this[int index]
        {
            get { return "随便"; }
        }

        public int this[string key]
        {
            get { return 100; }
        }
    }

    class MyList
    {
        public Cell first;
        public Cell curCell;
        public int count;

        public void Add(Cell cell)
        {
            if (first == null)
            {
                first = cell;
                count++;
                return;
            }

            curCell = first;
            while (curCell.next != null)
            {
                curCell = curCell.next;
            }
            count++;
            curCell.next = cell;
        }

        public void Remove(Cell cell)
        {
            if (first == null)
            {
                return;
            }
            curCell = first;
            while (curCell.next != cell)
            {
                curCell = curCell.next;
            }
            curCell.next = curCell.next.next;
            count--;
        }

        public Cell this[int index]
        {
            get
            {
                if (first == null)
                {
                    return null;
                }
                curCell = first;
                for (int i = 0; i < index; i++)
                {
                    curCell = curCell.next;
                }
                return curCell;
            }
            set
            {
                this[index].next = value;
            }
        }
    }

    class Cell
    {
        public string name;
        public Cell next;

        public Cell(string name)
        {
            this.name = name;
        }
    }
}
