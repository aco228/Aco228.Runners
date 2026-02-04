using Aco228.Common.Attributes;
using Aco228.Runners.Actions;
using Aco228.Runners.Actions.Exceptions;
using Aco228.Runners.Consoler.WebServices;

namespace Aco228.Runners.Consoler.Actions;

public class DummyActionResponse
{
    public int userId { get; set; }
    public int id { get; set; }
    public string title { get; set; }
    public string body { get; set; }
    public bool completed { get; set; }
}

public class DummyGithubAction : ActionBase<int, DummyActionResponse>
{
    protected override ushort MaximumNumberOfErrorRetries => 3;
    
    [InjectService] public IDummyApiService WebService { get; set; }
    
    protected override async Task<DummyActionResponse> ExecuteInternal(int request)
    {
        Log($"Executing DummyGithubAction with request: {request}");
        
        var storeObj = GetData<bool?>("storeObj");
        if (storeObj == null)
        {
            SetData("storeObj", true);
            throw new ActionContinueException($"StoreObj is not set.");
        }
        

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