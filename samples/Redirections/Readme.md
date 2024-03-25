Redirections
====

## Goal

Democrite has devised a dynamic solution for determining the implementation to be utilized for a **contract**. <br />
There are several reasons why multiple implementations might be necessary. <br />
Now, with the introduction of a feature called "**Redirection**" you can dynamically alter the implementation used for a specific **contract**.

It is usefull for:
- Testing: Ensure that implementation won't break existing sequences
- Competition: You can them make implementation in competition to select the best related to context.
- ...

This redirection could be setup a different level:
- Call Scope: During a call using **IDemocriteExucutorHanlder**. Limit the redirection only to a specific call, you can even specialize to a specific stage.
- Global Scope: Available on all the cluster

> [!CAUTION]
> The services **IGrainFactory** or **IVGrainProvider** are affected by redirection rules. 
> To bypass any redirection, respectively utilize the services **IGrainOrleanFactory** and **IVGrainDemocriteSystemProvider**.

## How to configure

### Call Scope

In the function **Sequence** you can now pass a second argument used to customize the execution.

```csharp

app.MapGet("/build/WithCustomRedirection", async ([FromServices] IDemocriteExecutionHandler exec, CancellationToken token) =>
{
    return await exec.Sequence<ITextBuilder>(sentenceBuildSeq.Uid, cfg =>
                     {
                         // Use contract redirection to target a different implementation
                         cfg.RedirectGrain<IComplementVGrain, ISadComplementVGrain>();
                     })
                     .SetInput(new TextBuilder())
                     .RunAsync<ITextBuilder>(token);
});
```

### Global

To inject a redirection a global level you need to contact the vgrain **IClusterRouteRegistryVGrain**.

> [!CAUTION]
> Attention: **IClusterRouteRegistryVGrain** will have an impact above all the cluster.
> The last parameter of sensible method require an IIdentityCard.
> This card will be used to managed the security and right.
> <br/>
> :warning: During this beta version, security is still in place but we start puting in place elements to prevent signature breaking change in the future.

```csharp

// System grain <see cref="IClusterRouteRegistryVGrain"> will managed all the redirection a cluster level

var grain = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(executionContext);

return await grain.RequestAppendRedirectionAsync(VGrainInterfaceRedirectionDefinition.Create<IComplementVGrain, ISadComplementVGrain>(), this.IdentityCard!);

```

## Sample

- <font color="orange">"/build"</font> : Generate a simple sentence using different VGrain contract like **IComplementVGrain**  and **ISeparatorComplementVGrain**
- <font color="orange">"/build/simplify"</font> : Same as build but use the select instruction to provider builder class and select only string as ouput

**Local Redirection test **

- <font color="orange">"/build/WithCustomRedirection"</font> : Like "/build" but redirect **IComplementVGrain** to **ISadComplementVGrain** (Local Redirection)

**Global Redirection test **

- <font color="orange">"/global/redirect/complement"</font> : Toggle redirection **IComplementVGrain** to **ISadComplementVGrain** to a global level
- <font color="orange">"/global/redirect/conjunction"</font> : Toggle redirection **ISeparatorComplementVGrain** to implementation **ConjuctionSeparatorComplementVGrain** to a global level
- <font color="orange">"/global/redirect/clear"</font> : Clear all global redirections
- <font color="orange">"/global/redirect/get"</font> : Get all the global redirection applyed