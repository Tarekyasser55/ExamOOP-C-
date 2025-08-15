using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;


namespace ExamOOP_C_;

public class Answer
{
    public int AnswerId { get; }
    public string AnswerText { get; }

    public Answer(int id, string text)
    {
        AnswerId = id;
        AnswerText = text ?? string.Empty;
    }

    public override string ToString() => $"{AnswerId}) {AnswerText}";
}

public enum QuestionType { TrueFalse, MCQ }

// Base Question
public abstract class Question : ICloneable, IComparable<Question>
{
    public string Header { get; protected set; }
    public string Body { get; protected set; }
    public int Mark { get; protected set; }

    // الاختيارات المتاحة
    public List<Answer> Answers { get; protected set; } = new();
    // رقم الإجابة الصحيحة
    public int CorrectAnswerId { get; protected set; }

    // لتخزين إجابة الطالب
    public int? UserAnswerId { get; set; }

    protected Question(string header, string body, int mark)
    {
        Header = header ?? "";
        Body = body ?? "";
        Mark = mark;
    }

    public abstract QuestionType Type { get; }

    // عرض السؤال والاختيارات
    public virtual void Display()
    {
        Console.WriteLine($"[{Header}]  ({Mark} mark)");
        Console.WriteLine(Body);
        foreach (var n in Answers)
            Console.WriteLine(n);
    }

    // تصحيح إجابة الطالب
    public virtual int Grade()
        => (UserAnswerId.HasValue && UserAnswerId.Value == CorrectAnswerId) ? Mark : 0;

    public object Clone()
    {
        // Deep clone
        var clone = (Question)MemberwiseClone();
        clone.Answers = new List<Answer>(Answers);
        clone.UserAnswerId = null;
        return clone;
    }

    public int CompareTo(Question? other)
    {
        if (other is null) return 1;
        // ترتيب حسب الدرجة ثم العنوان
        int byMark = other.Mark.CompareTo(Mark); // descending
        return byMark != 0 ? byMark : string.Compare(Header, other.Header, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
        => $"{Type}: {Header} (Mark={Mark})";
}

// True/False Question
public class TrueFalseQuestion : Question
{
    public override QuestionType Type => QuestionType.TrueFalse;

    // chaining: نستدعي الكونستركتور الأساسي
    public TrueFalseQuestion(string header, string body, int mark, bool correctIsTrue)
        : base(header, body, mark)
    {
        Answers.Add(new Answer(1, "True"));
        Answers.Add(new Answer(2, "False"));
        CorrectAnswerId = correctIsTrue ? 1 : 2;
    }
}

// MCQ (اختيار واحد)
public class McqQuestion : Question
{
    public override QuestionType Type => QuestionType.MCQ;

    public McqQuestion(string header, string body, int mark, IEnumerable<string> choices, int correctIndex1Based)
        : base(header, body, mark)
    {
        int i = 1;
        foreach (var c in choices)
            Answers.Add(new Answer(i++, c));

        if (correctIndex1Based < 1 || correctIndex1Based > Answers.Count)
            throw new ArgumentOutOfRangeException(nameof(correctIndex1Based));

        CorrectAnswerId = correctIndex1Based;
    }
}

// ========== Exams ==========
public abstract class Exam
{
    public int TimeInMinutes { get; protected set; }
    public int NumberOfQuestions => Questions.Count;
    public List<Question> Questions { get; protected set; } = new();
    public Subject Subject { get; internal set; } = null!;

    protected Exam(int timeInMinutes)
    {
        TimeInMinutes = timeInMinutes;
    }

    // بناء الامتحان بإضافة الأسئلة
    public void AddQuestion(Question q) => Questions.Add(q);

    // تشغيل الامتحان (إدخال إجابات المستخدم)
    public virtual void Run()
    {
        Console.WriteLine($"=== {GetType().Name} for {Subject.SubjectName} ===");
        Console.WriteLine($"Time: {TimeInMinutes} minutes  |  Questions: {NumberOfQuestions}");
        Console.WriteLine(new string('-', 50));

        // ممكن نرتّب حسب IComparable
        Questions.Sort();

        int index = 1;
        foreach (var q in Questions)
        {
            Console.WriteLine($"\nQ{index++}.");
            q.Display();

            // إدخال رقم اختيار المستخدم
            int ans;
            while (true)
            {
                Console.Write("Your answer (enter choice number): ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out ans)) break;
                Console.WriteLine("Invalid input. Try again.");
            }
            q.UserAnswerId = ans;
        }
    }

    public int CalculateTotalGrade()
    {
        int total = 0;
        foreach (var q in Questions) total += q.Grade();
        return total;
    }

    // شكل العرض بعد انتهاء الامتحان
    public abstract void ShowExam();
}

// Practical: يعرض الإجابات الصحيحة بعد الانتهاء
public class PracticalExam : Exam
{
    public PracticalExam(int timeInMinutes) : base(timeInMinutes) { }

    public override void ShowExam()
    {
        Console.WriteLine("\n--- Practical Exam: Correct Answers ---");
        int i = 1;
        foreach (var q in Questions)
        {
            Console.WriteLine($"\nQ{i++}: {q.Body}");
            Console.WriteLine("Choices:");
            foreach (var a in q.Answers) Console.WriteLine(a);

            var correct = q.Answers.Find(a => a.AnswerId == q.CorrectAnswerId)!;
            Console.WriteLine($"Correct Answer: {correct}");
        }

        Console.WriteLine($"\nYour Grade: {CalculateTotalGrade()} / {TotalMarks()}");
    }

    private int TotalMarks()
    {
        int sum = 0; foreach (var q in Questions) sum += q.Mark; return sum;
    }
}

// Final: يعرض الأسئلة + إجابتك + الصحيحة + الدرجة
public class FinalExam : Exam
{
    public FinalExam(int timeInMinutes) : base(timeInMinutes) { }

    public override void ShowExam()
    {
        Console.WriteLine("\n--- Final Exam Review ---");
        int i = 1;
        foreach (var q in Questions)
        {
            Console.WriteLine($"\nQ{i++}: {q.Body}  (Mark {q.Mark})");
            foreach (var a in q.Answers) Console.WriteLine(a);

            var user = q.UserAnswerId.HasValue
                ? q.Answers.Find(a => a.AnswerId == q.UserAnswerId.Value)?.ToString() ?? "N/A"
                : "N/A";
            var correct = q.Answers.Find(a => a.AnswerId == q.CorrectAnswerId)!.ToString();

            Console.WriteLine($"Your Answer: {user}");
            Console.WriteLine($"Correct Answer: {correct}");
            Console.WriteLine($"Earned: {q.Grade()}");
        }

        Console.WriteLine($"\nTotal Grade: {CalculateTotalGrade()} / {TotalMarks()}");
    }

    private int TotalMarks()
    {
        int sum = 0; foreach (var q in Questions) sum += q.Mark; return sum;
    }
}

// ========== Subject ==========
public class Subject
{
    public int SubjectId { get; }
    public string SubjectName { get; }
    public Exam? Exam { get; private set; }

    public Subject(int id, string name)
    {
        SubjectId = id;
        SubjectName = name ?? "";
    }

    // Factory: إنشاء امتحان للمادة
    public void CreateExam(string examType)
    {
        Exam = examType.Equals("final", StringComparison.OrdinalIgnoreCase)
            ? new FinalExam(timeInMinutes: 120)
            : new PracticalExam(timeInMinutes: 30);

        Exam.Subject = this;
    }

    public override string ToString() => $"{SubjectName} (Id={SubjectId})";
}