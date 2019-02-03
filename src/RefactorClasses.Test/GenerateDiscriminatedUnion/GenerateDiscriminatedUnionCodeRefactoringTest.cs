using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorClasses.Test.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;

namespace RefactorClasses.Test.GenerateDiscriminatedUnion
{
    [TestClass]
    public class GenerateDiscriminatedUnionCodeRefactoringTest : DiagnosticVerifier
    {
        [TestMethod]
        public async Task Class_Empty_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public Test() {}
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(140, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_NonAbstract_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public Test() {}
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(140, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_NonAbstract_IsIgnored1()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public static FirstCase FirstCase(int a) {}
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(140, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithIndexer_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Test
    {
        public static FirstCase FirstCase(int a) {}

        public int this[int t]
        {
            get => t;
            set { }
        }
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(149, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithPropertyEvent_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Test
    {
        public static FirstCase FirstCase(int a) {}

        public event EventHandler a
        {
            add { }
            remove { }
        }
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(149, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithEventField_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Test
    {
        public static FirstCase FirstCase(int a) {}

        public event EventHandler a;
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(149, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithField_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Test
    {
        public static FirstCase FirstCase(int a) {}

        private int a;
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(149, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task PartialClass_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract partial class Test
    {
        public static FirstCase FirstCase(int a) {}

        private int a;
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(157, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithProperty_IsIgnored()
        {
            // Arrange
            var testString = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Test
    {
        public static FirstCase FirstCase(int a) {}

        public int Property { get; set; }
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(149, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithSomeProperties_ToStringMatchingPropertiesIsGenerated()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

public abstract class DuBase
{
    public static FirstCase FirstCase(int number, string name, AnEnum1 enumField) { }

    public static SecondCase SecondCase() { }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

public abstract class DuBase
{
    public static FirstCase FirstCase(int number, string name, AnEnum1 enumField) => new FirstCase(number, name, enumField);

    public static SecondCase SecondCase() => new SecondCase();
}

public sealed class FirstCase : DuBase
{
    public int Number { get; }
    public string Name { get; }
    public AnEnum1 EnumField { get; }

    public FirstCase(int number, string name, AnEnum1 enumField)
    {
        Number = number;
        Name = name;
        EnumField = enumField;
    }
}

public sealed class SecondCase : DuBase
{
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(198, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);
            Assert.IsNotNull(registeredAction);

            var changedDocument = await ApplyRefactoring(document, registeredAction);
            var changedText = (await changedDocument.GetTextAsync()).ToString();

            // Assert
            Assert.AreEqual(expectedText, changedText);
        }

        [TestMethod]
        public async Task Class_WithSomeProperties_ToStringWithoutAllProperties_IsReplaced_WithCompleteOne()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

public class Class2<T>
{
    public AnEnum1 EnumProp { get; }

    public T Prop1 { get; }

    public int Klo { get; }

    public override string ToString() => $""{nameof(Class2)} {nameof(EnumProp)}={EnumProp}"";
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

public class Class2<T>
{
    public AnEnum1 EnumProp { get; }

    public T Prop1 { get; }

    public int Klo { get; }

    public override string ToString() => $""{nameof(Class2)} {nameof(EnumProp)}={EnumProp} {nameof(Prop1)}={Prop1} {nameof(Klo)}={Klo}"";
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(239, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);
            Assert.IsNotNull(registeredAction);

            var changedDocument = await ApplyRefactoring(document, registeredAction);
            var changedText = (await changedDocument.GetTextAsync()).ToString();

            // Assert
            Assert.AreEqual(expectedText, changedText);
        }

        public async Task<Document> ApplyRefactoring(Document originalDocument, CodeAction codeAction)
        {
            var operations = await codeAction.GetOperationsAsync(default(CancellationToken));
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(originalDocument.Id);
        }

        private RefactorClasses.GenerateDiscriminatedUnion.RefactoringProvider CreateSut() =>
            new RefactorClasses.GenerateDiscriminatedUnion.RefactoringProvider();


        private CodeRefactoringContext CreateRefactoringContext(
            Document document,
            TextSpan textSpan,
            Action<CodeAction> registerRefactoring) =>
                new CodeRefactoringContext(
                    document,
                    textSpan,
                    registerRefactoring,
                    default(CancellationToken));

        private CodeRefactoringContext CreateRefactoringContext(
            string documentText,
            TextSpan textSpan,
            Action<CodeAction> registerRefactoring) =>
                new CodeRefactoringContext(
                    CreateDocument(documentText),
                    textSpan,
                    registerRefactoring,
                    default(CancellationToken));
    }
}
