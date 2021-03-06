﻿namespace Shelve.Core
{
    using System.Linq;
    using System.Collections.Generic;

    internal class HashedVariables
    {
        internal Dictionary<string, ValueGetter> getters;
        private Dictionary<string, IValueHolder> holders;

        public HashedVariables()
        {
            getters = new Dictionary<string, ValueGetter>();
            holders = new Dictionary<string, IValueHolder>();
        }

        public Sequence this[string name] => holders[name] as Sequence;

        public IValueHolder CreateIterator(string name, double value = 0)
        {
            HashMember(name, new Iterator(name, value));

            return holders[name];
        }

        public IValueHolder CreateVariable(string name, double value = 0)
        {
            HashMember(name, new Variable(name, value));

            return holders[name];
        }

        public IValueHolder CreateDynamicValue(string name, double value = 0)
        {
            HashMember(name, new DynamicValueHolder(name, value));

            return holders[name];
        }

        public bool Contains(string key) => holders.ContainsKey(key);

        public HashedVariables Merge(HashedVariables another)
        {
            var merged = new Dictionary<string, IValueHolder>(holders);

            foreach (var holder in another.holders)
            {
                if (merged.ContainsKey(holder.Key))
                {
                    var targetSequence = merged[holder.Key] as Sequence;

                    var seqence = holder.Value as Sequence;
                    int expressionsInSequence = seqence.hashedSequence.Count;

                    while (expressionsInSequence --> 0)
                    {
                        targetSequence.AddExpression(seqence.hashedSequence.CircularInspect().Value);
                    }
                }
                else
                {
                    merged.Add(holder.Key, holder.Value);
                }
            }

            foreach (var getter in another.getters.Values)
            {
                getter.TargetSource = this;
            }

            holders = merged;

            return this;
        }

        private void HashMember(string name, IValueHolder member)
        {
            if (!holders.ContainsKey(name))
            {
                holders.Add(name, member);
            }

            if (!getters.ContainsKey(name))
            {
                getters.Add(name, new ValueGetter(name, this));
            }
        }
    }
}