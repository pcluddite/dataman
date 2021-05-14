﻿using System;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public class TextAnswer : Answer
    {
        public const string TYPE = "text";

        public string Value { get; set; }
        public bool MatchCase { get; set; }

        protected TextAnswer(XmlNode node)
        {
            Value = node.Attributes("value").Value;
            MatchCase = node.Attributes("matchCase").Value(false);
        }

        public TextAnswer(string value)
        {
            Value = value;
        }

        public TextAnswer(string value, bool matchCase)
        {
            Value = value;
            MatchCase = matchCase;
        }

        public override bool IsCorrect(string input)
        {
            if (MatchCase)
            {
                return string.Equals(Value, input, StringComparison.CurrentCulture);
            }
            else
            {
                return string.Equals(Value, input, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public override Answer CloneWithNewInput(params string[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            if (input.Length != 1)
                throw new ArgumentException();
            return new TextAnswer(input[0], MatchCase);
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = base.ToXml(doc);
            node.Attributes("value").Value = Value;
            if (MatchCase)
                node.Attributes("matchCase").Value = MatchCase.ToString();
            return node;
        }

        public override bool Equals(Answer other)
        {
            return Equals(other as TextAnswer);
        }

        public virtual bool Equals(TextAnswer other)
        {
            if (ReferenceEquals(other, this))
                return true;
            if ((object)other == null)
                return false;
            return Value == other.Value && MatchCase == other.MatchCase;
        }

        public override int GetHashCode()
        {
            return (Value == null ? 0 : Value.GetHashCode()) ^ MatchCase.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
