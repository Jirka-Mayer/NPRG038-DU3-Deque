﻿using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class Deque<T> : /*IList<T>,*/ IEnumerable<T>
{
    public const int BlockSize = 16;

    private class Block
    {
        public T[] items;

        public Block()
        {
            items = new T[BlockSize];
        }
    }

    private Block[] blocks;

    private int firstItem = -1;
    public int Length { get; private set; } = 0;
    public int Count => Length;
    private int lastItem => (firstItem + Length) % blocks.Length * BlockSize;
    private int firstBlock => firstItem / BlockSize;
    private int lastBlock => lastItem / BlockSize;
    public bool Empty => Length == 0;
    public bool Full => Length == blocks.Length * BlockSize;

    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    public bool IsSynchronized => false;
    public object SyncRoot => throw new InvalidOperationException();

    public Deque(int capacity = BlockSize * 2)
    {
        if (capacity < BlockSize)
            capacity = BlockSize;

        blocks = new Block[capacity / BlockSize];
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            index = (firstItem + index) % (blocks.Length * BlockSize);
            if (blocks[index / BlockSize] == null)
                blocks[index / BlockSize] = new Block();
            return blocks[index / BlockSize].items[index % BlockSize];
        }

        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            index = (firstItem + index) % (blocks.Length * BlockSize);
            if (blocks[index / BlockSize] == null)
                blocks[index / BlockSize] = new Block();
            blocks[index / BlockSize].items[index % BlockSize] = value;
        }
    }

    public void Add(T item)
    {
        if (Full)
            Grow();

        if (Empty)
        {
            firstItem = 0;
            Length = 1;
        }
        else
        {
            Length++;
        }

        this[Length - 1] = item;
    }

    public void Clear()
    {
        firstItem = 0;
        Length = 0;

        blocks = new Block[2];
    }

    public bool Contains(T item) => IndexOf(item) != -1;

    public int IndexOf(T item)
    {
        int index = 0;
        
        foreach (T i in this)
        {
            if (i.Equals(item))
                return index;

            index++;
        }

        return -1;
    }

    private void Grow()
    {
        Block[] newBlocks = new Block[blocks.Length * 2];
        
        for (int i = 0; i < Length; i++)
        {
            if (newBlocks[i / BlockSize] == null)
                newBlocks[i / BlockSize] = new Block();
            newBlocks[i / BlockSize].items[i % BlockSize] = this[i];
        }

        blocks = newBlocks;
        firstItem = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator<T>(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public class Enumerator<U> : IEnumerator<U>
    {
        public Deque<U> deque;
        int position = -1;

        public Enumerator(Deque<U> deque)
        {
            this.deque = deque;
        }

        public bool MoveNext()
        {
            position++;
            return (position < deque.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current => Current;

        public U Current => deque[position];

        public void Dispose() {}
    }
}

public static class DequeTest
{

}

/**/

[TestFixture]
public class MyTests
{
    [Test]
    public void itCanAddValuesAndIterateOverThem()
    {
        var d = new Deque<int>();

        d.Add(1);
        d.Add(2);
        d.Add(3);

        int index = 0;
        foreach (int item in d)
        {
            Assert.AreEqual(index, item - 1);
            index++;
        }
    }

    [Test]
    public void itCanGrow()
    {
        var d = new Deque<int>(Deque<int>.BlockSize);

        Assert.True(d.Empty);
        Assert.False(d.Full); // has some initial capacity

        // note that 8 is a power of 2 (block count doubles)
        for (int i = 0; i < 8 * Deque<int>.BlockSize; i++)
            d.Add(i);

        for (int i = 0; i < 8 * Deque<int>.BlockSize; i++)
            Assert.AreEqual(i, d[i]);

        Assert.True(d.Full);
        Assert.False(d.Empty);

        d.Add(-1);
        Assert.False(d.Full);
        Assert.AreEqual(-1, d[8 * Deque<int>.BlockSize]);
    }

    [Test]
    public void indexOfWorks()
    {
        var d = new Deque<int>();

        for (int i = 0; i < 50; i++)
            d.Add(i);

        for (int i = 0; i < 50; i++)
            Assert.AreEqual(i, d.IndexOf(i));

        Assert.AreEqual(-1, d.IndexOf(-50));
        Assert.AreEqual(-1, d.IndexOf(100000));
    }
}

/**/
