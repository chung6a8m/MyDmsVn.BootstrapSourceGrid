using System;
using System.Reflection;
using System.Threading;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using MyDmsVn.BootstrapSourceGrid.Editors;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapEditorRegistryTests
{
    [Test]
    public void RegistryAndGridExposeTheSupportedPublicCreationSurface()
    {
        Assert.Multiple((Action)(() =>
        {
            Assert.That(typeof(BootstrapSourceGridEditorRegistry).IsPublic, Is.True);
            AssertPublicProperty(
                typeof(BootstrapSourceGridControl),
                "BootstrapEditors",
                typeof(BootstrapSourceGridEditorRegistry));
            AssertPublicCreateMethod("CreateTextBox", typeof(BootstrapTextBoxEditor));
            AssertPublicCreateMethod(
                "CreateFormattedTextBox",
                typeof(BootstrapFormattedTextBoxEditor));
            AssertPublicCreateMethod("CreateLookupBox", typeof(BootstrapLookupBoxEditor));
            AssertPublicProperty(
                typeof(BootstrapTextBoxEditor),
                "BootstrapControl",
                typeof(BootstrapTextBox));
            AssertPublicProperty(
                typeof(BootstrapFormattedTextBoxEditor),
                "BootstrapControl",
                typeof(BootstrapFormattedTextBox));
            AssertPublicProperty(
                typeof(BootstrapLookupBoxEditor),
                "BootstrapControl",
                typeof(BootstrapLookupBox));
        }));
    }

    [TestCase(typeof(BootstrapTextBoxEditor))]
    [TestCase(typeof(BootstrapFormattedTextBoxEditor))]
    [TestCase(typeof(BootstrapLookupBoxEditor))]
    public void AdapterConstructorsAreNotPublic(Type adapterType)
    {
        Assert.That(
            adapterType.GetConstructors(BindingFlags.Instance | BindingFlags.Public),
            Is.Empty);
    }

    [TestCase("CreateTextBox")]
    [TestCase("CreateFormattedTextBox")]
    [TestCase("CreateLookupBox")]
    public void CreateMethodsRejectNullValueTypeImmediately(string methodName)
    {
        using (var grid = new BootstrapSourceGridControl())
        {
            Action create = methodName switch
            {
                "CreateTextBox" => () => grid.BootstrapEditors.CreateTextBox(null!),
                "CreateFormattedTextBox" => () =>
                    grid.BootstrapEditors.CreateFormattedTextBox(null!),
                "CreateLookupBox" => () => grid.BootstrapEditors.CreateLookupBox(null!),
                _ => throw new ArgumentOutOfRangeException(nameof(methodName)),
            };

            Assert.That(
                create,
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("valueType"));
        }
    }

    [Test]
    public void RegistryConstructorRejectsNullOwner()
    {
        Action create = () => new BootstrapSourceGridEditorRegistry(null!);

        Assert.That(
            create,
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("owner"));
    }

    private static void AssertPublicCreateMethod(string name, Type returnType)
    {
        var method = typeof(BootstrapSourceGridEditorRegistry).GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: new[] { typeof(Type) },
            modifiers: null);

        Assert.That(method, Is.Not.Null, $"Missing public {name}(Type).");
        if (method is null)
        {
            return;
        }

        Assert.That(method.ReturnType, Is.EqualTo(returnType));
    }

    private static void AssertPublicProperty(Type ownerType, string name, Type propertyType)
    {
        var property = ownerType.GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.That(property, Is.Not.Null, $"Missing public {ownerType.Name}.{name}.");
        if (property is null)
        {
            return;
        }

        Assert.That(property.PropertyType, Is.EqualTo(propertyType));
        Assert.That(property.CanRead, Is.True);
        Assert.That(property.SetMethod, Is.Null);
        Assert.That(property.GetMethod!.IsPublic, Is.True);
    }
}
