using System.Text;

namespace GrpcTodo.CLI.Lib;


public sealed class ConfigsManager
{
    public static string UserHomeFolder => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string ConfigsFolderName => ".gl";
    public static string ConfigsFileName => "configs";
    private static string _dirPath = UserHomeFolder + "/" + ConfigsFolderName;
    private static string _configsFilePath = _dirPath + "/" + ConfigsFileName;

    private record Info(string Value, int LineIndex, int KeyIndex, string[] Lines);

    public ConfigsManager()
    {
        CreateConfigFilesIfNotExists();
    }

    private void CreateConfigFilesIfNotExists()
    {
        if (!Directory.Exists(_dirPath))
            Directory.CreateDirectory(_dirPath);

        if (!File.Exists(_configsFilePath))
        {
            using var file = File.CreateText(_configsFilePath);

            file.Write("# please, do not edit this file manually");
            file.Close();
        }
    }

    private (string value, ushort index) GetKey(string line)
    {
        var key = "";
        ushort index = 2;
        var chr = line[2];

        while (chr != '=')
        {
            key += chr;
            chr = line[++index];
        }

        return (key, (ushort)(index + 1));
    }

    private string GetValue(string line, ushort startIndex)
    {
        return line[((byte)startIndex)..];
    }

    private string ReadFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var bytes = new byte[fs.Length];

        fs.Read(bytes, 0, (int)fs.Length);

        fs.Close();

        return Encoding.UTF8.GetString(bytes);
    }

    private Info? GetDetailed(string key)
    {
        var text = ReadFile(_configsFilePath);

        string[] lines = text.Replace("\r\n", "\n").Split("\n");

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.StartsWith("@ "))
            {
                var lineKey = GetKey(line);

                if (lineKey.value == key)
                {
                    var value = GetValue(line, lineKey.index);

                    return new(value, lineKey.index, i, lines);
                }
            }
        }

        return null;
    }

    public string? Get(string key)
    {
        CreateConfigFilesIfNotExists();

        return GetDetailed(key)?.Value;
    }

    public void Remove(string key)
    {
        CreateConfigFilesIfNotExists();

        var keyOnFile = GetDetailed(key);

        if (keyOnFile is null)
            return;

        StringBuilder newFileContent = new();

        var (value, _, lineIndex, lines) = keyOnFile;

        for (int i = 0; i < lines.Length; i++)
        {
            if (i != lineIndex)
            {
                newFileContent.AppendLine(lines[i]);
            }
        }

        using var fs = new FileStream(_configsFilePath, FileMode.Create, FileAccess.Write);

        var bytes = Encoding.UTF8.GetBytes(newFileContent.ToString());

        fs.Write(bytes);

        fs.Close();
    }

    public void Set(string key, string value)
    {
        CreateConfigFilesIfNotExists();

        var keyOnFile = GetDetailed(key);

        StringBuilder newFileContent = new();

        if (keyOnFile is not null)
        {
            var (keyValue, keyIndex, lineIndex, lines) = keyOnFile;

            for (int i = 0; i < lines.Length; i++)
            {
                if (i != lineIndex)
                {
                    newFileContent.AppendLine(lines[i]);
                }
                else
                {
                    newFileContent.Append(lines[i].Replace(keyValue, value));
                }
            }
        }
        else
        {
            var currentFileContent = ReadFile(_configsFilePath);

            newFileContent.AppendLine(currentFileContent);
            newFileContent.AppendLine($"@ {key}={value}");
        }

        using var fs = new FileStream(_configsFilePath, FileMode.Create, FileAccess.Write);

        var bytes = Encoding.UTF8.GetBytes(newFileContent.ToString());

        fs.Write(bytes);

        fs.Close();
    }
}