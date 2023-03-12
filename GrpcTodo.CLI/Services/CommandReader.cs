using GrpcTodo.CLI.Models;
using GrpcTodo.CLI.Utils;
using GrpcTodo.CLI.Enums;

namespace GrpcTodo.CLI.Services;

public sealed class CommandReader
{
    private readonly Menu _menu;
    private readonly string[] _args;

    public CommandReader(Menu menu, string[] args)
    {
        _args = args.Where(arg => !arg.StartsWith("--")).ToArray();
        _menu = menu;
    }

    private static int ComputeLevenshteinDistance(string source, string target)
    {
        if ((source == null) || (target == null)) return 0;
        if ((source.Length == 0) || (target.Length == 0)) return 0;
        if (source == target) return source.Length;

        int sourceWordCount = source.Length;
        int targetWordCount = target.Length;

        if (sourceWordCount == 0)
            return targetWordCount;

        if (targetWordCount == 0)
            return sourceWordCount;

        int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

        for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
        for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

        for (int i = 1; i <= sourceWordCount; i++)
        {
            for (int j = 1; j <= targetWordCount; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceWordCount, targetWordCount];
    }

    public void GetNearestCommand(string wrongCommand)
    {
        var commandsPath = _menu.GetMenuCommands();

        int currentLevenshteinDistance = ComputeLevenshteinDistance(wrongCommand, commandsPath[0]);
        var currentCommand = commandsPath[0];

        for (var i = 1; i < commandsPath.Count; i++)
        {
            var levenshteinDistance = ComputeLevenshteinDistance(wrongCommand, commandsPath[i]);

            if (levenshteinDistance < currentLevenshteinDistance)
            {
                currentLevenshteinDistance = levenshteinDistance;
                currentCommand = commandsPath[i];
            }
        }

        ConsoleWritter.WriteSuccess($@"did you mean ""{currentCommand}""", "tip");
    }

    private List<(string path, MenuOption option)> ReadCommandsWithPaths(MenuOption option)
    {
        List<(string path, MenuOption command)> commandsWithPaths = new();

        void ExtractOptions(List<MenuOption> options, string path, MenuOption? menuOption)
        {
            if (option.Path != path && menuOption is not null)
                commandsWithPaths.Add((path, (MenuOption)menuOption));

            foreach (var opt in options)
            {
                var pth = $"{path} {opt.Path}";

                ExtractOptions(opt.Children, pth, opt);
            }
        }

        ExtractOptions(option.Children, option.Path, option);

        return commandsWithPaths;
    }

    public MenuOption? Read()
    {
        if (!HasPossibleOptions())
            return null;

        Dictionary<string, string> aliases = new()
        {
            { "atu", "account token update" }
        };

        List<(string path, MenuOption option)> commandsWithPaths = new();

        foreach (var option in Menu.Options)
        {
            var optionCommandsWithPaths = ReadCommandsWithPaths(option);

            commandsWithPaths.AddRange(optionCommandsWithPaths);
        }

        var alias = string.Join(' ', _args);

        if (aliases.ContainsKey(alias))
        {
            foreach (var command in commandsWithPaths)
            {
                if (command.path == aliases[alias])
                {
                    Console.WriteLine($"alias {alias} executes the command {command.option.Command}");
                    return command.option;
                }
            }
        }

        var rootOptions = Menu.Options;
        var currentArgPos = 0;
        var currentOptionPos = 0;

        while (currentOptionPos < rootOptions.Count)
        {
            var option = rootOptions[currentOptionPos];

            if (option.Path == _args[currentArgPos])
            {
                if (currentArgPos == _args.Length - 1)
                    if (option.Command is not null)
                        return option;
                    else
                        return null;

                rootOptions = option.Children;
                currentOptionPos = 0;
                currentArgPos++;
            }
            else
                currentOptionPos++;
        }

        return null;
    }

    public bool HasPossibleOptions()
    {
        return _args.Length >= 1;
    }

    public override string ToString()
    {
        return string.Join(' ', _args);
    }
}
