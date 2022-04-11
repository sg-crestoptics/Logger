using Logger;

//TestTrace();
//TestLogToConsole();
TestGuidAccess(new Guid());
//TestDownloadAsync(@"C:\Users\Simone Giovinazzo\dev\TestFiles\asynclog.txt");
//TestDownloadSync(@"C:\Users\Simone Giovinazzo\dev\TestFiles\synclog.txt");

void TestTrace()
{
    Guid guidInfo = SGLogger.AddLogEvent(SGEventType.Info, $"PROVA_INFO", $"Just an info message.");
    Guid guidWarning = SGLogger.AddLogEvent(SGEventType.Warning, $"PROVA_WARNING", $"Just a warning message.");
    Guid guidError = SGLogger.AddLogEvent(SGEventType.Error, $"PROVA_ERROR", $"Just an error message.");
    Guid guidAbort = SGLogger.AddLogEvent(SGEventType.Abort, $"PROVA_ABORT", $"Just an abort message.");
    SGLogger.TraceEvent(guidInfo);
    SGLogger.TraceEvent(guidWarning);
    SGLogger.TraceEvent(guidError);
    SGLogger.TraceEvent(guidAbort);

}
void TestLogToConsole()
{
    Guid guidInfo = SGLogger.AddLogEvent(SGEventType.Info, $"PROVA_INFO", $"Just an info message.");
    Guid guidWarning = SGLogger.AddLogEvent(SGEventType.Warning, $"PROVA_WARNING", $"Just a warning message.");
    Guid guidError = SGLogger.AddLogEvent(SGEventType.Error, $"PROVA_ERROR", $"Just an error message.");
    Guid guidAbort = SGLogger.AddLogEvent(SGEventType.Abort, $"PROVA_ABORT", $"Just an abort message.");
    SGLogger.LogToConsole(guidInfo);
    SGLogger.LogToConsole(guidWarning);
    SGLogger.LogToConsole(guidError);
    SGLogger.LogToConsole(guidAbort);

}

void TestGuidAccess(Guid guid)
{
    try
    {
        SGEvent logEvent = SGLogger.GetMessage(guid);
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

void TestDownloadAsync(string path)
{

    Thread thread1 = new Thread(() =>
    {
        for (int i = 0; i < 50000; i++)
        {
            SGLogger.LogToConsole(SGLogger.AddLogEvent(SGEventType.Info, $"Thread 1", $"Tentativo numero {i}"));
        }
    });

    Thread thread2 = new Thread(() =>
    {
        for (int i = 0; i < 50000; i++)
        {
            SGLogger.AddLogEvent(SGEventType.Info, $"Thread 2", $"Tentativo numero {i}");
        }
    });

    thread1.Start();
    thread2.Start();

    thread1.Join();
    thread2.Join();

    SGLogger.DownloadLogToTxtAsync(path).Wait();
}

void TestDownloadSync(string path)
{
    Thread thread1 = new Thread(() =>
    {
        for (int i = 0; i < 50000; i++)
        {
            SGLogger.AddLogEvent(SGEventType.Info, $"Thread 1", $"Tentativo numero {i}");
        }
    });

    Thread thread2 = new Thread(() =>
    {
        for (int i = 0; i < 50000; i++)
        {
            SGLogger.AddLogEvent(SGEventType.Info, $"Thread 2", $"Tentativo numero {i}");
        }
    });

    thread1.Start();
    thread2.Start();

    thread1.Join();
    thread2.Join();

    SGLogger.DownloadLogToTxtAsync(path).Wait();
}