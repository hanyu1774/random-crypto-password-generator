using NET.ConsoleApp.RandomPasswords.Workflows;

sealed class Program
{
    public static void Main(string[] args)
    {
        Workflow workflow = new();
        workflow.run();
    }
}
