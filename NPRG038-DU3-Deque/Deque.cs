using System;
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
    private int lastItem => (firstItem + Length) % blocks.Length * BlockSize;
    private int firstBlock => firstItem / BlockSize;
    private int lastBlock => lastItem / BlockSize;
    public bool Empty => Length == 0;
    public bool Full => Length == blocks.Length * BlockSize;

    public Deque(int capacity = BlockSize * 2)
    {
        if (capacity < BlockSize)
            capacity = BlockSize;

        blocks = new Block[capacity / BlockSize];
        for (int i = 0; i < blocks.Length; i++)
            blocks[i] = new Block();
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            index = (firstItem + index) % (blocks.Length * BlockSize);
            return blocks[index / BlockSize].items[index % BlockSize];
        }

        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            index = (firstItem + index) % (blocks.Length * BlockSize);
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

    private void Grow()
    {
        Block[] newBlocks = new Block[blocks.Length * 2];
        for (int i = 0; i < newBlocks.Length; i++)
            newBlocks[i] = new Block();
        
        for (int i = 0; i < Length; i++)
            newBlocks[i / BlockSize].items[i % BlockSize] = this[i];

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
}

/**/
