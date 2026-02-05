# Aco228.Runners - Workflow Verification Report

## Changes Since Last Review

| Change | File | Status |
|--------|------|--------|
| Added try-finally in `Execute()` | ActionDefinition.cs | Fixed |
| CancellationToken passed through execution chain | ActionDefinition.cs, ActionBase.cs | Fixed |
| Status set to `Executing` in `OnExecutionStarted()` | BackgroundServiceActionManager.cs | Fixed |
| Null result now throws exception (triggers retry) | ActionBase.cs | Fixed |
| All exit paths now use `break` ensuring `OnExit()` is called | ActionBase.cs | Fixed |
| `CollectActionDocument()` returns `List<>` instead of `IAsyncEnumerable` | ActionBackgroundServiceLoader.cs | Changed |

---

## Compilation Issue Found

**File**: `ActionBackgroundService.cs:44`

```csharp
await foreach (var actionDefinition in _loader.CollectActionDocument().WithCancellation(CancellationToken))
```

**Problem**: `CollectActionDocument()` now returns `Task<List<ActionDefinition>>`, but `await foreach` expects `IAsyncEnumerable<T>`.

**Fix**: Change to regular foreach:
```csharp
foreach (var actionDefinition in await _loader.CollectActionDocument())
```

---

## Workflow Trace

### 1. Service Startup

```
HostedServiceRunner.RunAsync()
  → ActionBackgroundService.Initialize()
    → Set up repos, machine contract
    → ReleaseMachineLocks() - release locks older than 5 minutes
```

### 2. Each Tick (every 15 seconds)

```
ExecuteTick()
  ├─ Check: RunningActions.Count >= MAX? → return early
  │
  ├─ CollectActionDocument()
  │   ├─ Phase 1: Query actions locked by THIS machine
  │   │   └─ For each: Create ActionDefinition, add to result list
  │   │
  │   └─ Phase 2: Lock NEW actions (for next tick)
  │       ├─ Query runnable actions (Waiting, Executing)
  │       ├─ Release stale locks (>25 min lock time or >35 min execution)
  │       ├─ Lock available actions (set LockBy, LockTimeTs, Status=Waiting)
  │       └─ Commit to MongoDB
  │       └─ NOTE: Does NOT add to result list (two-phase design)
  │
  └─ For each ActionDefinition in result:
      └─ Start(cancellationToken, RunningActions)
```

### 3. Action Execution

```
ActionDefinition.Start()
  ├─ RunningTask = Execute(ct).WaitAsync(ct)
  └─ Add to RunningActionCollection

ActionDefinition.Execute()
  try:
    ├─ Actions.GetByType() - create action instance
    ├─ BackgroundServiceActionManager.Initialize() - load ActionDataDocument
    └─ action.ExecuteInBackground(manager, ct)
  finally:
    └─ RunningActionCollection.Remove(this)  ← ALWAYS runs

ExecuteInBackground()
  ├─ Deserialize request
  ├─ Load ErrorCount from document
  └─ GetResponse(request).WaitAsync(ct)

GetResponse()
  ├─ OnExecutionStarted()  ← Sets Status=Executing, saves to DB
  │
  ├─ RETRY LOOP:
  │   try:
  │   │ result = ExecuteInternal(request)
  │   │ if (result == null) throw InvalidOperationException
  │   │ OnResultReceived(result)  ← MoveToFinished
  │   │ break
  │   │
  │   catch ActionContinueException:
  │   │ ChangeStatus(Waiting)
  │   │ break
  │   │
  │   catch ActionErrorException:
  │   │ OnFatalError()  ← MoveToFailed
  │   │ break
  │   │
  │   catch Exception:
  │     ErrorCount++
  │     if (ErrorCount >= Max):
  │       OnFatalError()  ← MoveToFailed
  │       break
  │     OnError()  ← Save ErrorCount
  │
  │   await Task.Delay(DelayBetweenRetries)
  │
  └─ OnExit()  ← ALWAYS called (saves logs, releases lock if not completed)
```

---

## State Machine

```
                    ┌─────────────────────────────────────────┐
                    │                                         │
                    ▼                                         │
┌─────────┐    ┌─────────┐    ┌───────────┐    ┌──────────┐  │
│ Waiting │───▶│ Locked  │───▶│ Executing │───▶│ Finished │  │
└─────────┘    │(LockBy) │    └───────────┘    └──────────┘  │
     ▲         └─────────┘         │                         │
     │              │              ├───▶ Failed              │
     │              │              │                         │
     └──────────────┴──────────────┴─ ActionContinueException┘
         (lock released)              (back to Waiting)
```

---

## Verified Fixes

### 1. try-finally in Execute()
```csharp
private async Task Execute(CancellationToken cancellationToken)
{
    try
    {
        // ... execution logic ...
    }
    finally
    {
        RunningActionCollection.Remove(this);  // Always runs
    }
}
```
**Result**: Actions are always removed from collection, even on exception.

### 2. OnExit() Always Called
```csharp
// All paths now use break instead of return
catch (Exception ex)
{
    if (ErrorCount >= MaximumNumberOfErrorRetries)
    {
        await ActionManager.OnFatalError(...);
        break;  // Was: return default
    }
}
// ...
await ActionManager.OnExit();  // Always reached
```
**Result**: Logs are saved in all failure scenarios.

### 3. Status Set to Executing
```csharp
public async Task OnExecutionStarted()
{
    _actionDocument.Status = ActionStatus.Executing;  // Added
    _actionDocument.ExecutionStartedTs = DT.GetUnix();
    await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
}
```
**Result**: Actions show correct status during execution.

### 4. Null Result Handled
```csharp
result = await ExecuteInternal(request);
if (result == null)
    throw new InvalidOperationException("Action returned null");
```
**Result**: Null results trigger retry/failure instead of infinite loop.

### 5. CancellationToken Propagated
```csharp
// ActionDefinition.Start()
RunningTask = Execute(cancellationToken).WaitAsync(cancellationToken);

// ActionDefinition.Execute()
await action.ExecuteInBackground(backgroundServiceManager, cancellationToken);

// ActionBase.ExecuteInBackground()
return await GetResponse(request).WaitAsync(cancellationToken);
```
**Result**: Shutdown can interrupt waiting operations.

---

## Remaining Issues

### 1. Compilation Error (Must Fix)

`ExecuteTick()` uses `await foreach` on `Task<List<>>`:
```csharp
// Current (won't compile):
await foreach (var actionDefinition in _loader.CollectActionDocument().WithCancellation(CancellationToken))

// Should be:
foreach (var actionDefinition in await _loader.CollectActionDocument())
```

### 2. MoveToCompleted Not Atomic (Medium Priority)

```csharp
await ...DeleteAsync(document);
await ...InsertOrUpdateAsync(completedDocument);
```

If delete succeeds but insert fails, action is lost. Consider:
- Insert first, then delete
- Or use MongoDB transaction

### 3. Infrastructure Exceptions Not Caught (Low Priority)

`OnExecutionStarted()` and `OnExit()` can throw MongoDB errors that propagate unhandled. However, with the try-finally fix, at least `RunningActionCollection.Remove()` will still run.

---

## Two-Phase Locking Confirmed

The design maintains two-phase locking:

**Tick N:**
- Phase 1: Return actions locked in previous tick
- Phase 2: Lock new actions (saved to DB, NOT returned)

**Tick N+1:**
- Phase 1: Return actions locked in Tick N
- Phase 2: Lock more new actions

This prevents race conditions without atomic MongoDB operations.

---

## Summary

| Category | Status |
|----------|--------|
| Core execution flow | Working |
| Error handling | Fixed |
| Cleanup on failure | Fixed |
| Status tracking | Fixed |
| Log persistence | Fixed |
| Cancellation support | Added |
| Compilation | Has error (easy fix) |
| Atomic operations | Not implemented |

**Verdict**: After fixing the compilation error, the workflow should function correctly. The core logic is sound. The only remaining concern is `MoveToCompleted()` atomicity, which could cause data loss in rare failure scenarios.
