using System;
using System.Runtime.Serialization;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    [DataContract]
    public class Grid<T>
    {
        private CompositeDictionary<int, T> _dictionary = new CompositeDictionary<int, T>(true);

        public Grid(int numberOfRows, int numberOfColumns)
        {
            Guard.ArgumentBigger(1, numberOfRows, "numberOfRows");
            Guard.ArgumentBigger(1, numberOfColumns, "numberOfColumns");
            this.NumberOfRows = numberOfRows;
            this.NumberOfColumns = numberOfColumns;
        }

        public int NumberOfRows { get; private set; }

        public int NumberOfColumns { get; private set; }

        public void Add(int rowNumber, int columnNumber, T item)
        {
            Guard.ArgumentInRange(1, this.NumberOfRows, rowNumber, "rowNumber");
            Guard.ArgumentInRange(1, this.NumberOfColumns, columnNumber, "columnNumber");
            this._dictionary.PerformAtomicOperation(() =>
            {
                T existingItem;
                if (this._dictionary.TryGetValue(out existingItem, rowNumber, columnNumber))
                {
                    throw new Exception("The cell is already occupied");
                }

                this._dictionary.AddOrUpdate(item, rowNumber, columnNumber);
            });
        }

        public void Remove(int rowNumber, int columnNumber)
        {
            Guard.ArgumentInRange(1, this.NumberOfRows, rowNumber, "rowNumber");
            Guard.ArgumentInRange(1, this.NumberOfColumns, columnNumber, "columnNumber");
            this._dictionary.PerformAtomicOperation(() =>
                {
                    T existingItem;
                    if (!this._dictionary.TryGetValue(out existingItem, rowNumber, columnNumber))
                    {
                        throw new Exception("There is no item at the given cell");
                    }

                    this._dictionary.RemoveIfExists(rowNumber, columnNumber);
                });
        }
    }
}
