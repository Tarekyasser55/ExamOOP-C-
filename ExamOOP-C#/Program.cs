namespace ExamOOP_C_;

internal class Program
{
    static void Main(string[] args)
    {
        
        var subject = new Subject(101, "OOP_C#");
       Console.Write("please enter the type of Exam(1 for practical ||2 for final:");
        int type_Exam = int.Parse(Console.ReadLine()!);
        if(type_Exam ==1)
           subject.CreateExam("practical"); 
        else
            subject.CreateExam("final");


        if (subject.Exam is null) return;

       
        subject.Exam.AddQuestion(new McqQuestion(
            header: "MCQ",
            body: "Which keyword enables inheritance in C#?",
            mark: 5,
            choices: new[] { "extends", "inherits", ":", "base" },
            correctIndex1Based: 3));

     
        subject.Exam.AddQuestion(new TrueFalseQuestion(
            header: "True/False",
            body: "Interfaces in C# can contain fields.",
            mark: 5,
            correctIsTrue: false));

        
        subject.Exam.AddQuestion(new McqQuestion(
            header: "MCQ",
            body: "Which interface is used to support sorting?",
            mark: 5,
            choices: new[] { "IDisposable", "ICloneable", "IComparable", "IEnumerable" },
            correctIndex1Based: 3));

       
        subject.Exam.Run();

        
        subject.Exam.ShowExam();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
