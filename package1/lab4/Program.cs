using Lab4;

if (!DesTest.Check())
    throw new Exception("DES known-answer test failed.");

Console.WriteLine("DES known-answer test: OK");
