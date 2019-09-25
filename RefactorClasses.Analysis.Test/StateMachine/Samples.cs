using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Generators;
using RefactorClasses.Analysis.Inspections.Class;
using RefactorClasses.Analysis.Inspections.Method;
using RefactorClasses.Analysis.Syntax;
using Xunit;

namespace RefactorClasses.Analysis.Test.StateMachine
{
    using SF = SyntaxFactory;
    using GH = GeneratorHelper;
    using EGH = ExpressionGenerationHelper;

    public class Samples
    {

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    [StateMachine(ContextType = typeof(ContextBase), StateType = typeof(StateBase), TriggerType = (typeof(TriggerBase)))]
    [StateMachine(""c"")]
    [StateMachine(""d"")]
    public class StateMachineImpl
    {

    }

    public class TriggerBase { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class StateMachineAttribute : Attribute
    {
        public Type StateType { get; set; }

        public Type TriggerType { get; set; }

        public Type ContextType { get; set; }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);
            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());

            var classInspector = new ClassInspector(classDeclaration);
            var semanticInspector = classInspector.CreateSemanticQuery(semanticModel);

            if (semanticInspector.TryFindFirstAttributeMatching("StateMachineAttribute", out var atData))
            {
                int a = 10;
                // parse named arguments looking for types - some of them might need to be
                // generated, some might be already present
            }

            // Act

            // Assert
        }

        [Fact]
        public async Task Test2()
        {
            // Arrange
            var text = @"
using System;
using System.Threading.Tasks;

namespace RefactorClasses.Analysis.Test
{
    [StateMachine(ContextType = typeof(ContextBase), StateType = typeof(StateBase), TriggerType = (typeof(TriggerBase)))]
    public class StateMachineImpl
    {
        public void DoSomething(
            int a,
            Test1 testClass, string fdeee)
        {
        }

        public async Task<int> TaskMethodReturningSomething(int a, float b)
        {
            return 10;
        }

        public System.Threading.Tasks.Task AsyncOperationsSupport(int a, float b)
        {
            return Task.CompletedTask;
        }

        public async Task TaskMethod(int a, float b)
        {
            return;
        }

        public async Task TaskMethodWithArrays(int[] a, float[] b)
        {
            return;
        }

        public async Task TaskMethodWithTuples((int, float) a, float[] b)
        {
            return;
        }

        private void PrintSomething() {}
    }

    public class TriggerBase { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class StateMachineAttribute : Attribute
    {
        public Type StateType { get; set; }

        public Type TriggerType { get; set; }

        public Type ContextType { get; set; }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);
            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());

            var classInspector = new ClassInspector(classDeclaration);
            var semanticInspector = classInspector.CreateSemanticQuery(semanticModel);

            bool foundAttribute = semanticInspector.TryFindFirstAttributeMatching(
                "StateMachineAttribute", out var atData);
            var triggerType = atData
                ?.NamedArguments
                .FirstOrDefault(kvp => kvp.Key.Equals("TriggerType"));
            if (triggerType == null)
            {
                return;
            }

            var methods = classInspector.FindMatchingMethods(
                mi => mi.Check(m => m.IsPublic() && !m.IsStatic()).Passed);

            foreach (var method in methods)
            {
                var msq = method.CreateSemanticQuery(semanticModel);
                var returnType = msq.GetReturnType();
                var isTaskReturn = IsTask(returnType.Symbol);

                var parameters = method.Parameters.Select(par => par.Type).ToList();

                // TODO: will throw if array
                var triggerTypeName = triggerType.Value.Value.Value as INamedTypeSymbol;
                if (triggerTypeName == null)
                {
                    return;
                }

                var recordBuilder = new RecordBuilder(method.Name)
                    .AddModifiers(Modifiers.Public)
                    .AddBaseTypes(GeneratorHelper.Identifier(triggerTypeName.Name))
                    .AddProperties(
                        method.Parameters
                            .Select(p => (p.Type, p.Name)).ToArray());

                if (isTaskReturn.Value.IsTask())
                {
                    var boolTcs = GeneratorHelper.GenericName(
                        "TaskCompletionSource",
                        Types.Bool);

                    var initializer = ExpressionGenerationHelper.CreateObject(boolTcs);
                    recordBuilder.AddField(boolTcs, "result", initializer);

                    var resolveMethod = new MethodBuilder(GH.IdentifierToken("Resolve"))
                        .Body(new BodyBuilder()
                            .AddVoidMemberInvocation(
                                GH.Identifier("result"),
                                GH.Identifier("TrySetResult"),
                                SF.Argument(GH.Identifier("true")))
                            .Build())
                        .Build();

                    var cancelMethod = new MethodBuilder(GH.IdentifierToken("Cancel"))
                        .Body(new BodyBuilder()
                            .AddVoidMemberInvocation(
                                GH.Identifier("result"),
                                GH.Identifier("TrySetCanceled"))
                            .Build())
                        .Build();

                    var rejectMethod = new MethodBuilder(GH.IdentifierToken("Cancel"))
                        .AddParameter(GH.Identifier("Exception"), GH.IdentifierToken("exc"))
                        .Body(new BodyBuilder()
                            .AddVoidMemberInvocation(
                                GH.Identifier("result"),
                                GH.Identifier("TrySetException"),
                                SF.Argument(GH.Identifier("exc")))
                            .Build())
                        .Build();

                    recordBuilder.AddMethod(resolveMethod);
                    recordBuilder.AddMethod(cancelMethod);
                    recordBuilder.AddMethod(rejectMethod);

                    int ddddd = 0;
                }
                else if (isTaskReturn.Value.IsTypedTask(out var taskType))
                {
                    var typedTcs = GeneratorHelper.GenericName(
                        "TaskCompletionSource",
                        GeneratorHelper.Identifier(taskType.Name));

                    var initializer = ExpressionGenerationHelper.CreateObject(typedTcs);
                    recordBuilder.AddField(typedTcs, "result", initializer);
                }

                var record = recordBuilder.Build();

                // TODO: if task is returned -> generate TaskCompletionSource
                // and matching methods

                var rs = record.ToString();

                int a = 10;
            }

            // Act

            // Assert

            IsTaskResult? IsTask(ISymbol symbol)
            {
                var namedSymbol = symbol as INamedTypeSymbol;
                if (namedSymbol == null)
                {
                    return null;
                }

                if (namedSymbol.Name == "Task"
                    && namedSymbol?.ContainingNamespace?.ToString() == "System.Threading.Tasks")
                {
                    var firstTypeArg = namedSymbol.TypeArguments.FirstOrDefault();
                    if (firstTypeArg != null)
                    {
                        return IsTaskResult.TypedTask(firstTypeArg);
                    }
                    else
                    {
                        return IsTaskResult.Task();
                    }
                }

                return IsTaskResult.NotATask();
            }

            //var tcs = new TaskCompletionSource<int>();
            //tcs.TrySetException()
        }

        private readonly struct IsTaskResult
        {
            public static IsTaskResult NotATask() => new IsTaskResult(ResultType.NotATask);

            public static IsTaskResult Task() => new IsTaskResult(ResultType.Task);

            public static IsTaskResult TypedTask(ITypeSymbol taskType) => new IsTaskResult(
                ResultType.TypedTask, taskType);

            public bool IsNotATask() => this.resultType == ResultType.NotATask;

            public bool IsTask() => this.resultType == ResultType.Task;

            public bool IsTypedTask(out ITypeSymbol taskType)
            {
                if (this.resultType == ResultType.TypedTask)
                {
                    taskType = this.typeSymbol;
                    return true;
                }
                else
                {
                    taskType = null;
                    return false;
                }
            }

            #region private
            private enum ResultType { NotATask, Task, TypedTask }

            private readonly ResultType resultType;

            private readonly ITypeSymbol typeSymbol;

            private IsTaskResult(ResultType resultType, ITypeSymbol typeSymbol = default)
            {
                this.resultType = resultType;
                this.typeSymbol = typeSymbol;
            }
            #endregion
        }
    }
}
