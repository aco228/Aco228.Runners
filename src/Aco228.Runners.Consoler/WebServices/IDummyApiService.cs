using System.Text.Json.Serialization;
using Aco228.WService;
using Aco228.WService.Attributes;
using Aco228.WService.Base;
using Aco228.WService.Exceptions;
using Aco228.WService.Models.Attributes.MethodAttributes;

namespace Aco228.Runners.Consoler.WebServices;


public class PostResponse
{
    public int userId { get; set; }
    public int id { get; set; }
    public string title { get; set; }
    public string body { get; set; }
    public bool completed { get; set; }
}

public class FilterPosts
{
    public int userId { get; set; }
}

public class CreatePostRequest
{
    public string title { get; set; }
    public string body { get; set; }
    public int userId { get; set; } 
}

public class UpdatePostRequest
{
    public string title { get; set; }
    public string body { get; set; }
    public int userId { get; set; } 
}

public class PatchPostRequest
{
    [JsonPropertyName("title")]
    public string DasIstTitle { get; set; } 
}


public class DummyApiServiceConf : ApiServiceConf
{
    public override string BaseUrl => "https://jsonplaceholder.typicode.com/";

    public override void OnException(WebApiRequestException exception)
    {
        Console.WriteLine(exception.Message);
    }

    public override void OnBeforeRequest(WebApiMethodType methodType, ref string url, ref HttpContent? httpContent, string? httpContentString)
    {
        Console.WriteLine($"[{methodType}] {url} :: {httpContentString}");
    }
}


[ApiServiceDecorator(Type = typeof(DummyApiServiceConf))]
public interface IDummyBase : IApiService
{
    
}

public interface IDummyApiService : IDummyBase
{
    [ApiGet("todos/{id}")]
    Task<string> GetTodo(int id);

    [ApiGet("/posts")]
    Task<string> GetAllPosts();
    
    [ApiGet("/posts?{filterPosts}")]
    Task<string> FilterPosts(FilterPosts filterPosts);
    
    [ApiPost("/posts")]
    Task<PostResponse> CreatePost(CreatePostRequest req);

    [ApiPut("/posts/{id}")]
    Task<string> UpdatePost(int id, UpdatePostRequest req);
    
    [ApiGet("/posts/{id}")]
    Task<PostResponse> GetPostById(int id);
    
    [ApiPatch("/posts/{id}")]
    Task<string> PatchPost(int id, PatchPostRequest req);
    
    [ApiDelete("/posts/{id}")]
    Task DeletePost(int id);
    
}