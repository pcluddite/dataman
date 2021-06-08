//
//    Quiz
//    Copyright (C) 2009-2021 Timothy Baxendale
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baxendale.Quiz.Objects
{
    internal class QuestionAnswerMap : IDictionary<Question, Answer>, ICollection<KeyValuePair<Question, Answer>>, IEnumerable<KeyValuePair<Question, Answer>>
    {
        private const int DEFAULT_CAPCITY = 10;

        private IDictionary<Question, Answer> userAnswers;

        public QuestionAnswerMap()
            : this(DEFAULT_CAPCITY)
        {
        }

        public QuestionAnswerMap(int capacity)
        {
            userAnswers = new Dictionary<Question, Answer>(capacity);
        }

        public void Add(Question question, Answer answer)
        {
            userAnswers.Add(question, answer);
        }

        public bool ContainsQuestion(Question q)
        {
            return userAnswers.ContainsKey(q);
        }

        public ICollection<Question> Questions
        {
            get
            {
                return userAnswers.Keys;
            }
        }

        public Answer Remove(Question q)
        {
            Answer a;
            if (!userAnswers.TryGetValue(q, out a))
                return null;

            if (!userAnswers.Remove(q))
                throw new InvalidOperationException("This wasn't supposed to happen");

            return a;
        }

        public ICollection<Answer> Answers
        {
            get
            {
                return userAnswers.Values;
            }
        }

        public Answer this[Question question]
        {
            get
            {
                return userAnswers[question];
            }
            set
            {
                userAnswers[question] = value;
            }
        }

        public void Clear()
        {
            userAnswers.Clear();
        }

        public int Count
        {
            get
            {
                return userAnswers.Count;
            }
        }

        #region IDictionary<Question,Answer> Members

        bool IDictionary<Question, Answer>.ContainsKey(Question key)
        {
            return ContainsQuestion(key);
        }

        ICollection<Question> IDictionary<Question, Answer>.Keys
        {
            get
            {
                return Questions;
            }
        }

        bool IDictionary<Question, Answer>.Remove(Question key)
        {
            return Remove(key) != null;
        }

        bool IDictionary<Question, Answer>.TryGetValue(Question key, out Answer value)
        {
            return userAnswers.TryGetValue(key, out value);
        }

        ICollection<Answer> IDictionary<Question, Answer>.Values
        {
            get
            {
                return Answers;
            }
        }

        #endregion


        #region ICollection<KeyValuePair<Question,Answer>> Members

        void ICollection<KeyValuePair<Question, Answer>>.Add(KeyValuePair<Question, Answer> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<Question, Answer>>.Contains(KeyValuePair<Question, Answer> item)
        {
            return ((ICollection<KeyValuePair<Question, Answer>>)userAnswers).Contains(item);
        }

        void ICollection<KeyValuePair<Question, Answer>>.CopyTo(KeyValuePair<Question, Answer>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Question, Answer>>)userAnswers).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<Question, Answer>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<KeyValuePair<Question, Answer>>.Remove(KeyValuePair<Question, Answer> item)
        {
            return ((ICollection<KeyValuePair<Question, Answer>>)userAnswers).Remove(item);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Question, Answer>> GetEnumerator()
        {
            return userAnswers.GetEnumerator();
        }

        #endregion
    }
}
