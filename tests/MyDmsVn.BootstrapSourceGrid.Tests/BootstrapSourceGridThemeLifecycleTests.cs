using System.Threading;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapSourceGridThemeLifecycleTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void CurrentSnapshotMatchesThemeAtConstruction()
    {
        var original = BootstrapThemeManager.CurrentTheme;
        var theme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

        try
        {
            BootstrapThemeManager.CurrentTheme = theme;

            using (var grid = new BootstrapSourceGridControl())
            {
                Assert.That(grid.CurrentThemeSnapshot.CellBackColor, Is.EqualTo(theme.Colors.Surface));
                Assert.That(grid.CurrentThemeSnapshot.CellForeColor, Is.EqualTo(theme.Colors.Text));
                Assert.That(grid.BackColor, Is.EqualTo(theme.Colors.Surface));
                Assert.That(grid.ForeColor, Is.EqualTo(theme.Colors.Text));
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = original;
        }
    }

    [Test]
    public void ThemeChangedUpdatesSnapshotWithoutCreatingHandle()
    {
        var original = BootstrapThemeManager.CurrentTheme;
        var light = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
        var dark = BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

        try
        {
            BootstrapThemeManager.CurrentTheme = light;
            using (var grid = new BootstrapSourceGridControl())
            {
                var previousSnapshot = grid.CurrentThemeSnapshot;

                BootstrapThemeManager.CurrentTheme = dark;

                Assert.That(grid.CurrentThemeSnapshot, Is.Not.SameAs(previousSnapshot));
                Assert.That(grid.CurrentThemeSnapshot.CellBackColor, Is.EqualTo(dark.Colors.Surface));
                Assert.That(grid.CurrentThemeSnapshot.CellForeColor, Is.EqualTo(dark.Colors.Text));
                Assert.That(grid.IsHandleCreated, Is.False);
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = original;
        }
    }

    [Test]
    public void DisposedGridStopsReactingToThemeChanges()
    {
        var original = BootstrapThemeManager.CurrentTheme;
        var light = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
        var dark = BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

        try
        {
            BootstrapThemeManager.CurrentTheme = light;
            var grid = new BootstrapSourceGridControl();
            var snapshotBeforeDisposal = grid.CurrentThemeSnapshot;
            grid.Dispose();

            BootstrapThemeManager.CurrentTheme = dark;

            Assert.That(grid.CurrentThemeSnapshot, Is.SameAs(snapshotBeforeDisposal));
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = original;
        }
    }

    [Test]
    public void MultipleThemeSwitchesRemainStable()
    {
        var original = BootstrapThemeManager.CurrentTheme;
        var light = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
        var dark = BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

        try
        {
            BootstrapThemeManager.CurrentTheme = light;
            using (var grid = new BootstrapSourceGridControl())
            {
                BootstrapThemeManager.CurrentTheme = dark;
                Assert.That(grid.CurrentThemeSnapshot.SelectionBackColor, Is.EqualTo(dark.Colors.Primary));

                BootstrapThemeManager.CurrentTheme = light;
                Assert.That(grid.CurrentThemeSnapshot.SelectionBackColor, Is.EqualTo(light.Colors.Primary));
                Assert.That(grid.BackColor, Is.EqualTo(light.Colors.Surface));
                Assert.That(grid.ForeColor, Is.EqualTo(light.Colors.Text));
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = original;
        }
    }
}
