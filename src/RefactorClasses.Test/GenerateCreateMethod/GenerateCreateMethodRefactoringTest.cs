using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;

namespace RefactorClasses.Test.GenerateCreateMethod
{
    [TestClass]
    public class GenerateCreateMethodRefactoringTest : DiagnosticVerifier
    {
        [TestMethod]
        public async Task NormalMethod_RefactoringNotRegistered()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b, string g)
    {
    }

    public int Method(int a)
    {
    }
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(202, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task SimpleConstructor_NoCreateMethod_AddsCreateMethod()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b, string g)
    {
    }

    public int Method(int a)
    {
    }
}
";

            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b, string g)
    {
    }

    public static AAAA Create(int a, int b, string g) => new AAAA(a, b, g);

    public int Method(int a)
    {
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(143, 0), a => registeredAction = a);
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
        public async Task SimpleConstructor_SimilarCreateMethodExists_ReplacesCreateMethod()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b, int g)
    {
    }

    public int Method(int a)
    {
    }

    public static AAAA Create(int a, int b, string g) => new AAAA(a, b, g);
}
";

            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b, int g)
    {
    }

    public int Method(int a)
    {
    }

    public static AAAA Create(int a, int b, int g) => new AAAA(a, b, g);
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(143, 0), a => registeredAction = a);
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
        public async Task ConstructorCallingOtherConstructor_NoCreateMethod_AddsCreate()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b) : this(a, b, 0)
    {
    }

    public AAAA(int a, int b, int g)
    {
    }

    public int Method(int a)
    {
    }
}
";

            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class AAAA
{
    public AAAA(int a, int b) : this(a, b, 0)
    {
    }

    public static AAAA Create(int a, int b) => new AAAA(a, b);

    public AAAA(int a, int b, int g)
    {
    }

    public int Method(int a)
    {
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(142, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);
            Assert.IsNotNull(registeredAction);

            var changedDocument = await ApplyRefactoring(document, registeredAction);
            var changedText = (await changedDocument.GetTextAsync()).ToString();

            // Assert
            Assert.AreEqual(expectedText, changedText);
        }

        private RefactorClasses.GenerateCreateMethod.RefactoringProvider CreateSut() =>
            new RefactorClasses.GenerateCreateMethod.RefactoringProvider();

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

    }
}
