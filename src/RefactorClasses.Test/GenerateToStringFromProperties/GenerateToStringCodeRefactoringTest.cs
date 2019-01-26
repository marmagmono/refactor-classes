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

namespace RefactorClasses.Test.GenerateToStringFromProperties
{
    [TestClass]
    public class GenerateToStringCodeRefactoringTest : DiagnosticVerifier
    {
        [TestMethod]
        public async Task Class_Empty_IsIgnored()
        {
            // Arrange
            var testString = ClassSamples.EmptyClass;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task Class_WithoutNonStaticProperties_IsIgnored()
        {
            // Arrange
            var testString = ClassSamples.ClassWithoutNonStaticProperties;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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
            var testString = ClassSamples.ClassWithIndexer;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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
            var testString = ClassSamples.ClassWithPropertyEvent;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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
            var testString = ClassSamples.ClassWithEventField;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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
            var testString = ClassSamples.ClassWithField;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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
            var testString = ClassSamples.PartialClass;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task StaticClass_IsIgnored()
        {
            // Arrange
            var testString = ClassSamples.StaticClass;

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(154, 0), a => registeredAction = a);
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

public class Class2<T>
{
    public AnEnum1 EnumProp { get; }

    public T Prop1 { get; }

    public int Klo { get; }
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

    public override string ToString() => $""{nameof(Class2)} EnumProp={EnumProp} Prop1={Prop1} Klo={Klo}"";
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

        [TestMethod]
        public async Task Class_WithSomeProperties_ConstructorWithNotAllProperties_IsReplaced_WithCompleteOne()
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
    public Class2(AnEnum1 enumProp, int klo)
    {
        EnumProp = enumProp;
        Prop1 = prop1;
        Klo = klo;
    }

    public AnEnum1 EnumProp { get; }

    public T Prop1 { get; }

    public int Klo { get; }
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
    public Class2(AnEnum1 enumProp, T prop1, int klo)
    {
        EnumProp = enumProp;
        Prop1 = prop1;
        Klo = klo;
    }

    public AnEnum1 EnumProp { get; }

    public T Prop1 { get; }

    public int Klo { get; }
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

        private RefactorClasses.GenerateToStringFromProperties.RefactoringProvider CreateSut() =>
            new RefactorClasses.GenerateToStringFromProperties.RefactoringProvider();


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
