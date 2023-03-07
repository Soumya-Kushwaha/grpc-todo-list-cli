using GrpcTodo.CLI.Enums;
using GrpcTodo.CLI.Models;
using GrpcTodo.CLI.Utils;

namespace GrpcTodo.CLI;

public sealed class Menu
{
    public List<MenuOption> Options = new()
    {
        new MenuOption {
            Path = "task",
            Children = new () {
                new MenuOption {
                    Path = "create",
                    Command = Command.CreateTask,
                    Description = "create a new task",
                    Children = new()
                },
                new MenuOption {
                    Path = "complete",
                    Command = Command.CompleteTask,
                    Description = "complete a task",
                    Children = new()
                },
                new MenuOption {
                    Path = "uncomplete",
                    Command = Command.UncompleteTask,
                    Description = "uncomplete a task",
                    Children = new()
                },
                new MenuOption {
                    Path = "list",
                    Command = Command.ListAllTasks,
                    Description = "list all tasks",
                    Children = new()
                },
                new MenuOption {
                    Path = "delete",
                    Command = Command.DeleteTask,
                    Description = "delete a task",
                    Children = new()
                },
            }
        },
        new MenuOption {
            Path = "account",
            IsImplemented = true,

            Children = new () {
                new MenuOption {
                    Description = "create new account",
                    Path = "create",
                    Command = Command.CreateAccount,
                    Children = new (),
                    IsImplemented = true
                },
                new MenuOption {
                    Path = "login",
                    Command = Command.Login,
                    Description = "make login",
                    Children = new()
                }
            }
        },
    };

    private static void ShowAvailableOptionsRecursively(List<MenuOption> options, int tabs = 0)
    {
        foreach (var option in options)
        {
            string tab = new string(' ', tabs);

            string path = $"{tab} {option.Path}";

            if (tabs == 0)
            {
                Console.WriteLine();
            }

            if (option.IsImplemented)
                ConsoleWritter.WriteWithColor(path, ConsoleColor.Green, option.Description is null);
            else
                ConsoleWritter.WriteWithColor(path, ConsoleColor.Red, option.Description is null);

            if (option.Description is not null)
                ConsoleWritter.Write($@"{new string(' ', tabs)}{option.Description}", true);

            if (option.Children.Any())
                ShowAvailableOptionsRecursively(option.Children, tabs + 2);
        }
    }

    public void ShowAvailableOptions()
    {
        ConsoleWritter.Write("AVAILABLE COMMANDS:");

        ShowAvailableOptionsRecursively(Options);

        Console.WriteLine();
    }

    private static List<string> GetMenuOptionsPaths(MenuOption option)
    {
        List<string> paths = new();

        void ExtractPath(List<MenuOption> options, string path = "")
        {
            if (option.Path != path)
                paths.Add(path);

            foreach (var option in options)
            {
                var _path = $"{path} {option.Path}";

                ExtractPath(option.Children, _path);
            }
        }

        ExtractPath(option.Children, option.Path);

        return paths;
    }

    public List<string> GetMenuCommands()
    {
        List<string> commandsPath = new();

        foreach (var option in Options)
        {
            var menuOptionPaths = GetMenuOptionsPaths(option);

            commandsPath.AddRange(menuOptionPaths);
        }

        return commandsPath;
    }
}