using ExpenseTracker.ConsoleApp;
using ExpenseTracker.ConsoleApp.Console;
using ExpenseTracker.Core.Services;

var tracker = new ExpenseTracker.Core.Services.ExpenseTracker(new SystemLocalDateProvider());
var commandLoop = new CommandLoop(tracker);
commandLoop.Run(System.Console.In, System.Console.Out);
