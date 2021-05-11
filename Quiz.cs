using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VirtualFlashCards
{
    public class Quiz
    {
        public int Current { get; set; }

        public int Count
        {
            get
            {
                return allQuestions.Count;
            }
        }

        private List<Question> allQuestions = new List<Question>();
        private List<Question> incorrect = new List<Question>();

        public Question this[int index]
        {
            get
            {
                return allQuestions[index];
            }
        }

        public Quiz()
        {
            Current = 0;
        }

        public Quiz(IEnumerable<Question> questions)
        {
            Current = 0;
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

        public Question Next()
        {
            if (Current >= allQuestions.Count)
            {
                return null;
            }
            else
            {
                return allQuestions[Current++];
            }
        }

        public Question RemoveAt(int index)
        {
            Question q = allQuestions[index];
            allQuestions.RemoveAt(index);
            return q;
        }

        public void AddQuestion(Question q)
        {
            allQuestions.Add(q);
        }

        public void AddWrong(Question q)
        {
            incorrect.Add(q);
        }

        public Quiz Wrong()
        {
            return new Quiz(incorrect);
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
            if (n == null)
            {
                return new Quiz();
            }
            else
            {
                List<Question> questions = new List<Question>();
                foreach (XmlNode child in n.SelectNodes("question"))
                {
                    questions.Add(Question.FromXml(child));
                }
                return new Quiz(questions);
            }
        }

        public static Quiz FromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode quizNode = doc.SelectSingleNode("quiz");
            if (quizNode == null)
                throw new XmlException("File is not a valid quiz");
            return FromXml(quizNode);
        }
    }
}
