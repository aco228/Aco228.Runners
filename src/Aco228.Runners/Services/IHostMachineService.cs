using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents;
using Aco228.Runners.Models;

namespace Aco228.Runners.Services;

public interface IHostMachineService
{
    HostMachineContract GetMachineContract();
    string Name { get; }
    string ApplicationName { get; }
    internal Task Initialize();
    internal Task Tick();
}

public class HostMachineService : IHostMachineService
{
    private readonly HostMachineContract _machineContract;
    
    public HostMachineContract GetMachineContract() => _machineContract;
    public string Name => _machineContract.MachineName;
    public string ApplicationName => _machineContract.ApplicationName;
    private IMongoRepo<HostMachineDocument> HostMachineRepo { get; set; }
    private HostMachineDocument? _document = null;

    public HostMachineService(HostMachineContract machineContract)
    {
        _machineContract = machineContract;
    }
    
    public async Task Initialize()
    {
        HostMachineRepo = ServiceProviderHelper.GetService<IMongoRepo<HostMachineDocument>>()!;
        _document = await HostMachineRepo.FirstOrDefault(x => x.ApplicationName == ApplicationName && x.MachineName == Name);
        if (_document == null)
        {
            _document = new()
            {
                ApplicationName = _machineContract.ApplicationName,
                MachineName = _machineContract.MachineName,
            };
        }
    }

    public async Task Tick()
    {
        _document!.LastTickUtc = DateTime.UtcNow;
        await HostMachineRepo.InsertOrUpdateAsync(_document);
    }
}