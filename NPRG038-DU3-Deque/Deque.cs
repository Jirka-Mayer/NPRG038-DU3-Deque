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

public class Deque<T> : IList<T>, IDeque<T>, IEnumerable<T>
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

    private int enumeratorCount = 0;

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
            GuardEnumerationModification();

            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            index = (firstItem + index) % (blocks.Length * BlockSize);
            if (blocks[index / BlockSize] == null)
                blocks[index / BlockSize] = new Block();
            blocks[index / BlockSize].items[index % BlockSize] = value;
        }
    }

    private void GuardEnumerationModification()
    {
        if (enumeratorCount > 0)
            throw new InvalidOperationException("Deque is beign enumerated, cannot modify.");
    }

    public void Add(T item)
    {
        GuardEnumerationModification();

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
        GuardEnumerationModification();

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
        GuardEnumerationModification();

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
        GuardEnumerationModification();

        if (index < 0 || index >= Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = index; i < Length - 1; i++)
            this[i] = this[i + 1];
        
        this[Length - 1] = default(T);
        Length--;
    }

    public bool Remove(T item)
    {
        GuardEnumerationModification();

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

    public void PushBack(T item)
    {
        GuardEnumerationModification();

        Add(item);
    }

    public void PushFront(T item)
    {
        GuardEnumerationModification();

        if (Full)
            Grow();

        if (Empty)
            firstItem = 0;

        firstItem--;
        if (firstItem == -1)
            firstItem = blocks.Length * BlockSize - 1;
        Length++;
        this[0] = item;
    }

    public T PopBack()
    {
        GuardEnumerationModification();

        if (Empty)
            throw new InvalidOperationException();

        T item = this[Length - 1];
        RemoveAt(Length - 1);
        return item;
    }

    public T PopFront()
    {
        GuardEnumerationModification();

        if (Empty)
            throw new InvalidOperationException();

        T item = this[0];
        this[0] = default(T);
        Length--;
        firstItem = (firstItem + 1) % (blocks.Length * BlockSize);
        return item;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator<T>(this);
    }

    public IEnumerator<T> GetInversedEnumerator()
    {
        return new InversedEnumerator<T>(this);
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
            deque.enumeratorCount++;
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

        public void Dispose()
        {
            deque.enumeratorCount--;
        }
    }

    public class InversedEnumerator<U> : IEnumerator<U>
    {
        public Deque<U> deque;
        int position;

        public InversedEnumerator(Deque<U> deque)
        {
            this.deque = deque;
            deque.enumeratorCount++;

            position = deque.Length - 1;
        }

        public bool MoveNext()
        {
            position--;
            return (position >= 0);
        }

        public void Reset()
        {
            position = deque.Length - 1;
        }

        object IEnumerator.Current => Current;

        public U Current => deque[position];

        public void Dispose()
        {
            deque.enumeratorCount--;
        }
    }
}

public class InvertDequeAdapter<T> : IDeque<T>, IList<T>
{
    private Deque<T> subject;

    public int Count => subject.Length;
    public bool IsReadOnly => subject.IsReadOnly;
    public bool IsSynchronized => subject.IsSynchronized;

    public InvertDequeAdapter(Deque<T> subject)
    {
        this.subject = subject;
    }

    public T this[int index]
    {
        get => subject[subject.Length - 1 - index];
        set => subject[subject.Length - 1 - index] = value;
    }

    public void PushBack(T item) => subject.PushFront(item);
    public void PushFront(T item) => subject.PushBack(item);
    public T PopBack() => subject.PopFront();
    public T PopFront() => subject.PopBack();

    public void Clear() => subject.Clear();
    public void Add(T item) => subject.PushFront(item);
    public bool Contains(T item) => subject.Contains(item);
    public bool Remove(T item) => subject.Remove(item);
    public void Insert(int index, T item) => subject.Insert(subject.Length - 1 - index, item);
    public void RemoveAt(int index) => subject.RemoveAt(subject.Length - 1 - index);

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < subject.Length)
            throw new ArgumentException();

        int i = 0;
        foreach (T item in this)
        {
            array[arrayIndex + i] = this[i];
            i++;
        }
    }
    
    public int IndexOf(T item)
    {
        int index = subject.IndexOf(item);
        if (index == -1)
            return -1;
        return subject.Length - 1 - index;
    }


    public IEnumerator<T> GetEnumerator()
    {
        return subject.GetInversedEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class DequeTest
{
    public static IList<T> GetReverseView<T>(Deque<T> d)
    {
		return new InvertDequeAdapter<T>(d);
	}
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

    [Test]
    public void enumerationModificationThrows()
    {
        var d = new Deque<int>();
        for (int i = 0; i < 10; i++)
            d.Add(i);

        foreach (int i in d) {}

        Assert.Throws(typeof(InvalidOperationException), () => {
            foreach (int i in d) {
                d[0] = 42;
            }
        });

        foreach (int i in d) {
            break;
            d[0] = 42;
        }

        foreach (int i in d) {
            continue;
            d[0] = 42;
        }

        foreach (int i in d) {
            foreach (int j in d) {}
        }
    }

    [Test]
    public void dequeInterfaceWorks()
    {
        var d = new Deque<int>();
        d.PushFront(2);
        d.PushBack(3);
        d.PushFront(5);
        d.PushBack(4);
        Assert.AreEqual(5, d.PopFront());
        d.PushFront(1);
        d.PushFront(0);
        d.PushBack(10);
        Assert.AreEqual(10, d.PopBack());
        d.PushBack(5);

        for (int i = 0; i < 6; i++)
            Assert.AreEqual(i, d[i]);
    }
}

/**/
