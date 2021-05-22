using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualFlashCards.QuizData
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
