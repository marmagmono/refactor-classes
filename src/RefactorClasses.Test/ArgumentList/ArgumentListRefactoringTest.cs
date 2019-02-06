using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;

namespace RefactorClasses.Test.ArgumentList
{
    [TestClass]
    public class ArgumentListRefactoringTest : DiagnosticVerifier
    {
        [TestMethod]
        public async Task Refactoring_IgnoresEmptyArgumentList()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    private void Something()
    {
    }

    public Class3()
    {
        Something();
        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";
            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(311, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task SingleLineArgumentList_IsConvertedToMultilineArgumentList()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    private void Something(AnEnum1 enumProp, ref int klo, T aaa)
    {
    }

    public Class3(AnEnum1 enumProp, int klo, T aaa)
    {
        Something(enumProp, ref klo, aaa);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    private void Something(AnEnum1 enumProp, ref int klo, T aaa)
    {
    }

    public Class3(AnEnum1 enumProp, int klo, T aaa)
    {
        Something(
            enumProp,
            ref klo,
            aaa);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(394, 0), a => registeredAction = a);
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
        public async Task SingleLineArgumentList_IsConvertedToMultilineArgumentList1()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    private void OtherThing(AnEnum1 enumProp)
    {
    }

    public Class3(AnEnum1 enumProp)
    {
        OtherThing(enumProp);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    private void OtherThing(AnEnum1 enumProp)
    {
    }

    public Class3(AnEnum1 enumProp)
    {
        OtherThing(
            enumProp);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(352, 0), a => registeredAction = a);
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
        public async Task MultilineArgumentList_IsConvertedToSingleLineArgumentList()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    public Class3(AnEnum1 enumProp, int klo, T aaa)
    {
        Something(
            enumProp,
            ref klo,
            aaa);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    public Class3(AnEnum1 enumProp, int klo, T aaa)
    {
        Something(enumProp, ref klo, aaa);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(320, 0), a => registeredAction = a);
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
        public async Task MultilineArgumentList_IsConvertedToSingleLineArgumentList1()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    public Class3(AnEnum1 enumProp)
    {
        OtherThing(
            enumProp);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal enum AnEnum1
{
    FirstThing,
    SecondThing = 2
}

internal class Class3<T> where T : class
{
    public Class3(AnEnum1 enumProp)
    {
        OtherThing(enumProp);

        EnumProp = enumProp;
        Klo = klo != 0 ? klo : throw new Exception();
        Prop1 = aaa ?? throw new NullReferenceException();
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(302, 0), a => registeredAction = a);
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

        private RefactorClasses.ArgumentListRefactoring.RefactoringProvider CreateSut() =>
            new RefactorClasses.ArgumentListRefactoring.RefactoringProvider();
    }
}
