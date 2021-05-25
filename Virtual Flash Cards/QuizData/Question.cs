using System.Xml;
using Baxendale.DataManagement.Xml;

namespace VirtualFlashCards.QuizData
{
    public class Question : IXmlSerializableObject
    {
        [XmlSerialize(Name = "prompt")]
        public string Prompt { get; set; }

        [XmlSerialize(Name = "answer")]
        public Answer Answer { get; set; }

        public Question()
        {
        }

        public Question(string prompt, Answer answer)
        {
            Prompt = prompt;
            Answer = answer;
        }

        public override string ToString()
        {
            return Prompt;
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
