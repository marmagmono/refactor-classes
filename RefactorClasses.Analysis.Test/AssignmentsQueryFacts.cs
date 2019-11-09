using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.Inspections.Class;
using Xunit;

namespace RefactorClasses.Analysis.Test
{
    public class AssignmentsQueryFacts
    {
        [Fact]
        public void Test1()
        {
            var tree = CSharpSyntaxTree.ParseText("void Something(int a, int b)", CSharpParseOptions.Default);
            int b = 10;

            ClassDeclarationSyntax cc = null;
            var ci = ClassInspector.Create(cc);

            ci.FindMatchingConstructors(
                mi => true);
        }

        [Fact]
        public async Task Given_ParameterlessMethod_Then_NoAssignmentsAreFound()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    public static class Test
    {
        public static void Something()
        {
            Console.Out.WriteLine(""dswwww"");
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());
            var classInspector = new ClassInspector(classDeclaration);
            var mi = classInspector.FindMatchingMethods(m => m.Name.Equals("Something")).First();
            var methodSemanticQuery = mi.CreateSemanticQuery(semanticModel);

            // Act
            var assignments = methodSemanticQuery.FindAssignments();

            // Assert
            Assert.NotNull(assignments);
            Assert.Empty(assignments.Assignments);
            Assert.Empty(assignments.NotAssignedParameters);
        }

        [Fact]
        public async Task Given_MethodWithSimpleVariableAssignment_Then_AssignmentIsFound()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    public class Test1
    {
        public void MyMethod(int a, int b)
        {
            int u = 0;
            u = b;
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());
            var classInspector = new ClassInspector(classDeclaration);
            var mi = classInspector.FindMatchingMethods(m => m.Name.Equals("MyMethod")).First();
            var methodSemanticQuery = mi.CreateSemanticQuery(semanticModel);

            // Act
            var assignments = methodSemanticQuery.FindAssignments();

            // Assert
            Assert.Equal(1, assignments.Assignments.Count);
            Assert.Equal(1, assignments.Assignments.First().ParameterIdx);

            Assert.Equal(1, assignments.NotAssignedParameters.Count);
            Assert.Equal(0, assignments.NotAssignedParameters.First().ParameterIdx);
        }

        [Fact]
        public async Task Given_MethodWithConditionalVariableAssignmentWithThrow_Then_AssignmentIsFound()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    public class Test1
    {
        public void MyMethod(int a, int b)
        {
            int u = 0;
            u = a == 10 ? b : throw new ArgumentException();
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());
            var classInspector = new ClassInspector(classDeclaration);
            var mi = classInspector.FindMatchingMethods(m => m.Name.Equals("MyMethod")).First();
            var methodSemanticQuery = mi.CreateSemanticQuery(semanticModel);

            // Act
            var assignments = methodSemanticQuery.FindAssignments();

            // Assert
            Assert.Equal(1, assignments.Assignments.Count);
            Assert.Equal(1, assignments.Assignments.First().ParameterIdx);

            Assert.Equal(1, assignments.NotAssignedParameters.Count);
            Assert.Equal(0, assignments.NotAssignedParameters.First().ParameterIdx);
        }

        [Fact]
        public async Task Given_MethodWithConditionalVariableAssignmentWithThrow_ParameterOnSecondPosition_Then_AssignmentIsFound()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    public class Test1
    {
        public void MyMethod(int a, int b)
        {
            int u = 0;
            u = a == 10 ? throw new ArgumentException() : b;
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());
            var classInspector = new ClassInspector(classDeclaration);
            var mi = classInspector.FindMatchingMethods(m => m.Name.Equals("MyMethod")).First();
            var methodSemanticQuery = mi.CreateSemanticQuery(semanticModel);

            // Act
            var assignments = methodSemanticQuery.FindAssignments();

            // Assert
            Assert.Equal(1, assignments.Assignments.Count);
            Assert.Equal(1, assignments.Assignments.First().ParameterIdx);

            Assert.Equal(1, assignments.NotAssignedParameters.Count);
            Assert.Equal(0, assignments.NotAssignedParameters.First().ParameterIdx);
        }

        [Fact]
        public async Task Given_MethodWithCoalescingAssignmentWithThrow_Then_AssignmentIsFound()
        {
            // Arrange
            var text = @"
using System;

namespace RefactorClasses.Analysis.Test
{
    public class Test1
    {
        public void MyMethod(int a, string b)
        {
            int u = 0;
            u = b ?? throw new ArgumentException();
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = TestHelpers.CreateCompilation(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var classDeclaration = TestHelpers.FindFirstClassDeclaration(await tree.GetRootAsync());
            var classInspector = new ClassInspector(classDeclaration);
            var mi = classInspector.FindMatchingMethods(m => m.Name.Equals("MyMethod")).First();
            var methodSemanticQuery = mi.CreateSemanticQuery(semanticModel);

            // Act
            var assignments = methodSemanticQuery.FindAssignments();

            // Assert
            Assert.Equal(1, assignments.Assignments.Count);
            Assert.Equal(1, assignments.Assignments.First().ParameterIdx);

            Assert.Equal(1, assignments.NotAssignedParameters.Count);
            Assert.Equal(0, assignments.NotAssignedParameters.First().ParameterIdx);
        }
    }
}
