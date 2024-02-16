using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C4InterFlow.Visualization
{
    internal class Sequence
    {
        private const string NUMBERING_REGEX = @"^(\d)*(\.(\d)*)*";

        internal Sequence()
        {
            Index = 0;
        }

        private Sequence(int index, Sequence parent)
        {
            Index = index;
            Parent = parent;
        }

        private IList<Sequence> Sequences { get; } = new List<Sequence>();
        private int Index { get; }
        internal Sequence Parent { get; }

        internal Sequence PeekNextSequence()
        {
            var lastSequence = Sequences.LastOrDefault();
            return (lastSequence == null ? new Sequence(1, this) : new Sequence(lastSequence.Index + 1, this));
        }

        internal Sequence Increment()
        {
            var lastSequence = Sequences.LastOrDefault();

            if (lastSequence == null)
            {
                Sequences.Add(new Sequence(1, this));
            }
            else
            {
                Sequences.Add(new Sequence(lastSequence.Index + 1, this));
            }

            return Sequences.LastOrDefault();
        }
        
        private string GetSequence(Sequence currentSequence, string sequence = null)
        {
            var result = string.Empty;

            var parentSequence = currentSequence?.Parent != null && currentSequence?.Parent.Index > 0 ? currentSequence.Parent.Index.ToString() : null;

            if (!string.IsNullOrEmpty(sequence) && !string.IsNullOrEmpty(parentSequence))
            {
                result = $"{parentSequence}.{sequence}";
            }
            else if (string.IsNullOrEmpty(sequence))
            {
                result = parentSequence;
            }
            else if (string.IsNullOrEmpty(parentSequence))
            {
                result = sequence;
            }
            else
            {
                result = null;
            }

            if (currentSequence?.Parent != null)
            {
                result = GetSequence(currentSequence.Parent, result);
            }

            return result;
        }
        

        private string AppendSequence(string sequence, IList<Sequence> nextSequences)
        {
            foreach (var item in nextSequences)
            {
                sequence = AppendSequence($"{sequence}.{item.Index}", item.Sequences);
            }
            return sequence;
        }

        public override string ToString()
        {
            return AppendSequence(GetSequence(this, Index.ToString()), Sequences);
        }

        internal static string RemoveNumbering(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, NUMBERING_REGEX, string.Empty).Trim();
        }
    }
}
