// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.CodeGenerator.UnitTests.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;

    [VGrainMetaData("0BA724B0-9E93-4A78-B940-0749FA963C2D", 
                    simpleNameIdentifier: "i-simple-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface ISimpleGrain : IVGrain { }

    [VGrainMetaData("4BD7096C-30D3-4FBD-A653-D6EBA04C4944",
                    simpleNameIdentifier: "i-child-simple-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IChildSimpleGrain : ISimpleGrain { }

    [VGrainMetaData("1D2EA0E8-604C-4107-8FCF-C52FD0302CAB",
                    simpleNameIdentifier: "i-generic-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IGenericGrain<Type> : IVGrain { }

    [VGrainMetaData("1B57C7D2-45C8-42CA-B167-D4627D13758D",
                    simpleNameIdentifier: "i-generic-multiple-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IGenericMultipleGrain<Type, TOther> : IVGrain { }

    [VGrainMetaData("5255E2CB-465B-4034-A2A9-A2DED1535626",
                    simpleNameIdentifier: "i-generic-with-constraint-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IGenericWithConstraintGrain<Type> : IVGrain
        where Type : struct, ISimpleGrain
    {
    }

    [VGrainMetaData("DC5D3E3E-6945-456C-BA59-B593B2AE418A",
                    simpleNameIdentifier: "i-generic-simple-with-method-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IGenericSimpleWithMethodGrain<TArg> : IVGrain
    {
        [VGrainMetaDataMethod("gen-simple")]
        Task GenSimple(TArg arg, IExecutionContext executionContext);
    }

    [VGrainMetaData("0656A1F6-5E00-4CE9-87FD-0C3CBBE7A271",
                    simpleNameIdentifier: "i-simple-with-method-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface ISimpleWithMethodGrain : IVGrain 
    {
        [VGrainMetaDataMethod("simple")]
        Task Simple(string arg, IExecutionContext executionContext);

        [VGrainMetaDataMethod("simple-no-arg")]
        Task<string> SimpleNoArg(IExecutionContext executionContext);

        [VGrainMetaDataMethod("complex-no-arg")]
        Task Complex(IExecutionContext executionContext);

        [VGrainMetaDataMethod("complex")]
        Task Complex(string arg, IExecutionContext executionContext);

        [VGrainMetaDataMethod("complex-gen-arg-1")]
        Task Complex<TGenericArg>(TGenericArg arg, IExecutionContext executionContext);
        
        [VGrainMetaDataMethod("complex-gen-arg-1-multi")]
        Task Complex<TGenericArg>(TGenericArg arg, IExecutionContext<TGenericArg> executionContext);

        [VGrainMetaDataMethod("complex-gen-arg-2")]
        Task Complex<TGenericArg, TConfig>(TGenericArg arg, IExecutionContext<TConfig> executionContext);
    }

    [VGrainMetaData("8B5DFD0F-BBF4-4A80-83DC-9FC74CFB4930",
                    simpleNameIdentifier: "i-inherite-with-method-grain",
                    namespaceIdentifier: CodeGenTestConstants.BagNamespace)]
    internal interface IInheriteWithMethodGrain : ISimpleWithMethodGrain, IGenericSimpleWithMethodGrain<double>
    {
        [VGrainMetaDataMethod("simple-child")]
        Task SimpleChild(string arg, IExecutionContext executionContext);
    }
}
