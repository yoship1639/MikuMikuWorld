using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    struct Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
    }

    class CPair<T1, T2>
    {
        public T1 first = default(T1);
        public T2 second = default(T2);

        public CPair() { }
        public CPair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
    }

    class CPair<T1, T2, T3>
    {
        public T1 first = default(T1);
        public T2 second = default(T2);
        public T3 third = default(T3);

        public CPair() { }
        public CPair(T1 first, T2 second, T3 third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }

    class CPair<T1, T2, T3, T4>
    {
        public T1 first = default(T1);
        public T2 second = default(T2);
        public T3 third = default(T3);
        public T4 fourth = default(T4);

        public CPair() { }
        public CPair(T1 first, T2 second, T3 third, T4 fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }
    }

    class CPair<T1, T2, T3, T4, T5>
    {
        public T1 first = default(T1);
        public T2 second = default(T2);
        public T3 third = default(T3);
        public T4 fourth = default(T4);
        public T5 fifth = default(T5);

        public CPair() { }
        public CPair(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
            this.fifth = fifth;
        }
    }
}
