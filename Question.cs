using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VirtualFlashCards
{
    public class Question
    {
        public string Prompt { get; set; }
        public string CorrectAnswer { get; set; }

        public List<string> AnswerHistory { get; private set; }

        public bool MatchCase { get; set; }

        public Question(string prompt, string ans)
        {
            AnswerHistory = new List<string>();
            Prompt = prompt;
            CorrectAnswer = ans;
            MatchCase = false;
        }

        public bool IsCorrect(string ans)
        {
            AnswerHistory.Add(ans);
            if (MatchCase)
            {
                return ans.Equals(CorrectAnswer, StringComparison.CurrentCulture);
            }
            else
            {
                return ans.Equals(CorrectAnswer, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("question");
            elem.SetAttribute("prompt", Prompt);
            elem.SetAttribute("answer", CorrectAnswer);
            elem.SetAttribute("matchCase", MatchCase.ToString());
            return elem;
        }

        public static Question FromXml(XmlNode n)
        {
            string prompt = n.Attributes["prompt"].InnerText;
            string answer = n.Attributes["answer"].InnerText;
            bool matchCase = bool.Parse(n.Attributes["matchCase"].InnerText);
            return new Question(prompt, answer) { MatchCase = matchCase };
        }
    }
}
