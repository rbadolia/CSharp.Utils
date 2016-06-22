using System;
using System.Collections.Generic;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Algorithms
{
    public static class ShuffleHelper
    {
        #region Public Methods and Operators

        [StateIntact]
        public static int[] BuildShuffleSequence(int count)
        {
            var ints = new int[count];
            for (int i = 0; i < count; i++)
            {
                ints[i] = i;
            }

            InPlaceShuffle(ints);
            return ints;
        }

        [StateIntact]
        public static IEnumerable<Pair<int, int>> BuildShuffleSequence(int size, int count)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return Pair<int, int>.CreateNew(random.Next(size), random.Next(size));
            }
        }

        public static void InPlaceShuffle<T>(IList<T> list)
        {
            foreach (var pair in BuildShuffleSequence(list.Count, list.Count / 2))
            {
                T temp = list[pair.First];
                list[pair.First] = list[pair.Second];
                list[pair.Second] = temp;
            }
        }

        public static void InPlaceShuffle<T>(IList<T> list, int[] shuffleSequence)
        {
            for (int i = 0; i < shuffleSequence.Length; i++)
            {
                T temp = list[i];
                list[i] = list[shuffleSequence[i]];
                list[shuffleSequence[i]] = temp;
            }
        }

        [StateIntact]
        public static IEnumerable<T> Shuffle<T>(IList<T> list)
        {
            int[] ints = BuildShuffleSequence(list.Count);
            foreach (int i in ints)
            {
                yield return list[i];
            }
        }

        [StateIntact]
        public static IEnumerable<T> Shuffle<T>(IList<T> list, int[] shuffleSequence)
        {
            foreach (int sequence in shuffleSequence)
            {
                yield return list[sequence];
            }
        }

        #endregion Public Methods and Operators
    }
}
