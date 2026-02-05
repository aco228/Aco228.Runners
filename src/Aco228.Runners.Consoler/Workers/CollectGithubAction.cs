using Aco228.Common.Attributes;
using Aco228.Runners.Consoler.WebServices;
using Aco228.Runners.Core;

namespace Aco228.Runners.Consoler.Workers;

public class DummyActionResponse
{
    public int userId { get; set; }
    public int id { get; set; }
    public string title { get; set; }
    public string body { get; set; }
    public bool completed { get; set; }
}

public class CollectGithubAction : ActionResponseBase<int, DummyActionResponse>
{
    [InjectService] public IDummyApiService WebService { get; set; }
    
    protected override async Task<DummyActionResponse?> ExecuteInternal(int request)
    {
        var post = await WebService.GetPostById(request);
        return new()
        {
            id = post.id,
            body = post.body,
            completed = post.completed,
            title = post.title,
            userId = post.userId
        };
    }
}