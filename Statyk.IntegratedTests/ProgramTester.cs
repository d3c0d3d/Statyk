using System;
using System.Linq;
using static XR.Std.Cli;

namespace Statyk.IntegratedTests
{
    class ProgramTester
    {

        static bool _exit;

        static void Main(string[] args)
        {           
            while (!_exit)
            {
                try
                {
                    ShellCaret("TestName");
                    Console.ForegroundColor = ConsoleColor.White;
                    string[] c = ShellArgs();
                    if (c?.Length > 0)
                    {
                        switch (c[0])
                        {
                            case "-q":
                                _exit = true;
                                break;
                            case "clear":
                                Console.Clear();
                                break;
                            default:
                                Command.Run(c);
                                break;
                        }

                    }
                }
                catch (CliException ex)
                {
                    PrintErrorMessage(ex);
                }
                catch (Exception ex)
                {
                    PrintError(ex);
                }
            }

            PrintLn("Exiting..");
        }
    }

    internal class Command
    {
        internal static void Run(string[] args)
        {
            var name = args[0];
            var parms = args.ToArray().Skip(1).ToArray();

            new Tests().Init(name, parms);
        }
    }
}
