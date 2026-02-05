using System.Text.Json;

namespace Aco228.Runners.Documents;

public class ActionObjectModel
{
    public string Type { get; set; }
    public string Value { get; set; }


    public T? Get<T>()
        => JsonSerializer.Deserialize<T>(Value);

}