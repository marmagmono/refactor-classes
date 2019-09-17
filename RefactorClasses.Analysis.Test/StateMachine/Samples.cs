using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Generators;
using RefactorClasses.Analysis.Inspections.Class;
using RefactorClasses.Analysis.Inspections.Method;
using Xunit;

namespace RefactorClasses.Analysis.Test.StateMachine
{
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

        public async Task<int> TaskMethodReturningSomething(int a, float b)
        {
            return 10;
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

            var methods = classInspector.FindMatchingMethods(
                mi => mi.Check(m => m.IsPublic() && !m.IsStatic()).Passed);

            foreach (var method in methods)
            {
                var parameters = method.Parameters.Select(par => par.Type).ToList();
                var record = new RecordBuilder(method.Name)
                    .AddModifiers(Modifiers.Public)
                    .AddProperties(
                        method.Parameters
                            .Select(p => (p.Type, p.Name)).ToArray())
                    .Build();

                var rs = record.ToFullString();

                int a = 10;
            }

            // Act

            // Assert
        }
    }
}
