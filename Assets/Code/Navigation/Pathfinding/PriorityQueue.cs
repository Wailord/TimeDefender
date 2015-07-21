//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
// EDIT 2010 by Christoph Husse: Update() method didn't work correctly. Also
// each item is now carrying an index, so that updating can be performed
// efficiently.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assets.Code.Navigation
{
    internal class PriorityQueue<T> where T : IIndexedObject
    {
        protected T[] InnerList;
        protected IComparer<T> mComparer;
        private int _count;

        public PriorityQueue()
        {
            mComparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer, int capacity)
        {
            mComparer = comparer;
            InnerList = new T[capacity];
            _count = 0;
        }
        
        protected virtual int OnCompare(int i, int j)
        {
            return mComparer.Compare(InnerList[i], InnerList[j]);
        }

        public int Push(T item)
        {
            int p = _count, p2;
            item.Index = _count;
            InnerList[_count] = item;
            _count++;
            do
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    T h = InnerList[p];
                    InnerList[p] = InnerList[p2];
                    InnerList[p2] = h;

                    InnerList[p2].Index = p;
                    InnerList[p2].Index = p2;

                    p = p2;
                }
                else
                    break;
            } while (true);
            return p;
        }

        public T Pop()
        {
            T result = InnerList[0];
            int p = 0, p1, p2, pn;

            InnerList[0] = InnerList[_count - 1];
            InnerList[0].Index = 0;

            _count--;

            result.Index = -1;

            do
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (_count > p1 && OnCompare(p, p1) > 0)
                    p = p1;
                if (_count > p2 && OnCompare(p, p2) > 0)
                    p = p2;

                if (p == pn)
                    break;

                T h = InnerList[p];
                InnerList[p] = InnerList[pn];
                InnerList[pn] = h;

                InnerList[p].Index = p;
                InnerList[pn].Index = pn;

            } while (true);

            return result;
        }

        public void Update(T item)
        {
            while ((item.Index - 1 >= 0) && (OnCompare(item.Index - 1, item.Index) > 0))
            {
                T h = InnerList[item.Index - 1];
                InnerList[item.Index - 1] = InnerList[item.Index];
                InnerList[item.Index] = h;

                InnerList[item.Index - 1].Index = item.Index - 1;
                InnerList[item.Index].Index = item.Index;
            }

            while ((item.Index + 1 < _count) && (OnCompare(item.Index + 1, item.Index) < 0))
            {
                T h = InnerList[item.Index + 1];
                InnerList[item.Index + 1] = InnerList[item.Index];
                InnerList[item.Index] = h;

                InnerList[item.Index + 1].Index = item.Index + 1;
                InnerList[item.Index].Index = item.Index;
            }
        }

        public T Peek()
        {
            if (_count > 0)
                return InnerList[0];
            return default(T);
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}