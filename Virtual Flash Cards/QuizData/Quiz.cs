﻿using System;
using System.Collections.Generic;
using System.Xml;
using Baxendale.DataManagement.Xml;

namespace VirtualFlashCards.QuizData
{
    public class Quiz : IList<Question>
    {
        private const int VERSION = 4;

        private List<Question> allQuestions = new List<Question>();
        private QuestionAnswerMap incorrect = new QuestionAnswerMap();

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
            AddRange(questions);
        }

        public void Shuffle()
        {
            List<Question> oldQuestionList = allQuestions;
            List<Question> newQuestionList = new List<Question>(oldQuestionList.Count);
            Random r = new Random();
            while (oldQuestionList.Count > 0)
            {
                int oldIndex = r.Next(0, oldQuestionList.Count);
                Question q = oldQuestionList[oldIndex];
                oldQuestionList.RemoveAt(oldIndex);
                newQuestionList.Add(q);
            }
            allQuestions = newQuestionList;
        }

        public void AddWrongAnswer(int questionIndex, Answer wrongAnswer)
        {
            if (wrongAnswer == null)
                throw new ArgumentNullException();
            incorrect.Add(allQuestions[questionIndex], wrongAnswer);
        }

        public Answer GetWrongAnswer(int questionIndex)
        {
            return incorrect[allQuestions[questionIndex]];
        }

        public Quiz WrongQuiz()
        {
            return new Quiz(incorrect.Questions);
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

        public void AddRange(IEnumerable<Question> questions)
        {
            foreach (Question q in questions)
            {
                Add(q);
            }
        }

        #region IList<Question> Members

        int IList<Question>.IndexOf(Question item)
        {
            for (int idx = 0; idx < allQuestions.Count; ++idx)
            {
                if (item == allQuestions[idx])
                    return idx;
            }
            return -1;
        }

        void IList<Question>.Insert(int index, Question item)
        {
            allQuestions.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Question q = allQuestions[index];
            allQuestions.RemoveAt(index);
            incorrect.Remove(q);
        }

        #endregion

        #region ICollection<Question> Members

        public void Add(Question question)
        {
            allQuestions.Add(question);
        }

        public void Clear()
        {
            allQuestions.Clear();
            incorrect.Clear();
        }

        public bool Contains(Question question)
        {
            return allQuestions.Contains(question);
        }

        public void CopyTo(Question[] array, int arrayIndex)
        {
            allQuestions.CopyTo(array, arrayIndex);
        }

        bool ICollection<Question>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Question question)
        {
            if (!allQuestions.Remove(question))
                return false;
            incorrect.Remove(question);
            return true;
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
