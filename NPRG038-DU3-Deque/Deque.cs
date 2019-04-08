using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Interface representing a Deque data structure
/// </summary>
public interface IDeque<T> : IEnumerable<T>
{
    /// <summary>
    /// Add element to the end of the queue
    /// </summary>
    void PushBack(T item);

    /// <summary>
    /// Add element to the start of the queue
    /// </summary>
    void PushFront(T item);

    /// <summary>
    /// Pop element from the end of the queue
    /// Throws InvalidOperationException if empty
    /// </summary>
    T PopBack();

    /// <summary>
    /// Pop element from the start of the queue
    /// Throws InvalidOperationException if empty
    /// </summary>
    T PopFront();
}

public class Deque<T> : IList<T>, IEnumerable<T>
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

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < Length)
            throw new ArgumentException();

        int i = 0;
        foreach (T item in this)
        {
            array[arrayIndex + i] = this[i];
            i++;
        }
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Full)
            Grow();

        Length++;
        for (int i = Length - 1; i > index; i--)
            this[i] = this[i - 1];
        this[index] = item;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = index; i < Length - 1; i++)
            this[i] = this[i + 1];
        
        this[Length - 1] = default(T);
        Length--;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        
        if (index == -1)
            return false;

        RemoveAt(index);
        return true;
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

    [Test]
    public void copyToWorks()
    {
        var d = new Deque<int>();

        for (int i = 0; i < 10; i++)
            d.Add(i);

        int[] array = new int[15];
        d.CopyTo(array, 5);

        for (int i = 0; i < 10; i++)
            Assert.AreEqual(i, array[5 + i]);
    }

    [Test]
    public void insertWorks()
    {
        var d = new Deque<int>();

        for (int i = 0; i < 10; i++)
            d.Add(i);

        d.Insert(0, -1);
        for (int i = 0; i < 11; i++)
            Assert.AreEqual(i-1, d[i]);
    }

    [Test]
    public void removeAtWorks()
    {
        var d = new Deque<int>();

        for (int i = 0; i < 10; i++)
            d.Add(i);

        d.RemoveAt(0);
        for (int i = 0; i < 9; i++)
            Assert.AreEqual(i+1, d[i]);
    }
}

/**/
