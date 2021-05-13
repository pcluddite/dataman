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
    }
}
