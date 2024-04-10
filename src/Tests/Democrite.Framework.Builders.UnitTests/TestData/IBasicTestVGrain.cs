// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.UnitTests.TestData
{
    using Democrite.Framework.Core.Abstractions;

    using System.Net;
    using System.Threading.Tasks;

    public interface IBasicTestVGrain : IVGrain
    {
        Task<string> ExecuteStringAsync(IExecutionContext ctx);

        Task<IReadOnlyCollection<Uri>> GetUrisAsync(IExecutionContext ctx);

        Task<string> GetHtmlFromUriAsync(Uri page, IExecutionContext ctx);

        Task<Guid> ExecuteGuidAsync(IExecutionContext ctx);

        Task<IPAddress> ExecuteIpAddressAsync(string arg, IExecutionContext ctx);

        Task<IPAddress> ExecuteIpAddressAsync(Guid arg, IExecutionContext ctx);

        Task<IPAddress> ExecuteIpAddressAsync(char c, IExecutionContext ctx);
    }

    public interface IBasicTestOtherVGrain : IVGrain
    {
        Task<string> OtherExecuteStringAsync(IExecutionContext ctx);

        Task<Guid> OtherExecuteGuidAsync(IExecutionContext ctx);

        Task<IPAddress> OtherReturnIpAddressFromStringAsync(string arg, IExecutionContext ctx);

        Task<IPAddress> OtherReturnIpAddressFromGuidAsync(Guid arg, IExecutionContext ctx);

        Task<IPAddress> OtherReturnIpAddressFromCharAsync(char c, IExecutionContext ctx);

        Task<IPAddress> OtherReturnIpAddressFromIntAsync(int Lenght, IExecutionContext ctx);
    }

    public interface ITestStoreVGrain : IVGrain
    {
        Task Storage<TInput>(TInput input, IExecutionContext ctx);
    }
}
