using System;
using System.Collections.Generic;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public class Quiz : IList<Question>
    {
        private const int VERSION = 4;

        private List<Question> allQuestions = new List<Question>();
        private Dictionary<int, Answer> incorrect = new Dictionary<int, Answer>();

        public int Count
        {
            get
            {
                return allQuestions.Count;
            }
        }

        public Question this[int index]
        {
            get
            {
                return allQuestions[index];
            }
            set
            {
                allQuestions[index] = value;
            }
        }

        public Quiz()
        {
        }

        public Quiz(IEnumerable<Question> questions)
        {
            allQuestions.AddRange(questions);
        }

        public void Shuffle()
        {
            Random r = new Random();
            List<Question> old = allQuestions;
            allQuestions = new List<Question>();
            while (old.Count > 0)
            {
                int index = r.Next(0, old.Count);
                Question q = old[index];
                old.RemoveAt(index);
                allQuestions.Add(q);
            }
        }

        public void RemoveAt(int index)
        {
            Question q = allQuestions[index];
            allQuestions.RemoveAt(index);
            incorrect.Remove(index);
        }

        public void AddWrongAnswer(int questionIndex, Answer wrongAnswer)
        {
            incorrect.Add(questionIndex, wrongAnswer);
        }

        public Answer GetWrongAnswer(int questionIndex)
        {
            return allQuestions[questionIndex];
        }

        public Quiz WrongToQuiz()
        {
            return new Quiz(GetWrongEnumerator());
        }

        public IEnumerable<Question> GetWrongEnumerator()
        {
            for (int idx = 0; idx < allQuestions.Count; ++idx)
            {
                if (incorrect.ContainsKey(idx))
                    yield return allQuestions[idx];
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("quiz");
            foreach (Question q in allQuestions)
            {
                elem.AppendChild(q.ToXml(doc));
            }
            return elem;
        }

        public static Quiz FromXml(XmlNode n)
        {
            List<Question> questions = new List<Question>();
            foreach (XmlNode child in n.SelectNodes("question"))
            {
                questions.Add(Question.FromXml(child));
            }
            return new Quiz(questions);
        }

        public static Quiz FromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode quizNode = doc.SelectSingleNode("quiz");
            if (quizNode == null)
                throw new XmlException("Could not find quiz node. This file may be corrupt.");
            if (quizNode.Attributes("version").Value(0) != VERSION)
                throw new XmlException("This quiz file is not compatible with this version of Flash Cards");
            return FromXml(quizNode);
        }

        #region IList<Question> Members

        int IList<Question>.IndexOf(Question item)
        {
            for (int idx = 0; idx < allQuestions.Count; ++idx)
            {
                if (ReferenceEquals(item, allQuestions[idx]))
                    return idx;
            }
            return -1;
        }

        void IList<Question>.Insert(int index, Question item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<Question> Members

        void ICollection<Question>.Add(Question item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Question>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<Question>.Contains(Question item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Question>.CopyTo(Question[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<Question>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<Question>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<Question>.Remove(Question item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Question> Members

        IEnumerator<Question> IEnumerable<Question>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
