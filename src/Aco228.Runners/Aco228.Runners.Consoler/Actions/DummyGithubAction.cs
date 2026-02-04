using Aco228.Common.Attributes;
using Aco228.Runners.Actions;
using Aco228.Runners.Actions.Exceptions;
using Aco228.Runners.Consoler.WebServices;

namespace Aco228.Runners.Consoler.Actions;

public class DummyActionResponse
{
    public int id { get; set; }
    public string full_name { get; set; }
    public string description { get; set; }
}

public class DummyGithubAction : ActionBase<int, DummyActionResponse>
{
    protected override ushort MaximumNumberOfErrorRetries => 3;
    
    [InjectService] public IGithubService GithubService { get; set; }
    
    protected override async Task<DummyActionResponse> ExecuteInternal(int request)
    {
        var data = await GithubService.GetRepos("aco228");
        var entry = data.FirstOrDefault(x => x.id == request);
        if (entry == null)
            throw new ActionErrorException($"Entry with id:{request} could not be found");
        
        return new()
        {
            id = entry.id,
            full_name = entry.full_name,
            description = entry.description,
        };
    }
}