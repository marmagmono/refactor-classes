using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorClasses.Test.Samples;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;

namespace RefactorClasses.Test.ClassMembersModifications
{
    [TestClass]
    public class ClassMembersModificationsRefactoringTest : DiagnosticVerifier
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
        public async Task Class_WithTwoNonTrivialConstructors_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int field;
    public int Prop1 { get; set; }
    public int Prop2 { get; set; }

    public Test(int prop1) => Prop1 = prop1;

    public Test(int prop1, int prop2)
    {
        Prop1 = prop1;
        Prop2 = prop2;
    }
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(147, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task StaticPropertyKlo_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    public static int klo { get; set; }
    public int klo2 { get; set; }
    public int klo4444 { get; set; }

    public Test(int klo4444)
    {
        this.klo4444 = klo4444;
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(164, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task PropertyEvent_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    public event EventHandler a
    {
        add { }
        remove { }
    }

    public int klo { get; set; }

    public Test(int klo)
    {
        this.klo = klo;
    }
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(207, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task EventField_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    public event EventHandler a;

    public int klo { get; set; }

    public Test(int klo)
    {
        this.klo = klo;
    }
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
        public async Task Indexer_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    public int this[int i]
    {
        get { return i; }
        set { }
    }

    public int klo { get; set; }

    public Test(int klo)
    {
        this.klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(207, 0), a => registeredAction = a);
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

public partial class Test
{
    private int eee;

    public int klo { get; set; }

    public Test(int klo)
    {
        this.klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(153, 0), a => registeredAction = a);
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
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public static class Test
{
    private int eee;

    public int klo { get; set; }

    public Test(int klo)
    {
        this.klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(152, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        [Ignore("TODO: Not implemented yet")]
        public async Task ClassWithNonTrivialArrowConstructor_IsIgnored()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public int klo { get; set; }

    public int Ooo { get; set; }

    public Test(int klo, int ooo) => (this.klo, Ooo) = (klo, ooo);
}
";

            CodeAction registeredAction = null;
            var context = CreateRefactoringContext(testString, new TextSpan(156, 0), a => registeredAction = a);
            var sut = CreateSut();

            // Act
            await sut.ComputeRefactoringsAsync(context);

            // Assert
            Assert.IsNull(registeredAction);
        }

        [TestMethod]
        public async Task ClassWithField_ParameterAddedToConstructor()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public int klo { get; set; }

    public int Ooo { get; set; }

    public Test(int eee, int klo, int ooo)
    {
        this.eee = eee;
        this.klo = klo;
        Ooo = ooo;
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public int klo { get; set; }

    public int Ooo { get; set; }

    public Test(int eee, int uuu, int klo, int ooo)
    {
        this.eee = eee;
        this.uuu = uuu;
        this.klo = klo;
        Ooo = ooo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to uuu
            var context = CreateRefactoringContext(document, new TextSpan(148, 0), a => registeredAction = a);
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
        public async Task ClassWithField_ParameterAddedToConstructor_IndentIsPreserved()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public int klo { get; set; }

    public int Ooo { get; set; }

    public Test(
        int eee,
        int klo,
        int ooo)
    {
        this.eee = eee;
        this.klo = klo;
        Ooo = ooo;
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public int klo { get; set; }

    public int Ooo { get; set; }

    public Test(
        int eee,
        int uuu,
        int klo,
        int ooo)
    {
        this.eee = eee;
        this.uuu = uuu;
        this.klo = klo;
        Ooo = ooo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to uuu
            var context = CreateRefactoringContext(document, new TextSpan(148, 0), a => registeredAction = a);
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
        public async Task ClassWithProperty_PropertyAddedToConstructor()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test(int ooo)
    {
        Ooo = ooo;
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test(string klo, int ooo)
    {
        Klo = klo;
        Ooo = ooo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to Klo
            var context = CreateRefactoringContext(document, new TextSpan(185, 0), a => registeredAction = a);
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
        public async Task ClassWithProperty_PropertyAddedToConstructor1()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test(int ooo)
    {
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test(int ooo, string klo)
    {
        Klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to Klo
            var context = CreateRefactoringContext(document, new TextSpan(173, 0), a => registeredAction = a);
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
        public async Task ClassWithProperty_PropertyAddedToConstructor2()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public float AnotherThing { get; set; }
    public string Klo { get; set; }
    public int Ooo { get; set; }

    public Test(float ttyyy, int ooo)
    {
        AnotherThing = ttyyy;
        Ooo = ooo;
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public float AnotherThing { get; set; }
    public string Klo { get; set; }
    public int Ooo { get; set; }

    public Test(float ttyyy, string klo, int ooo)
    {
        AnotherThing = ttyyy;
        Klo = klo;
        Ooo = ooo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to Klo
            var context = CreateRefactoringContext(document, new TextSpan(219, 0), a => registeredAction = a);
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
        public async Task ClassWithProperty_PropertyAddedToConstructor3()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test()
    {
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class Test
{
    private int eee, uuu;

    public string Klo { get; set; }

    public int Ooo { get; set; }

    public Test(string klo)
    {
        Klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Should point to Klo
            var context = CreateRefactoringContext(document, new TextSpan(185, 0), a => registeredAction = a);
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
        public async Task ClassWithProperty_MultipleFields_CursorAfterComma_Works()
        {
            // Arrange
            var testString = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testanalyzer
{
    class Class1
    {
        private string a, b, v;

        private string ottt;

        private string otta;

        public string Something { get; set; }

        public string OtherThing { get; set; }

        public Class1 WithSomething(string something) => new Class1(something);

        public Class1(string something)
        {
            Something = something;
            this.otta = something;
        }
    }
}
";
            var expectedText = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testanalyzer
{
    class Class1
    {
        private string a, b, v;

        private string ottt;

        private string otta;

        public string Something { get; set; }

        public string OtherThing { get; set; }

        public Class1 WithSomething(string something) => new Class1(something);

        public Class1(string b, string something)
        {
            Something = something;
            this.b = b;
            this.otta = something;
        }
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            // Cursor after 'b,'
            var context = CreateRefactoringContext(document, new TextSpan(204, 0), a => registeredAction = a);
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
        public async Task Class_WithSomeProperties_ConstructorMatchingPropertiesIsGenerated()
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

    public Class2(AnEnum1 enumProp, int klo)
    {
        EnumProp = enumProp;
        Klo = klo;
    }
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

    public Class2(AnEnum1 enumProp, T prop1, int klo)
    {
        EnumProp = enumProp;
        Prop1 = prop1;
        Klo = klo;
    }
}
";

            CodeAction registeredAction = null;
            var document = CreateDocument(testString);
            var context = CreateRefactoringContext(document, new TextSpan(264, 0), a => registeredAction = a);
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

        private RefactorClasses.ClassMembersModifications.RefactoringProvider CreateSut() =>
            new RefactorClasses.ClassMembersModifications.RefactoringProvider();

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
