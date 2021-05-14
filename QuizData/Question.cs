using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public class Question
    {
        public string Prompt { get; set; }
        public Answer Answer { get; set; }

        public Question(string prompt, Answer answer)
        {
            Prompt = prompt;
            Answer = answer;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("question");
            elem.Attributes("prompt").Value = Prompt;
            elem.AppendChild(Answer.ToXml(doc));
            return elem;
        }

        public static Question FromXml(XmlNode n)
        {
            Answer a = Answer.FromXml(n.SelectSingleNode("answer"));
            return new Question(n.Attributes("prompt").Value, a);
        }

        public override bool Equals(object obj)
        {
            Question q = obj as Question;
            if ((object)q == null)
                return false;
            return Equals(q);
        }

        public bool Equals(Question q)
        {
            if (ReferenceEquals(q, this))
                return true;
            if ((object)q == null)
                return false;
            return Prompt == q.Prompt && Answer == q.Answer;
        }

        public static bool operator ==(Question left, Question right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if ((object)left == null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(Question left, Question right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return (Prompt == null ? 0 : Prompt.GetHashCode()) | (Answer == null ? 0 : Answer.GetHashCode());
        }
    }
}
