using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Generators;
using RefactorClasses.Analysis.Inspections.Method;
using RefactorClasses.Analysis.Inspections.Type.Semantic;
using RefactorClasses.Analysis.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;

namespace RefactorClasses.GenerateClassFromMethod
{
    using SF = SyntaxFactory;
    using GH = GeneratorHelper;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateClassFromMethod"), Shared]
    public class ClassFromMethodRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, methodDeclarationSyntax) =
                await context.FindSyntaxForCurrentSpan<MethodDeclarationSyntax>();

            if (document == null || methodDeclarationSyntax == null) return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate create method using constructor",
                    (c) => GenerateClassFromMethod(document, methodDeclarationSyntax, c)));

            return;
        }

        private static async Task<Document> GenerateClassFromMethod(
            Document document,
            MethodDeclarationSyntax method,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var parentClass = method.Parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (parentClass == null) return document;

            var classDeclaration = CreateClass(semanticModel, method);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.InsertNodesAfter(parentClass, new[] { classDeclaration });
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static ClassDeclarationSyntax CreateClass(
            SemanticModel semanticModel,
            MethodDeclarationSyntax method)
        {
            var mi = new MethodInspector(method);
            var semanticQuery = mi.CreateSemanticQuery(semanticModel);
            var isTaskReturn = SymbolSemanticQuery.IsTask(
                semanticQuery.GetReturnType().Symbol);

            var parameters = mi.Parameters.Select(par => par.Type).ToList();

            var recordBuilder = new RecordBuilder($"{mi.Name}Command")
                    .AddModifiers(Modifiers.Public)
                    //.AddBaseTypes(GeneratorHelper.Identifier(triggerTypeName.Name))
                    .AddProperties(
                        mi.Parameters
                            .Select(p => (p.Type, p.Name)).ToArray());

            if (isTaskReturn.IsTask())
            {
                AddTaskUtilities(recordBuilder, Types.Bool);
            }
            else if (isTaskReturn.IsTypedTask(out var typeSymbol))
            {
                // TODO: name does not exactly works fo predefined types
                AddTaskUtilities(recordBuilder, GH.Identifier(typeSymbol.Name));
            }

            return recordBuilder.Build();
        }

        private static void AddTaskUtilities(
            RecordBuilder recordBuilder,
            TypeSyntax tcsType)
        {
            var tcs = GeneratorHelper.GenericName(
                "TaskCompletionSource",
                tcsType);

            var initializer = ExpressionGenerationHelper.CreateObject(tcs);
            recordBuilder.AddField(tcs, "result", initializer);

            var resolveMethod = new MethodBuilder(GH.IdentifierToken("Resolve"))
                        .Modifiers(Modifiers.Public)
                        .AddParameter(tcsType, GH.IdentifierToken("value"))
                        .Body(new BodyBuilder()
                            .AddVoidMemberInvocation(
                                GH.Identifier("result"),
                                GH.Identifier("TrySetResult"),
                                SF.Argument(GH.Identifier("value")))
                            .Build())
                        .Build();

            var cancelMethod = new MethodBuilder(GH.IdentifierToken("Cancel"))
                .Modifiers(Modifiers.Public)
                .Body(new BodyBuilder()
                    .AddVoidMemberInvocation(
                        GH.Identifier("result"),
                        GH.Identifier("TrySetCanceled"))
                    .Build())
                .Build();

            var rejectMethod = new MethodBuilder(GH.IdentifierToken("Reject"))
                .Modifiers(Modifiers.Public)
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
